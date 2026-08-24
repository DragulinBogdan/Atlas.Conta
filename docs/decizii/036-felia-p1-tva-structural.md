# Decizia 36 — Felia P1 — profil privat + TVA structural

- **Data**: 2026-07-23 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §36
- **Docs**: docs/privat/p1-tva-design.md

---

**Felia P1 — profil privat + TVA structural — executată** (designul fixat
`docs/privat/p1-tva-design.md`); validată e2e în ModelCheck pe AMBELE
profiluri. Tranșările de implementare:
(a) **`TipTva` (nomenclator: cotă × regim + conturile 4426/4427/4428 ca
date + mapări SAF-T/D394) și `TipTvaId?`+`ValoareTva` pe baza
`DocumentDetaliu`**; `CotaTva` ștearsă de pe cele trei derivate (migrația
`TvaStructural`). Calculul = helper comun `TvaService.CalculeazaValori`
(formula §3: Capitalizat → brut în Valoare; Normal/TI → net + ValoareTva;
Scutit/Neimpozabil/null → net), apelat din `PregatesteOperare` al
FCT/FCL/DEC; pe FCT `ValoareTva` nenulă culeasă NU se suprascrie (factura
furnizorului bate rotunjirea). `Document.Total` explicit BRUT
(Σ Valoare+ValoareTva) — `ImperechereService.Total` și plata autogenerată
sting brutul (liniile plății clonează Valoare+ValoareTva; TipTva rămâne
null pe plată).
(b) **Pasul TVA în motor** (faza „calculează" din `Opereaza`, înaintea
gardianului de dimensiuni): per linie cu ValoareTva≠0 și regim care
postează, condiționat de rândul `PoliticaTva` al tipului (Directie +
SursaContrapartida + fallback — tabelă nouă, simetrică PoliticaScadenta/
PoliticaValidare); TaxareInversa = 4426=4427 indiferent de direcție;
dimensiunile rândului TVA: linie → default polimorf header + Material din
lot, FĂRĂ override-uri de regulă. Rândurile TVA intră în aceeași listă de
note → `VerificaDimensiuniObligatorii`, materializarea și storno le
acoperă natural. Conexul clonează `TipTvaId` ca informație, NU ValoareTva
(TVA-ul se postează pe documentul sursă — NIR-ul duce netul; evaluarea
stocului la privat = net, lotul primește preț net).
(c) **`ContaSeeder` spart în nucleu + pachete de profil**: nucleul neutru
ține enum `ProfilContabil`, ancorele TipDocument, perioadele, repartitorii
minimali și MECANISMELE (ContDinSimbol/tăierea segmentelor,
SeedContImplicitTipMaterial, SeedContare6xxDin3xx cu parametru de
excepții, gardianul `VerificaProfil` — ancora planului: 891.01.00 bugetar
vs 4426 privat, profilul nu se amestecă pe aceeași bază); `ProfilBugetar`
= conținutul de azi mutat + TipTva Capitalizat (21/19-istoric/11/0, fără
conturi de TVA, fără PoliticaTva — zero schimbare de comportament);
`ProfilPrivat` = plan OMFP 1802 (CSV `plan-conturi-omfp.csv`, format
Account,ParentAccount,Denumire; Sumator = are copii; DimensiuniObligatorii
pornesc GOALE), Clasă/Tip pe simboluri OMFP (301/302/303/345/371/381 stoc,
6xx servicii, 704/706/707/708 venituri, TRZ/T tehnice), derivările proprii
(excepțiile 371→607, 345→711, 381→608; plus inventar = **7588**, nu 791),
conturi proprii CASA→5311 / BANCA→5121, fallback-uri 401/404/4111/542,
PoliticaValidare doar FCL⊘Stoc (fără clasificație bugetară). `Updater`
citește `ProfilContabil` din appsettings (default Bugetar, per bază — 35d).
(d) **Seed TipTva privat cu codurile SAF-T reale** (extrase din
RO_SAFT_SchemaDefCod 16.02.2026, direcționale livrare/achiziție):
N21 310344/301104, N11 310351/301105, N9 310310/301102 (tranzitoriu, expiră
31.07.2026 — marcat în denumire), TI21 310312/300906, NED21=Capitalizat
−/351104, SDD 310314, SFD 310326, NIM 310324; 4428 legat ca REZERVAT pe
rândurile Normal/TI. `CategorieD394` rămâne null — categoria e direcțională
la nivel de operațiune, se fixează la proiecția D394 (checklist 35c).
(e) **ModelCheck parametrizat pe profil**: implicit = suita bugetară pe
baza aplicației (verde integral; singura schimbare în scenarii: culegerea
CotaTva → TipTva, rândul istoric CAP19 păstrează valorile-ancoră);
`dotnet run privat` = bază DEDICATĂ `Atlas.Conta.ModelCheck.Privat` pe
care unealta o migrează și o seed-uiește singură (ContaSeeder direct,
aceeași cale ca updater-ul) + blocul e2e privat: FCT stoc+serviciu (NIR
net, rând 4426 și pentru linia de stoc FĂRĂ regulă principală, 401 brut,
imperecherea plății automate pe brut), FCL cu 4427, DEC cu 4426=542 pe
titular, taxare inversă, capitalizat, ValoareTva manuală păstrată, storno
cu rândurile TVA inverse. Idempotent la rulări repetate.
(f) Amânate, documentate (design §8): TVA la încasare (regim + 4428 +
transfer la imperechere), facturi nesosite (408/4428), regularizarea de
rotunjire per document×cotă (e-Factura), prorata/ajustări/D300/D394/SAF-T
ca proiecții peste registre, închiderea lunară de TVA (4423/4424).
