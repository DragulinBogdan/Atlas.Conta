# Pas 3 (transferul), explorarea 1: inventarul implementării curente, pe responsabilități

Scop: un inventar EXACT și verificabil (`fișier:linie`) a ce există azi în
`nou/Atlas.Conta.BackOffice/` ca să se poată decide ce se transferă în nucleul
nou (cubul de postări), ce se rescrie și ce dispare. Nu propui, nu judeci,
nu compari cu cubul — doar clasifici după criteriile de mai jos. Raportul tău
e SINGURUL context pe care coordonatorul îl primește despre cod.

Citește întâi (scurt): `docs/nucleu/nucleu-cub-design.md` §2–§7 (vocabularul:
postare, tranzacție, contract, conservare / rezolvarea coordonatelor /
admisibilitate, unitate, storno, snapshot) și `docs/invarianti.md` I–IV.
NU citi `docs/nucleu/nucleu-fizica.md` și `nucleu-coordonate-rapoarte.md`
(sunt ale altor pași). CodeGraph există (`.codegraph/`): folosește
`codegraph explore` înainte de grep.

Scrie în `run-nucleu/transfer/01-inventar-implementare.md` (Markdown, română,
slim). Secțiuni obligatorii:

## A. Motorul (`Module/Motor/*.cs`, 21 fișiere)
Un tabel cu o linie per SERVICIU/clasă (nu per fișier dacă fișierul are mai
multe): `serviciu | responsabilitate (o frază) | linii | citește (tabele) |
scrie (tabele) | IObjectSpace: da/nu | fel`. `fel` e UNUL din:
- `regulă` — calcul care ar putea fi pur (intrări plate → ieșiri), chiar dacă
  azi e amestecat cu I/O; spune care metode sunt calculul;
- `mecanism` — scrie/citește forma registrelor de azi (RegistruContabil/
  Stoc/Tva/Imobilizari, SolduriPerioada*, PartideDeschise, Imperecheri);
- `gardian` — admisibilitate (perioadă, editare, profil);
- `adaptor` — entitate → fapt plat, cheie → rând, tranzacție.
Pentru `MotorOperare` detaliază: fazele lui `Opereaza` (calculează → validează
→ materializează) cu liniile, ce scrie în fiecare registru, cum face stornoul
(`:669-681` e „în roșu"), conexul/secundarul, anularea. Pentru
`ImperechereService`, `DescarcareService`, `StocService`,
`LoturiCulegereService`, `AmortizareService`, `PerioadaService`,
`SolduriService`, `InchidereTvaService`, `CorectieService`: care e REGULA
(intrări → ieșiri) și care e mecanismul din jur.

## B. Hook-urile documentului
Lista COMPLETĂ a metodelor virtuale/abstracte de pe `Document`,
`DocumentDetaliu` și interfețele consumate de motor (`IVerificabilLaCommit`,
`IDocumentCuRegistruPropriu`, orice alta): `hook | semnătură | cine îl
cheamă (fișier:linie) | câte override-uri | ce decid override-urile (2–3
exemple concrete de frunze cu fișier:linie)`. Marchează care hook-uri
primesc `IObjectSpace`.

## C. Documentele tipate (frunzele)
Un tabel: `tip (ClrType) | fișier | câmpuri proprii de culegere (nume) |
ce postează azi (contabil / stoc / TVA / imobilizări / terți) | linii de
conex generate (spre ce tip) | hook-uri suprascrise`. Toate cele 20 de tipuri
concrete. Notează explicit: FCT (FacturaIntrare) — ce postează linia ei și
ce postează NIR-ul conex; DVI; documentele de trezorerie (stingere); AMO/PIF/CAS.

## D. Politicile (date)
Din `BusinessObjects/Politici*.cs` (sau unde sunt): un tabel `tabelă de
politică | cheia | ce parametrizează | cine o consumă (fișier:linie) |
versionată pe perioadă? | rânduri pe seed privat (dacă e ușor de numărat din
seed)`. Include: contare, stoc (semn/filtru/TipStoc), numerotare, conex,
scadență, TVA, validări, amortizare, deductibilitate, închidere,
`PoliticaMiscareSaft`, `MapareD300`, rotunjire/profil.

## E. Gardienii
`GardianEditare` (1.312 linii): lista REGULILOR lui (literele (a)–(o) sau cum
sunt numite), o frază fiecare, ce citesc (tracker / bază), `fișier:linie`.
`GardianPerioada`, `VerificareProfilService`, `VerificaTipDocument`: o linie
fiecare.

## F. Registrele și proiecțiile
Cele patru registre + snapshot-urile + `PartideDeschise` + `Imperecheri`:
coloanele (nume, tip) din `Registre.cs`; cine SCRIE fiecare (fișier:linie);
câte proiecții/rapoarte CITESC fiecare (numără fișierele din `Proiectii/`,
`Saft/`, `Api/` care le referă; nu le inventaria pe fiecare — pasul 1 a
făcut-o). Ce citește din registre ALTCEVA decât rapoartele (motorul,
gardienii, Import1C, ModelCheck).

## G. ModelCheck
`nou/tools/ModelCheck`: cum e organizat (fișiere, familii de probe), câte
probe per familie pe profilul privat (din ultima rulare sau din cod —
1450 în total), ce pornește (host, baze, seed), și pentru fiecare familie:
ce anume probează — modelul EF / motorul (cifre de postare) / gardienii /
politicile / API-ul / Import1C. Marchează familiile care probează FORMA
registrelor (ar cădea la schimbarea formei) contra celor care probează
REGULA (Σ, solduri, refuzuri — rămân valabile pe orice formă).

## H. Import1C
`nou/tools/Import1C`: bucla (Bucla/Reluare), cum operează documentele (prin
`MotorOperare.Opereaza`? semnătura), ce compară raportul de reconciliere
(`reconciliere-*.txt`: pe ce agregate — registre? solduri? per lună?) și
de unde citește cifrele „așteptate” (1C) și „obținute” (registrele de azi —
care coloane). Ce ar avea nevoie de la un motor nou ca să producă ACELAȘI
raport. Dimensiunea lui (fișiere, linii).

## I. Ușile (API, XAF)
Doar cât să se vadă cuplajul cu motorul: cine cheamă `Opereaza`/`Storneaza`/
`Anuleaza`/`valideaza` (lista apelanților direcți cu fișier:linie), ce DTO
primește clientul din operare (planul? refuzuri?), și unde stă „Explică”.

Reguli:
- Citește codul, nu ghici. Orice afirmație cu `fișier:linie`.
- Fără propuneri, fără „ar trebui”, fără comparație cu cubul.
- Nu modifici nimic, nu rulezi build-uri, nu atingi baze.
- Regulă de oprire: când toate cele 9 secțiuni au tabelele cerute, te
  oprești. Dacă o secțiune nu se poate completa din cod (ex. numărul de probe
  per familie), spui exact ce lipsește și de ce, nu aproximezi tăcut.
- Răspunsul final către coordonator: max 15 linii — ce ai acoperit, ce
  n-ai găsit, cele 3 lucruri pe care le-ai vrea semnalate (locuri unde regula
  și mecanismul sunt de nedespărțit; hook-uri care cer instanța; ce citește
  motorul din registre).
