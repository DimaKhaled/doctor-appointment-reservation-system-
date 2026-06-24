# Doctor Appointment Management System (DAMS)

ASP.NET Core MVC implementation for the Doctor Appointment Reservation System.

## Phase 0 Scope

- SQL Server schema through EF Core models and migration.
- Patient registration.
- Login, logout, and change password.
- Cookie authentication.
- Role-based authorization for Patient, Doctor, and Admin.
- Shared Bootstrap 5 layout and navigation.
- Seed accounts for all three roles.

## Requirements

- .NET SDK 9
- SQL Server LocalDB or SQL Server Express
- EF Core CLI tool:

```powershell
dotnet tool install --global dotnet-ef --version 9.*
```

## Setup Steps

1. Open PowerShell in the repository folder.

```powershell
cd "C:\Users\20100\Documents\Doctor Appointment"
```

2. Restore and build the solution.

```powershell
dotnet restore
dotnet build Dams.sln
```

3. Create or update the local database.

```powershell
dotnet ef database update --project Dams.Web\Dams.Web.csproj --startup-project Dams.Web\Dams.Web.csproj
```

4. Run the MVC app.

```powershell
dotnet run --project Dams.Web\Dams.Web.csproj --launch-profile http
```

5. Open the app.

```text
http://localhost:5141
```

## Seeded Accounts

All seeded accounts use this password:

```text
Dams@123
```

| Role | Email |
| --- | --- |
| Patient | patient@dams.local |
| Doctor | doctor@dams.local |
| Admin | admin@dams.local |

## Notes For The Team

- Role names must stay exactly: `Patient`, `Doctor`, `Admin`.
- All pages should use `Views/Shared/_Layout.cshtml`.
- Shared application styling belongs in `Dams.Web/wwwroot/css/site.css`.
- The database connection string is in `Dams.Web/appsettings.json`.
