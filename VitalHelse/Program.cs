using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUsers>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

//required for session storage
builder.Services.AddDistributedMemoryCache(); 
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


var app = builder.Build();

// Call the database initializer
using (var services = app.Services.CreateScope())
{
    var um = services.ServiceProvider.GetRequiredService<UserManager<AspNetUsers>>();
    var rm = services.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    ApplicationDbInitializer.Initialize(db, um, rm);
    
    TestData.Initialize(db);
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
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();