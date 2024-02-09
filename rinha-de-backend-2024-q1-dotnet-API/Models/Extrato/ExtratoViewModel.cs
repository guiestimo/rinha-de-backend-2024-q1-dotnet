using rinha_de_backend_2024_q1_dotnet_API.Entities;

namespace rinha_de_backend_2024_q1_dotnet_API.Models.Extrato
{
    public record ExtratoViewModel
    {
        public SaldoViewModel Saldo { get; set; }
        public List<Transacao> UltimasTransacoes { get; set; }
    }
}
