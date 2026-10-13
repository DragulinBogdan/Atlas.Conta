#requires -Version 7.0
param([Parameter(Mandatory)][string]$Director,
    [Parameter(Mandatory)][ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$baza = "Atlas.Conta.Http.Privat$Sufix"
$conexiune = "Host=127.0.0.1;Port=5446;Username=postgres;Password=postgres;Database=$baza"
$module = Join-Path $repo 'nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module'
$web = Join-Path $repo 'nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi'
$blazor = Join-Path $repo 'nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server'
if (Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue) { throw 'Portul 5093 este ocupat.' }
function Ruleaza([string]$Nume, [string]$Program, [string[]]$Argumente) {
    & $Program @Argumente 2>&1 | Tee-Object -FilePath (Join-Path $Director "$Nume.log") | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "$Nume a eșuat: $LASTEXITCODE" }
}
Ruleaza 'build-web' 'dotnet' @('build', $web, '--nologo')
Push-Location $module
try { Ruleaza 'migrare-http' 'dotnet' @('ef','database','update','--context','BackOfficeEFCoreDbContext','--connection',$conexiune) }
finally { Pop-Location }
$env:ConnectionStrings__ConnectionString = "EFCoreProvider=Postgres;$conexiune;Persist Security Info=true"
$env:ProfilContabil = 'Privat'
$env:PGPORT = '5446'
Push-Location $blazor
try { Ruleaza 'seed-http' 'dotnet' @('run','--no-launch-profile','--','--updateDatabase','--forceUpdate','--silent') }
finally { Pop-Location }
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5093'
$env:Logging__EventLog__LogLevel__Default = 'None'
[ordered]@{ baza = $baza; portPostgres = 5446; url = $env:ASPNETCORE_URLS
    moduleSha256 = (Get-FileHash (Join-Path $web 'bin/Debug/net10.0/Atlas.Conta.BackOffice.Module.dll')).Hash
    webApiSha256 = (Get-FileHash (Join-Path $web 'bin/Debug/net10.0/Atlas.Conta.BackOffice.WebApi.dll')).Hash
} | ConvertTo-Json | Set-Content (Join-Path $Director 'http-mediu.json') -Encoding utf8
$gazda = Start-Process dotnet -ArgumentList 'bin/Debug/net10.0/Atlas.Conta.BackOffice.WebApi.dll' -WorkingDirectory $web `
    -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Director 'host.log') -RedirectStandardError (Join-Path $Director 'host.err')
try {
    $pornit = $false
    foreach ($i in 1..60) {
        if ($gazda.HasExited) { throw "Hostul a ieșit: $($gazda.ExitCode)" }
        if (Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue) { $pornit = $true; break }
        Start-Sleep -Seconds 1
    }
    if (!$pornit) { throw 'Hostul nu a pornit.' }
    Ruleaza 'refuzuri' 'pwsh' @('-NoProfile','-File',(Join-Path $repo 'nou/tools/ProbeHttp/refuzuri.ps1'),'-Host','http://127.0.0.1:5093','-Baza',$baza)
    Ruleaza 'origini-http' 'python' @('-X','utf8',(Join-Path $repo 'nou/tools/ProbeHttp/origini-partide.py'),
        '--host','http://127.0.0.1:5093','--baza',$baza)
}
finally { if (!$gazda.HasExited) { Stop-Process -Id $gazda.Id -Force } }
