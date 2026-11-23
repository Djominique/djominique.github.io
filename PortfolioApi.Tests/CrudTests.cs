
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PortfolioApi.Tests;

public class CrudTests : TestBasis
{
    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task GetAlle_GeeftEenLijst(string soort)
    {
        // Arrange: geen login nodig.

        // Act
        var response = await Client.GetAsync($"/api/{soort}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task GetOpId_GeeftJuisteGegevens(string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");
        int id = await MaakAsync(soort);

        // Act
        var response = await Client.GetAsync($"/api/{soort}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(id, json.GetProperty("id").GetInt32());
        Assert.Equal("Test", json.GetProperty("titel").GetString());
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Post_MaaktGegevensAan(string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");

        // Act
        int id = await MaakAsync(soort);

        // Assert: bestaat het nieuwe object echt?
        var response = await Client.GetAsync($"/api/{soort}/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Test", json.GetProperty("titel").GetString());
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Put_WijzigtGegevens(string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");
        int id = await MaakAsync(soort);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/{soort}/{id}",
            TestData(soort, "Gewijzigde titel"));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync(
            $"/api/{soort}/{id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var json = await getResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Gewijzigde titel",
            json.GetProperty("titel").GetString());
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Delete_VerwijdertGegevens(string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");
        int id = await MaakAsync(soort);

        // Act
        var response = await Client.DeleteAsync(
            $"/api/{soort}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync(
            $"/api/{soort}/{id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
