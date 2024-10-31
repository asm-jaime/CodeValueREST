using CodeValueREST.Features.CodeValues.Models;
using Dapper;
using DataAccess.Postgres;
using GpnDs.ISDR.DataAccess;
using System.Data;

namespace CodeValueREST.Features.CodeValues.Providers;

public class CodeValueProvider
{
    private readonly IDbConnector _connector;

    #region SQL Statements

    private readonly string _createReaderQueryParams = @"
            CREATE TEMPORARY TABLE IF NOT EXISTS reader_query_params (
                num_col BIGINT,
                str_col VARCHAR(2000),
                tag VARCHAR(2000)
            ) ON COMMIT PRESERVE ROWS";

    private readonly string _insertIdsInTemp = @"INSERT INTO reader_query_params(num_col, tag) VALUES(:p, 'serial_number')";
    private readonly string _insertCodesInTemp = @"INSERT INTO reader_query_params(num_col, tag) VALUES(:p, 'code')";
    private readonly string _insertValuesInTemp = @"INSERT INTO reader_query_params(str_col, tag) VALUES(:p, 'value')";

    private readonly string _withIds = @"
            serialNumbers AS (
                SELECT num_col
                FROM reader_query_params
                WHERE tag = 'serial_number'
            )";

    private readonly string _withCodes = @"
            codes AS (
                SELECT num_col
                FROM reader_query_params
                WHERE tag = 'code'
            )";

    private readonly string _withValues = @"
            values AS (
                SELECT str_col
                FROM reader_query_params
                WHERE tag = 'value'
            )";

    private readonly string _baseQuery = @"
            {withs}
            SELECT
                code_value.serial_number AS SerialNumber,
                code_value.code AS Code,
                code_value.value AS Value
            FROM code_value
            {joins}
            WHERE 1=1 {condition}";

    #endregion

    public CodeValueProvider(IDbConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<IList<CodeValue>> ListAsync(CodeValueFilter filter)
    {
        using var connection = _connector.Connect();
        var (query, parameters) = GetQueryParams(filter, connection);

        if(string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var result = (await connection.QueryAsync<CodeValue>(query, parameters)).ToList();
        return result;
    }

    private (string, DynamicParameters) GetQueryParams(CodeValueFilter filter, IDbConnection connection)
    {
        if(
            filter.Ids != null && !filter.Ids.Any() &&
            filter.Codes != null && !filter.Codes.Any() &&
            filter.Values != null && !filter.Values.Any()
        )
        {
            return (string.Empty, new DynamicParameters());
        }

        if(
            filter.Ids != null ||
            filter.Codes != null ||
            filter.Values != null
        )
        {
            connection.Execute(_createReaderQueryParams);
            connection.Execute("DELETE FROM reader_query_params");

            var bulkWriter = new PostgresDbBulkWriter();

            if(filter.Ids != null && filter.Ids.Any())
            {
                bulkWriter.Write(connection, _insertIdsInTemp, filter.Ids.Select(i => new { p = i }).Distinct().ToList());
            }

            if(filter.Codes != null && filter.Codes.Any())
            {
                bulkWriter.Write(connection, _insertCodesInTemp, filter.Codes.Select(i => new { p = i }).Distinct().ToList());
            }

            if(filter.Values != null && filter.Values.Any())
            {
                bulkWriter.Write(connection, _insertValuesInTemp, filter.Values.Select(i => new { p = i }).Distinct().ToList());
            }
        }

        var withs = new List<string>();
        var joins = new List<string>();
        var parameters = new DynamicParameters();

        if(filter.Ids != null && filter.Ids.Any())
        {
            withs.Add(_withIds);
            joins.Add("INNER JOIN serialNumbers ON code_value.serial_number = serialNumbers.num_col");
        }

        if(filter.Codes != null && filter.Codes.Any())
        {
            withs.Add(_withCodes);
            joins.Add("INNER JOIN codes ON code_value.code = codes.num_col");
        }

        if(filter.Values != null && filter.Values.Any())
        {
            withs.Add(_withValues);
            joins.Add("INNER JOIN values ON code_value.value = values.str_col");
        }

        string query = _baseQuery
            .Replace("{withs}", withs.Any() ? "WITH " + string.Join(", ", withs) : string.Empty)
            .Replace("{joins}", string.Join(" ", joins))
            .Replace("{condition}", string.Empty);

        return (query, parameters);
    }
}
