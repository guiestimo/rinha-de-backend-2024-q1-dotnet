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
    public class CommandProvider(IOptions<DbOptions> dbOptions)
    {
        private readonly NpgsqlDataSource _dataSource = new NpgsqlSlimDataSourceBuilder(dbOptions.Value.ConnectionString).Build();

        public async Task<NpgsqlConnection> CreateConnectionAsync() => await _dataSource.OpenConnectionAsync();

        public NpgsqlCommand GetTransactionCommandAsync(int valor, int id, string descricao, char tipo)
        {
            NpgsqlCommand command;
            if (tipo == 'd')
                command = new NpgsqlCommand(Scripts.InsertDebitoTransaction);
            else
                command = new NpgsqlCommand(Scripts.InsertCreditoTransaction);

            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar });
            command.Parameters[0].Value = valor;
            command.Parameters[1].Value = id;
            command.Parameters[2].Value = descricao;

            return command;            
        }

        public NpgsqlCommand GetExtratoCommandAsync(int idCliente)
        {
            var command = new NpgsqlCommand(Scripts.GetExtrato);
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);            

            return command;
        }

        public NpgsqlCommand GetSaldoCommand(int idCliente)
        {
            var command = new NpgsqlCommand(Scripts.GetSaldo);
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);

            return command;
        }
    }
}
