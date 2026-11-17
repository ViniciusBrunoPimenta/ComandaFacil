using System.Security.Claims;
using ComandaFacil.Data;
using ComandaFacil.Models;
using ComandaFacil.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComandaFacil.Controllers;

public class ContaController : BaseController
{
    private readonly AppDbContext _db;

    public ContaController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Mesas");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(modelo);

        var login = modelo.Login.Trim().ToLower();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Login.ToLower() == login && u.Ativo);

        var senhaCorreta = usuario != null &&
            new PasswordHasher<Usuario>().VerifyHashedPassword(usuario, usuario.SenhaHash, modelo.Senha)
                != PasswordVerificationResult.Failed;

        if (!senhaCorreta)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");
            return View(modelo);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };
        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Mesas");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AcessoNegado() => View();
}
