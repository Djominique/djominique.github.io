using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models;
using PortfolioApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<BlogpostService>();

builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PortfolioDbContext>();

builder.Services.AddAuthentication();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditPortfolio", policy =>
        policy.RequireRole("Editor", "Admin"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGroup("/auth")
    .MapIdentityApi<User>();

await DbSeeder.SeedAsync(app.Services);

app.Run();