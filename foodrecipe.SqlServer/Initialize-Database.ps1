[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,127}$')]
    [string]$DatabaseName = 'FoodiyaLocal',
    [string]$Server = '.'
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw 'Install sqlcmd before initializing the Foodiya database.'
}

$baselinePath = Join-Path $PSScriptRoot 'Initialize.sql'
if (-not (Test-Path -LiteralPath $baselinePath)) { throw 'Initialize.sql is missing.' }
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $repositoryRoot "artifacts/database/$DatabaseName"
[IO.Directory]::CreateDirectory($logDirectory) | Out-Null
$logPath = Join-Path $logDirectory 'Initialize.sql.log'

# Development setup uses Windows authentication and never replaces a database.
sqlcmd -S $Server -E -C -b -Q "IF DB_ID(N'$DatabaseName') IS NOT NULL THROW 50000, 'Database already exists. Keep it or choose a new DatabaseName.', 1; CREATE DATABASE [$DatabaseName];"
if ($LASTEXITCODE -ne 0) { throw 'Database creation failed.' }

sqlcmd -S $Server -E -C -b -f 65001 -d $DatabaseName -i $baselinePath -o $logPath
if ($LASTEXITCODE -ne 0) { throw "Database initialization failed. See $logPath." }

Write-Host "Initialized $DatabaseName on $Server from Initialize.sql."
Write-Host "SQL log: $logPath"
