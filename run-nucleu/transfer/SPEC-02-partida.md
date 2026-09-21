# Pas 3 (transferul), explorarea 2: „ce e o partidă” și împerecherea — cod + cifre

Scop: faptele (din cod și din baza de import) de care are nevoie
coordonatorul ca să tranșeze două întrebări de modelare ale nucleului nou:
(1) ce e „o partidă” — documentul, sau postarea pe contul de terț — și
(2) cum se reprezintă împerecherea în cubul de postări. NU tranșezi tu;
livrezi cifre și descrierea exactă a semanticii de azi.

Citește întâi: `docs/nucleu/nucleu-cub-design.md` §2–§4 și §7;
`docs/nucleu/nucleu-fizica.md` §3 (ultimul punct, „1 diferă”) și §4.4
(propunerea netestată + cele trei rupturi găsite de review);
`run-nucleu/fizica/02-semantica-mapare.md` (ce e Partener/Unitate în cub);
`run-nucleu/fizica/review/04-imperecheri-mapare.sql` (SQL-ul care a găsit
rupturile); `run-nucleu/fizica/pas1/01-schema.sql` (coloanele cubului).
Decizia 88 (`docs/decizii/088-*.md`) doar sub-punctele despre împerechere
(fapt datat, 88j) și partide.

Mediu: Postgres 18.4 în docker `contapal-postgres-1`, fără psql local:
`docker exec contapal-postgres-1 psql -U postgres -d Atlas.Conta.Nucleu.Fizica.x1 -Atc "..."`;
scripturi: `docker cp f.sql contapal-postgres-1:/tmp/` + `-f /tmp/f.sql`.
Baza `Atlas.Conta.Nucleu.Fizica.x1`: schema `public` = registrele de azi
(F0, clona Flax: `RegistruContabil`, `RegistruTva`, `Documente`,
`DocumentDetalii`, `Imperecheri`, `PartideDeschise`, `PlanConturi`,
`Repartitori`…), `f2."Postare"` = cubul (partiții pe `Spatiu` 1/2/3),
`cub."Tranzactie"`, `cub."Sold"`. Ai voie să creezi DOAR schema `tr` pentru
tabele/vederi ajutătoare; nimic în `public`/`f2`/`cub`; Flax nu se atinge;
`Atlas.Conta.Nucleu.Fizica.x10` nu se folosește. Toate SQL-urile le ții în
`run-nucleu/transfer/02-sql/`, ieșirile brute în `02-out/`. Capcană: TOATE
`Imperecheri.Data` pe Flax sunt 2026-09-18 (artefact de import) — nu trage
concluzii din data împerecherii.

Scrie în `run-nucleu/transfer/02-partida-imperechere.md`:

## A. Semantica de azi, din cod (fișier:linie pe fiecare afirmație)
1. Partida azi: `Document.TotalStingere` (cine o scrie, din ce), `Imperecheri`
   (coloane, cine scrie, `ImperechereService.AsignatFataDe`, `SensDeStins`,
   `PoateFiStins` — pe care frunze și ce decid), `PartideDeschise` (cine
   materializează, cheia, ce înseamnă „rest”), `ImperecheriProiectii.cs:120-169`
   (cele șase ramuri: ce literal de `Sens`/`Tip` pune fiecare tip de
   document), regula de dată 88j, ce se întâmplă la storno-ul unui document
   împerecheat și la storno-ul stingătorului.
2. Stingerea din trezorerie: cum alege `DocumentTrezorerie` ce stinge
   (linii, `LaturaPereche`, plata parțială, avansul pe 419/409, decontul cu
   angajatul 542/4xx), ce postează contabil pe fiecare caz.
3. Conexul FCT→NIR: cine postează pe 401 și cu ce sumă (linia FCT, linia NIR,
   linia de TVA), `LinieSursa`, ce se întâmplă la o FCT de servicii (fără
   NIR), la o FCT cu mai multe NIR-uri, la storno-ul NIR-ului.
