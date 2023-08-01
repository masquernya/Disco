using System.Text.Json.Serialization;
using Disco.Web;
using Disco.Web.Exceptions;
using Disco.Web.Middleware;
using Disco.Web.Models;
using Disco.Web.Services;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5194");
// Add services to the container.
Config.frontendUrl = builder.Configuration.GetValue<string>("FrontedUrl");
Config.adminUserId = builder.Configuration.GetValue<long>("AdminUserId");
Config.hcaptchaPrivate = builder.Configuration.GetValue<string>("HCaptcha:Private");
Config.hcatpchaPublic = builder.Configuration.GetValue<string>("HCaptcha:Public");
var  FrontendCorsPolicy = "_frontendCorsAllowSpecificOrigins";
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
builder.Services.AddSwaggerGen();
var clientId = builder.Configuration.GetValue<string>("DiscordOauth:ClientId");
var clientSecret = builder.Configuration.GetValue<string>("DiscordOauth:Secret");
var redirect = builder.Configuration.GetValue<string>("DiscordOauth:Redirect");
// DI
var discord = new DiscordService(clientId, clientSecret, redirect);
builder.Services.AddTransient<IUserService>(_ => new UserService());
builder.Services.AddSingleton<IDiscordService>(discord);
builder.Services.AddSingleton<IRateLimitService>(_ => new InMemoryRateLimitService());
builder.Services.AddSingleton<ICaptchaService>(_ => new CaptchaService());

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

app.UseAuthorization();

app.MapControllers();

app.Run();