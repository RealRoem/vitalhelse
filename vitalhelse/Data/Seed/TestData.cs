using VitalHelse.Models;
using VitalHelse.Models.Shipping;

namespace VitalHelse.Data;

/// <summary>
/// Provides test and demo data for the VitalHelse application.
/// This class should only be executed in Development environments
/// to prevent accidental seeding of test data into production.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Initializes the database with categories and products if empty.
    /// </summary>
    /// <param name="db">Database context for the application.</param>
    /// <param name="env">Environment context (used to prevent execution in production).</param>
    public static void Initialize(ApplicationDbContext db, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return; // Prevent accidental seeding in production

        db.Database.EnsureCreated();

        if (!db.Categories.Any())
        {
            SeedCategories(db);
            SeedProducts(db);
            SeedShipping(db);
        }
    }


    // =========================================================
    //                 KATEGORI-SEEDING (RENT HIERARKI)
    // =========================================================
    /// <summary>
    /// Seeds a hierarchical category structure for testing.
    /// </summary>
    private static void SeedCategories(ApplicationDbContext db)
    {
        // --------------------------
        // 1️⃣ Hovedkategorier
        // --------------------------
        var hudpleie = new Category { CategoryName = "hudpleie" };
        var aloeVera = new Category { CategoryName = "aloe-vera" };
        var hygiene = new Category { CategoryName = "hygiene" };
        var massasje = new Category { CategoryName = "massasje" };
        var tilbud = new Category { CategoryName = "tilbud" };

        db.Categories.AddRange(hudpleie, aloeVera, hygiene, massasje, tilbud);
        db.SaveChanges();

        // --------------------------
        // 2️⃣ Hudpleie
        // --------------------------
        var hudpleieUnder = new List<Category>
        {
            new() { CategoryName = "ansikt", ParentCategoryId = hudpleie.CategoryId },
            new() { CategoryName = "hender", ParentCategoryId = hudpleie.CategoryId },
            new() { CategoryName = "føtter", ParentCategoryId = hudpleie.CategoryId },
            new() { CategoryName = "kropp", ParentCategoryId = hudpleie.CategoryId },
            new() { CategoryName = "serum", ParentCategoryId = hudpleie.CategoryId }
        };
        db.Categories.AddRange(hudpleieUnder);

        // --------------------------
        // 3️⃣ Aloe Vera
        // --------------------------
        var aloeUnder = new List<Category>
        {
            new() { CategoryName = "hudpleie", ParentCategoryId = aloeVera.CategoryId },
            new() { CategoryName = "massasje", ParentCategoryId = aloeVera.CategoryId },
            new() { CategoryName = "sport", ParentCategoryId = aloeVera.CategoryId },
            new() { CategoryName = "etter-sol", ParentCategoryId = aloeVera.CategoryId }
        };
        db.Categories.AddRange(aloeUnder);

        // --------------------------
        // 4️⃣ Hygiene
        // --------------------------
        var hygieneUnder = new List<Category>
        {
            new() { CategoryName = "desinfeksjon", ParentCategoryId = hygiene.CategoryId },
            new() { CategoryName = "overflatevask", ParentCategoryId = hygiene.CategoryId },
            new() { CategoryName = "dispensere", ParentCategoryId = hygiene.CategoryId },
            new() { CategoryName = "såpe-og-påfyll", ParentCategoryId = hygiene.CategoryId }
        };
        db.Categories.AddRange(hygieneUnder);

        // --------------------------
        // 5️⃣ Massasje
        // --------------------------
        var massasjeUnder = new List<Category>
        {
            new() { CategoryName = "massasjeolje", ParentCategoryId = massasje.CategoryId },
            new() { CategoryName = "kjøleprodukter", ParentCategoryId = massasje.CategoryId },
            new() { CategoryName = "varmeprodukter", ParentCategoryId = massasje.CategoryId },
            new() { CategoryName = "muskelpleie", ParentCategoryId = massasje.CategoryId }
        };
        db.Categories.AddRange(massasjeUnder);

        // --------------------------
        // 6️⃣ Tilbud
        // --------------------------
        var tilbudUnder = new List<Category>
        {
            new() { CategoryName = "nyheter", ParentCategoryId = tilbud.CategoryId },
            new() { CategoryName = "bestselgere", ParentCategoryId = tilbud.CategoryId },
            new() { CategoryName = "outlet", ParentCategoryId = tilbud.CategoryId },
            new() { CategoryName = "kampanjer", ParentCategoryId = tilbud.CategoryId }
        };
        db.Categories.AddRange(tilbudUnder);

        db.SaveChanges();
    }
    
        /*public static void SeedProducts(ApplicationDbContext db)
    {
        if (db.Products.Any()) return;

        // Hent nødvendige kategorier
        var hudAnsikt     = db.Categories.First(c => c.CategoryName == "ansikt");
        var hudHender     = db.Categories.First(c => c.CategoryName == "hender");
        var hudFøtter     = db.Categories.First(c => c.CategoryName == "føtter");
        var hudKropp      = db.Categories.First(c => c.CategoryName == "kropp");

        var aloeHud       = db.Categories.First(c => c.CategoryName == "hudpleie" && c.ParentCategory.CategoryName == "aloe-vera");
        var aloeSport     = db.Categories.First(c => c.CategoryName == "sport");
        var aloeMassasje  = db.Categories.First(c => c.CategoryName == "massasje" && c.ParentCategory.CategoryName == "aloe-vera");

        var hygHånd       = db.Categories.First(c => c.CategoryName == "desinfeksjon");
        var massOlje      = db.Categories.First(c => c.CategoryName == "massasjeolje");
        var massMuskel    = db.Categories.First(c => c.CategoryName == "muskelpleie");

        // ==============================
        // 🌿 PRODUKTOVERSIKT
        // ==============================
        var produkter = new List<Product>
        {
            new()
            {
                ProductName = "Hyaluronic Acid Day Cream 50 ml",
                ProductPriceInVAT = 299,
                ProductPriceExVAT = 200,
                StockCount = -40,
                ProductCampaignPrice = 200,
                ProductDescription = "Lett og fuktighetsgivende dagkrem som gir huden glød og mykhet.",
                LabelDescription = "Fuktighetskrem – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Hyaluronic Acid Day Cream 50 ml.png" }, new(){ PicturePath = "/images/products/Hyaluronic Acid Night Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } }
            },
            new()
            {
                ProductName = "Hyaluronic Acid Night Cream 50 ml",
                ProductPriceInVAT = 329,
                ProductPriceExVAT = 200,
                StockCount = 35,
                ProductDescription = "Rik nattkrem som fukter i dybden og reduserer tørrhetslinjer mens du sover.",
                LabelDescription = "Nattkrem – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Hyaluronic Acid Night Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } }
            },
            new()
            {
                ProductName = "AloeV Hyaluronic Acid Night Cream 50 ml",
                ProductPriceInVAT = 339,
                ProductPriceExVAT = 200,
                StockCount = 25,
                ProductDescription = "Nattkrem med Aloe Vera og hyaluronsyre – roer huden og gir dyp fuktighet.",
                LabelDescription = "Aloe Vera – Hudpleie",
                ProductPictures = { new() { PicturePath = "/images/products/AloeV Hyaluronic Acid Night Cream 50 ml.avif" } },
                ProductCategories = { new() { Category = aloeHud } }
            },
            new()
            {
                ProductName = "Victory Face Cream 50 ml",
                ProductPriceInVAT = 259,
                ProductPriceExVAT = 200,
                StockCount = 60,
                ProductDescription = "Allsidig ansiktskrem for normal til tørr hud – gir næring og mykhet.",
                LabelDescription = "Daglig pleie – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Face Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } }
            },
            new()
            {
                ProductName = "Hand & Foot Creme Extreme 100 gr",
                ProductPriceInVAT = 229,
                ProductPriceExVAT = 200,
                StockCount = 45,
                ProductDescription = "Intensiv krem for ru og tørre hender og føtter – mykgjør og beskytter.",
                LabelDescription = "Ekstra rik – Hender/Føtter",
                ProductPictures = { new() { PicturePath = "/images/products/Hand & Foot Creme Extreme, 100 gr.png" } },
                ProductCategories = { new() { Category = hudHender } },
                ProductVisibility = true
            },
            new()
            {
                ProductName = "Foot Cream 75 ml",
                ProductPriceInVAT = 199,
                ProductPriceExVAT = 200,
                StockCount = 70,
                ProductDescription = "Pleier og frisker opp slitne føtter – trekker raskt inn.",
                LabelDescription = "Fotpleie",
                ProductPictures = { new() { PicturePath = "/images/products/footcream.png" } },
                ProductCategories = { new() { Category = hudFøtter } }
            },
            new()
            {
                ProductName = "ColdFace Kuldekrem 50 ml",
                ProductPriceInVAT = 279,
                ProductPriceExVAT = 200,
                StockCount = 30,
                ProductDescription = "Beskyttende kuldekrem som motvirker tørr hud i kaldt klima.",
                LabelDescription = "Beskyttende – Kuldekrem",
                ProductPictures = { new() { PicturePath = "/images/products/coldcream.avif" } },
                ProductCategories = { new() { Category = hudKropp } }
            },
            new()
            {
                ProductName = "Victory Active Muscle 120 ml",
                ProductPriceInVAT = 189,
                ProductPriceExVAT = 200,
                StockCount = 55,
                ProductDescription = "Kjølende muskelkrem for restitusjon og lindring etter fysisk aktivitet.",
                LabelDescription = "Kjølende – Muskelpleie",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Active Muscle, 120 ml.png" } },
                ProductCategories = { new() { Category = massMuskel } }
            },
            new()
            {
                ProductName = "Victory Aloe Vera Sport Extreme 120 ml",
                ProductPriceInVAT = 219,
                ProductPriceExVAT = 200,
                StockCount = 50,
                ProductDescription = "Sterk kjølende sportskrem med Aloe Vera og mentol for økt sirkulasjon.",
                LabelDescription = "Sport – Aloe Vera",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Aloe Vera Sport Extreme 120 ml.png" } },
                ProductCategories = { new() { Category = aloeSport } }
            },
            new()
            {
                ProductName = "Victory Aloe Vera Strong Hot 120 ml",
                ProductPriceInVAT = 219,
                ProductPriceExVAT = 200,
                StockCount = 40,
                ProductDescription = "Varmende Aloe Vera-krem for lindring av stive og ømme muskler.",
                LabelDescription = "Varmende – Aloe Vera",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Aloe Vera Strong Hot, 120 m.png" } },
                ProductCategories = { new() { Category = aloeSport } }
            },
            new()
            {
                ProductName = "Victory Therapeutic Massage 250 ml",
                ProductPriceInVAT = 249,
                ProductPriceExVAT = 200,
                StockCount = 30,
                ProductDescription = "Massasjekrem med behagelig tekstur – ideell for velvære og terapi.",
                LabelDescription = "Massasje – Terapi",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Therapeutic Massage, 250 ml.png" } },
                ProductCategories = { new() { Category = massOlje } }
            },
            new()
            {
                ProductName = "EmuMedica EmuCream 120 ml",
                ProductPriceInVAT = 249,
                ProductPriceExVAT = 200,
                StockCount = 45,
                ProductDescription = "Multifunksjonell krem med Emu-olje som roer ned sensitiv hud.",
                LabelDescription = "Hudpleie – Kropp",
                ProductPictures = { new() { PicturePath = "/images/products/EmuMedica EmuCream, 120 ml.png" } },
                ProductCategories = { new() { Category = hudKropp } }
            },
            new()
            {
                ProductName = "HeatMed AloeV Cold 125 ml",
                ProductPriceInVAT = 239,
                ProductPriceExVAT = 200,
                StockCount = 30,
                ProductDescription = "Kjølende Aloe Vera-gel som lindrer overanstrengte muskler og ledd.",
                LabelDescription = "Aloe Vera – Massasje",
                ProductPictures = { new() { PicturePath = "/images/products/HeatMed AloeV Cold, 125 ml.png" } },
                ProductCategories = { new() { Category = aloeMassasje } }
            },
            new()
            {
                ProductName = "Crystal Clean Desinfiserende Håndgel 5L",
                ProductPriceInVAT = 399,
                ProductPriceExVAT = 200,
                StockCount = 20,
                ProductDescription = "Effektiv hånddesinfeksjon for profesjonell bruk – 70% alkohol.",
                LabelDescription = "Desinfeksjon – Hender",
                ProductPictures = { new() { PicturePath = "/images/products/Crystal Clean Desinfiserende Håndgel 5 liter.avif" } },
                ProductCategories = { new() { Category = hygHånd } }
            },
            new()
            {
                ProductName = "Eco-Bac 85% Håndsprit 1L",
                ProductPriceInVAT = 199,
                ProductPriceExVAT = 200,
                StockCount = 40,
                ProductDescription = "Håndsprit med 85% alkohol – effektiv og mild mot huden.",
                LabelDescription = "Desinfeksjon – Hender",
                ProductPictures = { new() { PicturePath = "/images/products/Eco-Bac 85% Håndsprit 1 liter.avif" } },
                ProductCategories = { new() { Category = hygHånd } }
            }
        };

        db.Products.AddRange(produkter);
        db.SaveChanges();
    }*/

    private static void SeedShipping(ApplicationDbContext db)
    {
        var shippingPriceThresholds = new List<ShippingPriceThreshold>
        {
            new()
            {
                MinOrderAmount = 0,
                ShippingPrice = 200
            },
            new()
            {
                MinOrderAmount = 400,
                ShippingPrice = 100
            },
            new()
            {
                MinOrderAmount = 100,
                ShippingPrice = 100
            }
        };
        
        var shippingMethods = new List<ShippingMethod>
        {
            new()
            {
                MethodName = "Henting i butikk",
                RateMultiplier = 1.5m
            },
            new()
            {
                MethodName = "Henting på lager",
                RateMultiplier = 0
            },
            new()
            {
                MethodName = "Levering hjem",
                RateMultiplier = 2
            }
        };
        db.ShippingPriceThresholds.AddRange(shippingPriceThresholds);
        db.ShippingMethods.AddRange(shippingMethods);
        db.SaveChanges();
    }


