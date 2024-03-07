using Npgsql;
using NpgsqlTypes;
using rinha_de_backend_2024_q1_dotnet_API.Repository;
using System.Collections.Concurrent;

namespace rinha_de_backend_2024_q1_dotnet_API.TransactionCommands
{
    public class CreditDebitTransactionPool
    {
        private readonly ConcurrentQueue<NpgsqlCommand> _creditPool;
        private readonly ConcurrentQueue<NpgsqlCommand> _debitPool;
        private const int POOL_SIZE = 300;

        public CreditDebitTransactionPool()
        {
            _creditPool = new ConcurrentQueue<NpgsqlCommand>();
            _debitPool = new ConcurrentQueue<NpgsqlCommand>();
        }

        public void FeedPool()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                var commandCredit = CreateCreditTransaction();
                var commandDebit = CreateDebitTransaction();

                _creditPool.Enqueue(commandCredit);
                _debitPool.Enqueue(commandDebit);
            }
        }

        private static NpgsqlCommand CreateCreditTransaction()
        {
            var command = new NpgsqlCommand(Scripts.InsertCreditoTransaction);
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar });

            return command;
        }

        private static NpgsqlCommand CreateDebitTransaction()
        {
            var command = new NpgsqlCommand(Scripts.InsertDebitoTransaction);
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar });

            return command;
        }

        public NpgsqlCommand GetCreditCommand()
        {
            if (!_creditPool.IsEmpty && _creditPool.TryDequeue(out var command))
                return command;

            return CreateCreditTransaction();
        }
        public NpgsqlCommand GetDebitCommand()
        {
            if (!_debitPool.IsEmpty && _debitPool.TryDequeue(out var command))
                return command;

            return CreateDebitTransaction();
        }

        public void ReturnCreditCommandToPool(NpgsqlCommand command)
        {
            command.Parameters[0] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };
            command.Parameters[1] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };
            command.Parameters[2] = new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar };

            _creditPool.Enqueue(command);
        }

        public void ReturnDebitCommandToPool(NpgsqlCommand command)
        {
            command.Parameters[0] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };
            command.Parameters[1] = new NpgsqlParameter<int>() { NpgsqlDbType = NpgsqlDbType.Integer };
            command.Parameters[2] = new NpgsqlParameter<string>() { NpgsqlDbType = NpgsqlDbType.Varchar };

            _debitPool.Enqueue(command);
        }
    }
}
