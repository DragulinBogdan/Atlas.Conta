#requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet('Nucleu', 'Scenarii', 'Integral', 'Infrastructura', 'D9bReferinta', 'D9bScriere', 'D9bComparatie', 'D9bRaport', 'D9bHttp')][string]$Suita = 'Scenarii',
    [string]$Tip = 'BCS',
    [ValidateSet('Privat', 'Bugetar', 'Ambele')][string]$Profil = 'Ambele',
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix = '.Codex',
    [switch]$PregatesteBaze,
    [string]$Citiri,
    [string]$ScriereA,
    [string]$ScriereB
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
$modEroriAnterior = $null
. (Join-Path $PSScriptRoot 'mod-nesupravegheat.ps1')

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
    $modEroriAnterior = Activeaza-ModNesupravegheat
    New-Item -ItemType Directory -Path $director -Force | Out-Null
    $temporar = Join-Path $director 'tmp'
    New-Item -ItemType Directory -Path $temporar | Out-Null
    $env:TEMP = $temporar
    $env:TMP = $temporar
    Push-Location $repo
    $locatieSchimbata = $true
    $env:MODELCHECK_BAZA_SUFIX = $Sufix
    if ($Suita -eq 'D9bReferinta' -and ($Profil -ne 'Privat' -or $Sufix -ne '.CodexD9bA')) {
        throw 'Referința D9b cere -Profil Privat -Sufix .CodexD9bA.'
    }
    if ($Suita -eq 'D9bComparatie' -and ($Profil -ne 'Privat' -or $Sufix -ne '.CodexD9bB')) {
        throw 'Comparația D9b cere -Profil Privat -Sufix .CodexD9bB.'
    }
    if ($Suita -eq 'D9bScriere' -and ($Profil -ne 'Privat' -or $Sufix -notin @('.CodexD9bA', '.CodexD9bB'))) {
        throw 'Scrierea D9b cere -Profil Privat și o bază D9b dedicată.'
    }
    $commit = (& git rev-parse HEAD).Trim()
    $modificari = @(& git status --short)
    Write-Host "Suita=$Suita; profil=$Profil; sufix=$Sufix; commit=$commit; loguri=$director"
    if ($Suita -eq 'Infrastructura') {
        Executa 'infrastructura' 'pwsh' @('-NoProfile', '-File',
            (Join-Path $PSScriptRoot 'probe-exceptii.ps1'), '-Director', $director)
    }
    elseif ($Suita -eq 'D9bRaport') {
        if (!$Citiri -or !$ScriereA -or !$ScriereB) { throw 'Raportul cere -Citiri, -ScriereA și -ScriereB.' }
        Executa 'raport-d9b' 'python' @('-X', 'utf8', (Join-Path $PSScriptRoot 'd9b-raport.py'),
            '--citiri', $Citiri, '--scriere-a', $ScriereA, '--scriere-b', $ScriereB,
            '--destinatie', (Join-Path $director 'comparatie-completa.json'))
    }
    elseif ($Suita -eq 'D9bHttp') {
        if ($Profil -ne 'Privat') { throw 'D9bHttp cere -Profil Privat.' }
        Executa 'http-d9b' 'pwsh' @('-NoProfile', '-File',
            (Join-Path $PSScriptRoot 'd9b-http.ps1'), '-Director', $director, '-Sufix', $Sufix)
    }
    elseif ($Suita -eq 'Nucleu') {
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
            if ($Suita -eq 'D9bReferinta') { $argumente += @('--d9b-referinta', $director) }
            if ($Suita -eq 'D9bScriere') { $argumente += @('--d9b-scriere', $director) }
            if ($Suita -eq 'D9bComparatie') { $argumente += @('--d9b-comparatie', $director) }
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
    Restaureaza-ModErori $modEroriAnterior
    if (Test-Path -LiteralPath $director) {
        [ordered]@{
            inceput = $inceput.ToUniversalTime().ToString('o'); sfarsit = (Get-Date).ToUniversalTime().ToString('o')
            commit = $commit; modificari = $modificari; suita = $Suita; profil = $Profil; tip = $Tip
            baze = @(if ($Suita -in @('Scenarii', 'Integral', 'D9bReferinta', 'D9bScriere', 'D9bComparatie')) {
                if ($Profil -in @('Bugetar', 'Ambele')) { "Atlas.Conta.BackOffice$Sufix" }
                if ($Profil -in @('Privat', 'Ambele')) { "Atlas.Conta.ModelCheck.Privat$Sufix" }
            } elseif ($Suita -eq 'D9bHttp') { "Atlas.Conta.Http.Privat$Sufix" })
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
