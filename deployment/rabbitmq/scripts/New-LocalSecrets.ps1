[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$taskBrokerRoot = Split-Path -Parent $PSScriptRoot
$taskEnvironmentPath = Join-Path $taskBrokerRoot '.env'
if (Test-Path -LiteralPath $taskEnvironmentPath) {
    if (-not (Select-String -LiteralPath $taskEnvironmentPath -Pattern '^RabbitMq__Port=' -Quiet)) {
        [IO.File]::AppendAllText($taskEnvironmentPath, "RabbitMq__Port=5673`nRABBITMQ_MANAGEMENT_PORT=15673`n")
    }
    Write-Host 'RabbitMQ configuration already exists; keeping its credentials.'
    return
}
$taskPassword = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
$taskPassword = $taskPassword.TrimEnd('=').Replace('+', '-').Replace('/', '_')
[IO.File]::WriteAllLines($taskEnvironmentPath, @(
    'RabbitMq__UserName=foodiya'
    "RabbitMq__Password=$taskPassword"
    'RabbitMq__VirtualHost=foodiya'
    'RabbitMq__Port=5673'
    'RABBITMQ_MANAGEMENT_PORT=15673'
), [Text.UTF8Encoding]::new($false))
Write-Host 'Generated RabbitMQ .env. Credential values were not printed.'
