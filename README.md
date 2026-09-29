# Pivot

**We turn things around.** Pivot is a pretend startup with one product: a cube you can turn, where every turn goes
on the record. Under the name it is a Rubik's cube simulator: it starts solved, oriented like
[rubiks-cube-solver.com](https://rubiks-cube-solver.com/) (green front, red right, white up), and can turn any face.

It has three ways in: a console app that prints the result the brief asks for, a Web API, and a React front end.

## Prerequisites

| Tool | Version | Needed for |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 | Everything .NET: console, API, tests |
| [Node.js](https://nodejs.org/) | 22 or 24 LTS (20.19 or later works) | The web front end only |
| [Docker](https://docs.docker.com/get-docker/) | Any recent version with Compose | Optional |

Everything is free. The commands below work in PowerShell, Command Prompt, macOS and Linux shells, run from the
repository root. Check the tools with `dotnet --version` and `node --version`.

## Build and run

1. Get the code and build it:

   ```bash
   git clone https://github.com/gdobrilov/Pivot.git
   cd Pivot
   dotnet build
   ```

2. Run the console app. With no arguments it applies the sequence from the brief, F R' U B' L D':

   ```bash
   dotnet run --project src/RubiksCube.Console
   ```

   It prints the solved cube and then the final state, which matches the picture in the brief sticker for sticker:

   ```
   Final state after F R' U B' L D':
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

   Your own sequence (a face letter, `'` for anti-clockwise, `2` for 180°), and the cube after every move:

   ```bash
   dotnet run --project src/RubiksCube.Console -- "R U R' U'"
   dotnet run --project src/RubiksCube.Console -- --steps
   ```

   Invalid input prints what was wrong and exits with code 1.

3. Run the tests:

   ```bash
   dotnet test
   ```

   That covers all the .NET projects. The front end has its own tests (step 5).

4. Start the API. It listens on <http://localhost:5080> and creates and migrates a SQLite file, `rubiks.db`, on
   first start:

   ```bash
   dotnet run --project src/RubiksCube.Api
   ```

   With `dotnet run` it runs in Development, so API docs are at <http://localhost:5080/scalar>. `/health` checks the
   database.

5. In a second terminal, start the front end:

   ```bash
   cd src/web
   npm install
   npm run dev
   ```

   Open <http://localhost:5173>. Vite forwards `/api` to the API on port 5080. Click a face, hover a rotation to see
   which stickers would change, click to turn it. The page remembers its cube, so a reload picks up where you left off.
   `npm test`, `npm run lint`, `npm run typecheck` and `npm run build` are there too.

Visual Studio or Rider: open `RubiksCube.sln` and run `RubiksCube.Console` or `RubiksCube.Api`.

## Docker (optional)

Nothing above needs Docker. If you prefer containers:

```bash
docker compose up --build
```

That starts the web front end on <http://localhost:3000> and the API on <http://localhost:8080> (docs at `/scalar`).
The SQLite file lives in a named volume, so it survives restarts; `docker compose down -v` deletes it.

The console app on its own:

```bash
docker compose run --rm console
docker compose run --rm console "R U R' U'"
```

## API

| Method | Route | What it does |
|---|---|---|
| `POST` | `/api/cubes` | Creates a solved cube. `201` with a `Location` header. |
| `GET` | `/api/cubes/{id}` | The current state. |
| `GET` | `/api/cubes/{id}/preview?face=Right&rotation=AntiClockwise` | What a turn would do. Nothing is saved. |
| `POST` | `/api/cubes/{id}/rotations` | Turns a face. Body: `{ "face": "Front", "rotation": "Clockwise" }`. |
| `GET` | `/api/cubes/{id}/rotations` | The log: every turn, undo and reset, with its time in UTC. |
| `POST` | `/api/cubes/{id}/undo` | Undoes the last turn. The undo is logged; nothing is deleted. |
| `POST` | `/api/cubes/{id}/reset` | Back to solved. The log stays. |
| `GET` | `/api/cubes/{id}/net` | The net as plain text, the same as the console prints. |

Faces are `Up`, `Left`, `Front`, `Right`, `Back`, `Down`; rotations are `Clockwise`, `AntiClockwise`, `Half`. Errors
come back as RFC 9457 problem details: `400` for missing or invalid input, `404` for an unknown cube, `409` when there
is nothing to undo or two requests change the same cube at the same moment.

## How it is built

```
 Console ──────────────► Application ──► Domain
 API (composition root) ─► Application
                        └► Infrastructure ──► Application
 web ── HTTP ──► API
```

**Domain** is plain C# with no dependencies. `Cube` is immutable: a turn returns a new cube. Each face is a
`FaceGrid` of rows and columns as seen from outside. A turn is a rearrangement (a permutation) of all the stickers,
worked out once per move from a table in `FaceGeometry`. For each face the table names the four strips of neighbouring
stickers that move with it, in words ("Up's bottom row, read from the bottom-left"), so there is no special case per
face. Anti-clockwise is the reverse rearrangement and 180° is the clockwise one applied twice.

`CubeSession` is a cube plus an append-only log of rotations, undos and resets. Undo applies the inverse of the last
move still in effect and adds a log line, so the log doubles as an audit trail. Each change raises an event.

**Application** holds the use cases, one handler per command or query, and the interfaces it needs
(`ICubeSessionRepository`, `IDomainEventDispatcher`, plus .NET's `TimeProvider`). Expected failures come back as a
`Result` rather than exceptions and become status codes in one place in the API. Command handlers save first and only
then pass the events on.

**Infrastructure** stores sessions with EF Core and SQLite: the cube as a string of colour letters, the log as its own
table written in the same transaction, and a version number that stops two simultaneous writes from both winning.
Events go to listeners in the same process; today the only one writes to the log file.

**Web** is React and TypeScript. The server is the only place that knows how a cube turns; the page asks it for the
state, for previews and for the log.

## Choices I would defend

- Structured input (`face`, `rotation`) is the API contract because that is what a user picks. Notation such as
  `F R' U2` is for the console and the tests.
- `Move` is a `readonly record struct`: small, no identity, used as a dictionary key. `Cube` is a class because it is
  larger and shared, and `CubeSession` is an entity.
- SQLite gives real relational storage with nothing to install. Tests run against in-memory SQLite built by the same
  migration, not EF's InMemory provider, which does not behave like a database.
- No MediatR, FluentAssertions or AutoMapper. All three now need a commercial licence, and what they would replace
  here is a few dozen lines.
- The build treats warnings as errors with the .NET 10 recommended analysers, and package versions live in one file.

## Left out on purpose

- Turning the whole cube and middle-slice moves. `Move` would gain a layer depth; the geometry table would not change.
- Other cube sizes in the API and UI. The domain has no hard-coded 3 and its tests turn 2x2 and 4x4 cubes, but the
  brief is about 3x3.
- Users, authentication and rate limiting. Nothing in the brief needs them; "who" would be one more column in the log.
- A message broker, an outbox or separate services. Events are dispatched after the save in the same process; that
  dispatch is where an outbox would go if they ever had to leave it.
- Migrating at startup suits a single instance. With several, the migration would move to the deployment pipeline.
