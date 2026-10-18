[CmdletBinding()]
param([switch]$RestartBroker)
$ErrorActionPreference = 'Stop'
$taskBrokerRoot = Split-Path -Parent $PSScriptRoot
$taskIdentityRoot = Join-Path (Split-Path -Parent $taskBrokerRoot) 'foodiya.app.Identity'
$taskSavedEnvironment = @{}
try {
    foreach ($taskLine in Get-Content -LiteralPath (Join-Path $taskBrokerRoot '.env')) {
        if ($taskLine -match '^(RabbitMq__[^=]+)=(.*)$') {
            $taskName = $Matches[1]
            $taskValue = $Matches[2]
            $taskSavedEnvironment[$taskName] = [Environment]::GetEnvironmentVariable($taskName, 'Process')
            [Environment]::SetEnvironmentVariable($taskName, $taskValue, 'Process')
        }
    }
    foreach ($taskName in @('RABBITMQ_TESTS', 'RABBITMQ_RESTART_TESTS')) {
        $taskSavedEnvironment[$taskName] = [Environment]::GetEnvironmentVariable($taskName, 'Process')
    }
    $env:RABBITMQ_TESTS = '1'
    $env:RABBITMQ_RESTART_TESTS = if ($RestartBroker) { '1' } else { '0' }
    Push-Location -LiteralPath $taskIdentityRoot
    try {
        & dotnet test --project tests/foodiya.app.Identity.Integration.Tests/foodiya.app.Identity.Integration.Tests.csproj --filter 'TestCategory=RabbitMQ'
        if ($LASTEXITCODE -ne 0) { throw 'RabbitMQ integration tests failed.' }
    }
    finally { Pop-Location }
}
finally {
    foreach ($taskName in $taskSavedEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($taskName, $taskSavedEnvironment[$taskName], 'Process')
    }
}
