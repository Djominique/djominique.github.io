# Portfolio API

## Project

Deze applicatie is de backend van mijn portfoliosite. De API is gebouwd met ASP.NET Core Web API en bevat functionaliteit voor het beheren van projecten en blogposts.

De API ondersteunt voor zowel Projects als Blogposts volledige CRUD-functionaliteit:

- GET alle items
- GET op id
- POST
- PUT op id
- DELETE op id

Daarnaast heb ik ASP.NET Core Identity toegevoegd voor authenticatie en autorisatie.

## Technieken

Voor dit project gebruik ik:

- .NET 10
- ASP.NET Core Web API
- Controllers
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Swagger / Swashbuckle

## Gekozen API-stijl

Ik heb gekozen voor Controllers in plaats van Minimal API.

Controllers zorgen ervoor dat de HTTP-afhandeling duidelijk gescheiden blijft van de applicatielogica en datatoegang. De Controllers roepen Services aan en de Services gebruiken de PortfolioDbContext voor toegang tot de database.

## Codestructuur

De applicatie is opgebouwd in verschillende lagen:

```text
Controllers/
    ProjectsController.cs
    BlogpostsController.cs

Services/
    ProjectService.cs
    BlogpostService.cs

Data/
    PortfolioDbContext.cs
    DbSeeder.cs

Models/
    Project.cs
    Blogpost.cs
    User.cs

DTOs/
    ProjectDto.cs
    CreateProjectDto.cs
    UpdateProjectDto.cs
    BlogpostDto.cs
    CreateBlogpostDto.cs
    UpdateBlogpostDto.cs
```

De verantwoordelijkheden zijn als volgt verdeeld:

- **Controllers:** verwerken HTTP-requests en bepalen het HTTP-resultaat.
- **Services:** bevatten de applicatielogica en voeren CRUD-bewerkingen uit.
- **Data:** bevat de DbContext en de koppeling met SQL Server.
- **Models:** beschrijven de entiteiten die door EF Core worden opgeslagen.
- **DTOs:** bepalen welke gegevens de API ontvangt en terugstuurt.

## Database

De applicatie gebruikt SQL Server als database.

De connectionstring staat in:

```text
appsettings.json
```

De PortfolioDbContext gebruikt Entity Framework Core om verbinding te maken met SQL Server.

De database bevat onder andere gegevens voor:

- Projects
- Blogposts
- Identity-gebruikers
- Identity-rollen

## Database aanmaken

De database wordt aangemaakt en bijgewerkt met Entity Framework Core migrations.

Voor de eerste versie van de database:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Na het toevoegen van ASP.NET Core Identity is een extra migration aangemaakt:

```bash
dotnet ef migrations add AddIdentity
dotnet ef database update
```

Hierdoor worden ook de Identity-tabellen aangemaakt, zoals:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
```

## Applicatie starten

Ga in een terminal naar de map waarin `PortfolioApi.csproj` staat.

Start daarna de API met:

```bash
dotnet run
```

In de terminal wordt vervolgens het lokale adres van de API weergegeven.

Swagger kan daarna via de `/swagger`-route worden geopend.

## Authenticatie en autorisatie

De API gebruikt ASP.NET Core Identity.

Gebruikers kunnen inloggen via de Identity-endpoints. Wachtwoorden worden door Identity gehasht opgeslagen.

De applicatie gebruikt de rollen:

- Admin
- Editor

De GET-endpoints voor Projects en Blogposts zijn openbaar.

Voor POST, PUT en DELETE geldt een authorization-policy. Alleen gebruikers met de rol Admin of Editor mogen deze endpoints gebruiken.

## Repository

De broncode van dit project staat in mijn Git-repository.

De commitgeschiedenis is in de repository terug te vinden en laat de verschillende stappen van de ontwikkeling zien.
https://github.com/Djominique/djominique.github.io 
