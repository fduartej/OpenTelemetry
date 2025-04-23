using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReFactoring.Models;
using ReFactoring.Data;

namespace ReFactoring.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private readonly  ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger,IConfiguration config, ApplicationDbContext context)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }

    public IActionResult Index()
    {

        /*using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CustomerId"] = "12345",
            ["CustomerName"] = "John Doe",
            ["RequestId"] = HttpContext.TraceIdentifier
        }))
        {
            _logger.LogInformation("Inicio del proceso del cliente");
            _logger.LogInformation("Otra operación interna");
            _logger.LogWarning("Algo no salió como se esperaba");




        }*/
        _logger.LogInformation("Probando log desde HomeController a Application Insights.");
        _logger.LogWarning("Este es un warning de prueba.");
        _logger.LogError("Simulando un error en el controlador.");

        var customers =  _context.DbSetCustomer.ToList();

        _logger.LogInformation("Fetched {1} products from the database.", customers.Count);


        var usuario = _config["APPPortal-BasicAuthUser"];
        var clave = _config["APPPortal-BasicAuthPassword"];
        var getdata = _config["APPPortal-GetData"];
        var conexion = _config["SQLFNB-DefaultConnection"];


        _logger.LogInformation( $"Usuario: {usuario}, Conexión: {conexion}, getdata: {getdata}");
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
