namespace dWebShop.Web;

public static class InspirationData
{
    public enum ContentType { Story, Reference, Collection }

    public record ContentTypeInfo(string Label, string Plural, string Desc);

    public static readonly IReadOnlyDictionary<ContentType, ContentTypeInfo> Types =
        new Dictionary<ContentType, ContentTypeInfo>
        {
            [ContentType.Story]      = new("Priča",     "Priče",     "Edukativni i inspiracijski članci"),
            [ContentType.Reference]  = new("Referenca", "Reference", "Realizovani projekti sa fotografijama"),
            [ContentType.Collection] = new("Kolekcija", "Kolekcije", "Sistemska rješenja i dizajn linije"),
        };

    public abstract record Section;
    public record Paragraph(string Text) : Section;
    public record Heading(string Text) : Section;
    public record BulletList(string[] Items) : Section;
    public record Callout(string Label, string Text) : Section;

    public record InspirationItem(
        string Slug,
        ContentType Type,
        bool Featured,
        string Title,
        string Lede,
        string HeroLabel,
        string PublishedAt,
        int ReadMin,
        string[] Authors,
        string[] Tags,
        string[] LinkedProductSlugs,
        Section[] Sections
    );

    public static readonly IReadOnlyDictionary<string, InspirationItem[]> Content =
        new Dictionary<string, InspirationItem[]>
        {
            ["sto"] =
            [
                new(
                    Slug: "izolacija-se-isplati",
                    Type: ContentType.Story,
                    Featured: true,
                    Title: "Izolacija se isplati",
                    Lede: "Ulaganje u fasadnu izolaciju vraća se kroz samo nekoliko sezona grijanja — pod uslovom da se sistem projektuje i ugrađuje kao cjelina, ne kao zbir komponenti.",
                    HeroLabel: "porodicna kuca / ETICS sistem",
                    PublishedAt: "2024-10-12",
                    ReadMin: 6,
                    Authors: ["Tehnicki tim ASGifiks"],
                    Tags: ["Energetska efikasnost", "Sanacija", "ETICS"],
                    LinkedProductSlugs: ["sto-armat-classic", "sto-eps-100", "sto-mw-035"],
                    Sections:
                    [
                        new Paragraph("Najcesce pitanje koje dobijemo na licu mjesta jeste: \"Koliko ce se izolacija stvarno isplatiti?\" Odgovor zavisi od cetiri varijable -- U-faktor zida, klimatska zona, tip grijanja i cijena energenata. Za prosjecnu porodicnu kucu u Hercegovini sa zidom bez izolacije, povrat ulaganja u 12 cm EPS-a krece se izmedju 6 i 9 sezona grijanja."),
                        new Callout("U-vrijednost", "Cilj kod sanacije u BiH klimi: U <= 0.25 W/m2K za vanjske zidove. Postize se sa 12-16 cm EPS-a klase 035-038 ili 10-14 cm mineralne vune."),
                        new Heading("Sta cini ETICS sustavom?"),
                        new Paragraph("ETICS (External Thermal Insulation Composite System) nije ime za izolacijsku plocu - nego za medusobno usaglaseni sklop sedam slojeva: ljepilo, izolacijska ploca, mehanicka fiksacija, armaturni mort, mrezica, temeljni premaz i zavrsna zbuka. Garancija na sistem vazi samo ako su sve komponente iz iste sistemske familije i pravilno ugradene."),
                        new Paragraph("To je razlog zasto STO sisteme isporucujemo kao komplet, sa specifikacijom svakog sloja. Sloj koji nedostaje ili je zamijenjen ekvivalentom iz drugog sistema ponistava garanciju proizvodaca na cijeli sklop."),
                        new Heading("EPS ili mineralna vuna?"),
                        new BulletList(
                        [
                            "EPS - niza cijena, lambda ~= 0.036, jednostavniji za rad. Najcesci izbor za stambene objekte do 22 m visine.",
                            "MW (mineralna vuna) - visa cijena, lambda ~= 0.035, A1 negorivo. Obavezno na objektima preko 22m i u zonama povisenog pozarnog rizika.",
                            "Hibridni sistemi - kombinacija MW pojaseva oko otvora i EPS na ostatku, cest izbor kod javnih objekata.",
                        ]),
                        new Paragraph("Za vecinu nasih projekata u BiH, tacka prevoja gdje cijene EPS-a i MW-a postaju srodne je oko debljine 16 cm. Iznad toga MW se cesto isplati i zbog A1 klase i akusticnih svojstava."),
                    ]
                ),

                new(
                    Slug: "bionicke-fasadne-boje",
                    Type: ContentType.Story,
                    Featured: true,
                    Title: "Bioničke fasadne boje",
                    Lede: "StoColor Dryonic — boja inspirisana kukcem iz pustinje Namib. Mikro-struktura površine usmjerava jutarnju rosu i kondenzaciju da otječu prije nego što stignu nahraniti alge.",
                    HeroLabel: "fasada nakon kise / dryonic",
                    PublishedAt: "2024-08-04",
                    ReadMin: 4,
                    Authors: ["ASGifiks / STO Tehnicki"],
                    Tags: ["Zavrsni sloj", "Anti-algae", "Inovacija"],
                    LinkedProductSlugs: ["sto-color-x"],
                    Sections:
                    [
                        new Paragraph("Pustinjski kukac Stenocara gracilipes prezivljava u Namibu zato sto njegova oklopna ledja imaju mikro-strukturu koja sakuplja jutarnju maglu - kapljice se skupljaju i kotrljaju direktno u usta. Inzenjeri u STO laboratoriji su tu istu logiku primijenili obratno: kako odvesti vodu sa fasade prije nego sto stigne nahraniti alge i gljivice."),
                        new Callout("Princip", "Hidrofilna mikro-struktura povrsine -- kapljice se ne odbijaju, vec se kapilarno povlace u smjeru gravitacije. Suse se u trecini vremena u odnosu na klasicnu silikat-disperzionu boju."),
                        new Heading("Sta to znaci u praksi"),
                        new Paragraph("Klasicna boja sa anti-algae aditivom radi tako sto biocidi ubijaju alge i postepeno ispiraju iz fasade tokom 3-5 godina. Dryonic logika je preventivna: nema vlage, nema alga, nema potrebe za biocidima. Manja toksicnost prema okolini, duzi vijek bez gubitka funkcije."),
                        new Heading("Kada ima smisla"),
                        new BulletList(
                        [
                            "Sjeverne i sjeverozapadne fasade gdje sunce slabo dolazi i vlaga se zadrzava.",
                            "Objekti uz vodu, gustu vegetaciju ili u dolinama sa jutarnjom maglom.",
                            "Renovacije gdje je prethodna fasada bila opterecena algama.",
                        ]),
                        new Paragraph("Cijena po m2 je oko 15-20% visa od standardne silikat-disperzione boje, ali kada se uracuna izostanak pranja i obnove premaza, ravnopravna je vec nakon 8 godina."),
                    ]
                ),

                new(
                    Slug: "produzenje-sezone-fasade",
                    Type: ContentType.Story,
                    Featured: false,
                    Title: "Produženje sezone — žbukanje u jesen i ranu zimu",
                    Lede: "STO QS tehnologija dopušta ugradnju ETICS sistema do +1°C. Praktične smjernice kako iskoristiti hladne mjesece bez kompromisa kvalitete.",
                    HeroLabel: "fasada / jesenji rad",
                    PublishedAt: "2024-09-21",
                    ReadMin: 5,
                    Authors: ["Tehnicki tim ASGifiks"],
                    Tags: ["Sezona", "Aplikacija", "Logistika"],
                    LinkedProductSlugs: ["sto-armat-classic", "sto-deco-color"],
                    Sections:
                    [
                        new Paragraph("Standardna pravila ETICS aplikacije propisuju temperaturu okruzenja i podloge >= +5C tokom rada i 48h nakon. U BiH klimi to znaci da se sezona za fasade prakticno zavrsava u prvoj polovini novembra i tek otvara opet u martu. Za izvodace sa kapacitetom, to su tri izgubljena mjeseca u godini."),
                        new Callout("QS klasa", "Quick & Strong komponente (StoLevell QS, StoColor QS) mogu se ugradivati na temperaturama do +1C i daju 80% finalne snage vec nakon 6 sati. Cijena u proracunu projekta cca 8-12% visa od standardne klase."),
                        new Heading("Sta se mijenja u praksi"),
                        new BulletList(
                        [
                            "Ljepilo: StoLevell Uni Plus -> StoLevell QS - radi do +1C.",
                            "Zavrsna zbuka: standardna silikatna -> StoSilco K QS - mraz tolerantna.",
                            "Boja: ako se aplicira na finale -> StoColor QS.",
                            "Vremenski raspored: kraci prozor susenja, fiksiranje na drugu jutarnju smjenu.",
                        ]),
                        new Paragraph("Najveca greska u praksi je miks - QS armatura sa standardnom zavrsnom zbukom. To razbija temperaturni prozor sistema i ponistava garanciju. Ili cijeli QS sklop, ili klasican."),
                    ]
                ),

                new(
                    Slug: "stosignature-zbukane-povrsine",
                    Type: ContentType.Collection,
                    Featured: true,
                    Title: "StoSignature — kreativne žbukane površine",
                    Lede: "Pet definisanih efekata završne žbuke — od metalnog sjaja do efekta ručno obrađenog betona. Sa specifikacijom alata, slojeva i tehnike za svaki.",
                    HeroLabel: "unutrasnja zbuka / signature",
                    PublishedAt: "2024-06-15",
                    ReadMin: 7,
                    Authors: ["StoDesign / ASGifiks"],
                    Tags: ["Interijer", "Zavrsni sloj", "Dizajn"],
                    LinkedProductSlugs: ["sto-deco-color"],
                    Sections:
                    [
                        new Paragraph("StoSignature je linija pet efekata zavrsne zbuke namijenjena enterijerima gdje zid postaje karakter prostorije, a ne pozadina. Svaki efekat ima propisanu kombinaciju materijala, alata i broja slojeva, pa je rezultat reproducibilan izmedju projekata."),
                        new Heading("Pet efekata"),
                        new BulletList(
                        [
                            "Calce Metallica - metalni sjaj, podsjeca na patiniranu mjed.",
                            "Calce Concreto - efekat sirovog betona sa kontrolisanim mrljama.",
                            "Calce Tadelakt - glatka mediteranska zbuka, vodoodbojna.",
                            "Calce Spatolato - slojeviti selak finish za velike povrsine.",
                            "Calce Travertino - efekat prirodnog kamena, sa varijacijama tona.",
                        ]),
                        new Callout("Aplikator", "StoSignature sistemi se isporucuju samo certificiranim aplikatorima. Debljine slojeva, vrijeme izmedju obrada i alat su dio garancije."),
                        new Paragraph("Cijena po m2 za StoSignature krece se izmedju 38 i 95 KM, ovisno o efektu i povrsini. Najpopularniji u BiH stambenim projektima su Calce Concreto i Tadelakt."),
                    ]
                ),

                new(
                    Slug: "referenca-hotel-neum",
                    Type: ContentType.Reference,
                    Featured: false,
                    Title: "Hotelski kompleks Neum — 5.500 m² STO fasade",
                    Lede: "Sanacija postojeće fasade i nove faze izgradnje — kompletan STO ETICS sistem sa dekorativnom žbukom u dvije nijanse. Trogodišnja garancija na sklop.",
                    HeroLabel: "hotel neum / fasada",
                    PublishedAt: "2023-11-30",
                    ReadMin: 5,
                    Authors: ["Projekt: Galic Gradnja / ASGifiks"],
                    Tags: ["Hotel", "Sanacija", "Velika povrsina"],
                    LinkedProductSlugs: ["sto-armat-classic", "sto-deco-color", "sto-color-x", "sto-mw-035"],
                    Sections:
                    [
                        new Paragraph("Projekat je obuhvatao dvije etape: sanaciju postojeceg krila iz 1980-ih (oko 3.000 m2) i kompletnu fasadu nove dogradnje (2.500 m2). Klimatska izlozenost - primorska, sa visokom relativnom vlaznocu i soli u zraku - diktirala je izbor sistema otpornog na alge i biokoroziju."),
                        new Callout("Specifikacija", "STO ETICS / MW 16 cm (visina > 22m) / StoArmat Classic + mrezica / StoSilco K 1.5mm / StoColor Dryonic zavrsni premaz u dvije RAL nijanse."),
                        new Heading("Logisticki izazovi"),
                        new Paragraph("Velika narudzba mineralne vune (preko 400 paleta) zahtijevala je faznu isporuku sa skladista u Tomislavgradu - ukupno 14 dostava tokom 11 sedmica. Koordinacija sa izvodacima pratila je dinamiku gradevinskih radova."),
                        new Heading("Rezultat"),
                        new BulletList(
                        [
                            "Zavrseno u roku - 22 mjeseca od pocetka do predaje fasade.",
                            "0 reklamacija sistema u prve dvije godine eksploatacije.",
                            "Energetski razred objekta: B+ (prije sanacije E).",
                        ]),
                    ]
                ),

                new(
                    Slug: "referenca-skola-tomislavgrad",
                    Type: ContentType.Reference,
                    Featured: false,
                    Title: "Osnovna škola Tomislavgrad — energetska obnova",
                    Lede: "Javni objekat iz 1972. — kompletna toplinska sanacija sa STO ETICS EPS sistemom. Dvostruka godišnja ušteda na grijanju već u prvoj sezoni.",
                    HeroLabel: "skola / fasadni rad",
                    PublishedAt: "2023-08-12",
                    ReadMin: 4,
                    Authors: ["Projekt: ASGifiks tehnicki nadzor"],
                    Tags: ["Javni objekti", "Sanacija", "Energetska efikasnost"],
                    LinkedProductSlugs: ["sto-armat-classic", "sto-eps-100"],
                    Sections:
                    [
                        new Paragraph("Skola iz 1972. - 2.600 m2 neizolirane betonske fasade, zidovi U ~= 1.4 W/m2K. Cilj projekta: spustiti potrosnju energije za grijanje barem 40%, uz zadrzavanje arhitektonskog karaktera objekta."),
                        new Callout("Specifikacija", "STO ETICS / EPS 15 cm klase 035 / StoArmat Classic / StoSilco K 2mm / StoColor X u dvije nijanse (toplo siva i bjelokost)."),
                        new Paragraph("Tokom radova skola je radila - sve aktivnosti su bile organizovane van smjena, sa zastitnim platnima ispred prozora ucionica. Posebna paznja na detalje oko prozora i nadstresnica."),
                        new Heading("Rezultat - prva zima"),
                        new BulletList(
                        [
                            "Potrosnja prirodnog gasa: -47% u odnosu na prosjek prethodnih 3 godine.",
                            "Investicija: 168.000 KM. Procijenjeni povrat: 7 godina.",
                            "Energetski razred: D => B.",
                        ]),
                    ]
                ),
            ],

            ["xypex"] = [],
            ["cortec"] = [],
        };

    public static InspirationItem? Find(string brandKey, string slug) =>
        Content.GetValueOrDefault(brandKey)?.FirstOrDefault(x => x.Slug == slug);

    public static InspirationItem[] ForBrand(string brandKey) =>
        Content.GetValueOrDefault(brandKey) ?? [];

    public static InspirationItem[] ByType(string brandKey, ContentType type) =>
        ForBrand(brandKey).Where(x => x.Type == type).ToArray();

    public static InspirationItem[] Featured(string brandKey) =>
        ForBrand(brandKey).Where(x => x.Featured).ToArray();
}
