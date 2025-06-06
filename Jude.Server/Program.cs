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
builder.Services.ConfigureAuthorization();
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();
app.UseExceptionHandler(options => { });

app.UseCors("jude");

app.Run();
