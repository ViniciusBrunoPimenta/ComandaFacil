using ComandaFacil.Data;
using ComandaFacil.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

// UC01 - Gerenciar produtos
[Authorize(Roles = nameof(PerfilUsuario.Administrador))]
public class ProdutosController : BaseController
{
    private readonly AppDbContext _db;

    public ProdutosController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var produtos = await _db.Produtos.AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Categoria!.Nome).ThenBy(p => p.Nome)
            .ToListAsync();
        return View(produtos);
    }

    public async Task<IActionResult> Criar()
    {
        if (!await _db.Categorias.AnyAsync())
        {
            Erro("Cadastre uma categoria antes de cadastrar produtos.");
            return RedirectToAction("Criar", "Categorias");
        }

        await CarregarCategorias();
        return View("Form", new Produto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar([Bind("Nome,Preco,CategoriaId,Ativo")] Produto produto)
    {
        await ValidarCategoria(produto);
        if (!ModelState.IsValid)
        {
            await CarregarCategorias();
            return View("Form", produto);
        }

        produto.Nome = produto.Nome.Trim();
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        Sucesso($"{produto.Nome} cadastrado.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var produto = await _db.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();

        await CarregarCategorias();
        return View("Form", produto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Preco,CategoriaId,Ativo")] Produto produto)
    {
        if (id != produto.Id)
            return BadRequest();

        await ValidarCategoria(produto);
        if (!ModelState.IsValid)
        {
            await CarregarCategorias();
            return View("Form", produto);
        }

        produto.Nome = produto.Nome.Trim();
        _db.Produtos.Update(produto);
        await _db.SaveChangesAsync();

        Sucesso($"{produto.Nome} atualizado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var produto = await _db.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();

        // RN03: produto que já foi vendido é só desativado, para manter o histórico
        var jaVendido = await _db.ItensComanda.AnyAsync(i => i.ProdutoId == id);
        if (jaVendido)
        {
            produto.Ativo = false;
            Sucesso($"{produto.Nome} já foi vendido antes, então foi desativado em vez de excluído.");
        }
        else
        {
            _db.Produtos.Remove(produto);
            Sucesso($"{produto.Nome} excluído.");
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarCategorias()
    {
        var categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.Id), nameof(Categoria.Nome));
    }

    private async Task ValidarCategoria(Produto produto)
    {
        if (produto.CategoriaId > 0 && !await _db.Categorias.AnyAsync(c => c.Id == produto.CategoriaId))
            ModelState.AddModelError(nameof(Produto.CategoriaId), "Categoria inválida.");
    }
}
