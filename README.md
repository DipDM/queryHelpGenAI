# AI Smart Query Builder

A .NET 8 WebAPI that helps users generate, validate, save, and safely execute parameterized SQL queries against external databases using Generative AI (OpenAI). It acts as a middleware assistant: connect any target database (SQL Server / PostgreSQL), let the AI produce SQL from natural language and schema, preview or run queries under a read-only account, and audit all runs.

---

## Features

* Natural-language → SQL generation using LLM (AIService)
* Schema discovery (INFORMATION_SCHEMA) for target databases via `DataSource` entries
* Save and manage `QueryTemplates` and `SavedQueries`
* Preview and execute parameterized, read-only queries via `QueryRunner` (SQL Server & PostgreSQL)
* JWT-based authentication and role-based authorization
* Audit and run history (`QueryRuns`, `AuditLogs`)
* Swagger UI with JWT "Authorize" support
* Caching for schema lookup, AI prompt safety checks, and basic SQL safety heuristics

---

## Quick start (local development)

Prerequisites:

* .NET 8 SDK
* Docker (optional, for sample DB)
* SQL Server or PostgreSQL (or use provided Docker scripts)
* An OpenAI API key (or other LLM provider) for AI suggestions

1. Clone the repo (example placeholder):

   ```bash
   git clone https://github.com/dipDM/queryHelpGenAI.git
   cd queryHelp
   ```

2. Set up user secrets (development) to store sensitive keys:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "replace-with-a-long-secret"
   dotnet user-secrets set "AI:ApiKey" "sk-..."
   dotnet user-secrets set "DefaultAdmin:Username" "admin"
   dotnet user-secrets set "DefaultAdmin:Password" "Admin@123"
   ```

3. Configure `appsettings.json` connection string for metadata DB (this project uses SQL Server by default for metadata). Example `appsettings.json` excerpt:

```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=(localdb)\\mssqllocaldb;Database=AiSmartQueryBuilderMetadata;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "",
    "Issuer": "AiSmartQueryBuilder",
    "Audience": "AiSmartQueryBuilderClients",
    "ExpireMinutes": 60
  },
  "AI": {
    "Provider": "OpenAI",
    "Model": "gpt-4o-mini",
    "ApiKey": ""
  }
}
```

4. Run EF Core migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef database update
```

5. Run the app:

```bash
dotnet run
```

6. Open Swagger UI (e.g. `https://localhost:5001/swagger`) and use the **Auth** endpoints to create/login an admin user. Then `POST /api/datasources` to register an external DB and `GET /api/datasources/debug/schema/{id}` to verify schema discovery. Use `/api/ai/suggest` for SQL generation and `/api/queryexecution/preview` to run previews.

---

## Important endpoints (summary)

* `POST /api/auth/register` — register user
* `POST /api/auth/login` — login and get JWT
* `POST /api/datasources` — add a new external DB (connectionString in body)
* `GET /api/datasources` — list datasources
* `GET /api/datasources/debug/schema/{id}` — view discovered schema
* `POST /api/ai/suggest` — generate SQL from natural language
* `POST /api/queryexecution/preview` — preview (execute) a SQL on a datasource
* `POST /api/savedqueries/{id}/run` — run a saved query
* `GET /api/queryruns/{id}` — view run status and metadata

---

## Security & Best practices

* **Never** store real API keys or credentials in source code. Use User Secrets for dev and a secret manager (Key Vault / AWS Secrets Manager) in production.
* Use dedicated **read-only** DB users for target datasources; never execute with admin credentials.
* Replace the simple regex-based SQL safety checks with a proper SQL parser (T-SQL or PostgreSQL grammar) for production.
* Enforce per-user rate limits and quotas on LLM calls to control cost.
* Audit every AI prompt and resulting SQL in `AuditLogs`.

---

## File layout (important files)

* `Controllers/` — controllers including `AuthController`, `DataSourcesController`, `AiController`, `QueryExecutionController`
* `Services/` — `AiService`, `SchemaService`, `QueryRunnerService`
* `Data/` — `ApplicationDbContext` and EF models / migrations
* `DTOs/` — request/response DTOs
* `Helpers/` — `SqlSafety` and other utilities
* `Settings/` — `AiSettings`, `JwtSettings`

---
