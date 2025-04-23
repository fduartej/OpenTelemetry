using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.EntityFrameworkCore;
using ReFactoring.Data;
using ReFactoring.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración base desde archivo JSON
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables(); // Por si tienes overrides variables del sistema para el vault


// Cargar desde appsettings o variables de entorno
var config = builder.Configuration;
foreach (var kvp in config.AsEnumerable())
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
var tenantId = config["AzureAD:TenantId"];
var clientId = config["AzureAD:ClientId"];
var clientSecret = config["AzureAD:ClientSecret"];
var keyVaultUrl = config["KeyVault:VaultUri"];
Console.WriteLine($"TenantId: {tenantId}");
var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

// Configurar el Key Vault cargando los secretos
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
}
else
{
    Console.WriteLine("KeyVault URL is not configured.");
}

// Leer el valor del Key Vault
var appInsightsConnectionString = builder.Configuration["APPINSIGHTS-ConnectionString"];
if (string.IsNullOrEmpty(appInsightsConnectionString))
{
    Console.WriteLine("APPINSIGHTS-ConnectionString is not configured.");
}
var applicationName = builder.Configuration["applicationName"] ?? "AppFNB-OnPrem";

try
{
    builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddAzureMonitorTraceExporter(o =>
            {
                o.ConnectionString = appInsightsConnectionString;
            });
    });
     Console.WriteLine("Tracing configurado correctamente.");
}catch (Exception ex)
{
    Console.WriteLine($"Error al configurar OpenTelemetry: {ex.Message}");
}


try
{
    // Configurar el logging para Azure Monitor
    builder.Services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(); // Proveedor estándar de consola para logging
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddAzureMonitorLogExporter(o =>
            {
                o.ConnectionString = appInsightsConnectionString;
            });
        });
    });
    Console.WriteLine("Logging para Azure Monitor configurado correctamente.");
}catch (Exception ex)
{
    Console.WriteLine($"Error al configurar el logging para Azure Monitor: {ex.Message}");
}

// database connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("DataSource=app.db;Cache=Shared"));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseMiddleware<LoggingContextMiddleware>();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();