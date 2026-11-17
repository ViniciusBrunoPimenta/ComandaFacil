using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o usuário.")]
    [Display(Name = "Usuário")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;
}
