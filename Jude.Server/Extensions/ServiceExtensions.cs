using Jude.Config;
using Jude.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Jude.Server.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services)
    {
        services.AddDbContext<JudeDbContext>(options =>
            options.UseNpgsql(AppConfig.DatabaseOptions.ConnectionString)
        );
        return services;
    }
}
