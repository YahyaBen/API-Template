[CmdletBinding()]
param(
    [string]$IdentityUrl = 'http://localhost:5000',
    [string]$ApiUrl = 'http://localhost:5135'
)

$ErrorActionPreference = 'Stop'
foreach ($url in @($IdentityUrl, $ApiUrl)) {
    $uri = [uri]$url
    if (-not $uri.IsAbsoluteUri -or -not $uri.IsLoopback) {
        throw 'This smoke test only supports APIs running on localhost.'
    }
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$runDirectory = Join-Path $repositoryRoot 'artifacts/local-run'
$processStatePath = Join-Path $runDirectory 'processes.json'
if (-not (Test-Path -LiteralPath $processStatePath)) {
    throw 'Start the local APIs with scripts/Start-LocalApis.ps1 first.'
}
$processState = Get-Content -LiteralPath $processStatePath -Raw | ConvertFrom-Json
$script:checks = [Collections.Generic.List[object]]::new()

function Invoke-CheckedRequest {
    param(
        [string]$Name,
        [string]$Uri,
        [int]$ExpectedStatus = 200,
        [string]$Method = 'GET',
        [object]$Body,
        [string]$AccessToken
    )

    $parameters = @{
        Uri = $Uri
        Method = $Method
        TimeoutSec = 30
        SkipHttpErrorCheck = $true
    }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = ConvertTo-Json -InputObject $Body -Depth 8 -Compress
    }
    if ($AccessToken) { $parameters.Headers = @{ Authorization = "Bearer $AccessToken" } }
    $response = Invoke-WebRequest @parameters
    if ([int]$response.StatusCode -ne $ExpectedStatus) {
        throw "$Name returned HTTP $($response.StatusCode); expected $ExpectedStatus. Inspect local logs without sharing credentials."
    }
    $script:checks.Add([pscustomobject]@{ Check=$Name; Status=[int]$response.StatusCode })
    $content = if ($response.Content -is [byte[]]) {
        [Text.Encoding]::UTF8.GetString($response.Content)
    } else { [string]$response.Content }
    if ($response.Headers['Content-Type'] -match 'json' -and $content) {
        return ConvertFrom-Json -InputObject $content
    }
    return $content
}

$null = Invoke-CheckedRequest -Name 'Identity health' -Uri "$IdentityUrl/health"
$null = Invoke-CheckedRequest -Name 'Foodiya health' -Uri "$ApiUrl/health"
$null = Invoke-CheckedRequest -Name 'Identity Scalar page' -Uri "$IdentityUrl/scalar/v1"
$null = Invoke-CheckedRequest -Name 'Foodiya Scalar page' -Uri "$ApiUrl/scalar/v1"
$identityDocument = Invoke-CheckedRequest -Name 'Identity OpenAPI' -Uri "$IdentityUrl/openapi/v1.json"
$apiDocument = Invoke-CheckedRequest -Name 'Foodiya OpenAPI' -Uri "$ApiUrl/swagger/v1/swagger.json"
$discovery = Invoke-CheckedRequest -Name 'OIDC discovery' -Uri "$IdentityUrl/.well-known/openid-configuration"
$jwks = Invoke-CheckedRequest -Name 'Public JWKS' -Uri "$IdentityUrl/.well-known/jwks.json"
if ($discovery.issuer -ne $IdentityUrl -or @($jwks.keys).Count -eq 0) {
    throw 'Identity discovery/JWKS did not match the running service.'
}
if (@($jwks.keys | Where-Object { $_.d -or $_.p -or $_.q }).Count -gt 0) {
    throw 'JWKS exposed private key fields.'
}
$null = Invoke-CheckedRequest -Name 'Anonymous Foodiya access rejected' -Uri "$ApiUrl/api/Referential" -ExpectedStatus 401
$null = Invoke-CheckedRequest -Name 'Malformed JWT rejected' -Uri "$ApiUrl/api/Referential" -AccessToken 'invalid.test.token' -ExpectedStatus 401

