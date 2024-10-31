using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DataAccess.Postgres;

public class PostgresConnectionWrapper : IDbConnection
{
    private NpgsqlConnection _implementation;
    private bool _removeFromPoolOnDispose;

    public PostgresConnectionWrapper(string connectionString, bool removeFromPoolOnDispose)
    {
        if(string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }

        _implementation = new NpgsqlConnection(connectionString);
        _removeFromPoolOnDispose = removeFromPoolOnDispose;
    }

    public IDbConnection Implementation
    {
        get { return _implementation; }
    }

    public string ConnectionString
    {
        get
        {
            return _implementation.ConnectionString;
        }
        set
        {
            _implementation.ConnectionString = value;
        }
    }

    public int ConnectionTimeout
    {
        get
        {
            return _implementation.ConnectionTimeout;
        }
    }

    public string Database
    {
        get
        {
            return _implementation.Database;
        }
    }

    public ConnectionState State
    {
        get
        {
            return _implementation.State;
        }
    }

    public IDbTransaction BeginTransaction()
    {
        return BeginTransaction(IsolationLevel.Unspecified);
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        try
        {
            if(il == IsolationLevel.Unspecified)
            {
                return _implementation.BeginTransaction();
            }

            return _implementation.BeginTransaction(il);
        }
        catch(InvalidOperationException)
        {
            // if nested transaction
            return new PostgresSavepointWrapper(_implementation);
        }
    }

    public void ChangeDatabase(string databaseName)
    {
        _implementation.ChangeDatabase(databaseName);
    }

    public void Close()
    {
        _implementation.Close();
    }

    public IDbCommand CreateCommand()
    {
        return _implementation.CreateCommand();
    }

    public void Open()
    {
        _implementation.Open();
    }

    public Task OpenAsync()
    {
        return _implementation.OpenAsync();
    }

    public void Dispose()
    {
        _implementation.Dispose();

        if(_removeFromPoolOnDispose)
        {
            NpgsqlConnection.ClearPool(_implementation);
        }
    }

    public NpgsqlBinaryImporter BeginBinaryImport(string command)
    {
        return _implementation.BeginBinaryImport(command);
    }
}