using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace TestedApi.Test.Integration.Controllers;

public class DocumentsControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetDocumentsReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();
        // Act
        var response = await client.GetAsync("/documents");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    }
}