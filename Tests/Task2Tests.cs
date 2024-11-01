using Dapper;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Tests;

record NameNumber(string ClientName, long ContactsNumber);
record Client(int Id, string ClientName);

[TestFixture]
public class Task2Tests
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
create table if not exists client (
  id serial primary key,
  client_name varchar(200) NOT NULL
);

create table if not exists client_contact (
  id serial primary key,
  client_id integer not null,
  contact_type varchar(255) not null,
  contact_value varchar(255) not null
);
";
        var insertDataToTablesQuery = @"
insert into client(client_name)
values
    ('bob'),
    ('stive'),
    ('colin');

insert into client_contact(client_id, contact_type, contact_value)
values
    (1, 'first', 'some1'),
    (1, 'second', 'some2'),
    (1, 'fff', 'some'),
    (2, 'third', 'some3');
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
    public async Task TestSqlQuery_ShouldReturnNamesNumberContacts()
    {

        //Arrange
        var testData = new List<NameNumber>() { new("colin", 0), new("stive", 1), new("bob", 3)  };
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        //Act
        var retrievedData = (await db.QueryAsync<NameNumber>(@"
  select client_name as ClientName, COUNT(cc.Id) as ContactsNumber from client c
  left join client_contact cc on cc.client_id = c.id
  group by c.id, c.client_name
")).ToList();
        //Assert
        retrievedData.Should().BeEquivalentTo(testData, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task TestSqlQuery_ShouldReturnClientsWhoHaveMoreThan2Contacts()
    {
        //Arrange
        var testData = new List<Client>() {new(1, "bob") };
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        //Act
        var retrievedData = (await db.QueryAsync<Client>(@"
  select c.id as Id, client_name as ClientName from client c
  left join client_contact cc on cc.client_id = c.id
  group by c.id, c.client_name
  having COUNT(cc.Id) > 2
  
")).ToList();
        //Assert
        retrievedData.Should().BeEquivalentTo(testData, options => options.WithStrictOrdering());
    }
}
