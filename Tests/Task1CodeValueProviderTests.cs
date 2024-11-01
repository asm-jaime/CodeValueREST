using CodeValueREST.Features.CodeValues;
using CodeValueREST.Features.CodeValues.Models;
using CodeValueREST.Features.CodeValues.Providers;
using DataAccess.Postgres;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests;


[TestFixture]
[Category("DataAccess")]
class Task1CodeValueProviderTests
{

    private static CodeValueProvider _codeValueProvider;


    [OneTimeSetUp]
    public static void Connect()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = $"Host={configuration.GetSection("DATABASE_HOST").Value}; " +
                               $"Port={configuration.GetSection("DATABASE_PORT").Value}; " +
                               $"Password={configuration.GetSection("DATABASE_CORE_PASSWORD").Value}; " +
                               $"User Id={configuration.GetSection("DATABASE_CORE_USER").Value}; " +
                               $"Timeout = 60; " +
                               $"Command Timeout = 300;";
        var connector = new PostgresDbConnector(connectionString);
        _codeValueProvider = new CodeValueProvider(connector);
    }

    [Test]
    public async Task CodeValueProvider_CanAsyncListByFilter_Test()
    {
        //Arrange

        //Act
        var result = await _codeValueProvider.ListAsync(new CodeValueFilter { Values = ["value1"] });

        //Assert
        result.Count.Should().Be(1);
    }

    [Test]
    public async Task CodeValueProvider_CanPutRange_Test()
    {
        //Arrange
        var codeValues = new List<CodeValue>() { new() { Code = 0, Value = "value0" } };

        //Act
        var result = await _codeValueProvider.PutRange(codeValues);

        //Assert
        result.Count.Should().Be(1);
    }
}

