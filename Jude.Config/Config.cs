using dotenv.net;

namespace Jude.Config;

public static class AppConfig
{
    public static void Initialize()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.Equals(environment, "Development"))
        {
            DotEnv.Load();
        }
    }

    public static DatabaseOptions DatabaseOptions { get; } =
    new()
    {
        ConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ??
                            throw new Exception("'DATABASE_URL' environment variable is not set."),
    };
}


public class DatabaseOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}