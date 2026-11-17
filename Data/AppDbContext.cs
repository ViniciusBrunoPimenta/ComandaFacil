using ComandaFacil.Models;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ItemComanda> ItensComanda => Set<ItemComanda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasIndex(u => u.Login).IsUnique();
            e.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Mesa>(e =>
        {
            e.ToTable("Mesa");
            e.HasIndex(m => m.Numero).IsUnique();
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasIndex(c => c.Nome).IsUnique();
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("Produto");
            e.Property(p => p.Preco).HasPrecision(10, 2);
            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Produtos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comanda>(e =>
        {
            e.ToTable("Comanda");
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.FormaPagamento).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Total).HasPrecision(10, 2);

            e.HasOne(c => c.Mesa)
             .WithMany(m => m.Comandas)
             .HasForeignKey(c => c.MesaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Usuario)
             .WithMany(u => u.Comandas)
             .HasForeignKey(c => c.UsuarioId)
             .OnDelete(DeleteBehavior.Restrict);

            // RN01: só uma comanda aberta por mesa, garantido também no banco
            e.HasIndex(c => c.MesaId)
             .IsUnique()
             .HasFilter("\"Status\" = 'Aberta'");
        });

        modelBuilder.Entity<ItemComanda>(e =>
        {
            e.ToTable("ItemComanda");
            e.Property(i => i.PrecoUnitario).HasPrecision(10, 2);

            e.HasOne(i => i.Comanda)
             .WithMany(c => c.Itens)
             .HasForeignKey(i => i.ComandaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Produto)
             .WithMany(p => p.Itens)
             .HasForeignKey(i => i.ProdutoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
