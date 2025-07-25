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

    public static Azure Azure { get; } =
        new Azure(
            new AzureAI(
                "gpt-4.1",
                Environment.GetEnvironmentVariable("AZURE_AI_ENDPOINT")
                    ?? throw new Exception("AZURE AI Endpoint is not set"),
                Environment.GetEnvironmentVariable("AZURE_AI_APIKEY")
                    ?? throw new Exception("AZURE AI api key is not set")
            ),
            new AzureSearch(
                Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_ENDPOINT")
                    ?? throw new Exception("AZURE AI Search Endpoint is not set"),
                Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_APIKEY")
                    ?? throw new Exception("AZURE AI Search API Key is not set")
            ),
            new AzureBlob(
                Environment.GetEnvironmentVariable("AZURE_BLOB_ACCOUNT")
                    ?? throw new Exception("AZURE Blob Account is not set"),
                Environment.GetEnvironmentVariable("AZURE_BLOB_ACESS_KEY")
                    ?? throw new Exception("AZURE Blob Access Key is not set"),
                Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER")
                    ?? throw new Exception("AZURE Blob Container is not set"),
                Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING")
                    ?? throw new Exception("AZURE Blob Connection String is not set")
            )
        );

    public static CustomAzureAI CustomAzureAI { get; } =
        new CustomAzureAI(
            "text-embedding-3-small",
            Environment.GetEnvironmentVariable("CUSTOM_AZURE_AI_ENDPOINT")
                ?? throw new Exception("Customer azure ai endpoint is not set"),
            Environment.GetEnvironmentVariable("CUSTOM_AZURE_AI_APIKEY")
                ?? throw new Exception("custom azure ai apikey is not set")
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

public record Azure(AzureAI AI, AzureSearch Search, AzureBlob Blob);

public record AzureAI(string ModelId, string Endpoint, string ApiKey);

public record AzureSearch(string Endpoint, string ApiKey);

public record AzureBlob(
    string Account,
    string AccessKey,
    string Container,
    string ConnectionString
);

public record CustomAzureAI(string ModelId, string Endpoint, string ApiKey);
