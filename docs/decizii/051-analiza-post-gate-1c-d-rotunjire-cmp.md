# Decizia 51 — Analiza post-gate 1C-d

- **Data**: 2026-07-27 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §51
- **Docs**: docs/architecture-notes-2026-07-28.md

---

**Analiza post-gate 1C-d (sesiune de analiză, fără cod): structura ține,
rotunjirea devine convenție de profil, CMP parcat cu raționament.**
Tranșările:
(a) **Structura de documente NU se redeschide** — gate-ul e proba (~205k
documente, 0 eșecuri de operare, 1 cheie reziduală din 66.553); tot ce a
scos anul ca lipsă de model s-a fixat în timpul fazei (scara numerică 49e,
identitatea produs×cont 50a — a importului); restul deschis (lotul 50e,
G3) e integral în unealtă/seed, nu în schemă.
(b) **Reziduul de rotunjire e sum(round) vs round(sum), nu round vs
round**: motorul rotunjește per rând de registru (`Scara.RotunjesteBani`
per linie), sursa poartă valoarea întreagă a lotului — reziduul per cheie
e structural și nu dispare prin nicio convenție; CONVENȚIA alege doar
sensul (AwayFromZero împinge midpoint-urile sistematic în același sens —
ieșirile, cantitate parțială × preț cu 6 zecimale, cad pe jumătatea de
ban mai des decât intrările → deriva netă −4,18/an măsurată; rotunjirea
bancară ar face-o să se compenseze).
(c) **Convenția de rotunjire devine dată de profil** (închide „decizia
parcată" 49e/HANDOFF; motivată de completitudine — acoperă o cerere
viitoare, nu de reconciliere): `MidpointRounding` în seed-ul profilului,
lângă ancoră; citită O DATĂ la pornire și fixată static în `Scara` (fără
politici prin semnăturile motorului); ÎNGHEȚATĂ per bază — schimbarea pe
o bază vie amestecă istoricul (gardianul `VerificaProfil` o poate păzi);
alarma reconcilierii din Import1C citește convenția în loc s-o presupună;
profilul Flax rămâne AwayFromZero. Implementare = felie mică aditivă,
lipită de 1C-d-final (aceeași rulare de re-validare).
(d) **CMP: config-ul Flax „preț mediu ponderat" degenerează în
identificare specifică** prin subconto-ul pe lot — media ponderată se
calculează în interiorul combinației produs × doc achiziție × depozit,
iar un lot are un singur preț de intrare, deci media = prețul lotului;
de asta decizia 13 reproduce anul la cent. Singura manifestare reală =
costul per depozit la transfer (cheia −0,69, deja măsurată ca divergență
cu semne opuse de hunk-ul BTR-cost). „Configurare fără motor" NU există
pentru evaluare (metoda de evaluare E motorul: finalizarea lotului,
PregatesteOperare pe DSC/BCS/BTR/ASM/RLF, gardianul de sold, storno);
câștigul de potrivire ar fi 0,69 lei/an — nejustificat. **Parcat cu
nume: `PoliticaEvaluare`** (CMP per produs×gestiune) ca extensie viitoare
de profil, condiționată de cerință reală (bază 1C fără subconto pe lot la
importul generic 35a / client OMFP care ține CMP); a forța valorile 1C în
import ca „potrivire" rămâne interzis — harness-ul validează modelul
nostru, nu transcrie sursa.
(e) **Schița `PoliticaEvaluare`, clarificată (sesiunea 27.07.2026), ca
raționamentul să nu se piardă:** lotul pentru CANTITATE, pool-ul de produs
pentru VALOARE — media la o dată = Σvaloare/Σcantitate din RegistruStoc
per produs(×gestiune), aceeași mașinărie de prefix-sum ca gardianul de
sold, deci FĂRĂ stare nouă persistată; politica schimbă doar funcția de
evaluare în punctele de descărcare (PregatesteOperare pe DSC/BCS/BTR/RLF),
lotul păstrează PretUnitar (intrarea), iar snapshot-ul de ieșire e rândul
de registru (Valoare — proiecția poziției de document; NU un preț pe lot:
sub CMP perpetuu media e funcție de timp). **Varianta principală: CMP
PERIODIC** (OMFP pct. 96(2) — media lunii): ieșirile lunii la media
provizorie curentă + document de regularizare 607/711 la închidere
(precedentul ITV/InchidereTvaService) — prietenos cu registrele
append-only, fără repostare; **CMP perpetuu = doar evaluare la nevoie**
(costul lui real: valoarea postată depinde de pool-ul la acea dată ⇒
operarea retroactivă cere repostare/interdicție — lumea recalculului 1C
din care registrele au ieșit deliberat). De fixat la design: granularitatea
(produs global vs produs×gestiune — 1C face per depozit, sursa cheii
−0,69; per gestiune transferul mută valoare la CMP), purjarea reziduului
la cantitate 0, interacțiunile RLF/plus inventar/ASM. Conformitate:
modelul ACTUAL e apărat ca FIFO (pct. 96(3), auto-FIFO pe „primul lot
intrat", override-ul = excepție), nu ca identificare specifică (pct.
285(4) o interzice pe fungibile în număr mare); consecvența metodei
(OMFP) ⇒ PoliticaEvaluare înghețată per bază, sub VerificaProfil, ca
rotunjirea 51c.