public static void SeedProducts(ApplicationDbContext db)
{
    if (db.Products.Any()) return;

    // Hent kategorier
    var hudAnsikt     = db.Categories.First(c => c.CategoryName == "ansikt");
    var hudHender     = db.Categories.First(c => c.CategoryName == "hender");
    var hudFøtter     = db.Categories.First(c => c.CategoryName == "føtter");
    var hudKropp      = db.Categories.First(c => c.CategoryName == "kropp");

    var aloeHud       = db.Categories.First(c => c.CategoryName == "hudpleie" && c.ParentCategory.CategoryName == "aloe-vera");
    var aloeSport     = db.Categories.First(c => c.CategoryName == "sport");
    var aloeMassasje  = db.Categories.First(c => c.CategoryName == "massasje" && c.ParentCategory.CategoryName == "aloe-vera");

    var hygHånd       = db.Categories.First(c => c.CategoryName == "desinfeksjon");
    var massOlje      = db.Categories.First(c => c.CategoryName == "massasjeolje");
    var massMuskel    = db.Categories.First(c => c.CategoryName == "muskelpleie");

    var produkter = new List<Product>
    {
        // =====================================================================
        //  IMAGE 1.avif – Produkt 1
        // =====================================================================
        new()
        {
            ProductName = "HydraBalance Face Cream 50 ml",
            ProductPriceInVAT = 289,
            ProductPriceExVAT = 289 * 0.8m,
            StockCount = 44,
            ProductDescription =
                "En lett, fuktighetsgivende ansiktskrem som balanserer huden og gir en jevn glød gjennom dagen.",
            LabelDescription = "Dagkrem – Ansikt",
            ProductPictures = { new() { PicturePath = "/images/products/1.avif" } },
            ProductCategories = { new() { Category = hudAnsikt } }
        },

        // =====================================================================
        //  IMAGE 1.avif – Produkt 2
        // =====================================================================
        new()
        {
            ProductName = "HydraBalance Night Repair 50 ml",
            ProductPriceInVAT = 319,
            ProductPriceExVAT = 319 * 0.8m,
            StockCount = 38,
            ProductDescription =
                "Nattkrem utviklet for å reparere hudbarrieren og gi intens fukt mens du sover.",
            LabelDescription = "Nattpleie – Ansikt",
            ProductPictures = { new() { PicturePath = "/images/products/1.avif" } },
            ProductCategories = { new() { Category = hudAnsikt } }
        },

        // =====================================================================
        //  IMAGE 2.avif — Produkt 3
        // =====================================================================
        new()
        {
            ProductName = "AloeV Radiant Body Lotion 200 ml",
            ProductPriceInVAT = 249,
            ProductPriceExVAT = 249 * 0.8m,
            StockCount = 62,
            ProductDescription =
                "Mykgjørende body lotion med Aloe Vera som tilfører fukt og roer irritert hud.",
            LabelDescription = "Hudpleie – Kropp",
            ProductPictures = { new() { PicturePath = "/images/products/2.avif" } },
            ProductCategories = { new() { Category = hudKropp } }
        },

        // =====================================================================
        //  IMAGE 2.avif — Produkt 4
        // =====================================================================
        new()
        {
            ProductName = "AloeV Soothing After Sun 150 ml",
            ProductPriceInVAT = 219,
            ProductPriceExVAT = 219 * 0.8m,
            StockCount = 58,
            ProductDescription =
                "Kjølende gel utviklet for å lindre huden etter soleksponering. Reduserer rødhet og stramhet.",
            LabelDescription = "Etter Sol – Aloe Vera",
            ProductPictures = { new() { PicturePath = "/images/products/2.avif" } },
            ProductCategories = { new() { Category = aloeHud } }
        },

        // =====================================================================
        //  IMAGE 3.avif — Produkt 5
        // =====================================================================
        new()
        {
            ProductName = "HandCare Ultra Soft 75 ml",
            ProductPriceInVAT = 129,
            ProductPriceExVAT = 129 * 0.8m,
            StockCount = 71,
            ProductDescription =
                "Mild og pleiende håndkrem som gir næring til tørre hender uten å føles klissete.",
            LabelDescription = "Mykgjørende – Hender",
            ProductPictures = { new() { PicturePath = "/images/products/3.avif" } },
            ProductCategories = { new() { Category = hudHender } }
        },

        // =====================================================================
        //  IMAGE 3.avif — Produkt 6
        // =====================================================================
        new()
        {
            ProductName = "HandCare Repair Extreme 100 ml",
            ProductPriceInVAT = 159,
            ProductPriceExVAT = 159 * 0.8m,
            StockCount = 46,
            ProductDescription =
                "Reparerende håndkrem spesielt utviklet for svært tørre og sprukne hender.",
            LabelDescription = "Ekstra rik – Hender",
            ProductPictures = { new() { PicturePath = "/images/products/3.avif" } },
            ProductCategories = { new() { Category = hudHender } }
        },

        // =====================================================================
        //  IMAGE 4.avif — Produkt 7
        // =====================================================================
        new()
        {
            ProductName = "Cooling Foot Gel 100 ml",
            ProductPriceInVAT = 169,
            ProductPriceExVAT = 169 * 0.8m,
            StockCount = 65,
            ProductDescription =
                "Forfriskende fotgel som reduserer hevelse og gir en avkjølende effekt etter lange dager.",
            LabelDescription = "Fotpleie – Kjølende",
            ProductPictures = { new() { PicturePath = "/images/products/4.avif" } },
            ProductCategories = { new() { Category = hudFøtter } }
        },

        // =====================================================================
        //  IMAGE 4.avif — Produkt 8
        // =====================================================================
        new()
        {
            ProductName = "Revitalize Foot Cream 75 ml",
            ProductPriceInVAT = 189,
            ProductPriceExVAT = 189 * 0.8m,
            StockCount = 50,
            ProductDescription =
                "Næringsrik fotkrem som mykgjør hard hud og gir langvarig fuktighet.",
            LabelDescription = "Fotpleie – Mykgjørende",
            ProductPictures = { new() { PicturePath = "/images/products/4.avif" } },
            ProductCategories = { new() { Category = hudFøtter } }
        },

        // =====================================================================
        //  IMAGE 5.avif — Produkt 9
        // =====================================================================
        new()
        {
            ProductName = "MuscleFreeze Cold Gel 150 ml",
            ProductPriceInVAT = 199,
            ProductPriceExVAT = 199 * 0.8m,
            StockCount = 41,
            ProductDescription =
                "Intens kjølende muskelgel som lindrer ømme muskler etter trening.",
            LabelDescription = "Muskelpleie – Kjølende",
            ProductPictures = { new() { PicturePath = "/images/products/5.avif" } },
            ProductCategories = { new() { Category = massMuskel } }
        },

        // =====================================================================
        //  IMAGE 5.avif — Produkt 10
        // =====================================================================
        new()
        {
            ProductName = "MuscleHeat Warming Cream 120 ml",
            ProductPriceInVAT = 219,
            ProductPriceExVAT = 219 * 0.8m,
            StockCount = 47,
            ProductDescription =
                "Oppvarmende krem for stive muskler – stimulerer blodsirkulasjonen.",
            LabelDescription = "Muskelpleie – Varmende",
            ProductPictures = { new() { PicturePath = "/images/products/5.avif" } },
            ProductCategories = { new() { Category = massMuskel } }
        },
        
        // =====================================================================
        //  IMAGE 6.avif — Produkt 11
        // =====================================================================
        new()
        {
            ProductName = "PureAloe Massage Oil 250 ml",
            ProductPriceInVAT = 259,
            ProductPriceExVAT = 259 * 0.8m,
            StockCount = 39,
            ProductDescription =
                "Massasjeolje med Aloe Vera som gir god glid og nærer huden under behandling.",
            LabelDescription = "Massasje – Olje",
            ProductPictures = { new() { PicturePath = "/images/products/6.avif" } },
            ProductCategories = { new() { Category = massOlje } }
        },

        // =====================================================================
        //  IMAGE 6.avif — Produkt 12
        // =====================================================================
        new()
        {
            ProductName = "RelaxTherapy Massage Cream 200 ml",
            ProductPriceInVAT = 229,
            ProductPriceExVAT = 229 * 0.8m,
            StockCount = 55,
            ProductDescription =
                "Myk massasjekrem med terapeutisk effekt – egnet for velvære og behandling.",
            LabelDescription = "Massasje – Krem",
            ProductPictures = { new() { PicturePath = "/images/products/6.avif" } },
            ProductCategories = { new() { Category = massOlje } }
        },

        // =====================================================================
        //  IMAGE 7.avif — Produkt 13
        // =====================================================================
        new()
        {
            ProductName = "CrystalClean Handgel 500 ml",
            ProductPriceInVAT = 139,
            ProductPriceExVAT = 139 * 0.8m,
            StockCount = 80,
            ProductDescription =
                "Effektiv desinfiserende håndgel med 70% alkohol – tørker raskt og føles mild.",
            LabelDescription = "Desinfeksjon – Hender",
            ProductPictures = { new() { PicturePath = "/images/products/7.avif" } },
            ProductCategories = { new() { Category = hygHånd } }
        },

        // =====================================================================
        //  IMAGE 7.avif — Produkt 14
        // =====================================================================
        new()
        {
            ProductName = "CrystalClean Surface Spray 1L",
            ProductPriceInVAT = 179,
            ProductPriceExVAT = 179 * 0.8m,
            StockCount = 63,
            ProductDescription =
                "Overflatedesinfeksjon for hjem og arbeidsplass. Effektiv mot bakterier og virus.",
            LabelDescription = "Hygiene – Overflatevask",
            ProductPictures = { new() { PicturePath = "/images/products/7.avif" } },
            ProductCategories = { new() { Category = hygHånd } }
        },

        // =====================================================================
        //  IMAGE 8.avif — Produkt 15
        // =====================================================================
        new()
        {
            ProductName = "AloeSport Intense Cool 125 ml",
            ProductPriceInVAT = 189,
            ProductPriceExVAT = 189 * 0.8m,
            StockCount = 44,
            ProductDescription =
                "Sterk kjølende sports-gel for aktive utøvere som trenger rask lindring.",
            LabelDescription = "Sport – Kjølende",
            ProductPictures = { new() { PicturePath = "/images/products/8.avif" } },
            ProductCategories = { new() { Category = aloeSport } }
        },

        // =====================================================================
        //  IMAGE 8.avif — Produkt 16
        // =====================================================================
        new()
        {
            ProductName = "AloeSport Hot Boost 125 ml",
            ProductPriceInVAT = 199,
            ProductPriceExVAT = 199 * 0.8m,
            StockCount = 42,
            ProductDescription =
                "Varmende sports-krem for å aktivere muskler og forebygge stivhet.",
            LabelDescription = "Sport – Varmende",
            ProductPictures = { new() { PicturePath = "/images/products/8.avif" } },
            ProductCategories = { new() { Category = aloeSport } }
        },

        // =====================================================================
        //  IMAGE 9.avif — Produkt 17
        // =====================================================================
        new()
        {
            ProductName = "SilkyTouch Body Lotion 200 ml",
            ProductPriceInVAT = 199,
            ProductPriceExVAT = 199 * 0.8m,
            StockCount = 59,
            ProductDescription =
                "Næringsrik body lotion som gir silkemyk hud og langvarig fuktighet.",
            LabelDescription = "Hudpleie – Kropp",
            ProductPictures = { new() { PicturePath = "/images/products/9.avif" } },
            ProductCategories = { new() { Category = hudKropp } }
        },

        // =====================================================================
        //  IMAGE 9.avif — Produkt 18
        // =====================================================================
        new()
        {
            ProductName = "SilkyTouch Shower Cream 250 ml",
            ProductPriceInVAT = 169,
            ProductPriceExVAT = 169 * 0.8m,
            StockCount = 73,
            ProductDescription =
                "Mild dusjkrem som rengjør huden skånsomt og tilfører fukt.",
            LabelDescription = "Hudpleie – Kropp",
            ProductPictures = { new() { PicturePath = "/images/products/9.avif" } },
            ProductCategories = { new() { Category = hudKropp } }
        },

        // =====================================================================
        //  IMAGE 10.avif — Produkt 19
        // =====================================================================
        new()
        {
            ProductName = "EmuTherapy Intensive Cream 120 ml",
            ProductPriceInVAT = 259,
            ProductPriceExVAT = 259 * 0.8m,
            StockCount = 31,
            ProductDescription =
                "Krem med Emu-olje som roer sensitiv hud og styrker hudbarrieren.",
            LabelDescription = "Hudpleie – Kropp",
            ProductPictures = { new() { PicturePath = "/images/products/10.avif" } },
            ProductCategories = { new() { Category = hudKropp } }
        },

        // =====================================================================
        //  IMAGE 10.avif — Produkt 20
        // =====================================================================
        new()
        {
            ProductName = "EmuTherapy Barrier Repair 100 ml",
            ProductPriceInVAT = 239,
            ProductPriceExVAT = 239 * 0.8m,
            StockCount = 29,
            ProductDescription =
                "Reparerende krem som beskytter og gjenoppretter hudens naturlige fuktbalanse.",
            LabelDescription = "Hudpleie – Reparerende",
            ProductPictures = { new() { PicturePath = "/images/products/10.avif" } },
            ProductCategories = { new() { Category = hudKropp } }
        },
        
        // =====================================================================
//  IMAGE 11.avif — Produkt 21
// =====================================================================
new()
{
    ProductName = "AloeDerm Fresh Gel 100 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 64,
    ProductDescription =
        "Beroligende Aloe Vera-gel som reduserer rødhet og gir rask fuktighet til irritert hud.",
    LabelDescription = "Aloe Vera – Hudpleie",
    ProductPictures = { new() { PicturePath = "/images/products/11.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 11.avif — Produkt 22
// =====================================================================
new()
{
    ProductName = "AloeDerm Repair Cream 75 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 52,
    ProductDescription =
        "Reparerende Aloe Vera-krem for tørr og sensitiv hud. Gir langvarig pleie.",
    LabelDescription = "Hudpleie – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/11.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 12.avif — Produkt 23
// =====================================================================
new()
{
    ProductName = "SportMax Cooling Gel 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 48,
    ProductDescription =
        "Avkjølende gel for aktive muskler. Perfekt etter harde treningsøkter.",
    LabelDescription = "Sport – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/12.avif" } },
    ProductCategories = { new() { Category = aloeSport } }
},

// =====================================================================
//  IMAGE 12.avif — Produkt 24
// =====================================================================
new()
{
    ProductName = "SportMax Warming Oil 125 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 43,
    ProductDescription =
        "Oppvarmende sportsolje som øker blodsirkulasjonen og forbereder musklene på aktivitet.",
    LabelDescription = "Sport – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/12.avif" } },
    ProductCategories = { new() { Category = aloeSport } }
},

// =====================================================================
//  IMAGE 13.avif — Produkt 25
// =====================================================================
new()
{
    ProductName = "ThermaHeat Massage Oil 250 ml",
    ProductPriceInVAT = 259,
    ProductPriceExVAT = 259 * 0.8m,
    StockCount = 37,
    ProductDescription =
        "Varmende massasjeolje for dype muskelbehandlinger og velvære.",
    LabelDescription = "Massasje – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/13.avif" } },
    ProductCategories = { new() { Category = massOlje } }
},

