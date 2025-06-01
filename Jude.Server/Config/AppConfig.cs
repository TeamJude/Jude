using dotenv.net;

namespace Jude.Server.Config;

public static class AppConfig
{
    public static void Initialize()
    {
        //get the current environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        //if we're in dev then load the env variables from the .env file, else in prod they will be set and loaded from whereever we are hosting our app
        if (string.Equals(environment, "Development"))
        {
            DotEnv.Load();
        }
    }

    public static Database Database { get; } =
        new Database(
            Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? throw new Exception("Database URL is not set")
        );

    public static Client Client { get; } =
        new Client(
            Environment.GetEnvironmentVariable("CLIENT_URL")
                ?? throw new Exception("Client URL is not set")
        );

    public static JwtConfig JwtConfig { get; } =
        new JwtConfig(
            Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new Exception("Jwt Secret is not set"),
            Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new Exception("Jwt Issuer is not set"),
            Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? throw new Exception("Jwt Audience is not set")
        );
}

public record Database(string ConnectionString);

public record Client(string Url);

public record JwtConfig(string Secret, string Issuer, string Audience);
