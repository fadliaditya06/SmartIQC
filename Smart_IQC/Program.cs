using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Smart_IQC.Function;
using Smart_IQC.Service;
using Smart_IQC.Function.Data;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var configuration = builder.Configuration;

string connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=Smart_IQC;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Smart_IQC.Function.DatabaseAccessLayer>();
builder.Services.AddTransient<ITokenService, TokenService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAny", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("RequireAdmin", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("RequireInspector", policy => policy.RequireAuthenticatedUser());
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();


app.Use(async (context, next) =>
{
    // Call the next middleware in the pipeline
    await next();

    // If the response status is 403 Forbidden
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
    {
        // Redirect to the Access Denied page
        context.Response.Redirect("/");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
