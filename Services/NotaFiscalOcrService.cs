using Tesseract;

namespace Itemiza.Services;

public class NotaFiscalOcrService
{
    private readonly IWebHostEnvironment _environment;

    public NotaFiscalOcrService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string ExtrairTexto(string caminhoImagem)
    {
        var pastaIdioma = Path.Combine(
            _environment.ContentRootPath,
            "tessdata");

        using var engine = new TesseractEngine(
            pastaIdioma,
            "por",
            EngineMode.Default);

        // Carrega a imagem original da nota fiscal.
        using var imagemOriginal = Pix.LoadFromFile(
            caminhoImagem);

        // Amplia a imagem para tentar melhorar a leitura.
        using var imagemAmpliada = imagemOriginal.Scale(
            2f,
            2f);

        // Executa o OCR utilizando o idioma português.
        using var pagina = engine.Process(
            imagemAmpliada,
            PageSegMode.Auto);

        var texto = pagina.GetText();

        Console.WriteLine(
            "===== TEXTO EXTRAÍDO DA NOTA =====");

        Console.WriteLine(texto);

        Console.WriteLine(
            "===== FIM DO TEXTO =====");

        return texto;
    }
}
    