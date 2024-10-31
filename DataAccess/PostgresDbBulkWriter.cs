using FastMember;
using GpnDs.ISDR.DataAccess;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataAccess.Postgres;

public class PostgresDbBulkWriter : IDbBulkWriter
{
    private const string ColumnAndPropertyNameForDataFieldName = "p2";

    public int AffectedRows { get; set; }

    public IDbBulkWriter Insert<T>(IDbConnection connection, string targetTableName, IList<T> data, IList<ColumnToPropertyMapping> mappings = null)
    {
        return this;
    }

    public IDbBulkWriter Write<T>(IDbConnection connection, in string sql, IList<T> data, string values = "values(:p)")
    {
        var (query, name) = GetQueryAndNameFromSql(sql);

        _ = connection ?? throw new ArgumentNullException(nameof(connection));
        _ = data ?? throw new ArgumentNullException(nameof(data));
        var posgresConnection = (PostgresConnectionWrapper)connection;

        if(string.IsNullOrEmpty(query))
        {
            throw new ArgumentNullException(nameof(query));
        }

        if(string.IsNullOrEmpty(values))
        {
            throw new ArgumentNullException(nameof(values));
        }

        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if(data.Count == 0)
        {
            return this;
        }

        var mappings = GetOrderedMappingsFromSql<T>(values);

        Dictionary<string, Array> parameterValues = InitializeParameterValues<T>(mappings, data.Count);
        FillParameterValues(parameterValues, data);

        FillParameterValuesAndMappingByDataFieldName(mappings, parameterValues, data.Count, name, ColumnAndPropertyNameForDataFieldName);

        var sqlStmt = "copy " + query + " from stdin (format binary)";

        using(var writer = posgresConnection.BeginBinaryImport(sqlStmt))
        {
            foreach(var (item, index) in WithIndex(data))
            {
                writer.StartRow();
                foreach(var mapping in mappings)
                {
                    var value = parameterValues[mapping.Property].GetValue(index);
                    var type = parameterValues[mapping.Property].GetType().GetElementType();

                    if(value == null)
                    {
                        writer.WriteNull();
                    }
                    else
                    {
                        writer.Write(value, TypeMapping(type));
                    }
                }
            }

            writer.Complete();
        }

        return this;
    }

