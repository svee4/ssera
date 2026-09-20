# Blazor migration: .NET 10 upgrade + feature parity plan

Branch: `blazor` · Baseline verified 2026-09-21 · `dotnet build src/Ssera.sln` currently succeeds (0 errors, 43 warnings).

Goal: upgrade the platform to .NET 10 and bring the Blazor WASM client to feature parity with the
SvelteKit client that lives on `main`. The only additive feature in scope is finishing the
include/exclude tag filtering the branch already started (D1); everything else is parity.

## Agent boundaries (hard rules)

The implementing agent must never:

- access, read, copy, modify, or delete production data of any kind (the deployed `sqlite.db`, any
  server filesystem, any remote host);
- read, set, request, or invent secrets or credentials (Google API keys, user-secrets, `.env` files,
  registry credentials, reverse-proxy config);
- trigger deployment workflows, push images, or run anything against `ssera.estral.fi`, GHCR, or
  other production infrastructure;
- enable, modify, or configure the Google ingestion workers. They are commented out in `Program.cs`
  for development (D5) and stay that way until the user says otherwise. Never supply or run them
  with credentials.

Production data, secrets, server config, and deployment are handled by the user. Where a step needs
any of these, the plan marks it as **user action** and the agent only documents the requirement.

Local verification uses the API's own auto-created, gitignored `src/Ssera.Api/sqlite.db` (empty on
first run). If richer data is needed for testing, the user supplies a sanitized local copy or seeds
it themselves; the agent does not obtain data from production.

## How to work this plan

- Phases run in order. Do not skip or merge phases.
- Every step lists **Files**, **Work** and **Verify**. A step is done when its verify passes.
- Every phase ends with a **CHECKPOINT**. Stop there, report what changed, and wait for the user's
  confirmation before continuing. The user reviews/tests at each checkpoint.
