using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using System.ComponentModel;

namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public class ClientesDummy
    {
        public readonly List<Cliente> _clientes;
        public readonly List<Transacao> _transacoes;
        public ClientesDummy()
        {
            _clientes =
            [
                new(1, 100000, 0),
                new(2, 80000, 0),
                new(3, 1000000, 0),
                new(4, 10000000, 0),
                new(5, 500000, 0)
            ];

            _transacoes = [];
        }

        public void AddTransacao(TransacoesRequest request, Cliente cliente)
        {
            _transacoes.Add(new Transacao(request.Valor, request.Tipo, DateTime.Now, request.Descricao, cliente.Id));

            UpdateSaldoCliente(cliente, request);
        }

        private static void UpdateSaldoCliente(Cliente cliente, TransacoesRequest request)
        {
            cliente.AtualizarSaldo(request.Valor, request.Tipo);
        }

        public Cliente? RetornaClientePorId(int id)
        {
            return _clientes.FirstOrDefault(x => x.Id == id);
        }

        public ExtratoViewModel? RetornaExtrato(int clienteId)
        {
            var clientes =
                            (from cliente in _clientes
                             where cliente.Id == clienteId
                             let transacoes = _transacoes.Where(x => x.ClienteId == cliente.Id).Take(10).ToList()
                             select new ExtratoViewModel
                             {
                                 Saldo = new SaldoViewModel
                                 {
                                     DataExtrato = DateTime.Now,
                                     Limite = cliente.Limite,
                                     Total = cliente.Saldo
                                 },
                                 UltimasTransacoes = transacoes
                             }).FirstOrDefault();
            return clientes;
        }
    }
}
