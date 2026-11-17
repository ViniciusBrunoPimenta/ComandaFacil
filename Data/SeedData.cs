using ComandaFacil.Models;
using Microsoft.AspNetCore.Identity;

namespace ComandaFacil.Data;

public static class SeedData
{
    public const int QuantidadeDeMesas = 12;

    public static void Popular(AppDbContext db)
    {
        if (!db.Usuarios.Any())
        {
            var hasher = new PasswordHasher<Usuario>();

            var admin = new Usuario { Nome = "Dono", Login = "admin", Perfil = PerfilUsuario.Administrador };
            admin.SenhaHash = hasher.HashPassword(admin, "admin123");

            var atendente = new Usuario { Nome = "Atendente", Login = "atendente", Perfil = PerfilUsuario.Atendente };
            atendente.SenhaHash = hasher.HashPassword(atendente, "atendente123");

            db.Usuarios.AddRange(admin, atendente);
        }

        if (!db.Mesas.Any())
        {
            for (var numero = 1; numero <= QuantidadeDeMesas; numero++)
                db.Mesas.Add(new Mesa { Numero = numero });
        }

        if (!db.Categorias.Any())
        {
            var lanches = new Categoria { Nome = "Lanches" };
            var porcoes = new Categoria { Nome = "Porções" };
            var bebidas = new Categoria { Nome = "Bebidas" };
            var sobremesas = new Categoria { Nome = "Sobremesas" };

            db.Produtos.AddRange(
                new Produto { Nome = "X-Burguer", Preco = 22.00m, Categoria = lanches },
                new Produto { Nome = "X-Salada", Preco = 24.00m, Categoria = lanches },
                new Produto { Nome = "X-Bacon", Preco = 27.00m, Categoria = lanches },
                new Produto { Nome = "Misto quente", Preco = 12.00m, Categoria = lanches },
                new Produto { Nome = "Batata frita", Preco = 25.00m, Categoria = porcoes },
                new Produto { Nome = "Calabresa acebolada", Preco = 32.00m, Categoria = porcoes },
                new Produto { Nome = "Refrigerante lata", Preco = 6.00m, Categoria = bebidas },
                new Produto { Nome = "Suco natural", Preco = 9.00m, Categoria = bebidas },
                new Produto { Nome = "Água mineral", Preco = 4.00m, Categoria = bebidas },
                new Produto { Nome = "Pudim", Preco = 8.00m, Categoria = sobremesas },
                new Produto { Nome = "Açaí 300 ml", Preco = 15.00m, Categoria = sobremesas });
        }

        db.SaveChanges();
    }
}
