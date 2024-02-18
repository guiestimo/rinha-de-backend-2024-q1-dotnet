using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Options;

namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public class Database(IOptions<DbOptions> dbOptions)
    {
        private readonly NpgsqlDataSource _dataSource = new NpgsqlSlimDataSourceBuilder(dbOptions.Value.ConnectionString).Build();

        private NpgsqlConnection CreateConnection() => _dataSource.OpenConnection();

        public async Task<int?> GetClienteAsync(int idCliente)
        {
            using var connection = CreateConnection();
            using var command = new NpgsqlCommand(Scripts.GetCliente);
            command.Connection = connection;

            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = await command.ExecuteReaderAsync();
            int? id = null;
            while (await reader.ReadAsync())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        public async Task<object?> AddTransacaoAsync(Transacao transacao)
        {
            object? response = null;
            try
            {
                response = await UpdateSaldoAsync(transacao);
                using var connection = CreateConnection();
                using var command = new NpgsqlCommand(Scripts.InsertTransaction);
                command.Connection = connection;
                command.Parameters.AddWithValue("@idCliente", NpgsqlDbType.Integer, transacao.ClienteId);
                command.Parameters.AddWithValue("@valor", NpgsqlDbType.Integer, transacao.Valor);
                command.Parameters.AddWithValue("@tipo", NpgsqlDbType.Varchar, transacao.Tipo);
                command.Parameters.AddWithValue("@descricao", NpgsqlDbType.Varchar, transacao.Descricao);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception) { }

            return response;
        }

        private async Task<object?> UpdateSaldoAsync(Transacao transacao)
        {
            using var connection = CreateConnection();
            using var command = new NpgsqlCommand(Scripts.UpdateSaldo(transacao.Tipo));
            command.Connection = connection;
            command.Parameters.AddWithValue("@valor", NpgsqlDbType.Integer, transacao.Valor);
            command.Parameters.AddWithValue("@idCliente", NpgsqlDbType.Integer, transacao.ClienteId);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return new { Limite = reader.GetInt32(0), Saldo = reader.GetInt32(1) };
            }

            return null;
        }

        public async Task<ExtratoViewModel> GetExtratoAsync(int idCliente)
        {
            var extrato = new ExtratoViewModel()
            {
                Saldo = await GetSaldoAsync(idCliente),
                UltimasTransacoes = []
            };

            using var command = new NpgsqlCommand(Scripts.GetExtrato, CreateConnection());
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                extrato.UltimasTransacoes.Add(new Transacao(
                    reader.GetInt32(0),
                    reader.GetChar(1),
                    reader.GetDateTime(3),
                    reader.GetString(2),
                    reader.GetInt32(4)));
            }

            return extrato;
        }

        private async Task<SaldoViewModel> GetSaldoAsync(int idCliente)
        {
            var saldo = new SaldoViewModel();
            using var connection = CreateConnection();
            using var command = new NpgsqlCommand("SELECT saldo, limite FROM CLIENTE WHERE ID = @IdCliente");
            command.Connection = connection;
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                saldo = new SaldoViewModel
                {
                    Total = reader.GetInt32(0),
                    Limite = reader.GetInt32(1),
                    DataExtrato = DateTime.Now
                };
            }

            return saldo;
        }
    }
}
