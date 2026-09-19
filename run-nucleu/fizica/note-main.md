# Notele coordonatorului (pasul 2, fizica) — constatări pe parcurs, pentru sinteză

- Explorarea F0 (01): snapshot-ul 12/2025 nu servește nicio balanță a lui 2025
  (referința cerută e 2024-12-31, inexistentă ⇒ registrul se citește integral);
  `sold-parteneri`/`documente-cu-rest` la 31.12 citesc DOAR snapshot.
- Pe 4111 „repartitorul laturii" e UnitateInterna (107.033) / ContPropriu
  (31.381), Partener doar 5.825 — confirmă amendamentul 4 al pasului 1
  (partenerul pe postarea de terț). În cub: Partener = repartitorul de tip
  Partener de pe ORICARE latură a rândului, pe ambele postări.
- `IX_RegistruTva_PerioadaAn_PerioadaLuna` există și nu e folosit (predicat
  aritmetic în `TvaProiectii.cs:145-157`): Seq Scan 90.732 rânduri vs Index
  Only Scan 1,6 ms.
- Scrierea F0: 50 rânduri contabile = 57 ms, din care ~33 ms în 20 de triggere
  FK (16 coloane plate de dimensiune cu FK propriu).
- Cifre exacte Flax: contabil 304.382, stoc 283.498, TVA 90.732, 205.186
  documente operate, 0 storno, 0 GCRecord nenul (de confirmat în pas1).
- Reconcilierea pas 1 (prima rulare, cu despicare): (a)–(e) toate 0/exacte;
  cub ×1 = 1.093.614 postări (contabil 628.652 = 608.764 + 19.888 despicări;
  stoc 283.498; fiscal 181.464); 8 versiuni CodTva; 38 documente stingătoare
  cu mai multe postări de terț; 0 rânduri fiscale cu taxă zero.
- **Artefact de date Flax**: TOATE cele 44.448 `Imperecheri` au
  `Data = 2026-09-18` (data rulării importului) ⇒ `PartideDeschise` la
  12/2025 arată 24.937 facturi de ieșire integral neîncasate (77 M) deși au
  împerechere. Corect față de regula „împerecherea e fapt datat" (88), dar
  cifra de raport e a importului, nu a realității. Consecință pentru cub:
  amendamentul 3 — împerecherea = tranzacție proprie datată (Fel 4), două
  postări pe același cont și aceeași latură (−Suma pe partida stingătorului,
  +Suma pe partida facturii); Σ per (Cont, Latura) neschimbată.
- Mărimi ×1 (prima rulare): f1 188 MB tabelă + 187 MB indexi EF-style (17
  btree); f2 188 + 101 MB doar cu indexii de identitate; F0 (3 registre)
  108 + 120 MB; `SolduriPerioadaContabil` 45 + 110 MB (indexii > tabela).
- Pas 1 re-rulat cu amendamentul 3 (17:35): (a)–(e) verzi; contabil 697.660
  (= 2·304.382 + 2·44.448), stoc 283.498, fiscal 181.464; total ×1 =
  1.162.622 postări; tranzacții 249.622 (205.173 operare + 1 deschidere +
  44.448 împerechere). f3 extins la 2016…2026 (împerecherile din 2026).
- (f) la 31.12.2025: FacturaIesire 36.666 coincid / 2.030 diferă;
  FacturaIntrare 1.082 / 17.797. Cauză STRUCTURALĂ (nu de mapare): pe Flax
  datoria față de furnizor e împărțită pe două documente prin conexul
  FCT→NIR — NIR-ul creditează 401 cu netul (83,6 M / 34.289 rânduri), factura
  doar cu TVA-ul (10,9 M / 31.211; raport postat/TotalStingere = 21/121,
  19/119). În cub partida se deschide pe documentul care POSTEAZĂ, deci NIR
  și FCT au partide separate; `Imperecheri`/`PartideDeschise` tratează
  factura ca partidă unică. = limita §5.6 din pasul 1 (linia FCT vs NIR),
  acum cu cifre; se tranșează la pasul 3 (modelarea documentelor).
- x10 gata 17:41 (replicarea 9 treceri ≈ 25 s fiecare + indexi 1 min);
  agentul pasului 1 n-a mai livrat raportul final (idle) — verificat direct:
  postări x10 = 6.975.448 contabil / 2.772.457 stoc / 1.814.640 fiscal
  (formulele exacte: ×10 minus deschiderile nereplicate). Mărimi x10: f1
  2.007 MB + 1.752 MB indexi (EF-style); f2 2.009 MB + 448 MB (identitate);
  f3 2.010 + 710 MB; F0 (3 registre) 1.072 MB + 779 MB; Tranzactie 182+100 MB.
  Rând cub ≈ 174 B; rând RegistruContabil ≈ 180 B (dar 2 postări per rând).
