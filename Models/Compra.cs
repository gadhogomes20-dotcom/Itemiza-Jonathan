
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itemiza.Models;

public class Compra
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Descricao { get; set; } = "";

    [Range(0.01, 999999999)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Required, StringLength(80)]
    public string Categoria { get; set; } = "Outros";

    public DateTime DataCompra { get; set; } = DateTime.Today;

    [StringLength(150)]
    public string? Empresa { get; set; }

    public TimeOnly? HoraCompra { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public ICollection<NotaFiscal> NotasFiscais { get; set; }
        = new List<NotaFiscal>();

    public ICollection<ItemCompra> Itens { get; set; }
        = new List<ItemCompra>();
}
