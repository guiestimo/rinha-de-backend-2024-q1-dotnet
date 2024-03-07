using Models.Extrato;
using Npgsql;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using rinha_de_backend_2024_q1_dotnet_API.Options;
using rinha_de_backend_2024_q1_dotnet_API.Repository;
using rinha_de_backend_2024_q1_dotnet_API.TransactionCommands;
using rinha_de_backend_2024_q1_dotnet_API.TransactionCommands.Enums;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var connectionString = builder.Configuration.GetConnectionString("RinhaDotNet");
builder.Services.Configure<DbOptions>(options
        => options.ConnectionString = connectionString
            ?? throw new ArgumentNullException(connectionString));

builder.Services.AddSingleton<ConnectionProvider>();
builder.Services.AddSingleton<CreditDebitTransactionPool>();
builder.Services.AddSingleton<BankStatementTransactionPool>();


var app = builder.Build();
var clienteApi = app.MapGroup("/clientes");

// Feed pool in app startup
var creditTransactionPool = app.Services.GetRequiredService<CreditDebitTransactionPool>();
var bankStatementTransactionPool = app.Services.GetRequiredService<BankStatementTransactionPool>();

var feedTransactionPool = Task.Run(creditTransactionPool.FeedPool);
var feedBankStatementTransactionPool = Task.Run(bankStatementTransactionPool.FeedPool);


await Task.WhenAll(feedTransactionPool, feedBankStatementTransactionPool);

clienteApi.MapPost("{id}/transacoes",
    async (int id,
           TransacoesRequest request,
           CreditDebitTransactionPool creditDebitTransactionPool,
           ConnectionProvider connectionProvider) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();
    if (string.IsNullOrWhiteSpace(request.Descricao) || request.Descricao.Length > 10)
        return TypedResults.UnprocessableEntity();
    if (int.TryParse(request.Valor?.ToString(), out var valor) is false)
        return TypedResults.UnprocessableEntity();
    if (request.Tipo != 'c' && request.Tipo != 'd')
        return TypedResults.UnprocessableEntity();

    NpgsqlCommand command = null!;
    TransacoesResponse response = new()!;

    try
    {
        if (request.Tipo == 'c')
            command = creditDebitTransactionPool.GetCreditCommand();
        else
            command = creditDebitTransactionPool.GetDebitCommand();

        command.Parameters[0].Value = valor;
        command.Parameters[1].Value = id;
        command.Parameters[2].Value = request.Descricao;

        await using var connection = await connectionProvider.CreateConnectionAsync();
        command.Connection = connection;

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (reader.GetBoolean(2) is false)
            {
                return TypedResults.UnprocessableEntity();
            }

            response.Limite = reader.GetInt32(1);
            response.Saldo = reader.GetInt32(0);
        }

    }
    finally
    {
        command.Connection = null;

        if (request.Tipo == 'c')
            creditDebitTransactionPool.ReturnCreditCommandToPool(command);
        else
            creditDebitTransactionPool.ReturnDebitCommandToPool(command);
    }

    return Results.Ok(response);
});

clienteApi.MapGet("{id}/extrato",
    async (int id,
           BankStatementTransactionPool bankStatementTransactionPool,
           ConnectionProvider connectionProvider) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();

    var dictCommand = bankStatementTransactionPool.GetCommand();
    dictCommand.TryGetValue(CommandType.SaldoCommand, out var saldoCommand);
    saldoCommand.Parameters[0].Value = id;

    await using var connection = await connectionProvider.CreateConnectionAsync();
    saldoCommand.Connection = connection;

    var saldo = new SaldoExtratoResponse();
    using (var saldoReader = await saldoCommand.ExecuteReaderAsync())
    {
        while (await saldoReader.ReadAsync())
        {
            saldo = new SaldoExtratoResponse
            {
                Total = saldoReader.GetInt32(0),
                Limite = saldoReader.GetInt32(1),
                DataExtrato = DateTime.Now
            };
        }
    }


    dictCommand.TryGetValue(CommandType.ExtratoCommand, out var extratoCommand);
    extratoCommand.Parameters[0].Value = id;
    extratoCommand.Connection = connection;

    var extrato = new ExtratoViewModel() { Saldo = saldo, UltimasTransacoes = new List<TransacaoExtratoResponse>(10) };
    using (var extratoReader = await extratoCommand.ExecuteReaderAsync())
    {
        while (await extratoReader.ReadAsync())
        {
            extrato.UltimasTransacoes.Add(new TransacaoExtratoResponse(
                extratoReader.GetInt32(0),
                extratoReader.GetChar(1),
                extratoReader.GetDateTime(3),
                extratoReader.GetString(2),
                extratoReader.GetInt32(4)));
        }
    }

    saldoCommand.Connection = null;
    extratoCommand.Connection = null;
    bankStatementTransactionPool.ReturnCommandToPool(dictCommand);

    return Results.Ok(extrato);
});


app.Run();

[JsonSerializable(typeof(TransacoesRequest))]
[JsonSerializable(typeof(TransacoesResponse))]
[JsonSerializable(typeof(ExtratoViewModel))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}