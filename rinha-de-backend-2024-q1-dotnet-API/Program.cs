using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Enums;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using rinha_de_backend_2024_q1_dotnet_API.Options;
using rinha_de_backend_2024_q1_dotnet_API.Repository;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RinhaDotNet");
builder.Services.Configure<DbOptions>(options
        => options.ConnectionString = connectionString
            ?? throw new ArgumentNullException(connectionString));
builder.Services.AddSingleton<Database>();


var app = builder.Build();
var clienteApi = app.MapGroup("/clientes");


clienteApi.MapPost("{id}/transacoes", async (int id, TransacoesRequest request, Database database) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();
    if (string.IsNullOrWhiteSpace(request.Descricao) || request.Descricao.Length > 10)
        return TypedResults.UnprocessableEntity();
    if (int.TryParse(request.Valor?.ToString(), out var valor) is false)
        return TypedResults.UnprocessableEntity();
    if (request.Tipo != 'c' && request.Tipo != 'd')
        return TypedResults.UnprocessableEntity();

    var response = await database.AddTransactionAsync(valor, id, request.Descricao, request.Tipo);

    if (response is null)
        return TypedResults.UnprocessableEntity();

    return Results.Ok(response);
});

clienteApi.MapGet("{id}/extrato", async (int id, Database database) =>
{
    if (id is (< 1 or > 5))
        return TypedResults.NotFound();

    var extrato = await database.GetExtratoAsync(id);

    return Results.Ok(extrato);
});


app.Run();
