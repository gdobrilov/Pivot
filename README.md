# Pivot

**We turn things around.** Pivot is a fictional startup with exactly one product: a cube you can turn.
This repository is the entire company. It is my submission for the TTC Group Rubik's Cube challenge.

The cube starts solved and oriented like [rubiks-cube-solver.com](https://rubiks-cube-solver.com/):
green front, red right, white up. Every quarter turn goes on the record.

## Quick start

You need the free [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and nothing else.

```bash
dotnet run --project src/RubiksCube.Console
```

That prints the solved cube, applies the sequence from the brief (**F R' U B' L D'**) and prints the result:

```
       R O G
       B W W
       B B B
G Y Y  O R R  Y B O  Y B W
O O G  O G W  R R W  O B Y
B G O  W W W  O Y R  Y Y W
       G G B
       R Y R
       R G G
```

It matches the picture in the brief tile for tile. `dotnet test` runs everything.

## What is in the box

| Project | What it does |
|---|---|
| `src/RubiksCube.Domain` | The cube, the geometry of a turn, and `CubeSession` (a cube plus its log). No dependencies. |
| `src/RubiksCube.Application` | One handler per command/query, the ports they need, read models. |
| `src/RubiksCube.Infrastructure` | EF Core + SQLite, migration, clock, in-process event dispatcher. |
| `src/RubiksCube.Console` | **The deliverable.** Prints the net before and after a sequence. |
| `src/RubiksCube.Api` | Web API: sessions, turns, preview, audit log, undo. OpenAPI docs at `/scalar`. |
| `src/web` | React + TypeScript. Click a face, pick how far, see the preview, turn it. |
| `tests/*` | xUnit per layer (domain, handlers, EF on SQLite, HTTP end to end, console) and Vitest for the UI. |

Everything is free and open source. No paid tools.

## Running the rest

**API** (<http://localhost:5000>, docs at `/scalar`, health at `/health`):

```bash
dotnet run --project src/RubiksCube.Api
```

| Method | Route | What |
|---|---|---|
| `POST` | `/api/cubes` | New solved cube. `201` + `Location`. |
| `GET` | `/api/cubes/{id}` | Current state. |
| `GET` | `/api/cubes/{id}/preview?face=Right&rotation=AntiClockwise` | What the turn would do. Nothing stored. |
| `POST` | `/api/cubes/{id}/rotations` | `{ "face": "Front", "rotation": "Clockwise" }`. Rotation: `Clockwise`, `AntiClockwise`, `Half`. |
| `GET` | `/api/cubes/{id}/rotations` | The log: every turn, undo and reset, with UTC time. |
| `POST` | `/api/cubes/{id}/rotations/undo` | Undo the last effective turn. Recorded, not deleted. |
| `POST` | `/api/cubes/{id}/reset` | Back to solved. The log stays. |
| `GET` | `/api/cubes/{id}/net` | The net as `text/plain`. |

Errors are RFC 9457 problem details: `400` bad input or nothing to undo, `404` unknown cube, `409` two requests
changed the same cube at once. The SQLite file (`rubiks.db`) is created and migrated on first start.

**Web** (start the API first):

```bash
cd src/web
npm install
npm run dev        # http://localhost:5173, proxies /api to the API
```

Also `npm test`, `npm run lint`, `npm run typecheck`, `npm run build`.

**Docker** (optional; nothing above needs it):

```bash
docker compose up --build          # web on :3000, API on :8080, SQLite in a volume
docker compose run --rm console    # the brief's output; add moves as arguments
```

**Console options:** `-- --steps` prints the cube after every move; `-- "R U R' U'"` applies your own sequence
(face letter, `'` for anti-clockwise, `2` for 180°). Invalid input prints why and exits with 1.

## Tests

| Suite | Proves |
|---|---|
| Domain | Every single turn against the solver site; the brief's picture; four turns = identity, `X X'` = identity, `X2` = `X X`; the reading-direction rule; the model has no hard-coded 3; log, undo and reset semantics. |
| Application | Each handler with fakes: validation, not found, conflict; events go out only after a successful save. |
| Infrastructure | The real EF mapping on in-memory SQLite built by the migration: round trip, concurrency conflict, readable columns. |
| Console | Output captured: the brief's result, `--steps`, exit code and usage on bad input. |
| Api | HTTP end to end in-process: statuses, `Location`, enum names, the brief's sequence, preview, log, problem details. |
| web | Net, face selection, preview highlighting, picker, log, API client, session hook. |

CI runs the .NET build and tests on Windows and Linux, the web checks, and builds the Docker images.

## How it is built

```
 Console ─┐                                   ┌─ Infrastructure (EF Core/SQLite, clock, dispatcher)
          ├─► Application ──► Domain ◄────────┘
     API ─┘   (commands, queries, ports)
     web talks to the API over HTTP only
```

**The cube.** Immutable: a turn returns a new cube. Each face is a `FaceGrid` (rows and columns as seen from
outside). A turn is a permutation of the 54 stickers, built once per move from a table in `FaceGeometry` that says,
in words, which four neighbouring strips a face drags round and from which corner each is read. Anti-clockwise is
the inverse permutation, 180° is the clockwise one twice. There is no `if` per face.

**The session.** `CubeSession` holds the cube and an append-only log (`Rotation`, `Undo`, `Reset`). Undo applies the
inverse of the last effective move and writes a log line; nothing is deleted. The log is the audit trail. Every
change raises an event that is dispatched after the change is saved.

**The layers.** Application defines each use case as a command or query with one handler, and the ports it needs
(`ICubeSessionRepository`, `IClock`, `IDomainEventDispatcher`). Expected failures come back as a `Result`, mapped to a
status code in one place. Infrastructure maps the aggregate with Fluent configuration only: the cube as a facelet
string, the log as an owned table in the same transaction, `Version` as the concurrency token. The dispatcher is
in-process; a message bus would replace it without the handlers changing, and "save, then dispatch" is where an
outbox would go.

**Decisions worth a sentence.** Structured input (`face`, `rotation`) is the API contract; notation is for the console
and tests. Preview is a query: `Turn` without saving. `Move` is a `readonly record struct` (a small value used as a
key); `Cube` is a class (large, shared by reference); `CubeSession` is an entity. SQLite because it is a real
relational database with no install; tests use in-memory SQLite through the same migration, not the EF InMemory
provider. No MediatR, FluentAssertions or AutoMapper: all three went commercial, and the replacements are a few dozen
lines. Nullable, warnings as errors, latest recommended analyzers, central package versions.

**Deliberately not here.** Whole-cube rotations and slice moves (`Move` would gain a layer depth; the table would not
change). Other cube sizes in the API and UI (the model supports them; the brief is 3x3). Users and authentication
(nothing in the brief needs them; "who" would be one more column on the log). Brokers, outbox, microservices (see
above for where they would go).
