using System.ComponentModel.DataAnnotations;

namespace ComandaFacil.Models;

public class Mesa
{
    public int Id { get; set; }

    [Range(1, 999)]
    [Display(Name = "Número")]
    public int Numero { get; set; }

    public StatusMesa Status { get; set; } = StatusMesa.Livre;

    public List<Comanda> Comandas { get; set; } = new();

    public void Ocupar()
    {
        if (Status != StatusMesa.Livre)
            throw new InvalidOperationException($"A mesa {Numero} não está livre.");
        Status = StatusMesa.Ocupada;
    }

    public void SolicitarConta()
    {
        if (Status == StatusMesa.Livre)
            throw new InvalidOperationException($"A mesa {Numero} não tem comanda aberta.");
        Status = StatusMesa.AguardandoPagamento;
    }

    public void Liberar() => Status = StatusMesa.Livre;
}
