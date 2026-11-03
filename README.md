# JobRadar

A personal job-finding automation tool built with C# / .NET 10.

> Automatically fetches, filters, scores, and reports new job listings — so you focus on applying, not searching.

## Features

- Fetches job listings from the Jooble API
- Filters by location, role, and seniority (configurable)
- Scores each listing with a transparent explanation (e.g. "+3 Junior in title, +2 C#, −2 support")
- Detects duplicates across sources and agencies
- Sends a daily email with **new** listings only (sorted by score)
- Runs automatically via GitHub Actions (Mon–Fri)
- Web dashboard (coming in v8)

## Tech Stack

| Concern        | Technology                        |
|----------------|-----------------------------------|
| Language       | C# / .NET 10                      |
| Architecture   | Generic Host, Dependency Injection|
| Database       | SQLite (Dapper)                   |
| Email          | MailKit (SMTP)                    |
| Tests          | xUnit                             |
| CI/CD          | GitHub Actions                    |

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

# Run (uses FakeJobSource by default – no API quota consumed)
dotnet run --project src/JobRadar.Cli
```

## Configuration

All search terms, filter rules, scoring weights, locations, and salary targets live in
`src/JobRadar.Cli/appsettings.json` — not in code.

Sensitive values (API keys, email credentials) go in `.env` only.

## Project Structure

```
src/JobRadar.Core/     Domain models, filtering, scoring (no external dependencies)
src/JobRadar.Cli/      Console app: sources, storage, reporting, email
src/JobRadar.Web/      Web dashboard (v8)
tests/JobRadar.Tests/  xUnit unit tests
fixtures/              Sample API responses for offline development
docs/                  Architecture overview, learning log, key rotation guide
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full architecture overview.

## Roadmap

- [x] v0 – Project setup, secrets, CI skeleton
- [ ] v1 – First job fetch (FakeJobSource + fixtures)
- [ ] v2 – Filter, scoring, deduplication, unit tests
- [ ] v3 – Real Jooble API, request counter, `check-key` command
- [ ] v4 – SQLite memory (show new listings only)
- [ ] v5 – Markdown/HTML report, CLI options
- [ ] v6 – Email via Gmail SMTP (MailKit)
- [ ] v7 – GitHub Actions CI + daily scheduled run
- [ ] v8 – Web dashboard (ASP.NET Core)
- [ ] v9 – Portfolio polish, diagrams, README badges

## License

MIT — see [LICENSE](LICENSE)
