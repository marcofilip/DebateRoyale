using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DebateRoyale.Data;
using DebateRoyale.Models;
using DebateRoyale.Hubs;
using DebateRoyale.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Simpler for this example
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3; // For easy testing
    options.User.RequireUniqueEmail = false; // Using Username as primary
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Stanze"); // Example: Secure all pages in /Stanze
    options.Conventions.AuthorizePage("/Stanza");   // Secure the Stanza.cshtml page
});

builder.Services.AddSignalR();

// Register custom services
builder.Services.AddSingleton<RoomStateService>();
builder.Services.AddSingleton<GeminiService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRolesAndAdminUser(userManager, roleManager, app.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

async Task SeedRolesAndAdminUser(UserManager<ApplicationUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IConfiguration configuration)
{
    string adminRoleName = "Admin";
    string adminEmail = configuration["AdminUser:Email"] ?? "admin@debateroyale.com";
    string adminUsername = configuration["AdminUser:Username"] ?? "admin";
    string adminPassword = configuration["AdminUser:Password"] ?? "AdminPa$$w0rd"; // Prendi da config sicura!

    // Assicura che il ruolo Admin esista
    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
    }

    // Assicura che l'utente Admin esista
    var adminUser = await userManager.FindByNameAsync(adminUsername);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminUsername, Email = adminEmail, EmailConfirmed = true }; // EmailConfirmed = true per bypassare la conferma
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            // Assegna il ruolo Admin all'utente Admin
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
        else
        {
            // Logga gli errori se la creazione dell'utente fallisce
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            foreach (var error in result.Errors)
            {
                logger.LogError("Error creating admin user: {ErrorDescription}", error.Description);
            }
        }
    }
    else
    {
        // Se l'utente esiste, assicurati che abbia il ruolo Admin
        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Crucial: Must be before UseAuthorization
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<StanzaHub>("/stanzaHub"); // Map SignalR Hub

app.Run();