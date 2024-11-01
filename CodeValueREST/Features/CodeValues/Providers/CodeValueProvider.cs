using CodeValueREST.Features.CodeValues.Models;
using Dapper;
using DataAccess;
using DataAccess.Postgres;
using System.Data;

namespace CodeValueREST.Features.CodeValues.Providers;

public class CodeValueProvider(IDbConnector connector)
{
    private readonly IDbConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));

    #region SQL Statements

    private readonly string _createReaderQueryParams =
@"create temporary table if not exists reader_query_params
(
  num_col  integer,
  guid_col uuid,
  str_col  varchar(2000),
  dt_col   timestamp,
  tag      varchar(2000)
)
on commit preserve rows";

    private readonly string _insertIdsInTemp = @"insert into reader_query_params(num_col, tag) values(:p, 'id')";
    private readonly string _insertCodesInTemp = @"insert into reader_query_params(num_col, tag) values(:p, 'code')";
    private readonly string _insertValuesInTemp = @"insert into reader_query_params(str_col, tag) values(:p, 'value')";

    private readonly string _withIds = @"
            ids AS (
                SELECT num_col
                FROM reader_query_params
                WHERE tag = 'id'
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
                code_value.id AS Id,
                code_value.code AS Code,
                code_value.value AS Value
            FROM code_value
            {joins}
            WHERE 1=1 {condition}";

    #endregion

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

        if(filter.Ids != null || filter.Codes != null || filter.Values != null)
        {
            connection.Execute(_createReaderQueryParams);
            connection.Execute("delete from reader_query_params");

            new PostgresDbBulkWriter()
                .Write(connection, _insertIdsInTemp, (filter.Ids ?? Enumerable.Empty<int>()).Select(i => new { p = i }).Distinct().ToList())
                .Write(connection, _insertCodesInTemp, (filter.Codes ?? Enumerable.Empty<int>()).Select(i => new { p = i }).Distinct().ToList())
                .Write(connection, _insertValuesInTemp, (filter.Values ?? Enumerable.Empty<string>()).Select(i => new { p = i }).Distinct().ToList());
        }

        var withs = string.Empty;
        var joins = string.Empty;
        var condition = string.Empty;
        var parameters = new DynamicParameters();

        if(filter.Ids != null && filter.Ids.Any())
        {
            withs += string.IsNullOrEmpty(withs) ? _withIds : ", " + _withIds;
            joins += "inner join ids on code_value.id = ids.num_col ";
        }

        if(filter.Codes != null && filter.Codes.Any())
        {
            withs += string.IsNullOrEmpty(withs) ? _withCodes : ", " + _withCodes;
            joins += "inner join codes on code_value.code = codes.num_col ";
        }

        if(filter.Values != null && filter.Values.Any())
        {
            withs += string.IsNullOrEmpty(withs) ? _withValues : ", " + _withValues;
            joins += "inner join values on code_value.value = values.str_col ";
        }

        string query = _baseQuery
            .Replace("{withs}", string.IsNullOrEmpty(withs) ? withs : "with " + withs)
            .Replace("{joins}", joins)
            .Replace("{condition}", condition);

        return (query, parameters);
    }
}