// =====================================================================
//  IMAGE 13.avif — Produkt 26
// =====================================================================
new()
{
    ProductName = "ThermaHeat Professional Cream 200 ml",
    ProductPriceInVAT = 239,
    ProductPriceExVAT = 239 * 0.8m,
    StockCount = 41,
    ProductDescription =
        "Massasjekrem for profesjonelle terapeuter. Gir varme og god glid.",
    LabelDescription = "Massasje – Terapi",
    ProductPictures = { new() { PicturePath = "/images/products/13.avif" } },
    ProductCategories = { new() { Category = massOlje } }
},

// =====================================================================
//  IMAGE 14.avif — Produkt 27
// =====================================================================
new()
{
    ProductName = "PureHands Antibac Gel 250 ml",
    ProductPriceInVAT = 129,
    ProductPriceExVAT = 129 * 0.8m,
    StockCount = 77,
    ProductDescription =
        "Hånddesinfeksjon med mild duft og fuktighetsgivende ingredienser.",
    LabelDescription = "Desinfeksjon – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/14.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 14.avif — Produkt 28
// =====================================================================
new()
{
    ProductName = "PureHands Antibac Spray 500 ml",
    ProductPriceInVAT = 149,
    ProductPriceExVAT = 149 * 0.8m,
    StockCount = 69,
    ProductDescription =
        "Kraftig og effektiv antibac-spray for overflater og utstyr.",
    LabelDescription = "Hygiene – Overflate",
    ProductPictures = { new() { PicturePath = "/images/products/14.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 15.avif — Produkt 29
// =====================================================================
new()
{
    ProductName = "BodyRevive Lotion 200 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 56,
    ProductDescription =
        "Fuktighetsgivende body lotion som gir en jevn og myk hud gjennom hele dagen.",
    LabelDescription = "Hudpleie – Kropp",
    ProductPictures = { new() { PicturePath = "/images/products/15.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 15.avif — Produkt 30
// =====================================================================
new()
{
    ProductName = "BodyRevive Shower Oil 250 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 61,
    ProductDescription =
        "Skånsom dusjolje som tilfører intens fukt og etterlater huden silkemyk.",
    LabelDescription = "Hudpleie – Kropp",
    ProductPictures = { new() { PicturePath = "/images/products/15.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 16.avif — Produkt 31
// =====================================================================
new()
{
    ProductName = "AloeTherapy Soothing Lotion 150 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 48,
    ProductDescription =
        "Lett body lotion med Aloe Vera som beroliger og fukter huden.",
    LabelDescription = "Aloe Vera – Kropp",
    ProductPictures = { new() { PicturePath = "/images/products/16.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 16.avif — Produkt 32
// =====================================================================
new()
{
    ProductName = "AloeTherapy Gel Repair 125 ml",
    ProductPriceInVAT = 179,
    ProductPriceExVAT = 179 * 0.8m,
    StockCount = 53,
    ProductDescription =
        "Reparerende Aloe Vera-gel som lindrer irritasjoner og gir rask fukt.",
    LabelDescription = "Hudpleie – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/16.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 17.avif — Produkt 33
// =====================================================================
new()
{
    ProductName = "MuscleRecovery Cool Boost 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 45,
    ProductDescription =
        "Sterk kjølende gel som hjelper ømme og overbelastede muskler med restitusjon.",
    LabelDescription = "Muskelpleie – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/17.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 17.avif — Produkt 34
// =====================================================================
new()
{
    ProductName = "MuscleRecovery Heat Therapy 120 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 38,
    ProductDescription =
        "Oppvarmende muskelkrem som øker blodsirkulasjonen og reduserer stivhet.",
    LabelDescription = "Muskelpleie – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/17.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 18.avif — Produkt 35
// =====================================================================
new()
{
    ProductName = "GentleFace Daily Wash 150 ml",
    ProductPriceInVAT = 159,
    ProductPriceExVAT = 159 * 0.8m,
    StockCount = 72,
    ProductDescription =
        "Mild ansiktsrens som fjerner urenheter uten å tørke ut huden.",
    LabelDescription = "Ansiktsrens – Daglig bruk",
    ProductPictures = { new() { PicturePath = "/images/products/18.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 18.avif — Produkt 36
// =====================================================================
new()
{
    ProductName = "GentleFace Deep Clean 150 ml",
    ProductPriceInVAT = 179,
    ProductPriceExVAT = 179 * 0.8m,
    StockCount = 63,
    ProductDescription =
        "Dyptrensende ansiktsvask for å redusere porer og gi en renere hudtone.",
    LabelDescription = "Ansiktsrens – Dyp",
    ProductPictures = { new() { PicturePath = "/images/products/18.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 19.avif — Produkt 37
// =====================================================================
new()
{
    ProductName = "CrystalClean Surface Wipes 50 stk",
    ProductPriceInVAT = 89,
    ProductPriceExVAT = 89 * 0.8m,
    StockCount = 120,
    ProductDescription =
        "Desinfiserende våtservietter for rask og effektiv rengjøring av overflater.",
    LabelDescription = "Hygiene – Rengjøring",
    ProductPictures = { new() { PicturePath = "/images/products/19.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 19.avif — Produkt 38
// =====================================================================
new()
{
    ProductName = "CrystalClean Antibac Wet 40 stk",
    ProductPriceInVAT = 79,
    ProductPriceExVAT = 79 * 0.8m,
    StockCount = 131,
    ProductDescription =
        "Håndservietter med antibac for rask desinfisering når du er på farten.",
    LabelDescription = "Hygiene – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/19.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 20.avif — Produkt 39
// =====================================================================
new()
{
    ProductName = "AloeMedica Soothing Cream 120 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 44,
    ProductDescription =
        "Beroligende Aloe Vera-krem som gir lett fuktighet og reduserer irritasjoner.",
    LabelDescription = "Aloe Vera – Hudpleie",
    ProductPictures = { new() { PicturePath = "/images/products/20.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 20.avif — Produkt 40
// =====================================================================
new()
{
    ProductName = "AloeMedica Intensive Repair 120 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 36,
    ProductDescription =
        "Intensiv reparasjonskrem med Aloe Vera for skadet, tørr og sensitiv hud.",
    LabelDescription = "Hudpleie – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/20.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 21.avif — Produkt 41
// =====================================================================
new()
{
    ProductName = "HydraGlow Moisturizing Cream 100 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 52,
    ProductDescription =
        "Fuktighetsgivende krem som gir en naturlig glød og forbedrer hudens elastisitet.",
    LabelDescription = "Hudpleie – Glød",
    ProductPictures = { new() { PicturePath = "/images/products/21.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 21.avif — Produkt 42
// =====================================================================
new()
{
    ProductName = "HydraGlow Revitalizing Serum 50 ml",
    ProductPriceInVAT = 259,
    ProductPriceExVAT = 259 * 0.8m,
    StockCount = 34,
    ProductDescription =
        "Lett serum som trekker raskt inn og reduserer tørrhet med dyp fukt.",
    LabelDescription = "Ansikt – Serum",
    ProductPictures = { new() { PicturePath = "/images/products/21.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 22.avif — Produkt 43
// =====================================================================
new()
{
    ProductName = "MusclePro Instant Cold Gel 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 47,
    ProductDescription =
        "Kraftig kjølende gel som bidrar til rask restitusjon av ømme muskler.",
    LabelDescription = "Muskelpleie – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/22.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 22.avif — Produkt 44
// =====================================================================
new()
{
    ProductName = "MusclePro Deep Heat Cream 120 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 39,
    ProductDescription =
        "Varmende muskelkrem som er ideell for ømme og stive muskelområder.",
    LabelDescription = "Muskelpleie – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/22.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 23.avif — Produkt 45
// =====================================================================
new()
{
    ProductName = "SilkBody Nourishing Lotion 250 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 62,
    ProductDescription =
        "Rik body lotion som gir langvarig fuktighet og etterlater huden silkemyk.",
    LabelDescription = "Kropp – Fuktighetsgivende",
    ProductPictures = { new() { PicturePath = "/images/products/23.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 23.avif — Produkt 46
// =====================================================================
new()
{
    ProductName = "SilkBody Shower Cream 200 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 58,
    ProductDescription =
        "Mild dusjkrem som rengjør og pleier huden uten å tørke den ut.",
    LabelDescription = "Kropp – Rens",
    ProductPictures = { new() { PicturePath = "/images/products/23.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 24.avif — Produkt 47
// =====================================================================
new()
{
    ProductName = "CrystalClean Ultra Antibac 500 ml",
    ProductPriceInVAT = 149,
    ProductPriceExVAT = 149 * 0.8m,
    StockCount = 78,
    ProductDescription =
        "Desinfiserende antibac med 70% alkohol og mild duft.",
    LabelDescription = "Hygiene – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/24.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 24.avif — Produkt 48
// =====================================================================
new()
{
    ProductName = "CrystalClean Ultra Wipes 80 stk",
    ProductPriceInVAT = 99,
    ProductPriceExVAT = 99 * 0.8m,
    StockCount = 110,
    ProductDescription =
        "Desinfiserende våtservietter som effektivt fjerner bakterier på hender og overflater.",
    LabelDescription = "Hygiene – Rengjøring",
    ProductPictures = { new() { PicturePath = "/images/products/24.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 25.avif — Produkt 49
// =====================================================================
new()
{
    ProductName = "AloeFresh Cooling Lotion 150 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 57,
    ProductDescription =
        "Avkjølende Aloe Vera-lotion som passer perfekt etter trening eller sol.",
    LabelDescription = "Aloe Vera – Kropp",
    ProductPictures = { new() { PicturePath = "/images/products/25.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 25.avif — Produkt 50
// =====================================================================
new()
{
    ProductName = "AloeFresh Intensive Gel 120 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 63,
    ProductDescription =
        "Kjølende og beroligende gel for irritert og varm hud.",
    LabelDescription = "Aloe Vera – Gel",
    ProductPictures = { new() { PicturePath = "/images/products/25.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 26.avif — Produkt 51
// =====================================================================
new()
{
    ProductName = "RelaxOil Professional Massage 500 ml",
    ProductPriceInVAT = 279,
    ProductPriceExVAT = 279 * 0.8m,
    StockCount = 41,
    ProductDescription =
        "Massasjeolje utviklet for profesjonell bruk. Gir optimal glid og langvarig komfort.",
    LabelDescription = "Massasje – Olje",
    ProductPictures = { new() { PicturePath = "/images/products/26.avif" } },
    ProductCategories = { new() { Category = massOlje } }
},

// =====================================================================
//  IMAGE 26.avif — Produkt 52
// =====================================================================
new()
{
    ProductName = "RelaxOil Warm Therapy 250 ml",
    ProductPriceInVAT = 249,
    ProductPriceExVAT = 249 * 0.8m,
    StockCount = 37,
    ProductDescription =
        "Varmende massasjeolje som virker avslappende på stive muskler.",
    LabelDescription = "Massasje – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/26.avif" } },
    ProductCategories = { new() { Category = massOlje } }
},

// =====================================================================
//  IMAGE 27.avif — Produkt 53
// =====================================================================
new()
{
    ProductName = "HandCare Ultra Clean Gel 300 ml",
    ProductPriceInVAT = 119,
    ProductPriceExVAT = 119 * 0.8m,
    StockCount = 94,
    ProductDescription =
        "Effektiv hånddesinfeksjonsgel med rask virketid og mild duft.",
    LabelDescription = "Hygiene – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/27.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 27.avif — Produkt 54
// =====================================================================
new()
{
    ProductName = "HandCare Ultra Clean Spray 400 ml",
    ProductPriceInVAT = 129,
    ProductPriceExVAT = 129 * 0.8m,
    StockCount = 88,
    ProductDescription =
        "Desinfiserende spray som er perfekt for håndflater og overflater.",
    LabelDescription = "Hygiene – Spray",
    ProductPictures = { new() { PicturePath = "/images/products/27.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 28.avif — Produkt 55
// =====================================================================
new()
{
    ProductName = "AloeVital Body Cream 200 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 51,
    ProductDescription =
        "Næringsrik Aloe Vera-krem som gir dyp fuktighet til hele kroppen.",
    LabelDescription = "Kropp – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/28.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 28.avif — Produkt 56
// =====================================================================
new()
{
    ProductName = "AloeVital After Sun Gel 150 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 46,
    ProductDescription =
        "Beroligende Aloe Vera-gel som lindrer og kjøler huden etter soleksponering.",
    LabelDescription = "Etter Sol – Aloe Vera",
    ProductPictures = { new() { PicturePath = "/images/products/28.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 29.avif — Produkt 57
// =====================================================================
new()
{
    ProductName = "MuscleTherm Strong Heat 150 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 39,
    ProductDescription =
        "Sterk varmegel for dyp muskelbehandling og økt mobilitet.",
    LabelDescription = "Muskelpleie – Kraftig varme",
    ProductPictures = { new() { PicturePath = "/images/products/29.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 29.avif — Produkt 58
// =====================================================================
new()
{
    ProductName = "MuscleTherm Medium Warm 120 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 44,
    ProductDescription =
        "Medium varmende muskelkrem for moderat lindring av stivhet.",
    LabelDescription = "Muskelpleie – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/29.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 30.avif — Produkt 59
// =====================================================================
new()
{
    ProductName = "GentleSkin Face Milk 150 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 55,
    ProductDescription =
        "Rensende og fuktgivende ansiktsmelk som er skånsom mot sensitiv hud.",
    LabelDescription = "Ansikt – Rens",
    ProductPictures = { new() { PicturePath = "/images/products/30.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 30.avif — Produkt 60
// =====================================================================
new()
{
    ProductName = "GentleSkin Refreshing Toner 125 ml",
    ProductPriceInVAT = 149,
    ProductPriceExVAT = 149 * 0.8m,
    StockCount = 61,
    ProductDescription =
        "Oppfriskende ansiktstoner som balanserer hudtonen og fjerner rester etter rens.",
    LabelDescription = "Ansikt – Toner",
    ProductPictures = { new() { PicturePath = "/images/products/30.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},
// =====================================================================
//  IMAGE 31.avif — Produkt 61
// =====================================================================
new()
{
    ProductName = "AloeSense Refresh Gel 150 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 57,
    ProductDescription =
        "Forfriskende Aloe Vera-gel som gir kjøling og lindring til varm og irritert hud.",
    LabelDescription = "Aloe Vera – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/31.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 31.avif — Produkt 62
// =====================================================================
new()
{
    ProductName = "AloeSense Moist Repair 120 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 49,
    ProductDescription =
        "Reparerende Aloe Vera-krem som styrker hudbarrieren og gir dyp fuktighet.",
    LabelDescription = "Aloe Vera – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/31.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 32.avif — Produkt 63
// =====================================================================
new()
{
    ProductName = "CrystalClean Pro Sanitizer 1L",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 84,
    ProductDescription =
        "Profesjonell antibac for bedrifter, klinikker og hjem. Dreper 99.9% av bakterier.",
    LabelDescription = "Hygiene – Desinfeksjon",
    ProductPictures = { new() { PicturePath = "/images/products/32.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 32.avif — Produkt 64
// =====================================================================
new()
{
    ProductName = "CrystalClean Pro Surface 1L",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 61,
    ProductDescription =
        "Kraftig overflatedesinfeksjon egnet for profesjonelle miljøer.",
    LabelDescription = "Hygiene – Overflate",
    ProductPictures = { new() { PicturePath = "/images/products/32.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 33.avif — Produkt 65
// =====================================================================
new()
{
    ProductName = "AloeBoost Sport Cool 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 46,
    ProductDescription =
        "Sterk kjølende sports-gel med Aloe Vera for intensiv restitusjon.",
    LabelDescription = "Sport – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/33.avif" } },
    ProductCategories = { new() { Category = aloeSport } }
},

// =====================================================================
//  IMAGE 33.avif — Produkt 66
// =====================================================================
new()
{
    ProductName = "AloeBoost Sport Heat 125 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 42,
    ProductDescription =
        "Aktiverende varmegel for stive muskler – perfekt før trening.",
    LabelDescription = "Sport – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/33.avif" } },
    ProductCategories = { new() { Category = aloeSport } }
},

// =====================================================================
//  IMAGE 34.avif — Produkt 67
// =====================================================================
new()
{
    ProductName = "DeepRecover Muscle Balm 120 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 38,
    ProductDescription =
        "Lindrende muskelbalsam for dype spenninger og stølhet.",
    LabelDescription = "Muskelpleie – Balsam",
    ProductPictures = { new() { PicturePath = "/images/products/34.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 34.avif — Produkt 68
// =====================================================================
new()
{
    ProductName = "DeepRecover Heat Balm 120 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 33,
    ProductDescription =
        "Oppvarmende muskelbalsam som gir dyp varmeeffekt for spente muskler.",
    LabelDescription = "Muskelpleie – Varmebalsam",
    ProductPictures = { new() { PicturePath = "/images/products/34.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 35.avif — Produkt 69
// =====================================================================
new()
{
    ProductName = "SoftHands Gentle Soap 300 ml",
    ProductPriceInVAT = 119,
    ProductPriceExVAT = 119 * 0.8m,
    StockCount = 102,
    ProductDescription =
        "Mild håndsåpe som rengjør effektivt uten å tørke ut huden.",
    LabelDescription = "Hygiene – Håndsåpe",
    ProductPictures = { new() { PicturePath = "/images/products/35.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 35.avif — Produkt 70
// =====================================================================
new()
{
    ProductName = "SoftHands Antibac Foam 250 ml",
    ProductPriceInVAT = 129,
    ProductPriceExVAT = 129 * 0.8m,
    StockCount = 95,
    ProductDescription =
        "Skånsom antibac-foam som dreper bakterier uten sterk lukt.",
    LabelDescription = "Hygiene – Skumrens",
    ProductPictures = { new() { PicturePath = "/images/products/35.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 36.avif — Produkt 71
// =====================================================================
new()
{
    ProductName = "AloePure Body Gel 200 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 58,
    ProductDescription =
        "Aloe Vera-basert kroppsgel som gir lett kjøling og rask fuktighet.",
    LabelDescription = "Kropp – Aloe Vera",
    ProductPictures = { new() { PicturePath = "/images/products/36.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 36.avif — Produkt 72
// =====================================================================
new()
{
    ProductName = "AloePure Rejuvenating Cream 150 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 41,
    ProductDescription =
        "Fuktighetsgivende kroppskrem med Aloe Vera og botaniske ekstrakter.",
    LabelDescription = "Kropp – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/36.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 37.avif — Produkt 73
// =====================================================================
new()
{
    ProductName = "MuscleImpact Cooling Cream 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 47,
    ProductDescription =
        "Kjølende krem med mentol og eukalyptus for effektiv lindring.",
    LabelDescription = "Muskelpleie – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/37.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 37.avif — Produkt 74
// =====================================================================
new()
{
    ProductName = "MuscleImpact Thermal Cream 150 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 35,
    ProductDescription =
        "Varmende muskelkrem som øker sirkulasjonen og gir lindring ved stivhet.",
    LabelDescription = "Muskelpleie – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/37.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 38.avif — Produkt 75
// =====================================================================
new()
{
    ProductName = "AloeDerma Skin Soother 125 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 60,
    ProductDescription =
        "Aloe Vera-gel som beroliger sensitiv eller solbrent hud.",
    LabelDescription = "Aloe Vera – Beroligende",
    ProductPictures = { new() { PicturePath = "/images/products/38.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 38.avif — Produkt 76
// =====================================================================
new()
{
    ProductName = "AloeDerma Hydrating Lotion 150 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 44,
    ProductDescription =
        "Fuktighetsgivende Aloe Vera-lotion som absorberes raskt.",
    LabelDescription = "Aloe Vera – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/38.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 39.avif — Produkt 77
// =====================================================================
new()
{
    ProductName = "CleanPro Surface Cleaner 500 ml",
    ProductPriceInVAT = 149,
    ProductPriceExVAT = 149 * 0.8m,
    StockCount = 89,
    ProductDescription =
        "Kraftig rengjøringsspray for profesjonell bruk. Effektiv mot virus og bakterier.",
    LabelDescription = "Hygiene – Overflaterens",
    ProductPictures = { new() { PicturePath = "/images/products/39.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 39.avif — Produkt 78
// =====================================================================
new()
{
    ProductName = "CleanPro Ultra Antibac 500 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 81,
    ProductDescription =
        "Antibac-løsning med høy effektivitet. Perfekt for arbeidsplasser og hjemmebruk.",
    LabelDescription = "Hygiene – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/39.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 40.avif — Produkt 79
// =====================================================================
new()
{
    ProductName = "AloeCure Relief Gel 150 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 52,
    ProductDescription =
        "Lindrende Aloe Vera-gel, perfekt etter sol eller irritasjon.",
    LabelDescription = "Aloe Vera – Lindrende",
    ProductPictures = { new() { PicturePath = "/images/products/40.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 40.avif — Produkt 80
// =====================================================================
new()
{
    ProductName = "AloeCure Moist Lotion 150 ml",
    ProductPriceInVAT = 209,
    ProductPriceExVAT = 209 * 0.8m,
    StockCount = 41,
    ProductDescription =
        "Fuktighetsgivende lotion med Aloe Vera som gir frisk og balansert hud.",
    LabelDescription = "Aloe Vera – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/40.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 41.avif — Produkt 81
// =====================================================================
new()
{
    ProductName = "MuscleActive Freeze Gel 150 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 37,
    ProductDescription =
        "Kjølende muskelgel for restitusjon etter trening og belastning.",
    LabelDescription = "Muskelpleie – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/41.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 41.avif — Produkt 82
// =====================================================================
new()
{
    ProductName = "MuscleActive Heat Boost 150 ml",
    ProductPriceInVAT = 239,
    ProductPriceExVAT = 239 * 0.8m,
    StockCount = 34,
    ProductDescription =
        "Varmende gel utviklet for å aktivere og løsne stive muskler.",
    LabelDescription = "Muskelpleie – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/41.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 42.avif — Produkt 83
// =====================================================================
new()
{
    ProductName = "AloeBoost Ultra Cool 150 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = -12,
    ProductCampaignPrice = 149,
    ProductDescription = "Ekstra kjølende Aloe Vera-gel for rask lindring av varme og irritasjoner.",
    LabelDescription = "Aloe Vera – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/42.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 42.avif — Produkt 84
// =====================================================================
new()
{
    ProductName = "AloeBoost Ultra Heat 150 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 33,
    ProductCampaignPrice = 189,
    ProductDescription = "Varmende gel for dype muskelspenninger og økt sirkulasjon.",
    LabelDescription = "Muskelpleie – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/42.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 43.avif — Produkt 85
// =====================================================================
new()
{
    ProductName = "CrystalClean Compact Antibac 150 ml",
    ProductPriceInVAT = 99,
    ProductPriceExVAT = 99 * 0.8m,
    StockCount = 120,
    ProductDescription = "Kompakt hånddesinfeksjon for bruk hjemme eller på farten.",
    LabelDescription = "Hygiene – Hender",
    ProductPictures = { new() { PicturePath = "/images/products/43.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 43.avif — Produkt 86
// =====================================================================
new()
{
    ProductName = "CrystalClean Rapid Wipes 60 stk",
    ProductPriceInVAT = 79,
    ProductPriceExVAT = 79 * 0.8m,
    StockCount = -5,
    ProductCampaignPrice = 59,
    ProductDescription = "Praktiske våtservietter for rask rengjøring av hender og overflater.",
    LabelDescription = "Hygiene – Rengjøring",
    ProductPictures = { new() { PicturePath = "/images/products/43.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 44.avif — Produkt 87
// =====================================================================
new()
{
    ProductName = "GentleTouch Daily Body Lotion 250 ml",
    ProductPriceInVAT = 169,
    ProductPriceExVAT = 169 * 0.8m,
    StockCount = 50,
    ProductDescription = "Lett body lotion som gir daglig fuktighet og pleie.",
    LabelDescription = "Hudpleie – Kropp",
    ProductPictures = { new() { PicturePath = "/images/products/44.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 44.avif — Produkt 88
// =====================================================================
new()
{
    ProductName = "GentleTouch Intensive Repair 200 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = -8,
    ProductCampaignPrice = 139,
    ProductDescription = "Intensiv reparasjonskrem som gjenoppretter tørr og skadet hud.",
    LabelDescription = "Hudpleie – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/44.avif" } },
    ProductCategories = { new() { Category = hudKropp } }
},

// =====================================================================
//  IMAGE 45.avif — Produkt 89
// =====================================================================
new()
{
    ProductName = "AloeRevive UltraGel 200 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = 61,
    ProductDescription = "Aloe Vera-gel med ekstra fukt og styrkende plantenæringsstoffer.",
    LabelDescription = "Aloe Vera – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/45.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 45.avif — Produkt 90
// =====================================================================
new()
{
    ProductName = "AloeRevive Soothing Cream 150 ml",
    ProductPriceInVAT = 249,
    ProductPriceExVAT = 249 * 0.8m,
    StockCount = 29,
    ProductCampaignPrice = 199,
    ProductDescription = "Beroligende krem for sensitiv hud som trenger ekstra pleie.",
    LabelDescription = "Aloe Vera – Beroligende",
    ProductPictures = { new() { PicturePath = "/images/products/45.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 46.avif — Produkt 91
// =====================================================================
new()
{
    ProductName = "HeatPro Deep Therapy 150 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 44,
    ProductDescription = "Kraftig varmegel som gir dyp lindring til muskelområder.",
    LabelDescription = "Muskelpleie – Dyp varme",
    ProductPictures = { new() { PicturePath = "/images/products/46.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 46.avif — Produkt 92
// =====================================================================
new()
{
    ProductName = "HeatPro Warming Oil 200 ml",
    ProductPriceInVAT = 259,
    ProductPriceExVAT = 259 * 0.8m,
    StockCount = -3,
    ProductCampaignPrice = 209,
    ProductDescription = "Massasjeolje med varmeeffekt for profesjonell bruk.",
    LabelDescription = "Massasje – Varme",
    ProductPictures = { new() { PicturePath = "/images/products/46.avif" } },
    ProductCategories = { new() { Category = massOlje } }
},

// =====================================================================
//  IMAGE 47.avif — Produkt 93
// =====================================================================
new()
{
    ProductName = "EcoHands Foam Cleanser 250 ml",
    ProductPriceInVAT = 129,
    ProductPriceExVAT = 129 * 0.8m,
    StockCount = 77,
    ProductDescription = "Mild skumrens for hendene. Svært skånsom for sensitiv hud.",
    LabelDescription = "Hygiene – Håndrens",
    ProductPictures = { new() { PicturePath = "/images/products/47.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 47.avif — Produkt 94
// =====================================================================
new()
{
    ProductName = "EcoHands Antibac Spray 300 ml",
    ProductPriceInVAT = 99,
    ProductPriceExVAT = 99 * 0.8m,
    StockCount = -10,
    ProductCampaignPrice = 69,
    ProductDescription = "Antibac-spray som sørger for rask desinfeksjon av hender og berøringsflater.",
    LabelDescription = "Hygiene – Spray",
    ProductPictures = { new() { PicturePath = "/images/products/47.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 48.avif — Produkt 95
// =====================================================================
new()
{
    ProductName = "AloeRestore Skin Cream 200 ml",
    ProductPriceInVAT = 229,
    ProductPriceExVAT = 229 * 0.8m,
    StockCount = 51,
    ProductDescription = "Reparerende Aloe Vera-krem for tørr og stresset hud.",
    LabelDescription = "Aloe Vera – Reparerende",
    ProductPictures = { new() { PicturePath = "/images/products/48.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 48.avif — Produkt 96
// =====================================================================
new()
{
    ProductName = "AloeRestore Barrier Gel 150 ml",
    ProductPriceInVAT = 189,
    ProductPriceExVAT = 189 * 0.8m,
    StockCount = 32,
    ProductCampaignPrice = 149,
    ProductDescription = "Gel som styrker hudbarrieren og gir rask lindring.",
    LabelDescription = "Aloe Vera – Barrierepleie",
    ProductPictures = { new() { PicturePath = "/images/products/48.avif" } },
    ProductCategories = { new() { Category = aloeHud } }
},

// =====================================================================
//  IMAGE 49.avif — Produkt 97
// =====================================================================
new()
{
    ProductName = "MuscleZen Cold Therapy 120 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 40,
    ProductDescription = "Kjølende muskelgel som lindrer smerter og tretthet.",
    LabelDescription = "Muskelpleie – Kjølende",
    ProductPictures = { new() { PicturePath = "/images/products/49.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 49.avif — Produkt 98
// =====================================================================
new()
{
    ProductName = "MuscleZen Heat Therapy 120 ml",
    ProductPriceInVAT = 219,
    ProductPriceExVAT = 219 * 0.8m,
    StockCount = -7,
    ProductCampaignPrice = 169,
    ProductDescription = "Varmende gel for muskelstivhet og tretthet, perfekt for trening.",
    LabelDescription = "Muskelpleie – Varmende",
    ProductPictures = { new() { PicturePath = "/images/products/49.avif" } },
    ProductCategories = { new() { Category = massMuskel } }
},

// =====================================================================
//  IMAGE 50.avif — Produkt 99
// =====================================================================
new()
{
    ProductName = "DailyGlow Face Cream 75 ml",
    ProductPriceInVAT = 179,
    ProductPriceExVAT = 179 * 0.8m,
    StockCount = 63,
    ProductDescription = "Fuktighetskrem som gir naturlig glød og myk hud hele dagen.",
    LabelDescription = "Ansikt – Fuktighet",
    ProductPictures = { new() { PicturePath = "/images/products/50.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 50.avif — Produkt 100
// =====================================================================
new()
{
    ProductName = "DailyGlow Night Repair 75 ml",
    ProductPriceInVAT = 199,
    ProductPriceExVAT = 199 * 0.8m,
    StockCount = 48,
    ProductCampaignPrice = 159,
    ProductDescription = "Reparerende nattkrem som jobber aktivt med hudens gjenoppbygging.",
    LabelDescription = "Ansikt – Nattpleie",
    ProductPictures = { new() { PicturePath = "/images/products/50.avif" } },
    ProductCategories = { new() { Category = hudAnsikt } }
},

// =====================================================================
//  IMAGE 51.avif — Produkt 101
// =====================================================================
new()
{
    ProductName = "EcoFresh Hand Wash 250 ml",
    ProductPriceInVAT = 129,
    ProductPriceExVAT = 129 * 0.8m,
    StockCount = 84,
    ProductDescription = "Mild håndsåpe med naturlige planteekstrakter.",
    LabelDescription = "Hygiene – Såpe",
    ProductPictures = { new() { PicturePath = "/images/products/51.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},

// =====================================================================
//  IMAGE 51.avif — Produkt 102
// =====================================================================
new()
{
    ProductName = "EcoFresh Antibac Gel 150 ml",
    ProductPriceInVAT = 99,
    ProductPriceExVAT = 99 * 0.8m,
    StockCount = -15,
    ProductCampaignPrice = 69,
    ProductDescription = "Alkoholsbasert antibac-gel med rask tørketid og mild duft.",
    LabelDescription = "Hygiene – Antibac",
    ProductPictures = { new() { PicturePath = "/images/products/51.avif" } },
    ProductCategories = { new() { Category = hygHånd } }
},



    };

    db.Products.AddRange(produkter);
    db.SaveChanges();
}


}
