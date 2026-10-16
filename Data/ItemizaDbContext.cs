
using Itemiza.Models;
using Microsoft.EntityFrameworkCore;

namespace Itemiza.Data;

public class ItemizaDbContext : DbContext
{
    public ItemizaDbContext(
        DbContextOptions<ItemizaDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();

    public DbSet<Compra> Compras => Set<Compra>();

    public DbSet<ItemCompra> ItensCompra => Set<ItemCompra>();
}
