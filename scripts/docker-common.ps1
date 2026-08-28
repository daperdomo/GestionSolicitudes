function Resolve-DockerExecutable {
    $dockerCommand = Get-Command docker -ErrorAction SilentlyContinue
    if ($null -ne $dockerCommand) {
        return $dockerCommand.Source
    }

    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'Programs\DockerDesktop\resources\bin\docker.exe'),
        (Join-Path $env:ProgramFiles 'Docker\Docker\resources\bin\docker.exe')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw 'Docker CLI no esta instalado. Instale Docker Desktop y abra una terminal nueva.'
}

function Assert-DockerEngine {
    param(
        [Parameter(Mandatory)]
        [string]$DockerExecutable
    )

    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $DockerExecutable info *> $null
        $dockerExitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($dockerExitCode -ne 0) {
        throw 'Docker CLI esta instalado, pero el motor de Docker Desktop no esta disponible. Inicie Docker Desktop. Si muestra HCS_E_HYPERV_NOT_INSTALLED, habilite Virtual Machine Platform con "wsl --install --no-distribution" desde PowerShell como Administrador y reinicie Windows.'
    }
}
