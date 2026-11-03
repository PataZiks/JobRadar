# Architecture

## Overview

JobRadar is a .NET 10 console application that automatically fetches, filters, scores,
and reports job listings from external APIs.

## Project Layout

    src/JobRadar.Core/     Domain models, filtering, scoring, deduplication (no external deps)
    src/JobRadar.Cli/      Entry point, Generic Host, DI, sources, storage, reporting, email
    src/JobRadar.Web/      Web dashboard (planned for v8)
    tests/JobRadar.Tests/  xUnit tests for Core logic
    fixtures/              Sample API responses for offline development

## Data Flow

    Load config
        → Fetch jobs via IJobSource (FakeJobSource / JoobleJobSource)
        → Filter (exclude Senior/Praktikum/etc.)
        → Score (Junior title, skills, salary, home-office, ...)
        → Deduplicate (same job via multiple sources/agencies)
        → Check against SQLite (show new listings only)
        → Generate Markdown/HTML report
        → Send email (if new listings exist)
        → Update SQLite

## Key Decisions

| Concern        | Choice              | Reason                                      |
|----------------|---------------------|---------------------------------------------|
| DB access      | Dapper              | Lightweight; developer learns real SQL      |
| Secrets        | .env + DotNetEnv    | Simple; matches real-world practice         |
| Email          | MailKit             | Active maintenance; full RFC compliance     |
| Source abstraction | IJobSource      | Swap sources without touching Core logic    |
| Test fixtures  | JSON files          | Offline dev; no API quota consumed          |
