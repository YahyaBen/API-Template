[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,127}$')]
    [string]$DatabaseName,
    [string]$Server = '.'
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw 'Install sqlcmd before initializing the Foodiya database.'
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $repositoryRoot "artifacts/database/$DatabaseName"
[IO.Directory]::CreateDirectory($logDirectory) | Out-Null

# This helper uses Windows authentication and refuses an existing database.
sqlcmd -S $Server -E -C -b -Q "IF DB_ID(N'$DatabaseName') IS NOT NULL THROW 50000, 'Database already exists; review migrations before changing an existing database.', 1; CREATE DATABASE [$DatabaseName];"
if ($LASTEXITCODE -ne 0) { throw 'Database creation failed.' }

$scripts = @(
    'FoodRecipeApp_SQL.sql'
    'Migration_SoftDelete.sql'
    'Migration_RBAC_AppUserRole.sql'
    'Migration_RefreshToken.sql'
    'Migration_GoogleAuth_AppUserExternalLogin.sql'
    'Migration_SaaS_Referential_Standardization.sql'
    'Migration_DailyRecipeStats.sql'
    'Migration_Uuid_Code_AllTables.sql'
    'Migration_AddColor_ChefDifficultyRecipeAppUserRecipeNutrition.sql'
    'Migration_ImageUrl_AllTables.sql'
    'Migration_StatsDate_DateToDatetime2.sql'
    'Migration_IdentityInboxMessage.sql'
    'Migration_AppUserEmail256.sql'
    'Migration_IdentityProfileUnicodeNames.sql'
)

foreach ($script in $scripts) {
    $logPath = Join-Path $logDirectory "$script.log"
    sqlcmd -S $Server -E -C -b -d $DatabaseName -i (Join-Path $PSScriptRoot $script) -o $logPath
    if ($LASTEXITCODE -ne 0) {
        Get-Content -LiteralPath $logPath -Tail 8
        throw "Database initialization stopped at $script. See $logPath."
    }
    Write-Host "Applied $script"
}

Write-Host "Initialized $DatabaseName on $Server. SQL logs: $logDirectory"
