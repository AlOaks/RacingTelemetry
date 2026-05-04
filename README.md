# Racing Telemetry API

A real-time F1 telemetry ingestion and analysis API built with C# and .NET. Syncs lap and car data from the [OpenF1 API](https://openf1.org), stores it across PostgreSQL and MongoDB, and serves live session updates via SignalR WebSockets. Includes a dashboard UI for exploring driver sessions, lap times, and telemetry charts.

---

## Stack

- **ASP.NET Minimal API** — REST endpoints
- **Entity Framework Core + PostgreSQL** — drivers, sessions, laps, driver-session relationships
- **MongoDB** — raw telemetry points (speed, throttle, brake, gear, RPM, position)
- **SignalR** — real-time push to connected clients when a new session is synced
- **FluentValidation** — input validation
- **JWT** — authentication (implemented, relaxed for demo)
- **OpenF1 API** — free public F1 data source, no key required

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Setup

### 1. Clone the repo

```bash
git clone git@github.com:AlOaks/RacingTelemetry.git
cd RacingTelemetry
```

### 2. Start the databases

Make sure Docker Desktop is running, then:

```bash
# PostgreSQL
docker run --name racing-postgres \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=racingtelemetry \
  -p 5432:5432 \
  -d postgres

# MongoDB
docker run --name racing-mongo \
  -e MONGO_INITDB_DATABASE=racingtelemetry \
  -p 27017:27017 \
  -d mongo
```

### 3. Update connection strings

Open `appsettings.json` and update if needed (defaults match the Docker commands above):

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=racingtelemetry;Username=postgres;Password=password"
},
"MongoDB": {
  "ConnectionString": "mongodb://localhost:27017",
  "Database": "racingtelemetry"
}
```

### 4. Run migrations

```bash
dotnet ef database update
```

### 5. Start the API

```bash
dotnet watch
```

The app will be available at `http://localhost:5054` (or whichever port .NET assigns — check the terminal output).

Open your browser at that URL to see the dashboard.

---

## Syncing Data

Use the Sync panel in the UI or call the endpoint directly:

```bash
curl -X POST http://localhost:5054/sync/session/{sessionKey}/{driverNumber}
```

The same session can be synced for multiple drivers — each driver gets their own lap and telemetry data linked to the shared session.

> **Note:** OpenF1 restricts API access during live F1 sessions. If you get a 401, wait until the session ends and try again.

### Ready-to-use sessions

These are verified 2023 F1 race sessions with confirmed telemetry data:

| Circuit    | Country   | Session Key | Driver          | Driver # |
| ---------- | --------- | ----------- | --------------- | -------- |
| Monza      | Italy     | 9161        | Carlos Sainz    | 55       |
| Marina Bay | Singapore | 9165        | Fernando Alonso | 14       |
| Suzuka     | Japan     | 9173        | Max Verstappen  | 1        |
| COTA       | USA       | 9197        | Lewis Hamilton  | 44       |
| Yas Marina | Abu Dhabi | 9222        | George Russell  | 63       |

Sync all 5 (wait ~30 seconds between each to avoid OpenF1 rate limits):

```bash
curl -X POST http://localhost:5054/sync/session/9161/55
curl -X POST http://localhost:5054/sync/session/9165/14
curl -X POST http://localhost:5054/sync/session/9173/1
curl -X POST http://localhost:5054/sync/session/9197/44
curl -X POST http://localhost:5054/sync/session/9222/63
```

You can also sync multiple drivers into the same session. For example, to add Lewis Hamilton to the Monza session:

```bash
curl -X POST http://localhost:5054/sync/session/9161/44
```

---

## API Endpoints

### Auth

| Method | Endpoint      | Description         |
| ------ | ------------- | ------------------- |
| POST   | `/auth/login` | Returns a JWT token |

### Drivers

| Method | Endpoint                 | Description               |
| ------ | ------------------------ | ------------------------- |
| GET    | `/drivers`               | All drivers               |
| GET    | `/drivers/{id}`          | Driver by ID              |
| GET    | `/drivers/{id}/sessions` | All sessions for a driver |
| POST   | `/drivers`               | Create a driver manually  |

### Sessions

| Method | Endpoint                     | Description                                    |
| ------ | ---------------------------- | ---------------------------------------------- |
| GET    | `/sessions`                  | All sessions                                   |
| GET    | `/sessions/{id}`             | Session by ID                                  |
| GET    | `/sessions/{id}/laps`        | All laps for a session (all drivers)           |
| GET    | `/sessions/{id}/leaderboard` | Drivers ranked by fastest lap for that session |
| POST   | `/sessions`                  | Create a session manually                      |

### Laps

| Method | Endpoint                            | Description                          |
| ------ | ----------------------------------- | ------------------------------------ |
| GET    | `/laps/session/{sessionId}/fastest` | Fastest lap for a session            |
| GET    | `/laps/{id}/telemetry`              | Telemetry points for a lap (MongoDB) |
| POST   | `/laps`                             | Submit a lap with telemetry manually |

### Sync

| Method | Endpoint                                    | Description                |
| ------ | ------------------------------------------- | -------------------------- |
| POST   | `/sync/session/{sessionKey}/{driverNumber}` | Sync a session from OpenF1 |

### SignalR

| Hub URL             | Event           | Description                              |
| ------------------- | --------------- | ---------------------------------------- |
| `/hubs/leaderboard` | `SessionSynced` | Fired after every sync with session info |

---

## Architecture Notes

- **PostgreSQL** stores structured relational data — drivers, sessions, laps. Drivers and sessions have a many-to-many relationship through a `DriverSession` junction table, so multiple drivers can share the same session.
- **MongoDB** stores raw telemetry points — high volume, variable structure, referenced by `LapId` back to Postgres.
- **Transactions** — Postgres operations are wrapped in a database transaction. If any step fails, everything rolls back. MongoDB telemetry is inserted after the Postgres commit and handled separately since cross-database transactions aren't supported without a replica set.
- **Session leaderboard** — per-session ranking by fastest lap per driver. Global leaderboards across sessions are intentionally omitted since lap times aren't comparable across different circuits.
- **SignalR** notifies connected clients when a sync completes — clients refetch the session list via REST rather than receiving the full payload over the socket.
- **OpenF1** is a free public API with no authentication required for historical data. Access is restricted during live sessions.
