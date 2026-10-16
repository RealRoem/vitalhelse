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

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUsers>(options => options.SignIn.RequireConfirmedAccount = true)
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
builder.Services.AddScoped<DiscountService>();

builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");
    options.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
    options.Domain = Environment.GetEnvironmentVariable("DOMAIN");
});

builder.Services.Configure<EmailConfig>(options =>
{
    options.Host = Environment.GetEnvironmentVariable("EMAIL__HOST");
    options.UserName = Environment.GetEnvironmentVariable("EMAIL__USERNAME");
    options.Password = Environment.GetEnvironmentVariable("EMAIL__PASSWORD");
});

builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<IEmailSender, RegisterAndForgottenPassService>();
builder.Services.Configure<AuthMessageSenderOptions>(options =>
{
    options.SenderGridKey = Environment.GetEnvironmentVariable("AUTHMESSAGESENDEROPTIONS__SENDERGRIDKEY");
});

builder.Services
    .AddAuthentication();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Privileged", policy =>
        policy.RequireRole("Admin", "Staff"));



var app = builder.Build();

// Call the database initializer
using (var services = app.Services.CreateScope())
{
    var um = services.ServiceProvider.GetRequiredService<UserManager<AspNetUsers>>();
    var rm = services.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    var env = services.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    
    ApplicationDbInitializer.InitializeAsync(db, um, rm);
    TestData.Initialize(db, env);
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

app.UseHttpsRedirection();
app.UseStaticFiles();
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

using (var scope = app.Services.CreateScope())
{
    var syncService = scope.ServiceProvider.GetRequiredService<TripletexSyncService>();
    var result = await syncService.SyncProductsAsync();
    Console.WriteLine($"Tripletex Sync Completed: Added={result.added}, Updated={result.updated}, Hidden={result.hidden}");
}
app.MapControllers();

app.Run();