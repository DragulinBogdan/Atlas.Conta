#requires -Version 7
<#
═══════════════════════════════════════════════════════════════════════════════
  refuzuri.ps1 — matricea de refuzuri de ACCES, măsurată pe HTTP (felia 22, F22-D9)
═══════════════════════════════════════════════════════════════════════════════

CE PROBEAZĂ
  Regula unică a lui F22-D1, pe toate ușile: 401 → 400 (binding) → 404 → 403 → 422.
    * 404 = subiectul cererii nu ți-e vizibil (inexistent SAU ascuns de securitate,
      DELIBERAT nedistinse — altfel API-ul devine un oracol de existență);
    * 403 = subiectul e vizibil (sau întrebarea e pe TIP), dar operația nu ți-e
      permisă — Create pe tip, Write/Delete pe instanță, Read pe tip;
    * 422 = refuz de DOMENIU, pe o cerere pe care AI dreptul s-o faci;
    * corpul e `EroriDto` (`{"Erori":[…]}`, `application/json`) pe TOATE 403/404
      ale feliei — inclusiv pe `api/odata/*`, unde până la felia 22 ieșea
      `text/plain` englezesc (70-r1 / 77-r8).

DE CE PE HTTP, ȘI NU ÎN ModelCheck (66h, F22-D9)
  ModelCheck rulează pe ObjectSpace-uri NEAUTENTIFICATE: nu are strategie de
  securitate, deci nu poate distinge 403 de 422 și nu vede deloc filtrarea
  `SecurityQueryCompiler`. Ordinea dintre gardianul de domeniu, verificarea
  DevExpress din `SaveChanges` și filtrele MVC/OData e o proprietate a
  PIPELINE-ului, nu a Module-ului — se măsoară doar pe calea reală.

CEI TREI UTILIZATORI (F22-D7)
  Admin   — administrator: trece de orice permisiune, deci arată DOMENIUL (422).
  Cititor — rolul `Cititori`: Read pe tot, zero Create/Write/Delete ⇒ oracolul
            lui 403 (subiectul e vizibil, operația nu).
  User    — rolul `Default`: nu vede documentele ⇒ oracolul lui 404 pe instanțe,
            403 pe întrebările fără subiect (tip), 200 gol pe liste.

FĂRĂ URME
  Tot ce scrie pe `Admin` (un NIR draft) se șterge în `finally`. `POST
  api/itv/genereaza` se probează DOAR ca `Cititor`/`User`: pe `Admin` ar SCRIE un
  draft ori de câte ori luna e liberă (capcana feliei 21) — iar luna cerută e
  una DEJA închisă (`InchidereVie`), ca nici măcar un gate picat să nu scrie.
  A doua rulare consecutivă trebuie să dea exact aceleași rezultate.

UTILIZARE
  pwsh -File nou/tools/ProbeHttp/refuzuri.ps1
  pwsh -File nou/tools/ProbeHttp/refuzuri.ps1 -Host https://localhost:5001 -Utilizatori Admin,Cititor,User
  Cod de ieșire: 0 = toate PASS, 1 = cel puțin un FAIL, 2 = descoperirea a picat.
#>