$suffix = [Guid]::NewGuid().ToString('N').Substring(0,12)
$email = "scalar-$suffix@example.com"
$password = 'Aa1!' + [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
$registration = Invoke-CheckedRequest -Name 'Register test account' -Uri "$IdentityUrl/api/auth/register" -Method POST -ExpectedStatus 202 -Body @{
    UserName = "scalar_$suffix"
    Email = $email
    Password = $password
    FirstName = 'Scalar'
    LastName = 'SmokeTest'
    PreferredLanguage = 'en'
}

# The Development email adapter writes the synthetic account's OTP locally.
# Never print it, the password, or the returned token pair.
$otp = $null
$otpPattern = 'Development registration OTP for ' + [regex]::Escape($email) + ': (?<code>\d{6})\.'
for ($attempt=0; $attempt -lt 20 -and -not $otp; $attempt++) {
    $logStream = [IO.FileStream]::new($processState.IdentityStdout,
        [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
    $logReader = [IO.StreamReader]::new($logStream)
    try { $localLog = $logReader.ReadToEnd() }
    finally { $logReader.Dispose() }
    $otpMatch = [regex]::Match($localLog, $otpPattern)
    if ($otpMatch.Success) { $otp=$otpMatch.Groups['code'].Value; break }
    Start-Sleep -Milliseconds 250
}
if (-not $otp) { throw 'Development OTP was not found in the local Identity log.' }
$null = Invoke-CheckedRequest -Name 'Confirm registration OTP' -Uri "$IdentityUrl/api/auth/register/confirm" -Method POST -Body @{
    ChallengeId=$registration.challengeId
    Code=$otp
}
$login = Invoke-CheckedRequest -Name 'Login and issue JWT' -Uri "$IdentityUrl/api/auth/login" -Method POST -Body @{
    Login=$email
    Password=$password
    DeviceInstallationId="scalar-smoke-$suffix"
    Platform='web'
    DeviceName='Local Scalar smoke test'
}
$me = Invoke-CheckedRequest -Name 'Identity current user' -Uri "$IdentityUrl/api/users/me" -AccessToken $login.accessToken
if ($me.email -ne $email) { throw 'Identity returned a different account.' }

$references = Invoke-CheckedRequest -Name 'Foodiya accepts Identity JWT' -Uri "$ApiUrl/api/Referential" -AccessToken $login.accessToken
if (@($references.cuisines).Count -eq 0 -or @($references.difficulties).Count -eq 0) {
    throw 'Foodiya reference data was not initialized.'
}
foreach ($path in $apiDocument.paths.PSObject.Properties) {
    if ($path.Name -notmatch '^/api/[^/]+$' -or $path.Name -eq '/api/Referential' -or -not $path.Value.get) { continue }
    if (@($path.Value.get.parameters | Where-Object { $_.required }).Count -gt 0) { continue }
    $null = Invoke-CheckedRequest -Name "Foodiya collection $($path.Name)" -Uri ($ApiUrl + $path.Name) -AccessToken $login.accessToken
}
$null = Invoke-CheckedRequest -Name 'User role cannot create cuisine' -Uri "$ApiUrl/api/Cuisine" -Method POST -Body @{} -AccessToken $login.accessToken -ExpectedStatus 403
$tokenParts = $login.accessToken.Split('.')
$signature = $tokenParts[2]
$index = [int][Math]::Floor($signature.Length / 2)
$replacement = if ($signature[$index] -eq 'A') { 'B' } else { 'A' }
$tokenParts[2] = $signature.Substring(0,$index) + $replacement + $signature.Substring($index+1)
$null = Invoke-CheckedRequest -Name 'Tampered JWT signature rejected' -Uri "$ApiUrl/api/Referential" -AccessToken ($tokenParts -join '.') -ExpectedStatus 401

$refreshed = Invoke-CheckedRequest -Name 'Refresh token rotation' -Uri "$IdentityUrl/api/auth/token/refresh" -Method POST -Body @{ RefreshToken=$login.refreshToken }
if ($refreshed.refreshToken -eq $login.refreshToken) { throw 'Refresh token did not rotate.' }
$null = Invoke-CheckedRequest -Name 'Foodiya accepts refreshed JWT' -Uri "$ApiUrl/api/Referential" -AccessToken $refreshed.accessToken
$null = Invoke-CheckedRequest -Name 'Logout session' -Uri "$IdentityUrl/api/auth/logout" -Method POST -AccessToken $refreshed.accessToken -ExpectedStatus 204
$null = Invoke-CheckedRequest -Name 'Identity rejects revoked session' -Uri "$IdentityUrl/api/users/me" -AccessToken $refreshed.accessToken -ExpectedStatus 401
$null = Invoke-CheckedRequest -Name 'Refresh replay rejected after logout' -Uri "$IdentityUrl/api/auth/token/refresh" -Method POST -Body @{ RefreshToken=$login.refreshToken } -ExpectedStatus 401

$report = [ordered]@{
    TestedAtUtc=[DateTime]::UtcNow.ToString('o')
    IdentityUrl=$IdentityUrl
    ApiUrl=$ApiUrl
    IdentityDocumentedPaths=@($identityDocument.paths.PSObject.Properties).Count
    ApiDocumentedPaths=@($apiDocument.paths.PSObject.Properties).Count
    Checks=@($script:checks.ToArray())
    Passed=$script:checks.Count
}
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runDirectory 'smoke-results.json') -Encoding utf8
$script:checks | Format-Table -AutoSize
Write-Host "Passed $($script:checks.Count) live checks. Credentials and tokens were not printed or saved in the report."
