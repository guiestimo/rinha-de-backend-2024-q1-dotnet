using System.Text.Json.Serialization;

namespace rinha_de_backend_2024_q1_dotnet_API.Entities
{
    public record Transacao(int Valor, char Tipo, DateTime DataTransacao, string Descricao, int ClienteId)
    {
        [JsonIgnore]
        public int ClienteId { get; private set; } = ClienteId;

        public int Valor { get; private set; } = Valor;

        public char Tipo { get; private set; } = Tipo;

        public string Descricao { get; private set; } = Descricao;

        [JsonPropertyName("realizada_em")]
        public DateTime DataTransacao { get; private set; } = DataTransacao;
    }
}