- Keep each step as one small, reviewable change set. Do not reformat unrelated code.
- Build commands (from repo root):
  - `dotnet build src/Ssera.sln` (API + Shared + Blazor client; no Node needed anymore)
  - `dotnet run --project src/Ssera.Api/Ssera.Api.csproj` (https://localhost:7224, docs at `/scalar/v1`)
  - `dotnet run --project src/Ssera.Client/Ssera.Client.csproj` (http://localhost:5062)
- Client API base URL currently lives in `src/Ssera.Client/wwwroot/appsettings.json` (`SseraApiUrl`,
  default `https://localhost:7224`). The API enables `AllowAnyOrigin` CORS in Development.
- The dev API creates an empty, auto-migrated `sqlite.db`. Meaningful gallery/events testing needs
  data: **user action** — the user seeds or supplies a local test DB. The agent never touches
  production data or runs workers with real credentials (see Agent boundaries).
- Accepted differences (confirmed, not bugs): Radzen theme switcher (new), JSON URL state (D2),
  Blazor WASM initial load is heavier than the Svelte SSR-less SPA.

## Resolved decisions (user, 2026-09-21)

- **D1 — Tag filtering → finish it.** Add `GET /api/images/tags` and make the Include/Exclude
  dropdown functional. This is an explicitly approved additive feature (the API side already exists;
  the UI is half-built).
- **D2 — URL state → keep the blob.** Gallery keeps `?filters={json}&page=N`; reuse the same
  mechanism for events.
- **D3 — Client hosting → deferred**, not part of this migration. The deployment/hosting stage comes
  after. No client Dockerfile or hosting work in this plan.
- **D4 — Page size → fixed dropdown.** Keep `[50, 100, 500, 1000]`; do not switch to a number input.
- **D5 — Workers → leave commented out.** They stay disabled for development until the user says
  otherwise. Do not enable, modify, or configure them.
- **D6 — Production client API URL → deferred**, part of the post-migration hosting stage.
- **Upgrade fallback:** if a package upgrade (especially Immediate.* or Radzen) requires invasive
  changes, stop and report the options to the user; do not pin or downgrade unilaterally.
- **Accepted differences → confirmed:** theme switcher, JSON URL state, heavier WASM initial load.

## Baseline parity audit (what this plan fixes)

| Svelte (`main`) feature | Blazor `blazor` branch state | Fixed in |
| --- | --- | --- |
| Home at `/` | Routed at `/asdasd`, leftover Test button/HttpClient call | Phase 2 |
| Nav: Home/Events/Gallery/History | Nav points `/`, `/events`, `/gallery`, `/log`; none correct | Phase 2, 6, 7 |
| Events page (`/events`) | Missing | Phase 6 |
| History page (`/history`) | Missing | Phase 7 |
| Gallery filters (orderBy, sort, tag search, page size, eras, members, apply + dirty) | Partial; tag search replaced by the dropdown (D1); page size dropdown; no dirty indicator | Phase 5 |
| Gallery results list (masonry, date, era + tags, links, mobile 2-col) | Works; era not shown; no mobile breakpoint | Phase 5 |
| Gallery paging (top/bottom, page input, X–Y of Z, page reset) | Missing, always page 1 | Phase 5 |
| Gallery error UI (status, detail, trace ids) | Missing, unhandled exception | Phase 4/5 |
| Tag filtering server side | Svelte `tagSearch` removed; new tags endpoint + Include/Exclude instead (D1) | Phase 3 + 5 |
| `Hot` era | Removed from shared enum/mappings while DB still has it | Phase 3 |
| URL state (shared links, reload, back/forward) | JSON blob instead of individual params (D2, accepted) | Phase 5/6 |
| Client Docker image | `Dockerfile` deleted, workflow still references it | Deferred (post-migration hosting) |
| Both ingestion workers | Commented out in `Program.cs` for development | No change (D5) |
| Debug leftovers (2 s delay, console logs, test code) | Present | Phase 2/4/5 |

Out of scope: deployment/hosting, client Docker image, production API URL/CORS and workflow changes
(all deferred to the post-migration hosting stage per D3/D6). Backlog, explicitly not parity:
date-range filters, orientation filters (the two `TODO`s in `Filters.razor`), auth, pagination of
Drive ingestion, CI test pipeline.

---

## Phase 1 — .NET 10 and dependency upgrade

No behavior changes in this phase. Fix compile/analyzer fallout only.

### 1.1 SDK and language level

- Files: `global.json`, `src/Directory.Build.props`, `src/Ssera.Client/Ssera.Client.csproj`.
- Work:
  - Replace `global.json` contents with `{ "sdk": { "version": "10.0.100", "rollForward": "latestFeature" } }`.
    This accepts the installed 10.0.3xx SDKs and refuses the local .NET 11 preview.
  - `Directory.Build.props`: `<TargetFramework>net10.0</TargetFramework>`, `<LangVersion>14</LangVersion>`.
  - Remove the `<LangVersion>preview</LangVersion>` override from `Ssera.Client.csproj`.
- Verify: `dotnet --version` reports a 10.0.x SDK from the repo root; `dotnet build src/Ssera.Client/Ssera.Client.csproj` still fails only on package TFM mismatches (expected until 1.3).

### 1.2 API and Shared package upgrade

- Files: `src/Ssera.Api/Ssera.Api.csproj`, `src/Ssera.Shared/Ssera.Shared.csproj`.
- Work: bump to latest stable at implementation time (values verified on NuGet 2026-09-21):

  | Package | From | To |
  | --- | --- | --- |
  | Immediate.Apis | 1.6.0 | 6.4.0 |
  | Immediate.Handlers | 2.0.0 | 4.2.0 |
  | Immediate.Validations | 2.3.0 | 3.7.0 |
  | Microsoft.EntityFrameworkCore / .Sqlite / .Design | 9.0.3 | 10.0.12 |
  | Microsoft.AspNetCore.OpenApi | 9.0.3 | 10.0.12 |
  | Scalar.AspNetCore | 2.1.2 | 2.17.6 |
  | Google.Apis.Sheets.v4 | 1.69.0.3694 | 1.75.0.4178 |
  | Google.Apis.Drive.v3 | 1.69.0.3703 | 1.76.0.4261 |
  | Microsoft.Extensions.Configuration.Abstractions (Shared) | 9.0.8 | 10.0.12 |

- Work: the Immediate.* libraries are multiple majors behind. Read their release notes (NuGet page →
  repository link) before changing code; expect breaking changes in `IValidationTarget<T>`,
  `[Validate]`, `[MapGet]` and generated handler registration. Adapt the existing feature files, do
  not rewrite them. If a breaking change is too invasive, pin the highest compatible version and
  record the reason in this plan.
- Verify: `dotnet build src/Ssera.Api/Ssera.Api.csproj` succeeds; `dotnet run` starts, migrates,
  and `/scalar/v1` lists `/api/events`, `/api/history`, `/api/images`.

### 1.3 Blazor client package upgrade

- Files: `src/Ssera.Client/Ssera.Client.csproj`.
- Work: bump `Microsoft.AspNetCore.Components.WebAssembly` and `.DevServer` to 10.0.12,
  `Microsoft.Extensions.Http` to 10.0.12, and `Radzen.Blazor` from 7.2.3 to 11.4.1.
- Work: Radzen spans four majors. Fix compile errors and check: `AddRadzenComponents`,
  `RadzenTheme` + `Themes.Free`, `Radzen.ThemeService.SetTheme`, `RadzenStack`,
  `RadzenDropDown` (`Data`/`TextProperty`/`ValueProperty`/`Change`/`Multiple`/`AllowFiltering`/
  `LoadData`), `RadzenCheckBox`/`RadzenLabel`, `RadzenLink`, `RadzenButton`, `RadzenProgressBarCircular`.
  If `Themes.Free`/theme APIs moved, update `ThemeService.cs` and `MainLayout.razor` accordingly.
- Verify: client builds and runs; `/` renders Radzen-styled gallery; theme dropdown switches themes.

### 1.4 API Docker image and tooling

- Files: `src/Ssera.Api/Dockerfile`, `src/.dockerignore`.
- Work: `sdk:9.0` → `sdk:10.0`, `aspnet:9.0` → `aspnet:10.0`. Do not create a client Dockerfile;
  client hosting is deferred (D3).
- Work: update the global EF tool (`dotnet tool update --global dotnet-ef --version 10.*`) so future
  migrations use EF 10. No migration changes are expected in this phase.
- Verify: `dotnet build src/Ssera.sln`; `docker build -f src/Ssera.Api/Dockerfile -t ssera-api-test .`
  (optional if Docker available).

### 1.5 Analyzer fallout

- Work: `AnalysisLevel=latest-all` gains new rules on .NET 10. Triage every new warning: fix real
  issues, and only extend the `NoWarn` list in `src/Directory.Build.props` for rules that are wrong
  for this codebase, with the existing comment style. Watch for framework obsoletions (e.g.
  forwarded-headers APIs in `Program.cs`).
- Verify: build output is no worse than the 43 pre-existing warnings; no new warning related to
  files touched in later phases.

**CHECKPOINT A — user tests**
1. `dotnet build src/Ssera.sln` → succeeds.
2. `dotnet run --project src/Ssera.Api/Ssera.Api.csproj` → Scalar docs open and all three endpoints are listed.
3. `dotnet run --project src/Ssera.Client/Ssera.Client.csproj` → app loads, gallery request reaches the API, theme dropdown works.
4. Report any package that needed pinning/downgrading (the agent must have stopped for this) and
   any visible Radzen regression.

---

## Phase 2 — Baseline cleanup and routing

Restores the parts of the Svelte skeleton that do not need new pages.

### 2.1 Remove debug leftovers

- Files: `src/Ssera.Client/Pages/Home.razor.cs`, `src/Ssera.Client/Pages/Home.razor`,
  `src/Ssera.Client/Infra/ImageApiService.cs`, `src/Ssera.Client/wwwroot/js/masonry.js`.
- Work: delete the `Test()` button, `_meow` field and direct `HttpClient` call from `Home`;
  remove `await Task.Delay(TimeSpan.FromSeconds(2), token)` from `ImageApiService`;
  remove `console.log`/`console.time` debug lines from `masonry.js`.
- Verify: gallery loads without the artificial delay; no console spam.

### 2.2 Fix routes and navigation

- Files: `src/Ssera.Client/Pages/Home.razor`, `src/Ssera.Client/Pages/Gallery/Gallery.razor`,
  `src/Ssera.Client/Layout/MainLayout.razor`.
- Work: Home gets `@page "/"`; Gallery gets `@page "/gallery"`; nav becomes
  Home `/`, Events `/events`, Gallery `/gallery`, History `/history`. Events/History links stay
  until Phases 6/7 add the pages, or add nav entries in those phases instead (pick one, keep the
  final state correct).
- Verify: `/` shows Home, `/gallery` shows the gallery, nav highlights still work (RadzenLink uses
  `Target`, not `Href` — confirm it navigates and looks right, fix if not).

### 2.3 Remove stale Svelte artifacts

- Work: delete `src/Ssera.Client/.svelte-kit/`, `src/Ssera.Client/build/`, `src/Ssera.Client/node_modules/`
  (all untracked leftovers no longer used) and `src/Ssera.Client/Ssera.Client.sln` (nested duplicate
  solution not referenced anywhere).
- Work: remove the duplicated stylesheet `<link>` in `src/Ssera.Client/wwwroot/index.html`.
- Verify: `dotnet build src/Ssera.sln` and the client still run.

**CHECKPOINT B — user tests**
1. `/` = Home, `/gallery` = Gallery, no `/asdasd`.
2. Workers remain commented out (D5); nothing about them changed.
3. Repo root `git status` shows only intended changes; no Svelte dirs remain.

---

## Phase 3 — API parity work

### 3.1 Restore the `Hot` era

- Files: `src/Ssera.Shared/Images/Filters.cs`, `src/Ssera.Api/Features/Images/GetImages.cs`.
- Work: add `Hot` after `Crazy` in the `Era` enum; add its `GetDisplayName` case; add
  `Era.Hot → TopLevelKind.Hot` and `TopLevelKind.Hot → Era.Hot` mappings. The DB enum already has
  `Hot` (`src/Ssera.Api/Data/ImageArchive.cs`).
- Verify (Scalar): `GET /api/images?requestParameters={"page":1,"pageSize":50,"eras":["Hot"]}`
  returns the Hot images (or `[]` on an empty DB) and no 500.

### 3.2 Tags listing endpoint (D1)

- Files: new `src/Ssera.Api/Features/Images/GetImageTags.cs` following the existing feature
  conventions (sealed partial class, `[Handler]`, `[MapGet("/api/images/tags")]`, `[Validate]`
  request record, static `HandleAsync`).
- Work: request `{ string? Search }`; query `dbContext.Set<ImageArchiveTag>()`, `Select(t => t.Tag)`,
  `Distinct()`, optional `EF.Functions.Like` on search (case-insensitive `ToLower` if needed),
  order by tag, cap at a sane limit (e.g. 500). Response: `IReadOnlyList<string>`.
  `ImageArchiveTag` has an index on `Tag`.
- Verify (Scalar): `GET /api/images/tags` returns distinct tags; `?search=press` filters.

### 3.3 Move events/history response DTOs into Shared


- Files: `src/Ssera.Api/Features/Events/GetEvents.cs`,
  `src/Ssera.Api/Features/History/GetHistory.cs`, new files under `src/Ssera.Shared/Events/` and
  `src/Ssera.Shared/History/`.
- Work: add `GetEventsResponse(List<Event> Results, DateTime? LastUpdate, int TotalResults)` and
  `Event(DateTime Date, EventType Type, string? Title, string? Link)` to Shared using the existing
  `Ssera.Shared.Events.Filters` enums; add `HistoryEntry(DateTimeOffset Timestamp, string WorkerName,
  string Message)` to Shared. Replace the API-local records with the shared ones. Do not change the
  JSON shape (property names stay camelCase).
- Verify (Scalar): both endpoints return the same payload as before.

### 3.4 Shared problem-details DTO

- Files: new `src/Ssera.Shared/Errors/ApiProblemDetails.cs`.
- Work: record with `status`, `type`, `title`, `detail`, `activityTraceId`, `requestTraceId` and an
  optional `errors: Dictionary<string, string[]>`, matching what `ExceptionHandlerMiddleware` emits
  (`ExceptionHandlerMiddleware.cs:56-71`) and what the Svelte `ApiHelper.ProblemDetails` consumed.
- Verify: payload seen from a deliberately bad request (e.g. `GET /api/images?requestParameters={}`)
  deserializes into this type in later phases.

**CHECKPOINT C — user tests**
1. Scalar: `/api/images` with `Hot`, `/api/images/tags`, `/api/events`, `/api/history` all respond.
2. Compare `/api/events` JSON against what the live Svelte site consumes (same field names).
3. No migrations were added (schema unchanged).

---

## Phase 4 — Client infrastructure

### 4.1 API services and error handling

- Files: `src/Ssera.Client/Infra/ImageApiService.cs`, new `EventsApiService.cs`, `HistoryApiService.cs`,
  `TagApiService.cs` (or one `SseraApiClient` split by area), new `ApiException.cs`,
  `src/Ssera.Client/Program.cs`.
- Work:
  - Register one named `HttpClient` ("SseraApi") with `BaseAddress` from `SseraApiUrl`, falling back
    to `builder.HostEnvironment.BaseAddress` when the key is absent; services call relative paths
    `api/images`, `api/images/tags`, `api/events`, `api/history`.
  - Remove the duplicate `AddScoped<ImageApiService>()` registration.
  - On non-success status, parse the body into `ApiProblemDetails` and throw
    `ApiException(status, problem, rawBody)`; on network failure throw a wrapped exception. Include
    the raw body in the exception message for development.
  - `ImageApiService.GetImagesAsync(GetImagesQuery, int page, CancellationToken)` and
    `GetTagsAsync(string? search, CancellationToken)`.
  - `EventsApiService.GetEventsAsync(...)` builds individual repeated query params
    (`eventTypes=A&eventTypes=B`) because `/api/events` keeps its original contract.
  - `HistoryApiService.GetHistoryAsync()`.
  - Wire `CancellationToken` through; do not add artificial delays.
- Verify: gallery still loads through the new services; break the API URL in `appsettings.json` and
  confirm an `ApiException` is thrown (visible in console) instead of an unhandled exception.

### 4.2 Reusable components

- Files: new `src/Ssera.Client/Infra/Components/SseraPagingControls.razor(.cs)`,
  `SseraError.razor`.
- Work:
  - `SseraPagingControls`: two-way bindable `Page`, inputs `PageSize`, `TotalResults`,
    `MaxPage` (or derive max page); renders `<` button, number input, `>` button and
    "Showing X - Y of Z results" exactly like Svelte's `PageControls.svelte`. Clamp on change:
    `page < 1 → 1`, `page > maxPage → maxPage`; disable prev on page 1 and next on max page;
    `onchange`/blur semantics like Svelte (typed value only applied on blur/enter).
  - `SseraError`: renders `(status) message` plus "Request trace id" and "Activity trace id" lines,
    matching `gallery/+page.svelte` on `main`.
- Verify: temporarily render both on Home with fake data; behavior matches the Svelte component.

### 4.3 URL state helper

- Files: new `src/Ssera.Client/Infra/FilterUrlState.cs`; refactor `Gallery.razor.cs` to use it.
- Work:
  - `T? Read<T>(NavigationManager, string key)` / `void Write<T>(NavigationManager, ...)` using
    `JsonSerializer` with web defaults and a single `Uri.EscapeDataString` over the whole JSON value
    (drop the per-tag `HttpUtility.UrlEncode` hack; tags round-trip via the JSON string).
  - Add a separate `page` query parameter (int, default 1) alongside `filters`.
  - Always preserve unknown existing query params when navigating.
- Verify: set filters + page, reload the page, state restores; paste the URL into a new tab and it
  restores; a malformed `filters` value falls back to defaults without throwing.

**CHECKPOINT D — user tests**
1. Gallery still works after the service/URL refactor (regression pass).
2. Deliberately stop the API → gallery shows a readable error (temporary placement is fine; Phase 5
   wires it into the gallery).
3. Check a shared gallery URL round-trips filters.

---

## Phase 5 — Gallery feature parity

### 5.1 Paging

- Files: `Gallery.razor`, `Gallery.razor.cs`.
- Work: track current page (from URL), pass `Page` in `GetImagesQuery` instead of the hardcoded 1,
  render `SseraPagingControls` above and below the results, use `TotalResults` from the response for
  `maxPage` and the X–Y-of-Z text, update the URL on page change, and reset to page 1 whenever
  filters are applied. If the server clamps/returns empty for page > max, clamp client-side.
- Verify: with a DB of > 1 page, navigate pages, reload, share a link to page N, apply a filter and
  confirm it returns to page 1.

### 5.2 Error handling

- Files: `Gallery.razor`, `Gallery.razor.cs`.
- Work: catch `ApiException`/network errors; keep the last successful `Images` visible; show
  `SseraError` with status, message, trace ids (Svelte behavior); re-enable the Apply button.
- Verify: point the client at a stopped/incorrect API or request an invalid page size → error panel
  with trace ids, no unhandled exception overlay.

### 5.3 Era display and mobile masonry

- Files: `Results.razor`, `Results.razor.css`.
- Work: render the era (human name via `Era.GetDisplayName`) as the first tag before the tag list,
  matching Svelte's `if (res.era) tags = [res.era, ...tags]`. Add the Svelte mobile breakpoint:
  `@media (width <= 650px)` → two equal columns and full-width images. Keep masonry JS in sync.
- Verify: cards show date, era, tags; at ≤650 px width the grid shows two columns.

### 5.4 Functional tag filter (D1, approved additive feature)

- Files: `Filters.razor`, `Filters.razor.cs`, `TagApiService`.
- Work: replace the hardcoded `_tagsDropdownData` with the `/api/images/tags` endpoint using
  Radzen `LoadData`/`AllowFiltering` (server-side search). Keep the Include/Exclude selection type.
  Preserve selected tags when the dropdown re-queries.
- Verify: tag list contains real tags; search narrows it; include vs exclude produce different
  result counts; selection round-trips through the URL.

### 5.5 Filter polish

- Files: `Filters.razor`, `Filters.razor.cs`, `FiltersModel.cs`.
- Work: keep the page-size dropdown (D4); its fixed values `[50, 100, 500, 1000]` are all within the
  API's 10–1000 validation range. Add the Svelte "Unapplied changes" indicator: track a dirty flag
  on any control change, clear on Apply. Disable Apply while loading.
- Verify: changing a control shows the dirty marker; Apply clears it; Apply is disabled during a
  request.

**CHECKPOINT E — user tests (side-by-side against the live Svelte site)**
1. Filters: order by, sort, eras, members, tags (include/exclude), page size, dirty marker.
2. Results: era first, tags, date, links open correct Drive files, masonry columns.
3. Paging: top and bottom controls, page input, totals, reset on filter change, URL round-trip.
4. Error panel: stop the API mid-session, apply → error with trace ids; restart and apply → recovers.
5. Responsive: narrow window ≤650 px gives two columns.

---

## Phase 6 — Events page

### 6.1 Presentation helpers

- Files: new `src/Ssera.Shared/Events/EventTypePresentation.cs`.
- Work: add the human display names from Svelte's `AllEventTypes`
  (`TeasersMV → "Teasers/MV"`, `MusicShows → "Music Shows"`, `Misc → "Miscellaneous"`, etc.) and the
  `TypeColors` map (`TeasersMV #ff00ff`, `Performance #ff9900`, ...). A `GetColor`/`GetDisplayName`
  pair keeps the API and client consistent.

### 6.2 Page and filters

- Files: new `src/Ssera.Client/Pages/Events/Events.razor` (+ `.razor.cs`), filters component.
- Work: port `events/+page.svelte` and `ListFilters.svelte`:
  - Filters: Order by (Date/Type/Title), Sort (Ascending/Descending), Event type checkboxes with
    accent color and the dirty background highlight, Search title, Rows per page (10–1000),
    Apply + "Unsaved changes".
  - Table: `#`, Date, Type, Title, Link (or `-`); row background
    `color-mix(in srgb, {TypeColor}, white 80%)`; number = `(page - 1) * pageSize + index + 1`;
    responsive wrapping for `.col-type` under 800 px; date via `ToShortDateString()`.
  - Paging: `SseraPagingControls` top and bottom; reset to page 1 when filters change.
  - URL state: per D2; page in URL; restore on load.
  - Loading/error states via the Phase 4 components.
- Verify: compare against the Svelte events page; check a page with many rows, links, and URL reload.

### 6.3 Navigation

- Files: `MainLayout.razor`.
- Work: ensure the Events link points to the working page.

**CHECKPOINT F — user tests**
1. `/events` matches the Svelte page row-for-row on the same data.
2. Filters, dirty marker, paging, URL sharing, error UI all behave.
3. Type colors and the mobile/narrow layout look right.

---

## Phase 7 — History page

### 7.1 Page

- Files: new `src/Ssera.Client/Pages/History/History.razor` (+ `.razor.cs`), `HistoryApiService` usage.
- Work: port `history/+page.svelte`: intro text "The worker runs data import and aggregation on a
  schedule", table with Timestamp (`ToLocalTime().ToString()`), Worker, Message (preserve newlines
  with `white-space: pre-line`), newest 50 entries, loading/error states.
- Work: nav label becomes History → `/history`.

**CHECKPOINT G — user tests**
1. `/history` shows worker log rows, newest first, newlines preserved.
2. Error state when the API is down.

---

## Phase 8 — Documentation (deployment deferred)

Deployment/hosting is out of scope (D3/D6). Do not create a client Dockerfile, change workflows, or
touch production config. Deferred items are listed at the end of this phase.

### 8.1 Documentation and cleanup

- Files: `AGENTS.md`, `README.md` (if it mentions Svelte), `.gitignore` if needed.
- Work: rewrite AGENTS.md for the new architecture: Blazor WASM client + Shared project, no Node/npm
  commands, `dotnet build src/Ssera.sln` as the single build, current client ports and config, new
  endpoint `/api/images/tags`, updated conventions (Radzen, shared DTOs, URL state), the workers
  stay commented out for development (D5), and the known pre-existing warnings. Remove the Svelte
  and "stale leftovers" sections, and note the deferred deployment items below.
- Verify: an agent following AGENTS.md can build/run the app without opening this plan.

### Deferred to the post-migration hosting stage

- Client Docker image and serving strategy (D3).
- Production API URL, `appsettings.Production.json`, and server CORS config (D6) — **user action**.
- `.github/workflows/docker-build-and-push.yml`: it still references the deleted
  `src/Ssera.Client/Dockerfile`; leave it untouched and fix it in the hosting stage.
- Image publishing / workflow dispatch — **user action**.

**CHECKPOINT H — user tests**
1. AGENTS.md matches reality; build/run commands verified.
2. `dotnet build src/Ssera.sln` succeeds; `git status` shows only intended changes.

---

## Phase 9 — Final parity audit

The user runs the table below (against the live Svelte site or `main` checked out locally) and ticks
each row. The agent may use `main` locally for read-only comparison but does not access production
infrastructure. Anything failing becomes a bug fix before calling the migration done.

| # | Check |
| --- | --- |
| 1 | Home content identical to Svelte home, at `/` |
| 2 | Nav: Home / Events / Gallery / History all resolve |
| 3 | Gallery: order by Date and Tags, ascending/descending |
| 4 | Gallery: era and member multi-selects |
| 5 | Gallery: tag filter works (include and exclude, per D1) |
| 6 | Gallery: page size constrained to 10–1000 |
| 7 | Gallery: dirty marker and Apply behavior |
| 8 | Gallery: results show date, era, tags and open Drive links |
| 9 | Gallery: masonry and two-column mobile layout |
| 10 | Gallery: paging controls top/bottom, totals, page reset |
| 11 | Gallery: URL restores filters + page |
| 12 | Gallery: error panel with status/detail/trace ids |
| 13 | Events: filters (order, sort, types, search, page size) |
| 14 | Events: table columns, colors, row numbering, links |
| 15 | Events: paging, URL state, error state |
| 16 | History: table with timestamp/worker/message, newest 50 |
| 17 | Theme switcher persists across reloads (new feature, verify no regression) |
| 18 | API endpoints unchanged for events/history plus new tags endpoint |
| 19 | AGENTS.md accurately describes build/run; deployment explicitly deferred |
| 20 | `dotnet build src/Ssera.sln` warning count is not regressed without justification |

**CHECKPOINT I — sign-off**: user confirms parity, closes tracking of known accepted differences.

---

## Risks

- **Radzen 7 → 11** and **Immediate.* majors** are the two riskiest upgrades. Both are isolated in
  Phase 1 and gated by Checkpoint A. Do not start Phase 2 until the upgraded stack runs.
- The new `/api/images/tags` endpoint plus the `Hot` era are additive and cannot break the Svelte
  client on `main` (it is deleted on this branch anyway; both are needed only by the Blazor client).
- Paging relies on `TotalResults`, which the API already returns; no server change needed.
- Keep `GetEvents` and `GetHistory` response shapes frozen — only move the DTO definitions to Shared.
