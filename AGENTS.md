# Coding Agent Instructions

This file provides guidance to coding agents when working with code in this repository.

## Overview

Source code for [martincostello.com](https://martincostello.com/) — an ASP.NET Core personal website published as a Native AOT container.

The solution targets the version of .NET specified in the global.json file and uses Central Package Management.

## Commands

### Build and Test

```powershell
# Full build and test
./build.ps1

# Build only (skip tests)
./build.ps1 -SkipTests

# Run all .NET tests
dotnet test --configuration Release

# Run a specific test project
dotnet test tests/Website.Tests/Website.Tests.csproj --configuration Release

# Run a single test by name
dotnet test tests/Website.Tests/Website.Tests.csproj --configuration Release --filter "FullyQualifiedName~TestName"

# Run .NET app locally
dotnet run --project src/Website/Website.csproj

# Run with Aspire AppHost (orchestrated)
dotnet run --project src/Website.AppHost/Website.AppHost.csproj
```

### Frontend (TypeScript/CSS)

All npm commands run from `src/Website/`:

```bash
npm run build        # compile + format + lint (full build)
npm run compile      # webpack bundle only
npm run lint         # ESLint on TypeScript
npm run format       # prettier + stylelint with auto-fix
npm run format-check # check formatting without fixing
npm run test         # vitest (TypeScript unit tests)
npm run watch        # webpack watch mode
```

## Architecture

### Project Structure

- `src/Website/` — Main ASP.NET Core web app (AOT-compiled)
- `src/Website.AppHost/` — .NET Aspire orchestration host
- `tests/Website.Tests/` — Unit and integration tests using `Microsoft.AspNetCore.Mvc.Testing`
- `tests/Website.EndToEndTests/` — Playwright-based end-to-end tests against a running website by its URL (requires `WEBSITE_URL` environment variable)
- `tests/Website.Tests.Shared/` — Shared test infrastructure (compiled into both test projects)

### Application Layers

**Entry point**: `Program.cs` calls `builder.AddWebsite()` and `app.UseWebsite()` — both defined as extension methods in `WebsiteBuilder.cs`.

**Routing**: All routes are mapped directly in `WebsiteBuilder.UseWebsite()`. Pages use [RazorSlices](https://github.com/DamianEdwards/RazorSlices) (`.cshtml` files in `src/Website/Slices/`) rendered via `Results.RazorSlice<T>()`. There is no MVC controller layer.

**Redirects**: `RedirectsModule.cs` maps shortlink routes (e.g. `/gh` → GitHub) and crawler-trap paths (from `SiteOptions.CrawlerPaths`) that redirect to random YouTube videos.

**Configuration**: All site settings flow through `SiteOptions` (bound from `"Site"` config section). Key sub-options: `Analytics`, `ContentSecurityPolicyOrigins`, `ExternalLinks`, `Metadata`, `CrawlerPaths`.

**Security headers**: `CustomHttpHeadersMiddleware` sets CSP, HSTS headers, and other security headers on every response. CSP is built at startup from `SiteOptions`.

**Telemetry**: OpenTelemetry (OTLP exporter) + Sentry. Configured in `Extensions/TelemetryExtensions.cs`.

**Frontend assets**: TypeScript in `assets/scripts/`, CSS in `assets/styles/`. Webpack bundles them into `wwwroot/assets/`. The build only runs webpack if `wwwroot/assets/js/main.js` doesn't exist yet.

### Testing

- **Integration tests** (`Website.Tests/Integration/`) use `TestServerFixture` (extends `WebApplicationFactory<MetaModel>`) with `testsettings.json` overrides. Tests inherit from `IntegrationTest` base class.
- **E2E tests** (`Website.EndToEndTests/`) use Playwright and require `WEBSITE_URL` environment variable pointing to a running instance; tests are skipped if the variable is absent.
- Test assertions use **Shouldly**. Test framework is **xUnit v3**.

### Key Conventions

- Namespace root: `MartinCostello.Website`
- AOT-compatible JSON serialization: `ApplicationJsonSerializerContext` (source-generated, registered via `TypeInfoResolverChain`)
- Build artifacts go to `artifacts/` (UseArtifactsOutput is enabled)
- `GitMetadata.cs` is auto-generated at the solution root and included in all projects via `Directory.Build.props`
