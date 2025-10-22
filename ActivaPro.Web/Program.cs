using ActivaPro.Application.Profiles;
using ActivaPro.Application.Services.Implementations;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Repository.Implementations;
using ActivaPro.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar D.I.
//Repository 
//AUTOR - builder.Services.AddTransient<IRepositoryAutor, RepositoryAutor>();
//Services 
//AUTOR -builder.Services.AddTransient<IServiceAutor, ServiceAutor>();

//Configurar Automapper 
//AUTOR -
//builder.Services.AddAutoMapper(config =>
//{
//  config.AddProfile<AutorProfile>();
//});
// Configurar D.I.
//Repository
builder.Services.AddTransient<IRepoTecnico, TecnicoRepo>();
builder.Services.AddTransient<IRepoCategorias, CategoriasRepo>();
builder.Services.AddTransient<IRepoTicketes, TicketesRepo>();
builder.Services.AddTransient<IRepoAsignaciones, AsignacionesRepo>();

//Services
builder.Services.AddTransient<ITecnicoService, TecnicoService>();
builder.Services.AddTransient<ICategoriaService, CategoriaService>();
builder.Services.AddTransient<ITicketesService, TicketesService>();
builder.Services.AddTransient<IAsignacionesService, AsignacionesService>();

//Configurar Automapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<TecnicoProfile>();
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CategoriaProfile>();
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<TicketesProfile>();
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<TicketesProfile>();
    cfg.AddProfile<AsignacionesProfile>();
});


// Configuar Conexión a la Base de Datos SQL 
builder.Services.AddDbContext<ActivaProContext>(options =>
{
    // it read appsettings.json file 
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDataBase"));
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});




//***********************
//Configuración Serilog
// Logger. P.E. Verbose = muestra SQl Statement
var logger = new LoggerConfiguration()
                    // Limitar la información de depuración
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .Enrich.FromLogContext()
                    // Log LogEventLevel.Verbose muestra mucha información, pero no es necesaria solo para el proceso de depuración
                    .WriteTo.Console(LogEventLevel.Information)
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File(@"Logs\Info-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File(@"Logs\Debug-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File(@"Logs\Warning-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File(@"Logs\Error-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal).WriteTo.File(@"Logs\Fatal-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
                    .CreateLogger();

builder.Host.UseSerilog(logger);
//***************************

//Activar soporte a la solicitud de registrocon SERILOG 



var app = builder.Build();

app.UseSerilogRequestLogging();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Error control Middleware 
  //  app.UseMiddleware<ErrorHandlingMiddleware>();
}






app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
//Activar Antiforgery
app.UseAntiforgery();