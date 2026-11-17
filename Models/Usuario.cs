using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Atendente;

    public bool Ativo { get; set; } = true;

    public List<Comanda> Comandas { get; set; } = new();
}
