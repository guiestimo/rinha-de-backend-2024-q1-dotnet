using System.Text.Json.Serialization;

namespace rinha_de_backend_2024_q1_dotnet_API.Entities
{
    public class Cliente(int id, int limite, int saldo)
    {
        [JsonIgnore]
        public int Id { get; private set; } = id;
        public int Limite { get; private set; } = limite;
        public int Saldo { get; private set; } = saldo;

        public void AtualizarSaldo(int valor, char tipo)
        {
            if (tipo.Equals('c'))
                Saldo += valor;
            else
                Saldo -= valor;
        }
    }
}
