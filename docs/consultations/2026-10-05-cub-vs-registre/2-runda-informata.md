# Runda 2 (informată): verificarea pe cod

Primești trei lucruri: răspunsul tău din runda 1, dosarul de fapte
(`docs/dosar.md`) și acces de citire la o clonă a repo-ului, la commit-ul `a5df5fe`. Instrucțiunile proiectului pentru agenți sunt mutate în `docs/instructiuni-proiect/`.

## Ce afli abia acum

Echipa a ales cubul de postări (în runda 1: „forma C", adică opțiunea A, nu
opțiunea C) prin decizia 090, pe 2026-09-20, și l-a implementat în două
săptămâni. De aici înainte formele se numesc „cubul" și „registrele", iar
literele A–D desemnează numai opțiunile. Azi ambele căi de scriere există, citirile sunt pe cub, iar
contractul care șterge registrele e aprobat și neexecutat. Întrebarea
owner-ului, înainte de ștergere: ce s-a câștigat, ce s-a pierdut și care
dintre opțiunile A, B, C rămâne.

Nu ți se cere să confirmi decizia și nici să o răstorni. Documentele din repo
sunt scrise de autorii direcției actuale, inclusiv `CLAUDE.md` și dosarul.
Citește-le ca dovadă de intenție. Proba e codul, catalogul de scenarii și
măsurătorile.

Precizarea owner-ului pentru opțiunea B: registrele derivate ar fi calea
principală de citire a rapoartelor. Cubul ar ține structura și coerența, iar
registrele ar ține cache-ul și simplitatea fiecărui raport în contextul lui.

## Reguli de lucru

- Numai citire: fără modificări, fără build, fără ModelCheck, fără rulări pe
  baze. Ai voie cu citire de fișiere, căutare și `git log`.
- Fiecare afirmație de fapt poartă `fișier:linie`. Ce nu poți ancora
  marchezi `[judecată]`.
- Nu citi `comunicari/`.

## Sarcinile

1. **Verifică-ți runda 1.** Ia lista ta de fapte care ți-ar schimba
   verdictul și răspunde la fiecare din cod sau din măsurători.
2. **Predicții contra rezultat.** `docs/nucleu/nucleu-bilant.md` a fost
   scris înainte de implementare. Pentru fiecare rând din tabelul lui spune:
   confirmat, infirmat sau neverificabil, cu dovada de azi. Rândul „forma
   declarației fluxului" era necunoscuta principală; tratează-l pe larg.
3. **Cazurile de probă pe cod real.** Pentru cazurile 1–5 și 7 găsește
   reprezentarea pe ambele căi (dosar §5). Compară: câte concepte trebuie să
   știe cine citește un singur tip, câte linii, câte cazuri speciale, cât e
   cod și cât e date de politică, unde se poate greși în tăcere.
4. **Opțiunea B, concret.** Schițează registrele ca proiecție pe codul
   acesta: care registre, cu ce formă (nu neapărat cea de azi), derivare
   sincronă sau nu, reconstrucție, cine citește ce. Compar-o cu A știind că
   citirile sunt deja pe cub. Spune ce ar trebui păstrat din ce șterge
   TR-D9a și ce s-ar rescrie.
5. **Opțiunea C, concret.** Spune ce ar costa întoarcerea și care dintre
   defectele de model din `nucleu-bilant.md` §2.1 s-ar putea repara rămânând
   pe registre.
6. **Portița.** Regula de azi: nota contabilă trece prin motor și nu atinge
   o coordonată cu unitate fără să numească unitatea. Judecă regula și spune
   cum arată portița în fiecare opțiune, pe cele trei variante ale cazului 4.
7. **Viteza.** Citește măsurătorile (dosar §6). Spune pentru fiecare opțiune
   ce optimizări rămân deschise fără schimbarea modelului și ce se închide.
8. **ALOP.** Uită-te la implementarea veche (dosar §9) cât să înțelegi
   fluxul. Schițează extensia în fiecare opțiune și spune ce structură nouă
   cere.
9. **Dosarul.** Spune ce omite sau înclină.

## Ce livrezi

1. Verdictul pe fiecare dintre cele șapte criterii din runda 1, cu dovada.
2. Ce ți-ai schimbat față de runda 1 și din ce cauză. Dacă nu ți-ai schimbat
   nimic, spune ce dovadă ai căutat și n-ai găsit.
3. Recomandarea pentru TR-D9a: se execută cum e, se amendează sau se
   oprește. Dacă se amendează, spune exact ce.
4. Ce ar trebui măsurat sau prototipat înainte de decizie, dacă e ceva, cu
   costul estimat în zile.
5. Încrederea ta în recomandare, pe o scară de la 1 la 5, și ce o limitează.

Nu fi diplomat. Cel mult 4.000 de cuvinte, în română.
