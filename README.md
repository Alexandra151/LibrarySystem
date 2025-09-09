# LibrarySystem

Projekt zaliczeniowy (.NET 8) w architekturze **Clean Architecture** z trzema modułami Presentation:
- **REST API** (ASP.NET Core Web API): CRUD na `Authors`, `Books`, `Loans`; middleware nagłówków; **JWT**; EF **InMemory**; **Swagger**; **Configuration**.
- **GraphQL** (HotChocolate): query `books`, `authors`, `loans`.
- **Razor Pages** (panel administracyjny): szybkie linki do Swagger/GraphQL/REST, mini-klient Authors (REST) i Books (GraphQL), logowanie (cookie) i proxy JWT.

## Struktura rozwiązań
- `LibrarySystem.Domain` – encje: `Author`, `Book`, `Loan`.
- `LibrarySystem.Application` – (miejsce na logikę aplikacyjną).
- `LibrarySystem.Infrastructure` – EF InMemory, `LibraryDbContext`, DI.
- `LibrarySystem.Presentation` – **REST API + GraphQL**, JWT, middleware, Swagger.
- `LibrarySystem.Web` – **Razor Pages** (panel admin, login + cookie, delegating handler z Bearer).
- `LibrarySystem.Tests` – testy integracyjne (xUnit + `WebApplicationFactory`).

## Wymagania uruchomieniowe
- .NET SDK 8.x  
- Windows/WSL/macOS (w projekcie używam certyfikatu dev HTTPS).

## Uruchomienie

```powershell
# 1) API (Swagger + GraphQL)
dotnet run --project .\LibrarySystem.Presentation\LibrarySystem.Presentation.csproj
# Swagger: https://localhost:7182/swagger
# GraphQL: https://localhost:7182/graphql
# Uwaga: dla /api/* wymagany nagłówek:  X-Client-Name: smoke

# 2) Panel Razor Pages (admin)
dotnet run --project .\LibrarySystem.Web\LibrarySystem.Web.csproj
# Panel: https://localhost:7113

# 3) Testy
dotnet test

Loginy demo
admin / admin123 (Admin)
librarian / librarian123 (Librarian)

Smoke script
Uruchom w katalogu repo:
powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\curl-smoke.ps1
