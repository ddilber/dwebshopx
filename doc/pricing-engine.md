# dWebShop – Pricing Engine Implementation

**Status:** Implemented (Phase 1 + Phase 2)  
**Date:** 2026-05-19  
**Branch:** `claude/quirky-joliot-ee1a47`

---

## Overview

The pricing engine implements the B2B wholesale pricing model described in the technical design document. It provides deterministic, auditable price calculation with customer-specific pricing, multi-tier discounts, and immutable order snapshots.

The implementation is split into two phases:

- **Phase 1** – Domain model extensions and database schema (entities + EF migration)
- **Phase 2** – Pricing engine pipeline (`IPricingEngine` service)

The existing `IPricingService` (simple cart price resolution) is kept untouched alongside the new engine.

---

## Phase 1 – Domain Model

### New Entities

#### `VatRate`
> `src/dWebShop.Domain/Entities/Pricing/VatRate.cs`

Versioned VAT rate. Allows VAT changes to not affect historical invoices.

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | E.g. "Standard 25%", "Reduced 13%" |
| `Rate` | `decimal(5,2)` | Percentage value, e.g. `25.00` |
| `ValidFrom` | `DateTime` | Start of validity |
| `ValidTo` | `DateTime?` | End of validity (null = indefinite) |
| `IsActive` | `bool` | Soft disable |

#### `PaymentTerms`
> `src/dWebShop.Domain/Entities/Pricing/PaymentTerms.cs`

Payment conditions including optional cash discount (skonto).

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | E.g. "Net 30" |
| `DueDays` | `int` | Days until invoice is due |
| `CashDiscountPercent` | `decimal(5,2)?` | % discount if paid early |
| `CashDiscountDays` | `int?` | Days within which early payment qualifies |
| `IsActive` | `bool` | |

#### `CustomerProductPrice`
> `src/dWebShop.Domain/Entities/Pricing/CustomerProductPrice.cs`

Highest-priority per-customer price override for a specific SKU. Supports quantity tiers via `MinQuantity`.

| Property | Type | Notes |
|---|---|---|
| `PartnerId` | `int` | FK → Partners |
| `ProductSkuId` | `int` | FK → ProductSkus |
| `Price` | `decimal(18,4)` | |
| `MinQuantity` | `decimal(18,4)?` | Tier threshold |
| `ValidFrom` | `DateTime` | |
| `ValidTo` | `DateTime?` | |

Index: `(PartnerId, ProductSkuId)`

#### `DiscountDefinition`
> `src/dWebShop.Domain/Entities/Pricing/Discount.cs`

Logical identity of a discount. Contains the business rules for stacking and priority.

| Property | Type | Notes |
|---|---|---|
| `Name` | `string(200)` | |
| `Code` | `string(100)` | Unique business code |
| `Type` | `DiscountType` | See enum below |
| `AllowStacking` | `bool` | Whether this discount stacks with others |
| `Priority` | `int` | Higher = applied/evaluated first |
| `IsActive` | `bool` | |

#### `DiscountVersion`
> `src/dWebShop.Domain/Entities/Pricing/Discount.cs`

Versioned snapshot of a discount's active period. Only `IsPublished` versions are active.

| Property | Type | Notes |
|---|---|---|
| `DiscountDefinitionId` | `int` | FK → DiscountDefinitions |
| `ValidFrom` | `DateTime` | |
| `ValidTo` | `DateTime?` | |
| `IsPublished` | `bool` | Must be true to be applied |

#### `DiscountRule`
> `src/dWebShop.Domain/Entities/Pricing/Discount.cs`

Individual rule within a discount version. Specifies what the discount targets and under what conditions.

| Property | Type | Notes |
|---|---|---|
| `DiscountVersionId` | `int` | FK → DiscountVersions |
| `TargetType` | `DiscountTargetType` | See enum below |
| `TargetId` | `int` | ID of the target entity |
| `Value` | `decimal(18,4)` | % or fixed amount depending on Type |
| `MinQuantity` | `decimal(18,4)?` | Minimum line quantity to activate |
| `MinOrderAmount` | `decimal(18,4)?` | Minimum order subtotal (for Order-type rules) |
| `IsExclusive` | `bool` | If true, blocks all other discounts on the same target |

