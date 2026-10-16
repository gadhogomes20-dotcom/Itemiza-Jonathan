using System.ComponentModel.DataAnnotations;

namespace Itemiza.Models;

public class NotaFiscal
{
    public int Id { get; set; }

    [Required, StringLength(255)]
    public string NomeArquivo { get; set; } = "";

    [Required, StringLength(500)]
    public string CaminhoArquivo { get; set; } = "";

    [Required, StringLength(100)]
    public string TipoConteudo { get; set; } = "";

    public long TamanhoBytes { get; set; }

    public DateTime DataEnvio { get; set; } = DateTime.Now;

    [StringLength(30)]
    public string StatusProcessamento { get; set; } = "Aguardando análise";

    // Texto reconhecido pelo OCR da nota fiscal.
    public string? TextoExtraido { get; set; }

    public int? CompraId { get; set; }

    public Compra? Compra { get; set; }
}
