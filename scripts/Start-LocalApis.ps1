[CmdletBinding()]
param(
    [string]$IdentityConnectionString = $env:ConnectionStrings__IdentityDatabase,
    [string]$ApiConnectionString = $env:ConnectionStrings__DefaultConnection,
    [string]$IdentityUrl = 'http://localhost:5000',
    [string]$ApiUrl = 'http://localhost:5135',
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
if (-not [OperatingSystem]::IsWindows() -or
    -not (Get-Command Start-Process).Parameters.ContainsKey('Environment')) {
    throw 'This background launcher requires Windows and PowerShell 7.4 or later. Use dotnet run in separate terminals on other platforms.'
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$runDirectory = Join-Path $repositoryRoot 'artifacts/local-run'
[IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$databaseStatePath = Join-Path $runDirectory 'state.json'
$databaseState = if (Test-Path -LiteralPath $databaseStatePath) {
    Get-Content -LiteralPath $databaseStatePath -Raw | ConvertFrom-Json
}
$localApiSettingsPath = Join-Path $repositoryRoot 'Foodiya.API/appsettings.Local.json'
if (-not $ApiConnectionString -and (Test-Path -LiteralPath $localApiSettingsPath)) {
    $localSettings = Get-Content -LiteralPath $localApiSettingsPath -Raw | ConvertFrom-Json
    $ApiConnectionString = $localSettings.ConnectionStrings.DefaultConnection
}
if (-not $IdentityConnectionString -and $databaseState.IdentityDatabase) {
    $IdentityConnectionString = "Server=$($databaseState.SqlServer);Database=$($databaseState.IdentityDatabase);Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
if (-not $ApiConnectionString -and $databaseState.FoodiyaDatabase) {
    $ApiConnectionString = "Server=$($databaseState.SqlServer);Database=$($databaseState.FoodiyaDatabase);Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
if (-not $IdentityConnectionString) {
    $IdentityConnectionString = 'Server=(localdb)\MSSQLLocalDB;Database=FOODIYA_APP_IDENTITY;Trusted_Connection=True;TrustServerCertificate=True'
}
if (-not $ApiConnectionString) { throw 'Configure the Foodiya connection in appsettings.Local.json or ConnectionStrings__DefaultConnection first.' }

foreach ($url in @($IdentityUrl,$ApiUrl)) {
    $uri = [uri]$url
    if (-not $uri.IsAbsoluteUri -or -not $uri.IsLoopback -or $uri.Scheme -notin @('http','https')) {
        throw 'The Development launcher only supports loopback HTTP/HTTPS URLs.'
    }
    if (Get-NetTCPConnection -LocalPort $uri.Port -State Listen -ErrorAction SilentlyContinue) {
        throw "Port $($uri.Port) is occupied. Existing processes were not changed."
    }
}

if (-not $SkipBuild) {
    dotnet build (Join-Path $repositoryRoot 'API-Template.slnx') --configuration Release --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}
$dotnetPath = (Get-Command dotnet).Source
$identityStdout = Join-Path $runDirectory 'identity.stdout.log'
$identityStderr = Join-Path $runDirectory 'identity.stderr.log'
$apiStdout = Join-Path $runDirectory 'api.stdout.log'
$apiStderr = Join-Path $runDirectory 'api.stderr.log'
$identityEnvironment = @{
    ASPNETCORE_ENVIRONMENT='Development'
    DOTNET_ENVIRONMENT='Development'
    ConnectionStrings__IdentityDatabase=$IdentityConnectionString
    Tokens__Issuer=$IdentityUrl
    Outbox__Enabled='false'
    Cleanup__Enabled='false'
}
$apiEnvironment = @{
    ASPNETCORE_ENVIRONMENT='Development'
    DOTNET_ENVIRONMENT='Development'
    ConnectionStrings__DefaultConnection=$ApiConnectionString
    Authentication__Jwt__Authority=$IdentityUrl
    Authentication__Jwt__Issuer=$IdentityUrl
    Authentication__Jwt__Audience='foodiya.app.Identity.Clients'
    IdentityEvents__Enabled='false'
}

$identityProcess = Start-Process -FilePath $dotnetPath -ArgumentList @('run','--project','foodiya.app.Identity.Api','--configuration','Release','--no-build','--no-launch-profile','--urls',$IdentityUrl) -WorkingDirectory $repositoryRoot -WindowStyle Hidden -Environment $identityEnvironment -RedirectStandardOutput $identityStdout -RedirectStandardError $identityStderr -PassThru
$apiProcess = Start-Process -FilePath $dotnetPath -ArgumentList @('run','--project','Foodiya.API','--configuration','Release','--no-build','--no-launch-profile','--urls',$ApiUrl) -WorkingDirectory $repositoryRoot -WindowStyle Hidden -Environment $apiEnvironment -RedirectStandardOutput $apiStdout -RedirectStandardError $apiStderr -PassThru
[ordered]@{
    StartedAtUtc=[DateTime]::UtcNow.ToString('o')
    IdentityProcessId=$identityProcess.Id
    ApiProcessId=$apiProcess.Id
    IdentityUrl=$IdentityUrl
    FoodiyaUrl=$ApiUrl
    IdentityStdout=$identityStdout
    IdentityStderr=$identityStderr
    ApiStdout=$apiStdout
    ApiStderr=$apiStderr
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runDirectory 'processes.json') -Encoding utf8

foreach ($url in @($IdentityUrl,$ApiUrl)) {
    $healthy = $false
    for ($attempt=0; $attempt -lt 20; $attempt++) {
        try {
            if ((Invoke-WebRequest -Uri "$url/health" -TimeoutSec 2).StatusCode -eq 200) {
                $healthy=$true
                break
            }
        } catch { }
        Start-Sleep -Milliseconds 500
    }
    if (-not $healthy) { throw "API did not start at $url. Inspect the local logs in $runDirectory." }
    Write-Host "Scalar ready: $url/scalar/v1"
}
