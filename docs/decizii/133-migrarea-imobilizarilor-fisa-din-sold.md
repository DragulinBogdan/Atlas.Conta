# 133 — Migrarea imobilizărilor: fișa se naște din soldul 1C la graniță, amortizarea o face motorul, ieșirea se face pe fișă

- Data: 2026-10-17
- Stare: propusă
- Execuție: etapa E4, pasul 2 (2026-10-17), branch `e4-pas2-fise-mf`; ianuarie–martie 2025 cu bază recreată: contractele 1–5 îndeplinite, ITV-ul lui martie generat
- Arii: import, imobilizari, partide
- Amendează: —
- Închide: F26-r13
- Docs: `docs/import/e4-migrarea-contract.md` (pasul 2); `docs/stare-curenta/import.md`

## Regula durabilă

**Fișa unei imobilizări migrate se naște din soldul 1C la graniță și din parametrii de amortizare ai sursei. De acolo încolo, amortizarea și ieșirea le face motorul, pe fișă. Diferența față de rândurile 1C are nume.**

(a) **Fișa la graniță.** Un MF cu parametri de amortizare în 1C și sold pe un singur cont de activ 20x/21x devine imobilizare în PIF-ul de deschidere (data: ziua dinaintea graniței; înregistrarea: granița). Valorile fișei:
- valoarea de intrare = soldul pe activ;
- amortizarea cumulată = soldul pe contul 28x principal, plafonat la valoare; cea fiscală e egală cu cea contabilă;
- lunile rămase = restul împărțit la cota fixă 1C (valoarea de calcul / termenul de amortizare), rotunjit la cel mai apropiat întreg;
- durata = maximul dintre termenul de utilizare, termenul de amortizare și lunile rămase.

Perechea (activ, amortizare) din 1C care diferă de politica tipului primește un tip geamăn, cu politica proprie. Restul soldurilor de MF rămân fără fișă, fiecare cu motiv numit:
- teren sau MF fără parametri;
- imobilizare în curs;
- MF doar cu amortizare;
- al doilea cont 28x;
- amortizare peste valoare;
- amortizare cu sold debitor.

(b) **Soldul fără subconto MF de semn opus naturii contului** (debit pe 28x, credit pe 20x/21x) nu are o fișă care să-l poarte. Controlul deschiderii pe cont = fișele + soldurile fără fișă. Diferența față de Balanță e Δ declarat la deschidere, purtat lunar de contractul 1, ca Δ-ul de stoc fără detaliu de lot (M1-D6).

(c) **Rândul negativ al unui stingător pe o poziție inițială** e netarea 1C a unei poziții de sens opus a partenerului în aceeași plată. Atlas nu o aplică. Mișcarea intră ca neaplicată pe partidă (107h), iar contractul 5 o numește „stingere refuzată”.

(d) **Reziduul golirii lotului la retur** (T-r2) trece pe contul politicii de diferențe, iar 1C nu îl postează. Conectorul îl declară divergență pe perechea postată de motor, după operare.

(e) **Amortizarea lunii o generează motorul** (AMO), înaintea ITV-ului. Rândurile 68x = 28x ale închiderii 1C pe un MF cu fișă nu se mai transcriu. Diferența pe pereche (1C − Atlas) e divergența „cota Atlas pe restul de la PIF față de cota 1C”.

(f) **PIF-ul 1C din an devine PIF Atlas** la data 1C. Valoarea = Σ debitelor 2xx cu subconto MF până la PIF. Suportul îl dă puntea facturii de achiziție.

(g) [amendat de 134] **Vânzarea unui MF cu fișă devine CAS** (cauza Vânzare), prin regenerarea L3. Rândurile 1C 28x / 658x = 2xx ale facturii sunt acoperite de CAS. Diferența e divergența „ieșirea fișei”.

(h) **Contractul 5 numește încă două fapte ale notei**:
- partea unei note cu cel puțin o latură numită de sursă și lăsată pe FIFO (E4-r1, și când sursa are mișcare pe partidă);
- partea peste restul partidei numite, al cărei excedent trece pe FIFO (N-D7).

## Context

Pasul 2 al etapei E4 cere fișele la deschidere (E4-D3, F26-r13) și ianuarie–martie 2025 verde, cu ITV-ul lui martie. Pe Flax, deschiderea fără fișe lăsa vânzarea din 10.03.2025 (FLXCORP000017232, multifuncționala BH C360i) refuzată de gardianul poziției fără fișă, iar ITV-ul lui martie nu se mai genera.

Recensământul la 01.01.2025: 92 de fișe (valoare Σ 3.805.511,88, amortizare Σ 949.551,12, 25 amortizate integral) și 26 de solduri fără fișă. Pe 2814, 1C ține un sold debitor de 2.179,98 fără subconto MF, din notele dinainte de 2023. Niciun MF nu îl poartă.

Februarie și martie n-au fost rulate înainte de pasul 2 (măsurătoarea de plecare acoperă doar ianuarie). Primele lor rulări au scos trei clase de diferențe, niciuna pe imobilizări: (c), (d) și (h).

## Tranșări

- (a) rotunjire la cel mai apropiat întreg, nu în sus: restul 1C e k cote plus reziduul rotunjirilor de bani. Rotunjirea în sus adăuga o lună pentru reziduu și cobora cota Atlas pe toată durata. Pe ianuarie divergența de cotă era Σ |Δ| 567,07 cu rotunjirea în sus și 11,02 cu cea la întreg.
- (b) Δ declarat, nu fișă redusă: o fișă cu amortizarea redusă ar fi amortizat altfel decât 1C de la prima lună. O poziție debitoare pe 28x fără fișă e refuzată de model (SC-IMO-33). Fișele rămân exacte, iar diferența se vede la deschidere, cu nume.
- (c) refuz numit, nu stingere între două partide inițiale: aplicarea ar cere ca o partidă inițială să stingă o altă partidă inițială a aceluiași partener, adică o regulă de motor (regula de oprire 1). Cazul de pe Flax e de 403,09 lei pe februarie.
- (e) motorul, nu transcrierea: amortizarea transcrisă din 1C n-ar fi trecut prin fișă. Fișa ar fi rămas cu cumulul de la graniță, iar CAS-ul ar fi scos alt cumul decât cel postat.

## Ce rămâne deschis

- Soldul debitor de 2.179,98 pe 2814 fără subconto MF e o curățenie a sursei. Migrarea reală îl poartă ca Δ declarat până la regularizarea lui în 1C.
- Vânzările din iulie, casarea din decembrie și PIF-ul necorporal din septembrie intră la pasul 3, pe aceleași reguli.

## Istoric

- 2026-10-17: scrisă la pasul 2 al etapei E4; propusă owner-ului.