4. Cazurile partener–partener: compensarea (există document?), nota
   contabilă cu doi parteneri, transferul între partide ale aceluiași
   partener (avans → factură → regularizare): ce tipuri de document le fac
   azi și ce postează.

## B. Cifrele, pe Fizica.x1 (fiecare cu SQL-ul și ieșirea brută)
1. Per tip de document (`ClrType`): câte documente operate, câte postează pe
   ≥1 cont de terț (`PlanConturi.RolTert<>0`), distribuția numărului de
   conturi de terț DISTINCTE pe document (1 / 2 / 3+), și numărul de
   parteneri distincți pe document (0 / 1 / 2+).
2. FCT→NIR: câte FCT au ≥1 NIR conex, câte NIR au FCT sursă, câte NIR nu au;
   pentru perechile FCT–NIR: Σ postat pe 401 de FCT, de NIR, și
   `FCT.TotalStingere`; câte FCT au `TotalStingere` = Σ(401 FCT + 401 NIR)
   exact; câte au mai multe NIR-uri.
3. Cele 1.200 de împerecheri cu contul de terț al stinsului ≠ al
   stingătorului: tabel pe (tip stins, tip stingător, cont stins, cont
   stingător, număr, Σ). Cele 1.179 documente stinse fără postare de terț:
   pe tip, cu 3 exemple (ID, număr) și ce postează de fapt.
4. Avansul: documente cu postare pe 419/409 — pe tip; câte facturi de
   ieșire au împerechere cu un avans (stingător = document cu 419) și cifra
   „rest brut vs rest compensat” pe ele (F0: `TotalStingere − Σ Asignat`;
   cub: Σ pe toate conturile de terț ale documentului).
5. Decontul cu angajatul și partener–partener: documente cu ≥2 parteneri
   distincți pe postările de terț (pe tip, număr, 3 exemple); postări pe
   542/4xx ale angajaților.
6. **Cele trei definiții candidate de partidă**, numărate la 31.12.2025:
   - A = documentul (`Unitate` = DocumentId, ca în cubul măsurat);
   - B = documentul × contul de terț (o partidă per (DocumentId, Cont));
   - C = lanțul conex (FCT + NIR-urile ei = o partidă; altfel ca A).
   Pentru fiecare: numărul de partide cu rest ≠ 0, Σ rest pe debitor și pe
   creditor, și câte coincid cu `PartideDeschise` la 12/2025 (pe
   DocumentId, cu diferența de rest). Definiția care coincide cel mai des e
   FAPT, nu recomandare.
7. Împerecherea ca tranzacție (§4.4 al fizicii): sub fiecare definiție A/B/C,
   câte din cele 44.448 de împerecheri sunt reprezentabile ca „două postări
   pe același (Cont, Latura), −Suma pe partida stingătorului, +Suma pe
   partida stinsă” FĂRĂ să se rupă (contul există pe ambele partide, latura
   e aceeași, suma ≤ restul); câte se rup și de ce (categorii).

## C. Ce n-ai putut proba
Listă scurtă (ex. valuta, partidele în valută, cazuri fără date pe Flax).

Reguli:
- Cifrele vin din SQL, nu din estimări; `reltuples` interzis, `count(*)`.
- Fără propuneri, fără „ar trebui”. Faptele se raportează inclusiv când
  contrazic designul.
- Nu atingi `nou/`, nu rulezi build-uri, nu scrii în alte baze.
- Regulă de oprire: A (4 puncte) + B (7 puncte) + C. Dacă o cifră nu se
  poate obține (coloană lipsă, ambiguitate de mapare), spui ce lipsește;
  nu inventezi maparea.
- Răspunsul final către coordonator: max 15 linii — cifrele-cheie (B2, B3,
  B6, B7), surprizele, ce n-ai putut proba.
