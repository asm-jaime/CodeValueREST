using Dapper;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Tests;


record IdenticalInterval(int Id, DateTime Sd, DateTime Ed);

[TestFixture]
public class Task3Tests
{
    private PostgreSqlContainer _postgresContainer;
    private string _connectionString;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        await _postgresContainer.StartAsync();

        _connectionString = _postgresContainer.GetConnectionString();

        using IDbConnection db = new NpgsqlConnection(_connectionString);
        var createTablesQuery = @"
create table if not exists dates (
  id serial primary key,
  client_id integer,
  dt timestamp
);
";
        var insertDataToTablesQuery = @"
insert into dates (client_id, dt)
values
    (1, to_timestamp('01/01/2021', 'DD/MM/YYYY')),
    (1, to_timestamp('10/01/2021', 'DD/MM/YYYY')),
    (1, to_timestamp('30/01/2021', 'DD/MM/YYYY')),
    (2, to_timestamp('15/01/2021', 'DD/MM/YYYY')),
    (2, to_timestamp('30/01/2021', 'DD/MM/YYYY'));
";
        await db.ExecuteAsync(createTablesQuery);
        await db.ExecuteAsync(insertDataToTablesQuery);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if(_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }

    [Test]
    public async Task TestSqlQuery_ShouldReturnIdenticalIntervals()
    {

        //Arrange
        var testData = new List<IdenticalInterval>() {
            new(1, new DateTime(2021, 1, 01), new DateTime(2021, 1, 10)),
            new(1, new DateTime(2021, 1, 10), new DateTime(2021, 1, 30)),
            new(2, new DateTime(2021, 1, 15), new DateTime(2021, 1, 30)),
        };
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        //Act
        var retrievedData = (await db.QueryAsync<IdenticalInterval>(@"
with cte as (
  select
    client_id as Id,
    Dt as Sd,
    lead(Dt) over (partition by client_id order by Dt) as Ed
  from
    dates
)
select Id, Sd, Ed from cte
where
  Ed is not null;
")).ToList();
        //Assert
        retrievedData.Should().BeEquivalentTo(testData, options => options.WithStrictOrdering());
    }
}
