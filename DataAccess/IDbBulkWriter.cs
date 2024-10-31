using System.Collections.Generic;
using System.Data;

namespace DataAccess;

public interface IDbBulkWriter
{
    IDbBulkWriter Insert<T>(IDbConnection connection, string targetTableName, IList<T> data, IList<ColumnToPropertyMapping> mappings = null);

    IDbBulkWriter Write<T>(IDbConnection connection, in string sql, IList<T> data, string values = "values(:p)");
}
