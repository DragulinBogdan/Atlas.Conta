#requires -Version 7.0
# SAF-B8 D3: `--perf-saft` în rețeaua containerului Postgres (loopback Linux, fără proxy-ul de porturi
# Docker Desktop), cu binarul ModelCheck deja construit; DUK rulează apoi pe Windows peste XML-urile produse.
[CmdletBinding()]
param(
    [string]$Container = 'contapal-postgres-1',
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.ClaudeS3',
    [string]$Imagine = 'mcr.microsoft.com/dotnet/aspnet:10.0'
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$nume = 'perf-saft-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$director = Join-Path $repo "run-verificari/$nume"
New-Item -ItemType Directory -Force $director | Out-Null
$dll = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0/ModelCheck.dll'
[ordered]@{
    commit = (& git -C $repo rev-parse HEAD).Trim(); modificari = @(& git -C $repo status --short)
    dllSha256 = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash; container = $Container; imagine = $Imagine; sufix = $Sufix
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $director 'rulare.json') -Encoding utf8

$id = docker run -d --name $nume --network "container:$Container" -v "${repo}:/repo" `
    -w /repo/nou/tools/ModelCheck/bin/Debug/net10.0 `
    -e "MODELCHECK_BAZA_SUFIX=$Sufix" -e 'MODELCHECK_CONEXIUNE_EXTRA=Port=5432;Gss Encryption Mode=Disable' `
    -e "PERF_SAFT_DIR=/repo/run-verificari/$nume" $Imagine dotnet ModelCheck.dll --perf-saft privat
$cod = [int](docker wait $id)
docker logs $id *> (Join-Path $director 'perf.log')
docker rm $id | Out-Null
Write-Host "perf: exit $cod, $director"
Push-Location $repo
try { dotnet $dll --perf-saft-duk $director privat *> (Join-Path $director 'duk.log') } finally { Pop-Location }
Write-Host "duk: exit $LASTEXITCODE"
exit ($cod -bor $LASTEXITCODE)
