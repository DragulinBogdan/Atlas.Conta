#requires -Version 7.0
# X-D5 / X-D3: `--perf-cub` pe fiecare profil, pe instanța nativă (5446), cu binarul ModelCheck deja construit;
# DUK rulează apoi peste XML-urile private.
[CmdletBinding()]
param(
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.Perf',
    [ValidateSet('privat', 'bugetar')][string[]]$Profil = @('privat', 'bugetar'),
    [string]$Istoric = '0,6,12',
    [string]$Trepte = '1,4,16,64',
    [int]$Unitati = 16,
    [string]$Operatii = '',
    [string]$Nume = 'perf-cub-' + (Get-Date -Format 'yyyyMMdd-HHmmss'),
    [switch]$PregatesteBaze
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$director = Join-Path $repo "run-verificari/$Nume"
New-Item -ItemType Directory -Force $director | Out-Null
$bin = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0'
$dll = Join-Path $bin 'ModelCheck.dll'
[ordered]@{
    commit = (& git -C $repo rev-parse HEAD).Trim(); modificari = @(& git -C $repo status --short)
    dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash; instanta = '127.0.0.1:5446'; sufix = $Sufix
    profil = $Profil; istoric = $Istoric; trepte = $Trepte; unitati = $Unitati; operatii = $Operatii
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $director 'rulare.json') -Encoding utf8

if ($PregatesteBaze) {
    python -X utf8 (Join-Path $PSScriptRoot 'pregateste-baze.py') --sufix $Sufix
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
$env:MODELCHECK_BAZA_SUFIX = $Sufix
$env:PERF_CUB_M = $Istoric; $env:PERF_CUB_K = $Trepte; $env:PERF_CUB_UNITATI = $Unitati; $env:PERF_CUB_OPERATII = $Operatii
$cod = 0
foreach ($p in $Profil) {
    $sub = Join-Path $director $p
    New-Item -ItemType Directory -Force $sub | Out-Null
    $env:PERF_CUB_DIR = $sub
    $argumente = @($dll, '--perf-cub') + $(if ($p -eq 'privat') { 'privat' } else { @() })
    Push-Location $bin
    try { dotnet @argumente *> (Join-Path $sub 'perf.log') } finally { Pop-Location }
    $iesire = $LASTEXITCODE
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
