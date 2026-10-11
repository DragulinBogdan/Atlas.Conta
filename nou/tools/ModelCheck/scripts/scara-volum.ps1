#requires -Version 7.0
# D9-A1: `--scara-volum privat` pe instanța nativă (5446), cu binarul ModelCheck deja construit, pe o clonă curată a
# bazei private. Baza rezultată e bază de citire.
[CmdletBinding()]
param(
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.Vol',
    [string]$Factori = '1,10,100,F',
    [long]$Postari = 5000000,
    [int]$BugetMinute = 150,
    [string]$Nume = 'scara-volum-' + (Get-Date -Format 'yyyyMMdd-HHmmss'),
    [switch]$PregatesteBaze
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$director = Join-Path $repo "run-verificari/$Nume"
New-Item -ItemType Directory -Force $director | Out-Null
$bin = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0'
$dll = Join-Path $bin 'ModelCheck.dll'
$sistem = Get-CimInstance Win32_ComputerSystem
$mediu = 'instanța nativă 127.0.0.1:5446, gazda: {0:0.0} GiB, {1} procesoare logice' -f ($sistem.TotalPhysicalMemory / 1GB), $sistem.NumberOfLogicalProcessors
[ordered]@{
    commit = (& git -C $repo rev-parse HEAD).Trim(); modificari = @(& git -C $repo status --short)
    dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash; sufix = $Sufix
    factori = $Factori; postari = $Postari; bugetMinute = $BugetMinute; mediu = $mediu
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $director 'rulare.json') -Encoding utf8

if ($PregatesteBaze) {
    python -X utf8 (Join-Path $PSScriptRoot 'pregateste-baze.py') --sufix $Sufix --profil Privat
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
$env:MODELCHECK_BAZA_SUFIX = $Sufix
$env:MODELCHECK_CONEXIUNE_EXTRA = 'Command Timeout=0'
$env:SCARA_VOLUM_DIR = $director; $env:SCARA_VOLUM_F = $Factori; $env:SCARA_VOLUM_POSTARI = $Postari
$env:SCARA_VOLUM_BUGET_MIN = $BugetMinute; $env:SCARA_VOLUM_MEDIU = $mediu
Push-Location $bin
try { dotnet $dll --scara-volum privat *> (Join-Path $director 'scara.log') } finally { Pop-Location }
$iesire = $LASTEXITCODE
Write-Host "scara: exit $iesire, $director"
exit $iesire
