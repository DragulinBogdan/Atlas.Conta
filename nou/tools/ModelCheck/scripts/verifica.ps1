#requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet('Nucleu', 'Scenarii', 'Integral')][string]$Suita = 'Scenarii',
    [string]$Tip = 'BCS',
    [ValidateSet('Privat', 'Bugetar', 'Ambele')][string]$Profil = 'Ambele',
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.Codex',
    [switch]$PregatesteBaze
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$director = Join-Path $repo ('run-verificari/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$mutex = [Threading.Mutex]::new($false, 'Local\Atlas.Conta.Verificari')
$preluat = $false
$locatieSchimbata = $false
$sufixAnterior = $env:MODELCHECK_BAZA_SUFIX
$tempAnterior = $env:TEMP
$tmpAnterior = $env:TMP
$rezultate = [Collections.Generic.List[object]]::new()
$inceput = Get-Date
$codIesire = 1

function Executa([string]$Etapa, [string]$Executabil, [string[]]$Argumente) {
    $log = Join-Path $director ($Etapa + '.log')
    $ceas = [Diagnostics.Stopwatch]::StartNew()
    & $Executabil @Argumente 2>&1 | Tee-Object -FilePath $log | Out-Host
    $cod = $LASTEXITCODE
    $rezultate.Add([ordered]@{
        etapa = $Etapa; comanda = $Executabil; argumente = $Argumente
        exit = $cod; secunde = [Math]::Round($ceas.Elapsed.TotalSeconds, 2); log = $log
    })
    if ($cod -ne 0) { throw "$Etapa a eșuat cu exit $cod. Log: $log" }
    if ($Etapa.StartsWith('scenarii-') -and (
        !(Select-String -LiteralPath $log -Pattern '^Scenarii ' -Quiet) -or
        !(Select-String -LiteralPath $log -Pattern '^Toate verificările au trecut\.' -Quiet) -or
        (Select-String -LiteralPath $log -Pattern ': NICIUNA$' -Quiet))) {
        throw "Scenariile nu au fost executate. Log: $log"
    }
}

try {
    try { $preluat = $mutex.WaitOne(0) }
    catch [Threading.AbandonedMutexException] { $preluat = $true }
    if (!$preluat) { throw 'Altă verificare Atlas.Conta rulează prin acest script; așteaptă încheierea ei.' }
    New-Item -ItemType Directory -Path $director -Force | Out-Null
    $temporar = Join-Path $director 'tmp'
    New-Item -ItemType Directory -Path $temporar | Out-Null
    $env:TEMP = $temporar
    $env:TMP = $temporar
    Push-Location $repo
    $locatieSchimbata = $true
    $env:MODELCHECK_BAZA_SUFIX = $Sufix
    $commit = (& git rev-parse HEAD).Trim()
    $modificari = @(& git status --short)
    Write-Host "Suita=$Suita; profil=$Profil; sufix=$Sufix; commit=$commit; loguri=$director"
    if ($Suita -eq 'Nucleu') {
        Executa 'nucleu' 'dotnet' @('test', 'nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.slnx', '--nologo')
    }
    else {
        if ($PregatesteBaze) {
            Executa 'pregatire' 'python' @('-X', 'utf8', (Join-Path $PSScriptRoot 'pregateste-baze.py'), '--sufix', $Sufix, '--profil', $Profil)
        }
        Executa 'build' 'dotnet' @('build', 'nou/tools/ModelCheck/ModelCheck.csproj', '--nologo')
        $dll = Join-Path $repo 'nou/tools/ModelCheck/bin/Debug/net10.0/ModelCheck.dll'
        $hash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
        foreach ($profilCurent in @('Bugetar', 'Privat')) {
            if ($Profil -notin @($profilCurent, 'Ambele')) { continue }
            $argumente = @($dll)
            if ($Suita -eq 'Scenarii') { $argumente += @('--scenarii', $Tip) }
            if ($profilCurent -eq 'Privat') { $argumente += 'privat' }
            Executa ($Suita.ToLowerInvariant() + '-' + $profilCurent.ToLowerInvariant()) 'dotnet' $argumente
        }
    }
    $codIesire = 0
}
catch {
    Write-Host $_ -ForegroundColor Red
}
finally {
    if (Test-Path -LiteralPath $director) {
        [ordered]@{
            inceput = $inceput.ToUniversalTime().ToString('o'); sfarsit = (Get-Date).ToUniversalTime().ToString('o')
            commit = $commit; modificari = $modificari; suita = $Suita; profil = $Profil; tip = $Tip
            baze = @(if ($Suita -ne 'Nucleu') {
                if ($Profil -in @('Bugetar', 'Ambele')) { "Atlas.Conta.BackOffice$Sufix" }
                if ($Profil -in @('Privat', 'Ambele')) { "Atlas.Conta.ModelCheck.Privat$Sufix" }
            })
            dllSha256 = $hash; exit = $codIesire; etape = $rezultate.ToArray()
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $director 'rezultat.json') -Encoding utf8
    }
    $env:MODELCHECK_BAZA_SUFIX = $sufixAnterior
    $env:TEMP = $tempAnterior
    $env:TMP = $tmpAnterior
    if ($locatieSchimbata) { Pop-Location }
    if ($preluat) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
exit $codIesire
