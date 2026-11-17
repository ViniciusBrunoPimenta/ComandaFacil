using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.Models;

public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do produto.")]
    [StringLength(100, ErrorMessage = "Use no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, 99999.99, ErrorMessage = "Informe um preço maior que zero.")]
    [Display(Name = "Preço (R$)")]
    public decimal Preco { get; set; }

    [Display(Name = "Disponível para venda")]
    public bool Ativo { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Escolha uma categoria.")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public List<ItemComanda> Itens { get; set; } = new();
}
