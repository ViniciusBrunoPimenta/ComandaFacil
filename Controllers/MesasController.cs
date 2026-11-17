using ComandaFacil.Data;
using ComandaFacil.Models;
using ComandaFacil.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

public class MesasController : BaseController
{
    private readonly AppDbContext _db;

    public MesasController(AppDbContext db) => _db = db;

    // Painel de mesas com cor por status
    public async Task<IActionResult> Index()
    {
        var mesas = await _db.Mesas.AsNoTracking().OrderBy(m => m.Numero).ToListAsync();

        var abertas = await _db.Comandas.AsNoTracking()
            .Where(c => c.Status == StatusComanda.Aberta)
            .Select(c => new { c.Id, c.MesaId, c.Total, c.DataAbertura })
            .ToListAsync();

        var painel = mesas.Select(m =>
        {
            var comanda = abertas.FirstOrDefault(c => c.MesaId == m.Id);
            return new PainelMesaViewModel
            {
                MesaId = m.Id,
                Numero = m.Numero,
                Status = m.Status,
                ComandaId = comanda?.Id,
                Total = comanda?.Total,
                AbertaEm = comanda?.DataAbertura
            };
        }).ToList();

        return View(painel);
    }
}
