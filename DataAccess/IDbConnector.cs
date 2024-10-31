﻿using System.Data;
using System.Threading.Tasks;

namespace GpnDs.ISDR.DataAccess;

public interface IDbConnector
{
    IDbConnection Connect(bool removeFromPoolOnDispose = false);

    Task<IDbConnection> ConnectAsync(bool removeFromPoolOnDispose = false);

    IDbBulkWriter GetBulkWriter();

    string GetConnectionString();

    void SetConnectionString(string connectionString);
}