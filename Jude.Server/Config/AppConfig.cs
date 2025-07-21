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

    public static CIMASConfig CIMAS { get; } =
        new CIMASConfig(
            Environment.GetEnvironmentVariable("CIMAS_CLAIMS_SWITCH_ENDPOINT")
                ?? throw new Exception("CIMAS Claims Switch Endpoint is not set"),
            Environment.GetEnvironmentVariable("CIMAS_ACCOUNT_NAME")
                ?? throw new Exception("CIMAS Account Name is not set"),
            Environment.GetEnvironmentVariable("CIMAS_ACCOUNT_PASSWORD")
                ?? throw new Exception("CIMAS Account Password is not set"),
            Environment.GetEnvironmentVariable("CIMAS_PRACTICE_NUMBER")
                ?? throw new Exception("CIMAS Practice number is not set"),
            Environment.GetEnvironmentVariable("CIMAS_PRICING_API_ENDPOINT")
                ?? throw new Exception("CIMAS Pricing API Endpoint is not set"),
            Environment.GetEnvironmentVariable("CIMAS_PRICING_API_USERNAME")
                ?? throw new Exception("CIMAS Pricing API Username is not set"),
            Environment.GetEnvironmentVariable("CIMAS_PRICING_API_PASSWORD")
                ?? throw new Exception("CIMAS Pricing API Password is not set")
        );

    public static AzureAI AzureAI { get; } =
        new AzureAI(
            "gpt-4.1",
            Environment.GetEnvironmentVariable("AZURE_AI_ENDPOINT")
                ?? throw new Exception("AZURE AI Endpoint is not set"),
            Environment.GetEnvironmentVariable("AZURE_AI_APIKEY")
                ?? throw new Exception("AZURE AI api key is not set")
        );
}

public record Database(string ConnectionString);

public record Client(string Url);

public record JwtConfig(string Secret, string Issuer, string Audience);

public record CIMASConfig(
    string ClaimsSwitchEndpoint,
    string AccountName,
    string AccountPassword,
    string PracticeNumber,
    string PricingApiEndpoint,
    string PricingApiUsername,
    string PricingApiPassword
);

public record AzureAI(string ModelId, string Endpoint, string ApiKey);
