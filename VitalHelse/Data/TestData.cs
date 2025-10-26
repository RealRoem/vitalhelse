using VitalHelse.Models;

namespace VitalHelse.Data;

public static class TestData
{
    public static void Initialize(ApplicationDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Categories.Any()) {
            SeedCategories(db);
            SeedPhysicalAttributes(db);
            SeedProducts(db);
            

        }
    }
    private static void SeedPhysicalAttributes(ApplicationDbContext db)
    {
        if (db.PhysicalAttributes.Any()) return;

        var sizes = new List<PhysicalAttribute>
        {
            new() { Attribute = "100gr" },
            new() { Attribute = "50ml" },
            new() { Attribute = "100ml" },
            new() { Attribute = "150ml" },
            new() { Attribute = "200ml" },
            new() { Attribute = "250ml" },
            new() { Attribute = "300ml" },
            new() { Attribute = "350ml" },
            new() { Attribute = "400ml" },
        };

        db.PhysicalAttributes.AddRange(sizes);
        db.SaveChanges();
    }

    

    // =========================================================
    //                 KATEGORI-SEEDING (RENT HIERARKI)
    // =========================================================
    
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
            new() { CategoryName = "hånddesinfeksjon", ParentCategoryId = hygiene.CategoryId },
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
    
        public static void SeedProducts(ApplicationDbContext db)
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

        var hygHånd       = db.Categories.First(c => c.CategoryName == "hånddesinfeksjon");
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
                ProductPrice = 299,
                StockCount = 40,
                ProductDescription = "Lett og fuktighetsgivende dagkrem som gir huden glød og mykhet.",
                LabelDescription = "Fuktighetskrem – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Hyaluronic Acid Day Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } }
            },
            new()
            {
                ProductName = "Hyaluronic Acid Night Cream 50 ml",
                ProductPrice = 329,
                StockCount = 35,
                ProductDescription = "Rik nattkrem som fukter i dybden og reduserer tørrhetslinjer mens du sover.",
                LabelDescription = "Nattkrem – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Hyaluronic Acid Night Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } },
                PhysicalProductAttributes ={new() { PhysicalAttribute = db.PhysicalAttributes.First(a => a.Attribute == "50ml") }}
            },
            new()
            {
                ProductName = "AloeV Hyaluronic Acid Night Cream 50 ml",
                ProductPrice = 339,
                StockCount = 25,
                ProductDescription = "Nattkrem med Aloe Vera og hyaluronsyre – roer huden og gir dyp fuktighet.",
                LabelDescription = "Aloe Vera – Hudpleie",
                ProductPictures = { new() { PicturePath = "/images/products/AloeV Hyaluronic Acid Night Cream 50 ml.png" } },
                ProductCategories = { new() { Category = aloeHud } },
                PhysicalProductAttributes ={new() { PhysicalAttribute = db.PhysicalAttributes.First(a => a.Attribute == "50ml") }}
            },
            new()
            {
                ProductName = "Victory Face Cream 50 ml",
                ProductPrice = 259,
                StockCount = 60,
                ProductDescription = "Allsidig ansiktskrem for normal til tørr hud – gir næring og mykhet.",
                LabelDescription = "Daglig pleie – Ansikt",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Face Cream 50 ml.png" } },
                ProductCategories = { new() { Category = hudAnsikt } }
            },
            new()
            {
                ProductName = "Hand & Foot Creme Extreme 100 gr",
                ProductPrice = 229,
                StockCount = 45,
                ProductDescription = "Intensiv krem for ru og tørre hender og føtter – mykgjør og beskytter.",
                LabelDescription = "Ekstra rik – Hender/Føtter",
                ProductPictures = { new() { PicturePath = "/images/products/Hand & Foot Creme Extreme, 100 gr.png" } },
                ProductCategories = { new() { Category = hudHender } },
                PhysicalProductAttributes ={new() { PhysicalAttribute = db.PhysicalAttributes.First(a => a.Attribute == "100gr") }}
            },
            new()
            {
                ProductName = "Foot Cream 75 ml",
                ProductPrice = 199,
                StockCount = 70,
                ProductDescription = "Pleier og frisker opp slitne føtter – trekker raskt inn.",
                LabelDescription = "Fotpleie",
                ProductPictures = { new() { PicturePath = "/images/products/footcream.png" } },
                ProductCategories = { new() { Category = hudFøtter } }
            },
            new()
            {
                ProductName = "ColdFace Kuldekrem 50 ml",
                ProductPrice = 279,
                StockCount = 30,
                ProductDescription = "Beskyttende kuldekrem som motvirker tørr hud i kaldt klima.",
                LabelDescription = "Beskyttende – Kuldekrem",
                ProductPictures = { new() { PicturePath = "/images/products/coldcream.png" } },
                ProductCategories = { new() { Category = hudKropp } }
            },
            new()
            {
                ProductName = "Victory Active Muscle 120 ml",
                ProductPrice = 189,
                StockCount = 55,
                ProductDescription = "Kjølende muskelkrem for restitusjon og lindring etter fysisk aktivitet.",
                LabelDescription = "Kjølende – Muskelpleie",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Active Muscle, 120 ml.png" } },
                ProductCategories = { new() { Category = massMuskel } }
            },
            new()
            {
                ProductName = "Victory Aloe Vera Sport Extreme 120 ml",
                ProductPrice = 219,
                StockCount = 50,
                ProductDescription = "Sterk kjølende sportskrem med Aloe Vera og mentol for økt sirkulasjon.",
                LabelDescription = "Sport – Aloe Vera",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Aloe Vera Sport Extreme 120 ml.png" } },
                ProductCategories = { new() { Category = aloeSport } }
            },
            new()
            {
                ProductName = "Victory Aloe Vera Strong Hot 120 ml",
                ProductPrice = 219,
                StockCount = 40,
                ProductDescription = "Varmende Aloe Vera-krem for lindring av stive og ømme muskler.",
                LabelDescription = "Varmende – Aloe Vera",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Aloe Vera Strong Hot, 120 m.png" } },
                ProductCategories = { new() { Category = aloeSport } }
            },
            new()
            {
                ProductName = "Victory Therapeutic Massage 250 ml",
                ProductPrice = 249,
                StockCount = 30,
                ProductDescription = "Massasjekrem med behagelig tekstur – ideell for velvære og terapi.",
                LabelDescription = "Massasje – Terapi",
                ProductPictures = { new() { PicturePath = "/images/products/Victory Therapeutic Massage, 250 ml.png" } },
                ProductCategories = { new() { Category = massOlje } }
            },
            new()
            {
                ProductName = "EmuMedica EmuCream 120 ml",
                ProductPrice = 249,
                StockCount = 45,
                ProductDescription = "Multifunksjonell krem med Emu-olje som roer ned sensitiv hud.",
                LabelDescription = "Hudpleie – Kropp",
                ProductPictures = { new() { PicturePath = "/images/products/EmuMedica EmuCream, 120 ml.png" } },
                ProductCategories = { new() { Category = hudKropp } }
            },
            new()
            {
                ProductName = "HeatMed AloeV Cold 125 ml",
                ProductPrice = 239,
                StockCount = 30,
                ProductDescription = "Kjølende Aloe Vera-gel som lindrer overanstrengte muskler og ledd.",
                LabelDescription = "Aloe Vera – Massasje",
                ProductPictures = { new() { PicturePath = "/images/products/HeatMed AloeV Cold, 125 ml.png" } },
                ProductCategories = { new() { Category = aloeMassasje } }
            },
            new()
            {
                ProductName = "Crystal Clean Desinfiserende Håndgel 5L",
                ProductPrice = 399,
                StockCount = 20,
                ProductDescription = "Effektiv hånddesinfeksjon for profesjonell bruk – 70% alkohol.",
                LabelDescription = "Desinfeksjon – Hender",
                ProductPictures = { new() { PicturePath = "/images/products/Crystal Clean Desinfiserende Håndgel 5 liter.png" } },
                ProductCategories = { new() { Category = hygHånd } }
            },
            new()
            {
                ProductName = "Eco-Bac 85% Håndsprit 1L",
                ProductPrice = 199,
                StockCount = 40,
                ProductDescription = "Håndsprit med 85% alkohol – effektiv og mild mot huden.",
                LabelDescription = "Desinfeksjon – Hender",
                ProductPictures = { new() { PicturePath = "/images/products/Eco-Bac 85% Håndsprit 1 liter.png" } },
                ProductCategories = { new() { Category = hygHånd } }
            }
        };
        
        
        db.Products.AddRange(produkter);
        db.SaveChanges();
    }
}
