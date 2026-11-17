using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComandaFacil.Models;

public class ItemComanda
{
    public int Id { get; set; }

    public int ComandaId { get; set; }
    public Comanda? Comanda { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    [Range(1, 99)]
    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    [StringLength(200)]
    [Display(Name = "Observação")]
    public string? Observacao { get; set; }

    [NotMapped]
    public decimal Subtotal => Quantidade * PrecoUnitario;
}
