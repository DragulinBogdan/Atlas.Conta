#requires -Version 7.0
# D9-A1: `--scara-volum privat` în rețeaua containerului Postgres (loopback Linux, fără proxy-ul de porturi Docker Desktop),
# cu binarul ModelCheck deja construit, pe o clonă curată a bazei private. Baza rezultată e bază de citire.
[CmdletBinding()]
param(
    [string]$Container = 'contapal-postgres-1',
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.D9Vol',
    [string]$Factori = '1,10,100,F',
    [long]$Postari = 5000000,
    [int]$BugetMinute = 150,
    [string]$Imagine = 'mcr.microsoft.com/dotnet/aspnet:10.0'
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$nume = 'scara-volum-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$director = Join-Path $repo "run-verificari/$nume"
New-Item -ItemType Directory -Force $director | Out-Null
$dll = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0/ModelCheck.dll'
$memorieContainer = [long](docker inspect $Container --format '{{.HostConfig.Memory}}')
$memorieDocker = [long](docker info --format '{{.MemTotal}}')
$procesoare = [int](docker info --format '{{.NCPU}}')
$mediu = "container $Container, limita de memorie " + $(if ($memorieContainer -eq 0) { 'nesetată' } else { '{0:0.0} GiB' -f ($memorieContainer / 1GB) }) `
    + (', Docker: {0:0.0} GiB, {1} procesoare' -f ($memorieDocker / 1GB), $procesoare)
[ordered]@{
    commit = (& git -C $repo rev-parse HEAD).Trim(); modificari = @(& git -C $repo status --short)
    dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash; container = $Container; imagine = $Imagine; sufix = $Sufix
    factori = $Factori; postari = $Postari; bugetMinute = $BugetMinute; mediu = $mediu
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $director 'rulare.json') -Encoding utf8

$id = docker run -d --name $nume --network "container:$Container" -v "${repo}:/repo" `
    -w /repo/nou/tools/ModelCheck/bin/Debug/net10.0 `
    -e "MODELCHECK_BAZA_SUFIX=$Sufix" -e 'MODELCHECK_CONEXIUNE_EXTRA=Port=5432;Gss Encryption Mode=Disable;Command Timeout=0' `
    -e "SCARA_VOLUM_DIR=/repo/run-verificari/$nume" -e "SCARA_VOLUM_F=$Factori" -e "SCARA_VOLUM_POSTARI=$Postari" `
    -e "SCARA_VOLUM_BUGET_MIN=$BugetMinute" -e "SCARA_VOLUM_MEDIU=$mediu" $Imagine dotnet ModelCheck.dll --scara-volum privat
$iesire = [int](docker wait $id)
docker logs $id *> (Join-Path $director 'scara.log')
docker rm $id | Out-Null
Write-Host "scara: exit $iesire, $director"
exit $iesire
