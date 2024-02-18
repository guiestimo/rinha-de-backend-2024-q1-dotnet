namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public static class Scripts
    {
        public static string GetCliente =
            "SELECT ID FROM CLIENTE WHERE ID = @IdCliente";

        public static string InsertTransaction =
            "INSERT INTO transacao(id_cliente, valor, tipo, descricao) VALUES(@idCliente, @valor, @tipo, @descricao)";

        public static string UpdateSaldo(char tipo) =>
            $"UPDATE cliente SET saldo = saldo {(tipo == 'c' ? '+' : '-')} @valor WHERE ID = @idCliente RETURNING limite, saldo";

        public static string GetExtrato =
            "SELECT valor, tipo, descricao, data_transacao, id_cliente FROM transacao where id_cliente = @idCliente " +
            "ORDER BY data_transacao DESC " +
            "LIMIT 10";


    }
}
