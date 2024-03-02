using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using rinha_de_backend_2024_q1_dotnet_API.Options;
using System.Drawing;

namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public class Database(IOptions<DbOptions> dbOptions)
    {
        private readonly NpgsqlDataSource _dataSource = new NpgsqlSlimDataSourceBuilder(dbOptions.Value.ConnectionString).Build();

        private async Task<NpgsqlConnection> CreateConnectionAsync() => await _dataSource.OpenConnectionAsync();

        public async Task<TransacoesResponse?> AddTransactionAsync(int valor, int id, string descricao, char tipo)
        {
            NpgsqlCommand command;
            if (tipo == 'd')
                command = GetDebitoCommand();
            else
                command = GetCreditoCommand();

            using var connection = await CreateConnectionAsync();
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar });
            command.Parameters[0].Value = valor;
            command.Parameters[1].Value = id;
            command.Parameters[2].Value = descricao;

            command.Connection = connection;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader.GetBoolean(2) is false)
                {
                    return null;
                }

                return new TransacoesResponse(reader.GetInt32(1), reader.GetInt32(0));
            }

            return null;
        }

        private static NpgsqlCommand GetCreditoCommand()
        {
            return new NpgsqlCommand(Scripts.InsertCreditoTransaction);
        }

        private static NpgsqlCommand GetDebitoCommand()
        {
            return new NpgsqlCommand(Scripts.InsertDebitoTransaction);
        }

        public async Task<ExtratoViewModel> GetExtratoAsync(int idCliente)
        {
            var extrato = new ExtratoViewModel()
            {
                Saldo = await GetSaldoAsync(idCliente),
                UltimasTransacoes = []
            };

            using var command = new NpgsqlCommand(Scripts.GetExtrato, await CreateConnectionAsync());
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

        private async Task<SaldoResponse> GetSaldoAsync(int idCliente)
        {
            var saldo = new SaldoResponse();
            using var connection = await CreateConnectionAsync();
            using var command = new NpgsqlCommand("SELECT saldo, limite FROM CLIENTE WHERE ID = @IdCliente");
            command.Connection = connection;
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                saldo = new SaldoResponse
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
