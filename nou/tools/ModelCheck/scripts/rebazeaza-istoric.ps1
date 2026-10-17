[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Baza,
    [int]$Port = 5446,
    [string]$Referinta = 'Atlas.Conta.Schema.Nou',
    [int]$PortReferinta = 5446
)
# 120 (b, c): o bază construită din lanțul vechi trece pe migrația de bază fără recreare numai dacă schema ei
# e identică cu a referinței (construită din migrația nouă); atunci i se rescrie doar istoricul migrațiilor.
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$env:PGPASSWORD = 'postgres'
$psql = 'D:\PostgreSQL\18\pgsql\bin\psql.exe'
& (Join-Path $PSScriptRoot 'compara-scheme.ps1') -BazaA $Baza -Port $Port -BazaB $Referinta -PortB $PortReferinta `
    -Iesire (Join-Path $PSScriptRoot "..\..\..\..\run-verificari\compara-scheme\$Baza-$Port") | Select-Object -Last 1
if ($LASTEXITCODE -ne 0) { throw "Schema lui $Baza nu corespunde migrației de bază; baza se recreează (102b)." }
$istoric = & $psql -h 127.0.0.1 -p $PortReferinta -U postgres -d $Referinta -AtX -F ',' -c 'select "MigrationId", "ProductVersion" from "__EFMigrationsHistory"'
if (@($istoric).Count -ne 1) { throw "Referința trebuie să aibă exact o migrație." }
$id, $versiune = ($istoric -split ',')
& $psql -h 127.0.0.1 -p $Port -U postgres -d $Baza -AtX -v ON_ERROR_STOP=1 -c @"
begin; delete from "__EFMigrationsHistory"; insert into "__EFMigrationsHistory" values ('$id', '$versiune'); commit;
"@
if ($LASTEXITCODE -ne 0) { throw "Rescrierea istoricului pe $Baza a eșuat." }
Write-Output "$Baza (port $Port): istoric rescris la $id."
