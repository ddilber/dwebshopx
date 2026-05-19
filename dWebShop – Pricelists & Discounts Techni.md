dWebShop – Pricelists & Discounts Technical Design
Overview

The dWebShop pricing system is designed for B2B wholesale scenarios with:

Customer-specific pricing
Multiple pricelists
Product/group/category discounts
Quantity discounts
Order-level discounts
Cash discount (kasa-skonto)
Versioned pricing and discount rules
Historical order consistency
Multi-tenant/organisation-ready architecture

The goal is to provide a deterministic and auditable pricing engine that can support enterprise-grade ERP and webshop scenarios.

Core Requirements
Functional Requirements
Pricing

The system must support:

Global product base price
Customer-specific pricelist
Customer-specific product override price
Product category pricing
Quantity tiers
Time-limited prices
Currency support
VAT handling
Discounts

The system must support:

Product discounts
Product group/category discounts
Customer discounts
Quantity discounts
Order total discounts
Promotional discounts
Payment-term cash discounts (skonto)
Validity periods
Priority and stacking rules
High-Level Architecture
Product Base Price
        ↓
Customer Pricelist
        ↓
Customer Product Override
        ↓
Item Discounts
        ↓
Quantity Discounts
        ↓
Order Discounts
        ↓
Cash Discount (informational/payment)
        ↓
Final Price
Core Domain Model
Product
class Product
{
    Guid Id;
    string Code;
    string Name;

    decimal BasePrice;
    Guid VatRateId;

    Guid? ProductGroupId;
    Guid? CategoryId;

    bool IsActive;
}
Customer
class Customer
{
    Guid Id;

    string Code;
    string Name;

    Guid? DefaultPriceListId;
    Guid? PaymentTermsId;

    bool IsActive;
}
Pricelist Model
PriceList

Represents logical pricelist identity.

class PriceList
{
    Guid Id;

    string Name;
    string Code;

    bool IsDefault;
    bool IsActive;
}
PriceListVersion

Versioned snapshot of pricelist.

class PriceListVersion
{
    Guid Id;

    Guid PriceListId;

    int VersionNumber;

    DateTime ValidFrom;
    DateTime? ValidTo;

    bool IsPublished;
}
PriceListItem
class PriceListItem
{
    Guid Id;

    Guid PriceListVersionId;
    Guid ProductId;

    decimal Price;

    decimal? MinQuantity;

    string Currency;

    DateTime ValidFrom;
    DateTime? ValidTo;
}
Customer Product Override

Highest priority direct customer price.

class CustomerProductPrice
{
    Guid Id;

    Guid CustomerId;
    Guid ProductId;

    decimal Price;

    decimal? MinQuantity;

    DateTime ValidFrom;
    DateTime? ValidTo;
}
Discount System
DiscountDefinition

Logical discount identity.

class DiscountDefinition
{
    Guid Id;

    string Name;
    string Code;

    DiscountType Type;

    bool AllowStacking;

    int Priority;
}
DiscountVersion

Versioned rules.

class DiscountVersion
{
    Guid Id;

    Guid DiscountDefinitionId;

    DateTime ValidFrom;
    DateTime? ValidTo;

    bool IsPublished;
}
Discount Targets
Supported Targets
enum DiscountTargetType
{
    Product,
    ProductGroup,
    Category,
    Customer,
    Order,
    PaymentTerm
}
Discount Types
enum DiscountType
{
    Percentage,
    FixedAmount,
    FixedPrice
}
Discount Rule
class DiscountRule
{
    Guid Id;

    Guid DiscountVersionId;

    DiscountTargetType TargetType;

    Guid TargetId;

    decimal Value;

    decimal? MinQuantity;
    decimal? MinOrderAmount;

    bool IsExclusive;
}
Payment Terms / Cash Discount
PaymentTerms
class PaymentTerms
{
    Guid Id;

    string Name;

    int DueDays;

    decimal? CashDiscountPercent;
    int? CashDiscountDays;
}

Example:

