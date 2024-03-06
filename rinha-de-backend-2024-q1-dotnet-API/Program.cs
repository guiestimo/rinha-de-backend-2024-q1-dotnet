using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Enums;
using rinha_de_backend_2024_q1_dotnet_API.Models.Extrato;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using rinha_de_backend_2024_q1_dotnet_API.Options;
using rinha_de_backend_2024_q1_dotnet_API.Repository;
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
builder.Services.AddSingleton<CommandProvider>();

var app = builder.Build();
var clienteApi = app.MapGroup("/clientes");


clienteApi.MapPost("{id}/transacoes", async (int id, TransacoesRequest request, CommandProvider commandProvider) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();
    if (string.IsNullOrWhiteSpace(request.Descricao) || request.Descricao.Length > 10)
        return TypedResults.UnprocessableEntity();
    if (int.TryParse(request.Valor?.ToString(), out var valor) is false)
        return TypedResults.UnprocessableEntity();
    if (request.Tipo != 'c' && request.Tipo != 'd')
        return TypedResults.UnprocessableEntity();

    TransacoesResponse response = null!;

    var command = commandProvider.GetTransactionCommandAsync(valor, id, request.Descricao, request.Tipo);
    using var connection = await commandProvider.CreateConnectionAsync();
    command.Connection = connection;

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        if (reader.GetBoolean(2) is false)
        {
            return TypedResults.UnprocessableEntity();
        }

        response = new(reader.GetInt32(1), reader.GetInt32(0));
    }

    return Results.Ok(response);
});

clienteApi.MapGet("{id}/extrato", async (int id, CommandProvider commandProvider) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();

    var saldoCommand = commandProvider.GetSaldoCommand(id);
    using var connection = await commandProvider.CreateConnectionAsync();
    saldoCommand.Connection = connection;

    using var saldoReader = await saldoCommand.ExecuteReaderAsync();

    var saldo = new SaldoResponse();
    while (await saldoReader.ReadAsync())
    {
        saldo = new SaldoResponse
        {
            Total = saldoReader.GetInt32(0),
            Limite = saldoReader.GetInt32(1),
            DataExtrato = DateTime.Now
        };
    }

    await saldoReader.DisposeAsync();

    var extratoCommand = commandProvider.GetExtratoCommandAsync(id);
    extratoCommand.Connection = connection;

    using var extratoReader = await extratoCommand.ExecuteReaderAsync();

    var extrato = new ExtratoViewModel() { Saldo = saldo, UltimasTransacoes = [] };
    while (await extratoReader.ReadAsync())
    {
        extrato.UltimasTransacoes.Add(new Transacao(
            extratoReader.GetInt32(0),
            extratoReader.GetChar(1),
            extratoReader.GetDateTime(3),
            extratoReader.GetString(2),
            extratoReader.GetInt32(4)));
    }

    return Results.Ok(extrato);
});


app.Run();

[JsonSerializable(typeof(TransacoesRequest))]
[JsonSerializable(typeof(TransacoesResponse))]
[JsonSerializable(typeof(ExtratoViewModel))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}