# Clean Architecture Web API Template (ASP.NET Core 8)

This template provides a Clean Architecture setup for ASP.NET Core 8 with SQL Server, JWT auth, and basic API wiring.

## Project layout
- `src/CleanTemplate.Domain`: Entities and core business rules.
- `src/CleanTemplate.Application`: Use cases, interfaces, and DTOs.
- `src/CleanTemplate.Infrastructure`: EF Core, Identity, external integrations.
- `src/CleanTemplate.WebApi`: API endpoints, DI, auth, Swagger.
- `tests/CleanTemplate.UnitTests`: Unit tests.
- `tests/CleanTemplate.IntegrationTests`: Integration tests with an in-memory DB.

## Run locally
1. Update `Jwt:Key` and the SQL Server connection string in `src/CleanTemplate.WebApi/appsettings.json`.
2. Create the database:
   ```powershell
   dotnet ef migrations add InitialCreate -s src/CleanTemplate.WebApi -p src/CleanTemplate.Infrastructure
   dotnet ef database update -s src/CleanTemplate.WebApi -p src/CleanTemplate.Infrastructure
   ```
3. Run the API:
   ```powershell
   dotnet run --project src/CleanTemplate.WebApi
   ```

## Use as a dotnet template
From the repository root:
```powershell
dotnet new install .
dotnet new cleanapi -n MyProject
```

To uninstall:
```powershell
dotnet new uninstall .
```
