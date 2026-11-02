# RaceDay | PROG6212 Programming 2B | Parts 1–3

A South African road-running, walking and cycling event-management solution built with ASP.NET Core 8.0. It comprises a **Code-First SQL Server Web API**, a separate **ASP.NET Core MVC frontend that consumes that API**, integration tests, Azure Blob-compatible image storage, and Docker packaging. Built to follow the 2026 RaceDay brief for all three PoE parts.

> **Submission rule:** The official PoE accepts your **GitHub repository link on ARC**, **not a ZIP**. This ZIP is a source folder to open in Visual Studio, review, run and commit. The GitHub commit history, actual green CI screenshot and unlisted human-narrated YouTube videos cannot be manufactured and must be supplied by the person submitting the work. If AI assistance was used, disclose it according to the brief. Do not claim this source as unaided personal work.

## Quick start: full system using Docker Desktop

**Requirements:** Docker Desktop running with Linux containers enabled, enough RAM for SQL Server and internet to obtain the container images and NuGet packages. On Windows, enable WSL2 if Docker Desktop requires it.

From the repository root, run:

```powershell
docker compose up --build -d
docker compose ps
docker compose logs api --tail 80
```

Once the API has started successfully, visit:

- **MVC website:** http://localhost:8080
- **API Swagger UI:** http://localhost:5080/swagger
- **API health:** http://localhost:5080/health

The first startup creates the `RaceDayDb` SQL Server database using EF Core's Code-First model and seeds sample users, events, categories, enrolments and a completed race result. The Compose configuration also starts Azurite (a local Azure Blob Storage emulator), so banner and profile picture uploads can be tested without buying a cloud subscription. In production, substitute a real Azure Blob Storage connection string. The MVC talks **only to the API**, never directly to SQL Server or Azure.

Stop the stack with `docker compose down`. This keeps database and Blob data volumes. `docker compose down -v` **deletes all demo data**.

### Demo accounts (local/testing only)

| Role | Email | Password |
|---|---|---|
| Organiser | organiser@raceday.test | DemoOrg123! |
| Organiser | organiser2@raceday.test | DemoOrg123! |
| Participant | runner@raceday.test | DemoRunner123! |
| Participant | runner2@raceday.test | DemoRunner123! |

Passwords are stored as ASP.NET Core Identity PBKDF2 hashes, not in plaintext in the database. The displayed credentials are **deliberately public demo credentials** and must be replaced for a non-demo deployment.

### Visual Studio 2022 run, without containerising the MVC

1. Install .NET 8 SDK, Visual Studio 2022 ASP.NET and web development workload, Docker Desktop, and SQL Server (or use the SQL Server service in Compose, exposed locally on port 14333).
2. Run `docker compose up -d sqlserver azurite` to provide database and local Blob dependencies.
3. If the default SA password was changed, configure `ConnectionStrings__RaceDay` accordingly. The default local-only password in this sample is in `appsettings.json` and the Docker SQL Server host port is `14333`.
4. For **API uploads outside Docker**, configure `AzureBlob__ConnectionString` to `UseDevelopmentStorage=true` (Azurite must be reachable at localhost:10000), or use a real Azure Storage connection string.
5. Open **RaceDay.sln**. Restore NuGet packages with `dotnet restore RaceDay.sln`.
6. Start `RaceDay.Api` on http://localhost:5080 (launch profile provided).
7. Start `RaceDay.Mvc` on http://localhost:5081. Open the site. MVC calls the API at http://localhost:5080.
8. Browse http://localhost:5080/swagger to test the API. Use `POST /api/auth/login` first; your browser's API session cookie will accompany later Swagger requests.

### Tests and build

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln -c Release
dotnet test RaceDay.sln -c Release
```

The integration test project (`tests/RaceDay.Api.Tests`) runs API endpoints against an isolated in-memory **SQLite** database, so GitHub Actions does not require a running SQL Server. The deployed API uses SQL Server, as required by the assessment.

## Project structure

```text
RaceDay.sln
README.md
Dockerfile references: docker/Dockerfile.api + docker/Dockerfile.mvc
docker-compose.yml
.github/workflows/raceday-ci.yml
docs/
  RaceDay_ERD.png
  RaceDay_ERD.dot
  RaceDay_ERD_Explanation.md
  RaceDay_API_Endpoint_Plan.md
  RaceDay_Database.sql
  Part_1_Checklist.md
  Part_2_Checklist.md
  Part_3_Checklist.md
  SUBMISSION_EVIDENCE.md
src/
  RaceDay.Contracts/   Shared request and response DTOs
  RaceDay.Api/         SQL Server/EF Core, session auth, Swagger, Blob storage
  RaceDay.Mvc/         MVC controllers, Razor views, REST API client, Bootstrap styling
tests/
  RaceDay.Api.Tests/   Registration, login, access rules, CRUD, enrolments, results