    public IDbBulkWriter CopyInsert<T>(PostgresConnectionWrapper connection, string sql, string values, IList<T> data)
    {
        _ = connection ?? throw new ArgumentNullException(nameof(connection));
        _ = data ?? throw new ArgumentNullException(nameof(data));

        if(string.IsNullOrEmpty(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }

        if(string.IsNullOrEmpty(values))
        {
            throw new ArgumentNullException(nameof(values));
        }

        if(data.Count == 0)
        {
            return this;
        }

        var mappings = GetOrderedMappingsFromSql<T>(values);
        Dictionary<string, Array> parameterValues = InitializeParameterValues<T>(mappings, data.Count);
        FillParameterValues(parameterValues, data);
        var sqlStmt = "copy " + sql + " from stdin (format binary)";
        /*using (var command = CreateCommand(connection, sql, mappings, parameterValues))
        {
            AffectedRows = command.ExecuteNonQuery();
        }*/
        using(var writer = connection.BeginBinaryImport(sqlStmt))
        {
            foreach(var (item, index) in WithIndex(data))
            {
                writer.StartRow();
                foreach(var mapping in mappings)
                {
                    var value = parameterValues[mapping.Property].GetValue(index);
                    var type = parameterValues[mapping.Property].GetType().GetElementType();
                    if(value == null)
                    {
                        writer.WriteNull();
                    }
                    else
                    {
                        writer.Write(value, TypeMappingOld(type));
                    }
                }
            }

            writer.Complete();
        }

        return this;
    }

    private static Dictionary<string, Array> InitializeParameterValues<T>(IList<ColumnToPropertyMapping> mappings, int numberOfRows)
    {
        var values = new Dictionary<string, Array>(mappings.Count);
        var accessor = TypeAccessor.Create(typeof(T));
        var members = accessor.GetMembers().ToDictionary(m => m.Name);
        foreach(var mapping in mappings)
        {
            var member = members[mapping.Property];
            if(member.Type.IsEnum)
            {
                values[mapping.Property] = Array.CreateInstance(typeof(long), numberOfRows);
            }
            else
            {
                values[mapping.Property] = Array.CreateInstance(member.Type, numberOfRows);
            }
        }

        return values;
    }

    private static void FillParameterValues<T>(Dictionary<string, Array> parameterValues, IList<T> data)
    {
        var accessor = TypeAccessor.Create(typeof(T));
        for(var rowNumber = 0; rowNumber < data.Count; rowNumber++)
        {
            var row = data[rowNumber];
            foreach(var pair in parameterValues)
            {
                Array parameterValue = pair.Value;
                var propertyValue = accessor[row, pair.Key];
                parameterValue.SetValue(propertyValue, rowNumber);
            }
        }
    }

    private IList<ColumnToPropertyMapping> GetOrderedMappingsFromSql<T>(string sql)
    {
        var accessor = TypeAccessor.Create(typeof(T));
        var members = accessor.GetMembers();

        var parameterNamesFromSql = WithIndex(GetParameterNamesFromSql(sql));

        var result = new List<ColumnToPropertyMapping>();
        foreach(var parameter in parameterNamesFromSql)
        {
            result.Add(members
                .Where(m => parameter.item == m.Name)
                .Select(m => new ColumnToPropertyMapping(m.Name, m.Name))
                .FirstOrDefault());
        }

        return result;
    }

    private HashSet<string> GetParameterNamesFromSql(string sql)
    {
        var parameters = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        var parameterNameGroup = "name";
        var parameterNamePattern = new Regex($@":(?<{parameterNameGroup}>\w+)", RegexOptions.Compiled);
        var match = parameterNamePattern.Match(sql);
        while(match.Success)
        {
            parameters.Add(match.Groups[parameterNameGroup].Value);
            match = match.NextMatch();
        }

        return parameters;
    }

    private NpgsqlDbType TypeMapping(Type type)
    {
        var result = default(NpgsqlDbType);
        switch(type)
        {
            case Type boolType when boolType == typeof(bool) || boolType == typeof(bool?):
                {
                    result = NpgsqlDbType.Boolean;
                    break;
                }
            case Type intType when intType == typeof(int) || intType == typeof(int?):
                {
                    result = NpgsqlDbType.Integer;
                    break;
                }

            case Type stringType when stringType == typeof(string):
                {
                    result = NpgsqlDbType.Varchar;
                    break;
                }

            case Type dateType when dateType == typeof(DateTime) || dateType == typeof(DateTime?):
                {
                    result = NpgsqlDbType.Timestamp;
                    break;
                }

            case Type decimalType when decimalType == typeof(decimal) || decimalType == typeof(decimal?):
                {
                    result = NpgsqlDbType.Numeric;
                    break;
                }

            case Type doubleType when doubleType == typeof(double) || doubleType == typeof(double?):
                {
                    result = NpgsqlDbType.Double;
                    break;
                }

            case Type floatType when floatType == typeof(float) || floatType == typeof(float?):
                {
                    result = NpgsqlDbType.Double;
                    break;
                }

            case Type guidType when guidType == typeof(Guid) || guidType == typeof(Guid?):
                {
                    result = NpgsqlDbType.Uuid;
                    break;
                }

            case Type longType when longType == typeof(long) || longType == typeof(long?):
                {
                    result = NpgsqlDbType.Bigint;
                    break;
                }

            case Type timeType when timeType == typeof(DateTime) || timeType == typeof(DateTime?):
                {
                    result = NpgsqlDbType.Timestamp;
                    break;
                }
        }

        return result;
    }

    private NpgsqlDbType TypeMappingOld(Type type)
    {
        var result = default(NpgsqlDbType);
        switch(type)
        {
            case Type boolType when boolType == typeof(bool) || boolType == typeof(bool?):
                {
                    result = NpgsqlDbType.Boolean;
                    break;
                }
            case Type intType when intType == typeof(int) || intType == typeof(int?):
                {
                    result = NpgsqlDbType.Integer;
                    break;
                }

            case Type stringType when stringType == typeof(string):
                {
                    result = NpgsqlDbType.Varchar;
                    break;
                }

            case Type dateType when dateType == typeof(DateTime) || dateType == typeof(DateTime?):
                {
                    result = NpgsqlDbType.Timestamp;
                    break;
                }

            case Type decimalType when decimalType == typeof(decimal) || decimalType == typeof(decimal?):
                {
                    result = NpgsqlDbType.Numeric;
                    break;
                }

            case Type doubleType when doubleType == typeof(double) || doubleType == typeof(double?):
                {
                    result = NpgsqlDbType.Numeric;
                    break;
                }

            case Type floatType when floatType == typeof(float) || floatType == typeof(float?):
                {
                    result = NpgsqlDbType.Numeric;
                    break;
                }

            case Type guidType when guidType == typeof(Guid) || guidType == typeof(Guid?):
                {
                    result = NpgsqlDbType.Uuid;
                    break;
                }

            case Type longType when longType == typeof(long) || longType == typeof(long?):
                {
                    result = NpgsqlDbType.Numeric;
                    break;
                }

            case Type timeType when timeType == typeof(DateTime) || timeType == typeof(DateTime?):
                {
                    result = NpgsqlDbType.Timestamp;
                    break;
                }
        }

        return result;
    }

    private (string, string) GetQueryAndNameFromSql(string sql)
    {
        var sqls = sql.Replace("insert into ", string.Empty).Split("values");
        var query = sqls.FirstOrDefault().Trim();

        string type = string.Empty;
        Match match = Regex.Match(sqls.LastOrDefault(), "'(.*?)'");
        if(match.Success)
        {
            type = match.Groups[1].Value;
        }

        return (query, type);
    }

    private void FillParameterValuesAndMappingByDataFieldName(IList<ColumnToPropertyMapping> mappings, Dictionary<string, Array> parameterValues, int count, string dataFieldName, string columnName)
    {
        mappings.Add(new ColumnToPropertyMapping(columnName, columnName));
        parameterValues.Add(columnName, Enumerable.Range(0, count).Select(e => dataFieldName).ToArray());
    }

    private IEnumerable<(T item, int index)> WithIndex<T>(IEnumerable<T> self)
    {
        return self?.Select((item, index) => (item, index)) ?? new List<(T, int)>();
    }
}
