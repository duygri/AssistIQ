using System.Text;
using Npgsql;

namespace AssistIQ.Api.Security;

public static class ProductionSecurityValidator
{
    private const string DemoSigningKey = "dev-only-signing-key-change-before-production-32chars";
    private const string DemoDatabasePassword = "postgres";
    private const int MinimumSigningKeyBytes = 32;

    public static void Validate(IConfiguration configuration, bool isProduction)
    {
        if (!isProduction)
        {
            return;
        }

        var signingKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) ||
            Encoding.UTF8.GetByteCount(signingKey) < MinimumSigningKeyBytes ||
            signingKey.Equals(DemoSigningKey, StringComparison.Ordinal))
        {
            throw InvalidConfiguration();
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw InvalidConfiguration();
        }

        try
        {
            var databaseOptions = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(databaseOptions.Password) ||
                databaseOptions.Password.Equals(DemoDatabasePassword, StringComparison.Ordinal))
            {
                throw InvalidConfiguration();
            }
        }
        catch (ArgumentException)
        {
            throw InvalidConfiguration();
        }

        if (configuration.GetValue<bool>("SeedDemoDataOnStartup"))
        {
            throw InvalidConfiguration();
        }
    }

    private static InvalidOperationException InvalidConfiguration()
    {
        return new InvalidOperationException(
            "Production security configuration is invalid. Configure non-demo credentials and disable demo seeding.");
    }
}
