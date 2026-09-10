#requires -Version 7
<#
═══════════════════════════════════════════════════════════════════════════════
  refuzuri.ps1 — matricea de refuzuri de ACCES, măsurată pe HTTP (felia 22, F22-D9)
═══════════════════════════════════════════════════════════════════════════════

CE PROBEAZĂ
  Regula unică a lui F22-D1, pe toate ușile: 401 → 400 (binding) → 404 → 403 → 422.
  De la felia 23, aceeași regulă și pe POLITICI (ușa OData li s-a deschis,
  F23-D5): ce e refuz de domeniu pentru `Admin` (422 din `GardianEditare`)
  rămâne refuz de permisiune pentru ceilalți doi, cu verbul potrivit.
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

CEI PATRU UTILIZATORI (F22-D7, 83h)
  Admin        — administrator: trece de orice permisiune, deci arată DOMENIUL (422).
  Cititor      — rolul `Cititori`: Read pe tot, zero Create/Write/Delete ⇒ oracolul
                 lui 403 (subiectul e vizibil, operația nu).
  User         — rolul `Default`: nu vede documentele ⇒ oracolul lui 404 pe instanțe,
                 403 pe întrebările fără subiect (tip), 200 gol pe liste.
  Configurator — rolul `Configurator`: Read pe tot, Create/Write/Delete DOAR pe
                 `Politici.TipuriConfigurabile`. E oracolul separării de DREPTURI,
                 nu de rol administrativ: scrie politicile ca `Admin` (inclusiv
                 422-urile gardianului, care nu se uită la rol) și e refuzat cu
                 403 pe documente, comenzi, `Societate` și `Partener`.

FĂRĂ URME
  Tot ce scrie pe `Admin` (un NIR draft, iar de la felia 23 un partener și un
  rând de politică) se șterge în `finally`. `Configurator` are rândul LUI, pe o
  cheie proprie: dacă ar reface PATCH-ul pe rândul lui `Admin`, cererea ar putea
  fi un no-op și proba ar trece din alt motiv decât cel probat.
  `POST api/itv/genereaza` se probează DOAR ca `Cititor`/`User`/`Configurator`:
  pe `Admin` ar SCRIE un draft ori de câte ori luna e liberă (capcana feliei
  21) — iar luna cerută e una DEJA închisă
  (`InchidereVie`), ca nici măcar un gate picat să nu scrie. Niciun rând
  SEED-uit nu se modifică: probele de politică se fac pe rânduri create de
  script (vezi blocul F23 pentru ce anume NU se poate măsura din cauza asta).
  A doua rulare consecutivă trebuie să dea exact aceleași rezultate.

  Singura urmă rămasă, deliberat: rândurile de AUDIT produse de scrierile de mai
  sus. Un jurnal nu se șterge — de-aia e jurnal.

UTILIZARE
  pwsh -File nou/tools/ProbeHttp/refuzuri.ps1
  pwsh -File nou/tools/ProbeHttp/refuzuri.ps1 -Host https://localhost:5001 -Utilizatori Admin,Cititor,User,Configurator
  Cod de ieșire: 0 = toate PASS, 1 = cel puțin un FAIL, 2 = descoperirea a picat.
#>

