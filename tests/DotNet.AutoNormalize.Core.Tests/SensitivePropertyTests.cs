using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class SensitivePropertyTests
{
    public class CredentialsModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? UserPassword { get; set; }
        public string? AuthToken { get; set; }
        public string? ApiKey { get; set; }
        public string? ClientSecret { get; set; }
        public string? Signature { get; set; }
    }

    [Fact]
    public void Normalize_SensitiveProperties_ShieldedByDefault()
    {
        var normalizer = new AutoNormalizer();
        var model = new CredentialsModel
        {
            Username = "   alice_admin   ",
            Password = "  secret password with spaces  ",
            UserPassword = "  another secret  ",
            AuthToken = "  bearer_token_12345  ",
            ApiKey = "  sk_live_98765  ",
            ClientSecret = "  client_secret_xyz  ",
            Signature = "  cryptographic_signature  "
        };

        normalizer.Normalize(model);

        // Standard property is normalized
        Assert.Equal("alice_admin", model.Username);

        // Sensitive properties are preserved
        Assert.Equal("  secret password with spaces  ", model.Password);
        Assert.Equal("  another secret  ", model.UserPassword);
        Assert.Equal("  bearer_token_12345  ", model.AuthToken);
        Assert.Equal("  sk_live_98765  ", model.ApiKey);
        Assert.Equal("  client_secret_xyz  ", model.ClientSecret);
        Assert.Equal("  cryptographic_signature  ", model.Signature);
    }

    [Fact]
    public void Normalize_ProtectSensitivePropertiesDisabled_TrimsPasswords()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            ProtectSensitiveProperties = false
        });

        var model = new CredentialsModel
        {
            Username = "   alice   ",
            Password = "   unprotected_password   "
        };

        normalizer.Normalize(model);

        Assert.Equal("alice", model.Username);
        Assert.Equal("unprotected_password", model.Password);
    }
}
