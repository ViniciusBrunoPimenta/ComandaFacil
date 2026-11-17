using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(60, ErrorMessage = "Use no máximo 60 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public List<Produto> Produtos { get; set; } = new();
}
