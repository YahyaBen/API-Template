[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ApiUrl,
    [Parameter(Mandatory)][string]$Issuer,
    [Parameter(Mandatory)][string]$SigningKeyPath
)
$ErrorActionPreference='Stop'
foreach($url in @($ApiUrl,$Issuer)){if(-not ([uri]$url).IsLoopback){throw 'Security tests require loopback API URLs.'}}
$checks=[Collections.Generic.List[object]]::new()
$rsa=[Security.Cryptography.RSA]::Create()
$rsa.ImportFromPem((Get-Content -LiteralPath $SigningKeyPath -Raw))
function ConvertTo-Base64Url([byte[]]$bytes){[Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_')}
function New-Actor([string]$role){
    $uuid=[Guid]::NewGuid().ToString()
    $now=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $header=ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes('{"alg":"RS256","typ":"JWT","kid":"integration-test"}'))
    $payload=@{iss=$Issuer;aud='foodiya.app.Identity.Clients';sub=$uuid;email="test-$uuid@example.test";name='Integration Test';role=$role;exp=$now+600;iat=$now;nbf=$now-5;jti=[Guid]::NewGuid().ToString()} | ConvertTo-Json -Compress
    $body=ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($payload))
    $data="$header.$body"
    $signature=$rsa.SignData([Text.Encoding]::UTF8.GetBytes($data),[Security.Cryptography.HashAlgorithmName]::SHA256,[Security.Cryptography.RSASignaturePadding]::Pkcs1)
    @{Uuid=$uuid;Token="$data.$(ConvertTo-Base64Url $signature)"}
}
function Request([string]$name,[string]$method,[string]$path,$actor,[int]$expected=200,$body=$null){
    $parameters=@{Uri=$ApiUrl+$path;Method=$method;Headers=@{Authorization='Bearer '+$actor.Token};SkipHttpErrorCheck=$true;TimeoutSec=30}
    if($null -ne $body){$parameters.ContentType='application/json';$parameters.Body=ConvertTo-Json -InputObject $body -Depth 10 -Compress}
    $response=Invoke-WebRequest @parameters
    if([int]$response.StatusCode -ne $expected){throw "$name returned HTTP $($response.StatusCode), expected $expected. Inspect the ignored integration logs."}
    $checks.Add([pscustomobject]@{Check=$name;Status=[int]$response.StatusCode})
    if($response.Content -and $response.Headers['Content-Type'] -match 'json'){
        $content=if($response.Content -is [byte[]]){[Text.Encoding]::UTF8.GetString($response.Content)}else{[string]$response.Content}
        return ConvertFrom-Json $content
    }
}
try {
    $owner=New-Actor 'CHEF'; $other=New-Actor 'CHEF'; $admin=New-Actor 'ADMIN'
    $refs=Request 'Owner JWT accepted' GET '/api/Referential' $owner
    $null=Request 'Other user JWT accepted' GET '/api/Referential' $other
    $null=Request 'Admin JWT accepted' GET '/api/Referential' $admin
    $chef=Request 'Create own chef profile' POST '/api/ChefProfile/me' $owner 201 @{DisplayName='Owner Integration Chef'}
    $otherChef=Request 'Create second chef profile' POST '/api/ChefProfile/me' $other 201 @{DisplayName='Other Integration Chef'}
    $null=Request 'Other chef cannot edit profile' PATCH "/api/ChefProfile/$($chef.uuid)" $other 403 @{DisplayName='Forbidden change'}
    $null=Request 'Owner can edit profile' PATCH "/api/ChefProfile/$($chef.uuid)" $owner 200 @{DisplayName='Owner Integration Chef Updated'}
    $body=@{Title='Authorization '+[Guid]::NewGuid().ToString('N');DifficultyUuid=$refs.difficulties[0].uuid;Servings=2;Status='PUBLISHED'}
    $null=Request 'Cannot create recipe for another chef' POST "/api/Recipe/$($chef.uuid)" $other 403 $body
    $recipe=Request 'Owner can create recipe' POST "/api/Recipe/$($chef.uuid)" $owner 201 $body
    $path="/api/Recipe/$($recipe.uuid)"
    $null=Request 'Other chef cannot edit recipe' PATCH $path $other 403 @{Summary='Forbidden change'}
    $null=Request 'Other chef cannot toggle recipe' PATCH "$path/toggle-active" $other 403
    $null=Request 'Owner can edit recipe' PATCH $path $owner 200 @{Summary='Authorized change'}
    $null=Request 'Admin can edit recipe' PATCH $path $admin 200 @{Summary='Administrator change'}
    $imageBody=@{RecipeUuid=$recipe.uuid;ImageUrl='https://example.test/image.jpg';SortOrder=1}
    $null=Request 'Other chef cannot add recipe image' POST '/api/RecipeImage' $other 403 $imageBody
    $image=Request 'Owner can add recipe image' POST '/api/RecipeImage' $owner 201 $imageBody
    $null=Request 'Other chef cannot edit recipe image' PATCH "/api/RecipeImage/$($image.uuid)" $other 403 @{AltText='Forbidden change'}
    $null=Request 'Other chef cannot delete recipe image' DELETE "/api/RecipeImage/$($image.uuid)" $other 403
    $null=Request 'Owner can delete recipe image' DELETE "/api/RecipeImage/$($image.uuid)" $owner 204
    $stepBody=@{RecipeUuid=$recipe.uuid;StepNumber=1;Instruction='Integration test step'}
    $null=Request 'Other chef cannot add recipe step' POST '/api/RecipeStep' $other 403 $stepBody
    $step=Request 'Owner can add recipe step' POST '/api/RecipeStep' $owner 201 $stepBody
    $null=Request 'Other chef cannot edit recipe step' PATCH "/api/RecipeStep/$($step.uuid)" $other 403 @{Instruction='Forbidden change'}
    $null=Request 'Other chef cannot delete recipe step' DELETE "/api/RecipeStep/$($step.uuid)" $other 403
    $nutritionBody=@{RecipeUuid=$recipe.uuid;CaloriesPerServing=100}
    $null=Request 'Other chef cannot add recipe nutrition' POST '/api/RecipeNutrition' $other 403 $nutritionBody
    $null=Request 'Owner can add recipe nutrition' POST '/api/RecipeNutrition' $owner 201 $nutritionBody
    $null=Request 'Other chef cannot edit recipe nutrition' PATCH "/api/RecipeNutrition/$($recipe.uuid)" $other 403 @{CaloriesPerServing=200}
    $null=Request 'Other chef cannot delete recipe nutrition' DELETE "/api/RecipeNutrition/$($recipe.uuid)" $other 403
    $likeBody=@{RecipeUuid=$recipe.uuid;UserUuid=$owner.Uuid}
    $null=Request 'Cannot forge another user like' POST '/api/RecipeLike' $other 403 $likeBody
    $likeBody.UserUuid=$other.Uuid
    $null=Request 'User can create own like' POST '/api/RecipeLike' $other 201 $likeBody
    $null=Request 'Cannot delete another user like' DELETE "/api/RecipeLike/$($recipe.uuid)/$($other.Uuid)" $owner 403
    $null=Request 'User can delete own like' DELETE "/api/RecipeLike/$($recipe.uuid)/$($other.Uuid)" $other 204
    $shareBody=@{RecipeUuid=$recipe.uuid;SharedByUserUuid=$owner.Uuid;SharedWithUserUuid=$other.Uuid;ShareChannel='APPLICATION';ShareMessage='Integration private message'}
    $null=Request 'Cannot forge another share sender' POST '/api/RecipeShare' $other 403 $shareBody
    $share=Request 'User can create own share' POST '/api/RecipeShare' $owner 201 $shareBody
    $null=Request 'Recipient cannot edit sender share' PATCH "/api/RecipeShare/$($share.uuid)" $other 403 @{ShareMessage='Forbidden change'}
    $null=Request 'Recipient cannot delete sender share' DELETE "/api/RecipeShare/$($share.uuid)" $other 403
    $null=Request 'Sender can edit own share' PATCH "/api/RecipeShare/$($share.uuid)" $owner 200 @{ShareMessage='Authorized change'}
    $null=Request 'Sender can delete own share' DELETE "/api/RecipeShare/$($share.uuid)" $owner 204
    $null=Request 'Cannot forge deletion audit actor' DELETE $path $owner 403 @{DeletedByUserUuid=$other.Uuid;DeleteReason='Integration cleanup'}
    $null=Request 'Other chef cannot delete recipe' DELETE $path $other 403 @{DeletedByUserUuid=$other.Uuid;DeleteReason='Integration cleanup'}
    $null=Request 'Owner can delete recipe' DELETE $path $owner 204 @{DeletedByUserUuid=$owner.Uuid;DeleteReason='Integration cleanup'}
    $checks | Format-Table -AutoSize
    Write-Host "Passed $($checks.Count) resource authorization checks. No credentials or tokens were printed."
} finally {$rsa.Dispose()}
