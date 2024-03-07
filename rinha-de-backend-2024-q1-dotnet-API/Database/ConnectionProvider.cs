using Microsoft.Extensions.Options;
using Npgsql;
using rinha_de_backend_2024_q1_dotnet_API.Options;

namespace rinha_de_backend_2024_q1_dotnet_API.Repository
{
    public class ConnectionProvider(IOptions<DbOptions> dbOptions)
    {
        private readonly NpgsqlDataSource _dataSource = new NpgsqlSlimDataSourceBuilder(dbOptions.Value.ConnectionString).Build();

        public async Task<NpgsqlConnection> CreateConnectionAsync() => await _dataSource.OpenConnectionAsync();
    }
}
