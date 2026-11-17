using ComandaFacil.Data;
using ComandaFacil.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

// UC01 - Gerenciar categorias
[Authorize(Roles = nameof(PerfilUsuario.Administrador))]
public class CategoriasController : BaseController
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var categorias = await _db.Categorias.AsNoTracking()
            .Include(c => c.Produtos)
            .OrderBy(c => c.Nome)
            .ToListAsync();
        return View(categorias);
    }

    public IActionResult Criar() => View("Form", new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar([Bind("Nome")] Categoria categoria)
    {
        await ValidarNomeUnico(categoria);
        if (!ModelState.IsValid)
            return View("Form", categoria);

        categoria.Nome = categoria.Nome.Trim();
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        Sucesso($"Categoria {categoria.Nome} cadastrada.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        return categoria == null ? NotFound() : View("Form", categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Nome")] Categoria categoria)
    {
        if (id != categoria.Id)
            return BadRequest();

        await ValidarNomeUnico(categoria);
        if (!ModelState.IsValid)
            return View("Form", categoria);

        categoria.Nome = categoria.Nome.Trim();
        _db.Categorias.Update(categoria);
        await _db.SaveChangesAsync();

        Sucesso($"Categoria {categoria.Nome} atualizada.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var categoria = await _db.Categorias.Include(c => c.Produtos).FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null)
            return NotFound();

        if (categoria.Produtos.Count > 0)
        {
            Erro($"A categoria {categoria.Nome} tem produtos. Mova ou exclua os produtos antes.");
            return RedirectToAction(nameof(Index));
        }

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();

        Sucesso($"Categoria {categoria.Nome} excluída.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarNomeUnico(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nome))
            return;

        var nome = categoria.Nome.Trim().ToLower();
        var existe = await _db.Categorias.AnyAsync(c => c.Nome.ToLower() == nome && c.Id != categoria.Id);
        if (existe)
            ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");
    }
}
