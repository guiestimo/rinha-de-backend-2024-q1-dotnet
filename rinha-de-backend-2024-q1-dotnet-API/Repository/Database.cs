using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Options;

namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public class Database
    {
        private readonly DbOptions _dbOptions;
        private readonly NpgsqlDataSource _dataSource;
        public Database(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions.Value;
            _dataSource = new NpgsqlDataSourceBuilder(_dbOptions.ConnectionString).Build();
        }

        private NpgsqlConnection CreateConnection() => _dataSource.OpenConnection();

        public int? GetCliente(int idCliente)
        {
            using var command = new NpgsqlCommand("SELECT ID FROM CLIENTE WHERE ID = @IdCliente", CreateConnection());
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = command.ExecuteReader();
            int? id = null;
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            return id;
        }

        public object? AddTransacao(Transacao transacao)
        {
            object? response = null;
            try
            {
                response = UpdateSaldo(transacao);
                using var command = new NpgsqlCommand(Scripts.InsertTransaction, CreateConnection());
                command.Parameters.AddWithValue("@idCliente", NpgsqlDbType.Integer, transacao.ClienteId);
                command.Parameters.AddWithValue("@valor", NpgsqlDbType.Integer, transacao.Valor);
                command.Parameters.AddWithValue("@tipo", NpgsqlDbType.Varchar, transacao.Tipo);
                command.Parameters.AddWithValue("@descricao", NpgsqlDbType.Varchar, transacao.Descricao);
                command.ExecuteNonQuery();
            }
            catch (Exception) { }

            return response;
        }

        private object? UpdateSaldo(Transacao transacao)
        {
            using var command = new NpgsqlCommand(Scripts.UpdateSaldo(transacao.Tipo), CreateConnection());
            command.Parameters.AddWithValue("@valor", NpgsqlDbType.Integer, transacao.Valor);
            command.Parameters.AddWithValue("@idCliente", NpgsqlDbType.Integer, transacao.ClienteId);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new { Limite = reader.GetInt32(0), Saldo = reader.GetInt32(1) };
            }

            return null;
        }

        public ExtratoViewModel GetExtrato(int idCliente)
        {
            var extrato = new ExtratoViewModel()
            {
                Saldo = GetSaldo(idCliente),
                UltimasTransacoes = []
            };

            using var command = new NpgsqlCommand(Scripts.GetExtrato, CreateConnection());
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = command.ExecuteReader();
            while (reader.Read())
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

        private SaldoViewModel GetSaldo(int idCliente)
        {
            var saldo = new SaldoViewModel();
            using var command = new NpgsqlCommand("SELECT saldo, limite FROM CLIENTE WHERE ID = @IdCliente", CreateConnection());
            command.Parameters.AddWithValue("@IdCliente", NpgsqlDbType.Integer, idCliente);
            using var reader = command.ExecuteReader();
            while (reader.Read())
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
