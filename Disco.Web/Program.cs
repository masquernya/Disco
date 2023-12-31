using System.Text.Json.Serialization;
using Disco.Web;
using Disco.Web.Data;
using Disco.Web.Exceptions;
using Disco.Web.Middleware;
using Disco.Web.Models;
using Disco.Web.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5194");
// Add services to the container.
Config.frontendUrl = builder.Configuration.GetValue<string>("FrontedUrl");
Config.adminUserId = builder.Configuration.GetValue<long>("AdminUserId");
Config.hcaptchaPrivate = builder.Configuration.GetValue<string>("HCaptcha:Private");
Config.hcatpchaPublic = builder.Configuration.GetValue<string>("HCaptcha:Public");
var dbFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
DiscoContext.dbPath = System.IO.Path.Join(dbFolderPath, "disco.db");
var FrontendCorsPolicy = "_frontendCorsAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: FrontendCorsPolicy,
        policy  =>
        {
            policy.WithOrigins(Config.frontendUrl);
            policy.AllowCredentials();
            policy.WithHeaders("content-type", "xsrf");
            policy.WithExposedHeaders("xsrf", "content-type", "cache-control");
            policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
        });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(g =>
{
    g.SchemaGeneratorOptions.SchemaIdSelector = (type) => type.FullName;
});
var serverBaseUrl = builder.Configuration.GetValue<string>("BackendBaseUrl");
var clientId = builder.Configuration.GetValue<string>("DiscordOauth:ClientId");
var clientSecret = builder.Configuration.GetValue<string>("DiscordOauth:Secret");
var redirect = builder.Configuration.GetValue<string>("DiscordOauth:Redirect");
var fullImagePath = builder.Configuration.GetValue<string>("FullImagePath");
var botKey = builder.Configuration.GetValue<string>("BotAuthorizationKey");
UserUploadedImage.baseUrl = serverBaseUrl;
// DI
var discord = new DiscordService(clientId, clientSecret, redirect);
UserService.Configure(fullImagePath);
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
// specializations
builder.Services.AddSingleton<IBotService>(new BotService(botKey));
builder.Services.AddSingleton<IDiscordService>(discord);
// everything else
var transientServices = Type.GetType("Disco.Web.Services.ITransientDiscoService`1, Disco.Web")!;
var singletonServices = Type.GetType("Disco.Web.Services.ISingletonDiscoService`1, Disco.Web")!;
var services = typeof(Program).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "Disco.Web.Services" && t.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == transientServices || i.GetGenericTypeDefinition() == singletonServices)));
foreach (var service in services)
{
    var interfaces = service.GetInterfaces().Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == transientServices || i.GetGenericTypeDefinition() == singletonServices));
    foreach (var @interface in interfaces)
    {
        var serviceType = @interface.GetGenericArguments()[0];
        if (@interface.GetGenericTypeDefinition() == transientServices)
        {
            builder.Services.AddTransient(serviceType, service);
        }
        else if (@interface.GetGenericTypeDefinition() == singletonServices)
        {
            builder.Services.AddSingleton(serviceType, service);
        }
    }
}

// App
var app = builder.Build();
app.UseCors(FrontendCorsPolicy);
app.UseXsrfMiddleware();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is BaseWebException err)
        {
            context.Response.StatusCode = (int)err.status;
            await context.Response.WriteAsJsonAsync(new HttpError()
            {
                message = err.message,
                code = err.code,
            });
        }
        else
        {
#if DEBUG
            await context.Response.WriteAsJsonAsync(new HttpError()
            {
                message = exceptionHandlerPathFeature?.Error?.Message ?? "Unknown error",
                code = exceptionHandlerPathFeature?.Error?.GetType().FullName ?? "UnknownType",
            });
#else
            await context.Response.WriteAsJsonAsync(new HttpError()
            {
                message = "Internal server error",
                code = "InternalServerError",
            });
#endif
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fullImagePath),
    RequestPath = "/user-content/images",
    ServeUnknownFileTypes = false,
    DefaultContentType = "image/webp",
    OnPrepareResponse = context =>
    {
        if (context.Context.Response.StatusCode == 200)
            context.Context.Response.Headers.CacheControl = new StringValues("public, max-age=7889238, immutable");
    }
});
app.UseAuthorization();

app.MapControllers();

app.Run();