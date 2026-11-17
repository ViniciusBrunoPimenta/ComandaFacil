using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ComandaFacil.Controllers;

public abstract class BaseController : Controller
{
    protected int UsuarioLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected void Sucesso(string mensagem) => TempData["Sucesso"] = mensagem;

    protected void Erro(string mensagem) => TempData["Erro"] = mensagem;
}
