# Generarea `openapi.json` FĂRĂ HOST VIU (F2-D7 — datoria M2 a spike-ului).
#
# `swagger tofile` (Swashbuckle.AspNetCore.Cli, dotnet local tool declarat în
# `nou/dotnet-tools.json`) încarcă assembly-ul WebApi, construiește host-ul EXACT
# ca la pornire (`Startup.ConfigureServices` rulează integral) și cere documentul
# de la `ISwaggerProvider` — fără să pornească pipeline-ul HTTP. Consecință
# verificată: `IHostedService`-urile NU rulează, deci warmup-ul XAF nu pornește
# și baza de date nu e atinsă; comanda merge pe o mașină fără Postgres pornit.
#
# Ieșirea brută a CLI-ului se normalizează prin `dump-openapi.mjs` (aceeași
# serializare ca dump-ul din host viu) — vezi comentariul de acolo.
#
# Uz:  pnpm gen:openapi          (echivalent: pwsh -File scripts/gen-openapi.ps1)
#      pnpm verifica:drift       (regenerare + tipuri + `git diff --exit-code`)
[CmdletBinding()]
param(
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$radacinaClient = Split-Path -Parent $PSScriptRoot
$proiectWebApi = Join-Path $radacinaClient '..\Atlas.Conta.BackOffice\Atlas.Conta.BackOffice.WebApi' | Resolve-Path

Write-Host "Build ${Configuration}: $proiectWebApi"
dotnet build $proiectWebApi.Path -c $Configuration --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "Build-ul WebApi a eșuat." }

# TFM-ul nu se codifică aici: se ia din ce a produs build-ul.
$assembly = Get-ChildItem -Path (Join-Path $proiectWebApi "bin\$Configuration") `
    -Filter 'Atlas.Conta.BackOffice.WebApi.dll' -Recurse |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $assembly) { throw "Nu găsesc Atlas.Conta.BackOffice.WebApi.dll în bin\$Configuration." }

# Tool-ul e local (manifest) — restore-ul e idempotent și tăcut dacă e deja acolo.
Push-Location $proiectWebApi
try {
    dotnet tool restore | Out-Null
    $brut = Join-Path ([System.IO.Path]::GetTempPath()) "openapi-brut-$PID.json"
    dotnet swagger tofile --output $brut $assembly.FullName v1
    if ($LASTEXITCODE -ne 0) { throw "`swagger tofile` a eșuat." }
}
finally {
    Pop-Location
}

Push-Location $radacinaClient
try {
    $env:SWAGGER_FILE = $brut
    node scripts/dump-openapi.mjs
    if ($LASTEXITCODE -ne 0) { throw "Normalizarea openapi.json a eșuat." }
}
finally {
    Remove-Item Env:\SWAGGER_FILE -ErrorAction SilentlyContinue
    Remove-Item $brut -ErrorAction SilentlyContinue
    Pop-Location
}