[CmdletBinding()]
param(
    [Alias('Host')]
    [string]$HostUrl = 'https://localhost:5001',
    [string[]]$Utilizatori = @('Admin', 'Cititor', 'User')
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$OutputEncoding = [Text.Encoding]::UTF8

# ── Transport ───────────────────────────────────────────────────────────────
# `-SkipHttpErrorCheck` e obligatoriu: aici 4xx-ul E răspunsul așteptat, nu o
# excepție. `-SkipCertificateCheck` — certificat de dev.
function Invoke-Cerere {
    param(
        [string]$Metoda,
        [string]$Cale,
        [string]$Token,
        $Corp
    )
    $anteturi = @{}
    if ($Token) { $anteturi['Authorization'] = "Bearer $Token" }
    $parametri = @{
        Uri                  = "$HostUrl$Cale"
        Method               = $Metoda
        Headers              = $anteturi
        SkipCertificateCheck = $true
        SkipHttpErrorCheck   = $true
        MaximumRedirection   = 0
        TimeoutSec           = 120
    }
    if ($null -ne $Corp) {
        $parametri['Body'] = ($Corp | ConvertTo-Json -Depth 10 -Compress)
        $parametri['ContentType'] = 'application/json; charset=utf-8'
    }
    $cronometru = [Diagnostics.Stopwatch]::StartNew()
    $raspuns = Invoke-WebRequest @parametri
    $cronometru.Stop()
    # `Headers` e un dicționar generic — indexarea unei chei absente aruncă, iar
    # un 204 chiar n-are `Content-Type`.
    $tipContinut = ''
    if ($raspuns.Headers.ContainsKey('Content-Type')) {
        $tipContinut = ($raspuns.Headers['Content-Type'] -join '; ')
    }
    [pscustomobject]@{
        Status      = [int]$raspuns.StatusCode
        ContentType = $tipContinut
        Corp        = [string]$raspuns.Content
        Ms          = [int]$cronometru.ElapsedMilliseconds
    }
}

function Get-Token {
    param([string]$Utilizator)
    $r = Invoke-Cerere -Metoda POST -Cale '/api/Authentication/Authenticate' `
        -Corp @{ UserName = $Utilizator; Password = '' }
    if ($r.Status -ne 200) {
        throw "Autentificarea lui $Utilizator a picat: HTTP $($r.Status) — $($r.Corp)"
    }
    # Tokenul vine BRUT (string, nu JSON) — vezi memoria de rulare a pasului 5.
    $r.Corp.Trim('"')
}

# ── Acumulatorul de probe ───────────────────────────────────────────────────
$script:Rezultate = [Collections.Generic.List[object]]::new()
$script:Numar = 0

function Scurt {
    param([string]$Text, [int]$Lungime = 90)
    if (-not $Text) { return '' }
    $t = ($Text -replace '\s+', ' ').Trim()
    if ($t.Length -le $Lungime) { return $t }
    $t.Substring(0, $Lungime) + '…'
}

<#
  O probă = o cerere + un verdict. Verdictul cere TREI lucruri deodată, fiindcă
  „aproape corect" e FAIL:
    * statusul exact;
    * fragmentele din corp (`-Contine`) — un 403 care spune „modifica" acolo
      unde adevărul e „crea" e un mesaj greșit, nu o nuanță;
    * `Content-Type: application/json` pe tot ce e refuz al feliei (`-FaraJson`
      scutește doar cererile al căror succes NU e JSON: 204, XML).
#>
function Proba {
    param(
        [string]$Cerere,
        [string]$User,
        [int]$Asteptat,
        [string]$Metoda,
        [string]$Cale,
        $Corp,
        [string[]]$Contine = @(),
        [switch]$FaraJson,
        [string]$Nota
    )
    $script:Numar++
    $token = $script:Tokenuri[$User]
    $r = Invoke-Cerere -Metoda $Metoda -Cale $Cale -Token $token -Corp $Corp

    # CAPCANĂ de shell: tokenizatorul PowerShell tratează „ (U+201E) ca pe un
    # ghilimele DUBLU de deschidere/închidere — un „ dintr-un literal `"…"`
    # TERMINĂ șirul și rupe parsarea. Ghilimelele românești se pun deci prin
    # variabile, sau în literali cu apostrof (acolo doar ' și rudele lui închid).
    $gd = [char]0x201E   # „
    $gi = [char]0x201D   # ”

    $motive = [Collections.Generic.List[string]]::new()
    if ($r.Status -ne $Asteptat) { $motive.Add("status $($r.Status) ≠ $Asteptat") }
    # Căutare de SUBȘIR, nu `-like`: fragmentele conțin `[`/`]` (`"data":[]`),
    # pe care `-like` le-ar citi ca pe o clasă de caractere goală și ar arunca.
    #
    # Corpul se compară DESCĂPAT: cele două uși serializează același `EroriDto`
    # cu setări diferite — MVC scrie UTF-8 brut („nu există”), conducta OData
    # scrie escape-uri JSON (`ă`). Ambele sunt același șir pentru orice
    # client care parsează JSON, deci diferența e de SERIALIZARE, nu de contract
    # (F22-D4 cere mesajul, nu octeții lui). Fără descăpare, cele patru rânduri
    # OData cu diacritice ar pica din vina probei, nu a serverului.
    $corpDescapat = [regex]::Replace($r.Corp, '\\u([0-9a-fA-F]{4})',
        { param($m) [char][Convert]::ToInt32($m.Groups[1].Value, 16) })
    foreach ($fragment in $Contine) {
        if (-not $corpDescapat.Contains($fragment, [StringComparison]::Ordinal)) {
            $motive.Add('corpul nu conține ' + $gd + $fragment + $gi)
        }
    }
    if (-not $FaraJson -and $r.ContentType -notlike 'application/json*') {
        $motive.Add('Content-Type ' + $gd + $r.ContentType + $gi + ' ≠ application/json')
    }

    $asteptatText = "$Asteptat"
    if ($Contine.Count -gt 0) {
        $asteptatText += ' + ' + $gd + ($Contine -join ($gi + ', ' + $gd)) + $gi
    }
    if ($Nota) { $asteptatText += " ($Nota)" }

    $verdict = 'PASS'
    if ($motive.Count -gt 0) { $verdict = 'FAIL' }
    $tipScurt = '—'
    if ($r.ContentType) { $tipScurt = ($r.ContentType -split ';')[0] }

    $rand = [pscustomobject]@{
        Nr         = $script:Numar
        Cerere     = "``$Metoda $Cale``"
        User       = $User
        Asteptat   = $asteptatText
        Primit     = "$($r.Status) $tipScurt"
        # Coloana arată corpul DESCĂPAT (lizibil); `CorpIntreg` păstrează octeții
        # exacți de pe sârmă, ca raportul de FAIL să nu ascundă nimic.
        Corp       = Scurt $corpDescapat
        Ms         = $r.Ms
        Verdict    = $verdict
        Motive     = ($motive -join '; ')
        CorpIntreg = $r.Corp
    }
    $script:Rezultate.Add($rand)
    $rand
}

# ═══ 1. Autentificare ═══════════════════════════════════════════════════════
Write-Host "Host: $HostUrl" -ForegroundColor Cyan
$script:Tokenuri = @{}
foreach ($u in $Utilizatori) {
    $script:Tokenuri[$u] = Get-Token $u
    Write-Host "  autentificat: $u" -ForegroundColor DarkGray
}
$tokenAdmin = $script:Tokenuri['Admin']
if (-not $tokenAdmin) { Write-Error 'Admin lipsește din -Utilizatori: matricea are nevoie de el pentru descoperire și curățenie.'; exit 2 }

# ═══ 2. Descoperirea datelor, prin API (nimic hardcodat) ════════════════════
# Scriptul nu presupune nimic despre baza pe care rulează: își găsește singur
# un partener, o gestiune, o unitate internă și o lună de ITV DEJA închisă.
function Get-PrimaEntitate {
    param([string]$Set)
    $r = Invoke-Cerere -Metoda GET -Cale "/api/odata/$Set`?`$top=1" -Token $tokenAdmin
    if ($r.Status -ne 200) { throw "Descoperirea lui $Set a picat: HTTP $($r.Status)" }
    $valori = ($r.Corp | ConvertFrom-Json).value
    if (-not $valori) { throw "Setul OData $Set e GOL — matricea are nevoie de un rând." }
    $valori[0]
}

$curatenie = [Collections.Generic.List[scriptblock]]::new()
$codIesire = 0

try {
    $partener = Get-PrimaEntitate 'Partener'
    $gestiune = Get-PrimaEntitate 'Gestiune'
    $unitate = Get-PrimaEntitate 'UnitateInterna'
    Write-Host "  partener: $($partener.Cod)  gestiune: $($gestiune.Cod)  unitate: $($unitate.Cod)" -ForegroundColor DarkGray

    # Luna ITV pentru `genereaza`: una cu închidere OPERATĂ (deci `InchidereVie`),
    # ca proba să nu poată scrie nici dacă gate-ul ar fi picat. Fallback: o lună
    # din trecutul îndepărtat, unde nu există sold de închis.
    $itvLista = (Invoke-Cerere -Metoda GET -Cale '/api/itv?take=50' -Token $tokenAdmin).Corp | ConvertFrom-Json
    $itvOperat = $itvLista.data | Where-Object { $_.Stare -eq 'Operat' } | Select-Object -First 1
    $itvOricare = $itvLista.data | Select-Object -First 1
    if ($itvOperat) { $anItv = $itvOperat.An; $lunaItv = $itvOperat.Luna }
    else { $anItv = 2001; $lunaItv = 1 }
    if (-not $itvOricare) { throw 'Nu există nicio închidere de TVA pe bază — proba `GET api/itv/{id}` n-are subiect.' }
    $idItv = $itvOricare.Id
    Write-Host "  itv: $($itvOricare.Numar) ($idItv); genereaza pe $anItv-$lunaItv" -ForegroundColor DarkGray

    # Perioada pentru fișierul SAF-T: luna unei închideri operate e o lună cu
    # activitate. Gate-ul de 403 al lui `User` vine oricum înaintea proiecției.
    $anSaft = if ($itvOperat) { $itvOperat.An } else { 2025 }
    $lunaSaft = if ($itvOperat) { $itvOperat.Luna } else { 1 }

    # Draftul de NIR al lui Admin — subiectul VIZIBIL al probelor de instanță.
    $corpNir = @{
        Data       = (Get-Date -Format 'yyyy-MM-dd')
        PredatorId = $partener.ID
        PrimitorId = $gestiune.ID
        Linii      = @()
    }
    $creare = Proba -Cerere 'creare NIR' -User 'Admin' -Asteptat 201 `
        -Metoda POST -Cale '/api/nir' -Corp $corpNir -Nota 'draftul de lucru'
    if ($creare.Verdict -ne 'PASS') { throw "Nu s-a putut crea draftul de lucru: $($creare.CorpIntreg)" }
    $idNir = ($creare.CorpIntreg | ConvertFrom-Json).Id
    $curatenie.Add({
            $sters = Invoke-Cerere -Metoda DELETE -Cale "/api/nir/$idNir" -Token $tokenAdmin
            Write-Host "curățenie: DELETE /api/nir/$idNir → $($sters.Status)" -ForegroundColor DarkGray
        }.GetNewClosure())
    Write-Host "  nir draft: $idNir" -ForegroundColor DarkGray

    $idInexistent = [guid]::NewGuid()

    # ═══ 3. Matricea ════════════════════════════════════════════════════════

    # ── REST scriere: gate-ul explicit pe tipul feliei (F22-D2) ─────────────
    # `Cititor` vede NIR-ul dar n-are Create ⇒ 403 „crea". `User` nu-l vede, dar
    # crearea n-are subiect ⇒ tot 403 (întrebarea e pe TIP) — și NU 422 „nu
    # există în nomenclator", care era refuzul primului FK invizibil (76-r5).
    Proba -Cerere 'creare NIR' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/nir' -Corp $corpNir -Contine 'crea' | Out-Null
    Proba -Cerere 'creare NIR' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/nir' -Corp $corpNir -Contine 'crea' -Nota '76-r5: NU 422 „nu există”' | Out-Null

    # PUT/DELETE pe instanță: `Cititor` o VEDE ⇒ 403 cu verbul potrivit,
    # `User` n-o vede ⇒ 404 (nu 403 — nu-i confirmăm existența).
    Proba -Cerere 'modificare NIR' -User 'Cititor' -Asteptat 403 -Metoda PUT -Cale "/api/nir/$idNir" -Corp $corpNir -Contine 'modifica' | Out-Null
    Proba -Cerere 'modificare NIR' -User 'User' -Asteptat 404 -Metoda PUT -Cale "/api/nir/$idNir" -Corp $corpNir -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'ștergere NIR' -User 'Cititor' -Asteptat 403 -Metoda DELETE -Cale "/api/nir/$idNir" -Contine 'șterge' | Out-Null
    Proba -Cerere 'ștergere NIR' -User 'User' -Asteptat 404 -Metoda DELETE -Cale "/api/nir/$idNir" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── REST comenzi ───────────────────────────────────────────────────────
    Proba -Cerere 'validare NIR' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale "/api/nir/$idNir/valideaza" -Contine 'modifica' | Out-Null
    Proba -Cerere 'validare NIR' -User 'User' -Asteptat 404 -Metoda POST -Cale "/api/nir/$idNir/valideaza" -Contine 'nu există sau nu e vizibil' | Out-Null
    # 76-r4, închisă de F22-D2: gate-ul comenzii e pe tipul FELIEI, nu pe
    # `Document` — un id de NIR pe ușa FCT nu mai trece gate-ul ca să pice 422
    # din Apply, ci e 404 pe loc, chiar și pentru Admin.
    Proba -Cerere 'operare id NIR pe ușa FCT' -User 'Admin' -Asteptat 404 -Metoda POST -Cale "/api/fct/$idNir/opereaza" -Contine 'nu există sau nu e vizibil' -Nota '76-r4' | Out-Null
    Proba -Cerere 'operare id inexistent' -User 'Admin' -Asteptat 404 -Metoda POST -Cale "/api/nir/$idInexistent/opereaza" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── REST citire ────────────────────────────────────────────────────────
    Proba -Cerere 'citire NIR' -User 'Admin' -Asteptat 200 -Metoda GET -Cale "/api/nir/$idNir" | Out-Null
    Proba -Cerere 'citire NIR' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/nir/$idNir" | Out-Null
    Proba -Cerere 'citire NIR' -User 'User' -Asteptat 404 -Metoda GET -Cale "/api/nir/$idNir" -Contine 'nu există sau nu e vizibil' | Out-Null
    # Lista rămâne 200 FILTRAT (F22-D1): o listă goală e un adevăr.
    Proba -Cerere 'listă NIR' -User 'User' -Asteptat 200 -Metoda GET -Cale '/api/nir?take=5' -Contine '"data":[]' -Nota '200 filtrat' | Out-Null

    # ── ITV: cifre ale motorului, două drepturi (F22-D5) ───────────────────
    Proba -Cerere 'previzualizare ITV' -User 'Admin' -Asteptat 200 -Metoda GET -Cale "/api/itv/previzualizare?an=$anItv&luna=$lunaItv" | Out-Null
    Proba -Cerere 'previzualizare ITV' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/itv/previzualizare?an=$anItv&luna=$lunaItv" -Nota 'Read pe tot, inclusiv registru' | Out-Null
    Proba -Cerere 'previzualizare ITV' -User 'User' -Asteptat 403 -Metoda GET -Cale "/api/itv/previzualizare?an=$anItv&luna=$lunaItv" -Contine 'citi' | Out-Null
    Proba -Cerere 'citire ITV' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/itv/$idItv" | Out-Null
    Proba -Cerere 'citire ITV' -User 'User' -Asteptat 404 -Metoda GET -Cale "/api/itv/$idItv" -Contine 'nu există sau nu e vizibil' | Out-Null
    # `genereaza` NUMAI pe cei doi fără drept: pe Admin ar scrie un draft.
    $corpItv = @{ An = $anItv; Luna = $lunaItv; UnitateId = $unitate.ID }
    Proba -Cerere 'generare ITV' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/itv/genereaza' -Corp $corpItv -Contine 'crea' | Out-Null
    Proba -Cerere 'generare ITV' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/itv/genereaza' -Corp $corpItv -Contine 'crea' | Out-Null

    # ── OData: același contract ca REST (F22-D4, închide 70-r1/77-r8) ───────
    $idPartener = $partener.ID
    Proba -Cerere 'citire Partener' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/odata/Partener($idPartener)" | Out-Null
    Proba -Cerere 'citire Partener' -User 'User' -Asteptat 404 -Metoda GET -Cale "/api/odata/Partener($idPartener)" -Contine 'nu există sau nu e vizibil' -Nota 'EroriDto, nu text/plain' | Out-Null
    # PROBA lui F22-D3: corpul are `Cod`/`Denumire` GOALE, adică exact ce refuză
    # gardianul (77k). Dreptul trebuie să răspundă ÎNAINTEA domeniului ⇒ 403
    # „crea", nu 422 „Codul este obligatoriu".
    $partenerGol = @{ Cod = ''; Denumire = '' }
    Proba -Cerere 'creare Partener (Cod gol)' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'crea' -Nota 'F22-D3: dreptul înaintea domeniului' | Out-Null
    Proba -Cerere 'creare Partener (Cod gol)' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'crea' | Out-Null
    # Admin trece de permisiune ⇒ ajunge la gardian ⇒ 422 de DOMENIU. Perechea
    # de mai sus fără asta n-ar dovedi nimic: ar putea fi un 403 care ascunde
    # regula. Admin NU scrie nimic — cererea e refuzată la commit.
    Proba -Cerere 'creare Partener (Cod gol)' -User 'Admin' -Asteptat 422 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'obligatoriu' -Nota 'domeniul rămâne' | Out-Null
    $patch = @{ Localitate = 'X' }
    Proba -Cerere 'modificare Partener' -User 'Cititor' -Asteptat 403 -Metoda PATCH -Cale "/api/odata/Partener($idPartener)" -Corp $patch -Contine 'modifica' | Out-Null
    Proba -Cerere 'modificare Partener' -User 'User' -Asteptat 404 -Metoda PATCH -Cale "/api/odata/Partener($idPartener)" -Corp $patch -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'ștergere Partener' -User 'Cititor' -Asteptat 403 -Metoda DELETE -Cale "/api/odata/Partener($idPartener)" -Contine 'șterge' | Out-Null
    Proba -Cerere 'ștergere Partener' -User 'User' -Asteptat 404 -Metoda DELETE -Cale "/api/odata/Partener($idPartener)" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── Fișier: un fișier gol semnat e o declarație falsă (73g) ─────────────
    Proba -Cerere 'fișier SAF-T' -User 'User' -Asteptat 403 -Metoda GET -Cale "/api/proiectii/saft/xml?an=$anSaft&luna=$lunaSaft" -Contine 'citi' | Out-Null
    # `Cititor` are Read pe tot: se RAPORTEAZĂ ce iese (200 fișier sau 422 de
    # profil), nu se impune un cod — codul depinde de datele bazei, nu de felie.
    $saftCititor = Invoke-Cerere -Metoda GET -Cale "/api/proiectii/saft/xml?an=$anSaft&luna=$lunaSaft" -Token $script:Tokenuri['Cititor']
    $script:Numar++
    $verdictSaft = 'PASS'
    $motivSaft = ''
    if ($saftCititor.Status -eq 403) {
        $verdictSaft = 'FAIL'
        $motivSaft = 'Cititor are Read pe tot — un 403 aici ar fi un defect'
    }
    $script:Rezultate.Add([pscustomobject]@{
            Nr         = $script:Numar
            Cerere     = "``GET /api/proiectii/saft/xml?an=$anSaft&luna=$lunaSaft``"
            User       = 'Cititor'
            Asteptat   = 'oricare ≠ 403 (raportat, nu impus)'
            Primit     = "$($saftCititor.Status) $(($saftCititor.ContentType -split ';')[0])"
            Corp       = Scurt $saftCititor.Corp
            Ms         = $saftCititor.Ms
            Verdict    = $verdictSaft
            Motive     = $motivSaft
            CorpIntreg = $saftCititor.Corp
        })

    # ── Imperecheri: aceeași formă ca feliile de document ───────────────────
    # Corpul e deliberat MINIM: gate-ul de creare e pe TIP și vine ÎNAINTEA
    # Apply-ului, deci un corp care n-ar trece domeniul tot 403 trebuie să dea.
    $corpImperechere = @{ DocumentStingatorId = $idInexistent; DocumentId = $idInexistent; Suma = 1 }
    Proba -Cerere 'creare împerechere' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/imperecheri' -Corp $corpImperechere -Contine 'crea' | Out-Null
    Proba -Cerere 'creare împerechere' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/imperecheri' -Corp $corpImperechere -Contine 'crea' | Out-Null
    Proba -Cerere 'ștergere împerechere inexistentă' -User 'Admin' -Asteptat 404 -Metoda DELETE -Cale "/api/imperecheri/$idInexistent" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── Neautentificat: 401 rămâne primul (F22-D11) ────────────────────────
    $anonim = Invoke-Cerere -Metoda GET -Cale "/api/nir/$idNir"
    $script:Numar++
    $verdictAnonim = 'PASS'
    $motivAnonim = ''
    if ($anonim.Status -ne 401) {
        $verdictAnonim = 'FAIL'
        $motivAnonim = "status $($anonim.Status) ≠ 401"
    }
    $tipAnonim = '—'
    if ($anonim.ContentType) { $tipAnonim = ($anonim.ContentType -split ';')[0] }
    $script:Rezultate.Add([pscustomobject]@{
            Nr         = $script:Numar
            Cerere     = '`GET /api/nir/{draft}` fără token'
            User       = '(anonim)'
            Asteptat   = '401'
            Primit     = "$($anonim.Status) $tipAnonim"
            Corp       = Scurt $anonim.Corp
            Ms         = $anonim.Ms
            Verdict    = $verdictAnonim
            Motive     = $motivAnonim
            CorpIntreg = $anonim.Corp
        })

    # Ștergerea draftului ca Admin, la final — ultima probă a matricei ȘI
    # curățenia. Rulează aici ca să apară în tabel; `finally` o repetă doar dacă
    # execuția s-a rupt înainte (al doilea DELETE ar da 404, benign).
    $stergere = Proba -Cerere 'ștergere NIR' -User 'Admin' -Asteptat 204 -Metoda DELETE -Cale "/api/nir/$idNir" -FaraJson -Nota 'curățenie'
    if ($stergere.Verdict -eq 'PASS') { $curatenie.Clear() }
}
catch {
    Write-Host "EROARE: $_" -ForegroundColor Red
    $codIesire = 2
}
finally {
    foreach ($pas in $curatenie) { try { & $pas } catch { Write-Host "curățenia a picat: $_" -ForegroundColor Red } }
}

# ═══ 4. Tabelul ═════════════════════════════════════════════════════════════
$linii = @()
$linii += '| # | cerere | user | așteptat | primit | corp (scurt) | ms | verdict |'
$linii += '|---|---|---|---|---|---|---|---|'
foreach ($r in $script:Rezultate) {
    $corp = ($r.Corp -replace '\|', '\|')
    $linii += "| $($r.Nr) | $($r.Cerere) | $($r.User) | $($r.Asteptat) | $($r.Primit) | $corp | $($r.Ms) | **$($r.Verdict)** |"
}
$linii | ForEach-Object { Write-Output $_ }

$fail = @($script:Rezultate | Where-Object Verdict -eq 'FAIL')
Write-Output ''
Write-Output "Total: $($script:Rezultate.Count) probe, $($script:Rezultate.Count - $fail.Count) PASS, $($fail.Count) FAIL."
foreach ($f in $fail) {
    Write-Output ''
    Write-Output "FAIL #$($f.Nr) — $($f.Cerere) ca $($f.User): $($f.Motive)"
    Write-Output "  corp: $($f.CorpIntreg)"
}

if ($fail.Count -gt 0 -and $codIesire -eq 0) { $codIesire = 1 }
exit $codIesire
