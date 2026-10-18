[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

function New-UrlSafeSecret([int]$byteCount = 48) {
    $encoded = [Convert]::ToBase64String(
        [Security.Cryptography.RandomNumberGenerator]::GetBytes($byteCount))
    $encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$environmentPath = Join-Path $projectRoot '.env'
$secretsDirectory = Join-Path $projectRoot 'secrets'
$privateKeyPath = Join-Path $secretsDirectory 'jwt-private-key.pem'

if (-not $Force -and ((Test-Path -LiteralPath $environmentPath) -or
        (Test-Path -LiteralPath $privateKeyPath))) {
    throw 'Local secrets already exist. Use -Force only when intentionally rotating them.'
}

New-Item -ItemType Directory -Path $secretsDirectory -Force | Out-Null

$rsa = [Security.Cryptography.RSA]::Create(3072)
try {
    [IO.File]::WriteAllText(
        $privateKeyPath,
        $rsa.ExportPkcs8PrivateKeyPem(),
        [Text.UTF8Encoding]::new($false))
}
finally {
    $rsa.Dispose()
}

$keySuffix = [Convert]::ToHexString(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(6)).ToLowerInvariant()
$environment = @(
    "SQL_SERVER_SA_PASSWORD=A!a1$(New-UrlSafeSecret 36)"
    'IDENTITY_ISSUER=https://identity.foodiya.app'
    "Registration__OtpPepper=$(New-UrlSafeSecret)"
    "Tokens__RefreshTokenPepper=$(New-UrlSafeSecret)"
    "Tokens__KeyId=foodiya-app-identity-$keySuffix"
    "Outbox__SigningSecret=$(New-UrlSafeSecret)"
    'BootstrapAdmin__Enabled=false'
    'BootstrapAdmin__Email=admin@foodiya.app'
    'BootstrapAdmin__UserName=foodiya-admin'
    "BootstrapAdmin__Password=A!a1$(New-UrlSafeSecret 36)"
    'BootstrapAdmin__FirstName=Foodiya'
    'BootstrapAdmin__LastName=Administrator'
)

[IO.File]::WriteAllLines(
    $environmentPath,
    $environment,
    [Text.UTF8Encoding]::new($false))

Write-Host 'Generated .env and secrets/jwt-private-key.pem.'
Write-Host 'The secret values were not printed. Keep both paths out of source control.'
