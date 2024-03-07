using Models.Extrato;
using System.Text.Json.Serialization;

namespace rinha_de_backend_2024_q1_dotnet_API.Models.Extrato
{
    public record struct ExtratoViewModel
    {
        public SaldoExtratoResponse Saldo { get; set; }
        [JsonPropertyName("ultimas_transacoes")]
        public List<TransacaoExtratoResponse> UltimasTransacoes { get; set; }
    }
}
