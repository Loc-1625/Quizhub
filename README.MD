# QuizHub Run Guide (Local + Docker)

This document provides two clear ways to run QuizHub:
- Local Development mode
- Docker mode

Tech stack:
- Frontend: Vue.js (Vite)
- Backend: ASP.NET Core
- Database: Microsoft SQL Server
- ORM: Entity Framework Core (Migration + seed)

## 0. First-time setup after git pull

This project does not commit real secret files.
After pulling source code, create your local config files from templates.

### 0.1 Windows (PowerShell)

Run from project root:

```powershell
Copy-Item .env.example .env
Copy-Item frontend/.env.example frontend/.env
Copy-Item backend/QuizHub/appsettings.template.json backend/QuizHub/appsettings.Development.json
```

### 0.2 macOS/Linux

Run from project root:

```bash
cp .env.example .env
cp frontend/.env.example frontend/.env
cp backend/QuizHub/appsettings.template.json backend/QuizHub/appsettings.Development.json
```

Then edit values in:
- .env (Docker SQL password)
- frontend/.env (frontend local variables)
- backend/QuizHub/appsettings.Development.json (backend local secrets/connection string)

## 1. Run in Local Development

Use this mode when you want fast code changes and debugging.

### 1.1 Prerequisites

Install:
- .NET SDK 9.0+
- Node.js 20+
- SQL Server (Developer/Express/LocalDB)

Quick checks:

```bash
dotnet --version
node --version
npm --version
```

### 1.2 Backend setup and run

Go to backend project folder:

```bash
cd backend/QuizHub
```

> **⚠️ IMPORTANT:** Remember to modify the connection string in your `appsettings` file to match your local SQL Server instance before running!
> The default template uses Windows Integrated Security (`Server=localhost;Database=QuizHub;Trusted_Connection=True;...`).

Recommended local config file:
- backend/QuizHub/appsettings.Development.json

Run backend:

```bash
dotnet run
```

On startup, backend will:
1. Run EF Core migrations automatically
2. Create/update database schema
3. Run seed logic (including default admin if not exists)

Default backend URL in this project:
- http://localhost:5099

### 1.3 Frontend setup and run

Open a second terminal:

```bash
cd frontend
npm install
npm run dev
```

Default frontend URL:
- http://localhost:5173

In local mode, Vite proxy forwards /api, /hubs, /uploads to backend.

### 1.4 Stop local mode

- Stop frontend and backend terminals with Ctrl + C.

## 2. Run with Docker

Use this mode for demo/report with one command.

### 2.1 Prerequisites

Install Docker Desktop with Docker Compose v2.

Quick checks:

```bash
docker --version
docker compose version
```

### 2.2 Required Docker files

At project root:
- docker-compose.yml
- .env
- .env.example

Build/runtime files:
- backend/QuizHub/Dockerfile
- frontend/Dockerfile
- frontend/nginx.conf

### 2.3 Environment configuration

Create/update .env at project root:

```env
MSSQL_SA_PASSWORD=QhB_2026#Demo!Sql21
```

Use a SQL Server compatible strong password.

Note:
- Docker mode only needs root .env for docker-compose variables.
- frontend/.env is mainly for local Vite development.

### 2.4 Start all services

From project root:

```bash
docker compose up --build
```

What happens automatically:
1. SQL Server container starts
2. Backend starts after SQL Server is healthy
3. Backend runs EF Core migrations at startup
4. Backend runs seed logic (including default admin)
5. Frontend is built and served by Nginx

### 2.5 Access URLs

After startup:
- Frontend: http://localhost:5173
- Backend API: http://localhost:5099
- SQL Server: localhost:1433

### 2.6 Verify migration and seed

Check backend logs:

```bash
docker compose logs -f backend
```

Check service status:

```bash
docker compose ps
```

### 2.7 Stop Docker mode

Stop and keep database volume:

```bash
docker compose down
```

Stop and remove all (including SQL data):

```bash
docker compose down -v
```

### 2.8 Connect to SQL Server Database

You can connect to the SQL Server database running inside Docker using tools like **SQL Server Management Studio (SSMS)**, **Azure Data Studio**, or **DBeaver**.

**Connection details:**
- **Server Name / Host**: `localhost,1433` (or just `localhost`)
- **Authentication**: `SQL Server Authentication`
- **Login / Username**: `sa`
- **Password**: The password you set in your `.env` file (variable `MSSQL_SA_PASSWORD`)
- **Trust Server Certificate**: `True` (Important: Check this box/set to true to bypass SSL/TLS certificate errors)

*Note: Data changes made here are saved permanently in the `sqlserver_data` Docker volume.*

## 3. Common Docker maintenance

Rebuild selected services:

```bash
docker compose up --build backend frontend
```

Follow all logs:

```bash
docker compose logs -f
```

Restart one service:

```bash
docker compose restart backend
```

## 4. Notes and Troubleshooting

### 4.1 Connection string behavior

- Docker mode: backend uses environment variable from docker-compose
- Local mode: backend uses appsettings local connection string

### 4.2 SQL Server data persistence in Docker

Data is stored in Docker volume:
- sqlserver_data

Normal restart keeps data.
Only docker compose down -v removes it.

### 4.3 IPv6 causes EOF when pulling from MCR

In some networks, IPv6 routing to mcr.microsoft.com is unstable.
This can cause EOF errors when pulling Microsoft images, while Docker Hub images still pull normally.

Typical signs:
- docker pull alpine (or other images) succeeds
- docker pull mcr.microsoft.com/dotnet/aspnet:9.0 may succeed or fail intermittently
- docker pull mcr.microsoft.com/mssql/server:2022-latest fails with EOF

How to fix on Windows:
1. Temporarily disable IPv6 on the active network adapter (Wi-Fi/Ethernet)
2. Restart Docker Desktop completely
3. Retry:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker compose up --build
```

Optional PowerShell commands (run as Administrator):

```powershell
# Disable IPv6 binding on Wi-Fi
Disable-NetAdapterBinding -Name "Wi-Fi" -ComponentID ms_tcpip6

# Re-enable IPv6 binding later (if you need it)
Enable-NetAdapterBinding -Name "Wi-Fi" -ComponentID ms_tcpip6
```

If your adapter name is different, replace "Wi-Fi" with the correct adapter name shown by:

```powershell
Get-NetAdapter | Select-Object Name, Status
```
