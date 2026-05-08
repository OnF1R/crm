$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$sqlPath = Join-Path $PSScriptRoot 'seed-test-data.sql'
$containerPath = '/tmp/seed-test-data.sql'
$containerName = 'crm-postgres-1'

if (-not (Test-Path $sqlPath)) {
    throw "Seed SQL file not found: $sqlPath"
}

Push-Location $root
try {
    docker-compose up -d postgres | Out-Host

    $postgresRunning = docker inspect -f '{{.State.Running}}' $containerName 2>$null
    if ($postgresRunning -ne 'true') {
        throw "Postgres container '$containerName' is not running"
    }

    docker cp $sqlPath "${containerName}:$containerPath" | Out-Host
    docker exec -i $containerName psql -U crm_admin -d postgres -v ON_ERROR_STOP=1 -f $containerPath | Out-Host
}
finally {
    Pop-Location
}
