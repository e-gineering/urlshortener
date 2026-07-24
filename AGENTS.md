# AGENTS.md

Guidance for AI coding agents working in this repository.

## Style rules

- Do not use unicode characters such as em dashes, en dashes, or the ellipsis character. Use a plain hyphen or spell things out instead.

## Project overview

.NET 8 minimal API URL shortener with QR code generation, backed by Azure Table Storage. See README.md for setup and running instructions.

## Structure

- Egineering.UrlShortener - main web app; Program.cs holds the minimal API endpoints
- Egineering.UrlShortener.Services - business logic, Azure Table Storage service, QR code service
- wwwroot - static frontend (HTML, CSS, JS)

## Conventions

- Write endpoints (POST/PUT/PATCH under /api) are protected by a shared security token checked in ValidateAuthHeader in Program.cs, not a full auth framework.
- Keep endpoint handlers in Program.cs as minimal APIs; put business logic in the Services project.
