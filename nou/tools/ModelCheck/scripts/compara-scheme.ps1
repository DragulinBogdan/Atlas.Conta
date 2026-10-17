[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$BazaA,
    [Parameter(Mandatory)][string]$BazaB,
    [int]$Port = 5446,
    [int]$PortB = 0,
    [string]$Iesire = (Join-Path $PSScriptRoot '..\..\..\..\run-verificari\compara-scheme')
)
# 120 (c): schema a două baze (A = lanțul vechi, B = migrația de bază), comparată pe catalog.
# Nu contează datele, ordinea coloanelor, numele constrângerilor NOT NULL de sistem și terminațiile de
# linie din corpul funcțiilor. Valorile implicite lăsate de AddColumn în A și absente din B sunt urme ale
# lanțului și se listează separat; orice altă diferență pică proba.
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$env:PGPASSWORD = 'postgres'
$env:PGCLIENTENCODING = 'UTF8'
$psql = 'D:\PostgreSQL\18\pgsql\bin\psql.exe'
New-Item -ItemType Directory -Force $Iesire | Out-Null

$interogari = [ordered]@{
    coloane = @"
select table_name, column_name, udt_name, is_nullable,
       coalesce(character_maximum_length::text, ''), coalesce(numeric_precision::text, ''), coalesce(numeric_scale::text, ''),
       is_identity, coalesce(generation_expression, '')
from information_schema.columns where table_schema = 'public' order by 1, 2
"@
    constrangeri = @"
select conrelid::regclass::text, case when contype = 'n' then 'not_null' else conname end, contype::text, pg_get_constraintdef(oid, true)
from pg_constraint where connamespace = 'public'::regnamespace order by 1, 4, 2
"@
    indecsi = "select tablename, indexname, indexdef from pg_indexes where schemaname = 'public' order by 1, 2"
    vederi = "select viewname, replace(pg_get_viewdef((quote_ident(viewname))::regclass, true), chr(10), ' ') from pg_views where schemaname = 'public' order by 1"
    functii = @"
select p.proname, pg_get_function_identity_arguments(p.oid), replace(replace(pg_get_functiondef(p.oid), chr(13), ''), chr(10), ' ')
from pg_proc p join pg_namespace n on n.oid = p.pronamespace where n.nspname = 'public' order by 1, 2
"@
    partitii = @"
select c.relname, c.relkind::text, coalesce(pg_get_expr(c.relpartbound, c.oid), ''), coalesce(pg_get_partkeydef(c.oid), '')
from pg_class c where c.relnamespace = 'public'::regnamespace and (c.relispartition or c.relkind = 'p') order by 1
"@
    triggere = @"
select tgrelid::regclass::text, tgname, pg_get_triggerdef(oid, true)
from pg_trigger where not tgisinternal and tgrelid in (select oid from pg_class where relnamespace = 'public'::regnamespace) order by 1, 2
"@
    secvente = "select sequence_name, data_type from information_schema.sequences where sequence_schema = 'public' order by 1"
}

if ($PortB -eq 0) { $PortB = $Port }
function Citeste([string]$baza, [string]$sql) {
    $p = if ($baza -eq $BazaB) { $PortB } else { $Port }
    $r = & $psql -h 127.0.0.1 -p $p -U postgres -d $baza -AtX -F ' | ' -c ($sql -replace "\r?\n", ' ')
    if ($LASTEXITCODE -ne 0) { throw "psql a eșuat pe $baza" }
    @($r)
}

$diferente = 0
foreach ($parte in $interogari.Keys) {
    $a = Citeste $BazaA $interogari[$parte]
    $b = Citeste $BazaB $interogari[$parte]
    $a | Set-Content -Encoding utf8 (Join-Path $Iesire "$parte-A.txt")
    $b | Set-Content -Encoding utf8 (Join-Path $Iesire "$parte-B.txt")
    $a = @($a | Where-Object { $_ -ne $null }); $b = @($b | Where-Object { $_ -ne $null })
    $dif = if ($a.Count -eq 0 -or $b.Count -eq 0) { @($a | ForEach-Object { [pscustomobject]@{ SideIndicator = '<='; InputObject = $_ } }) + @($b | ForEach-Object { [pscustomobject]@{ SideIndicator = '=>'; InputObject = $_ } }) } else { @(Compare-Object $a $b) }
    $diferente += $dif.Count
    Write-Output ("{0,-13} A {1,5}  B {2,5}  diferențe {3}" -f $parte, $a.Count, $b.Count, $dif.Count)
    foreach ($d in ($dif | Select-Object -First 15)) { Write-Output ("   {0} {1}" -f $d.SideIndicator, $d.InputObject) }
}

$implicite = "select table_name, column_name, coalesce(column_default, '') from information_schema.columns where table_schema = 'public' order by 1, 2"
$ia = Citeste $BazaA $implicite; $ib = Citeste $BazaB $implicite
$cheieB = @{}; foreach ($r in $ib) { $p = $r -split ' \| ', 3; $cheieB["$($p[0]).$($p[1])"] = $p[2] }
$urme = @(); $straine = @()
foreach ($r in $ia) {
    $p = $r -split ' \| ', 3; $k = "$($p[0]).$($p[1])"
    if ($cheieB[$k] -ne $p[2]) {
        if ($cheieB[$k] -eq '' -and $p[2] -ne '') { $urme += "$k = $($p[2])" } else { $straine += "$k A='$($p[2])' B='$($cheieB[$k])'" }
    }
}
Write-Output ("implicite     urme ale lanțului (doar în A) {0}; alte diferențe {1}" -f $urme.Count, $straine.Count)
foreach ($u in $urme) { Write-Output "   urmă: $u" }
foreach ($s in $straine) { Write-Output "   DIFERIT: $s" }
$diferente += $straine.Count

if ($diferente -eq 0) { Write-Output "SCHEME IDENTICE: $BazaA = $BazaB (urme ale lanțului: $($urme.Count))" }
else { Write-Output "SCHEME DIFERITE: $diferente rânduri"; exit 1 }
