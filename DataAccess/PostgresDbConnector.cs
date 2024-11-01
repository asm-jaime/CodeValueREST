using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DataAccess.Postgres;

public class PostgresDbConnector : IDbConnector
{
    private string _connectionString;

    static PostgresDbConnector()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public PostgresDbConnector(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection Connect(bool removeFromPoolOnDispose = false)
    {
        var connection = new PostgresConnectionWrapper(_connectionString, removeFromPoolOnDispose);
        connection.Open();

        return connection;
    }

    public async Task<IDbConnection> ConnectAsync(bool removeFromPoolOnDispose = false)
    {
        var connection = new PostgresConnectionWrapper(_connectionString, removeFromPoolOnDispose);
        await connection.OpenAsync();

        return connection;
    }

    public IDbBulkWriter GetBulkWriter() => throw new NotImplementedException();

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }
}
