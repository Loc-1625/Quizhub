# QuizHub Run Guide (Local + Docker)

This document provides two clear ways to run QuizHub:
- Local Development mode
- Docker mode

Tech stack:
- Frontend: Vue.js (Vite)
- Backend: ASP.NET Core
- Database: Microsoft SQL Server
- ORM: Entity Framework Core (Migration + seed)

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

Check connection string in appsettings for local SQL Server.
Current local style in this project is Windows Integrated Security.

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
MSSQL_SA_PASSWORD=QhB_2026#Demo!Sql9
```

Use a SQL Server compatible strong password.

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

### 4.3 Image pull/network errors (MCR EOF)

If docker pull from mcr.microsoft.com fails with EOF:
1. Ensure Docker Desktop engine is running
2. Disable VPN/proxy/HTTPS inspection if possible
3. Try another network (for example mobile hotspot)
4. Retry docker compose up --build
