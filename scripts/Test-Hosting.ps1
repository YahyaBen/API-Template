# Invoked by Test-Integration.ps1; uses only that run's owned processes/databases.
$hostingChecks=0
function Check-Endpoint([string]$url,[int]$expected){
    $response=Invoke-WebRequest -Uri $url -TimeoutSec 15 -SkipHttpErrorCheck
    if([int]$response.StatusCode -ne $expected){throw "Hosting check returned HTTP $($response.StatusCode), expected ${expected}: $url"}
    $script:hostingChecks++
}
foreach($entry in @(@{Url=$apiUrl;Database=$apiDatabase},@{Url=$identityUrl;Database=$identityDatabase})){
    Check-Endpoint "$($entry.Url)/health/ready" 200
    if($entry.Database -notmatch '^ApiTemplate_Test_[a-f0-9]{12}_(Foodiya|Identity)$'){throw 'Readiness outage tests require an owned disposable database.'}
    Invoke-TestSql 'master' "ALTER DATABASE [$($entry.Database)] SET OFFLINE WITH ROLLBACK IMMEDIATE;"
    try {
        Check-Endpoint "$($entry.Url)/health/ready" 503
        Check-Endpoint "$($entry.Url)/health" 200
    } finally {Invoke-TestSql 'master' "ALTER DATABASE [$($entry.Database)] SET ONLINE;"}
}
foreach($process in $processes){if(-not $process.HasExited){Stop-Process -InputObject $process; $null=$process.WaitForExit(5000)}}
$productionOrigin='https://frontend.example.test'
$productionIdentity=$identityEnvironment.Clone()
$productionIdentity['ASPNETCORE_ENVIRONMENT']='Production';$productionIdentity['DOTNET_ENVIRONMENT']='Production'
$productionIdentity['Tokens__Issuer']='https://identity.example.test'
$productionIdentity['Registration__OtpPepper']=[Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$productionIdentity['Tokens__RefreshTokenPepper']=[Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$productionIdentity['EmailDelivery__Provider']='Smtp';$productionIdentity['EmailDelivery__Host']='smtp.example.test'
$productionIdentity['EmailDelivery__FromAddress']='no-reply@example.test'
$productionIdentity['CorsOrigins__0']=$productionOrigin
$productionApi=$apiEnvironment.Clone()
$productionApi['ASPNETCORE_ENVIRONMENT']='Production';$productionApi['DOTNET_ENVIRONMENT']='Production'
$productionApi['Authentication__Jwt__Authority']='https://identity.example.test'
$productionApi['Authentication__Jwt__Issuer']='https://identity.example.test'
$productionApi['CorsOrigins__0']=$productionOrigin
Start-TestApi 'foodiya.app.Identity.Api' $identityUrl $productionIdentity 'production-identity'
Start-TestApi 'Foodiya.API' $apiUrl $productionApi 'production-api'
foreach($url in @($identityUrl,$apiUrl)){
    Check-Endpoint "$url/health" 200
    Check-Endpoint "$url/health/ready" 200
    Check-Endpoint "$url/scalar/v1" 404
    $allowed=Invoke-WebRequest "$url/health" -Headers @{Origin=$productionOrigin}
    if($allowed.Headers['Access-Control-Allow-Origin'] -ne $productionOrigin){throw 'Allowed production CORS origin was not honored.'}
    $blocked=Invoke-WebRequest "$url/health" -Headers @{Origin='https://blocked.example.test'}
    if($blocked.Headers['Access-Control-Allow-Origin']){throw 'Unconfigured CORS origin was allowed.'}
    $script:hostingChecks+=2
}
Check-Endpoint "$apiUrl/swagger/v1/swagger.json" 404
Check-Endpoint "$apiUrl/swagger/index.html" 404
Check-Endpoint "$identityUrl/openapi/v1.json" 404
function Check-StartupRejected([string]$project,[hashtable]$environment,[string]$label,[string]$expectedMessage){
    $rejected=$false
    try{Start-TestApi $project 'http://localhost:5266' $environment $label}catch{$rejected=$true}
    $process=$processes[$processes.Count-1]
    if(-not $rejected -or -not $process.HasExited){throw "Unsafe configuration was accepted: $label"}
    $output=(Get-Content (Join-Path $runDirectory "$label.stdout.log") -Raw)+(Get-Content (Join-Path $runDirectory "$label.stderr.log") -Raw)
    if($output -notlike "*$expectedMessage*"){throw "Configuration check failed for an unexpected reason: $label"}
    $script:hostingChecks++
}
$bad=$productionApi.Clone();$bad['CorsOrigins__0']='*'
Check-StartupRejected 'Foodiya.API' $bad 'reject-wildcard-cors' 'CorsOrigins'
$bad=$productionApi.Clone();$bad['ReverseProxy__Enabled']='true'
Check-StartupRejected 'Foodiya.API' $bad 'reject-untrusted-forwarding' 'trusted proxy'
$bad=$productionApi.Clone();$bad['Authentication__Jwt__Authority']='http://identity.example.test'
Check-StartupRejected 'Foodiya.API' $bad 'reject-http-authority' 'HTTPS Authority'
$bad=$productionIdentity.Clone();$bad['EmailDelivery__Provider']='Disabled'
Check-StartupRejected 'foodiya.app.Identity.Api' $bad 'reject-disabled-production-email' 'must be Smtp'
Write-Host "Passed $hostingChecks hosting checks: SQL outage/readiness, Production startup, private documentation, CORS, and rejected unsafe configuration. SMTP transport and external HTTPS ingress were not contacted."
