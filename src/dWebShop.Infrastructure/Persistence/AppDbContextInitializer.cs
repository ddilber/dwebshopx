using dWebShop.Domain.Entities.Inspirations;
using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace dWebShop.Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppDbContextInitializer> _logger;

    public AppDbContextInitializer(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration,
        ILogger<AppDbContextInitializer> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        await _context.Database.MigrateAsync();
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedBrandsAsync();
        await SeedCatalogAsync();
        await SeedDemoPartnerAsync();
        await SeedInspirationsAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { "Admin", "Client" })
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!result.Succeeded)
                    _logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = _configuration["Seed:AdminEmail"] ?? "admin@dwebshop.local";
        var adminPassword = _configuration["Seed:AdminPassword"] ?? "Admin@12345!";
        var adminUserName = _configuration["Seed:AdminUserName"] ?? "admin";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true,
            IsApproved = true
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedDemoPartnerAsync()
    {
        const string email = "marin@galicgradnja.ba";

        if (await _userManager.FindByEmailAsync(email) is not null)
            return;

        var partner = new Partner
        {
            FirstName = "Marin",
            LastName = "Galić",
            CompanyName = "Galić Gradnja d.o.o.",
            Email = email,
            Phone = string.Empty,
            PartnerType = "B2B Partner",
            Tier = "Silver — 8% rabat",
            CreatedDate = new DateTime(2019, 1, 1)
        };
        _context.Partners.Add(partner);
        await _context.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = "marin.galic",
            Email = email,
            EmailConfirmed = true,
            IsApproved = true,
            PartnerId = partner.Id
        };

        var result = await _userManager.CreateAsync(user, "Demo@12345!");
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, "Client");
        else
            _logger.LogError("Failed to create demo partner user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private async Task SeedBrandsAsync()
    {
        // Brand display data (Name, Slug, marketing Description) comes from
        // CatalogSeedData so it stays a single source of truth with the
        // catalog seed below.
        foreach (var seed in CatalogSeedData.Brands)
        {
            var existing = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == seed.Slug);
            if (existing is null)
            {
                _context.Brands.Add(new Brand
                {
                    Name = seed.Name,
                    Slug = seed.Slug,
                    Description = seed.Description,
                    LogoImage = string.Empty,
                    SliderImage = string.Empty,
                });
            }
            else if (string.IsNullOrEmpty(existing.Description) || existing.Description.EndsWith(" brand"))
            {
                // Upgrade the placeholder description left by an older seed
                // ("STO brand" / "XYPEX brand" / "CORTEC brand") without
                // overwriting any hand-edited copy from the Admin.
                existing.Description = seed.Description;
            }
        }

        await _context.SaveChangesAsync();
    }

    // One-shot seed of the public catalog: categories per brand, then products
    // with their option/sku graph, info rows, and document stubs. Idempotent —
    // skipped entirely once any product exists, so subsequent boots don't
    // touch catalog data the Admin may have edited.
    private async Task SeedCatalogAsync()
    {
        if (await _context.Products.AnyAsync())
            return;

        // Resolve brand ids — SeedBrandsAsync already ran so all three exist.
        var brandIdBySlug = await _context.Brands
            .ToDictionaryAsync(b => b.Slug, b => b.Id);

        // 1) Categories. Insert any that don't yet exist for (brandSlug, slug).
        var categoryIdBySlug = new Dictionary<(string brandSlug, string catSlug), int>();
        foreach (var (brandSlug, cats) in CatalogSeedData.CategoriesByBrand)
        {
            if (!brandIdBySlug.TryGetValue(brandSlug, out var brandId)) continue;

            foreach (var cat in cats)
            {
                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.BrandId == brandId && c.Slug == cat.Slug);
                if (existing is not null)
                {
                    categoryIdBySlug[(brandSlug, cat.Slug)] = existing.Id;
                    continue;
                }

                var entity = new Category
                {
                    Name = cat.Name,
                    Slug = cat.Slug,
                    Description = cat.Description,
                    BrandId = brandId,
                };
                _context.Categories.Add(entity);
                await _context.SaveChangesAsync();
                categoryIdBySlug[(brandSlug, cat.Slug)] = entity.Id;
            }
        }

        // 2) Tags. Deduplicate across all products and create as needed.
        var allTagNames = CatalogSeedData.Products
            .SelectMany(p => p.Tags)
            .Distinct()
            .ToList();

        var tagIdByName = new Dictionary<string, int>();
        foreach (var name in allTagNames)
        {
            var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (existing is not null)
            {
                tagIdByName[name] = existing.Id;
                continue;
            }
            var tag = new Tag { Name = name, Slug = Slugify(name) };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            tagIdByName[name] = tag.Id;
        }

        // 3) Products + their full graph. Build entities in-memory then save
        // once per product so EF can resolve internal FKs in the same SCSU.
        foreach (var seed in CatalogSeedData.Products)
        {
            if (!brandIdBySlug.TryGetValue(seed.BrandSlug, out var brandId)) continue;
            categoryIdBySlug.TryGetValue((seed.BrandSlug, seed.CategorySlug), out var categoryId);

            var product = new Product
            {
                Name = seed.Name,
                Slug = seed.Slug,
                SKU = seed.Slug.ToUpperInvariant(),
                ExtRef = string.Empty,
                Description = seed.Short,
                Status = ProductStatus.Active,
                IsFeatured = false,
                BrandId = brandId,
                ProductDetails = new ProductDetails
                {
                    DetailDescription = seed.Desc,
                    Information = seed.Info
                        .Select(i => new ProductInfo { Key = i.Key, Data = i.Value })
                        .ToList(),
                    Documents = seed.Docs
                        // Path is left blank — the legacy site never linked to
                        // real PDFs; Admin can attach files later.
                        .Select(d => new ProductDocument { Name = d.Name, Path = string.Empty, Description = d.Size })
                        .ToList(),
                    Images = [],
                },
                Categories = categoryId != 0
                    ? [_context.Categories.Local.First(c => c.Id == categoryId) ??
                       _context.Categories.First(c => c.Id == categoryId)]
                    : [],
                Tags = seed.Tags
                    .Where(t => tagIdByName.ContainsKey(t))
                    .Select(t => _context.Tags.Local.FirstOrDefault(x => x.Id == tagIdByName[t]) ??
                                 _context.Tags.First(x => x.Id == tagIdByName[t]))
                    .ToList(),
            };

            // Build option entities and capture their value entities so we
            // can wire them onto SKUs by position below.
            var optionEntities = new List<ProductOption>();
            var valueEntitiesByOption = new List<List<ProductOptionValue>>();
            foreach (var opt in seed.Options)
            {
                var valueEntities = opt.Values
                    .Select(v => new ProductOptionValue { Name = v })
                    .ToList();
                optionEntities.Add(new ProductOption
                {
                    Name = opt.Name,
                    IsNamePart = false,
                    ProductOptionValues = valueEntities,
                });
                valueEntitiesByOption.Add(valueEntities);
            }
            product.ProductOptions = optionEntities;

            // SKUs. Each SKU's Opts[] aligns positionally with seed.Options,
            // so look up the corresponding ProductOptionValue by index.
            var skuEntities = new List<ProductSku>();
            foreach (var sku in seed.Skus)
            {
                var skuOptionValues = new List<SkuOptionValue>();
                for (var i = 0; i < sku.Opts.Length && i < optionEntities.Count; i++)
                {
                    var pov = valueEntitiesByOption[i]
                        .FirstOrDefault(v => v.Name == sku.Opts[i]);
                    if (pov is null) continue;
                    skuOptionValues.Add(new SkuOptionValue
                    {
                        ProductOption = optionEntities[i],
                        ProductOptionValue = pov,
                    });
                }

                skuEntities.Add(new ProductSku
                {
                    SKU = string.Empty,
                    ExtRef = string.Empty,
                    Name = string.Join(" · ", sku.Opts),
                    Gtin = string.Empty,
                    Price = sku.Price,
                    Tax = 0m,
                    StockQuantity = CatalogSeedData.StockFromBucket(sku.Stock),
                    LowStockThreshold = 5,
                    Uom = sku.Uom,
                    SkuOptionValues = skuOptionValues,
                });
            }
            product.ProductSkus = skuEntities;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
    }

    // Compact slugifier for tag names — lowercases, strips diacritics, replaces
    // whitespace with dashes. Mirrors the legacy slug style used by ShopData.
    private static string Slugify(string text)
    {
        var lowered = text.ToLowerInvariant();
        var normalized = new System.Text.StringBuilder(lowered.Length);
        foreach (var ch in lowered.Normalize(System.Text.NormalizationForm.FormD))
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) normalized.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_') normalized.Append('-');
        }
        return System.Text.RegularExpressions.Regex.Replace(normalized.ToString(), "-+", "-").Trim('-');
    }

    private async Task SeedInspirationsAsync()
    {
        if (await _context.Inspirations.AnyAsync())
            return;

        var sto = await _context.Brands.FirstOrDefaultAsync(b => b.Slug == "sto");
        if (sto is null) return;

        static string Sections(object[] sections) =>
            JsonSerializer.Serialize(sections, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var articles = new[]
        {
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "izolacija-se-isplati",
                ContentType  = InspirationContentType.Story,
                IsFeatured   = true,
                Published    = true,
                Title        = "Izolacija se isplati",
                Lede         = "Ulaganje u fasadnu izolaciju vraća se kroz samo nekoliko sezona grijanja — pod uslovom da se sistem projektuje i ugrađuje kao cjelina, ne kao zbir komponenti.",
                HeroLabel    = "porodicna kuca / ETICS sistem",
                PublishedAt  = "2024-10-12",
                ReadMin      = 6,
                Authors      = "Tehnicki tim ASGifiks",
                Tags         = "Energetska efikasnost|Sanacija|ETICS",
                LinkedProductSlugs = "sto-armat-classic|sto-eps-100|sto-mw-035",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "Najcesce pitanje koje dobijemo na licu mjesta jeste: \"Koliko ce se izolacija stvarno isplatiti?\" Odgovor zavisi od cetiri varijable -- U-faktor zida, klimatska zona, tip grijanja i cijena energenata. Za prosjecnu porodicnu kucu u Hercegovini sa zidom bez izolacije, povrat ulaganja u 12 cm EPS-a krece se izmedju 6 i 9 sezona grijanja." },
                    new { type = "callout", label = "U-vrijednost", text = "Cilj kod sanacije u BiH klimi: U <= 0.25 W/m2K za vanjske zidove. Postize se sa 12-16 cm EPS-a klase 035-038 ili 10-14 cm mineralne vune." },
                    new { type = "heading", text = "Sta cini ETICS sustavom?" },
                    new { type = "paragraph", text = "ETICS (External Thermal Insulation Composite System) nije ime za izolacijsku plocu - nego za medusobno usaglaseni sklop sedam slojeva: ljepilo, izolacijska ploca, mehanicka fiksacija, armaturni mort, mrezica, temeljni premaz i zavrsna zbuka. Garancija na sistem vazi samo ako su sve komponente iz iste sistemske familije i pravilno ugradene." },
                    new { type = "paragraph", text = "To je razlog zasto STO sisteme isporucujemo kao komplet, sa specifikacijom svakog sloja. Sloj koji nedostaje ili je zamijenjen ekvivalentom iz drugog sistema ponistava garanciju proizvodaca na cijeli sklop." },
                    new { type = "heading", text = "EPS ili mineralna vuna?" },
                    new { type = "bulletlist", items = new[] { "EPS - niza cijena, lambda ~= 0.036, jednostavniji za rad. Najcesci izbor za stambene objekte do 22 m visine.", "MW (mineralna vuna) - visa cijena, lambda ~= 0.035, A1 negorivo. Obavezno na objektima preko 22m i u zonama povisenog pozarnog rizika.", "Hibridni sistemi - kombinacija MW pojaseva oko otvora i EPS na ostatku, cest izbor kod javnih objekata." } },
                    new { type = "paragraph", text = "Za vecinu nasih projekata u BiH, tacka prevoja gdje cijene EPS-a i MW-a postaju srodne je oko debljine 16 cm. Iznad toga MW se cesto isplati i zbog A1 klase i akusticnih svojstava." },
                }),
            },
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "bionicke-fasadne-boje",
                ContentType  = InspirationContentType.Story,
                IsFeatured   = true,
                Published    = true,
                Title        = "Bioničke fasadne boje",
                Lede         = "StoColor Dryonic — boja inspirisana kukcem iz pustinje Namib. Mikro-struktura površine usmjerava jutarnju rosu i kondenzaciju da otječu prije nego što stignu nahraniti alge.",
                HeroLabel    = "fasada nakon kise / dryonic",
                PublishedAt  = "2024-08-04",
                ReadMin      = 4,
                Authors      = "ASGifiks / STO Tehnicki",
                Tags         = "Zavrsni sloj|Anti-algae|Inovacija",
                LinkedProductSlugs = "sto-color-x",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "Pustinjski kukac Stenocara gracilipes prezivljava u Namibu zato sto njegova oklopna ledja imaju mikro-strukturu koja sakuplja jutarnju maglu - kapljice se skupljaju i kotrljaju direktno u usta. Inzenjeri u STO laboratoriji su tu istu logiku primijenili obratno: kako odvesti vodu sa fasade prije nego sto stigne nahraniti alge i gljivice." },
                    new { type = "callout", label = "Princip", text = "Hidrofilna mikro-struktura povrsine -- kapljice se ne odbijaju, vec se kapilarno povlace u smjeru gravitacije. Suse se u trecini vremena u odnosu na klasicnu silikat-disperzionu boju." },
                    new { type = "heading", text = "Sta to znaci u praksi" },
                    new { type = "paragraph", text = "Klasicna boja sa anti-algae aditivom radi tako sto biocidi ubijaju alge i postepeno ispiraju iz fasade tokom 3-5 godina. Dryonic logika je preventivna: nema vlage, nema alga, nema potrebe za biocidima. Manja toksicnost prema okolini, duzi vijek bez gubitka funkcije." },
                    new { type = "heading", text = "Kada ima smisla" },
                    new { type = "bulletlist", items = new[] { "Sjeverne i sjeverozapadne fasade gdje sunce slabo dolazi i vlaga se zadrzava.", "Objekti uz vodu, gustu vegetaciju ili u dolinama sa jutarnjom maglom.", "Renovacije gdje je prethodna fasada bila opterecena algama." } },
                    new { type = "paragraph", text = "Cijena po m2 je oko 15-20% visa od standardne silikat-disperzione boje, ali kada se uracuna izostanak pranja i obnove premaza, ravnopravna je vec nakon 8 godina." },
                }),
            },
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "produzenje-sezone-fasade",
                ContentType  = InspirationContentType.Story,
                IsFeatured   = false,
                Published    = true,
                Title        = "Produženje sezone — žbukanje u jesen i ranu zimu",
                Lede         = "STO QS tehnologija dopušta ugradnju ETICS sistema do +1°C. Praktične smjernice kako iskoristiti hladne mjesece bez kompromisa kvalitete.",
                HeroLabel    = "fasada / jesenji rad",
                PublishedAt  = "2024-09-21",
                ReadMin      = 5,
                Authors      = "Tehnicki tim ASGifiks",
                Tags         = "Sezona|Aplikacija|Logistika",
                LinkedProductSlugs = "sto-armat-classic|sto-deco-color",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "Standardna pravila ETICS aplikacije propisuju temperaturu okruzenja i podloge >= +5C tokom rada i 48h nakon. U BiH klimi to znaci da se sezona za fasade prakticno zavrsava u prvoj polovini novembra i tek otvara opet u martu. Za izvodace sa kapacitetom, to su tri izgubljena mjeseca u godini." },
                    new { type = "callout", label = "QS klasa", text = "Quick & Strong komponente (StoLevell QS, StoColor QS) mogu se ugradivati na temperaturama do +1C i daju 80% finalne snage vec nakon 6 sati. Cijena u proracunu projekta cca 8-12% visa od standardne klase." },
                    new { type = "heading", text = "Sta se mijenja u praksi" },
                    new { type = "bulletlist", items = new[] { "Ljepilo: StoLevell Uni Plus -> StoLevell QS - radi do +1C.", "Zavrsna zbuka: standardna silikatna -> StoSilco K QS - mraz tolerantna.", "Boja: ako se aplicira na finale -> StoColor QS.", "Vremenski raspored: kraci prozor susenja, fiksiranje na drugu jutarnju smjenu." } },
                    new { type = "paragraph", text = "Najveca greska u praksi je miks - QS armatura sa standardnom zavrsnom zbukom. To razbija temperaturni prozor sistema i ponistava garanciju. Ili cijeli QS sklop, ili klasican." },
                }),
            },
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "stosignature-zbukane-povrsine",
                ContentType  = InspirationContentType.Collection,
                IsFeatured   = true,
                Published    = true,
                Title        = "StoSignature — kreativne žbukane površine",
                Lede         = "Pet definisanih efekata završne žbuke — od metalnog sjaja do efekta ručno obrađenog betona. Sa specifikacijom alata, slojeva i tehnike za svaki.",
                HeroLabel    = "unutrasnja zbuka / signature",
                PublishedAt  = "2024-06-15",
                ReadMin      = 7,
                Authors      = "StoDesign / ASGifiks",
                Tags         = "Interijer|Zavrsni sloj|Dizajn",
                LinkedProductSlugs = "sto-deco-color",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "StoSignature je linija pet efekata zavrsne zbuke namijenjena enterijerima gdje zid postaje karakter prostorije, a ne pozadina. Svaki efekat ima propisanu kombinaciju materijala, alata i broja slojeva, pa je rezultat reproducibilan izmedju projekata." },
                    new { type = "heading", text = "Pet efekata" },
                    new { type = "bulletlist", items = new[] { "Calce Metallica - metalni sjaj, podsjeca na patiniranu mjed.", "Calce Concreto - efekat sirovog betona sa kontrolisanim mrljama.", "Calce Tadelakt - glatka mediteranska zbuka, vodoodbojna.", "Calce Spatolato - slojeviti selak finish za velike povrsine.", "Calce Travertino - efekat prirodnog kamena, sa varijacijama tona." } },
                    new { type = "callout", label = "Aplikator", text = "StoSignature sistemi se isporucuju samo certificiranim aplikatorima. Debljine slojeva, vrijeme izmedju obrada i alat su dio garancije." },
                    new { type = "paragraph", text = "Cijena po m2 za StoSignature krece se izmedju 38 i 95 KM, ovisno o efektu i povrsini. Najpopularniji u BiH stambenim projektima su Calce Concreto i Tadelakt." },
                }),
            },
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "referenca-hotel-neum",
                ContentType  = InspirationContentType.Reference,
                IsFeatured   = false,
                Published    = true,
                Title        = "Hotelski kompleks Neum — 5.500 m² STO fasade",
                Lede         = "Sanacija postojeće fasade i nove faze izgradnje — kompletan STO ETICS sistem sa dekorativnom žbukom u dvije nijanse. Trogodišnja garancija na sklop.",
                HeroLabel    = "hotel neum / fasada",
                PublishedAt  = "2023-11-30",
                ReadMin      = 5,
                Authors      = "Projekt: Galic Gradnja / ASGifiks",
                Tags         = "Hotel|Sanacija|Velika povrsina",
                LinkedProductSlugs = "sto-armat-classic|sto-deco-color|sto-color-x|sto-mw-035",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "Projekat je obuhvatao dvije etape: sanaciju postojeceg krila iz 1980-ih (oko 3.000 m2) i kompletnu fasadu nove dogradnje (2.500 m2). Klimatska izlozenost - primorska, sa visokom relativnom vlaznocu i soli u zraku - diktirala je izbor sistema otpornog na alge i biokoroziju." },
                    new { type = "callout", label = "Specifikacija", text = "STO ETICS / MW 16 cm (visina > 22m) / StoArmat Classic + mrezica / StoSilco K 1.5mm / StoColor Dryonic zavrsni premaz u dvije RAL nijanse." },
                    new { type = "heading", text = "Logisticki izazovi" },
                    new { type = "paragraph", text = "Velika narudzba mineralne vune (preko 400 paleta) zahtijevala je faznu isporuku sa skladista u Tomislavgradu - ukupno 14 dostava tokom 11 sedmica. Koordinacija sa izvodacima pratila je dinamiku gradevinskih radova." },
                    new { type = "heading", text = "Rezultat" },
                    new { type = "bulletlist", items = new[] { "Zavrseno u roku - 22 mjeseca od pocetka do predaje fasade.", "0 reklamacija sistema u prve dvije godine eksploatacije.", "Energetski razred objekta: B+ (prije sanacije E)." } },
                }),
            },
            new Inspiration
            {
                BrandId      = sto.Id,
                Slug         = "referenca-skola-tomislavgrad",
                ContentType  = InspirationContentType.Reference,
                IsFeatured   = false,
                Published    = true,
                Title        = "Osnovna škola Tomislavgrad — energetska obnova",
                Lede         = "Javni objekat iz 1972. — kompletna toplinska sanacija sa STO ETICS EPS sistemom. Dvostruka godišnja ušteda na grijanju već u prvoj sezoni.",
                HeroLabel    = "skola / fasadni rad",
                PublishedAt  = "2023-08-12",
                ReadMin      = 4,
                Authors      = "Projekt: ASGifiks tehnicki nadzor",
                Tags         = "Javni objekti|Sanacija|Energetska efikasnost",
                LinkedProductSlugs = "sto-armat-classic|sto-eps-100",
                Content      = Sections(new object[]
                {
                    new { type = "paragraph", text = "Skola iz 1972. - 2.600 m2 neizolirane betonske fasade, zidovi U ~= 1.4 W/m2K. Cilj projekta: spustiti potrosnju energije za grijanje barem 40%, uz zadrzavanje arhitektonskog karaktera objekta." },
                    new { type = "callout", label = "Specifikacija", text = "STO ETICS / EPS 15 cm klase 035 / StoArmat Classic / StoSilco K 2mm / StoColor X u dvije nijanse (toplo siva i bjelokost)." },
                    new { type = "paragraph", text = "Tokom radova skola je radila - sve aktivnosti su bile organizovane van smjena, sa zastitnim platnima ispred prozora ucionica. Posebna paznja na detalje oko prozora i nadstresnica." },
                    new { type = "heading", text = "Rezultat - prva zima" },
                    new { type = "bulletlist", items = new[] { "Potrosnja prirodnog gasa: -47% u odnosu na prosjek prethodnih 3 godine.", "Investicija: 168.000 KM. Procijenjeni povrat: 7 godina.", "Energetski razred: D => B." } },
                }),
            },
        };

        _context.Inspirations.AddRange(articles);
        await _context.SaveChangesAsync();
    }
}
