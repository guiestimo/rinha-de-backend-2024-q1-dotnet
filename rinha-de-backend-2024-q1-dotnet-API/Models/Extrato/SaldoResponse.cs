using System.Text.Json.Serialization;

namespace rinha_de_backend_2024_q1_dotnet_API.Models.Extrato
{
    public record SaldoResponse
    {
        public int Total { get; set; }

        [JsonPropertyName("data_extrato")]
        public DateTime DataExtrato { get; set; }

        public int Limite { get; set; }
    }
}
