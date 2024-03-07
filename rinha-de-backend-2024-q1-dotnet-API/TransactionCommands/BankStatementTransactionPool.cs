using Npgsql;
using NpgsqlTypes;
using rinha_de_backend_2024_q1_dotnet_API.Repository;
using rinha_de_backend_2024_q1_dotnet_API.TransactionCommands.Enums;
using System.Collections.Concurrent;

namespace rinha_de_backend_2024_q1_dotnet_API.TransactionCommands
{
    public class BankStatementTransactionPool
    {
        private const int POOL_SIZE = 50;
        private readonly ConcurrentQueue<IDictionary<CommandType, NpgsqlCommand>> _pool;

        public BankStatementTransactionPool()
        {
            _pool = new ConcurrentQueue<IDictionary<CommandType, NpgsqlCommand>>();
        }

        public void FeedPool()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                var command = CreateCommand();

                _pool.Enqueue(command);
            }
        }

        private static IDictionary<CommandType, NpgsqlCommand> CreateCommand()
        {
            var saldoCommand = new NpgsqlCommand(Scripts.GetSaldo);
            saldoCommand.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });

            var extratoCommand = new NpgsqlCommand(Scripts.GetExtrato);
            extratoCommand.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });

            return new Dictionary<CommandType, NpgsqlCommand>
            {
                { CommandType.SaldoCommand,  saldoCommand },
                { CommandType.ExtratoCommand,  extratoCommand }
            };
        }

        public IDictionary<CommandType, NpgsqlCommand> GetCommand()
        {
            if (!_pool.IsEmpty && _pool.TryDequeue(out var command))
                return command;

            return CreateCommand();
        }

        public void ReturnCommandToPool(IDictionary<CommandType, NpgsqlCommand> command)
        {
            command[CommandType.SaldoCommand].Parameters[0] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };
            command[CommandType.ExtratoCommand].Parameters[0] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };

            _pool.Enqueue(command);
        }
    }
}
