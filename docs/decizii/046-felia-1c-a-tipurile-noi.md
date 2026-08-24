# Decizia 46 — Felia 1C-a — tipurile noi de model

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §46
- **Docs**: docs/import/faza-1c-design.md

---

**Felia 1C-a — tipurile noi de model — executată; validată e2e în
ModelCheck (privat 167 verificări, bugetar 212, idempotente) + review
advers dedicat cu cele 6 fix-uri de fond aplicate. Tranșările:**
(a) **Spike-ul storno REZOLVAT** (fixat în design §7): reprezentarea =
valori NEGATIVE pe corespondența ORIGINALĂ (minus pe aceeași latură) —
e deja convenția `Storneaza` (25d) și reprezentarea 1C; liniile se culeg
pozitive, `PregatesteOperare` semnează (mecanismul LDI 28a); stocul și
pasul TVA din motor NEMODIFICATE (semnul liniei face treaba); unica
extensie: `RegulaContare.PastreazaSemn` (valoarea se postează cu semnul
ei, fără normalizarea SemnFiltru). Rândurile returului NU poartă flag-ul
`Storno` — ăla rămâne al meta-operației (stornarea unui retur = rânduri
pozitive cu Storno=true, verificat e2e).
(b) **NTC (NotaContabila, al 12-lea derivat)** — importul de note din
decizia 9 + tip de culegere manuală: `NotaContabilaDetaliu` cu
`ILinieCuPostareExplicita` (conturi obligatorii, repartitori opționali,
valoare nenulă SEMNATĂ — notele storno de import trec ca atare), laturi
interne, Tip tehnic TRZ pe linii; fără reguli de stoc/contare, singura
politică = numerotarea NTC- (ambele profiluri). Motorul postează explicit
FĂRĂ regulă doar pe tipurile cu marker-ul `IDocumentCuPostareExplicita`
(fix review: altfel o linie străină cu conturi explicite injecta note pe
BTR/NIR sau dublă postare pe lanțul conex FCT→NIR).
(c) **ITV (`InchidereTva : NotaContabila` — moștenire concretă TPT; iese
din amânarea 36f)**: `InchidereTvaService.Genereaza(os, an, luna,
unitateId)` — precedentul DescarcareService: solduri CUMULATE la ultima
zi a lunii (prind și deschiderea; lunile închise lasă 0), draft cu max 3
linii (4427=4426 pe minim, excedent → 4423 / 4424), conturile exclusiv
din `PoliticaInchidereTva` (tabelă nouă; seed privat 4426/4427/4423/4424
+ numerotare ITV-; bugetar fără rând = tip inert), fără commit (al
apelantului). Gardieni: idempotență pe închiderea vie a lunii (Stornat
exclus — regenerarea merge), refuz zgomotos la generarea NE-cronologică
(fix review: închiderea „în urmă" dubla 4423/4424) și gardian ANTI-STALE
la operare (recalculează soldurile la Data prin helper-ul partajat
`Solduri` și cere potrivirea exactă cu liniile — fix review: documente
de TVA intrate între generare și operare). Nu închide perioada, nu
atinge 121 (design §6).
(d) **ASM (Asamblare — kitting n→m; BPR rămâne rezervat, 19)**: mecanica
LDI 28a — `DirectieAsamblare` (Consum/Produs, fără default valid) în semn
la operare; consumul descarcă loturi existente la preț de lot, produsul
creează lot la culegere cu `PretEvaluare` explicit; invariant
|Σproduse − Σconsumuri| ≤ 0.005; un set de reguli stoc +1 predator
(generic→Magazie, MF→Marfuri), ZERO reguli de contare (371=371 la
sintetic = zgomot, 23c); laturi ambele Gestiune; dezasamblarea = același
tip. Consumul unui lot produs de ACELAȘI document = refuz (fix review:
prețul nefinalizat lăsa valoare orfană în registrul de stoc).
(e) **RLF/RDC (retururile — cerință de produs)**: RLF = Gestiune→Partener
pe lotul original: 3xx=401 −V (regulă generică Natura=Stoc cu
PastreazaSemn, debit TipMaterial / credit RepartitorPrimitor fallback
401) + 4426=401 −TVA (PoliticaTva Deductibil); stoc −q prin Semn=+1 ×
linia negativă. RDC = UN document (nu pereche FCL+DSC — returul n-are
decuplarea temporală), linii pe DOUĂ roluri distinse prin LotId: venit
(Tip VEN, 4111=70x −V, 4111=4427 −TVA, cantitate pro-formă pozitivă) +
cost (lotul ORIGINAL, +q prin Semn=−1 pe primitor, 607=371 −cost derivat
cu `SeedContare6xxDin3xx(..., pastreazaSemn: true)` + excepțiile
profilului). `Total` devine VIRTUAL pe bază, override RDC = doar liniile
de venit, cu geamănul server-side `Document.LiniiCreanta` consumat de
`ImperechereService.Total` (fix review: divergența −121 vs −151 la
stingere). Capitalizat REFUZAT pe ambele (fix review: compundare la
re-operare / valoare peste costul lotului); linia de cost fără regulă
derivată = refuz (38c). Ambele folosesc detaliul de BAZĂ; TipTvaImplicit
N21 la culegere; bugetar inert (PastreazaSemn verificat inert la
default false).
(f) Semnalate, netranșate (la 1C-c/1C-d): imperecherea returului (Total
negativ; compensarea cu factura originală / rambursarea — se tranșează
unde apare Compensarea, maparea §4); toleranța ASM absolută per document
la importul cu multe linii rotunjite; disciplina de apelant ITV (commit
între apeluri `Genereaza`, storno-ul unei închideri la chiar data ei
dacă se vrea regenerarea lunii).
