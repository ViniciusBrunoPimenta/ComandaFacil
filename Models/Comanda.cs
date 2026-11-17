namespace ComandaFacil.Models;

public class Comanda
{
    public int Id { get; set; }

    public int MesaId { get; set; }
    public Mesa? Mesa { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime DataAbertura { get; set; } = DateTime.Now;
    public DateTime? DataFechamento { get; set; }

    public StatusComanda Status { get; set; } = StatusComanda.Aberta;
    public FormaPagamento? FormaPagamento { get; set; }
    public decimal Total { get; set; }

    public List<ItemComanda> Itens { get; set; } = new();

    public ItemComanda AdicionarItem(Produto produto, int quantidade, string? observacao)
    {
        GarantirAberta();
        if (!produto.Ativo)
            throw new InvalidOperationException($"{produto.Nome} não está disponível para venda.");
        if (quantidade < 1 || quantidade > 99)
            throw new InvalidOperationException("A quantidade deve ser entre 1 e 99.");
        if (observacao?.Trim().Length > 200)
            throw new InvalidOperationException("A observação pode ter no máximo 200 caracteres.");

        var item = new ItemComanda
        {
            ProdutoId = produto.Id,
            Produto = produto,
            Quantidade = quantidade,
            // Regra de negócio: guarda o preço do momento do pedido
            PrecoUnitario = produto.Preco,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim()
        };

        Itens.Add(item);
        CalcularTotal();
        return item;
    }

    public ItemComanda RemoverItem(int itemId)
    {
        GarantirAberta();
        var item = Itens.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item não encontrado nesta comanda.");

        Itens.Remove(item);
        CalcularTotal();
        return item;
    }

    public decimal CalcularTotal()
    {
        Total = Itens.Sum(i => i.Subtotal);
        return Total;
    }

    public void Fechar(FormaPagamento forma)
    {
        GarantirAberta();
        if (Itens.Count == 0)
            throw new InvalidOperationException("Lance pelo menos um item antes de fechar a comanda.");

        CalcularTotal();
        FormaPagamento = forma;
        Status = StatusComanda.Fechada;
        DataFechamento = DateTime.Now;
    }

    private void GarantirAberta()
    {
        if (Status != StatusComanda.Aberta)
            throw new InvalidOperationException("Esta comanda já foi fechada.");
    }
}
