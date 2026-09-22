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
