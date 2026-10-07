#requires -Version 7.0
<#
.SYNOPSIS
Izolarea mutanților perechii: scoate pe rând câte o disjuncție din predicatul `VerificaPerechi` și cere
să supraviețuiască numai mutantul ei.

.DESCRIPTION
Modifică temporar sursa invariantului și o restaurează. Blocajul verificărilor e luat înaintea citirii sursei
și ținut până după restaurare și rularea finală; `verifica.ps1` e apelat în același fir, deci îl reia.
Fără blocaj, sursa și binarele rămân neatinse. `-ProbaClasificator` probează numai judecata ieșirii, fără bază.
#>
[CmdletBinding()]
param(
    [ValidatePattern('^[._][A-Za-z0-9_-]{1,24}$')][string]$Sufix,
    [string]$Jurnal,
    [switch]$ProbaClasificator
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$sursa = Join-Path $repo 'nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Invarianti.cs'
$wrapper = Join-Path $PSScriptRoot 'verifica.ps1'
$rulari = Join-Path $repo 'run-verificari'
$scene = 'DESCHIDERE,BTR,BCS'
$nr8 = 'FAIL N-r8: `FelTranzactie.Transfer` apare'

# Mutanții perechii executați de scenele alese; cei doi de transformare cer ASM și nu țin de acest predicat.
$executati = @(
    'PERECHE-RUPTA', 'PERECHE-LIPSA', 'PERECHE-ORDINAL', 'PERECHE-CAUZA', 'PERECHE-VALOARE', 'PERECHE-VALUTA',
    'PERECHE-CANTITATE', 'PERECHE-LATURA', 'PERECHE-DOCUMENT', 'PERECHE-TRANSFER-LATURA',
    'PERECHE-TRANSFER-VALOARE', 'PERECHE-TRANSFER-VALUTA', 'PERECHE-DESCHIDERE')

$variante = @(
    @{ Mutant = 'PERECHE-DOCUMENT'; Vechi = 'x.Cate != 2 || x.Documente != 1 || x.Linii != 1'; Nou = 'x.Cate != 2 || x.Linii != 1'; Alte = @() }
    @{ Mutant = 'PERECHE-DESCHIDERE'; Vechi = '|| x.Fel == N.FelTranzactie.Deschidere'; Nou = ''; Alte = @() }
    # Proba de sursă N-r8 cere mențiunea `FelTranzactie.Transfer` în `VerificaPerechi`; varianta o scoate.
    @{ Mutant = 'PERECHE-TRANSFER-LATURA'; Vechi = '(x.Fel == N.FelTranzactie.Transfer && x.Laturi != 1)'; Nou = 'false'; Alte = @($nr8) }
    @{ Mutant = 'PERECHE-TRANSFER-VALOARE'; Vechi = 'x.Laturi == 1 && x.Valoare == 0m && x.Valuta == 0m'; Nou = 'x.Laturi == 1 && x.Valuta == 0m'; Alte = @() }
    @{ Mutant = 'PERECHE-TRANSFER-VALUTA'; Vechi = 'x.Laturi == 1 && x.Valoare == 0m && x.Valuta == 0m'; Nou = 'x.Laturi == 1 && x.Valoare == 0m'; Alte = @() }
)

# Abaterile unei rulări față de așteptare: starea exactă a fiecărui mutant, celelalte FAIL-uri, codul de ieșire.
function Abateri([string[]]$Linii, [int]$CodIesire, [string]$Supravietuitor, [string[]]$AlteAsteptate) {
    $stari = @{}
    foreach ($linie in $Linii) {
        if ($linie -cmatch '^(OK|FAIL)\s+INV-CUB-(PERECHE-[A-Z-]+) ') {
            if (!$stari.ContainsKey($Matches[2])) { $stari[$Matches[2]] = [Collections.Generic.SortedSet[string]]::new() }
            [void]$stari[$Matches[2]].Add($Matches[1])
        }
    }
    $abateri = [Collections.Generic.List[string]]::new()
    foreach ($nume in $executati) {
        $asteptat = if ($nume -eq $Supravietuitor) { 'FAIL' } else { 'OK' }
        $gasit = if ($stari.ContainsKey($nume)) { $stari[$nume] -join '+' } else { 'absent' }
        if ($gasit -cne $asteptat) { $abateri.Add(('{0}: {1}, așteptat {2}' -f $nume, $gasit, $asteptat)) }
    }
    foreach ($nume in $stari.Keys) {
        if ($nume -notin $executati) { $abateri.Add(('{0}: în afara selecției așteptate' -f $nume)) }
    }
    $alte = @($Linii | Where-Object { $_ -cmatch '^FAIL' -and $_ -cnotmatch 'INV-CUB-PERECHE' })
    foreach ($asteptata in $AlteAsteptate) {
        $cate = @($alte | Where-Object { $_.StartsWith($asteptata, [StringComparison]::Ordinal) }).Count
        if ($cate -ne 1) { $abateri.Add(('FAIL așteptat de {0} ori, nu o dată: {1}' -f $cate, $asteptata)) }
    }
    foreach ($linie in $alte) {
        if (!($AlteAsteptate | Where-Object { $linie.StartsWith($_, [StringComparison]::Ordinal) })) {
            $abateri.Add('FAIL neașteptat: ' + $linie.Substring(0, [Math]::Min(200, $linie.Length)))
        }
    }
    $exitAsteptat = if ($Supravietuitor -or $AlteAsteptate.Count -gt 0) { 1 } else { 0 }
    if ($CodIesire -ne $exitAsteptat) { $abateri.Add(('exit {0}, așteptat {1}' -f $CodIesire, $exitAsteptat)) }
    return , $abateri.ToArray()
}

function Linii-Sintetice([hashtable]$Stari, [string[]]$Alte = @()) {
    $linii = [Collections.Generic.List[string]]::new()
    foreach ($nume in $Stari.Keys) {
        foreach ($stare in $Stari[$nume]) { $linii.Add(('{0,-4} INV-CUB-{1} (privat): sintetic' -f $stare, $nume)) }
    }
    $linii.AddRange($Alte)
    return , $linii.ToArray()
}

if ($ProbaClasificator) {
    function Stari([string]$Supravietuitor) {
        $s = @{}
        foreach ($nume in $executati) { $s[$nume] = @(if ($nume -eq $Supravietuitor) { 'FAIL', 'FAIL' } else { 'OK' }) }
        return $s
    }
    $amestec = Stari 'PERECHE-DOCUMENT'; $amestec['PERECHE-CAUZA'] = @('FAIL', 'OK')
    $absent = Stari 'PERECHE-DOCUMENT'; $absent.Remove('PERECHE-VALUTA')
    $strain = Stari 'PERECHE-DOCUMENT'; $strain['PERECHE-TRANSFORMARE-VALORICA'] = @('OK')
    $ucis = Stari $null
    $cazuri = @(
        @{ Nume = 'varianta exactă'; Linii = (Linii-Sintetice (Stari 'PERECHE-DOCUMENT')); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $true }
        @{ Nume = 'alt mutant cu FAIL urmat de OK (IZ-R1)'; Linii = (Linii-Sintetice $amestec); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'mutant absent din rulare'; Linii = (Linii-Sintetice $absent); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'mutant din afara selecției'; Linii = (Linii-Sintetice $strain); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'mutantul așteptat e ucis'; Linii = (Linii-Sintetice $ucis); Exit = 0; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'FAIL străin de mutanți'; Linii = (Linii-Sintetice (Stari 'PERECHE-DOCUMENT') @('FAIL SC-BCS-01 (privat): sintetic')); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'latura transferului cu N-r8'; Linii = (Linii-Sintetice (Stari 'PERECHE-TRANSFER-LATURA') @($nr8 + ' sintetic')); Exit = 1; S = 'PERECHE-TRANSFER-LATURA'; Alte = @($nr8); Acceptat = $true }
        @{ Nume = 'latura transferului fără N-r8'; Linii = (Linii-Sintetice (Stari 'PERECHE-TRANSFER-LATURA')); Exit = 1; S = 'PERECHE-TRANSFER-LATURA'; Alte = @($nr8); Acceptat = $false }
        @{ Nume = 'N-r8 unde nu e așteptat'; Linii = (Linii-Sintetice (Stari 'PERECHE-DOCUMENT') @($nr8 + ' sintetic')); Exit = 1; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
        @{ Nume = 'predicatul întreg'; Linii = (Linii-Sintetice $ucis); Exit = 0; S = $null; Alte = @(); Acceptat = $true }
        @{ Nume = 'predicatul întreg cu exit 1'; Linii = (Linii-Sintetice $ucis); Exit = 1; S = $null; Alte = @(); Acceptat = $false }
        @{ Nume = 'variantă cu exit 0'; Linii = (Linii-Sintetice (Stari 'PERECHE-DOCUMENT')); Exit = 0; S = 'PERECHE-DOCUMENT'; Alte = @(); Acceptat = $false }
    )
    $picate = 0
    foreach ($caz in $cazuri) {
        $abateri = Abateri $caz.Linii $caz.Exit $caz.S $caz.Alte
        $acceptat = $abateri.Count -eq 0
        $corect = $acceptat -eq $caz.Acceptat
        if (!$corect) { $picate++ }
        Write-Output ('{0} {1}: {2}{3}' -f $(if ($corect) { 'OK  ' } else { 'FAIL' }), $caz.Nume,
            $(if ($acceptat) { 'acceptat' } else { 'refuzat' }), $(if ($abateri.Count) { ' — ' + ($abateri -join '; ') } else { '' }))
    }
    exit $(if ($picate) { 1 } else { 0 })
}

if (!$Sufix -or !$Jurnal) { throw 'Rețeta cere -Sufix și -Jurnal.' }

function Ruleaza {
    $inainte = @(Get-ChildItem -LiteralPath $rulari -Directory -Name -ErrorAction SilentlyContinue)
    & $wrapper -Suita Scenarii -Tip $scene -Profil Privat -Sufix $Sufix | Out-Host
    $cod = $LASTEXITCODE
    $director = @(Get-ChildItem -LiteralPath $rulari -Directory -Name |
        Where-Object { $_ -notin $inainte -and $_ -match '^\d{8}-\d{6}-\d{3}$' } | Sort-Object) | Select-Object -Last 1
    $log = if ($director) { Join-Path $rulari "$director/scenarii-privat.log" }
    $linii = if ($log -and (Test-Path -LiteralPath $log)) { @(Get-Content -LiteralPath $log -Encoding utf8) } else { @() }
    [pscustomobject]@{ Exit = $cod; Director = $director; Linii = $linii }
}

$consemnari = [Collections.Generic.List[string]]::new()
function Judeca([string]$Nume, $Rulare, [string]$Supravietuitor, [string[]]$AlteAsteptate) {
    $abateri = Abateri $Rulare.Linii $Rulare.Exit $Supravietuitor $AlteAsteptate
    $rand = '{0} {1}: {2}; exit {3}; {4}' -f $(if ($abateri.Count) { 'FAIL' } else { 'OK  ' }), $Nume,
        $(if ($Supravietuitor) { "supraviețuiește numai $Supravietuitor, ceilalți $($executati.Count - 1) uciși" } else { "toți cei $($executati.Count) uciși" }),
        $Rulare.Exit, $Rulare.Director
    $consemnari.Add($rand)
    foreach ($abatere in $abateri) { $consemnari.Add('    ' + $abatere) }
    Write-Host $rand
    return $abateri.Count -eq 0
}

$codare = [Text.UTF8Encoding]::new($false)
$mutex = [Threading.Mutex]::new($false, 'Local\Atlas.Conta.Verificari')
$preluat = $false
$bine = $true
try {
    try { $preluat = $mutex.WaitOne(0) }
    catch [Threading.AbandonedMutexException] { $preluat = $true }
    if (!$preluat) { throw 'Altă verificare Atlas.Conta rulează; sursa și binarele rămân neatinse.' }
    $octeti = [IO.File]::ReadAllBytes($sursa)
    $text = $codare.GetString($octeti)
    $amprenta = (Get-FileHash -LiteralPath $sursa -Algorithm SHA1).Hash
    foreach ($varianta in $variante) {
        if ([regex]::Matches($text, [regex]::Escape($varianta.Vechi)).Count -ne 1) {
            throw "Predicatul nu mai are forma așteptată pentru $($varianta.Mutant); rețeta trebuie adusă la zi."
        }
    }
    try {
        foreach ($varianta in $variante) {
            [IO.File]::WriteAllBytes($sursa, $codare.GetBytes($text.Replace($varianta.Vechi, $varianta.Nou)))
            $bine = (Judeca "fără disjuncția lui $($varianta.Mutant)" (Ruleaza) $varianta.Mutant $varianta.Alte) -and $bine
        }
    }
    catch {
        $bine = $false
        $consemnari.Add('FAIL excepție în variante: ' + $_)
        Write-Host $_ -ForegroundColor Red
    }
    finally {
        [IO.File]::WriteAllBytes($sursa, $octeti)
    }
    $bine = (Judeca 'predicatul întreg, după restaurare' (Ruleaza) $null @()) -and $bine
    $restaurat = (Get-FileHash -LiteralPath $sursa -Algorithm SHA1).Hash
    $identic = $restaurat -eq $amprenta
    $bine = $identic -and $bine
    $consemnari.Add(('{0} sursa restaurată identic (sha1 {1})' -f $(if ($identic) { 'OK  ' } else { 'FAIL' }), $restaurat.ToLowerInvariant()))
    Write-Host $consemnari[-1]
}
catch {
    $bine = $false
    $consemnari.Add('FAIL ' + $_)
    Write-Host $_ -ForegroundColor Red
}
finally {
    if ($preluat) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
    Set-Content -LiteralPath $Jurnal -Value $consemnari -Encoding utf8
}
exit $(if ($bine) { 0 } else { 1 })
