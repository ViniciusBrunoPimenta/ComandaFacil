using ComandaFacil.Data;
using ComandaFacil.Models;
using ComandaFacil.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

// UC05 - Consultar vendas do dia
[Authorize(Roles = nameof(PerfilUsuario.Administrador))]
public class RelatoriosController : BaseController
{
    private readonly AppDbContext _db;

    public RelatoriosController(AppDbContext db) => _db = db;

    public async Task<IActionResult> VendasDoDia(DateTime? data)
    {
        var dia = (data ?? DateTime.Today).Date;
        var diaSeguinte = dia.AddDays(1);

        var comandas = await _db.Comandas.AsNoTracking()
            .Include(c => c.Mesa)
            .Include(c => c.Itens).ThenInclude(i => i.Produto)
            .Where(c => c.Status == StatusComanda.Fechada
                     && c.DataFechamento >= dia
                     && c.DataFechamento < diaSeguinte)
            .ToListAsync();

        // Somas feitas em memória: o SQLite não soma colunas decimal no servidor
        var modelo = new VendasDoDiaViewModel
        {
            Data = dia,
            TotalVendido = comandas.Sum(c => c.Total),
            QuantidadeComandas = comandas.Count,
            Comandas = comandas.OrderByDescending(c => c.DataFechamento).ToList(),
            MaisVendidos = comandas
                .SelectMany(c => c.Itens)
                .GroupBy(i => i.Produto!.Nome)
                .Select(g => new ProdutoVendido(g.Key, g.Sum(i => i.Quantidade), g.Sum(i => i.Subtotal)))
                .OrderByDescending(p => p.Quantidade).ThenByDescending(p => p.Valor)
                .Take(10)
                .ToList(),
            PorFormaPagamento = comandas
                .Where(c => c.FormaPagamento.HasValue)
                .GroupBy(c => c.FormaPagamento!.Value)
                .Select(g => new VendaPorPagamento(g.Key, g.Count(), g.Sum(c => c.Total)))
                .OrderByDescending(v => v.Valor)
                .ToList()
        };

        return View(modelo);
    }
}
