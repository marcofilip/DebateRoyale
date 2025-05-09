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