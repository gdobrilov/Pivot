# Rubik's Cube Simulator — TTC Software Developer Challenge

A programmatic 3x3x3 Rubik's cube that can turn any face, with:

* a **console app** that prints the exploded view required by the brief (the challenge deliverable),
* a **REST API** (ASP.NET Core, EF Core + SQLite) where each cube is a session with an audit log, undo and a "preview this turn" query,
* a **React front end** where you click a face on the net, pick 90° / -90° / 180°, see which stickers will move before you commit, and watch the activity log fill up.

The cube starts solved and oriented like [rubiks-cube-solver.com](https://rubiks-cube-solver.com/):
**green front, red right, white up** (so orange left, blue back, yellow down).

```
       W W W
       W W W
       W W W
O O O  G G G  R R R  B B B
O O O  G G G  R R R  B B B
O O O  G G G  R R R  B B B
       Y Y Y
       Y Y Y
       Y Y Y
```

## Quick start (60 seconds)

Requires only the free [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0). From the repository root:

```bash
dotnet run --project src/RubiksCube.Console
```

This prints the solved cube, applies the challenge sequence **F R' U B' L D'** and prints the result:

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

This matches the exploded view in the brief tile for tile. To run all tests:

```bash
dotnet test
```

## What is in the box

| Project | Purpose |
|---|---|
| `src/RubiksCube.Domain` | The cube itself: colours, faces, moves, the geometry of a turn, and the `CubeSession` aggregate with its log. No dependencies. |
| `src/RubiksCube.Application` | Use cases as commands and queries with one handler each; the ports they need (repository, clock, event dispatcher); read models. |
| `src/RubiksCube.Infrastructure` | Adapters: EF Core + SQLite persistence with a migration, the system clock, the in-process event dispatcher and a logging listener. |
| `src/RubiksCube.Console` | **The challenge deliverable.** Domain + renderer only; prints the exploded view before and after a move sequence. |
| `src/RubiksCube.Api` | ASP.NET Core Web API (controllers) exposing sessions over HTTP, with OpenAPI docs and problem-details errors. |
| `src/web` | React + TypeScript (Vite) UI: click a face, pick a rotation, preview, undo, activity log. |
| `tests/*` | xUnit tests per .NET layer (domain, handlers, EF mapping against SQLite, HTTP end-to-end) and Vitest for the UI. |

Everything is free and open source: .NET SDK, Node.js, SQLite, xUnit, Vite, Vitest. No paid tools are needed to build, run or test.

## Prerequisites

| Tool | Version | Needed for |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | 9.0 or later | Console, API, .NET tests |
| [Node.js](https://nodejs.org/) | 20 LTS or later (ships with npm) | React front end |
| [Docker](https://docs.docker.com/get-docker/) | any recent version with Compose v2 | Optional: containerised run |

SQLite needs no installation; the API creates `rubiks.db` next to itself on first start and applies the migration automatically.

All commands work in Windows PowerShell / Command Prompt, macOS and Linux shells. Run them from the repository root unless stated otherwise.

## Building and running

### 1. Console (the challenge output)

```bash
dotnet run --project src/RubiksCube.Console
```

Options:

```bash
# Print the cube after every single move, with a plain-English description
dotnet run --project src/RubiksCube.Console -- --steps

# Apply your own sequence (Singmaster notation: U L F R B D, ' = anti-clockwise, 2 = 180°)
dotnet run --project src/RubiksCube.Console -- "R U R' U'"
```

An invalid move (for example `X`) prints an explanatory error and exits with code 1.

To produce a standalone executable instead of using `dotnet run`:

```bash
dotnet publish src/RubiksCube.Console -c Release -o out
out/rubiks            # Windows: out\rubiks.exe
```

### 2. API

```bash
dotnet run --project src/RubiksCube.Api
```

The API listens on <http://localhost:5000>. With `dotnet run` the environment is Development, so interactive API
documentation is available at <http://localhost:5000/scalar> and the OpenAPI document at `/openapi/v1.json`.
Health check (includes the database): `/health`.

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/cubes` | Create a solved cube session. `201` with the state and a `Location` header. |
| `GET` | `/api/cubes/{id}` | Current state of a session. |
| `GET` | `/api/cubes/{id}/preview?face=Right&rotation=AntiClockwise` | What the turn would do: the resulting faces and every sticker that changes. Nothing is stored. |
| `POST` | `/api/cubes/{id}/rotations` | Body `{ "face": "Front", "rotation": "Clockwise" }`. Turns the face. Rotation is `Clockwise`, `AntiClockwise` or `Half`. |
| `GET` | `/api/cubes/{id}/rotations` | The audit log: every rotation, undo and reset with its sequence number and UTC time. |
| `POST` | `/api/cubes/{id}/rotations/undo` | Reverses the most recent effective rotation (recorded as an entry, nothing is deleted). |
| `POST` | `/api/cubes/{id}/reset` | Back to solved; the log is kept. |
| `GET` | `/api/cubes/{id}/net` | The exploded view as `text/plain`, same renderer as the console. |

Errors follow [RFC 9457 problem details](https://www.rfc-editor.org/rfc/rfc9457): `400` for invalid input or "nothing
to undo", `404` for an unknown session, `409` if two requests changed the same session at once.

Example with curl:

```bash
curl -s -X POST http://localhost:5000/api/cubes
curl -s -X POST http://localhost:5000/api/cubes/<id>/rotations -H "Content-Type: application/json" -d "{\"face\":\"Front\",\"rotation\":\"Clockwise\"}"
curl -s http://localhost:5000/api/cubes/<id>/net
```

State shape:

```json
{
  "id": "0d4b…",
  "isSolved": false,
  "version": 2,
  "canUndo": true,
  "effectiveMoves": ["F", "R'"],
  "faces": { "up": ["White", "…9 colour names, row by row"], "left": ["…"], "front": ["…"], "right": ["…"], "back": ["…"], "down": ["…"] }
}
```

### 3. Web front end

Start the API first (section 2), then in a second terminal:

```bash
cd src/web
npm install
npm run dev
```

Open <http://localhost:5173>. The dev server proxies `/api` to `http://localhost:5000`.

How it works: click a face on the net to select it; three buttons appear (90° clockwise, 90° anti-clockwise, 180°).
Hovering a button asks the API for a preview and marks every sticker that would change, already showing its new
colour. Clicking applies the turn. Undo, Reset and "Run challenge sequence" are in the toolbar; the activity log
lists everything that happened with its time.

Other scripts: `npm test` (Vitest + Testing Library), `npm run lint`, `npm run typecheck`, `npm run build`.

### 4. Docker (optional)

Docker is a convenience, not a requirement. Everything above runs without it.

```bash
# API on http://localhost:8080 and web UI on http://localhost:3000; the SQLite file lives in a named volume
docker compose up --build

# Just the challenge output, in a container
docker compose run --rm console
docker compose run --rm console "R U R' U'"
```

The images are multi-stage builds; the .NET containers run as a non-root user. The web image is nginx serving the
built SPA and proxying `/api` to the API container.

## Tests

```bash
dotnet test                 # all .NET tests
cd src/web && npm test      # front-end tests
```

| Suite | What it proves |
|---|---|
| `RubiksCube.Domain.Tests` | The exact result of every single turn (cross-checked with rubiks-cube-solver.com) and of the challenge sequence; the algebra (four turns = identity, `X X'` = identity, `X'` = `X X X`, `X2` = `X X`, sexy move x6, centres fixed, sticker counts); the reading-direction rule of the geometry table; the model is not tied to 3x3; notation; `CubeSession` log, undo and reset semantics. |
| `RubiksCube.Application.Tests` | Each handler with test doubles: create, rotate, undo, reset, preview, log; validation, not-found and conflict paths; events are dispatched only after a successful save. |
| `RubiksCube.Infrastructure.Tests` | The real EF Core mapping against in-memory SQLite created by the migration: round trip of a session and its log, optimistic concurrency conflict between two contexts, what the columns actually contain. |
| `RubiksCube.Api.Tests` | HTTP end to end in-process (`WebApplicationFactory`) on the real stack: statuses, `Location`, enum names in JSON, the challenge sequence, preview, undo/reset in the log, problem details for 400/404, plain-text net, health. |
| `src/web` (Vitest) | Net rendering and face selection, preview highlighting, the rotation picker, the activity log, the API client, the session hook. |

CI (`.github/workflows/ci.yml`) runs the .NET build and tests on **Windows and Linux**, the web lint/typecheck/test/build,
and finally builds the Docker images and runs the console container.

## Design

### Architecture

Clean Architecture with the dependency rule pointing inwards, use cases as commands/queries with one handler each,
and in-process domain events:

```
 Console ─┐                                   ┌─ Infrastructure (EF Core/SQLite, clock, event dispatcher)
          ├─► Application ──► Domain ◄────────┘
     API ─┘   (commands, queries,
              ports, read models)
     web (React) talks to the API over HTTP only
```

* **Domain** has no dependencies and no I/O: `Cube` (immutable value), the geometry of a turn, and the `CubeSession`
  aggregate that owns the append-only log. It is the part worth testing most, so it is the easiest to test.
* **Application** defines each use case as a command (`RotateFace`, `UndoRotation`, `ResetCube`, `CreateCube`) or
  query (`GetCube`, `PreviewRotation`, `GetRotationLog`) with exactly one handler, plus the ports it needs:
  `ICubeSessionRepository`, `IClock`, `IDomainEventDispatcher`. Expected failures come back as a `Result` with a typed
  error; nothing throws for control flow. The command handlers share one rule, in one place: **save first, then
  dispatch events**.
* **Infrastructure** implements the ports. The aggregate is mapped without touching the domain (Fluent configuration
  only): the cube as a facelet string, the log as an owned table written in the same transaction, `Version` as an
  optimistic concurrency token. The event dispatcher calls listeners synchronously in-process.
* **API** and **Console** are thin. The API is one controller whose actions build a command or query, hand it to the
  handler injected for that action, and translate the `Result` into a status code in one shared base method.
  `[ApiController]` turns model-binding failures (an unknown face name, malformed JSON) into 400 problem details
  before any handler runs. The console does not even use DI: it calls the domain and the renderer directly, which
  keeps the challenge deliverable trivially readable.

### The cube model (how a turn works)

Each face is a `FaceGrid`: N×N stickers seen from outside the cube, oriented as in the net (row 0 top, column 0 left).
A turn of a face does two independent things: its own grid rotates, and four strips of stickers on the neighbouring
faces move round one place. Both are expressed as a single **permutation** of the flat sticker array, built once per
move from the geometry table and cached; applying a move is one loop. Anti-clockwise is the inverse permutation,
180° is the clockwise one applied twice.

The geometry table in `FaceGeometry` is the only place that knows how faces are glued together, and it is written in
words rather than indices. For Front:

```
Up.Bottom (read from bottom-left) → Right.Left (from top-left) → Down.Top (from top-right) → Left.Right (from bottom-right) → back to Up.Bottom
```

Each strip is read from the corner met first when travelling clockwise around the turning face, so sticker k of one
strip lands on sticker k of the next with no special cases. Back is not special either: it is simply drawn as seen from
behind. The rule is tested directly, and every single turn is pinned against the solver website.

### Sessions, audit log and undo

`CubeSession` keeps the cube and an append-only log of entries: `Rotation` (the move made), `Undo` (the move undone) and
`Reset`. The effective moves are derived from the log, so undo is "apply the inverse of the last effective move" and
never deletes anything: the log is the audit trail. Every change raises a domain event (`CubeRotated`,
`RotationUndone`, `CubeReset`) that is dispatched after the change has been saved.

The log rows are written in the same transaction as the state, which is why there is no outbox: state and audit cannot
disagree. If events ever needed to leave the process (a message bus), the dispatcher is the seam: the handler code
above it would not change, and the "save, then dispatch" step is where an outbox would slot in.

### Key decisions, briefly

* **Immutable `Cube`, value equality.** Safe to share, trivially comparable, impossible to observe half-turned.
* **`Move` is a `readonly record struct`.** A tiny value with no identity, used as a dictionary key; `Cube` and
  `CubeSession` are classes because one is large and shared by reference and the other is an entity with identity.
* **Structured input, not notation, at the API boundary.** A user picks a face and how far to turn it; that is the
  contract (`face`, `rotation`). Notation (`F R' U2`) is kept for the console and for tests.
* **Preview is a query.** "What would this turn do" is `Cube.Turn` without saving; it costs nothing and gives the UI
  its most useful feature.
* **EF Core with SQLite.** Real relational persistence, a committed migration applied at startup, and a concurrency
  token, without asking reviewers to install a database. Tests use in-memory SQLite created by the same migration, not
  the EF InMemory provider (which does not enforce relational behaviour).
* **No MediatR, no FluentAssertions, no AutoMapper.** All three moved to commercial licences; the handler interfaces
  and the dispatcher are a few dozen lines of plain code.
* **Strict compiler settings.** Nullable, warnings as errors, latest recommended analyzers, central package versions.

### What is deliberately not here

* Whole-cube rotations, slice and wide moves. `Face` is a position in space; `Move` would gain a layer depth and the
  geometry table would not change.
* Exposing other cube sizes. The model has no hard-coded 3 (`Cube.Solved(2)` works and is tested), but the brief is 3x3,
  so the API and UI do not offer a size.
* Users and authentication. Nothing in the brief needs them; the log records what and when. "Who" would come from an
  identity layer and be one more column on the log entry.
* Message brokers, outbox, microservices. See "Sessions, audit log and undo" for where they would go.
