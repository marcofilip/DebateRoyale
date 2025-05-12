using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DebateRoyale.Data;
using DebateRoyale.Models;
using DebateRoyale.Hubs;
using DebateRoyale.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3; 
    options.User.RequireUniqueEmail = false; 
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Stanze"); 
    options.Conventions.AuthorizePage("/Stanza");  
});

builder.Services.AddSignalR();

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
    string adminPassword = configuration["AdminUser:Password"] ?? "PASSWORD_HERE"; 

    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
    }

    var adminUser = await userManager.FindByNameAsync(adminUsername);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminUsername, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
        else
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            foreach (var error in result.Errors)
            {
                logger.LogError("Error creating admin user: {ErrorDescription}", error.Description);
            }
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}

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
app.UseAuthentication(); 
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<StanzaHub>("/stanzaHub"); 

app.Run();