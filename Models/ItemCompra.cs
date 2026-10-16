
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itemiza.Models;

public class ItemCompra
{
    public int Id { get; set; }

    public int CompraId { get; set; }

    public Compra? Compra { get; set; }

    [Required, StringLength(200)]
    public string Descricao { get; set; } = "";

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
