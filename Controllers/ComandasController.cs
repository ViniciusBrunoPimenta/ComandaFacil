using System.Globalization;
using ComandaFacil.Data;
using ComandaFacil.Models;
using ComandaFacil.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

public class ComandasController : BaseController
{
    // Ordem alfabética do português ("Água" antes de "Refrigerante"); o SQLite ordena por bytes
    private static readonly StringComparer OrdemPtBr =
        StringComparer.Create(new CultureInfo("pt-BR"), ignoreCase: true);

    private readonly AppDbContext _db;

    public ComandasController(AppDbContext db) => _db = db;

    // UC02 - Abrir comanda em uma mesa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(int mesaId)
    {
        var mesa = await _db.Mesas.FindAsync(mesaId);
        if (mesa == null)
            return NotFound();

        var existente = await _db.Comandas
            .FirstOrDefaultAsync(c => c.MesaId == mesaId && c.Status == StatusComanda.Aberta);
        if (existente != null)
            return RedirectToAction(nameof(Detalhes), new { id = existente.Id });

        try
        {
            mesa.Ocupar();
            var comanda = new Comanda { MesaId = mesa.Id, UsuarioId = UsuarioLogadoId };
            _db.Comandas.Add(comanda);
            await _db.SaveChangesAsync();

            Sucesso($"Comanda aberta na mesa {mesa.Numero}.");
            return RedirectToAction(nameof(Detalhes), new { id = comanda.Id });
        }
        catch (InvalidOperationException ex)
        {
            Erro(ex.Message);
        }
        catch (DbUpdateException)
        {
            Erro($"A mesa {mesa.Numero} acabou de ser aberta por outra pessoa.");
        }

        return RedirectToAction("Index", "Mesas");
    }

    // Tela da comanda (itens + cardápio)
    public async Task<IActionResult> Detalhes(int id)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        var produtos = (await _db.Produtos.AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Ativo)
                .ToListAsync())
            .OrderBy(p => p.Categoria!.Nome, OrdemPtBr).ThenBy(p => p.Nome, OrdemPtBr)
            .ToList();

        var modelo = new ComandaDetalhesViewModel
        {
            Comanda = comanda,
            Cardapio = produtos.GroupBy(p => p.Categoria!.Nome).ToList()
        };

        return View(modelo);
    }

    // UC03 - Lançar item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarItem(int id, int produtoId, int quantidade = 1, string? observacao = null)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        var produto = await _db.Produtos.FindAsync(produtoId);
        if (produto == null)
        {
            Erro("Produto não encontrado.");
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        try
        {
            comanda.AdicionarItem(produto, quantidade, observacao);

            // Se a conta já tinha sido pedida e o cliente pediu mais, a mesa volta a "Ocupada"
            if (comanda.Mesa!.Status == StatusMesa.AguardandoPagamento)
                comanda.Mesa.Status = StatusMesa.Ocupada;

            await _db.SaveChangesAsync();
            Sucesso($"{quantidade}x {produto.Nome} lançado.");
        }
        catch (InvalidOperationException ex)
        {
            Erro(ex.Message);
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // UC03 - Remover item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverItem(int id, int itemId)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        try
        {
            var item = comanda.RemoverItem(itemId);
            _db.ItensComanda.Remove(item);
            await _db.SaveChangesAsync();
            Sucesso($"{item.Produto?.Nome} removido.");
        }
        catch (InvalidOperationException ex)
        {
            Erro(ex.Message);
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    // UC04 - Cliente pediu a conta: mesa vai para "Aguardando pagamento"
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PedirConta(int id)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        if (comanda.Status == StatusComanda.Fechada)
            return RedirectToAction("Index", "Mesas");

        if (comanda.Itens.Count == 0)
        {
            Erro("Lance pelo menos um item antes de fechar a conta.");
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        comanda.Mesa!.SolicitarConta();
        comanda.CalcularTotal();
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Fechar), new { id });
    }

    // UC04 - Tela de fechamento (total + forma de pagamento)
    [HttpGet]
    public async Task<IActionResult> Fechar(int id)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        if (comanda.Status == StatusComanda.Fechada)
            return RedirectToAction("Index", "Mesas");

        return View(comanda);
    }

    // UC04 - Registrar pagamento, fechar comanda e liberar mesa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fechar(int id, FormaPagamento? formaPagamento)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        if (formaPagamento is null || !Enum.IsDefined(formaPagamento.Value))
        {
            Erro("Escolha a forma de pagamento.");
            return RedirectToAction(nameof(Fechar), new { id });
        }

        try
        {
            comanda.Fechar(formaPagamento.Value);
            comanda.Mesa!.Liberar();
            await _db.SaveChangesAsync();

            Sucesso($"Mesa {comanda.Mesa.Numero} liberada. Recebido {comanda.Total:C}.");
            return RedirectToAction("Index", "Mesas");
        }
        catch (InvalidOperationException ex)
        {
            Erro(ex.Message);
            return RedirectToAction(nameof(Detalhes), new { id });
        }
    }

    // Comanda aberta por engano (sem itens): libera a mesa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var comanda = await CarregarComanda(id);
        if (comanda == null)
            return NotFound();

        if (comanda.Status == StatusComanda.Fechada || comanda.Itens.Count > 0)
        {
            Erro("Só é possível liberar a mesa de uma comanda aberta e sem itens.");
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        comanda.Mesa!.Liberar();
        _db.Comandas.Remove(comanda);
        await _db.SaveChangesAsync();

        Sucesso($"Mesa {comanda.Mesa.Numero} liberada.");
        return RedirectToAction("Index", "Mesas");
    }

    private Task<Comanda?> CarregarComanda(int id) =>
        _db.Comandas
            .Include(c => c.Mesa)
            .Include(c => c.Usuario)
            .Include(c => c.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(c => c.Id == id);
}
