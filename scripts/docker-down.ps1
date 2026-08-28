[CmdletBinding()]
param(
    [switch]$RemoveData
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$environmentFile = Join-Path $repositoryRoot '.env.docker'
$environmentTemplate = Join-Path $repositoryRoot '.env.docker.example'
. (Join-Path $PSScriptRoot 'docker-common.ps1')

$dockerExecutable = Resolve-DockerExecutable
Assert-DockerEngine -DockerExecutable $dockerExecutable

if (-not (Test-Path -LiteralPath $environmentFile)) {
    $environmentFile = $environmentTemplate
}

$arguments = @('compose', '--env-file', $environmentFile, 'down')

if ($RemoveData) {
    $arguments += '--volumes'
    Write-Warning 'Se eliminaran la base SQL Server y los datos persistidos en los volumenes Docker.'
}

Push-Location $repositoryRoot
try {
    & $dockerExecutable @arguments
    if ($LASTEXITCODE -ne 0) {
        throw 'No fue posible detener los servicios Docker.'
    }
} finally {
    Pop-Location
}
