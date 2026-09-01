# 78. Căutarea fără diacritice pe proiecții — rescrierea predicatelor `DataSourceLoader` prin normalizatorul comun

- **Data**: 2026-09-01
- **Stare**: activă (perechea lui 77a: coloana generată rămâne a nomenclatoarelor pe OData; aici e ușa `DataSourceLoader` a proiecțiilor)
- **Docs**: `nou/.../Module/Api/CautareFiltru.cs`, `nou/.../Module/BusinessObjects/Comun/Cautare.cs`, `BackOfficeDbContext.AplicaFunctiaFaraDiacritice`; probele în `nou/tools/ModelCheck/Program.cs` (blocul „78")

## Context

77a a rezolvat căutarea fără diacritice pe **ușa OData a nomenclatoarelor**
(coloana generată STORED `Cautare` + rescrierea literalului în `beforeSend`).
A rămas însă a doua ușă de citire: **proiecțiile prin `DataSourceLoader`** —
FilterRow-ul și (nou, activat în aceeași zi pe toate grilele) căutarea din
`HeaderFilter` filtrează câmpuri ca `PredatorDenumire`/`PrimitorDenumire`,
care sunt **join-uri** (`Partener.Denumire` proiectat), nu coloane ale unui
tabel. Acolo `contains('ștefan')` rămânea sensibil la diacritice și caz —
exact gaura semnalată la 76-r6 pentru lookup-uri, dar pe listele de documente.

Instinctul inițial (coloană calculată persistată + contract) a fost cântărit
explicit: pe **nomenclatoare** el e deja decizia 77a și rămâne; pe
**documente/registre** o copie normalizată persistată a denumirii ar fi
denormalizare cu staleness la redenumirea partenerului, iar proiecțiile sunt
query-uri, nu tabele — nu au unde purta o coloană. Soluția unitară nu e „încă
o coloană", ci **un singur normalizator cu al patrulea consumator**.

## Decizia

**(a) Normalizarea devine funcție de query, tradusă pe exact fragmentul SQL al
coloanei generate.** `Cautare.FaraDiacritice(string)` — corp C# real
(null-propagant, spre deosebire de `Normalizeaza`, care întoarce `""`) — e
mapată de EF (`HasDbFunction` + `HasTranslation`, în
`AplicaFunctiaFaraDiacritice`) pe `translate(lower(x), De, La)`. Fragmentul
are **o singură ortografie**: `Cautare.FragmentSql(expresie)`, din care se
compun și `ExpresieSql` (coloana generată/migrația) și traducerea funcției —
două ortografii ar putea divergea doar împreună cu proba. `unaccent`/colația
ICU rămân respinse din motivele tranșate la 77a (al doilea normalizator,
STABLE, semantica egalității).

**(b) Un compilator custom global rescrie predicatele de string.**
`CautareFiltru.Inregistreaza()` (idempotent per proces; lista
`CustomFilterCompilers` a bibliotecii e statică) înregistrează un
`RegisterBinaryExpressionCompiler` care, DOAR pe
`contains`/`notcontains`/`startswith`/`endswith` cu accessor de tip `string`,
emite `FaraDiacritice(coalesce(camp, '')) op literalNormalizat` — literalul
normalizat în C#, pe partea constantei nu are ce căuta un apel SQL. Orice
nerezolvare (operație de comparație, câmp ne-string, accessor negăsit) întoarce
`null` = compilarea standard; rescrierea nu poate rupe filtrarea, cel mult n-o
atinge. **`=`/`<>` rămân exacte, deliberat**: lista de valori a
`HeaderFilter` trimite valoarea exactă din listă, iar o egalitate normalizată
ar topi valori distincte („Țeavă" == „Teava"). Zero schimbări per proiecție,
zero schimbări în client — WebApi îl înregistrează în `Startup`, ModelCheck în
proba proprie; host-ul Blazor nu folosește `DataSourceLoader`.

**(c) `coalesce(camp, '')` înaintea funcției** — sursele materializate în
memorie (fișa de cont: SQL brut → listă) execută expresia ca LINQ-to-Objects,
unde `null.Contains` ar arunca. În SQL, coalesce-ul e neutru pe
`contains`/`startswith`/`endswith` (nulul nu se potrivea nici înainte); pe
`notcontains` schimbă semantica: nulul **contează** ca „nu conține" (înainte:
`NOT(null)` = null = exclus). Asumat ca îmbunătățire, nu regresie — operatorul
care cere „nu conține X" vrea și rândurile fără valoare.

**(d) Probele.** ModelCheck (ambele profiluri, 0 FAIL): scenă cu partener
„Țestoasa Înțeleaptă" pe un FCT draft, încărcat prin `DataSourceLoader` peste
proiecția REALĂ a feliei (`FacturaIntrareApply.Lista`) — literal fără
diacritice găsește rândul cu diacritice și invers, inclusiv grafia veche cu
sedilă (ţ U+0163) pentru virgulița din bază (ț U+021B); `startswith`/
`endswith`/`notcontains`; cusătura de contract pe `ToQueryString` (SQL-ul emis
conține prefixul și sufixul lui `FragmentSql`); purjă fizică la final.
Pe calea reală (HTTP, baza Privat de import, 19.042 FCT):
`contains` „ROMÂNIA"/„românia"/„romania" ⇒ **același total (3.243)** la
56–97 ms; `startswith` „E.ON ENERGIE ROMÂNIA" ⇒ 13; „ilaş"/„ilaș" ⇒ 5, același
rând cu Ş-sedilă. Capcană de PROBARE, nu de produs: consola Windows (cp1252)
corupe literalii cu diacritice din `curl` — falsul „0 rânduri" inițial; proba
se trimite cu UTF-8 percent-encodat explicit (cum trimite browserul).
`has-pending-model-changes`: funcția mapată nu produce drift de migrație.

**(e) Performanța: nimic preventiv (59).** `contains` = `LIKE '%x%'`, ne-btree
oricum — rescrierea nu pierde niciun index existent; costul `translate` per
rând e sub zgomotul join-urilor proiecției (măsurat mai sus). Când o căutare
va avea cifră lentă: index **GIN `pg_trgm`** pe coloana `Cautare` (lookup-uri,
închide 77-r7) sau direct pe expresia `translate(lower(col), …)` — expresia e
IMMUTABLE, deci indexabilă fără persistare; extensia = decizie de migrație la
momentul cifrei.

## Ce rămâne deschis

- **78-r1** — grilele XAF Blazor nu trec prin `DataSourceLoader`, deci căutarea
  lor rămâne sensibilă la diacritice; asumat (XAF = vehicul de iterație, 44/53).
- **77-r4 rămâne**: `CodFiscal`/`Iban`/`Marca` intră în concatenarea coloanei
  `Cautare` (extinderea per entitate a lui 77a), nu în mecanismul de aici.
- **77-r7 rămâne**, cu calea de mai sus (trigram la cifră).
