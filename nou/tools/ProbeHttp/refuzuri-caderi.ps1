#requires -Version 7
<#
  refuzuri-caderi.ps1 — fixture-ul lui `refuzuri.ps1` se desface complet la o cădere
  după oricare dintre mutațiile lui (104-r5, review C104 pas 5).

  Rulează `refuzuri.ps1 -CadeDupa <punct>` pentru fiecare punct și cere cod 2
  (căderea injectată) și „rezidu fixture: zero”; codul 3 înseamnă rezidu rămas.

  pwsh -File nou/tools/ProbeHttp/refuzuri-caderi.ps1 [-HostUrl https://localhost:5001]
  Cod de ieșire: 0 = toate punctele curate, 1 = cel puțin un punct cu rezidu sau cod neașteptat.
#>
[CmdletBinding()]
param([string]$HostUrl = 'https://localhost:5001')

[Console]::OutputEncoding = [Text.Encoding]::UTF8
$matrice = Join-Path $PSScriptRoot 'refuzuri.ps1'
$puncte = 'Furnizor', 'Fct', 'OperareFct', 'OperarePlt', 'Imperechere', 'Itv', 'Angajat'
$picate = @()
foreach ($punct in $puncte) {
    $iesire = & pwsh -NoProfile -File $matrice -HostUrl $HostUrl -Utilizatori Admin -CadeDupa $punct 2>&1 | Out-String
    $cod = $LASTEXITCODE
    $curat = $iesire -match 'rezidu fixture: zero'
    $verdict = if ($cod -eq 2 -and $curat) { 'PASS' } else { 'FAIL' }
    Write-Host ("{0,-12} cod {1}  {2}" -f $punct, $cod, $verdict) -ForegroundColor ($verdict -eq 'PASS' ? 'Green' : 'Red')
    if ($verdict -ne 'PASS') {
        $picate += $punct
        Write-Host ($iesire -split "`n" | Select-String 'EROARE|REZIDU|curățenia' | Out-String)
    }
}
Write-Host "$($puncte.Count - $picate.Count)/$($puncte.Count) puncte de cădere fără rezidu"
exit ($picate.Count -gt 0 ? 1 : 0)
