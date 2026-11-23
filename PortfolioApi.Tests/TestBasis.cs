
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

public abstract class TestBasis : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

    private readonly List<int> projectIds = new();
    private readonly List<int> blogpostIds = new();
    private readonly List<string> emails = new();

    // Wordt voor iedere test uitgevoerd.
    public Task InitializeAsync()
    {
        Factory = new CustomWebApplicationFactory();

        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            });

        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<PortfolioDbContext>();

        if (db.Database.GetDbConnection().Database != "PortfolioTestDb")
            throw new Exception("Verkeerde testdatabase!");

        return Task.CompletedTask;
    }

    // Testgegevens voor Projects of Blogposts.
    protected object TestData(string soort, string titel = "Test")
    {
        if (soort == "Projects")
        {
            return new
            {
                titel,
                beschrijving = "Testbeschrijving",
                categorie = "Backend",
                gitHubUrl = "https://github.com/example/test",
                datum = DateTime.UtcNow
            };
        }

        return new
        {
            titel,
            inhoud = "Testinhoud",
            publicatiedatum = DateTime.UtcNow
        };
    }

    // Tijdelijke gebruiker met Gast-, Editor- of Admin-rol.
    protected async Task LoginAlsAsync(string rol)
    {
        string email = $"test-{Guid.NewGuid():N}@example.test";
        string wachtwoord = "Aa1!" + Guid.NewGuid().ToString("N");

        using var scope = Factory.Services.CreateScope();

        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        var gebruiker = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var resultaat = await users.CreateAsync(
            gebruiker, wachtwoord);

        Assert.True(resultaat.Succeeded);

        emails.Add(email);

        var rolResultaat = await users.AddToRoleAsync(
            gebruiker, rol);

        Assert.True(rolResultaat.Succeeded);

        var login = await Client.PostAsJsonAsync(
            "/auth/login",
            new { email, password = wachtwoord });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    // Een nieuw testproject of testblogpost maken.
    protected async Task<int> MaakAsync(string soort)
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/{soort}", TestData(soort));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        int id = json.GetProperty("id").GetInt32();

        if (soort == "Projects")
            projectIds.Add(id);
        else
            blogpostIds.Add(id);

        return id;
    }

    // Een POST, PUT of DELETE uitvoeren.
    protected async Task<HttpResponseMessage> SchrijfAsync(
        string methode, string route)
    {
        using var request = new HttpRequestMessage(
            new HttpMethod(methode), route);

        if (methode == "POST" || methode == "PUT")
        {
            string soort = route.Contains("Projects")
                ? "Projects"
                : "Blogposts";

            request.Content = JsonContent.Create(TestData(soort));
        }

        return await Client.SendAsync(request);
    }

    // Testgebruikers en testgegevens opruimen.
    public async Task DisposeAsync()
    {
        if (Factory == null)
            return;

        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<PortfolioDbContext>();

        if (db.Database.GetDbConnection().Database == "PortfolioTestDb")
        {
            await db.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ExecuteDeleteAsync();

            await db.Blogposts
                .Where(b => blogpostIds.Contains(b.Id))
                .ExecuteDeleteAsync();

            var users = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            foreach (string email in emails)
            {
                var gebruiker = await users.FindByEmailAsync(email);

                if (gebruiker != null)
                    await users.DeleteAsync(gebruiker);
            }
        }

        Client?.Dispose();
        Factory.Dispose();
    }

    // Gebruikers die via registratie zijn aangemaakt
    // ook bewaren voor het opruimen.
    protected void BewaarEmail(string email)
    {
        emails.Add(email);
    }
}
