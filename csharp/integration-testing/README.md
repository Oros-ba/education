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

## Introduce database for persistance and Queue for notification

## Testing with database with [Testcontainers](https://testcontainers.com/)

## Testing with authorization

## Reusing setup code

## Key Takeaways

- Create separate xunit project for integrations testing and use supporting libraries.
- Use testcontainers to provide dependencies for test.
- Change configuration for test application in order to use test dependencies.
- Provide custom testing auth. to avoid need to handle test users (in app or identity server)
- Use Base test class to prepare testing context.