Index: `(TargetType, TargetId)`

---

### Enums

```csharp
public enum DiscountType
{
    Percentage  = 0,   // Value is % deduction
    FixedAmount = 1,   // Value is subtracted from price (floor 0)
    FixedPrice  = 2,   // Value replaces the price entirely
}

public enum DiscountTargetType
{
    Product     = 0,   // TargetId = Product.Id
    ProductGroup = 1,  // TargetId = product group ID (future)
    Category    = 2,   // TargetId = Category.Id
    Customer    = 3,   // TargetId = Partner.Id
    Order       = 4,   // No specific TargetId; applies to order total
    PaymentTerm = 5,   // TargetId = PaymentTerms.Id (future)
}
```

---

### Extended Existing Entities

#### `Pricelist`
Added `Code` (unique business identifier) and `IsDefault` flag.

#### `PricelistItem`
Added `Currency` (`varchar(3)`, e.g. `"EUR"`), `ValidFrom`, `ValidTo` for time-bounded prices.

#### `Partner`
Added `PaymentTermsId` nullable FK → `PaymentTerms`. Allows per-customer payment conditions.

#### `ProductSku`
Added `VatRateId` nullable FK → `VatRate`. Backward-compatible: if null, the legacy `Tax` decimal field is used.

#### `OrderItem`
Added immutable pricing snapshot fields:

| Property | Type | Notes |
|---|---|---|
| `BasePrice` | `decimal(18,4)` | Price before discounts |
| `FinalPrice` | `decimal(18,4)` | Price after all discounts |
| `VatRateSnapshot` | `decimal(5,2)` | VAT rate captured at order time |
| `AppliedRulesJson` | `string?` | JSON array of `AppliedRuleRecord` |

These fields are written once at order creation and never recalculated. Historical orders remain correct even if pricing rules change.

---

### EF Configuration

All new entities are configured in `AppDbContext.OnModelCreating`:

