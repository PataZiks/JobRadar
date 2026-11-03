# CLAUDE.md – Project Rules for AI Assistants

Rules for AI assistants (Claude Code) working on this repository.

## Language
- Code, comments, commit messages, documentation: **English**
- Communication with the developer: **German**

## Workflow
1. Show a brief plan before implementing – wait for developer OK
2. Small steps: few files per step; run `dotnet build` and `dotnet test` after each step
3. Explain what was built and why (in German) after every step
4. Conventional Commits: `feat:`, `fix:`, `test:`, `docs:`, `chore:`
5. Never run `git push` without explicit developer approval

## Architecture
- `JobRadar.Core`: pure domain logic – zero external NuGet dependencies
- `JobRadar.Cli`: entry point, DI, infrastructure (sources, storage, reporting, notifications)
- All data sources implement `IJobSource`
- SQLite access via Dapper (no EF Core)
- Secrets via `.env` + DotNetEnv (not .NET User Secrets)

## API / Secrets Rules
- **Never** call real APIs (Jooble, email SMTP) without explicit developer permission
- Always use `FakeJobSource` with fixtures from `fixtures/` during development
- Never read, log, or print `.env` content or any secret value
- Never commit `.env`, `appsettings.Local.json`, `*.db`, or generated reports
- API key usage counter stored per key-hash only – never log the raw key

## Security
- gitleaks pre-commit hook must pass before every commit
- All sensitive config comes exclusively from environment variables
- Warn when Jooble request count approaches 90% of the configured limit (default: 500)
