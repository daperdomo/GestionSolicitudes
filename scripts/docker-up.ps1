[CmdletBinding()]
param(
    [switch]$Rebuild
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$environmentFile = Join-Path $repositoryRoot '.env.docker'
$environmentTemplate = Join-Path $repositoryRoot '.env.docker.example'
. (Join-Path $PSScriptRoot 'docker-common.ps1')

$dockerExecutable = Resolve-DockerExecutable
Assert-DockerEngine -DockerExecutable $dockerExecutable

if (-not (Test-Path -LiteralPath $environmentFile)) {
    Copy-Item -LiteralPath $environmentTemplate -Destination $environmentFile
    Write-Host 'Se creo .env.docker con credenciales exclusivas para desarrollo local.'
}

$arguments = @('compose', '--env-file', $environmentFile, 'up', '--detach', '--wait', '--wait-timeout', '180')
if ($Rebuild) {
    $arguments += '--build'
}

Push-Location $repositoryRoot
try {
    & $dockerExecutable @arguments
    if ($LASTEXITCODE -ne 0) {
        throw 'No fue posible levantar los servicios Docker.'
    }

    Write-Host ''
    Write-Host 'SB.Solicitudes esta disponible en:'
    Write-Host '  Web:     http://localhost:5173'
    Write-Host '  API:     http://localhost:5080'
    Write-Host '  Swagger: http://localhost:5080/swagger'
} finally {
    Pop-Location
}
