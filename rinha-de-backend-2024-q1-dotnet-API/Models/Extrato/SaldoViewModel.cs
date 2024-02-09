namespace rinha_de_backend_2024_q1_dotnet_API.Models.Extrato
{
    public record SaldoViewModel
    {
        public int Total { get; set; }
        public DateTime DataExtrato { get; set; }
        public int Limite { get; set; }
    }
}
