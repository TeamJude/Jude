using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Extensions;
using Jude.Server.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

//init environment var values into our appconfig
AppConfig.Initialize();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jude API", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition(
        "Bearer",   
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureDatabase();
builder.Services.ConfigureAuthentication();
builder.Services.ConfigureAuthorization();
builder.Services.AddServices();
builder.Services.AddAgentServices();
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

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();
app.UseExceptionHandler(options => { });

app.UseCors("jude");

app.MapPost("/api/agent/test", async ([FromServices] Jude.Server.Domains.Agents.AgentService agentService, [FromBody] string claimData) =>
{
    var result = await agentService.TestAgentAsync(claimData);
    if (result == null)
        return Results.BadRequest("Agent could not process the claim or returned invalid response.");
    return Results.Ok(result);
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<JudeDbContext>();

    context.Database.Migrate();

    var passwordHasher = services.GetRequiredService<IPasswordHasher<UserModel>>();
    await DbSeeder.SeedData(context, passwordHasher);
}

app.Run();
