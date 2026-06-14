# PropFilters

A lightweight .NET 10 REST API middleware that enables **GraphQL-style field selection over a standard REST endpoint**. Clients declare exactly which fields they want returned — no more, no less — without requiring any migration away from REST or changes to existing client integrations.

---

## Table of Contents

- [Why PropFilters?](#why-propfilters)
- [How It Works](#how-it-works)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [PropFilters Syntax](#propfilters-syntax)
  - [Basic Field Selection](#basic-field-selection)
  - [Nested Fields](#nested-fields)
  - [Array Slicing](#array-slicing)
  - [Combining Filters](#combining-filters)
- [Request & Response Examples](#request--response-examples)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Tech Stack](#tech-stack)

---

## Why PropFilters?

In a typical travel platform, a hotel search API returns a large unified payload covering availability data, content (images, descriptions, facilities, landmarks, etc.), pricing, and more. As the platform grows and serves many different clients — mobile apps, web frontends, B2B partners — each client needs a different subset of that data.

**The problem:** Migrating from REST to GraphQL solves field-selection elegantly, but forces every existing client to rewrite their integration.

**The solution:** PropFilters lets clients opt in to field selection **incrementally**, using the same REST endpoint they already call. Each client passes a `PropFilters` list in the request body specifying only the fields they care about. Clients that don't pass any filters receive the full response as before — zero breaking changes.

```
Without PropFilters  →  Full payload (every field)
With PropFilters     →  Only requested fields, shaped to spec
```

---

## How It Works

1. The client sends a `POST` request to `/api/content/v1/result` with a standard `HotelSearchRequest` body.
2. The optional `PropFilters` array declares dot-notation field paths, prefixed by `hotel.` or `content.`.
3. The server builds a **filter tree** from those paths and projects the hotel availability and content collections through it.
4. Only the requested fields are serialised and returned. Null values are omitted automatically.

---

## Project Structure

```
PropFilters/
├── Controllers/
│   └── HotelContentController.cs     # POST /api/content/v1/result
├── Models/
│   ├── HotelSearchRequest.cs         # Inbound request (includes PropFilters)
│   ├── HotelResponse.cs              # Full upstream response shape
│   ├── HotelAvailability.cs          # Availability / pricing data per hotel
│   └── HotelContent.cs              # Rich content per hotel (images, facilities, etc.)
├── PropFilter/
│   ├── FilterNode.cs                 # Tree node (field name + optional slice metadata)
│   ├── FilterTreeBuilder.cs          # Parses PropFilters strings → FilterNode tree
│   ├── HotelResultProjector.cs       # Orchestrates projection of hotel + content lists
│   └── NestedFieldProjector.cs       # Reflection-based recursive field projector
├── Program.cs                        # App bootstrap & DI registration
├── appsettings.json
├── appsettings.Development.json
├── hotel-response.json               # Sample data (simulates upstream API response)
└── PropFilters.csproj
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run locally

```bash
git clone <repo-url>
cd PropFilters
dotnet run --project PropFilters
```

The API will start at `https://localhost:7xxx` / `http://localhost:5xxx` (see `Properties/launchSettings.json` for the exact ports).

OpenAPI docs are available at `/openapi/v1.json` when running in Development mode.

---

## API Reference

### `POST /api/content/v1/result`

Returns hotel availability and content, optionally projected to only the fields listed in `PropFilters`.

#### Request Body

```json
{
  "CheckIn": "2025-09-01",
  "CheckOut": "2025-09-05",
  "Occupancies": [
    { "Adults": 2, "Ages": [] }
  ],
  "PropFilters": [
    "hotel.HotelId",
    "hotel.PriceInfo.TotalPrice",
    "hotel.PriceInfo.CurrencyCode",
    "content.HotelId",
    "content.Name",
    "content.StarRating",
    "content.Images[3]"
  ]
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `CheckIn` | `DateOnly` | Yes | Check-in date (`YYYY-MM-DD`) |
| `CheckOut` | `DateOnly` | Yes | Check-out date (`YYYY-MM-DD`) |
| `Occupancies` | `Occupancy[]` | Yes | Room occupancy requirements |
| `PropFilters` | `string[]` | No | Field paths to include in the response. Omit for the full payload. |

#### Response

```json
{
  "Token": "abc123",
  "HotelCount": 2,
  "Hotels": [ /* projected hotel availability objects */ ],
  "Content": [ /* projected hotel content objects */ ],
  "HasErrors": false
}
```

---

## PropFilters Syntax

All filter paths follow dot-notation and must be prefixed with either `hotel.` or `content.` to target the corresponding collection.

### Basic Field Selection

Select a top-level scalar field:

```
hotel.HotelId
hotel.Refundable
content.Name
content.StarRating
```

### Nested Fields

Use dots to traverse into nested objects:

```
hotel.PriceInfo.TotalPrice
hotel.PriceInfo.CurrencyCode
content.HotelContact.Phone
content.HotelContact.CountryName
content.HotelChain.Name
```

### Array Slicing

Append `[N]` to limit a list to the first N items, or `[^N]` to take the last N items:

```
content.Images[3]          → first 3 images
content.Images[^2]         → last 2 images
content.Facilities[5]      → first 5 facilities
content.Landmarks[^1]      → last landmark only
```

Slicing and sub-field selection can be combined:

```
content.Images[3].Url      → Url field from first 3 images
content.Images[3].ImageType
```

### Combining Filters

Multiple paths are combined additively. You can mix hotel and content fields freely:

```json
"PropFilters": [
  "hotel.HotelId",
  "hotel.PriceInfo.TotalPrice",
  "hotel.PriceInfo.CurrencyCode",
  "hotel.Refundable",
  "content.HotelId",
  "content.Name",
  "content.StarRating",
  "content.Description",
  "content.Images[1].Url",
  "content.HotelContact.Address",
  "content.HotelContact.CityName"
]
```

If `PropFilters` is `null` or empty, the full unfiltered response is returned.

---

## Request & Response Examples

### Full payload (no filters)

```json
{
  "CheckIn": "2025-09-01",
  "CheckOut": "2025-09-05",
  "Occupancies": [{ "Adults": 2, "Ages": [] }]
}
```

Response includes every field on every hotel and content object.

---

### Minimal availability view

```json
{
  "CheckIn": "2025-09-01",
  "CheckOut": "2025-09-05",
  "Occupancies": [{ "Adults": 2, "Ages": [] }],
  "PropFilters": [
    "hotel.HotelId",
    "hotel.PriceInfo.TotalPrice",
    "hotel.PriceInfo.CurrencyCode",
    "hotel.Refundable"
  ]
}
```

Response — each hotel object contains only the requested fields:

```json
{
  "Token": "abc123",
  "HotelCount": 1,
  "Hotels": [
    {
      "HotelId": "HTL001",
      "PriceInfo": {
        "TotalPrice": 320.00,
        "CurrencyCode": "USD"
      },
      "Refundable": "Yes"
    }
  ],
  "Content": [],
  "HasErrors": false
}
```

---

### Listing card view (thumbnail + name + rating + price)

```json
{
  "CheckIn": "2025-09-01",
  "CheckOut": "2025-09-05",
  "Occupancies": [{ "Adults": 2, "Ages": [] }],
  "PropFilters": [
    "hotel.HotelId",
    "hotel.PriceInfo.TotalPrice",
    "hotel.PriceInfo.CurrencyCode",
    "content.HotelId",
    "content.Name",
    "content.StarRating",
    "content.AccommodationType",
    "content.Images[1].Url"
  ]
}
```

---

### Detail page view (images, facilities, contact)

```json
{
  "CheckIn": "2025-09-01",
  "CheckOut": "2025-09-05",
  "Occupancies": [{ "Adults": 2, "Ages": [] }],
  "PropFilters": [
    "hotel.HotelId",
    "hotel.PriceInfo",
    "content.HotelId",
    "content.Name",
    "content.Description",
    "content.StarRating",
    "content.Images[10].Url",
    "content.Images[10].ImageType",
    "content.Facilities[5].Title",
    "content.Facilities[5].IsFree",
    "content.HotelContact.Address",
    "content.HotelContact.CityName",
    "content.HotelContact.Phone",
    "content.Landmarks[3].Title",
    "content.Landmarks[3].DistanceInKm"
  ]
}
```

---

## Architecture

```
HTTP Request
     │
     ▼
HotelContentController
     │  Reads hotel-response.json (simulates upstream)
     │  Calls FilterTreeBuilder.BuildFromPropFilters()
     ▼
FilterTreeBuilder
     │  Parses dot-notation paths with optional [N] / [^N] slice annotations
     │  Splits into hotel.* and content.* trees
     │  Returns two FilterNode trees
     ▼
HotelResultProjector
     │  Delegates to NestedFieldProjector for each list
     ▼
NestedFieldProjector
     │  Walks the FilterNode tree via reflection
     │  Handles nested objects, collections, and array slicing
     │  Returns List<object> (dictionary-shaped projections)
     ▼
Controller shapes final response → 200 OK
```

### Key design decisions

**Reflection with caching** — `NestedFieldProjector` uses `PropertyInfo` via reflection but caches the property map per type in a static dictionary, so the reflection cost is paid only once per type across the lifetime of the application.

**Case-insensitive matching** — both the filter tree and property lookup use `StringComparer.OrdinalIgnoreCase`, so clients can write `hotel.priceinfo.totalprice` or `hotel.PriceInfo.TotalPrice` interchangeably.

**Null-safe projection** — null values encountered during traversal are returned as `null` rather than throwing. The JSON serialiser is configured with `DefaultIgnoreCondition = WhenWritingNull`, so null fields are omitted from the response entirely.

**Graceful degradation** — malformed filter paths are silently skipped rather than failing the request. Unrecognised field names are ignored.

**Zero breaking changes** — clients that omit `PropFilters` receive the full response. The feature is purely additive.

---

## Configuration

### `appsettings.json`

Standard ASP.NET Core configuration. No PropFilters-specific settings are required.

### JSON serialisation (`Program.cs`)

```csharp
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;       // PascalCase preserved
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;                         // nulls omitted
});
```

### Dependency injection

```csharp
builder.Services.AddSingleton<HotelResultProjector>();
builder.Services.AddSingleton<NestedFieldProjector>();
```

Both services are registered as singletons because they are stateless and the property cache in `NestedFieldProjector` is intentionally shared across requests.

---

## Tech Stack

| Component | Technology |
|---|---|
| Framework | ASP.NET Core 10 (Minimal Hosting Model) |
| Language | C# 13 |
| API docs | Microsoft.AspNetCore.OpenApi 10.0.2 |
| Serialisation | System.Text.Json (built-in) |
| Field projection | Reflection + static property cache |
| Target runtime | .NET 10 |
| Author | Sohil Mansuri |
