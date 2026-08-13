# URL Shortener

[![CI](https://github.com/svasorcery/url-shortener-demo/actions/workflows/ci.yml/badge.svg)](https://github.com/svasorcery/url-shortener-demo/actions/workflows/ci.yml)

A compact URL-shortening API built to demonstrate clean backend fundamentals in a small codebase.

The service validates HTTP URLs, creates cryptographically random Base62 codes, persists mappings in PostgreSQL, caches resolved URLs in memory, and returns redirects through a minimal ASP.NET Core API.

## Stack

- .NET 8 and ASP.NET Core Minimal APIs
- PostgreSQL and Entity Framework Core
- FluentValidation
- In-memory caching
- xUnit v3, Testcontainers, and real PostgreSQL integration tests
- Docker Compose

## Run with Docker

Docker is the only prerequisite.

```bash
docker compose up --build
```

The API is available at `http://localhost:8080` and Swagger UI at `http://localhost:8080/swagger`.

Stop the stack and remove its local database volume:

```bash
docker compose down --volumes
```

## API

### Create a short URL

```http
POST /shorten
Content-Type: application/json

{
  "url": "https://example.com/articles/url-shortening"
}
```

Successful response:

```http
HTTP/1.1 201 Created
Location: http://localhost:8080/aB3dE9
Content-Type: application/json

{
  "code": "aB3dE9",
  "shortUrl": "http://localhost:8080/aB3dE9"
}
```

Only absolute HTTP and HTTPS URLs are accepted. Invalid input returns an RFC 7807 validation problem.

### Resolve a short URL

```http
GET /aB3dE9
```

The API returns `302 Found` with the original URL in the `Location` header. Unknown codes return `404 Not Found`.

## Test

The integration suite starts PostgreSQL through Testcontainers, applies the real EF Core migrations, and exercises the HTTP API and persistence layer together.

```bash
dotnet test UrlShortener.sln --configuration Release
```

Docker must be running for the integration tests.

## Design notes

- Six-character codes use the full Base62 alphabet and `RandomNumberGenerator`.
- A unique PostgreSQL index protects the code namespace.
- The stored model contains the code and original URL. The public short URL is derived at request time, so changing the host does not require a data migration.
- Successful resolutions are cached for one hour.
- EF Core migrations run when the application starts outside the test environment.
- CI builds with warnings as errors and runs the suite against PostgreSQL on every push and pull request.

## Scope

This repository stays deliberately focused on one service and one deployment unit. A larger production version would add concurrent collision retries, rate limiting, distributed caching, observability, expiration policies, custom aliases, analytics, and abuse protection.

That is where the enterprise spaceship begins. This version keeps the core readable.

## License

[MIT](LICENSE)
