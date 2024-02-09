using System.Text.Json.Serialization;

namespace rinha_de_backend_2024_q1_dotnet_API.Entities
{
    public class Transacao(int valor, char tipo, DateTime dataTransacao, string descricao, int clienteId)
    {
        [JsonIgnore]
        public int ClienteId { get; private set; } = clienteId;
        public int Valor { get; private set; } = valor;
        public char Tipo { get; private set; } = tipo;
        public DateTime DataTransacao { get; private set; } = dataTransacao;
        public string Descricao { get; private set; } = descricao;
    }
}
