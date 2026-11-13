# Building Management System

A property-management application with an ASP.NET Core 8 API, SQL Server persistence, and an Angular standalone frontend.

## Features

- JWT authentication and role-protected operations
- Units, tenants, leases, invoices, and payment management
- Dashboard KPIs and six-month collected-revenue/occupancy charts
- Filterable occupancy, revenue, arrears, lease-expiry, and document-expiry reports
- Multi-sheet Excel workbook export (`.xlsx`)
- In-app notifications for overdue invoices and leases/documents expiring within 30 days
- Idempotent startup seed data for units, tenants, and active leases

Notifications are stored in the application database and displayed in the dashboard. No email or SMS provider is configured.

## Requirements

- .NET SDK 8
- SQL Server (the default development connection targets a local SQL Server Express instance)
- Node.js and npm

## Run locally

1. Configure `ConnectionStrings:DefaultConnection` and set a random JWT signing key of at least 32 bytes using user secrets or environment variables:

   ```powershell
   dotnet user-secrets set "JwtSettings:SecretKey" "<random-secret-of-at-least-32-bytes>" --project BMS.API
   ```

   Never commit the secret. For deployment, use `JwtSettings__SecretKey` from the hosting environment's secret manager.
2. From the repository root, create/update the database:

   ```powershell
   dotnet ef database update --project BMS.Infrastructure --startup-project BMS.API
   ```

3. Start the API:

   ```powershell
   dotnet run --project BMS.API
   ```

   In Development, Swagger UI is available at `http://localhost:5014/swagger`. The API creates the Admin, Manager, and Viewer roles and seeds sample units, tenants, and leases on startup.
4. Start the Angular application in another terminal:

   ```powershell
   cd bms-frontend
   npm ci
   npm start
   ```

   The frontend uses `http://localhost:5014/api` by default; change `src/environments/environment.ts` when the API uses another address.
5. Register a user through `POST /api/auth/register`, then sign in through `POST /api/auth/login`. Use the returned JWT as a Bearer token in Swagger or the Postman collection.

## Reports and background processing

`GET /api/reports` accepts `fromDate`, `toDate`, and `expiryWithinDays` query parameters. Date filters apply to revenue and overdue balances; expiry results use the configured number of days from today. `GET /api/reports/export` produces a multi-sheet `.xlsx` workbook using the same filters.

The hosted background service checks at startup and then every 24 hours. It marks issued invoices past their due date as overdue and creates de-duplicated in-app notifications for active users.

## API assets

- [Postman collection](./docs/BMS.postman_collection.json)
- [Entity relationship diagram](./docs/ERD.md)
- Swagger UI: `/swagger` in Development

## Validation

```powershell
dotnet build BMS.sln
cd bms-frontend
npm run build
```
