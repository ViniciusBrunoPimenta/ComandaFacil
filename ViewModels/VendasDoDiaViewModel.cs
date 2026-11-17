using ComandaFacil.Models;

namespace ComandaFacil.ViewModels;

public class VendasDoDiaViewModel
{
    public DateTime Data { get; set; }
    public decimal TotalVendido { get; set; }
    public int QuantidadeComandas { get; set; }
    public decimal TicketMedio => QuantidadeComandas == 0 ? 0 : TotalVendido / QuantidadeComandas;
    public List<ProdutoVendido> MaisVendidos { get; set; } = new();
    public List<VendaPorPagamento> PorFormaPagamento { get; set; } = new();
    public List<Comanda> Comandas { get; set; } = new();
}

public record ProdutoVendido(string Produto, int Quantidade, decimal Valor);

public record VendaPorPagamento(FormaPagamento Forma, int Quantidade, decimal Valor);
