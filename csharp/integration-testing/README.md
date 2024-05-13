# Integration Testing in dotnet

## Setup projects

Create a new webapi application that will be tested

```sh
dotnet new webapi -n TestedApi
```

Update setup to support controllers

```csharp
builder.Services.AddControllers(); 
//...
app.UseRouting();
app.MapControllers();
```

also add partial class in Program.cs in order to reference it in testing project.

```csharp
public partial class Program
{

}
```

Create xUnit project add Microsoft.AspNetCore.Mvc.Testing

```sh
#create xUnit project
dotnet new xunit -n TestedApi.Test.Integration
#add it to Solution file tests.sln
dotnet sln tests.sln add TestedApi.Test.Integration/TestedApi.Test.Integration.csproj
#add package for testing
cd TestedApi.Test.Integration
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

**Microsoft.AspNetCore.Mvc.Testing contains WebApplicationFactory used to create web app. in memory.**

Reference project to test - in TestedApi.Tests.Integration.csproj add reference TestedApi.csproj.

```xml
<ItemGroup>
    <ProjectReference Include="../TestedApi/TestedApi.csproj" />
</ItemGroup>
```

## Add new controller and test

Create DocumentsController:

```csharp
//DocumentsController.cs
[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("DocumentController is working");
}
```

Run it

```sh
dotnet watch run --project TestedApi/TestedApi.csproj
```

You can **test manually**, open browser <http://localhost:5254/Documents> or use builtin [Swagger](http://localhost:5254/swagger/index.html), also there is option to write calls in [TestedApi.http](./TestedApi/TestedApi.http)

Better option is to **write a simple integration test**.

Using [IClassFixture](https://xunit.net/docs/shared-context#class-fixture) from xUnit and [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) from Microsoft.AspNetCore.Mvc.Testing create test for DocumentsController
See [DocumentsControllerTests.cs](./TestedApi.Test.Integration/DocumentsControllerTests.cs).

```csharp
public class DocumentsControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetDocuments_ReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();
        // Act
        var response = await client.GetAsync("/documents");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Use [FluentAssertions](https://fluentassertions.com/)

Add package FluentAssertions:

```sh
dotnet add package FluentAssertions
```

Use Fluent assertions and make assertion more strict

```csharp
// Assert
response.StatusCode.Should().Be(HttpStatusCode.OK);
var content = await response.Content.ReadAsStringAsync();
content.Should().Be("DocumentController is working");
```

Tip: See all options that this lib. provides to assert http [responses](https://fluentassertions.com/httpresponsemessages/) , [collections](https://fluentassertions.com/collections/) or other data types.

## Introduce database for persistance

Create a Document model and rewrite test to be more realistic adn expect list of Documents.

```csharp
    [Fact]
    public async Task GetDocumentsReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();
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
```

Test will fail and we go to green phase, we will use EF to persist Document model in MS SQL DB.

```sh
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

Use docker to run MSSQL.

```sh
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=your_Strong1Password!' -p 1433:1433 mcr.microsoft.com/mssql/server:latest
```

Create migrations to create database and to seed initial data for documents.

```sh
dotnet ef migrations add CreateDocumentsTable
dotnet ef migrations add SeedDocumentsData
```
in SeedDocumentsData.cs add data to be seeded.

```csharp
 protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Documents ON;

                INSERT INTO Documents (Id, Title, Author) 
                VALUES 
                (1, 'Document 1', 'Author of Document 1'),
                (2, 'Document 2', 'Author of Document 2'),
                (3, 'Document 3', 'Author of Document 3'),
                (4, 'Document 4', 'Author of Document 4'),
                (5, 'Document 5', 'Author of Document 5');

                SET IDENTITY_INSERT Documents OFF;
            ");
        }
```
Update database

```sh
dotnet ef database update
```
Run it
```sh
dotnet watch run --project TestedApi/TestedApi.csproj
```

Select manually all documents on <http://localhost:5254/swagger/index.html>

Now that we can also create other endpoints for creation, update and deletion of documents.

## Testing with database with [testcontainers](https://testcontainers.com/)

The test is green but there is the problem with database we use - we must use testing database.
For this we will use a testcontainers, and because we want to mimic real database we'll add MsSql testcontainer

```sh
dotnet add package Testcontainers
dotnet add package Testcontainers.MsSql
```
And using WebApplicationFactory replace database connection string for DbContext with testcontainer connection string.

At the moment this is in app. conifigured in Program by reading ConnectionString from appsettings.json as

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DocumentsDbContext>(options => options.UseSqlServer(connectionString));

```

In tests, we have to replace this connection string with testcontainer connection string.

```csharp

public class DocumentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WebApplicationFactory<Program> _factory;
    // Define testcontainer for MsSql
    private readonly MsSqlContainer _msSqlServerContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .Build();

    public DocumentsControllerTests(WebApplicationFactory<Program> factory)
    {
        // Start the MsSql container
        _msSqlServerContainer.StartAsync().Wait();

        // Configure services for test application
        _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                  // Find and remove the existing DbContextOptions<DocumentsDbContext> registration
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
```

## Testing with authorization

## Reusing setup code

## Key Takeaways

- Create separate xUnit project for integrations testing and use supporting libraries.
- Use testcontainers to provide dependencies for test.
- Change configuration for test application in order to use test dependencies.
- Provide custom testing auth. to avoid need to handle test users (in app or identity server)
- Use Base test class to prepare testing context.
