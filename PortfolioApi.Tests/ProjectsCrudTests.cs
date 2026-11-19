
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

public class ProjectsCrudTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    private readonly List<int> _testProjectIds = new();

    private string _adminEmail = "";
    private string _adminPassword = "";

    // Wordt vóór iedere test uitgevoerd.
    public async Task InitializeAsync()
    {
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

        // Extra beveiliging: nooit de normale database gebruiken.
        if (db.Database.GetDbConnection().Database != "PortfolioTestDb")
        {
            throw new InvalidOperationException(
                "De tests gebruiken niet PortfolioTestDb!");
        }

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        // Een unieke testgebruiker per test.
        _adminEmail = $"testadmin-{Guid.NewGuid():N}@example.test";
        _adminPassword = "Aa1!" + Guid.NewGuid().ToString("N");

        var admin = new User
        {
            UserName = _adminEmail,
            Email = _adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            admin, _adminPassword);

        Assert.True(result.Succeeded);

        var roleResult = await userManager.AddToRoleAsync(
            admin, "Admin");

        Assert.True(roleResult.Succeeded);

        // Inloggen via de echte HTTP-route.
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email = _adminEmail,
                password = _adminPassword
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    // Hulpmethode om een project via de API aan te maken.
    private async Task<int> MaakTestProjectAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Projects",
            new
            {
                titel = "Automatisch testproject",
                beschrijving = "Aangemaakt door xUnit",
                categorie = "Backend",
                gitHubUrl = "https://github.com/example/test",
                datum = DateTime.UtcNow
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        int id = json.RootElement.GetProperty("id").GetInt32();

        _testProjectIds.Add(id);

        return id;
    }

    [Fact]
    public async Task GetProjects_Geeft200EnEenLijst()
    {
        // Arrange: de testclient is ingelogd.

        // Act
        var response = await _client.GetAsync("/api/Projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            JsonValueKind.Array,
            json.RootElement.ValueKind);
    }

    [Fact]
    public async Task GetProject_MetBestaandId_GeeftProject()
    {
        // Arrange
        int id = await MaakTestProjectAsync();

        // Act
        var response = await _client.GetAsync($"/api/Projects/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            id,
            json.RootElement.GetProperty("id").GetInt32());

        Assert.Equal(
            "Automatisch testproject",
            json.RootElement.GetProperty("titel").GetString());
    }

    [Fact]
    public async Task CreateProject_Geeft201EnSlaatOp()
    {
        // Arrange
        var project = new
        {
            titel = "Project via POST-test",
            beschrijving = "Test of POST gegevens opslaat",
            categorie = "Testing",
            gitHubUrl = "https://github.com/example/post-test",
            datum = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Projects", project);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        int id = json.RootElement.GetProperty("id").GetInt32();

        _testProjectIds.Add(id);

        // Controleren of het project echt is opgeslagen.
        var getResponse = await _client.GetAsync(
            $"/api/Projects/{id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        using var opgeslagen = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Project via POST-test",
            opgeslagen.RootElement.GetProperty("titel").GetString());
    }

    [Fact]
    public async Task UpdateProject_WijzigtBestaandProject()
    {
        // Arrange
        int id = await MaakTestProjectAsync();

        var wijziging = new
        {
            titel = "Gewijzigd door xUnit",
            beschrijving = "Nieuwe beschrijving",
            categorie = "Software Engineering",
            gitHubUrl = "https://github.com/example/put-test",
            datum = DateTime.UtcNow
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Projects/{id}", wijziging);

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        // Controleren of de wijziging echt is opgeslagen.
        var getResponse = await _client.GetAsync(
            $"/api/Projects/{id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var json = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Gewijzigd door xUnit",
            json.RootElement.GetProperty("titel").GetString());
    }

    [Fact]
    public async Task DeleteProject_VerwijdertProject()
    {
        // Arrange
        int id = await MaakTestProjectAsync();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/Projects/{id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        // Een verwijderd project mag niet meer bestaan.
        var getResponse = await _client.GetAsync(
            $"/api/Projects/{id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    // Wordt na iedere test uitgevoerd, ook als de test faalt.
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
                // Alleen de projecten verwijderen die
                // deze test zelf heeft aangemaakt.
                await db.Projects
                    .Where(p => _testProjectIds.Contains(p.Id))
                    .ExecuteDeleteAsync();

                // Ook de tijdelijke testgebruiker verwijderen.
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<User>>();

                var user = await userManager.FindByEmailAsync(
                    _adminEmail);

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
