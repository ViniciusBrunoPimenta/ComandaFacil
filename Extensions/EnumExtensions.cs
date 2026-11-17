using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ComandaFacil.Extensions;

public static class EnumExtensions
{
    /// <summary>Nome amigável do enum (usa [Display(Name = ...)] quando existir).</summary>
    public static string Nome(this Enum valor)
    {
        var membro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        return membro?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();
    }
}
