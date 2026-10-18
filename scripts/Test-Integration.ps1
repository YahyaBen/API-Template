[CmdletBinding()]
param(
    [string]$Server='.',
    [string]$SqlServerContainer,
    [switch]$UseSqlAuthentication,
    [string]$ArtifactsRoot,
    [switch]$SkipAuthenticationSmoke
)
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path -Parent $PSScriptRoot
$fixture='ApiTemplate_Test_'+[Guid]::NewGuid().ToString('N').Substring(0,12)
$apiDatabase=$fixture+'_Foodiya'; $identityDatabase=$fixture+'_Identity'
$runDirectory=Join-Path $repositoryRoot "artifacts/integration/$fixture"
[IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$identityUrl='http://localhost:5200'; $apiUrl='http://localhost:5235'
$processes=[Collections.Generic.List[Diagnostics.Process]]::new()
$createdDatabases=[Collections.Generic.List[string]]::new()
$oldIdentityConnection=$env:ConnectionStrings__IdentityDatabase
if($UseSqlAuthentication -and -not $env:SQLCMDPASSWORD){throw 'Set SQLCMDPASSWORD for the dedicated SQL test server.'}
if($SqlServerContainer -and -not $UseSqlAuthentication){throw 'Container SQL setup requires -UseSqlAuthentication.'}
if(-not $SqlServerContainer -and -not (Get-Command sqlcmd -ErrorAction SilentlyContinue)){throw 'Install sqlcmd or pass -SqlServerContainer.'}
function Invoke-TestSql([string]$database,[string]$query,[string]$file){
    $authentication=if($UseSqlAuthentication){@('-U','sa')}else{@('-E')}
    $parameters=@('-S',$(if($SqlServerContainer){'localhost'}else{$Server}),'-C','-b','-f','65001','-d',$database)+$authentication
    if($file){
        if($SqlServerContainer){
            & docker cp $file "${SqlServerContainer}:/tmp/foodiya-integration.sql"
            if($LASTEXITCODE -ne 0){throw 'Copying the integration SQL failed.'}
            $file='/tmp/foodiya-integration.sql'
        }
        $parameters+=@('-i',$file)
    }else{$parameters+=@('-Q',$query)}
    if($SqlServerContainer){ & docker exec -e SQLCMDPASSWORD $SqlServerContainer /opt/mssql-tools18/bin/sqlcmd @parameters | Out-Null }
    else { & sqlcmd @parameters | Out-Null }
    if($LASTEXITCODE -ne 0){throw 'Disposable integration database setup/cleanup failed.'}
}
function Get-TestConnection([string]$database){
    $builder=[Data.Common.DbConnectionStringBuilder]::new()
    $builder['Server']=$Server; $builder['Database']=$database
    $builder['Encrypt']='True'; $builder['TrustServerCertificate']='True'
    if($UseSqlAuthentication){$builder['User ID']='sa';$builder['Password']=$env:SQLCMDPASSWORD}
    else{$builder['Integrated Security']='True'}
    $builder.ConnectionString
}
function Start-TestApi([string]$project,[string]$url,[hashtable]$environment,[string]$label){
    $dll=if($ArtifactsRoot){Join-Path $repositoryRoot "$ArtifactsRoot/bin/$project/release/$project.dll"}else{Join-Path $repositoryRoot "$project/bin/Release/net10.0/$project.dll"}
    if(-not (Test-Path -LiteralPath $dll)){throw "Build $project in Release first."}
    $arguments=@('"'+$dll+'"','--urls',$url,'--contentRoot','"'+(Join-Path $repositoryRoot $project)+'"')
    $parameters=@{FilePath=(Get-Command dotnet).Source;ArgumentList=$arguments;WorkingDirectory=$repositoryRoot;Environment=$environment;RedirectStandardOutput=(Join-Path $runDirectory "$label.stdout.log");RedirectStandardError=(Join-Path $runDirectory "$label.stderr.log");PassThru=$true}
    if([OperatingSystem]::IsWindows()){$parameters.WindowStyle='Hidden'}
    $process=Start-Process @parameters
    $processes.Add($process)
    $healthy=$false
    for($attempt=0;$attempt -lt 40;$attempt++){
        if($process.HasExited){break}
        try{$healthy=(Invoke-WebRequest "$url/health" -TimeoutSec 2).StatusCode -eq 200}catch{}
        if($healthy){break}
        Start-Sleep -Milliseconds 250
    }
    if(-not $healthy){throw "$project did not start. Inspect the ignored integration logs in $runDirectory."}
}
try {
    foreach($url in @($identityUrl,$apiUrl)){
        try{$null=Invoke-WebRequest "$url/health" -TimeoutSec 1;throw "Integration port $(([uri]$url).Port) is occupied."}
        catch {if($_.Exception.Message -like 'Integration port*'){throw}}
    }
    foreach($database in @($apiDatabase,$identityDatabase)){
        Invoke-TestSql 'master' "IF DB_ID(N'$database') IS NOT NULL THROW 50000, 'Test database already exists.', 1; CREATE DATABASE [$database];"
        $createdDatabases.Add($database)
    }
    Invoke-TestSql $apiDatabase -file (Join-Path $repositoryRoot 'foodrecipe.SqlServer/Initialize.sql')
    $env:ConnectionStrings__IdentityDatabase=Get-TestConnection $identityDatabase
    Push-Location $repositoryRoot
    try {
        & dotnet tool restore | Out-Null
        if($LASTEXITCODE -ne 0){throw 'Restoring EF tools failed.'}
        & dotnet tool run dotnet-ef database update --project foodiya.app.Identity.Api --startup-project foodiya.app.Identity.Api --configuration Release --no-build | Out-Null
        if($LASTEXITCODE -ne 0){throw 'Identity test migrations failed.'}
    } finally {Pop-Location}
    $keyPath=Join-Path $runDirectory 'test-signing-key.pem'
    $rsa=[Security.Cryptography.RSA]::Create(2048)
    try{[IO.File]::WriteAllText($keyPath,$rsa.ExportPkcs8PrivateKeyPem())}finally{$rsa.Dispose()}
    $identityEnvironment=@{ASPNETCORE_ENVIRONMENT='Development';DOTNET_ENVIRONMENT='Development';ConnectionStrings__IdentityDatabase=(Get-TestConnection $identityDatabase);Tokens__Issuer=$identityUrl;Tokens__KeyId='integration-test';Tokens__RsaPrivateKeyPath=$keyPath;Outbox__Enabled='false';Cleanup__Enabled='false'}
    $apiEnvironment=@{ASPNETCORE_ENVIRONMENT='Development';DOTNET_ENVIRONMENT='Development';ConnectionStrings__DefaultConnection=(Get-TestConnection $apiDatabase);Authentication__Jwt__Authority=$identityUrl;Authentication__Jwt__Issuer=$identityUrl;Authentication__Jwt__Audience='foodiya.app.Identity.Clients';IdentityEvents__Enabled='false'}
    Start-TestApi 'foodiya.app.Identity.Api' $identityUrl $identityEnvironment 'identity'
    Start-TestApi 'Foodiya.API' $apiUrl $apiEnvironment 'api'
    & (Join-Path $PSScriptRoot 'Test-ResourceAuthorization.ps1') -ApiUrl $apiUrl -Issuer $identityUrl -SigningKeyPath $keyPath
    if(-not $SkipAuthenticationSmoke){
        & (Join-Path $PSScriptRoot 'Test-LocalApis.ps1') -IdentityUrl $identityUrl -ApiUrl $apiUrl -IdentityLogPath (Join-Path $runDirectory 'identity.stdout.log')
    }
} finally {
    foreach($process in $processes){
        if(-not $process.HasExited){Stop-Process -InputObject $process -ErrorAction SilentlyContinue; $null=$process.WaitForExit(5000)}
        $process.Dispose()
    }
    $env:ConnectionStrings__IdentityDatabase=$oldIdentityConnection
    foreach($database in $createdDatabases){
        if($database -notmatch '^ApiTemplate_Test_[a-f0-9]{12}_(Foodiya|Identity)$'){throw 'Refusing cleanup of a non-test database.'}
        try{Invoke-TestSql 'master' "DROP DATABASE [$database];"}catch{Write-Warning "Test database $database could not be removed; it was not forcibly disconnected."}
    }
}
