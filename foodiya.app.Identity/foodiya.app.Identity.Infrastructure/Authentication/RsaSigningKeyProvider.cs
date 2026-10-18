using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Authentication;

public sealed class RsaSigningKeyProvider : IRsaSigningKeyProvider, IDisposable
{
    private readonly RSA _rsa;
    private readonly string _keyId;
    private readonly RsaSecurityKey _signingKey;
    private readonly RsaSecurityKey _validationKey;
    private readonly JsonWebKeySet _publicJwks;
    private bool _disposed;

    public RsaSigningKeyProvider(
        IOptions<AuthenticationTokenOptions> options,
        ILogger<RsaSigningKeyProvider> logger,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(environment);

        var tokenOptions = options.Value;
        _keyId = string.IsNullOrWhiteSpace(tokenOptions.KeyId)
            ? "foodiya-app-identity-v1"
            : tokenOptions.KeyId;

        var hasPem = !string.IsNullOrWhiteSpace(tokenOptions.RsaPrivateKeyPem);
        var hasPath = !string.IsNullOrWhiteSpace(tokenOptions.RsaPrivateKeyPath);
        if (hasPem && hasPath)
        {
            throw new InvalidOperationException(
                "Configure either Tokens:RsaPrivateKeyPem or Tokens:RsaPrivateKeyPath, not both.");
        }

        _rsa = RSA.Create();
        try
        {
            if (hasPem)
            {
                _rsa.ImportFromPem(tokenOptions.RsaPrivateKeyPem);
                logger.LogInformation("Loaded RSA signing key from configuration PEM (KeyId: {KeyId}).", _keyId);
            }
            else if (hasPath)
            {
                if (!File.Exists(tokenOptions.RsaPrivateKeyPath))
                {
                    throw new InvalidOperationException(
                        $"The RSA private-key file '{tokenOptions.RsaPrivateKeyPath}' does not exist.");
                }

                _rsa.ImportFromPem(File.ReadAllText(tokenOptions.RsaPrivateKeyPath));
                logger.LogInformation(
                    "Loaded RSA signing key from file {Path} (KeyId: {KeyId}).",
                    tokenOptions.RsaPrivateKeyPath,
                    _keyId);
            }
            else if (environment.IsDevelopment())
            {
                _rsa.KeySize = 2048;
                logger.LogInformation(
                    "Using an ephemeral development RSA signing key (KeyId: {KeyId}).",
                    _keyId);
            }
            else
            {
                throw new InvalidOperationException(
                    "Tokens:RsaPrivateKeyPem or Tokens:RsaPrivateKeyPath is required outside Development.");
            }

            ValidatePrivateKey(_rsa);
        }
        catch
        {
            _rsa.Dispose();
            throw;
        }

        _signingKey = new RsaSecurityKey(_rsa)
        {
            KeyId = _keyId
        };

        var publicParameters = _rsa.ExportParameters(false);
        _validationKey = new RsaSecurityKey(publicParameters)
        {
            KeyId = _keyId
        };

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_validationKey);
        jwk.KeyId = _keyId;
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;

        _publicJwks = new JsonWebKeySet();
        _publicJwks.Keys.Add(jwk);
    }

    public RsaSecurityKey GetSigningKey() => _signingKey;

    public RsaSecurityKey GetValidationKey() => _validationKey;

    public JsonWebKeySet GetPublicJwks() => _publicJwks;

    private static void ValidatePrivateKey(RSA rsa)
    {
        if (rsa.KeySize < 2048)
        {
            throw new InvalidOperationException("The RSA signing key must be at least 2048 bits.");
        }

        try
        {
            if (rsa.ExportParameters(true).D is null)
            {
                throw new InvalidOperationException("The configured RSA key does not contain private-key material.");
            }
        }
        catch (CryptographicException exception)
        {
            throw new InvalidOperationException(
                "The configured RSA key does not contain exportable private-key material.",
                exception);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _rsa.Dispose();
        _disposed = true;
    }
}