[CmdletBinding()]
param(
    [Alias('Host')]
    [string]$HostUrl = 'https://localhost:5001',
    [string[]]$Utilizatori = @('Admin', 'Cititor', 'User', 'Configurator')
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
    # `-Filtru` (OData `$filter`, ne-encodat) pentru descoperirile care au nevoie
    # de un rând ANUME, pe codul lui: tipurile de document și de TVA sunt referite
    # prin cod în tot restul sistemului, deci scriptul le caută la fel.
    param([string]$Set, [string]$Filtru)
    $cale = "/api/odata/$Set`?`$top=1"
    if ($Filtru) { $cale += '&$filter=' + [uri]::EscapeDataString($Filtru) }
    $r = Invoke-Cerere -Metoda GET -Cale $cale -Token $tokenAdmin
    if ($r.Status -ne 200) { throw "Descoperirea lui $Set a picat: HTTP $($r.Status)" }
    $valori = ($r.Corp | ConvertFrom-Json).value
    if (-not $valori) { throw "Setul OData $Set$(if ($Filtru) { " ($Filtru)" }) e GOL — matricea are nevoie de un rând." }
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
    # 83h: Write pe `PoliticaTvaImplicit` nu se scurge spre `NIR` printr-un FK
    # comun — documentele nu sunt în `TipuriConfigurabile`.
    Proba -Cerere 'creare NIR' -User 'Configurator' -Asteptat 403 -Metoda POST -Cale '/api/nir' -Corp $corpNir -Contine 'crea' | Out-Null

    # PUT/DELETE pe instanță: `Cititor` o VEDE ⇒ 403 cu verbul potrivit,
    # `User` n-o vede ⇒ 404 (nu 403 — nu-i confirmăm existența).
    Proba -Cerere 'modificare NIR' -User 'Cititor' -Asteptat 403 -Metoda PUT -Cale "/api/nir/$idNir" -Corp $corpNir -Contine 'modifica' | Out-Null
    Proba -Cerere 'modificare NIR' -User 'User' -Asteptat 404 -Metoda PUT -Cale "/api/nir/$idNir" -Corp $corpNir -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'modificare NIR' -User 'Configurator' -Asteptat 403 -Metoda PUT -Cale "/api/nir/$idNir" -Corp $corpNir -Contine 'modifica' | Out-Null
    Proba -Cerere 'ștergere NIR' -User 'Cititor' -Asteptat 403 -Metoda DELETE -Cale "/api/nir/$idNir" -Contine 'șterge' | Out-Null
    Proba -Cerere 'ștergere NIR' -User 'User' -Asteptat 404 -Metoda DELETE -Cale "/api/nir/$idNir" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── REST comenzi ───────────────────────────────────────────────────────
    Proba -Cerere 'validare NIR' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale "/api/nir/$idNir/valideaza" -Contine 'modifica' | Out-Null
    Proba -Cerere 'validare NIR' -User 'User' -Asteptat 404 -Metoda POST -Cale "/api/nir/$idNir/valideaza" -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'validare NIR' -User 'Configurator' -Asteptat 403 -Metoda POST -Cale "/api/nir/$idNir/valideaza" -Contine 'modifica' | Out-Null
    # 76-r4, închisă de F22-D2: gate-ul comenzii e pe tipul FELIEI, nu pe
    # `Document` — un id de NIR pe ușa FCT nu mai trece gate-ul ca să pice 422
    # din Apply, ci e 404 pe loc, chiar și pentru Admin.
    Proba -Cerere 'operare id NIR pe ușa FCT' -User 'Admin' -Asteptat 404 -Metoda POST -Cale "/api/fct/$idNir/opereaza" -Contine 'nu există sau nu e vizibil' -Nota '76-r4' | Out-Null
    Proba -Cerere 'operare id inexistent' -User 'Admin' -Asteptat 404 -Metoda POST -Cale "/api/nir/$idInexistent/opereaza" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── REST citire ────────────────────────────────────────────────────────
    Proba -Cerere 'citire NIR' -User 'Admin' -Asteptat 200 -Metoda GET -Cale "/api/nir/$idNir" | Out-Null
    Proba -Cerere 'citire NIR' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/nir/$idNir" | Out-Null
    Proba -Cerere 'citire NIR' -User 'User' -Asteptat 404 -Metoda GET -Cale "/api/nir/$idNir" -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'citire NIR' -User 'Configurator' -Asteptat 200 -Metoda GET -Cale "/api/nir/$idNir" -Nota 'Read pe tot' | Out-Null
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
    Proba -Cerere 'generare ITV' -User 'Configurator' -Asteptat 403 -Metoda POST -Cale '/api/itv/genereaza' -Corp $corpItv -Contine 'crea' -Nota 'nu scrie: gate-ul e pe TIP' | Out-Null

    # ── OData: același contract ca REST (F22-D4, închide 70-r1/77-r8) ───────
    $idPartener = $partener.ID
    Proba -Cerere 'citire Partener' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/odata/Partener($idPartener)" | Out-Null
    Proba -Cerere 'citire Partener' -User 'User' -Asteptat 404 -Metoda GET -Cale "/api/odata/Partener($idPartener)" -Contine 'nu există sau nu e vizibil' -Nota 'EroriDto, nu text/plain' | Out-Null
    Proba -Cerere 'citire Partener' -User 'Configurator' -Asteptat 200 -Metoda GET -Cale "/api/odata/Partener($idPartener)" | Out-Null
    # PROBA lui F22-D3: corpul are `Cod`/`Denumire` GOALE, adică exact ce refuză
    # gardianul (77k). Dreptul trebuie să răspundă ÎNAINTEA domeniului ⇒ 403
    # „crea", nu 422 „Codul este obligatoriu".
    $partenerGol = @{ Cod = ''; Denumire = '' }
    Proba -Cerere 'creare Partener (Cod gol)' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'crea' -Nota 'F22-D3: dreptul înaintea domeniului' | Out-Null
    Proba -Cerere 'creare Partener (Cod gol)' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'crea' | Out-Null
    Proba -Cerere 'creare Partener (Cod gol)' -User 'Configurator' -Asteptat 403 -Metoda POST -Cale '/api/odata/Partener' -Corp $partenerGol -Contine 'crea' -Nota 'partenerul nu e configurabil' | Out-Null
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
    # Review 80 m4: PASS doar pe 200 (fișier) sau 422 (profil/societate) —
    # un 403 e defect (Cititor are Read pe tot), iar un 500 nu e „oricare".
    if ($saftCititor.Status -notin 200, 422) {
        $verdictSaft = 'FAIL'
        $motivSaft = if ($saftCititor.Status -eq 403) { 'Cititor are Read pe tot — un 403 aici ar fi un defect' } else { "așteptat 200 sau 422, primit $($saftCititor.Status)" }
    }
    $script:Rezultate.Add([pscustomobject]@{
            Nr         = $script:Numar
            Cerere     = "``GET /api/proiectii/saft/xml?an=$anSaft&luna=$lunaSaft``"
            User       = 'Cititor'
            Asteptat   = '200 sau 422 (raportat, nu impus)'
            Primit     = "$($saftCititor.Status) $(($saftCititor.ContentType -split ';')[0])"
            Corp       = Scurt $saftCititor.Corp
            Ms         = $saftCititor.Ms
            Verdict    = $verdictSaft
            Motive     = $motivSaft
            CorpIntreg = $saftCititor.Corp
        })

    # ── Ușa NTC pe un id de ITV (review 80 M1) ─────────────────────────────
    # Sub TPT `GetObjectByKey<NotaContabila>` găsește și închiderea; felia NTC o
    # EXCLUDE la citire (79c), deci gate-ul o exclude și el (`PeUsaNtc`): 404 pe
    # TOATE verbele, pentru toți — nu 422 cu numărul închiderii pe care GET o neagă.
    Proba -Cerere 'citire ITV pe ușa NTC' -User 'Admin' -Asteptat 404 -Metoda GET -Cale "/api/ntc/$idItv" -Contine 'nu există sau nu e vizibil' -Nota 'M1' | Out-Null
    Proba -Cerere 'modificare ITV pe ușa NTC' -User 'Admin' -Asteptat 404 -Metoda PUT -Cale "/api/ntc/$idItv" -Corp @{ Data = '2026-10-31'; Linii = @() } -Contine 'nu există sau nu e vizibil' -Nota 'M1: NU 422' | Out-Null
    Proba -Cerere 'modificare ITV pe ușa NTC' -User 'Cititor' -Asteptat 404 -Metoda PUT -Cale "/api/ntc/$idItv" -Corp @{ Data = '2026-10-31'; Linii = @() } -Contine 'nu există sau nu e vizibil' -Nota 'M1: NU 403' | Out-Null
    Proba -Cerere 'validare ITV pe ușa NTC' -User 'Admin' -Asteptat 404 -Metoda POST -Cale "/api/ntc/$idItv/valideaza" -Contine 'nu există sau nu e vizibil' -Nota 'M1: NU 200' | Out-Null

    # ── Imperecheri: aceeași formă ca feliile de document ───────────────────
    # Corpul e deliberat MINIM: gate-ul de creare e pe TIP și vine ÎNAINTEA
    # Apply-ului, deci un corp care n-ar trece domeniul tot 403 trebuie să dea.
    $corpImperechere = @{ DocumentStingatorId = $idInexistent; DocumentId = $idInexistent; Suma = 1 }
    Proba -Cerere 'creare împerechere' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/imperecheri' -Corp $corpImperechere -Contine 'crea' | Out-Null
    Proba -Cerere 'creare împerechere' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/imperecheri' -Corp $corpImperechere -Contine 'crea' | Out-Null
    Proba -Cerere 'creare împerechere' -User 'Configurator' -Asteptat 403 -Metoda POST -Cale '/api/imperecheri' -Corp $corpImperechere -Contine 'crea' | Out-Null
    Proba -Cerere 'ștergere împerechere inexistentă' -User 'Admin' -Asteptat 404 -Metoda DELETE -Cale "/api/imperecheri/$idInexistent" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── F23: politicile, implicitele și auditul (F23-D10 + F23-D9) ─────────
    # Ce a schimbat măsurătoarea pasului 2 față de tabelul din contract, și de
    # ce probele de mai jos arată altfel decât acolo:
    #   * un PATCH OData care nu SCHIMBĂ nimic nu ajunge nici la gardian, nici
    #     la plasa de permisiuni (fără modificare, EF nu pune obiectul în
    #     `ModifiedObjects`) — un `Cititor` primește atunci 204, nu 403. Toate
    #     PATCH-urile de aici trimit deci o valoare DIFERITĂ de cea din bază;
    #     altfel proba ar fi vacuă, defectul m4 al feliei 22.
    #   * `DinSeed = true` se refuză doar pe rând NOU; pe unul EXISTENT e o
    #     stingere tăcută (204, rândul rămâne manual) — deci rândul „PATCH cu
    #     DinSeed ⇒ 422" din F23-D10 se scrie numai ca POST.
    #   * `GET api/politici/verificare` ca `User` e 403 („citi"), nu 200 gol:
    #     raportul e un VERDICT calculat pe ușa non-secured, iar un verdict
    #     filtrat n-ar fi gol, ar fi FALS (F23-D8 amendat, familia 73g/80e).
    #
    # FĂRĂ URME — și ce NU se poate proba din cauza asta. Rândurile de politică
    # atinse aici sunt CREATE de script și șterse la final; niciun rând seed-uit
    # nu se modifică. Consecința, declarată: stingerea timbrului `DinSeed` la
    # editare (F23-D4) nu se poate măsura pe HTTP fără urme — ar cere un PATCH
    # pe un rând `DinSeed = true`, iar OData nu poate scrie flagul înapoi
    # (scrierea lui pe rând existent e ignorată tăcut, pe rând nou e refuzată).
    # Proba acelei reguli rămâne cea din ModelCheck (F23-V4). Rândurile de
    # AUDIT produse de probe RĂMÂN în bază: un jurnal nu se șterge.

    # Descoperirea, ca peste tot: prin API, pe coduri, nimic hardcodat.
    $tipFcl = Get-PrimaEntitate 'TipDocument' "Cod eq 'FCL'"
    $tipFct = Get-PrimaEntitate 'TipDocument' "Cod eq 'FCT'"
    $tvaN21 = Get-PrimaEntitate 'TipTva' "Cod eq 'N21'"
    $tvaN19 = Get-PrimaEntitate 'TipTva' "Cod eq 'N19'"
    $implicitSeed = Get-PrimaEntitate 'PoliticaTvaImplicit' "TipDocumentId eq $($tipFcl.ID) and ClasaFiscala eq 'Ue'"
    Write-Host "  FCL: $($tipFcl.ID)  FCT: $($tipFct.ID)  N21: $($tvaN21.ID)  N19: $($tvaN19.ID)" -ForegroundColor DarkGray

    # Raportul de profil ÎNAINTE de rândurile de probă: pe o bază fără divergențe
    # e `[]`. Dacă aici iese ceva, e o constatare REALĂ a bazei — se raportează,
    # nu se normalizează (F23-D11).
    Proba -Cerere 'verificare profil' -User 'Admin' -Asteptat 200 -Metoda GET -Cale '/api/politici/verificare' -Contine '[]' -Nota 'bază fără divergențe' | Out-Null
    Proba -Cerere 'verificare profil' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale '/api/politici/verificare' -Contine '[]' | Out-Null
    Proba -Cerere 'verificare profil' -User 'Configurator' -Asteptat 200 -Metoda GET -Cale '/api/politici/verificare' -Contine '[]' -Nota 'cine întreține profilul îi vede și verdictul' | Out-Null
    # `User` n-are Read pe tipurile din care se compune verdictul ⇒ 403 pe TIP,
    # nu 200 gol: un raport calculat non-secured cere dreptul de citire pe tot ce
    # citește (F23-D8, perechea lui 80e).
    Proba -Cerere 'verificare profil' -User 'User' -Asteptat 403 -Metoda GET -Cale '/api/politici/verificare' -Contine 'citi' -Nota 'F23-D8: verdict, nu listă' | Out-Null

    # Partenerul UE de probă (regimul vine de la el) și rândul de politică pe
    # care se fac PATCH-urile. `Cod` poartă un timbru de timp: o a doua rulare
    # nu se ciocnește de rândul șters logic al primei.
    $corpPartenerUe = @{
        Cod            = "PROBA-F23-$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())"
        Denumire       = 'Probă F23 (refuzuri)'
        Tara           = 'DE'
        TipPersoana    = 'Juridica'
        InregistratTva = $false
    }
    $creareUe = Proba -Cerere 'creare partener UE' -User 'Admin' -Asteptat 201 -Metoda POST -Cale '/api/odata/Partener' -Corp $corpPartenerUe -Nota 'subiectul probelor de implicite'
    if ($creareUe.Verdict -ne 'PASS') { throw "Nu s-a putut crea partenerul de probă: $($creareUe.CorpIntreg)" }
    $idPartenerUe = ($creareUe.CorpIntreg | ConvertFrom-Json).ID
    $curatenie.Add({
            $sters = Invoke-Cerere -Metoda DELETE -Cale "/api/odata/Partener($idPartenerUe)" -Token $tokenAdmin
            Write-Host "curățenie: DELETE /api/odata/Partener($idPartenerUe) → $($sters.Status)" -ForegroundColor DarkGray
        }.GetNewClosure())

    # FCT × ExtraUe (fără dată) e cheie de SEED din 83g (→ IMP): rândul de probă
    # stă pe o dată proprie, ca ștergerea lui logică de la final să nu ocupe
    # cheia seed-ului (83j — rândul șters nu se recreează).
    $corpImplicitProba = @{ TipDocumentId = $tipFct.ID; ClasaFiscala = 'ExtraUe'; ValabilDeLa = '2029-01-01'; TipTvaId = $tvaN21.ID }
    $creareImplicit = Proba -Cerere 'creare PoliticaTvaImplicit (FCT × ExtraUe)' -User 'Admin' -Asteptat 201 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpImplicitProba -Contine '"DinSeed":false' -Nota 'rândul de lucru; DinSeed nu se fabrică'
    if ($creareImplicit.Verdict -ne 'PASS') { throw "Nu s-a putut crea rândul de politică de probă: $($creareImplicit.CorpIntreg)" }
    $idImplicitProba = ($creareImplicit.CorpIntreg | ConvertFrom-Json).ID
    $curatenie.Add({
            $sters = Invoke-Cerere -Metoda DELETE -Cale "/api/odata/PoliticaTvaImplicit($idImplicitProba)" -Token $tokenAdmin
            Write-Host "curățenie: DELETE /api/odata/PoliticaTvaImplicit($idImplicitProba) → $($sters.Status)" -ForegroundColor DarkGray
        }.GetNewClosure())

    # PATCH pe rândul de probă: aceeași matrice ca pe documente (403 vizibil /
    # 404 invizibil), pe o politică. ORDINEA contează — `Cititor` și `User`
    # merg ÎNAINTEA lui `Admin`, ca valoarea trimisă să fie încă diferită de cea
    # din bază; după PATCH-ul lui Admin aceeași cerere ar fi un no-op și ar da
    # 204 pentru oricine (vezi antetul blocului).
    $patchImplicit = @{ ValabilDeLa = '2030-01-01' }
    Proba -Cerere 'modificare politică' -User 'Cititor' -Asteptat 403 -Metoda PATCH -Cale "/api/odata/PoliticaTvaImplicit($idImplicitProba)" -Corp $patchImplicit -Contine 'modifica' | Out-Null
    Proba -Cerere 'modificare politică' -User 'User' -Asteptat 404 -Metoda PATCH -Cale "/api/odata/PoliticaTvaImplicit($idImplicitProba)" -Corp $patchImplicit -Contine 'nu există sau nu e vizibil' | Out-Null
    Proba -Cerere 'modificare politică' -User 'Admin' -Asteptat 204 -Metoda PATCH -Cale "/api/odata/PoliticaTvaImplicit($idImplicitProba)" -Corp $patchImplicit -FaraJson -Nota 'schimbare REALĂ' | Out-Null

    # ── Al patrulea oracol: `Configurator` SCRIE politicile (83h) ───────────
    # Rândul e AL LUI, pe o cheie liberă (FCT × ExtraUe × 2032), și moare aici —
    # raportul de profil de la finalul blocului trebuie să rămână gol.
    $corpImplicitConfig = @{ TipDocumentId = $tipFct.ID; ClasaFiscala = 'ExtraUe'; ValabilDeLa = '2032-01-01'; TipTvaId = $tvaN21.ID }
    $creareConfig = Proba -Cerere 'creare PoliticaTvaImplicit' -User 'Configurator' -Asteptat 201 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpImplicitConfig -Contine '"DinSeed":false' -Nota 'rândul propriu'
    if ($creareConfig.Verdict -ne 'PASS') { throw "Configurator n-a putut crea rândul lui de politică: $($creareConfig.CorpIntreg)" }
    $idImplicitConfig = ($creareConfig.CorpIntreg | ConvertFrom-Json).ID
    $curatenie.Add({
            $sters = Invoke-Cerere -Metoda DELETE -Cale "/api/odata/PoliticaTvaImplicit($idImplicitConfig)" -Token $tokenAdmin
            Write-Host "curățenie: DELETE /api/odata/PoliticaTvaImplicit($idImplicitConfig) → $($sters.Status)" -ForegroundColor DarkGray
        }.GetNewClosure())
    Proba -Cerere 'modificare politică' -User 'Configurator' -Asteptat 204 -Metoda PATCH -Cale "/api/odata/PoliticaTvaImplicit($idImplicitConfig)" -Corp @{ ValabilDeLa = '2033-01-01' } -FaraJson -Nota 'schimbare REALĂ' | Out-Null
    $stergereConfig = Proba -Cerere 'ștergere politică' -User 'Configurator' -Asteptat 200 -Metoda DELETE -Cale "/api/odata/PoliticaTvaImplicit($idImplicitConfig)" -FaraJson -Nota 'rândul propriu, fără urme'
    if ($stergereConfig.Verdict -eq 'PASS') { $curatenie.RemoveAt($curatenie.Count - 1) }

    # `Societate` (73a) e CRUD pe OData, dar antetul raportorului NU e politică
    # de profil: `Configurator` n-are Write pe el. PATCH-ul poartă o valoare
    # diferită de cea din bază, altfel n-ar ajunge nici la plasa de permisiuni.
    $societate = Get-PrimaEntitate 'Societate'
    Proba -Cerere 'modificare Societate' -User 'Configurator' -Asteptat 403 -Metoda PATCH -Cale "/api/odata/Societate($($societate.ID))" -Corp @{ Localitate = 'Probă F24' } -Contine 'modifica' | Out-Null

    # Gardianul pe politici (F23-D5): trei refuzuri de DOMENIU pe care le ating
    # doar cei cu DREPT de scriere (`Admin` și `Configurator`) — pentru
    # `Cititor` și `User` permisiunea răspunde ÎNAINTEA gardianului (80c), deci
    # 403 „crea", nu 422. Perechea e proba că domeniul nu se uită la rol.
    $corpTvaInactiv = @{ TipDocumentId = $tipFct.ID; ClasaFiscala = 'NeinregistratRo'; TipTvaId = $tvaN19.ID }
    Proba -Cerere 'creare politică spre TVA inactiv' -User 'Admin' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpTvaInactiv -Contine 'inactiv' -Nota 'un implicit spre inactiv ar fi sărit tăcut' | Out-Null
    Proba -Cerere 'creare politică spre TVA inactiv' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpTvaInactiv -Contine 'crea' -Nota '80c: dreptul înaintea domeniului' | Out-Null
    Proba -Cerere 'creare politică spre TVA inactiv' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpTvaInactiv -Contine 'crea' | Out-Null
    # `Configurator` ARE dreptul, deci ajunge la gardian: același 422 ca `Admin`.
    Proba -Cerere 'creare politică spre TVA inactiv' -User 'Configurator' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpTvaInactiv -Contine 'inactiv' -Nota 'domeniul nu se uită la rol' | Out-Null
    # Dublu pe cheia unui rând SEED-uit: mesajul e al GARDIANULUI (spune ce cheie
    # s-a repetat), nu textul constraint-ului — indexul rămâne plasa (60a).
    $corpDublu = @{ TipDocumentId = $tipFcl.ID; ClasaFiscala = 'Ue'; TipTvaId = $implicitSeed.TipTvaId }
    Proba -Cerere 'creare politică DUBLĂ pe cheie' -User 'Admin' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpDublu -Contine 'deja' -Nota 'mesajul gardianului, nu 23505' | Out-Null
    Proba -Cerere 'creare politică DUBLĂ pe cheie' -User 'Configurator' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpDublu -Contine 'deja' | Out-Null
    # Proveniența nu se declară de client (F23-D4): pe rând NOU, `DinSeed` = refuz.
    $corpProvenienta = @{ TipDocumentId = $tipFct.ID; ClasaFiscala = 'ExtraUe'; ValabilDeLa = '2031-01-01'; TipTvaId = $tvaN21.ID; DinSeed = $true }
    Proba -Cerere 'creare politică cu DinSeed=true' -User 'Admin' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpProvenienta -Contine 'proveniența' | Out-Null
    Proba -Cerere 'creare politică cu DinSeed=true' -User 'Configurator' -Asteptat 422 -Metoda POST -Cale '/api/odata/PoliticaTvaImplicit' -Corp $corpProvenienta -Contine 'proveniența' -Nota 'nici cine întreține profilul nu declară timbrul' | Out-Null

    # Dezactivarea unui `TipTva` REFERIT ca implicit: refuzul vine cu LISTA
    # referințelor (ancorele + politica de probă de mai sus).
    Proba -Cerere 'dezactivare TipTva referit' -User 'Admin' -Asteptat 422 -Metoda PATCH -Cale "/api/odata/TipTva($($tvaN21.ID))" -Corp @{ Activ = $false } -Contine 'ancora' -Nota 'lista referințelor' | Out-Null
    Proba -Cerere 'dezactivare TipTva referit' -User 'Configurator' -Asteptat 422 -Metoda PATCH -Cale "/api/odata/TipTva($($tvaN21.ID))" -Corp @{ Activ = $false } -Contine 'ancora' | Out-Null

    # `TipDocument` = ANCORA (decizia 20): `Cod` e identitate, rândurile nu se
    # creează și nu se șterg — dar ce e refuz de DOMENIU pentru Admin rămâne
    # refuz de PERMISIUNE pentru ceilalți, cu verbul potrivit și în ordinea 80a.
    Proba -Cerere 'modificare Cod pe ancoră' -User 'Admin' -Asteptat 422 -Metoda PATCH -Cale "/api/odata/TipDocument($($tipFct.ID))" -Corp @{ Cod = 'XXX' } -Contine 'identitatea' | Out-Null
    Proba -Cerere 'modificare Cod pe ancoră' -User 'Cititor' -Asteptat 403 -Metoda PATCH -Cale "/api/odata/TipDocument($($tipFct.ID))" -Corp @{ Cod = 'XXX' } -Contine 'modifica' | Out-Null
    Proba -Cerere 'modificare Cod pe ancoră' -User 'User' -Asteptat 404 -Metoda PATCH -Cale "/api/odata/TipDocument($($tipFct.ID))" -Corp @{ Cod = 'XXX' } -Contine 'nu există sau nu e vizibil' | Out-Null
    # Rolul ARE Write pe `TipDocument` (e în listă), deci refuzul e al
    # GARDIANULUI (81e), nu al permisiunii: 422 „identitatea", nu 403.
    Proba -Cerere 'modificare Cod pe ancoră' -User 'Configurator' -Asteptat 422 -Metoda PATCH -Cale "/api/odata/TipDocument($($tipFct.ID))" -Corp @{ Cod = 'XXX' } -Contine 'identitatea' | Out-Null
    $corpTipNou = @{ Cod = 'ZZZ'; Denumire = 'Probă'; ClrType = 'nimic' }
    Proba -Cerere 'creare tip de document' -User 'Admin' -Asteptat 422 -Metoda POST -Cale '/api/odata/TipDocument' -Corp $corpTipNou -Contine 'nu se creează' | Out-Null
    Proba -Cerere 'creare tip de document' -User 'Cititor' -Asteptat 403 -Metoda POST -Cale '/api/odata/TipDocument' -Corp $corpTipNou -Contine 'crea' | Out-Null
    Proba -Cerere 'creare tip de document' -User 'User' -Asteptat 403 -Metoda POST -Cale '/api/odata/TipDocument' -Corp $corpTipNou -Contine 'crea' | Out-Null
    Proba -Cerere 'ștergere ancoră' -User 'Admin' -Asteptat 422 -Metoda DELETE -Cale "/api/odata/TipDocument($($tipFct.ID))" -Contine 'ancora' -Nota 'CRUD deschis ≠ ancoră ștergibilă' | Out-Null
    Proba -Cerere 'ștergere ancoră' -User 'Cititor' -Asteptat 403 -Metoda DELETE -Cale "/api/odata/TipDocument($($tipFct.ID))" -Contine 'șterge' | Out-Null
    Proba -Cerere 'ștergere ancoră' -User 'User' -Asteptat 404 -Metoda DELETE -Cale "/api/odata/TipDocument($($tipFct.ID))" -Contine 'nu există sau nu e vizibil' | Out-Null

    # ── Implicitele (F23-D6): afordanță, nu validare ────────────────────────
    # Ruta n-are gate pe TIP — cine poate culege o linie poate întreba implicitul
    # ei. Ce vede fiecare e ce-i arată ușa SECURIZATĂ: `Admin`/`Cititor` văd
    # partenerul UE și politica lui, `User` nu vede nomenclatoarele, deci
    # primește un răspuns GOL și adevărat (`Niciuna`), nu unul generic și fals.
    Proba -Cerere 'implicit FCL × partener UE' -User 'Admin' -Asteptat 200 -Metoda GET -Cale "/api/implicite/tip-tva?tipDocument=FCL&partenerId=$idPartenerUe" -Contine 'SDD', 'Politica' | Out-Null
    Proba -Cerere 'implicit FCL × partener UE' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale "/api/implicite/tip-tva?tipDocument=FCL&partenerId=$idPartenerUe" -Contine 'SDD' | Out-Null
    Proba -Cerere 'implicit FCL × partener UE' -User 'Configurator' -Asteptat 200 -Metoda GET -Cale "/api/implicite/tip-tva?tipDocument=FCL&partenerId=$idPartenerUe" -Contine 'SDD' | Out-Null
    Proba -Cerere 'implicit FCL × partener UE' -User 'User' -Asteptat 200 -Metoda GET -Cale "/api/implicite/tip-tva?tipDocument=FCL&partenerId=$idPartenerUe" -Contine 'Niciuna' -Nota 'ce nu vezi nu-ți poate fi propus' | Out-Null
    # Fără oracol de existență (80a): partener LIPSĂ și partener INEXISTENT dau
    # același răspuns, până la ultimul octet — altfel `Motiv` ar fi un canal prin
    # care se află ce parteneri există.
    $implFaraPartener = Proba -Cerere 'implicit FCL fără partener' -User 'Admin' -Asteptat 200 -Metoda GET -Cale '/api/implicite/tip-tva?tipDocument=FCL' -Contine 'Ancora', 'Fără partener vizibil'
    $implPartenerFals = Proba -Cerere 'implicit FCL × partener inexistent' -User 'Admin' -Asteptat 200 -Metoda GET -Cale "/api/implicite/tip-tva?tipDocument=FCL&partenerId=$idInexistent" -Contine 'Ancora', 'Fără partener vizibil'
    $script:Numar++
    $verdictOracol = 'PASS'
    $motivOracol = ''
    if ($implFaraPartener.CorpIntreg -cne $implPartenerFals.CorpIntreg) {
        $verdictOracol = 'FAIL'
        $motivOracol = 'corpurile DIFERĂ ⇒ oracol de existență pe partener'
    }
    $script:Rezultate.Add([pscustomobject]@{
            Nr         = $script:Numar
            Cerere     = '(comparație) implicit fără partener == implicit cu partener inexistent'
            User       = 'Admin'
            Asteptat   = 'corpuri IDENTICE (80a: fără oracol de existență)'
            Primit     = $(if ($verdictOracol -eq 'PASS') { 'identice' } else { 'diferite' })
            Corp       = Scurt $implFaraPartener.Corp
            Ms         = 0
            Verdict    = $verdictOracol
            Motive     = $motivOracol
            CorpIntreg = $implPartenerFals.CorpIntreg
        })
    # Cod necunoscut = 400, ACELAȘI pentru toți: maparea cod → ancoră se face pe
    # ușa non-secured, tocmai ca 400 să însemne un singur lucru („codul nu există
    # în ancoră"), nu „nu ți-e vizibil TipDocument".
    Proba -Cerere 'implicit pe cod necunoscut' -User 'Admin' -Asteptat 400 -Metoda GET -Cale '/api/implicite/tip-tva?tipDocument=XYZ' -Contine 'necunoscut' | Out-Null
    Proba -Cerere 'implicit pe cod necunoscut' -User 'User' -Asteptat 400 -Metoda GET -Cale '/api/implicite/tip-tva?tipDocument=XYZ' -Contine 'necunoscut' -Nota 'același text ca Admin' | Out-Null

    # ── Auditul pe OData (F23-D9) ──────────────────────────────────────────
    # `UserName`/`ObjectType` sunt `[NotMapped]` pe `AuditDataItemPersistent` și
    # NU intră în EDM; utilizatorul se ia prin `$expand=UserObject` ⇒
    # `DefaultString`. Filtrul merge pe referința slabă: numele CLR COMPLET +
    # `Key` = Guid-ul ca string. Rândurile de mai jos sunt cele produse de POST-ul
    # și PATCH-ul de la începutul blocului — deci proba e a scrierilor REALE.
    $tipClrImplicit = 'Atlas.Conta.BackOffice.Module.BusinessObjects.PoliticaTvaImplicit'
    $filtruAudit = [uri]::EscapeDataString("AuditedObject/TypeName eq '$tipClrImplicit' and AuditedObject/Key eq '$idImplicitProba'")
    $caleAudit = '/api/odata/AuditDataItemPersistent?$filter=' + $filtruAudit + '&$expand=UserObject&$orderby=' + [uri]::EscapeDataString('ModifiedOn desc')
    Proba -Cerere 'audit pe rândul de politică' -User 'Admin' -Asteptat 200 -Metoda GET -Cale $caleAudit -Contine '"OperationType":"ObjectChanged"', '"PropertyName":"ValabilDeLa"', '"DefaultString":"Admin"' -Nota 'cine, când, ce câmp' | Out-Null
    Proba -Cerere 'audit pe rândul de politică' -User 'Cititor' -Asteptat 200 -Metoda GET -Cale $caleAudit -Contine '"OperationType":"ObjectChanged"' -Nota 'Read pe tot' | Out-Null
    Proba -Cerere 'audit pe rândul de politică' -User 'Configurator' -Asteptat 200 -Metoda GET -Cale $caleAudit -Contine '"OperationType":"ObjectChanged"' | Out-Null
    # Rolul `Default` vede DOAR rândurile de audit proprii (Updater.cs) — deci un
    # jurnal expus pe OData nu devine o fereastră spre activitatea altora.
    Proba -Cerere 'audit pe rândul de politică' -User 'User' -Asteptat 200 -Metoda GET -Cale $caleAudit -Contine '"value":[]' -Nota 'doar rândurile proprii' | Out-Null
    # Jurnalul e ReadOnly pe OData: scrierea nici nu are rută (405).
    Proba -Cerere 'scriere în jurnalul de audit' -User 'Admin' -Asteptat 405 -Metoda POST -Cale '/api/odata/AuditDataItemPersistent' -Corp @{ OperationType = 'x' } -FaraJson -Nota 'ReadOnly' | Out-Null

    # Curățenia rândurilor de probă, ca PROBE (ștergerea e tot o operație a
    # matricei), urmată de raportul de profil: dacă a rămas vreo urmă, el o vede.
    Proba -Cerere 'ștergere politică de probă' -User 'Admin' -Asteptat 200 -Metoda DELETE -Cale "/api/odata/PoliticaTvaImplicit($idImplicitProba)" -FaraJson -Nota 'curățenie' | Out-Null
    Proba -Cerere 'ștergere partener de probă' -User 'Admin' -Asteptat 200 -Metoda DELETE -Cale "/api/odata/Partener($idPartenerUe)" -FaraJson -Nota 'curățenie' | Out-Null
    Proba -Cerere 'verificare profil (după curățenie)' -User 'Admin' -Asteptat 200 -Metoda GET -Cale '/api/politici/verificare' -Contine '[]' -Nota 'fără urme' | Out-Null

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
