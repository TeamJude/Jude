using Jude.Server.Core.Helpers;
using Jude.Server.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure ASP.NET to return our standardized Result type for model validation errors.
// By default, ASP.NET controllers (with the [ApiController] attribute) perform automatic
// model validation on input DTOs
// If validation fails, the framework returns a 400 Bad Request with a ProblemDetails object (the default dotnet error result type).
// This override replaces that behavior by returning a custom Result.Fail(...) object instead,
// allowing us to maintain consistent error responses across the entire API.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = (actionContext) =>
    {
        var errors = actionContext
            .ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var errorResult = Result.Fail(errors);
        return new BadRequestObjectResult(errorResult);
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