```

## Part 1: System planning and database (100 marks)

- `docs/RaceDay_ERD.png` visualises six related tables, primary keys, foreign keys, unique constraints and relationship cardinalities; the editable DOT file is included.
- `docs/RaceDay_API_Endpoint_Plan.md` covers every exposed route and has all six required columns.
- `docs/RaceDay_Database.sql` is a standalone SQL Server Management Studio script which creates the six-table schema and seeds two Organisers, two Participants, three Events, four Categories, three Enrolments and one Result.
- The same entities and mappings appear in the API's `Data/RaceDayDbContext.cs`. The standalone script is for **Part 1 SQL/SSMS demonstration**. On a blank database, the running API generates the schema via `EnsureCreated` (Code First) and seeds equivalent demonstration data. **Do not run the Part 1 SQL script against a database already initialised by the API.** The two setup paths are alternatives, not cumulative steps.

## Part 2: REST API and automated tests (100 marks)

**Authentication:** `POST /api/auth/register`, `POST /api/auth/login` and `POST /api/auth/logout`. The API stores user ID and role in **server-side session state**. Registration supports both Organiser and Participant roles. Passwords are hashed with ASP.NET Core `PasswordHasher<AppUser>`.

**Roles:** Organisers can create/edit/delete only their own events, manage categories, view enrolled participants and publish results after events conclude. Participants can browse events, enrol in available categories, see their entries and view their own race results. Protected API endpoints return HTTP 401 (no session) or 403 (wrong role or ownership).

**REST routes:** Full route plan in `docs/RaceDay_API_Endpoint_Plan.md`; includes events, categories, enrolments, profiles, results and image uploads. Swagger `/swagger` exposes the API, with XML documentation for primary endpoints.

**Database:** SQL Server and Entity Framework Core Code First, six tables and constrained relationships. The API uses `EnsureCreated` for a simple first-run learning environment; EF migrations are recommended before later production schema upgrades.

**Tests:** 20 xUnit integration scenarios in `tests/RaceDay.Api.Tests/ApiBehaviourTests.cs`, covering success and failure of registration/login, anonymous access, role separation, event creation/edit/delete restrictions, duplicate/invalid entries, result publishing and logout.

**CI/CD:** `.github/workflows/raceday-ci.yml` validates Part 1 docs, restores/builds all projects and runs tests on pushes and pull requests. A **green screenshot** requires an actual successful GitHub Actions run.

## Part 3: MVC, Azure Blob Storage and Docker (100 marks)

The separate `RaceDay.Mvc` website has registration/login, role-specific navigation, an Organiser dashboard (events, total entries and upcoming dates), event CRUD, category management, enrolment status controls and result publication. Participant pages have upcoming event search/filter, category choice, immediate event entry, My Events, My Results and profile editing.

**Blob storage:** Organiser event banners and personal profile pictures go via MVC's HTTP client **to the API**, then into Azure Blob Storage. The API checks file type and signature (JPG, PNG, WEBP) and limits file size to 5 MB. On a local Compose startup, Azurite emulates Blob Storage. To connect to a live Azure account, set the API environment variable `AzureBlob__ConnectionString` to a valid Azure Storage connection string. Do not put Azure keys in public GitHub files.

**Session transfer:** After MVC login calls the API, it safely retains the API's server-session cookie on the MVC server-side session and forwards it when requesting API resources. The API does not assume that an MVC session alone grants access.

**Docker:** `docker/Dockerfile.mvc` packages the MVC application into an ASP.NET 8 runtime image. `docker-compose.yml` connects MVC, API, SQL Server and Azurite. The MVC uses `ApiSettings__BaseUrl=http://api:8080/` within the Docker network.

To prove a standalone MVC image/container (when **API, SQL Server and Azurite** have been started):

```powershell
docker compose up -d sqlserver azurite api
docker build -f docker/Dockerfile.mvc -t raceday-mvc:part3 .
docker run --rm --name raceday-mvc -p 8080:8080 --add-host host.docker.internal:host-gateway -e ApiSettings__BaseUrl=http://host.docker.internal:5080/ raceday-mvc:part3
```

On Windows or Mac, Docker Desktop provides `host.docker.internal`. On Linux the `--add-host` flag configures it. Do not run the Compose MVC service at the same time on host port 8080 as the standalone container.

## Before your final ARC submission

See `docs/SUBMISSION_EVIDENCE.md` and the checklists for each part. A code archive alone is not a complete assessed PoE. The official brief requires at least **20 meaningful, authentic Git commits per part**, successful CI, a screenshot of that green run, and an unlisted **YouTube demonstration for each part using the presenter's own voice**. Replace the evidence placeholders in this README with genuine links or screenshots only after obtaining them.

| Evidence | Current status |
|---|---|
| GitHub repository URL | **TO ADD after you push the source** |
| Part 1 video URL | **TO ADD after you record and upload** |
| Part 2 video URL | **TO ADD after you record and upload** |
| Part 3 video URL | **TO ADD after you record and upload** |
| Screenshot of green GitHub Actions run | **TO ADD after a passing GitHub run** |
| Local build/test + Docker demonstration evidence | **NOT YET VERIFIED in the file-generation environment** |

**Attribution/disclosure:** This implementation was drafted with AI assistance. Review, test, understand, adapt and disclose that assistance as required by the official 2026 PoE instructions; never present generated material or fictitious evidence as solely your own.

## Known environment scope and production considerations

This is a demonstration-focused coursework solution, not a production deployment. The Docker credentials are local development examples; change passwords and use a proper secret store and HTTPS for internet-facing deployments. Session state is held in memory and requires a shared distributed store for multiple API replicas. The app starts with `EnsureCreated` rather than versioned EF Core migrations. Blob emulator storage is not the Azure public cloud; use a real Azure Storage account if the assessor specifically requires showing a cloud-hosted object. No live build, runtime, Docker or cloud verification was possible where these files were generated because the .NET SDK and Docker engine are not installed there. **Do not report build/test success until you have actually run the commands above.**
