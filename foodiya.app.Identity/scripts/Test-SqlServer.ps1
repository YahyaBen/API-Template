[CmdletBinding()]
param([string]$ConnectionString = 'Server=localhost;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=10')
$ErrorActionPreference = 'Stop'
$taskPreviousConnection = [Environment]::GetEnvironmentVariable('SQLSERVER_TEST_CONNECTION', 'Process')
try {
    [Environment]::SetEnvironmentVariable('SQLSERVER_TEST_CONNECTION', $ConnectionString, 'Process')
    Push-Location -LiteralPath (Split-Path -Parent $PSScriptRoot)
    try {
        dotnet test --project tests/foodiya.app.Identity.Integration.Tests/foodiya.app.Identity.Integration.Tests.csproj --filter 'TestCategory=SqlServer'
        if ($LASTEXITCODE -ne 0) { throw 'SQL Server integration tests failed.' }
    }
    finally { Pop-Location }
}
finally {
    [Environment]::SetEnvironmentVariable('SQLSERVER_TEST_CONNECTION', $taskPreviousConnection, 'Process')
}
