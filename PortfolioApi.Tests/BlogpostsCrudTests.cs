
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Data;
using PortfolioApi.Models;
using Xunit;

namespace PortfolioApi.Tests;

public class BlogpostsCrudTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    private readonly List<int> _blogpostIds = new();

    private string _email = "";
    private string _password = "";

    public async Task InitializeAsync()
    {
        // Arrange: een aparte test-API starten.
        _factory = new CustomWebApplicationFactory();

        _client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            });

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<PortfolioDbContext>();

        if (db.Database.GetDbConnection().Database
            != "PortfolioTestDb")
        {
            throw new InvalidOperationException(
                "Verkeerde database!");
        }

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        // Iedere test krijgt een eigen Admin.
        _email = $"blog-admin-{Guid.NewGuid():N}@example.test";
        _password = "Aa1!" + Guid.NewGuid().ToString("N");

        var admin = new User
        {
            UserName = _email,
            Email = _email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            admin, _password);

        Assert.True(result.Succeeded);

        var roleResult = await userManager.AddToRoleAsync(
            admin, "Admin");

        Assert.True(roleResult.Succeeded);

        var login = await _client.PostAsJsonAsync(
            "/auth/login",
            new { email = _email, password = _password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    private async Task<int> MaakBlogpostAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Blogposts",
            new
            {
                titel = "Testblogpost",
                inhoud = "Dit is een bericht voor xUnit.",
                publicatiedatum = DateTime.UtcNow
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        int id = json.RootElement
            .GetProperty("id").GetInt32();

        _blogpostIds.Add(id);

        return id;
    }

    [Fact]
    public async Task GetBlogposts_Geeft200EnEenLijst()
    {
        // Arrange: testclient bestaat.

        // Act
        var response = await _client.GetAsync("/api/Blogposts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            JsonValueKind.Array,
            json.RootElement.ValueKind);
    }

    [Fact]
    public async Task GetBlogpost_MetBestaandId_Geeft200()
    {
        // Arrange
        int id = await MaakBlogpostAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/Blogposts/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            id,
            json.RootElement.GetProperty("id").GetInt32());

        Assert.Equal(
            "Testblogpost",
            json.RootElement.GetProperty("titel").GetString());
    }

    [Fact]
    public async Task CreateBlogpost_SlaatGegevensOp()
    {
        // Arrange
        var blogpost = new
        {
            titel = "Nieuwe blogpost",
            inhoud = "Deze inhoud moet opgeslagen worden.",
            publicatiedatum = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Blogposts", blogpost);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        int id = json.RootElement
            .GetProperty("id").GetInt32();

        _blogpostIds.Add(id);

        // Controleren of de blogpost echt bestaat.
        var getResponse = await _client.GetAsync(
            $"/api/Blogposts/{id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        using var opgeslagen = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Nieuwe blogpost",
            opgeslagen.RootElement
                .GetProperty("titel").GetString());

        Assert.Equal(
            "Deze inhoud moet opgeslagen worden.",
            opgeslagen.RootElement
                .GetProperty("inhoud").GetString());
    }

    [Fact]
    public async Task UpdateBlogpost_WijzigtGegevens()
    {
        // Arrange
        int id = await MaakBlogpostAsync();

        var wijziging = new
        {
            titel = "Bijgewerkte blogpost",
            inhoud = "De inhoud is gewijzigd.",
            publicatiedatum = DateTime.UtcNow
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Blogposts/{id}", wijziging);

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/Blogposts/{id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        using var json = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Bijgewerkte blogpost",
            json.RootElement.GetProperty("titel").GetString());

        Assert.Equal(
            "De inhoud is gewijzigd.",
            json.RootElement.GetProperty("inhoud").GetString());
    }

    [Fact]
    public async Task DeleteBlogpost_VerwijdertGegevens()
    {
        // Arrange
        int id = await MaakBlogpostAsync();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/Blogposts/{id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/Blogposts/{id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<PortfolioDbContext>();

            if (db.Database.GetDbConnection().Database
                == "PortfolioTestDb")
            {
                // Alleen de blogposts van deze test opruimen.
                await db.Blogposts
                    .Where(b => _blogpostIds.Contains(b.Id))
                    .ExecuteDeleteAsync();

                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<User>>();

                var user = await userManager.FindByEmailAsync(
                    _email);

                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                }
            }

            _client?.Dispose();
            _factory.Dispose();
        }
    }
}
