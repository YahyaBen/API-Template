#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p artifacts/startup
export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development
export ConnectionStrings__IdentityDatabase='Server=127.0.0.1;Database=IdentityStartup;Integrated Security=True;TrustServerCertificate=True'
export ConnectionStrings__DefaultConnection='Server=127.0.0.1;Database=FoodiyaStartup;Integrated Security=True;TrustServerCertificate=True'
export Tokens__Issuer=http://localhost:5000
export Authentication__Jwt__Authority=http://localhost:5000
export Authentication__Jwt__Issuer=http://localhost:5000
export Authentication__Jwt__Audience=foodiya.app.Identity.Clients
export Outbox__Enabled=false
export Cleanup__Enabled=false
export IdentityEvents__Enabled=false

dotnet run --project foodiya.app.Identity.Api --configuration Release --no-build --no-launch-profile --urls http://localhost:5000 >artifacts/startup/identity.log 2>&1 &
identity_pid=$!
api_pid=''
trap 'kill "$identity_pid" ${api_pid:+"$api_pid"} 2>/dev/null || true' EXIT
dotnet run --project Foodiya.API --configuration Release --no-build --no-launch-profile --urls http://localhost:5135 >artifacts/startup/api.log 2>&1 &
api_pid=$!

for port in 5000 5135; do
  curl --fail --silent --retry 20 --retry-connrefused --retry-delay 1 "http://localhost:$port/health" >/dev/null
  curl --fail --silent "http://localhost:$port/scalar/v1" | grep -i scalar >/dev/null
  echo "Health and Scalar passed on port $port"
done
curl --fail --silent http://localhost:5000/openapi/v1.json | python3 -c 'import json,sys; assert len(json.load(sys.stdin)["paths"]) > 0'
curl --fail --silent http://localhost:5135/swagger/v1/swagger.json | python3 -c 'import json,sys; assert len(json.load(sys.stdin)["paths"]) > 0'
curl --fail --silent http://localhost:5000/.well-known/openid-configuration | python3 -c 'import json,sys; assert json.load(sys.stdin)["issuer"] == "http://localhost:5000"'
status=$(curl --silent --output /dev/null --write-out '%{http_code}' http://localhost:5135/api/Referential)
test "$status" = 401
echo 'OpenAPI, discovery and anonymous JWT rejection passed.'
