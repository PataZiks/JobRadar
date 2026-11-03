# JobRadar

![CI](https://github.com/PataZiks/JobRadar/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

A personal job-finding automation tool built with C# / .NET 10.

> Automatically fetches, filters, scores, and reports new job listings — so you focus on applying, not searching.

---

## Features

- Fetches job listings from the **Jooble API** (configurable search terms & locations)
- **Filters** by seniority, role, and keywords — configurable, not hardcoded
- **Scores** each listing transparently: `+3 Junior in title, +2 C#, −1 salary below target`
- **Detects duplicates** across sources and agencies
- Remembers seen listings via **SQLite** — shows only new ones each run
- Generates a styled **HTML report** for every run
- Sends a **daily email** with new listings sorted by score (skipped if nothing new)
- Runs automatically via **GitHub Actions** Mon–Fri at 07:00
- API key usage tracked per key-hash — never logs the raw key
- Web dashboard (coming in v8)

---

## Architecture

```mermaid
flowchart LR
    subgraph Core ["JobRadar.Core (no external deps)"]
        M[JobPost] --> F[JobFilter]
        M --> S[JobScorer]
        M --> D[DuplicateDetector]
    end

    subgraph Cli ["JobRadar.Cli"]
        direction TB
        SRC["IJobSource\n(Fake / Jooble)"] --> R[JobRunner]
        F --> R
        S --> R
        D --> R
        DB[(SQLite)] --> R
        R --> REP[HtmlReportWriter]
        R --> MAIL[EmailNotifier\n(MailKit)]
    end

    CFG[appsettings.json] -.-> Cli
    ENV[.env / GitHub Secrets] -.-> Cli
```

---

## Tech Stack

| Concern        | Technology                         |
|----------------|------------------------------------|
| Language       | C# / .NET 10                       |
| Architecture   | Generic Host, Dependency Injection |
| Database       | SQLite + Dapper                    |
| Email          | MailKit (SMTP / Gmail)             |
| Tests          | xUnit (19 unit tests)              |
| CI/CD          | GitHub Actions                     |
| Secrets        | .env (local) + GitHub Secrets (CI) |

---

## Getting Started

```bash
# Clone
git clone https://github.com/PataZiks/JobRadar.git
cd JobRadar

# Configure secrets (never commit .env)
cp .env.example .env
# Edit .env with your actual values

# Build
dotnet build

# Run with fake data (no API quota consumed)
dotnet run --project src/JobRadar.Cli

# Run with real Jooble API (requires JOOBLE_API_KEY in .env)
dotnet run --project src/JobRadar.Cli -- --source=jooble

# Show all jobs (ignore seen-DB)
dotnet run --project src/JobRadar.Cli -- --all

# Dry run (no output written, no email sent)
dotnet run --project src/JobRadar.Cli -- --dry-run

# Test API key (costs 1 request)
dotnet run --project src/JobRadar.Cli -- check-key
```

---

## Configuration

All search terms, filter rules, scoring weights, locations, and salary targets live in
`src/JobRadar.Cli/appsettings.json` — not in code.

Sensitive values (API keys, email credentials) go in `.env` only — never in source.

See [docs/ROTATE_KEY.md](docs/ROTATE_KEY.md) for instructions on rotating the Jooble key.

---

## Project Structure

```
src/
  JobRadar.Core/          Domain models, filtering, scoring, deduplication (no external deps)
  JobRadar.Cli/           Console app: host, DI, sources, storage, reporting, email
tests/
  JobRadar.Tests/         19 xUnit unit tests (filter, scorer, deduplication)
fixtures/                 Sample Jooble API responses for offline development
docs/                     Architecture overview, learning log, key rotation guide
.github/workflows/        CI (on push) + daily scheduled run (Mon–Fri)
```

---

## Roadmap

- [x] v0 – Project setup, .gitignore, secrets baseline, gitleaks hook
- [x] v1 – JobPost model, IJobSource interface, FakeJobSource, console output
- [x] v2 – Configurable filter, scorer, deduplication, 19 unit tests, Generic Host + DI
- [x] v3 – JoobleJobSource (HttpClient), request counter per key-hash, `check-key`, .env loading
- [x] v4 – SQLite memory via Dapper — only new jobs shown per run
- [x] v5 – HTML report generation, CLI flags (`--all`, `--dry-run`, `--source`)
- [x] v6 – Email via Gmail SMTP (MailKit), skipped when no credentials configured
- [x] v7 – GitHub Actions: CI on push + daily scheduled run (Mon–Fri)
- [ ] v8 – Web dashboard (ASP.NET Core Minimal API + simple frontend)
- [ ] v9 – Screenshots, extended docs

---

## License

MIT — see [LICENSE](LICENSE)
