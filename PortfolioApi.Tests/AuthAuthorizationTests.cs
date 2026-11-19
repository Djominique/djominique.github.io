
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Data;
using PortfolioApi.Models;
using Xunit;

namespace PortfolioApi.Tests;

public class AuthAuthorizationTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    private readonly List<string> _userEmails = new();
    private readonly List<int> _projectIds = new();
    private readonly List<int> _blogpostIds = new();

    private static object GeldigeGegevens() => new
    {
        titel = "Security-test",
        beschrijving = "Testproject",
        categorie = "Testing",
        gitHubUrl = "https://github.com/example/security",
        datum = DateTime.UtcNow,
        inhoud = "Testinhoud voor een blogpost",
        publicatiedatum = DateTime.UtcNow
    };

    public Task InitializeAsync()
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

        if (db.Database.GetDbConnection().Database
            != "PortfolioTestDb")
        {
            throw new InvalidOperationException(
                "Tests mogen uitsluitend PortfolioTestDb gebruiken.");
        }

        return Task.CompletedTask;
    }

    // Hulpmethode: maak via de echte API een Gast.
    private async Task<(string Email, string Password)> RegistreerGastAsync(
        bool vraagAdminRolAan = false)
    {
        string email = $"gast-{Guid.NewGuid():N}@example.test";
        string password = "Aa1!" + Guid.NewGuid().ToString("N");

        _userEmails.Add(email);

        var response = await _client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email,
                password,
                role = vraagAdminRolAan ? "Admin" : "Gast"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (email, password);
    }

    // Hulpmethode: maak lokaal een gebruiker met een
    // specifieke rol, uitsluitend voor de tests.
    private async Task<(string Email, string Password)> MaakRolGebruikerAsync(
        string role)
    {
        string email = $"rol-{Guid.NewGuid():N}@example.test";
        string password = "Aa1!" + Guid.NewGuid().ToString("N");

        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            user, password);

        Assert.True(result.Succeeded);

        _userEmails.Add(email);

        var roleResult = await userManager.AddToRoleAsync(
            user, role);

        Assert.True(roleResult.Succeeded);

        return (email, password);
    }

    private async Task LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Bouw een HTTP-request voor een schrijfactie.
    private async Task<HttpResponseMessage> SchrijfAsync(
        string method, string path)
    {
        using var request = new HttpRequestMessage(
            new HttpMethod(method), path);

        if (method == "POST" || method == "PUT")
        {
            request.Content = JsonContent.Create(GeldigeGegevens());
        }

        return await _client.SendAsync(request);
    }

    [Fact]
    public async Task Register_GeeftAltijdGastRol()
    {
        // Arrange + Act: probeer zelf "Admin" mee te sturen.
        var gegevens = await RegistreerGastAsync(
            vraagAdminRolAan: true);

        // Assert: controleer echte Identity-rollen.
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        var user = await userManager.FindByEmailAsync(
            gegevens.Email);

        Assert.NotNull(user);

        var rollen = await userManager.GetRolesAsync(user!);

        Assert.Single(rollen);
        Assert.Equal("Gast", rollen[0]);
    }

    [Fact]
    public async Task Login_MetCorrectWachtwoord_Geeft200()
    {
        // Arrange
        var gast = await RegistreerGastAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email = gast.Email,
                password = gast.Password
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_MetVerkeerdWachtwoord_Geeft401()
    {
        // Arrange
        var gast = await RegistreerGastAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email = gast.Email,
                password = "VerkeerdWachtwoord123!"
            });

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Theory]
    [InlineData("/api/Projects")]
    [InlineData("/api/Blogposts")]
    public async Task Get_ZonderLogin_BlijftOpenbaar(string path)
    {
        // Act
        var response = await _client.GetAsync(path);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Projects")]
    [InlineData("PUT", "/api/Projects/999999999")]
    [InlineData("DELETE", "/api/Projects/999999999")]
    [InlineData("POST", "/api/Blogposts")]
    [InlineData("PUT", "/api/Blogposts/999999999")]
    [InlineData("DELETE", "/api/Blogposts/999999999")]
    public async Task Schrijven_ZonderLogin_Geeft401(
        string method, string path)
    {
        // Arrange: bewust niet inloggen.

        // Act
        var response = await SchrijfAsync(method, path);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Projects")]
    [InlineData("PUT", "/api/Projects/999999999")]
    [InlineData("DELETE", "/api/Projects/999999999")]
    [InlineData("POST", "/api/Blogposts")]
    [InlineData("PUT", "/api/Blogposts/999999999")]
    [InlineData("DELETE", "/api/Blogposts/999999999")]
    public async Task Gast_MagNietSchrijven_Geeft403(
        string method, string path)
    {
        // Arrange
        var gast = await RegistreerGastAsync();

        await LoginAsync(gast.Email, gast.Password);

        // Act
        var response = await SchrijfAsync(method, path);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Editor_MagToevoegenEnWijzigenMaarNietVerwijderen(
        string resource)
    {
        // Arrange
        var editor = await MaakRolGebruikerAsync("Editor");

        await LoginAsync(editor.Email, editor.Password);

        var postResponse = await _client.PostAsJsonAsync(
            $"/api/{resource}", GeldigeGegevens());

        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        using var json = System.Text.Json.JsonDocument.Parse(
            await postResponse.Content.ReadAsStringAsync());

        int id = json.RootElement
            .GetProperty("id").GetInt32();

        if (resource == "Projects")
            _projectIds.Add(id);
        else
            _blogpostIds.Add(id);

        // Act: wijzigen
        var putResponse = await _client.PutAsJsonAsync(
            $"/api/{resource}/{id}",
            GeldigeGegevens());

        // Assert: wijzigen mag.
        Assert.Equal(
            HttpStatusCode.NoContent,
            putResponse.StatusCode);

        // Act: verwijderen
        var deleteResponse = await _client.DeleteAsync(
            $"/api/{resource}/{id}");

        // Assert: verwijderen mag niet.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            deleteResponse.StatusCode);

        // Het object moet nog steeds bestaan.
        var getResponse = await _client.GetAsync(
            $"/api/{resource}/{id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Theory]
    [InlineData("/api/Projects/2147483647")]
    [InlineData("/api/Blogposts/2147483647")]
    public async Task Get_MetOnbekendId_Geeft404(string path)
    {
        // Act
        var response = await _client.GetAsync(path);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Theory]
    [InlineData("PUT", "/api/Projects/2147483647")]
    [InlineData("DELETE", "/api/Projects/2147483647")]
    [InlineData("PUT", "/api/Blogposts/2147483647")]
    [InlineData("DELETE", "/api/Blogposts/2147483647")]
    public async Task Admin_OnbekendId_Geeft404(
        string method, string path)
    {
        // Arrange
        var admin = await MaakRolGebruikerAsync("Admin");

        await LoginAsync(admin.Email, admin.Password);

        // Act
        var response = await SchrijfAsync(method, path);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/Projects")]
    [InlineData("PUT", "/api/Projects/2147483647")]
    [InlineData("POST", "/api/Blogposts")]
    [InlineData("PUT", "/api/Blogposts/2147483647")]
    public async Task OngeldigeJson_Geeft400(
        string method, string path)
    {
        // Arrange
        var admin = await MaakRolGebruikerAsync("Admin");

        await LoginAsync(admin.Email, admin.Password);

        // Opzettelijk ongeldige JSON.
        using var request = new HttpRequestMessage(
            new HttpMethod(method), path);

        request.Content = new StringContent(
            "{dit is geen geldige json",
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        if (_factory == null)
            return;

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<PortfolioDbContext>();

        if (db.Database.GetDbConnection().Database
            == "PortfolioTestDb")
        {
            await db.Projects
                .Where(p => _projectIds.Contains(p.Id))
                .ExecuteDeleteAsync();

            await db.Blogposts
                .Where(b => _blogpostIds.Contains(b.Id))
                .ExecuteDeleteAsync();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            foreach (string email in _userEmails)
            {
                var user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                }
            }
        }

        _client?.Dispose();
        _factory.Dispose();
    }
}
