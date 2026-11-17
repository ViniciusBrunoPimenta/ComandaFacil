using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.Models;

public enum PerfilUsuario
{
    Atendente,
    Administrador
}

public enum StatusMesa
{
    Livre,
    Ocupada,
    [Display(Name = "Aguardando pagamento")]
    AguardandoPagamento
}

public enum StatusComanda
{
    Aberta,
    Fechada
}

public enum FormaPagamento
{
    Dinheiro,
    Pix,
    [Display(Name = "Cartão de débito")]
    CartaoDebito,
    [Display(Name = "Cartão de crédito")]
    CartaoCredito
}
