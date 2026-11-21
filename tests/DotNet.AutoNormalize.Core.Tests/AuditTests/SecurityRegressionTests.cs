using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests.AuditTests;

public class SecurityRegressionTests
{
    public class SecurityEnvelope
    {
        public string? NormalField { get; set; }
        public string? Password { get; set; }
        public string? ApiKey { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? SecretKey { get; set; }
        public string? ClientSecret { get; set; }
        public string? Signature { get; set; }
        public string? PasswordHash { get; set; }

        [DoNotNormalize]
        public string? Base64Payload { get; set; }

        [PreserveWhitespace]
        public string? MarkdownDocumentation { get; set; }

        public NestedSecurity? NestedCredentials { get; set; }
    }

    public class NestedSecurity
    {
        public string? UserPassword { get; set; }
        public string? ServiceToken { get; set; }
        public string? NormalSubField { get; set; }
    }

    [Fact]
    public void SecurityAudit_AllSensitivePropertiesPreserved()
    {
        var normalizer = new AutoNormalizer();
        var envelope = new SecurityEnvelope
        {
            NormalField = "   Normal Input   ",
            Password = "   correct horse battery staple   ",
            ApiKey = "   sk_live_1234567890   ",
            AccessToken = "   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9   ",
            RefreshToken = "   dGhpc19pc19hX3JlZnJlc2hfdG9rZW4=   ",
            SecretKey = "   aws_secret_access_key   ",
            ClientSecret = "   oauth_client_secret_xyz   ",
            Signature = "   MEQCIDg8Vp3F7B...   ",
            PasswordHash = "   $2a$12$K1...   ",
            Base64Payload = "   aGVsbG8gd29ybGQ=   ",
            MarkdownDocumentation = "   # Title\n    code block indent",
            NestedCredentials = new NestedSecurity
            {
                UserPassword = "   nested_pass_123   ",
                ServiceToken = "   svc_token_456   ",
                NormalSubField = "   clean me   "
            }
        };

        normalizer.Normalize(envelope);

        // 1. Normal field is normalized
        Assert.Equal("Normal Input", envelope.NormalField);

        // 2. Sensitive fields are preserved exactly
        Assert.Equal("   correct horse battery staple   ", envelope.Password);
        Assert.Equal("   sk_live_1234567890   ", envelope.ApiKey);
        Assert.Equal("   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9   ", envelope.AccessToken);
        Assert.Equal("   dGhpc19pc19hX3JlZnJlc2hfdG9rZW4=   ", envelope.RefreshToken);
        Assert.Equal("   aws_secret_access_key   ", envelope.SecretKey);
        Assert.Equal("   oauth_client_secret_xyz   ", envelope.ClientSecret);
        Assert.Equal("   MEQCIDg8Vp3F7B...   ", envelope.Signature);
        Assert.Equal("   $2a$12$K1...   ", envelope.PasswordHash);

        // 3. Declarative opt-outs are preserved
        Assert.Equal("   aGVsbG8gd29ybGQ=   ", envelope.Base64Payload);
        Assert.Equal("   # Title\n    code block indent", envelope.MarkdownDocumentation);

        // 4. Nested sensitive fields are preserved
        Assert.NotNull(envelope.NestedCredentials);
        Assert.Equal("   nested_pass_123   ", envelope.NestedCredentials.UserPassword);
        Assert.Equal("   svc_token_456   ", envelope.NestedCredentials.ServiceToken);
        Assert.Equal("clean me", envelope.NestedCredentials.NormalSubField);
    }
}
