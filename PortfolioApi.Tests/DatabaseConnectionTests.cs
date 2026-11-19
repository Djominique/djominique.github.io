
using System.Net;
using Xunit;

namespace PortfolioApi.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public async Task GetProjects_Geeft200()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
