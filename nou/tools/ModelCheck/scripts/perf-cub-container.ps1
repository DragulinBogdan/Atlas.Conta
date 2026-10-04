#requires -Version 7.0
# X-D5 / X-D3: `--perf-cub` pe fiecare profil, în rețeaua containerului Postgres (loopback Linux, fără proxy-ul de
# porturi Docker Desktop), cu binarul ModelCheck deja construit; DUK rulează apoi pe Windows peste XML-urile private.
[CmdletBinding()]
param(
    [string]$Container = 'contapal-postgres-1',
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.ClaudeX2',
    [ValidateSet('privat', 'bugetar')][string[]]$Profil = @('privat', 'bugetar'),
    [string]$Istoric = '0,6,12',
    [string]$Trepte = '1,4,16,64',
    [int]$Unitati = 16,
    [string]$Operatii = '',
    [string]$Imagine = 'mcr.microsoft.com/dotnet/aspnet:10.0'
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$nume = 'perf-cub-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$director = Join-Path $repo "run-verificari/$nume"
New-Item -ItemType Directory -Force $director | Out-Null
$dll = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0/ModelCheck.dll'
[ordered]@{
    commit = (& git -C $repo rev-parse HEAD).Trim(); modificari = @(& git -C $repo status --short)
    dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash; container = $Container; imagine = $Imagine; sufix = $Sufix
    profil = $Profil; istoric = $Istoric; trepte = $Trepte; unitati = $Unitati; operatii = $Operatii
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $director 'rulare.json') -Encoding utf8

$cod = 0
foreach ($p in $Profil) {
    $sub = Join-Path $director $p
    New-Item -ItemType Directory -Force $sub | Out-Null
    $argumente = @('ModelCheck.dll', '--perf-cub') + $(if ($p -eq 'privat') { 'privat' } else { @() })
    $id = docker run -d --name "$nume-$p" --network "container:$Container" -v "${repo}:/repo" `
        -w /repo/nou/tools/ModelCheck/bin/Debug/net10.0 `
        -e "MODELCHECK_BAZA_SUFIX=$Sufix" -e 'MODELCHECK_CONEXIUNE_EXTRA=Port=5432;Gss Encryption Mode=Disable' `
        -e "PERF_CUB_DIR=/repo/run-verificari/$nume/$p" -e "PERF_CUB_M=$Istoric" -e "PERF_CUB_K=$Trepte" `
        -e "PERF_CUB_UNITATI=$Unitati" -e "PERF_CUB_OPERATII=$Operatii" $Imagine dotnet @argumente
    $iesire = [int](docker wait $id)
    docker logs $id *> (Join-Path $sub 'perf.log')
    docker rm $id | Out-Null
    Write-Host "perf $p`: exit $iesire, $sub"
    $cod = $cod -bor $iesire
    if ($p -eq 'privat' -and (Get-ChildItem -LiteralPath $sub -Filter 'perf-*-k*-cald.xml')) {
        Push-Location $repo
        try { dotnet $dll --perf-saft-duk $sub privat *> (Join-Path $sub 'duk.log') } finally { Pop-Location }
        Write-Host "duk: exit $LASTEXITCODE"
        $cod = $cod -bor $LASTEXITCODE
    }
}
exit $cod
