using ComandaFacil.Models;

namespace ComandaFacil.ViewModels;

public class PainelMesaViewModel
{
    public int MesaId { get; set; }
    public int Numero { get; set; }
    public StatusMesa Status { get; set; }
    public int? ComandaId { get; set; }
    public decimal? Total { get; set; }
    public DateTime? AbertaEm { get; set; }
}
