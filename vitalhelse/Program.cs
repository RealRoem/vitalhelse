using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using VitalHelse.Services;
using VitalHelse.Configuration;
using VitalHelse.Data;
using VitalHelse.Models;
using DiscountService = VitalHelse.Services.DiscountService;

// Optional .env file (Stripe, SendGrid, SMTP keys). Loaded before the builder so it reaches IConfiguration.
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var features = AppFeatures.From(builder.Configuration);
builder.Services.AddSingleton(features);
StripeConfiguration.ApiKey = builder.Configuration["STRIPE_SECRET_KEY"];

// Add services to the container.
// The database lives next to the project, independent of the working directory.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? $"DataSource={Path.Combine(builder.Environment.ContentRootPath, "app.db")}";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString, sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUsers>(options => options.SignIn.RequireConfirmedAccount = features.EmailEnabled)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options => { options.ViewLocationExpanders.Add(new AdminViewLocationExpander()); });

//required for session storage
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    //session expires in 30 min
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    //prevents js access
    options.Cookie.HttpOnly = true;     
    //required for GDPR compliance
    options.Cookie.IsEssential = true;           
});

builder.Services.AddRouting(options => { options.LowercaseUrls = true; });

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddScoped<TripletexService>();
builder.Services.AddScoped<TripletexSyncService>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddHostedService<CampaignScheduler>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<CartPricingService>();
builder.Services.AddScoped<OrderService>();

builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = builder.Configuration["STRIPE_PUBLISHABLE_KEY"];
    options.SecretKey = builder.Configuration["STRIPE_SECRET_KEY"];
    options.WebhookSecret = builder.Configuration["STRIPE_WEBHOOK_SECRET"];
    options.Domain = builder.Configuration["DOMAIN"];
});

builder.Services.Configure<EmailConfig>(options =>
{
    options.Host = builder.Configuration["EMAIL__HOST"];
    options.UserName = builder.Configuration["EMAIL__USERNAME"];
    options.Password = builder.Configuration["EMAIL__PASSWORD"];
});

builder.Services.AddTransient<EmailService>();
if (features.EmailEnabled)
    builder.Services.AddTransient<IEmailSender, RegisterAndForgottenPassService>();
else
    builder.Services.AddTransient<IEmailSender, LoggingEmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(options =>
{
    options.SenderGridKey = builder.Configuration["AUTHMESSAGESENDEROPTIONS__SENDERGRIDKEY"];
});

builder.Services
    .AddAuthentication();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Privileged", policy =>
        policy.RequireRole("Admin", "Staff"));



var app = builder.Build();

// Create and seed the database before serving requests
using (var services = app.Services.CreateScope())
{
    var um = services.ServiceProvider.GetRequiredService<UserManager<AspNetUsers>>();
    var rm = services.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await ApplicationDbInitializer.InitializeAsync(db, um, rm, app.Configuration.GetValue<bool>("Demo:ResetDatabase"));
    if (app.Configuration.GetValue("Demo:SeedData", true))
    {
        TestData.Initialize(db);
        await DemoData.InitializeAsync(db, um, services.ServiceProvider.GetRequiredService<CampaignService>(), app.Environment.WebRootPath);
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();

// Numbers, dates and prices are shown the Norwegian way on every machine, whatever the server's language is
var norwegian = Pricing.Culture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(norwegian),
    SupportedCultures = new[] { norwegian },
    SupportedUICultures = new[] { norwegian }
});
app.UseRouting();
app.UseSession();
app.UseAuthentication();

// Auto-logout middleware: hvis cookie er gyldig men brukeren ikke finnes i DB
app.Use(async (context, next) =>
{
    var userManager = context.RequestServices.GetRequiredService<UserManager<AspNetUsers>>();
    var user = await userManager.GetUserAsync(context.User);

    // Hvis cookien sier at du er logget inn, men brukeren ikke finnes → logg ut
    if (context.User.Identity?.IsAuthenticated == true && user == null)
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

if (features.TripletexEnabled)
{
    // Sync in the background so startup never waits on the network
    app.Lifetime.ApplicationStarted.Register(() => _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<TripletexSyncService>();
        var result = await syncService.SyncProductsAsync();
        Console.WriteLine($"Tripletex Sync Completed: Added={result.added}, Updated={result.updated}, Hidden={result.hidden}");
    }));
}
app.MapControllers();

app.Run();