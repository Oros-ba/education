using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using Testcontainers.MsSql;
using TestedApi.Model;

namespace TestedApi.Test.Integration.Controllers;

public class DocumentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly WebApplicationFactory<Program> _factory;
    private readonly MsSqlContainer _msSqlServerContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .Build();

    public DocumentsControllerTests(WebApplicationFactory<Program> factory)
    {
        //start the container
        _msSqlServerContainer.StartAsync().Wait();

        _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Find and remove the existing DbContextOptions<DocumentsContext> registration
                var descriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DocumentsDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Add a database context using the SQL Server container connection string
                services.AddDbContext<DocumentsDbContext>(options => options
                        .UseSqlServer(_msSqlServerContainer.GetConnectionString()));
                // Instantiate scope in order to get context and apply migrations
                using var scope = services.BuildServiceProvider().CreateScope();
                var documentsDbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
                documentsDbContext.Database.Migrate();
            });
        });


    }

    [Fact]
    public async Task GetDocumentsReturnsListOfDocuments()
    {
        // Arrange
        var client = _factory.CreateClient();
        var expectedDocuments = new List<Document>
            {
                new() { Id = 1, Title = "Document 1", Author = "Author of Document 1" },
                new() { Id = 2, Title = "Document 2", Author = "Author of Document 2" },
                new() { Id = 3, Title = "Document 3", Author = "Author of Document 3" },
                new() { Id = 4, Title = "Document 4", Author = "Author of Document 4" },
                new() { Id = 5, Title = "Document 5", Author = "Author of Document 5" },
            };
        // Act
        var response = await client.GetAsync("/documents");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var actualDocuments = JsonSerializer.Deserialize<List<Document>>(content, jsonSerializerOptions);
        actualDocuments.Should().NotBeEmpty()
        .And.HaveCount(5)
        .And.BeEquivalentTo(expectedDocuments);
    }


}