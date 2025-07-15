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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<JudeDbContext>();

    context.Database.Migrate();

    var passwordHasher = services.GetRequiredService<IPasswordHasher<UserModel>>();
    await DbSeeder.SeedData(context, passwordHasher);

    // Test CIMAS Pricing API
    try
    {
        var cimasProvider = services.GetRequiredService<Jude.Server.Domains.Claims.Providers.CIMAS.ICIMASProvider>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== Testing CIMAS Pricing API ===");

        // Test with tariff code - replace with your actual tariff number
        string testTariffCode = "01011"; // You can replace this with your tariff number

        logger.LogInformation("Getting pricing access token...");
        var tokenResult = await cimasProvider.GetPricingAccessTokenAsync();

        if (tokenResult.Success)
        {
            logger.LogInformation("✅ Successfully obtained pricing access token");

            logger.LogInformation("Looking up tariff code: {TariffCode}", testTariffCode);
            var tariffInput = new Jude.Server.Domains.Claims.Providers.CIMAS.TariffLookupInput(testTariffCode, tokenResult.Data!);
            var tariffResult = await cimasProvider.GetTariffByCodeAsync(tariffInput);

            if (tariffResult.Success)
            {
                var tariff = tariffResult.Data!;
                logger.LogInformation("✅ Successfully retrieved tariff:");
                logger.LogInformation("   Code: {Code}", tariff.Code);
                logger.LogInformation("   Description: {Description}", tariff.Description);
                logger.LogInformation("   Packages Count: {PackageCount}", tariff.Packages.Count);

                foreach (var package in tariff.Packages.Take(3)) // Show first 3 packages
                {
                    logger.LogInformation("   Package: {PackageName} - {Currency} {Amount}",
                        package.Name.Package, package.Currency, package.Amount);
                }
            }
            else
            {
                logger.LogError("❌ Failed to get tariff: {Errors}", string.Join(", ", tariffResult.Errors));
            }
        }
        else
        {
            logger.LogError("❌ Failed to get pricing access token: {Errors}", string.Join(", ", tokenResult.Errors));
        }

        logger.LogInformation("=== End CIMAS Pricing API Test ===");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error during CIMAS Pricing API test");
    }
}

app.Run();
