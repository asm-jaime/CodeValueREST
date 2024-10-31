using System;
using System.Data;
using System.Text.RegularExpressions;
using Npgsql;

namespace DataAccess.Postgres;

public class PostgresSavepointWrapper : IDbTransaction
{
    private readonly string _savepointName;

    private NpgsqlConnection _connection;

    private bool _committed;

    internal PostgresSavepointWrapper(NpgsqlConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _savepointName = $"SP_{Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", string.Empty)}";
        var command = _connection.CreateCommand();
        command.CommandText = "SAVEPOINT " + _savepointName;
        command.CommandTimeout = 0;
        command.ExecuteNonQuery();
    }

    public IDbConnection Connection => _connection;

    public IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

    public void Commit() => _committed = true;

    public void Dispose()
    {
        if (_committed)
        {
            return;
        }

        var command = _connection.CreateCommand();
        command.CommandText = "ROLLBACK TO SAVEPOINT " + _savepointName;
        command.CommandTimeout = 0;
        command.ExecuteNonQuery();
    }

    public void Rollback() => _committed = false;
}
