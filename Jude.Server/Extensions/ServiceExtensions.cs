using System.Text;
using Jude.Server.Config;
using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using Jude.Server.Data.Repository;
using Jude.Server.Domains.Agents;
using Jude.Server.Domains.Agents.Events;
using Jude.Server.Domains.Agents.Plugins;
using Jude.Server.Domains.Agents.Workflows;
using Jude.Server.Domains.Auth;
using Jude.Server.Domains.Auth.Authorization;
using Jude.Server.Domains.Claims;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Jude.Server.Domains.Fraud;
using Jude.Server.Domains.Rules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Jude.Server.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureDatabase(this IServiceCollection services)
    {
        var db = new NpgsqlDataSourceBuilder(AppConfig.Database.ConnectionString)
            .EnableDynamicJson()
            .Build();
        services.AddDbContext<JudeDbContext>(options =>
        {
            options.UseNpgsql(db);
        });
    }

    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                "Bearer",
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        SaveSigninToken = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = AppConfig.JwtConfig.Issuer,
                        ValidAudience = AppConfig.JwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(AppConfig.JwtConfig.Secret)
                        ),
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            ctx.Request.Cookies.TryGetValue(
                                Constants.AccessTokenCookieName,
                                out var accessToken
                            );

                            if (!string.IsNullOrEmpty(accessToken))
                                ctx.Token = accessToken;

                            return Task.CompletedTask;
                        },
                    };
                }
            );

        return services;
    }

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var feature in Features.All)
            {
                options.AddPolicy(
                    $"{feature}.Read",
                    policy =>
                        policy.Requirements.Add(new PermissionRequirement(feature, Permission.Read))
                );
                options.AddPolicy(
                    $"{feature}.Write",
                    policy =>
                        policy.Requirements.Add(
                            new PermissionRequirement(feature, Permission.Write)
                        )
                );
            }
        });

        services.AddScoped<IAuthorizationHandler, PermissionsAuthorizationHandler>();
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "jude",
                policy =>
                {
                    policy
                        .WithOrigins(AppConfig.Client.Url)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            );
        });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRulesService, RulesService>();
        services.AddScoped<IFraudService, FraudService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();

        services.AddSingleton(AppConfig.CIMAS);

        services.AddHttpClient<ICIMASProvider, CIMASProvider>();
        services.AddScoped<ICIMASProvider, CIMASProvider>();

        return services;
    }

    public static IServiceCollection AddAgentServices(this IServiceCollection services)
    {
        // Event Queue - Singleton for in-memory queue
        services.AddSingleton<IClaimIngestEventsQueue, ClaimIngestEventsQueue>();

        // Event Handler - Scoped for database access
        services.AddScoped<IClaimIngestEventHandler, ClaimIngestEventHandler>();

        // AI Agents - Scoped for database and service access
        services.AddScoped<Domains.Agents.Jude>();
        services.AddScoped<IAgentManager, AgentManager>();
        services.AddScoped<Orchestrator>();

        // Background Services
        services.AddHostedService<ClaimsIngestProcessor>();

        return services;
    }
}
