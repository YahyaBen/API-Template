[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ApiUrl,
    [Parameter(Mandatory)][string]$Issuer,
    [Parameter(Mandatory)][string]$SigningKeyPath,
    [string]$LimitedApiUrl
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
    if($expected -eq 429 -and -not $response.Headers['Retry-After']){throw 'Rate limit rejection omitted Retry-After.'}
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
    $stranger=New-Actor 'USER'
    $null=Request 'Stranger JWT accepted' GET '/api/Referential' $stranger
    $draftBody=@{Title='Visibility '+[Guid]::NewGuid().ToString('N');DifficultyUuid=$refs.difficulties[0].uuid;Servings=2;Status='DRAFT'}
    $draft=Request 'Owner can create draft' POST "/api/Recipe/$($chef.uuid)" $owner 201 $draftBody
    $draftPath="/api/Recipe/$($draft.uuid)"
    $null=Request 'Owner can read draft' GET $draftPath $owner
    $null=Request 'Admin can read draft' GET $draftPath $admin
    $null=Request 'Other user cannot read draft' GET $draftPath $other 404
    $draftStep=Request 'Owner can add draft step' POST '/api/RecipeStep' $owner 201 @{RecipeUuid=$draft.uuid;StepNumber=1;Instruction='Private instructions'}
    $draftImage=Request 'Owner can add draft image' POST '/api/RecipeImage' $owner 201 @{RecipeUuid=$draft.uuid;ImageUrl='https://example.test/private.jpg'}
    $null=Request 'Invalid decimal value returns validation error' POST '/api/RecipeNutrition' $owner 400 @{RecipeUuid=$draft.uuid;CaloriesPerServing=-1}
    $null=Request 'Owner can add draft nutrition' POST '/api/RecipeNutrition' $owner 201 @{RecipeUuid=$draft.uuid;CaloriesPerServing=100.25}
    $ingredients=Request 'Load public ingredients' GET '/api/Ingredient' $owner
    $draftIngredient=Request 'Owner can add draft ingredient' POST '/api/RecipeIngredient' $owner 201 @{RecipeUuid=$draft.uuid;IngredientUuid=$ingredients.data[0].uuid;Quantity=1.5}
    $childPaths=@("/api/RecipeStep/$($draftStep.uuid)","/api/RecipeImage/$($draftImage.uuid)","/api/RecipeNutrition/$($draft.uuid)","/api/RecipeIngredient/$($draftIngredient.uuid)")
    foreach($childPath in $childPaths){$null=Request "Draft child inaccessible: $childPath" GET $childPath $other 404}
    foreach($collection in @('RecipeStep','RecipeImage','RecipeNutrition','RecipeIngredient','RecipeLike')){
        $hidden=Request "Draft collection scoped: $collection" GET "/api/$collection`?recipeUuid=$($draft.uuid)" $other
        if(@($hidden.data).Count -ne 0 -or $hidden.meta.itemCount -ne 0){throw "$collection leaked draft data or counts."}
    }
    $null=Request 'Publish private recipe' PATCH $draftPath $owner 200 @{Status='PUBLISHED';Visibility='PRIVATE'}
    $null=Request 'Other user cannot read private recipe' GET $draftPath $other 404
    $null=Request 'Stranger cannot read private recipe' GET $draftPath $stranger 404
    $search=[uri]::EscapeDataString($draftBody.Title)
    $hidden=Request 'Private recipe excluded from browse' GET "/api/Recipe?search=$search" $stranger
    if(@($hidden.data).Count -ne 0 -or $hidden.meta.itemCount -ne 0){throw 'Private recipe leaked in browse results or counts.'}
    $grantBody=@{RecipeUuid=$draft.uuid;SharedByUserUuid=$owner.Uuid;SharedWithUserUuid=$other.Uuid;ShareChannel='APPLICATION';ShareMessage='Private recipient message'}
    $grant=Request 'Owner can grant private recipe access' POST '/api/RecipeShare' $owner 201 $grantBody
    $null=Request 'Recipient can read private recipe' GET $draftPath $other
    $null=Request 'Recipient can read own received share' GET "/api/RecipeShare/$($grant.uuid)" $other
    $null=Request 'Stranger cannot read share metadata' GET "/api/RecipeShare/$($grant.uuid)" $stranger 404
    foreach($childPath in $childPaths){$null=Request "Recipient can read child: $childPath" GET $childPath $other}
    $null=Request 'Recipient cannot reshare private recipe' POST '/api/RecipeShare' $other 403 @{RecipeUuid=$draft.uuid;SharedByUserUuid=$other.Uuid;SharedWithUserUuid=$stranger.Uuid;ShareChannel='APPLICATION'}
    $hiddenShares=Request 'Stranger share list is scoped' GET "/api/RecipeShare?recipeUuid=$($draft.uuid)" $stranger
    if(@($hiddenShares.data).Count -ne 0 -or $hiddenShares.meta.itemCount -ne 0){throw 'Share listing leaked private messages, participants or counts.'}
    $adminGrant=Request 'Admin can grant private recipe access' POST '/api/RecipeShare' $admin 201 @{RecipeUuid=$draft.uuid;SharedByUserUuid=$admin.Uuid;SharedWithUserUuid=$stranger.Uuid;ShareChannel='APPLICATION'}
    $null=Request 'Admin grant permits recipient read' GET $draftPath $stranger
    $null=Request 'Admin can revoke own grant' DELETE "/api/RecipeShare/$($adminGrant.uuid)" $admin 204
    $null=Request 'Revoked admin grant removes read access' GET $draftPath $stranger 404
    $null=Request 'Owner can revoke recipient access' DELETE "/api/RecipeShare/$($grant.uuid)" $owner 204
    $null=Request 'Revoked recipient cannot read recipe' GET $draftPath $other 404
    $null=Request 'Revoked recipient cannot read child' GET $childPaths[0] $other 404
    $null=Request 'Owner can make recipe unlisted' PATCH $draftPath $owner 200 @{Visibility='UNLISTED'}
    $null=Request 'Unlisted recipe accessible by known UUID' GET $draftPath $stranger
    $hidden=Request 'Unlisted recipe excluded from browse' GET "/api/Recipe?search=$search" $stranger
    if(@($hidden.data).Count -ne 0 -or $hidden.meta.itemCount -ne 0){throw 'Unlisted recipe leaked in browse results or counts.'}
    $null=Request 'Owner can deactivate recipe' PATCH "$draftPath/toggle-active" $owner
    $null=Request 'Inactive recipe hidden from others' GET $draftPath $stranger 404
    $null=Request 'Owner can still read inactive recipe' GET $draftPath $owner

    foreach($query in @('page=0','page=-1','page=2147483647','pageSize=0','pageSize=-1','pageSize=101')){
        $null=Request "Invalid pagination rejected: $query" GET "/api/Recipe?$query" $owner 400
    }
    $null=Request 'Maximum page size accepted' GET '/api/Recipe?pageSize=100' $owner
    $tooManySteps=@(1..101 | ForEach-Object {@{StepNumber=1;Instruction='Bounded collection'}})
    $null=Request 'Oversized nested collection rejected' POST "/api/Recipe/$($chef.uuid)" $owner 400 @{Title='Bounded steps';DifficultyUuid=$refs.difficulties[0].uuid;Servings=2;Steps=$tooManySteps}
    $null=Request 'Oversized request body rejected' POST "/api/Recipe/$($chef.uuid)" $owner 413 @{Title=('x'*1048600);DifficultyUuid=$refs.difficulties[0].uuid;Servings=2}
    if($LimitedApiUrl){
        if(-not ([uri]$LimitedApiUrl).IsLoopback){throw 'The rate limit test requires a loopback URL.'}
        $originalUrl=$ApiUrl
        try {
            $ApiUrl=$LimitedApiUrl
            $limited=New-Actor 'USER'; $independent=New-Actor 'USER'
            $null=Request 'First request within quota accepted' GET '/api/Referential' $limited
            $null=Request 'Second request within quota accepted' GET '/api/Referential' $limited
            $null=Request 'Excess requests limited with Retry-After' GET '/api/Referential' $limited 429
            $null=Request 'Another account has an independent quota' GET '/api/Referential' $independent
        } finally {$ApiUrl=$originalUrl}
    }

    $null=Request 'Cannot forge deletion audit actor' DELETE $path $owner 403 @{DeletedByUserUuid=$other.Uuid;DeleteReason='Integration cleanup'}
    $null=Request 'Other chef cannot delete recipe' DELETE $path $other 403 @{DeletedByUserUuid=$other.Uuid;DeleteReason='Integration cleanup'}
    $null=Request 'Owner can delete recipe' DELETE $path $owner 204 @{DeletedByUserUuid=$owner.Uuid;DeleteReason='Integration cleanup'}
    $checks | Format-Table -AutoSize
    Write-Host "Passed $($checks.Count) resource authorization checks. No credentials or tokens were printed."
} finally {$rsa.Dispose()}
