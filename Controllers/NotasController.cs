using Itemiza.Data;
using Itemiza.Models;
using Itemiza.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itemiza.Controllers;

public class NotasController : Controller
{
    private readonly ItemizaDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly NotaFiscalOcrService _ocr;

    private const long LimiteBytes = 10 * 1024 * 1024;

    private static readonly string[] ExtensoesPermitidas =
        [".pdf", ".jpg", ".jpeg", ".png"];

    public NotasController(
        ItemizaDbContext db,
        IWebHostEnvironment environment,
        NotaFiscalOcrService ocr)
    {
        _db = db;
        _environment = environment;
        _ocr = ocr;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalNotas = await _db.NotasFiscais.CountAsync();
        ViewBag.TotalCompras = await _db.Compras.CountAsync();
        ViewBag.TotalGasto =
            await _db.Compras.SumAsync(c => (decimal?)c.Valor) ?? 0m;

        return View(
            await _db.NotasFiscais
                .OrderByDescending(n => n.DataEnvio)
                .Take(8)
                .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Enviar(IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            TempData["Erro"] = "Selecione um arquivo antes de enviar.";
            return RedirectToAction(nameof(Index));
        }

        if (arquivo.Length > LimiteBytes)
        {
            TempData["Erro"] = "O arquivo ultrapassa o limite de 10 MB.";
            return RedirectToAction(nameof(Index));
        }

        var extensao =
            Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        if (!ExtensoesPermitidas.Contains(extensao))
        {
            TempData["Erro"] =
                "Formato não permitido. Envie PDF, JPG, JPEG ou PNG.";

            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            TempData["Erro"] =
                "Não foi possível localizar a pasta de arquivos do sistema.";

            return RedirectToAction(nameof(Index));
        }

        var pasta = Path.Combine(
            _environment.WebRootPath,
            "uploads");

        Directory.CreateDirectory(pasta);

        // Gera um nome exclusivo para o arquivo enviado.
        var nomeSeguro = $"{Guid.NewGuid():N}{extensao}";
        var caminhoCompleto = Path.Combine(pasta, nomeSeguro);

        await using (var stream = new FileStream(
            caminhoCompleto,
            FileMode.CreateNew))
        {
            await arquivo.CopyToAsync(stream);
        }

        var nota = new NotaFiscal
        {
            NomeArquivo = Path.GetFileName(arquivo.FileName),
            CaminhoArquivo = $"/uploads/{nomeSeguro}",
            TipoConteudo = arquivo.ContentType
                ?? "application/octet-stream",
            TamanhoBytes = arquivo.Length,
            DataEnvio = DateTime.Now,
            StatusProcessamento = "Aguardando análise"
        };

        // Executa OCR somente em imagens.
        // Se falhar, o arquivo ainda poderá ser salvo no banco.
        if (extensao is ".jpg" or ".jpeg" or ".png")
        {
            try
            {
                var texto = _ocr.ExtrairTexto(caminhoCompleto);

                nota.TextoExtraido = texto;

                nota.StatusProcessamento =
                    string.IsNullOrWhiteSpace(texto)
                        ? "Texto não reconhecido"
                        : "Texto extraído";
            }
            catch (Exception)
            {
                nota.StatusProcessamento = "Falha no OCR";
            }
        }
        else if (extensao == ".pdf")
        {
            // O serviço atual do Tesseract processa imagens,
            // não arquivos PDF diretamente.
            nota.StatusProcessamento =
                "PDF aguardando processamento";
        }

        try
        {
            _db.NotasFiscais.Add(nota);
            await _db.SaveChangesAsync();

            switch (nota.StatusProcessamento)
            {
                case "Texto extraído":
                    TempData["Sucesso"] =
                        "Nota enviada! O texto da imagem foi reconhecido e salvo.";
                    break;

                case "Texto não reconhecido":
                    TempData["Erro"] =
                        "A nota foi salva, mas nenhum texto foi reconhecido. " +
                        "Confira a qualidade da imagem.";
                    break;

                case "Falha no OCR":
                    TempData["Erro"] =
                        "A nota foi salva, mas o OCR falhou. " +
                        "Verifique o arquivo de idioma por.traineddata.";
                    break;

                case "PDF aguardando processamento":
                    TempData["Sucesso"] =
                        "PDF enviado e salvo. O processamento do PDF ainda será implementado.";
                    break;

                default:
                    TempData["Sucesso"] =
                        "Nota fiscal enviada e registrada no banco de dados.";
                    break;
            }
        }
        catch
        {
            // Apaga o arquivo se o registro no banco falhar.
            if (System.IO.File.Exists(caminhoCompleto))
            {
                System.IO.File.Delete(caminhoCompleto);
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Lista()
    {
        var notas = await _db.NotasFiscais
            .Include(n => n.Compra)
            .OrderByDescending(n => n.DataEnvio)
            .ToListAsync();

        return View(notas);
    }

    public async Task<IActionResult> Abrir(int id)
    {
        var nota = await _db.NotasFiscais.FindAsync(id);

        if (nota == null)
        {
            return NotFound();
        }

        var caminho = Path.Combine(
            _environment.WebRootPath,
            nota.CaminhoArquivo
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(caminho))
        {
            return NotFound();
        }

        return PhysicalFile(
            caminho,
            nota.TipoConteudo,
            enableRangeProcessing: true);
    }

    public async Task<IActionResult> Manual()
    {
        ViewBag.Notas = await _db.NotasFiscais
            .Where(n => n.CompraId == null)
            .OrderByDescending(n => n.DataEnvio)
            .ToListAsync();

        return View(new Compra
        {
            DataCompra = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manual(
        Compra compra,
        int? notaFiscalId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Notas = await _db.NotasFiscais
                .Where(n => n.CompraId == null)
                .OrderByDescending(n => n.DataEnvio)
                .ToListAsync();

            return View(compra);
        }

        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();

        if (notaFiscalId.HasValue)
        {
            var nota = await _db.NotasFiscais
                .FindAsync(notaFiscalId.Value);

            if (nota != null)
            {
                nota.CompraId = compra.Id;
                nota.StatusProcessamento =
                    "Compra vinculada manualmente";

                await _db.SaveChangesAsync();
            }
        }

        TempData["Sucesso"] = "Compra cadastrada com sucesso.";

        return RedirectToAction(nameof(Lista));
    }
}
