using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models;
using PortfolioApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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
        policy.RequireRole("Editor", "Admin"))
    .AddPolicy("CanDeletePortfolio", policy =>
        policy.RequireRole("Admin"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


await DbSeeder.SeedAsync(app.Services);

app.Run();

public partial class Program { }