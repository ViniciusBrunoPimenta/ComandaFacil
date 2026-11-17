using ComandaFacil.Models;

namespace ComandaFacil.ViewModels;

public class ComandaDetalhesViewModel
{
    public Comanda Comanda { get; set; } = null!;
    public List<IGrouping<string, Produto>> Cardapio { get; set; } = new();
}
