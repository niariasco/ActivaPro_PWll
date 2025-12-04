using ActivaPro.Application.Profiles;
using ActivaPro.Application.Services.Implementations;
using ActivaPro.Application.Services.Interfaces;
using ActivaPro.Infraestructure.Data;
using ActivaPro.Infraestructure.Repository.Implementations;
using ActivaPro.Infraestructure.Repository.Interfaces;
using ActivaPro.Web.Hubs;
using ActivaPro.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// MVC + Localización de vistas
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();

// Autenticación (cookies)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.LogoutPath = "/Account/Logout";
        opt.AccessDeniedPath = "/Account/AccessDenied";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Repositorios
builder.Services.AddTransient<IRepoTecnico, TecnicoRepo>();
builder.Services.AddTransient<IRepoCategorias, CategoriasRepo>();
builder.Services.AddTransient<IRepoTicketes, TicketesRepo>();
builder.Services.AddTransient<IRepoAsignaciones, AsignacionesRepo>();
builder.Services.AddScoped<IRepoEtiquetas, EtiquetasRepo>();
builder.Services.AddScoped<IRepoEspecialidades, EspecialidadesRepo>();
builder.Services.AddScoped<IRepoSLA_Tickets, SLA_TicketsRepo>();
builder.Services.AddScoped<IRepoUsuarios, UsuariosRepo>();

// Servicios
builder.Services.AddTransient<ITecnicoService, TecnicoService>();
builder.Services.AddTransient<ICategoriaService, CategoriaService>();
builder.Services.AddTransient<ITicketesService, TicketesService>();
builder.Services.AddTransient<IAsignacionesService, AsignacionesService>();
builder.Services.AddScoped<IEtiquetasService, EtiquetasService>();
builder.Services.AddScoped<IEspecialidadesService, EspecialidadesService>();
builder.Services.AddScoped<ISlaService, SlaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Notificaciones (SignalR)
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<INotificacionRealtimeSender, SignalRNotificacionRealtimeSender>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, ActivaPro.Web.Security.UserIdProvider>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<TecnicoProfile>());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<CategoriaProfile>());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<TicketesProfile>());
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<TicketesProfile>();
    cfg.AddProfile<AsignacionesProfile>();
});

// DB
builder.Services.AddDbContext<ActivaProContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDataBase"));
    if (builder.Environment.IsDevelopment()) opt.EnableSensitiveDataLogging();
});

// Serilog
var logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Information)
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File(@"Logs\Info-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File(@"Logs\Error-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Host.UseSerilog(logger);

var app = builder.Build();
app.UseSerilogRequestLogging();

// Localización: cookie y querystring antes de routing
var supportedCultures = new[] { new CultureInfo("es"), new CultureInfo("en") };
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
locOptions.RequestCultureProviders.Clear();
locOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());
locOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
app.UseRequestLocalization(locOptions);

// Error pages / HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Hub notificaciones
app.MapHub<NotificacionesHub>("/hubs/notificaciones");

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Redirección a Login (excepto rutas públicas)
app.Use(async (ctx, next) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
    {
        var path = ctx.Request.Path.Value?.ToLower();
        if (!path!.StartsWith("/account/login") &&
            !path.StartsWith("/account/register") &&
            !path.StartsWith("/account/accessdenied") &&
            !path.StartsWith("/account/forgotpassword") &&
            !path.StartsWith("/language/set") &&
            !path.StartsWith("/css") &&
            !path.StartsWith("/js") &&
            !path.StartsWith("/lib") &&
            !path.StartsWith("/hubs"))
        {
            ctx.Response.Redirect("/Account/Login");
            return;
        }
    }
    await next();
});

app.Run();