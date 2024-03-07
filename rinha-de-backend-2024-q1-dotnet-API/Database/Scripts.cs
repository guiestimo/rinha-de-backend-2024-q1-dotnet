namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public static class Scripts
    {

        public static string InsertDebitoTransaction =
            "select * from debito_transaction($1, $2, $3)";

        public static string InsertCreditoTransaction =
            "select * from credito_transaction($1, $2, $3)";


        public static string GetExtrato =
            "SELECT valor, tipo, descricao, data_transacao, id_cliente FROM transacao where id_cliente = $1 " +
            "ORDER BY data_transacao DESC " +
            "LIMIT 10";

        public static string GetSaldo =
            "SELECT saldo, limite FROM CLIENTE WHERE ID = $1";

    }
}
