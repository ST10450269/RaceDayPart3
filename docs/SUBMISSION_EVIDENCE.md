# RaceDay final submission evidence to collect

The source archive is for development. ARC submission must be your GitHub Classroom repository link (not ZIP).

## Authorship and history

1. Review every part, adapt and understand it, and disclose AI assistance as required by the PoE.
2. Push updates incrementally as genuine work is completed. You must have 20+ meaningful contributions **per part**; a newly packaged archive cannot retroactively create historical commits for earlier work.
3. Inspect the Actions tab to confirm a green **actual** build, then capture a screenshot and embed its relative file path into the README. No dummy screenshot is provided.

## Separate unlisted YouTube walkthrough videos

- **Part 1:** explain six-table ERD, relationship choices, endpoint plan and run the database script live inside SQL Server Management Studio.
- **Part 2:** show Swagger, register/login as both roles, blocked role actions, database integration and passing xUnit tests.
- **Part 3:** show MVC Organiser and Participant screens, create/update an event, upload banner and profile picture via API, enrol, publish results for an already-finished event, Docker Desktop and commands.

## Minimum final checks

Run `dotnet build RaceDay.sln -c Release` then `dotnet test RaceDay.sln -c Release`; use `docker compose up --build -d` and check both http://localhost:8080 and http://localhost:5080/swagger. Capture screenshots of the **real** results. Replace all TODO items in the README with actual verified links and evidence.
