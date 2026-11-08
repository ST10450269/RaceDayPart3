# RaceDay project learning map

This folder is a separate study copy of the RaceDay reference solution. The original `DockerDemoApi` files in the parent folder were left unchanged. Review, adapt, and credit/disclose the reference and AI assistance as required by your course; do not present copied work as independently authored.

## Read the code in this order

1. `src/RaceDay.Contracts/Dtos.cs` — request/response shapes shared by the API and MVC. `RegisterRequest` also enforces the registration password rule: at least eight characters with uppercase, lowercase, a number, and a symbol; its behavior is covered by an API test.
2. `src/RaceDay.Api/Models/Entities.cs` and `src/RaceDay.Api/Data/RaceDayDbContext.cs` — database entities and EF Core relationships.
3. `src/RaceDay.Api/Data/SeedData.cs` — local demo data and database initialization.
4. `src/RaceDay.Api/Program.cs` — services, session, database, Swagger, and health endpoint setup.
5. `src/RaceDay.Api/Controllers/AuthController.cs` and `src/RaceDay.Api/Auth/SessionAuthorizeAttribute.cs` — API login/session and role checks.
6. `src/RaceDay.Api/Controllers/EventsController.cs`, then the enrolment/results controllers — business rules and HTTP status codes.
7. `src/RaceDay.Mvc/Program.cs` and `src/RaceDay.Mvc/Services/ApiGateway.cs` — how the website calls the API and forwards its session cookie.
8. MVC controllers and matching `Views/` — how a browser action becomes an API request and then a rendered page.
9. `docker-compose.yml` and `docker/` — how MVC, API, SQL Server, and Azurite are connected.
10. `tests/RaceDay.Api.Tests/ApiBehaviourTests.cs` — executable examples of expected behavior.

## Trace one request

For a participant viewing events, follow: MVC `EventsController` → `ApiGateway` → API `EventsController` → `RaceDayDbContext` → SQL Server. For a protected request, the MVC gateway forwards the API session cookie returned by API login. Image uploads follow MVC → API → `BlobImageStore` → Azure Blob Storage (Azurite in local Docker).

## Verify the copy

From this folder, run `dotnet test RaceDay.sln -c Release`. For the full stack, start Docker Desktop first, then use the Compose commands in `README.md`. Never commit real Azure keys or passwords; keep secrets in local environment/configuration, not public files.
