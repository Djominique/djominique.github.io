
# Portfolio API

## 1. Beschrijving

Dit project is een ASP.NET Core Web API voor mijn portfolio.

Met de API kunnen projecten en blogposts worden opgehaald,
toegevoegd, gewijzigd en verwijderd.

De API gebruikt:
- C# en .NET 10
- ASP.NET Core Controllers
- Entity Framework Core
- SQL Server Express
- ASP.NET Core Identity
- xUnit voor integratietests
- Swagger voor het handmatig testen van endpoints

## 2. Projectstructuur

De applicatie is verdeeld in verschillende onderdelen:

- Controllers: ontvangen en verwerken HTTP-requests.
- Services: bevatten de logica voor de CRUD-acties.
- Models: beschrijven de database-entiteiten.
- DTOs: bepalen welke gegevens de API ontvangt en teruggeeft.
- Data: bevat de DbContext, migraties en DbSeeder.
- PortfolioApi.Tests: bevat de automatische integratietests.

## 3. Database

De applicatie gebruikt SQL Server Express.

Database voor normaal gebruik:
PortfolioDb

Database voor integratietests:
PortfolioTestDb

De testdatabase staat los van de normale database.
Hierdoor kunnen tests gegevens toevoegen, wijzigen en verwijderen
zonder de normale portfolio-gegevens te veranderen.

### Database instellen

De connectionstring wordt ingesteld via de configuratie van
de applicatie onder `ConnectionStrings:DefaultConnection`.

Gebruik hiervoor de naam van je eigen SQL Server-instantie.

### Migraties uitvoeren

Open een terminal in de map PortfolioApi.

Voer uit:

dotnet ef database update

Voor de aparte testdatabase:

dotnet ef database update --connection "Server=DESKTOP-UC9ROU3\SQLEXPRESS;Database=PortfolioTestDb;Trusted_Connection=True;TrustServerCertificate=True"

## 4. API starten

Open een terminal in PortfolioApi.

Voer uit:

dotnet restore
dotnet build
dotnet run

De API is lokaal bereikbaar via de URL die in de terminal verschijnt.

Bij mijn lokale configuratie:

http://localhost:5140/swagger

Met Swagger kunnen de endpoints handmatig getest worden.

## 5. API-endpoints

### Projects

GET /api/Projects
GET /api/Projects/{id}
POST /api/Projects
PUT /api/Projects/{id}
DELETE /api/Projects/{id}

### Blogposts

GET /api/Blogposts
GET /api/Blogposts/{id}
POST /api/Blogposts
PUT /api/Blogposts/{id}
DELETE /api/Blogposts/{id}

### Authenticatie

POST /auth/register
POST /auth/login
POST /auth/logout

## 6. Authenticatie en autorisatie

De API gebruikt ASP.NET Core Identity voor registratie,
inloggen en het beheren van gebruikersrollen.

Er zijn drie rollen:

Gast:
- Mag projecten en blogposts bekijken.
- Mag geen gegevens toevoegen, wijzigen of verwijderen.

Editor:
- Mag projecten en blogposts bekijken.
- Mag projecten en blogposts toevoegen en wijzigen.
- Mag geen projecten of blogposts verwijderen.

Admin:
- Mag projecten en blogposts bekijken.
- Mag projecten en blogposts toevoegen, wijzigen en verwijderen.

Bij openbare registratie krijgt een nieuwe gebruiker
automatisch de rol Gast.

De rechten worden gecontroleerd met authorization policies:

CanEditPortfolio:
Editor en Admin

CanDeletePortfolio:
Alleen Admin

GET-endpoints zijn openbaar.

Bij onvoldoende rechten geeft de API:
- 401 Unauthorized als de gebruiker niet is ingelogd.
- 403 Forbidden als de gebruiker is ingelogd,
  maar niet de juiste rol heeft.

## 7. Lokale testgebruikers

Voor lokaal testen zijn Admin- en Editor-gebruikers beschikbaar.

De DbSeeder maakt rollen aan en kan lokale testgebruikers toevoegen.

Wachtwoorden worden niet rechtstreeks in de broncode opgeslagen,
maar via .NET User Secrets.

Voorbeeld om lokaal een Admin-wachtwoord in te stellen:

dotnet user-secrets set "SeedUsers:AdminPassword" "EIGEN_WACHTWOORD"

Voor de Editor:

dotnet user-secrets set "SeedUsers:EditorPassword" "EIGEN_WACHTWOORD"

Deze commando's voer je uit vanuit de map PortfolioApi.

De voorbeeldwaarde EIGEN_WACHTWOORD moet worden vervangen
door een eigen wachtwoord.

Om een gebruiker lokaal de rol Editor of Admin te geven,
wordt UserManager.AddToRoleAsync gebruikt. Publieke
registratie kent nooit deze rollen toe.

## 8. Integratietests

Het project PortfolioApi.Tests bevat automatische tests
met xUnit en WebApplicationFactory.

Deze tests starten een testversie van de API en sturen
HTTP-requests naar de echte endpoints.

De tests gebruiken SQL Server en de aparte database
PortfolioTestDb.

### Tests uitvoeren

Open een terminal in de hoofdmap van de repository.

Voer uit:

dotnet test PortfolioApi.Tests/PortfolioApi.Tests.csproj

### Wat wordt getest?

- Alle vijf CRUD-routes voor Projects.
- Alle vijf CRUD-routes voor Blogposts.
- Registratie met de rol Gast.
- Inloggen met geldige en ongeldige gegevens.
- Openbare GET-routes.
- 401 bij niet-ingelogde schrijfacties.
- 403 bij schrijfacties zonder de juiste rol.
- Rechten van Editor en Admin.
- 404 bij niet-bestaande ID's.
- 400 bij ongeldige JSON en ontbrekende verplichte velden.
- Opslaan, wijzigen en verwijderen van gegevens.

De tests zijn opgebouwd volgens Arrange, Act en Assert.

## 9. Validatie en foutafhandeling

DTO's worden gebruikt om inkomende gegevens te valideren.

Verplichte velden zijn gemarkeerd met [Required].
URL-velden kunnen worden gecontroleerd met [Url].

Mogelijke HTTP-statuscodes:

200 OK: aanvraag geslaagd.
201 Created: nieuw object aangemaakt.
204 No Content: wijziging of verwijdering geslaagd.
400 Bad Request: ongeldige invoer.
401 Unauthorized: gebruiker is niet ingelogd.
403 Forbidden: gebruiker heeft onvoldoende rechten.
404 Not Found: gevraagde gegevens bestaan niet.

## 10. Beveiliging

- Wachtwoorden worden beheerd door ASP.NET Core Identity.
- Identity slaat wachtwoordhashes op in de database.
- Admin- en Editor-wachtwoorden horen niet in de broncode.
- De testdatabase is gescheiden van de normale database.
- Schrijf- en verwijderacties zijn beveiligd met rollen.
- Publieke registratie kent alleen de rol Gast toe.

## 11. Testbewijs

De werking van de API is vastgelegd met:
- Screenshots van Swagger.
- Een overzicht van alle routes.
- Een matrix met de rechten per gebruikersrol.
- Screenshots van de uitvoer van dotnet test.
- Een toelichting op de gebruikte integratietests.
