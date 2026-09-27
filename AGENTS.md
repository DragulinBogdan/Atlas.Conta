# Atlas.Conta — instrucțiuni pentru agenți

- Citește `CLAUDE.md`: este intrarea comună în proiect pentru Codex și Claude.
  Regulile sale de lucru se aplică ambilor agenți; nu le copia aici.
- Înaintea implementării citește `docs/invarianti.md`, contractul feliei și
  fișierul tipului din `docs/nucleu/scenarii/`. Citește deciziile invocate,
  nu întregul istoric. 091 amendează 090: Import1C nu mai este gate.
- Amendamentul explicit stabilește ținta; codul și probele stabilesc starea
  implementată. Raportează contradicțiile, nu schimba așteptarea ca să treacă
  implementarea. Nu redeschide direcția cubului fără un contraexemplu concret.
- Folosește CodeGraph înainte de căutări sau citiri pentru a localiza și
  înțelege codul, cât timp există `.codegraph/`. Documentația și configurația
  se citesc direct. Nu inițializa și nu reindexa proiectul din proprie inițiativă.
- Rulează verificările prin `nou/tools/ModelCheck/scripts/verifica.ps1`;
  instrucțiunile sunt în `docs/stare-curenta/dezvoltare-si-validare.md`.
  Bazele sufixate aparțin verificărilor. Flax se citește doar pentru recensământ.
- Așteptările scenariilor sunt numerice și independente de registrele vechi.
  Un refuz așteptat este rezultat al scenariului, nu stare de implementare.
- La predare precizează schimbarea, scenariile acoperite, comenzile executate
  și limitele rămase. Actualizează documentația afectată în aceeași schimbare.
- Nu lucra simultan cu alt agent în același checkout sau pe aceleași baze.
  Pentru execuție paralelă: worktree și sufix de bază distincte, cu rulările
  grele coordonate conform regulii comune. Nu lansa subagenți implicit.
- Refuzurile au coduri stabile în `Module/Declaratii/Coduri.cs`
  (`CoduriRefuz`), emise de declarant ca linia `COD: mesaj`. Proba asertează
  codul pe ușa declarației (`Materializare.Refuzuri`). Ușa entității
  (`OperareApi.Valideaza`/`Opereaza`) trece întâi prin `ValideazaOperare` al
  clasei și gardienii registrelor (`StocService`, perioadă, stare), care
  refuză cu text înaintea declarantului: acolo se asertează familia mesajului
  și absența efectelor, iar refuzul registrului se marchează ca atare în
  fișierul tipului (până la TR-D8). Tiparul: `ScenariiBcs.Refuzuri`.
- Fișierul tipului și scena lui urmează tiparul BCS (`BCS.md`,
  `ScenariiBcs.cs`): fixture prin documentele reale (FCT → NIR conex),
  comenzi prin `OperareApi` în ObjectSpace-uri noi, așteptări ca constante,
  un an propriu liber, curățenie pe marcaj în `finally`; nu comută
  `PosteazaInCub`, nu inserează în registre sau cub.
- Reguli de lucru (ale agentului) nu intră în `docs/stare-curenta/`: acolo
  stau doar mecanica și starea; regula pentru agenți stă aici, motivația în
  `docs/decizii/`. Un rând existent din docs se rescrie doar dacă pasul îl
  atinge; altfel se raportează.
- Predarea nu certifică singură: owner-ul și Claude reverifică prin rulare
  (`verifica.ps1`) și review înainte de commit. O afirmație despre cod
  („nu există coduri stabile”) se verifică în surse înainte de a intra în
  docs.
- Owner-ul poate amâna explicit review-urile intermediare pe o etapă de
  dezvoltare mai lungă. Amânarea se notează în mesajul de predare și în
  fișierul tipului („review advers amânat de owner până la sincronizare”);
  pașii continuă fără commit. La sincronizare, starea reverificată intră
  într-un commit de bază, iar corecturile review-urilor restante urmează
  câte un commit pe felie. Amânarea nu suspendă `verifica.ps1` la fiecare
  pas.
