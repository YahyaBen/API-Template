[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'New-LocalSecrets.ps1')
$taskBrokerRoot = Split-Path -Parent $PSScriptRoot
& docker compose --env-file (Join-Path $taskBrokerRoot '.env') -f (Join-Path $taskBrokerRoot 'docker-compose.yml') up -d --wait --wait-timeout 120
if ($LASTEXITCODE -ne 0) { throw 'RabbitMQ did not start successfully.' }
Write-Host 'RabbitMQ is ready. Management UI: http://localhost:15673; AMQP: localhost:5673 (default ports).'
