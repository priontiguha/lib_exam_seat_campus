# Library & Exam Portal

A full-stack university system for library operations and exam seat allocation.

## Stack
- ASP.NET Core Web API (.NET 8)
- PostgreSQL + EF Core
- React + Vite
- JWT authentication + role-based access control
- SignalR live notifications
- GraphQL query layer

## Project structure
- `LibraryExamAPI/` — backend API
- `client/` — React frontend
- `project.md` — phase-by-phase project brief

## Prerequisites
- .NET 8 SDK
- Node.js 18+
- PostgreSQL running locally
- pgAdmin installed if you want DB management UI

## Local database configuration
The project is configured to use:
- Host: `127.0.0.1` (IPv4 loopback to avoid Windows PostgreSQL IPv6 auth issues)
- Port: `5432`
- Database: `lib_seat_campus`
- Username: `postgres`
- Password: `123`

This is already set in:
- `LibraryExamAPI/appsettings.Development.json`

If PostgreSQL still rejects the connection on Windows, ensure your local `pg_hba.conf` includes an IPv4 rule such as:

```conf
host    all             all             127.0.0.1/32            md5
```

Create the database if it does not already exist:

```sql
CREATE DATABASE lib_seat_campus;
```

## Default credentials
Seeded accounts currently available in the app:

### Admin
- Email: `admin@library.edu`
- Password: `Admin@123`

### Student
- Email: `student1@library.edu`
- Password: `Student@123`

### Librarian
- Email: `librarian1@library.edu`
- Password: `Lib@1234`

## Run the backend
From the workspace root:

```powershell
cd .\LibraryExamAPI
 dotnet run --urls http://localhost:5121
```

The API will be available at:
- `http://localhost:5121`
- Swagger UI: `http://localhost:5121/swagger`

## Run the frontend
Open a second terminal:

```powershell
cd .\client
npm install
npm run dev -- --host 0.0.0.0 --port 5173
```

Frontend URL:
- `http://localhost:5173`

## Important verification endpoints
Backend health check:

```http
GET http://localhost:5121/api/ping
```

Expected response:

```json
{ "status": "ok", "message": "LibraryExamAPI is running." }
```

## Roles supported
- Admin
- Librarian
- Exam Coordinator
- Student

## Build and test verification
These commands were run successfully:

```powershell
cd .\LibraryExamAPI
dotnet build
dotnet test --nologo --verbosity minimal
```

```powershell
cd .\client
npm run build
```

## OTP email setup
OTP delivery is wired through SMTP and falls back to console output if SMTP is not configured. To enable real email delivery, update:

- `LibraryExamAPI/appsettings.Development.json`

with a valid Gmail or SMTP provider configuration:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "From": "noreply@library.edu",
  "EnableSsl": "true"
}
```

For Gmail, use an App Password instead of the normal account password.

## Troubleshooting
- If the API is locked by a stale process, stop the old `LibraryExamAPI` process before rebuilding.
- If PostgreSQL rejects connection, confirm the server is running and the password is `123`.
- If you changed model classes, run the app and allow EF Core migrations to apply automatically.

## Final project status
This project is now configured, integrated, and validated for local development.

It includes:
- JWT auth and RBAC for Admin, Librarian, Exam Coordinator, and Student roles
- Library management with issue/return, fine calculation, reports, and recommendations
- Exam seat allocation, room/invigilator management, manual override, and PDF export
- SignalR-based live notifications
- Dashboard analytics and GraphQL query exposure
- OTP verification flow with SMTP-ready email delivery and console fallback
- Audit logging and frontend dashboard polish

## Final verification
The following commands were executed successfully in the project environment:

```powershell
cd .\LibraryExamAPI.Tests
dotnet test --nologo --verbosity minimal
```

Result: 9 passed, 0 failed.

```powershell
cd .\client
npm run build
```

Result: Vite production build completed successfully.

## Summary
The backend and frontend both validate successfully, PostgreSQL is connected through the configured connection string, and the app is ready for local use in library and exam management workflows.
