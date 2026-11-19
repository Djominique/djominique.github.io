using Microsoft.AspNetCore.Identity;
using PortfolioApi.Models;

namespace PortfolioApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        string[] rollen = { "Gast", "Admin", "Editor" };

        foreach (var rol in rollen)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

 
const string adminEmail = "admin@portfolio.test";

var adminPassword = scope.ServiceProvider
    .GetRequiredService<IConfiguration>()
    ["SeedUsers:AdminPassword"];

var admin = await userManager.FindByEmailAsync(adminEmail);

if (admin == null)
{
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Admin-wachtwoord ontbreekt in User Secrets.");
    }

    admin = new User
    {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(admin, adminPassword);

    if (!result.Succeeded)
    {
        throw new InvalidOperationException(
            "Admin-gebruiker aanmaken mislukt.");
    }
}

if (!await userManager.IsInRoleAsync(admin, "Admin"))
{
    await userManager.AddToRoleAsync(admin, "Admin");
}

        const string editorEmail = "editor@portfolio.test";

var editorPassword = scope.ServiceProvider
    .GetRequiredService<IConfiguration>()
    ["SeedUsers:EditorPassword"];

var editor = await userManager.FindByEmailAsync(editorEmail);

if (editor == null && !string.IsNullOrEmpty(editorPassword))
{
    editor = new User
    {
        UserName = editorEmail,
        Email = editorEmail,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(editor, editorPassword);

    if (!result.Succeeded)
    {
        throw new Exception("Editor aanmaken mislukt.");
    }
}

if (editor != null &&
    !await userManager.IsInRoleAsync(editor, "Editor"))
{
    await userManager.AddToRoleAsync(editor, "Editor");
}
    }
}