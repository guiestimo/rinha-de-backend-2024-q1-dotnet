using rinha_de_backend_2024_q1_dotnet_API.Entities;
using rinha_de_backend_2024_q1_dotnet_API.Models.Transacoes;
using rinha_de_backend_2024_q1_dotnet_API.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ClientesDummy>();

var app = builder.Build();
var clienteApi = app.MapGroup("/clientes");

clienteApi.MapPost("{id}/transacoes", (int id, TransacoesRequest request, ClientesDummy dummy) =>
{
    var cliente = dummy.RetornaClientePorId(id);
    if (cliente is null)
        return Results.NotFound();

    if (request.Tipo.Equals('d') && (cliente.Saldo - request.Valor + cliente.Limite) < 0)
        return Results.BadRequest();


    dummy.AddTransacao(request, cliente);



    return Results.Ok(cliente);
});

clienteApi.MapGet("{id}/extrato", (int id, ClientesDummy dummy) =>
{
    var extrato = dummy.RetornaExtrato(id);

    if (extrato is null)
        return Results.NotFound();


    return Results.BadRequest(extrato);
});


app.Run();
