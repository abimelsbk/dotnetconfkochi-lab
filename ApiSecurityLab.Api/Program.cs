using ApiSecurityLab.Api.Authorization;
using ApiSecurityLab.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InvoicesDb"));

// AUTHENTICATION: who are you? Reads and validates the JWT.
// Settings come from appsettings.Development.json (written by dotnet user-jwts).
builder.Services.AddAuthentication("Bearer").AddJwtBearer();

// AUTHORIZATION: what are you allowed to do?
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InvoiceOwner", policy =>
        policy.Requirements.Add(new InvoiceOwnerRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, InvoiceAuthorizationHandler>();

var app = builder.Build();

// Load fake data: Alice owns 101 and 102, Bob owns 202.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(db);
}

// The demo web page (wwwroot/index.html).
app.UseDefaultFiles();
app.UseStaticFiles();

// Order matters: authentication first, then authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the test project start this app.
public partial class Program { }