- Decimal precision: `(18,4)` for prices/amounts, `(5,2)` for rates/percentages
- `DiscountDefinition.Code` has a unique index
- `DiscountRule.(TargetType, TargetId)` composite index for fast lookup
- `CustomerProductPrice.(PartnerId, ProductSkuId)` composite index
- Cascades: `DiscountDefinition → DiscountVersion → DiscountRule` all cascade delete
- Partner → PaymentTerms: `SetNull` on delete (don't lose partners if a payment term is removed)
- ProductSku → VatRate: `SetNull` on delete

---

### Migration

**Name:** `AddPricingEngineV2`  
**File:** `src/dWebShop.Infrastructure/Persistence/Migrations/20260518220924_AddPricingEngineV2.cs`

Creates 6 new tables and adds columns to 5 existing tables. Fully reversible via the `Down()` method.

---

## Phase 2 – Pricing Engine Pipeline

### Interface

```csharp
// src/dWebShop.Application/Services/IPricingEngine.cs

public interface IPricingEngine
{
    Task<PricingResult> CalculateAsync(PricingRequest request, CancellationToken ct = default);
}
```

### Request / Result Models

```
PricingRequest
├── PartnerId         int?          — null = anonymous / no partner pricing
├── Lines             List<PricingRequestLine>
│   ├── ProductSkuId  int
│   └── Quantity      decimal
└── PricingDate       DateTime?     — defaults to UtcNow if null

PricingResult
├── TotalBeforeDiscount  decimal    — sum of BasePrice × Qty across all lines
├── TotalItemDiscount    decimal    — sum of item-level discounts
├── TotalOrderDiscount   decimal    — order-level discount applied to subtotal
├── TotalDiscount        decimal    — TotalItemDiscount + TotalOrderDiscount
├── TotalAfterDiscount   decimal    — subtotal after all discounts
├── VatTotal             decimal    — sum of VAT amounts
├── GrandTotal           decimal    — TotalAfterDiscount + VatTotal
├── CashDiscountPercent  decimal?   — informational only (from PaymentTerms)
├── CashDiscountAmount   decimal?   — informational only
└── Lines             List<PricedLine>
    ├── ProductSkuId     int
    ├── Quantity         decimal
    ├── BasePrice        decimal    — resolved before discounts
    ├── UnitDiscount     decimal    — per-unit discount applied
    ├── FinalUnitPrice   decimal    — BasePrice - UnitDiscount
    ├── LineTotal        decimal    — FinalUnitPrice × Quantity
    ├── VatRate          decimal    — actual rate used (%)
    ├── VatAmount        decimal    — LineTotal × VatRate / 100
    └── AppliedRulesJson string    — JSON array of AppliedRuleRecord
```

`AppliedRuleRecord` (serialized into `AppliedRulesJson`):

```csharp
record AppliedRuleRecord(
    int    RuleId,
    string DiscountName,
    string DiscountCode,
    int    Type,         // DiscountType as int
    int    TargetType,   // DiscountTargetType as int
    decimal Value,
    decimal AmountOff    // actual amount subtracted at this step
);
```

---

### Pipeline Steps

The implementation (`PricingEngine.cs`) loads all data in bulk before processing, then runs through these steps in order:

```
CalculateAsync(PricingRequest)
│
├── 1. Bulk-load ProductSkus for all SKU IDs in the request
│
├── 2. Load active VatRates
│       Filter: ValidFrom ≤ pricingDate ≤ ValidTo (or ValidTo is null)
│
├── 3. Build Product → CategoryIds map
│       Used to match category-level discount rules
│
├── 4. Load Partner + PaymentTerms
│       Resolve active Pricelist (IsDefault → any assigned → none)
│
├── 5. Load CustomerProductPrices
│       Filter: PartnerId match, SKU in request, date valid
│
├── 6. Load PricelistItems
│       Filter: PricelistId resolved above, SKU in request, date valid
│
├── 7. Load active DiscountRules
│       Filter: version IsPublished, version date valid, definition IsActive
│       Target filter: Product targets, Category targets, Customer target, Order rules
│
├── 8. Per line: resolve price + apply item discounts
│       ├── Base price resolution (priority order):
│       │       CustomerProductPrice (best qty tier match)
│       │       → PricelistItem (best qty tier match)
│       │       → ProductSku.Price
│       │
│       ├── Collect applicable rules:
│       │       Product rules   (TargetId == Product.Id)
│       │       Category rules  (TargetId in product's CategoryIds)
│       │       Customer rules  (TargetId == PartnerId)
│       │       MinQuantity filter applied
│       │       Sorted by Priority descending
│       │
│       ├── Apply discounts:
│       │       If top rule IsExclusive → apply only that rule, stop
│       │       Else → apply all rules where AllowStacking == true, sequentially
│       │
│       └── Resolve VAT:
│               VatRate from VatRates table (if VatRateId set)
│               → fallback to ProductSku.Tax (legacy)
│
├── 9. Apply order-level discounts
│       Filter order rules by MinOrderAmount ≤ orderSubtotal
│       Same exclusive/stacking logic as item discounts
│
└── 10. Compute totals + cash discount (informational)
```

---

### Stacking Logic

```
Rules sorted by Priority DESC
│
├── Top rule IsExclusive == true?
│   └── YES → apply only this rule, ignore all others
│
└── NO → apply all rules where AllowStacking == true, in priority order
          Each rule receives the price output of the previous rule
          (sequential compounding, not parallel sum)
```

**Example — sequential stacking:**
```
Base price:   100.00
Rule 1 (-10%): 90.00   AmountOff = 10.00
Rule 2 (-5%):  85.50   AmountOff =  4.50
                       TotalOff  = 14.50
```

---

### Discount Type Formulas

| Type | Formula |
|---|---|
| `Percentage` | `price × (1 − value / 100)` |
| `FixedAmount` | `max(0, price − value)` |
| `FixedPrice` | `value` (replaces price directly) |

All results are rounded to 4 decimal places.

---

### VAT Resolution

1. If `ProductSku.VatRateId` is set → look up `VatRate.Rate` from the loaded rates (date-filtered)
2. If `VatRateId` is null → use `ProductSku.Tax` (legacy flat decimal, kept for backward compatibility)

Cash discount (skonto) is **informational only** — it is surfaced in `PricingResult.CashDiscountPercent` and `CashDiscountAmount` but is **not deducted from `GrandTotal`**. It is the customer's incentive to pay early.

---

### Registration

```csharp
// src/dWebShop.Infrastructure/DependencyInjection.cs
services.AddScoped<IPricingService, PricingService>();   // existing — cart
services.AddScoped<IPricingEngine, PricingEngine>();     // new — order pricing
```

Both services are scoped (per-request lifetime).

---

### Usage Example

```csharp
// Inject IPricingEngine
var result = await pricingEngine.CalculateAsync(new PricingRequest(
    PartnerId: 42,
    Lines: [
        new(ProductSkuId: 101, Quantity: 5),
        new(ProductSkuId: 207, Quantity: 1),
    ]
));

Console.WriteLine($"Grand total: {result.GrandTotal:F2}");

foreach (var line in result.Lines)
{
    Console.WriteLine($"SKU {line.ProductSkuId}: {line.FinalUnitPrice:F4} × {line.Quantity} = {line.LineTotal:F4}");
    Console.WriteLine($"  Rules applied: {line.AppliedRulesJson}");
}
```

---

### Writing Order Snapshots

When creating an `OrderItem`, copy the engine output directly:

```csharp
var orderItem = new OrderItem
{
    SkuId     = pricedLine.ProductSkuId,
    ProductId = ...,
    Quantity  = pricedLine.Quantity,
    Price     = pricedLine.FinalUnitPrice,
    Tax       = pricedLine.VatRate,
    Discount  = pricedLine.UnitDiscount,

    // Snapshot — written once, never recalculated
    BasePrice        = pricedLine.BasePrice,
    FinalPrice       = pricedLine.FinalUnitPrice,
    VatRateSnapshot  = pricedLine.VatRate,
    AppliedRulesJson = pricedLine.AppliedRulesJson,
};
```

The snapshot fields are immutable by convention — they record the pricing state at the moment of order creation. Do not recalculate them from live pricing tables.

---

## Relationship Between Services

| Service | Purpose | When to use |
|---|---|---|
| `IPricingService` | Simple price lookup (single SKU or batch) | Shopping cart, product listing |
| `IPricingEngine` | Full pipeline with discounts, VAT, snapshots | Order creation, admin quotes |

`IPricingService` continues to use the simpler resolution logic (pricelist override + `ClientDiscount` percentage). It does not use the new `DiscountRule` system. This is intentional — the cart does not need the full pipeline overhead.

---

## What Is Not Yet Implemented

| Feature | Status |
|---|---|
| `ProductGroup` discount target | Domain model ready (TargetType exists), no ProductGroupId on Product yet |
| `PaymentTerm` discount target | Enum value exists, not wired in engine |
| Pricelist versioning (`PriceListVersion`) | Not implemented; current model uses date ranges on items instead |
| Redis cache for active pricelists | Not implemented |
| Background recalculation workers | Not implemented |

---

## Phase 3 – Admin UI & CQRS Handlers

**Status:** Implemented  
**Date:** 2026-05-19  
**Branch:** `claude/quirky-joliot-ee1a47`

---

### CQRS Commands

All handlers follow the project's MediatR record-command pattern and inject `IAppDbContext`.

#### VatRate
> `src/dWebShop.Application/Features/Pricing/Commands/VatRateCommands.cs`

| Command | Returns | Description |
|---|---|---|
| `CreateVatRateCommand(Name, Rate, ValidFrom, ValidTo?, IsActive)` | `int` (new Id) | Create a new VAT rate |
| `UpdateVatRateCommand(Id, Name, Rate, ValidFrom, ValidTo?, IsActive)` | — | Update existing rate |
| `DeleteVatRateCommand(Id)` | — | Delete rate by Id |

#### PaymentTerms
> `src/dWebShop.Application/Features/Pricing/Commands/PaymentTermsCommands.cs`

| Command | Returns | Description |
|---|---|---|
| `CreatePaymentTermsCommand(Name, DueDays, CashDiscountPercent?, CashDiscountDays?, IsActive)` | `int` | Create |
| `UpdatePaymentTermsCommand(Id, ...)` | — | Update |
| `DeletePaymentTermsCommand(Id)` | — | Delete |

#### Discounts
> `src/dWebShop.Application/Features/Pricing/Commands/DiscountCommands.cs`

Three entity layers, each with its own commands:

**DiscountDefinition** (business identity)

| Command | Returns |
|---|---|
| `CreateDiscountDefinitionCommand(Name, Code, Type, AllowStacking, Priority, IsActive)` | `int` |
| `UpdateDiscountDefinitionCommand(Id, ...)` | — |
| `DeleteDiscountDefinitionCommand(Id)` | — (cascades versions + rules) |

**DiscountVersion** (active period)

| Command | Returns |
|---|---|
| `CreateDiscountVersionCommand(DiscountDefinitionId, ValidFrom, ValidTo?, IsPublished)` | `int` |
| `UpdateDiscountVersionCommand(Id, ValidFrom, ValidTo?, IsPublished)` | — |
| `DeleteDiscountVersionCommand(Id)` | — (cascades rules) |

**DiscountRule** (individual rule within a version)

| Command | Returns |
|---|---|
| `UpsertDiscountRuleCommand(Id?, DiscountVersionId, TargetType, TargetId, Value, MinQuantity?, MinOrderAmount?, IsExclusive)` | `int` |
| `DeleteDiscountRuleCommand(Id)` | — |

---

### CQRS Queries

#### VatRate
> `src/dWebShop.Application/Features/Pricing/Queries/VatRateQueries.cs`

```csharp
// Returns all VAT rates ordered by Name
record GetVatRatesQuery : IRequest<List<VatRateDto>>;

record VatRateDto(int Id, string Name, decimal Rate, DateTime ValidFrom, DateTime? ValidTo, bool IsActive);
```

#### PaymentTerms
> `src/dWebShop.Application/Features/Pricing/Queries/PaymentTermsQueries.cs`

```csharp
record GetPaymentTermsQuery : IRequest<List<PaymentTermsDto>>;

record PaymentTermsDto(int Id, string Name, int DueDays, decimal? CashDiscountPercent, int? CashDiscountDays, bool IsActive);
```

#### Discounts
> `src/dWebShop.Application/Features/Pricing/Queries/DiscountQueries.cs`

```csharp
// List view — priority descending, then name
record GetDiscountsQuery : IRequest<List<DiscountSummaryDto>>;

record DiscountSummaryDto(int Id, string Name, string Code, DiscountType Type, bool AllowStacking, int Priority, bool IsActive, int VersionCount);

// Detail view — includes full version + rule tree
record GetDiscountByIdQuery(int Id) : IRequest<DiscountDetailDto?>;

record DiscountDetailDto(int Id, string Name, string Code, DiscountType Type, bool AllowStacking, int Priority, bool IsActive, List<DiscountVersionDto> Versions);
record DiscountVersionDto(int Id, int DiscountDefinitionId, DateTime ValidFrom, DateTime? ValidTo, bool IsPublished, List<DiscountRuleDto> Rules);
record DiscountRuleDto(int Id, int DiscountVersionId, DiscountTargetType TargetType, int TargetId, decimal Value, decimal? MinQuantity, decimal? MinOrderAmount, bool IsExclusive);
```

---

### Admin Blazor Pages

All pages: `@attribute [Authorize(Roles = "Admin")]`, `@rendermode InteractiveServer`.  
All dialogs use the project's custom CSS overlay pattern (`img-picker-overlay` / `img-picker-dialog`) consistent with existing pages (Categories, Brands, etc.).

#### VAT Rates
> `src/dWebShop.Admin/Components/Pages/VatRates/Index.razor`  
> Route: `/vat-rates`

- `FluentDataGrid` listing all VAT rates (name, rate, validity range, active status)
- **Add** button opens an overlay form: Name, Rate (%), Valid From date, Valid To date (optional), Active toggle
- **Edit** button pre-fills the same form
- **Delete** button opens a confirmation overlay before calling `DeleteVatRateCommand`

#### Payment Terms
> `src/dWebShop.Admin/Components/Pages/PaymentTerms/Index.razor`  
> Route: `/payment-terms`

- `FluentDataGrid` listing all payment terms (name, due days, cash discount summary, active)
- **Add / Edit** overlay: Name, Due Days, Cash Discount % (optional), Cash Discount Days (optional), Active
- **Delete** with confirmation overlay
- Cash discount column shows `"2.00% / 10d"` or `"—"` when absent

#### Discounts — List
> `src/dWebShop.Admin/Components/Pages/Discounts/Index.razor`  
> Route: `/discounts`

- `FluentDataGrid` with columns: Name, Code, Type, Priority, Stacking allowed, Version count, Active
- **Add** opens an overlay to create the `DiscountDefinition` (Name, Code, Type, Priority, AllowStacking, IsActive)
- After save, navigates to the new record's detail page automatically
- **Detail** button navigates to `/discounts/{id}`
- **Delete** cascades through all versions and rules (EF cascade delete)

#### Discounts — Detail
> `src/dWebShop.Admin/Components/Pages/Discounts/Detail.razor`  
> Route: `/discounts/{Id:int}`

The detail page is the primary management surface for a discount. It is structured in three levels:

**Header** — shows definition summary (name, code, type, priority, stacking, active badge). **Edit** button opens an overlay to update the `DiscountDefinition`.

**Versions section** — lists all `DiscountVersion` records ordered newest-first. Each version card shows:
- Validity range and Published/Draft badge
- **Edit** button → overlay with ValidFrom, ValidTo, IsPublished fields
- **Delete** button → confirmation overlay (cascades rules)
- **Add Rule** button (per version)

**Rules table** (inside each version card) — `FluentDataGrid` with columns: Target, Value, Min Qty, Min Order, Exclusive. Each row has Edit and Delete buttons.

**Rule overlay form** — TargetType (`FluentSelect` over `DiscountTargetType` enum), Target ID, Value, Min Quantity, Min Order Amount, IsExclusive checkbox.

---

### Navigation

> `src/dWebShop.Admin/Components/Layout/NavMenu.razor`

Three new links added to the **Commerce** section:

| Label | Route | Icon |
|---|---|---|
| VAT Rates | `/vat-rates` | % (percent) |
| Payment Terms | `/payment-terms` | calendar |
| Discounts | `/discounts` | tag (reuses existing `IcoTag`) |

Two new SVG `RenderFragment` icons added: `IcoPercent` and `IcoCalendar`.

---

### Implementation Notes

- **`FluentNumberField` does not exist in FluentUI v5.0.0-rc.2.** All numeric inputs use native `<input type="number" class="co-input">` with `<label class="co-label">`, matching the pattern used in `Orders/Create.razor`.
- **`FluentDatePicker`** is available and works correctly with `DateTime?` via `@bind-Value`.
- **`FluentSelect<TOption, TValue>`** with explicit two type parameters is used for enum dropdowns (`DiscountType`, `DiscountTargetType`).
- Dialog overlays use Blazor state booleans (`_dialogOpen`, `_deleteTarget`, etc.) rather than `IDialogService`, which is consistent with all other admin pages in the project.
