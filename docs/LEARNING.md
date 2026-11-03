# Learning Log

Personal reference for concepts learned while building JobRadar.
Updated after each version milestone.

---

## v0 – Project Setup

### Solution vs. Project
A **.sln file** (Solution) groups multiple **.csproj files** (Projects).
One solution – three projects: Core, Cli, Tests. Each compiles independently.

### Generic Host
.NET's built-in host that wires up **Dependency Injection**, **Configuration** (appsettings.json,
environment variables), and **Logging** in one place – not just for web apps.

### Dependency Injection (DI)
Instead of writing `var service = new MyService()` everywhere, you *register* classes in a
container (in `Program.cs`) and *request* them via constructor parameters.
The container handles creation and lifetime.
Why: loose coupling, easy testing, swap implementations without changing callers.

### Interface
A contract that defines *what* a type can do, without specifying *how*.
`IJobSource` says "anything that can provide a list of jobs."
Concrete implementations: `FakeJobSource`, `JoobleJobSource` (v3).
You can swap them without changing any other code – a key interview topic.

### Conventional Commits
A standard commit message format used in professional teams:
- `feat:` – new feature
- `fix:` – bug fix
- `chore:` – tooling/setup (no production code change)
- `docs:` – documentation only
- `test:` – tests only

### .gitignore
Tells Git which files to never track. Secrets, build output, and personal config go here.
Once a file is tracked, `.gitignore` no longer helps – so this file must exist *before* `git add`.

### .env
A plain-text file with `KEY=value` lines for local secrets (API keys, passwords).
Never committed. `.env.example` shows only the variable *names* – safe to commit.

### Dapper vs. EF Core
**Dapper**: micro-ORM. You write SQL yourself; it maps results to C# objects. Lightweight.
**EF Core**: full ORM. Generates SQL for you. More magic, more complexity.
For a Junior portfolio project: Dapper is the better choice – you see the SQL, can explain it.

---

## v1 – First Job Fetch

### Model-Klasse (`JobPost`)
Eine **Model-Klasse** (auch "POCO") hält nur Daten, keine Logik.
`init` bedeutet: die Property kann nur beim Erstellen gesetzt werden (`new JobPost { Title = "..." }`),
danach ist sie unveränderlich. Das verhindert versehentliche Änderungen nach dem Laden.
`set` (bei `Score`, `ScoreReason`) bleibt änderbar – der Scorer setzt diese Werte später.

### Interface (`IJobSource`)
Ein **Interface** ist ein Vertrag ohne Implementierung.
`IJobSource` sagt: "Jede Klasse, die dieses Interface implementiert, muss `Name` und `FetchAsync()` anbieten."
Vorteil: `Program.cs` kennt nur `IJobSource` – nicht ob dahinter `FakeJobSource` oder `JoobleJobSource` steckt.
Die Quelle ist **austauschbar** ohne Änderung im aufrufenden Code.

### `async`/`await`
`FetchAsync()` gibt `Task<IReadOnlyList<JobPost>>` zurück.
`Task<T>` = "ich liefere *irgendwann* einen Wert vom Typ T."
`await` pausiert die Methode bis der Task fertig ist – ohne den Thread zu blockieren.
Wichtig für Netzwerkaufrufe (Jooble API): das Programm friert nicht ein während es wartet.

### JSON-Deserialisierung (`System.Text.Json`)
`JsonSerializer.DeserializeAsync<T>()` liest einen JSON-Stream und baut ein C#-Objekt daraus.
`PropertyNameCaseInsensitive = true` → `"title"` im JSON passt zu `Title` in C#.
Die privaten DTOs (`JoobleResponse`, `JoobleJob`) sind nur fürs Parsing – niemand sieht sie von außen.

### `AppContext.BaseDirectory`
Gibt den Ordner der kompilierten `.exe` zurück (`bin/Debug/net10.0/`).
Die App findet die Fixture-Datei damit unabhängig davon, von wo aus man `dotnet run` aufruft.

### `IReadOnlyList<T>` statt `List<T>`
Der Aufrufer kann die Liste lesen aber nicht verändern.
Prinzip: "Gib nach außen so wenig Rechte wie nötig."

---
