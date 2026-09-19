# Inventar: ce citesc rapoartele reale (pas 1 al designului `docs/nucleu/nucleu-cub-design.md`)

Scop: pentru FIECARE raport din lotul tău, un inventar EXACT și verificabil al
datelor pe care implementarea curentă le citește ca să producă raportul.
Nu propui nimic, nu judeci designul, nu compari cu cubul. Doar inventar, cu
trimiteri `fișier:linie`.

Citește întâi (scurt): `docs/nucleu/nucleu-cub-design.md` §2 (ca să știi ce
înseamnă „coordonată" / „măsură" / „Cauza" / „Unitate") și
`nou/.../BusinessObjects/Registre/Registre.cs` (cele patru registre).

Pentru fiecare raport scrie secțiunile:

1. **Identitate**: nume, cod (D300/D394/D406...), fișierele care îl produc
   (proiecție + controller + reguli), ce ușă îl expune (REST/OData/XAF).
2. **Granul rândului**: după ce se grupează rândurile de ieșire (ex.
   „cont × perioadă", „partener × CUI × tip operațiune × cotă",
   „document × linie"). Dacă raportul are mai multe secțiuni cu granule
   diferite (SAF-T are multe), câte o linie per secțiune.
3. **Tabelul coloanelor**: un tabel cu o linie per câmp de IEȘIRE:
   `câmp | sursă (tabelă.coloană sau calcul) | felul sursei | fișier:linie`
   unde felul sursei e UNUL din:
   - `registru` — coloană a unui registru append-only (RegistruContabil/Stoc/Tva/Terti/Imobilizari),
   - `snapshot` — SolduriPerioada / sume materializate,
   - `document` — citit din antetul sau linia documentului (nu din registru),
   - `nomenclator` — Partener/Produs/PlanConturi/Gestiune/TipTva/... (atribut static al unei coordonate),
   - `politica` — rând de politică / setare de profil / RandD300 / catalog,
   - `calcul` — derivat în cod din celelalte (spune formula pe scurt),
   - `perioada` — starea lanțului de perioade (închisă/deschisă, InchisaPrimaOara...).
4. **Filtrele și cheile de agregare**: pe ce filtrează (perioadă, DataInregistrare
   vs Data vs PerioadaDeclarare, stare document, Carte, storno), ce sumează.
5. **Ce citește din DOCUMENT și nu din registru**: listă explicită — sunt
   cazurile critice pentru pasul următor (arată ce nu e reconstruibil din
   postări fără legătura Cauza).
6. **Cazuri speciale** din cod: rânduri sintetice, excluderi, reguli de semn,
   tratamentul stornoului, rotunjiri de raportare, praguri.

Reguli:
- Citește codul, nu ghici. Fiecare afirmație are `fișier:linie`.
- Nu citi documentele legale integral (`docs/api/dXXX-structura-2026.md`);
  folosește-le doar ca să confirmi numele câmpurilor când codul e opac.
- Fără propuneri, fără „ar trebui", fără comparație cu cubul.
- Regulă de oprire: când fiecare raport din lot are cele 6 secțiuni, te
  oprești. Nu extinde lotul.
- Scrie rezultatul în fișierul indicat (Markdown, în română, cod slim).
  Răspunsul tău final către coordonator: max 15 linii — ce ai acoperit, ce
  n-ai găsit, cele 3 surprize cele mai mari (câmpuri citite din document, nu
  din registru; agregări ne-aditive; date care nu există nicăieri și se
  calculează la citire).
