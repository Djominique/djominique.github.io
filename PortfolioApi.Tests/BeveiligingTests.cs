
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Models;
using Xunit;

namespace PortfolioApi.Tests;

public class BeveiligingTests : TestBasis
{
    // 1. Registratie geeft altijd de rol Gast.
    [Fact]
    public async Task Registratie_GeeftGastRol()
    {
        // Arrange
        string email = $"gast-{Guid.NewGuid():N}@example.test";
        string wachtwoord = "Aa1!" + Guid.NewGuid().ToString("N");

        BewaarEmail(email);

        // Act: gebruiker vraagt expres Admin aan.
        var response = await Client.PostAsJsonAsync(
            "/auth/register",
            new { email, password = wachtwoord, role = "Admin" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = Factory.Services.CreateScope();

        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        var gebruiker = await users.FindByEmailAsync(email);

        Assert.NotNull(gebruiker);

        var rollen = await users.GetRolesAsync(gebruiker!);

        Assert.Single(rollen);
        Assert.Equal("Gast", rollen[0]);
    }

    // 2. Inloggen met juiste gegevens.
    [Fact]
    public async Task Login_MetGoedWachtwoord_Werkt()
    {
        // Arrange
        string email = $"gast-{Guid.NewGuid():N}@example.test";
        string wachtwoord = "Aa1!" + Guid.NewGuid().ToString("N");

        BewaarEmail(email);

        await Client.PostAsJsonAsync(
            "/auth/register",
            new { email, password = wachtwoord });

        // Act
        var response = await Client.PostAsJsonAsync(
            "/auth/login",
            new { email, password = wachtwoord });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 3. Inloggen met verkeerd wachtwoord.
    [Fact]
    public async Task Login_MetFoutWachtwoord_Geeft401()
    {
        // Arrange
        string email = $"gast-{Guid.NewGuid():N}@example.test";
        string wachtwoord = "Aa1!" + Guid.NewGuid().ToString("N");

        BewaarEmail(email);

        await Client.PostAsJsonAsync(
            "/auth/register",
            new { email, password = wachtwoord });

        // Act
        var response = await Client.PostAsJsonAsync(
            "/auth/login",
            new { email, password = "Verkeerd123!" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 4. GET is openbaar.
    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Get_ZonderLogin_Werkt(string soort)
    {
        // Act
        var response = await Client.GetAsync($"/api/{soort}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 5. Zonder login mag je niet schrijven.
    [Theory]
    [InlineData("POST", "Projects")]
    [InlineData("PUT", "Projects")]
    [InlineData("DELETE", "Projects")]
    [InlineData("POST", "Blogposts")]
    [InlineData("PUT", "Blogposts")]
    [InlineData("DELETE", "Blogposts")]
    public async Task ZonderLogin_Geeft401(
        string methode, string soort)
    {
        // Arrange
        string route = methode == "POST"
            ? $"/api/{soort}"
            : $"/api/{soort}/999999999";

        // Act
        var response = await SchrijfAsync(methode, route);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 6. Gast mag niet schrijven.
    [Theory]
    [InlineData("POST", "Projects")]
    [InlineData("PUT", "Projects")]
    [InlineData("DELETE", "Projects")]
    [InlineData("POST", "Blogposts")]
    [InlineData("PUT", "Blogposts")]
    [InlineData("DELETE", "Blogposts")]
    public async Task Gast_Geeft403(
        string methode, string soort)
    {
        // Arrange
        await LoginAlsAsync("Gast");

        string route = methode == "POST"
            ? $"/api/{soort}"
            : $"/api/{soort}/999999999";

        // Act
        var response = await SchrijfAsync(methode, route);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 7. Editor mag toevoegen en wijzigen, maar niet verwijderen.
    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Editor_MagNietVerwijderen(string soort)
    {
        // Arrange
        await LoginAlsAsync("Editor");

        // POST: Editor mag toevoegen.
        int id = await MaakAsync(soort);

        // PUT: Editor mag wijzigen.
        var put = await Client.PutAsJsonAsync(
            $"/api/{soort}/{id}",
            TestData(soort, "Gewijzigd"));

        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        // DELETE: Editor mag niet verwijderen.
        var delete = await Client.DeleteAsync(
            $"/api/{soort}/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);

        // Controleren of het object nog bestaat.
        var get = await Client.GetAsync($"/api/{soort}/{id}");

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    // 8. Een onbekend ID geeft 404.
    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task Get_OnbekendId_Geeft404(string soort)
    {
        // Act
        var response = await Client.GetAsync(
            $"/api/{soort}/2147483647");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 9. Admin met onbekend ID krijgt 404.
    [Theory]
    [InlineData("PUT", "Projects")]
    [InlineData("DELETE", "Projects")]
    [InlineData("PUT", "Blogposts")]
    [InlineData("DELETE", "Blogposts")]
    public async Task Admin_OnbekendId_Geeft404(
        string methode, string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");

        // Act
        var response = await SchrijfAsync(
            methode, $"/api/{soort}/2147483647");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 10. Ongeldige JSON geeft 400.
    [Theory]
    [InlineData("POST", "Projects")]
    [InlineData("PUT", "Projects")]
    [InlineData("POST", "Blogposts")]
    [InlineData("PUT", "Blogposts")]
    public async Task OngeldigeJson_Geeft400(
        string methode, string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");

        string route = methode == "POST"
            ? $"/api/{soort}"
            : $"/api/{soort}/2147483647";

        using var request = new HttpRequestMessage(
            new HttpMethod(methode), route);

        request.Content = new StringContent(
            "{dit is ongeldige json",
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 11. Een verplichte titel ontbreekt.
    [Theory]
    [InlineData("Projects")]
    [InlineData("Blogposts")]
    public async Task ZonderTitel_Geeft400(string soort)
    {
        // Arrange
        await LoginAlsAsync("Admin");

        object gegevens;

        if (soort == "Projects")
        {
            gegevens = new
            {
                beschrijving = "Test",
                categorie = "Backend",
                gitHubUrl = "https://github.com/example/test",
                datum = DateTime.UtcNow
            };
        }
        else
        {
            gegevens = new
            {
                inhoud = "Test",
                publicatiedatum = DateTime.UtcNow
            };
        }

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/{soort}", gegevens);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
