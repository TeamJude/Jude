using System.Reflection;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Extensions;
using Jude.Server.Middleware;
using Microsoft.AspNetCore.Mvc;

//init environment var values into our appconfig
AppConfig.Initialize();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureDatabase();
builder.Services.ConfigureAuthentication();
builder.Services.AddServices();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.ConfigureCors();

// Override default model validation response to return our custom ApiResponse format instead of default response for consistent API errors.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = (actionContext) =>
    {
        var errors = actionContext
            .ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = Result.Fail(errors);
        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

app.Map("/__embedded_resources", (HttpContext context) =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceNames = assembly.GetManifestResourceNames();
    context.Response.ContentType = "text/plain";
    return context.Response.WriteAsync("Embedded Resources:\n" + string.Join("\n", resourceNames));
});

app.UseCors("jude");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase), builder =>
{
    builder.UseMiddleware<Jude.Server.Middleware.EmbeddedFrontendMiddleware>(
        Assembly.GetExecutingAssembly(),
        "",
        "index.html"
    );
});


app.Run();
