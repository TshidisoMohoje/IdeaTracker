# Full-Stack .NET 8 and React TypeScript Application

## App walkthrough

[Watch the Loom video explaining how the app works](https://www.loom.com/share/5407ff9ce20d467686fda85a89ee793c)

A production-minded full-stack slice demonstrating a .NET 8 REST API, SQL Server persistence through Entity Framework Core, and a typed React frontend.

The backend can be developed in Visual Studio or from the .NET CLI. The frontend can be developed in VS Code or from the command line. Docker is not required.

## Stack

- .NET 8 SDK and ASP.NET Core Web API
- Entity Framework Core 8 with SQL Server
- AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
- FluentValidation 12.1.1
- React with TypeScript
- Vite development server
- SQL Server or SQL Server Express/Developer

## Prerequisites

Install the following before starting:

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Node.js LTS](https://nodejs.org/), which includes `npm`
3. [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads), SQL Server Express, or SQL Server Developer
4. Optional: [SQL Server Management Studio](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) or another SQL client
5. Optional: Visual Studio 2022 with the ASP.NET and web development workload for backend development
6. Optional: VS Code for frontend development

Verify the SDK and Node.js installations:

```powershell
dotnet --version
node --version
npm --version
```

The project targets .NET 8. The package references used by the API are:

- `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
- `FluentValidation` 12.1.1
- `FluentValidation.DependencyInjectionExtensions` 12.1.1
- `Microsoft.EntityFrameworkCore` 8.0.0
- `Microsoft.EntityFrameworkCore.Design` 8.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.0
- `Microsoft.EntityFrameworkCore.Tools` 8.0.0

These are application dependencies and should be restored by the project file with `dotnet restore`; they do not normally need to be installed globally.

## Repository layout

The repository is expected to contain a backend solution/project and a frontend application. After cloning, identify the exact paths with:

```powershell
Get-ChildItem -Recurse -File -Include *.sln,*.csproj,package.json
```

Typical layouts look like this:

```text
/
├── backend/       # .sln and ASP.NET Core API project
├── frontend/      # React TypeScript application and package.json
└── README.md
```

The commands below assume `backend` and `frontend` are the directory names. Replace them with the actual paths in this repository when necessary.

## Configure SQL Server

The API needs a SQL Server connection string before Entity Framework Core can create or access the database. A local SQL Server Express connection string often looks like this:

```text
Server=.\\SQLEXPRESS;Database=FullStackAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

For SQL Server LocalDB, use:

```text
Server=(localdb)\\MSSQLLocalDB;Database=FullStackAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

For SQL Server authentication, use a connection string appropriate for your local instance instead. Do not commit passwords, access tokens, or other secrets.

### Development configuration options

ASP.NET Core loads `appsettings.Development.json` when the environment is `Development`. A convenient local-only option is to add the connection string there:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FullStackAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Keep this file limited to non-sensitive local settings. If it contains credentials, add it to `.gitignore` and use user secrets or an environment variable instead.

The preferred option for a developer machine is .NET User Secrets. From the API project directory, run:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=FullStackAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

ASP.NET Core also maps environment variables to configuration keys by replacing `:` with `__`. For example, in PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\\MSSQLLocalDB;Database=FullStackAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

These variables apply only to the current PowerShell session. Set them again when opening a new terminal, or use User Secrets for persistent per-user development configuration.

## Create or update the database

If the repository includes EF Core migrations, run this from the solution or API project directory:

```powershell
dotnet ef database update
```

If the `dotnet ef` command is unavailable, install the EF Core CLI tool once:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
```

If the tool is already installed, update it when appropriate:

```powershell
dotnet tool update --global dotnet-ef --version 8.0.0
```

If the API and `DbContext` are in different projects, specify them explicitly. Adjust the paths and project names to match the repository:

```powershell
dotnet ef database update `
  --project .\backend\YourApi\YourApi.csproj `
  --startup-project .\backend\YourApi\YourApi.csproj
```

Do not run migrations until SQL Server is running and the configured connection string points to the intended local instance.

## Configure the frontend

The frontend should use a Vite environment file for the local API URL. Create `frontend/.env.local`:

```dotenv
VITE_API_BASE_URL=https://localhost:7036
```

Use the HTTP URL instead when the frontend is configured for HTTP:

```dotenv
VITE_API_BASE_URL=http://localhost:5052
```

Vite only exposes variables prefixed with `VITE_` to browser code. Do not place secrets in a `VITE_` variable because frontend environment values are bundled into client-side JavaScript.

The frontend code must read the same variable name, for example:

```ts
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
```

If this repository uses a different variable name, use that existing name in `.env.local` rather than adding a second configuration convention.

## Run the API and frontend separately

### Terminal 1: API

From the API project directory:

```powershell
dotnet restore
dotnet run --launch-profile https
```

If the repository does not define an `https` launch profile, use:

```powershell
dotnet run
```

The API is typically available at:

- `https://localhost:7036`
- `http://localhost:5052`

The exact ports come from the API project's `Properties/launchSettings.json`. Treat that file as authoritative if the ports differ.

On the first HTTPS request, the browser may warn about the local development certificate. Trust the certificate with:

```powershell
dotnet dev-certs https --trust
```

### Terminal 2: frontend

From the frontend directory:

```powershell
npm install
npm run dev
```

The Vite development server normally opens at:

```text
http://localhost:5173
```

If the frontend does not start automatically, open that URL manually. Keep the API terminal running while using the frontend.

## Scriptable full-stack startup

From the repository root, the following PowerShell commands install dependencies, apply database migrations, and start both applications in separate windows:

```powershell
$apiPath = Join-Path $PWD "backend"
$frontendPath = Join-Path $PWD "frontend"

Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$apiPath'; dotnet restore; dotnet ef database update; dotnet run --launch-profile https"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$frontendPath'; npm install; npm run dev"
```

Before running this command:

- Replace `backend` and `frontend` if the repository uses different directory names.
- Ensure `.env.local` exists in the frontend directory.
- Ensure the API connection string is configured through User Secrets, `appsettings.Development.json`, or the environment.
- If the API project is not the working directory under `backend`, update the API path to the directory containing its `.csproj` file.

For a repeatable team workflow, add repository scripts only after confirming the final folder and project names. Avoid committing machine-specific paths or secrets.

## Useful development commands

Backend:

```powershell
dotnet build
dotnet test
dotnet run
```

Frontend:

```powershell
npm install
npm run dev
npm run build
npm run preview
```

Inspect the API's OpenAPI/Swagger page, if enabled, at the URL printed by `dotnet run`, commonly `/swagger`.

## Common issues and solutions

### Port already in use

Another process may already be listening on port 7036, 5052, or 5173. Find the process on Windows:

```powershell
Get-NetTCPConnection -LocalPort 7036,5052,5173 -ErrorAction SilentlyContinue |
  Select-Object LocalPort,OwningProcess
```

Stop the owning process only after confirming it is safe to do so, or change the API ports in `Properties/launchSettings.json` and the matching `VITE_API_BASE_URL` value in `.env.local`. Vite can also use another port with `npm run dev -- --port 5174`.

### SQL Server cannot be reached

Confirm that the SQL Server service or LocalDB instance is installed and running. Check the server name in the connection string: `.\\SQLEXPRESS`, `(localdb)\\MSSQLLocalDB`, and `localhost` refer to different setups. Also verify authentication mode and that `TrustServerCertificate=True` is present for a local development connection when required.

### Database or table does not exist

Configure the connection string first, then run `dotnet ef database update`. If the project uses a non-default `DbContext` or separate startup project, repeat the command with `--context`, `--project`, and `--startup-project` as needed.

### `dotnet ef` is not recognized

Install the .NET 8 EF CLI tool and make sure the global .NET tools directory is on `PATH`:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
```

Restart the terminal after changing `PATH`.

### HTTPS certificate warning or SSL connection failure

Trust the local certificate:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Then restart the API and refresh the frontend. For local-only troubleshooting, use the HTTP API URL in `.env.local`.

### Browser CORS error

The frontend origin must be allowed by the API's CORS policy. The default Vite origin is `http://localhost:5173`; `https://localhost:5173` is a different origin. Confirm the frontend URL, API URL, and configured allowed origins all use the same protocol and port. Restart the API after changing CORS configuration.

### Frontend cannot read the API URL

Verify that the file is named `.env.local`, the variable starts with `VITE_`, and the frontend was restarted after editing it. Vite reads environment variables at startup, not on every request.

### Package restore or build errors

Confirm that the required .NET SDK is installed with `dotnet --version`, then run `dotnet restore`. For the frontend, remove an incomplete install only if necessary and reinstall with `npm install`. Check that the Node.js version is compatible with the frontend's `package.json` and lockfile.

### API starts but requests fail with 404 or 500

Check the route and HTTP method in Swagger or the browser network panel. Inspect the API terminal for validation, database, or mapping errors. A successful process start only confirms that the host started; it does not confirm that the database schema or requested endpoint is configured correctly.

## Security and repository hygiene

- Never commit passwords, connection strings containing credentials, API keys, or production certificates.
- Keep local secrets in .NET User Secrets, environment variables, or ignored local files.
- Commit a safe example such as `appsettings.Development.example.json` or `.env.example` when documenting required keys.
- Use development-only SQL Server databases and development certificates on local machines.
- Review CORS, authentication, logging, and connection settings before deploying outside local development.

## License

Add the repository's license information here before publishing if the project will be distributed publicly.