Condition	Value
Due date	30 days
Cash discount	2%
Valid if paid within	7 days
VAT Versioning
VatRate
class VatRate
{
    Guid Id;

    string Name;

    decimal Rate;

    DateTime ValidFrom;
    DateTime? ValidTo;
}

This ensures historical invoices remain correct after VAT changes.

Price Resolution Algorithm
Base Price Resolution

Priority order:

CustomerProductPrice
PriceListItem
Product.BasePrice
Discount Resolution
Item-Level

Apply in order:

Product-specific discount
Product group discount
Category discount
Customer discount
Quantity discount
Order-Level

After line totals:

Order total discount
Promotional order discount
Cash discount (optional informational)
Stacking Rules
Exclusive Discounts

If IsExclusive == true:

Stop further discount processing
Highest priority wins
Stackable Discounts

If allowed:

100.00
-10%
=90.00

-5%
=85.50

Sequential calculation preserves accounting correctness.

Historical Snapshot Strategy

Orders must remain immutable.

Order Snapshot
class OrderLineSnapshot
{
    decimal BasePrice;

    decimal FinalPrice;

    decimal VatRate;

    string AppliedRulesJson;
}

Never recalculate historical orders from live pricing tables.

Recommended Pricing Engine Design
Service Interface
public interface IPricingEngine
{
    Task<PricingResult> CalculateAsync(
        PricingRequest request);
}
Pricing Request
class PricingRequest
{
    Guid CustomerId;

    List<PricingRequestLine> Lines;

    DateTime PricingDate;
}
Pricing Result
class PricingResult
{
    decimal TotalBeforeDiscount;

    decimal TotalDiscount;

    decimal TotalAfterDiscount;

    decimal VatTotal;

    decimal GrandTotal;

    List<PricedLine> Lines;
}
Pricing Engine Pipeline
Recommended Flow
Load Customer
    ↓
Resolve Payment Terms
    ↓
Resolve Pricelist
    ↓
Resolve Base Prices
    ↓
Apply Item Discounts
    ↓
Apply Quantity Discounts
    ↓
Apply Order Discounts
    ↓
Calculate VAT
    ↓
Generate Snapshots
Performance Considerations
Important

Pricing systems become expensive quickly.

Recommended:

Precompiled pricing cache
In-memory rule indexes
Version snapshots
Batched product loading
Redis cache for active pricelists
Deterministic ordering
Recommended Database Indexes
Critical Indexes
PriceListItem
(ProductId, PriceListVersionId, MinQuantity)
CustomerProductPrice
(CustomerId, ProductId, MinQuantity)
DiscountRule
(TargetType, TargetId)
Multi-Tenant / Organisation Support

Recommended structure:

class OrganisationOwnedEntity
{
    Guid OrganisationId;
}

All pricing entities should inherit or contain OrganisationId.

This enables:

isolated customer pricing
separate rule sets
distributor-specific pricing
franchise scenarios
Audit & Traceability

Every pricing calculation should produce:

applied rules
skipped rules
priority resolution
discount chain
timestamps
pricing version ids

This is critical for:

accounting
legal disputes
ERP integrations
invoice reproduction
Recommended Future Extensions
Promotions
Buy X get Y
Bundle pricing
Campaign engine
Coupon codes
Advanced Pricing
Margin rules
Supplier-based pricing
Dynamic pricing
AI-assisted pricing
Region-based pricing
Recommended Technology Stack

Given existing dWebShop architecture:

ASP.NET Core
Blazor
EF Core
MediatR
PostgreSQL or MySQL
Redis cache
Background recalculation workers
Suggested Folder Structure
dWebShop.Domain
    Pricing/
    Discounts/
    Orders/

dWebShop.Application
    Pricing/
    Discounts/

dWebShop.Infrastructure
    Persistence/
    PricingCache/

dWebShop.Web
    Components/
    Admin/
Conclusion

The proposed design provides:

deterministic pricing
historical consistency
enterprise scalability
flexible discount modeling
auditable calculations
multi-organisation support
future ERP compatibility

The key architectural principle is:

Prices and discounts are versioned business rules, while orders are immutable historical snapshots.