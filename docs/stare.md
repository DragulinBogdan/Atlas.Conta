# Unde suntem

Generat de `docskit gen` din antete, registru și cod; nu se editează. Cronologia e în jurnal; comparația paginilor cu ultimul commit o face `docskit check` (gardianul `actualizat`).

## Rezumat

| Ce | Cât |
|---|---|
| Decizii | 118: 84 active, 33 amendate parțial, 0 depășite, 1 propuse |
| Restanțe | 375: 283 deschise, 92 închise |
| Pagini de stare curentă | 7 |
| Contracte de felie (`Tip: contract`) | 0, deschise 0 |
| Cataloage vii (`Tip: catalog`) | 0 |
| Fișiere Markdown în `docs/` | 283 |
| Citări de decizii din cod (`// N`) | 126 |

## Decizii în vigoare pe arie

### README (7)

- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [76](decizii/076-p5-felia19-ntc-asm-retururi.md) Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client
- [91](decizii/091-scenarii-in-loc-de-import.md) Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate; clona Flax rămâne sursă de întrebări; „rotund" primește definiție ca regulă de oprire a PoC-ului; stornoul după reevaluare = inversarea originalului + compensarea efectelor rămase active; proiecțiile persistate doar reconstruibile din postări; triajul restanțelor în patru stări — parțial amendată de 107, 118
- [103](decizii/103-fapte-fiscale-istorice-si-perioade-distincte.md) Fapte fiscale istorice și perioade distincte de declarare
- [104](decizii/104-straturi-izolate-entitati-proprii.md) Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată — parțial amendată de 106
- [110](decizii/110-taierea-cubul-singurul-scriitor.md) Tăierea: cubul e singurul scriitor și singura sursă; registrele, regimul dual și oracolul lor au ieșit (TR-D9a) — parțial amendată de 113
- [111](decizii/111-repartitor-unic-si-registre-tipate.md) Un singur repartitor de sold pe postare, partenerul fiscal în blocul fiscal, registre tipate pentru ce nu se balansează — propusă

### api-si-client (25)

- [39](decizii/039-atlas-dxf-constraint-translator.md) Pre-polish
- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [43](decizii/043-design-p5-clientul-react.md) Design pasul 5 — clientul React — parțial amendată de 104
- [53](decizii/053-gate-xaf.md) GATE XAF
- [55](decizii/055-p5-spike1-btr.md) Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer
- [56](decizii/056-p5-felia2-fct-nir.md) Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API — parțial amendată de 77, 81
- [57](decizii/057-p5-felia3-trezorerie.md) Pasul 5, felia 3 — trezoreria prin API
- [58](decizii/058-p5-felia4-fcl-dsc.md) Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API
- [60](decizii/060-p5-maruntisuri-f3-f4.md) Mărunțișurile F3/F4 golite
- [66](decizii/066-p5-felia9-raportare.md) Pasul 5, felia 9 — raportarea pe registre
- [68](decizii/068-p5-felia11-jurnale-tva.md) Pasul 5, felia 11 — jurnalele de TVA
- [70](decizii/070-p5-felia13-motor-structura.md) Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)
- [77](decizii/077-p5-felia20-finisaj-client.md) Pasul 5, felia 20 — finisajul clientului: căutarea fără diacritice, cache-ul de nomenclator, confirmările, `Neincluse` agregat, primele ecrane de nomenclator
- [79](decizii/079-p5-felia21-itv.md) Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client: comandă cu cauză + ecran de rezultat — parțial amendată de 80
- [80](decizii/080-refuzuri-acces-toate-usile.md) Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp
- [81](decizii/081-p5-felia23-implicite-politici.md) Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru
- [84](decizii/084-p5-felia24-politici-explica.md) Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul `Configurator`, cele șapte ecrane, potrivirea ca funcții pure și „Explică"
- [85](decizii/085-mod-acces-listview-xaf.md) Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): `Server` implicit cu paginare, `ServerView` opt-in pe registre, `Client` explicit pe grilele de culegere, IF/IFV doar cu prag măsurat
- [86](decizii/086-p5-felia25-dvi.md) Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/25 și 7/22, codurile SAF-T de import
- [87](decizii/087-p5-felia26-imobilizari.md) Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp — parțial amendată de 97
- [89](decizii/089-tph-discriminator-tipul-ca-data.md) Pasul 5, felia 28 — TPH cu discriminatorul mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat
- [103](decizii/103-fapte-fiscale-istorice-si-perioade-distincte.md) Fapte fiscale istorice și perioade distincte de declarare
- [104](decizii/104-straturi-izolate-entitati-proprii.md) Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată — parțial amendată de 106
- [106](decizii/106-regim-pe-stare.md) Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile
- [108](decizii/108-gate-transversal-tr-d8.md) Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată — parțial amendată de 110

### dezvoltare-si-validare (33)

- [14](decizii/014-registre-append-only.md) Registre persistate, append-only
- [15](decizii/015-dimensiuni-owned-type.md) Dimensiuni
- [21](decizii/021-seed-pe-functionalitate-nu-transcriere.md) Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy
- [23](decizii/023-felia-3a-persistenta-seed.md) Felia 3a
- [26](decizii/026-felia-3c1-nir-fct-conex.md) Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex — parțial amendată de 112
- [30](decizii/030-felia-3c4-factura-iesire.md) Felia 3c-4 — FacturaIesire
- [34](decizii/034-pasul-4-migrarea-datelor.md) Pasul 4 — migrarea datelor
- [36](decizii/036-felia-p1-tva-structural.md) Felia P1 — profil privat + TVA structural — parțial amendată de 71
- [39](decizii/039-atlas-dxf-constraint-translator.md) Pre-polish
- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [43](decizii/043-design-p5-clientul-react.md) Design pasul 5 — clientul React — parțial amendată de 104
- [45](decizii/045-design-faza-1c-import-reconciliere.md) Design FAZA 1C — conector de import + reconciliere pe an fiscal complet — parțial amendată de 75
- [47](decizii/047-felia-1c-b-import1c-deschiderea.md) Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea — parțial amendată de 107
- [49](decizii/049-felia-1c-c-documentele-prin-motor.md) Felia 1C-c — documentele 2025 prin motor
- [50](decizii/050-felia-1c-d-gate.md) Felia 1C-d — gate-ul fazei
- [56](decizii/056-p5-felia2-fct-nir.md) Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API — parțial amendată de 77, 81
- [66](decizii/066-p5-felia9-raportare.md) Pasul 5, felia 9 — raportarea pe registre
- [70](decizii/070-p5-felia13-motor-structura.md) Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)
- [80](decizii/080-refuzuri-acces-toate-usile.md) Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp
- [81](decizii/081-p5-felia23-implicite-politici.md) Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru
- [84](decizii/084-p5-felia24-politici-explica.md) Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul `Configurator`, cele șapte ecrane, potrivirea ca funcții pure și „Explică"
- [85](decizii/085-mod-acces-listview-xaf.md) Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): `Server` implicit cu paginare, `ServerView` opt-in pe registre, `Client` explicit pe grilele de culegere, IF/IFV doar cu prag măsurat
- [87](decizii/087-p5-felia26-imobilizari.md) Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp — parțial amendată de 97
- [89](decizii/089-tph-discriminator-tipul-ca-data.md) Pasul 5, felia 28 — TPH cu discriminatorul mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat
- [90](decizii/090-nucleu-cub-de-postari.md) Nucleul ca ledger n-dimensional — un singur cub de postări în locul celor patru registre; motor pur pe operand închis (`Contract = Postari + Decizii + Ipoteze`); unitatea nominalizată (lot = partidă = fișă) numită pe linie; împerecherea ca nominalizare la operare + document `Împerechere` cu tranzacție de fel `Transfer`; linia FCT postează recepția; o singură postare de stoc, `TipStoc` dispare structural; stornoul ca tranzacție distinctă = inversul cauzat ∪ atribuit; fiscala postată în `Carte=Fiscal`; ordinea TR-D6a…D10 cu Import1C ca probă supremă; XAF și React înghețate pe funcții — parțial amendată de 91, 92, 93, 95, 96, 110
- [91](decizii/091-scenarii-in-loc-de-import.md) Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate; clona Flax rămâne sursă de întrebări; „rotund" primește definiție ca regulă de oprire a PoC-ului; stornoul după reevaluare = inversarea originalului + compensarea efectelor rămase active; proiecțiile persistate doar reconstruibile din postări; triajul restanțelor în patru stări — parțial amendată de 107, 118
- [102](decizii/102-fara-compatibilitate-cu-bazele-de-dezvoltare.md) Fără compatibilitate cu bazele de dezvoltare până la prima producție
- [104](decizii/104-straturi-izolate-entitati-proprii.md) Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată — parțial amendată de 106
- [106](decizii/106-regim-pe-stare.md) Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile
- [107](decizii/107-deschiderea-de-terti-din-balanta-nivel3.md) Migrarea deschide terții per partidă din `BalantaNivel3`, prin `Materializare.Deschide`; stingerile anului pe partide inițiale; partener generic de migrare
- [108](decizii/108-gate-transversal-tr-d8.md) Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată — parțial amendată de 110
- [115](decizii/115-nota-numeste-partida.md) Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere
- [118](decizii/118-guvernanta-documentatiei.md) Guvernanța documentației: kitul unificat dă forma, Atlas.Conta îl aplică pe structura lui, cu gardieni care intră pe rând

### domeniu-si-operare (50)

- [10](decizii/010-plan-de-conturi-sintetic.md) Plan de conturi doar sintetic
- [13](decizii/013-evaluare-stoc-identificare-pe-lot.md) Evaluare stoc
- [14](decizii/014-registre-append-only.md) Registre persistate, append-only
- [15](decizii/015-dimensiuni-owned-type.md) Dimensiuni
- [16](decizii/016-repartitori-tpt-si-calitati.md) Repartitori — parțial amendată de 89
- [17](decizii/017-plati-incasari-imperechere-conex.md) Plăți/încasări = tipuri de document
- [20](decizii/020-un-singur-nomenclator-de-tipuri.md) Un singur nomenclator de tipuri, și e codul
- [22](decizii/022-testul-bazei-inchis.md) Testul bazei
- [23](decizii/023-felia-3a-persistenta-seed.md) Felia 3a
- [25](decizii/025-felia-3b-motorul-de-operare.md) Felia 3b
- [26](decizii/026-felia-3c1-nir-fct-conex.md) Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex — parțial amendată de 112
- [27](decizii/027-felia-3c2-bon-consum.md) Felia 3c-2 — BonConsum
- [30](decizii/030-felia-3c4-factura-iesire.md) Felia 3c-4 — FacturaIesire
- [31](decizii/031-felia-3c5-plata-incasare-imperechere.md) Felia 3c-5 — Plata/Incasare + Imperechere — parțial amendată de 76, 88
- [32](decizii/032-felia-3c6-decont.md) Felia 3c-6 — Decont
- [33](decizii/033-felia-3d-validare-transversala.md) Felia 3d — validarea transversală
- [36](decizii/036-felia-p1-tva-structural.md) Felia P1 — profil privat + TVA structural — parțial amendată de 71
- [37](decizii/037-design-p2-descarcare-gestiune.md) Design P2 — descărcarea de gestiune la FacturaIesire
- [41](decizii/041-restantele-40e-atlas-dxf-26-1-3-6.md) Felia „restanțele 40e
- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [46](decizii/046-felia-1c-a-tipurile-noi.md) Felia 1C-a — tipurile noi de model — parțial amendată de 113
- [48](decizii/048-analiza-pre-1c-c.md) Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție — parțial amendată de 76
- [49](decizii/049-felia-1c-c-documentele-prin-motor.md) Felia 1C-c — documentele 2025 prin motor
- [54](decizii/054-owned-vs-relational-invarianti-dimensiuni-pe-frunze.md) Sesiunea de arhitectură 2026-08-02
- [55](decizii/055-p5-spike1-btr.md) Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer
- [62](decizii/062-p5-felia5-nir-scriere.md) Pasul 5, felia 5 — NIR scriere
- [64](decizii/064-p5-felia7-virament-581.md) Pasul 5, felia 7 — viramentul intern — parțial amendată de 84
- [65](decizii/065-p5-felia8-decont-pereche.md) Pasul 5, felia 8 — Decont
- [75](decizii/075-p5-felia18-restante-s.md) Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție
- [76](decizii/076-p5-felia19-ntc-asm-retururi.md) Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client
- [82](decizii/082-stingere-automata-prin-contract.md) Stingerea automată prin contract de document
- [86](decizii/086-p5-felia25-dvi.md) Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/25 și 7/22, codurile SAF-T de import
- [87](decizii/087-p5-felia26-imobilizari.md) Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp — parțial amendată de 97
- [89](decizii/089-tph-discriminator-tipul-ca-data.md) Pasul 5, felia 28 — TPH cu discriminatorul mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat
- [90](decizii/090-nucleu-cub-de-postari.md) Nucleul ca ledger n-dimensional — un singur cub de postări în locul celor patru registre; motor pur pe operand închis (`Contract = Postari + Decizii + Ipoteze`); unitatea nominalizată (lot = partidă = fișă) numită pe linie; împerecherea ca nominalizare la operare + document `Împerechere` cu tranzacție de fel `Transfer`; linia FCT postează recepția; o singură postare de stoc, `TipStoc` dispare structural; stornoul ca tranzacție distinctă = inversul cauzat ∪ atribuit; fiscala postată în `Carte=Fiscal`; ordinea TR-D6a…D10 cu Import1C ca probă supremă; XAF și React înghețate pe funcții — parțial amendată de 91, 92, 93, 95, 96, 110
- [93](decizii/093-folosinta-gestiune-reala.md) Folosința păstrează gestiunea reală și lotul
- [95](decizii/095-producatori-completi-inaintea-citirilor.md) DEC și imobilizările complete pe cub înaintea TR-D8
- [97](decizii/097-pif-nominalizare-cu-suport-obligatoriu.md) PIF nominalizează valoarea contabilă cu suport obligatoriu — parțial amendată de 98
- [98](decizii/098-corecturi-review-sincronizare.md) Conexul editat postează delta; poziția fără fișă nu devine negativă; contul fișei vine din cub
- [100](decizii/100-partide-comerciale-bugetare.md) Partide comerciale în politica bugetară — parțial amendată de 102
- [101](decizii/101-imperechere-cu-efect-pe-partida.md) Împerecherea cere efect verificabil pe partidă — parțial amendată de 102
- [102](decizii/102-fara-compatibilitate-cu-bazele-de-dezvoltare.md) Fără compatibilitate cu bazele de dezvoltare până la prima producție
- [104](decizii/104-straturi-izolate-entitati-proprii.md) Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată — parțial amendată de 106
- [106](decizii/106-regim-pe-stare.md) Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile
- [109](decizii/109-taxa-nemarcata-pe-document-scrisa-pe-linie.md) Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei; taxa unui document real adusă de o cale fără culegere e marcată
- [112](decizii/112-receptia-facturii-pe-regula-ei.md) Recepția facturii se contează pe regula ei de contare (`FCT/Stoc`)
- [114](decizii/114-departajarea-fifo-pe-documentul-deschizator.md) Departajarea FIFO: data, documentul deschizător, apoi identificatorul — parțial amendată de 116
- [115](decizii/115-nota-numeste-partida.md) Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere
- [116](decizii/116-postarea-de-deschidere-departajeaza-unitatea-fara-document.md) Unitatea fără document se departajează pe postarea ei de deschidere; ID-urile postărilor urmează ordinea tranzacției — parțial amendată de 117
- [117](decizii/117-ordinalul-postarii-in-tranzactie.md) Postarea își poartă ordinalul în tranzacție; unitatea fără document se departajează pe el

### limite-curente (29)

- [37](decizii/037-design-p2-descarcare-gestiune.md) Design P2 — descărcarea de gestiune la FacturaIesire
- [41](decizii/041-restantele-40e-atlas-dxf-26-1-3-6.md) Felia „restanțele 40e
- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [43](decizii/043-design-p5-clientul-react.md) Design pasul 5 — clientul React — parțial amendată de 104
- [46](decizii/046-felia-1c-a-tipurile-noi.md) Felia 1C-a — tipurile noi de model — parțial amendată de 113
- [53](decizii/053-gate-xaf.md) GATE XAF
- [55](decizii/055-p5-spike1-btr.md) Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer
- [72](decizii/072-p5-felia15-partener-anaf.md) Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF
- [75](decizii/075-p5-felia18-restante-s.md) Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție
- [77](decizii/077-p5-felia20-finisaj-client.md) Pasul 5, felia 20 — finisajul clientului: căutarea fără diacritice, cache-ul de nomenclator, confirmările, `Neincluse` agregat, primele ecrane de nomenclator
- [80](decizii/080-refuzuri-acces-toate-usile.md) Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp
- [81](decizii/081-p5-felia23-implicite-politici.md) Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru
- [84](decizii/084-p5-felia24-politici-explica.md) Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul `Configurator`, cele șapte ecrane, potrivirea ca funcții pure și „Explică"
- [85](decizii/085-mod-acces-listview-xaf.md) Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): `Server` implicit cu paginare, `ServerView` opt-in pe registre, `Client` explicit pe grilele de culegere, IF/IFV doar cu prag măsurat
- [87](decizii/087-p5-felia26-imobilizari.md) Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp — parțial amendată de 97
- [89](decizii/089-tph-discriminator-tipul-ca-data.md) Pasul 5, felia 28 — TPH cu discriminatorul mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat
- [91](decizii/091-scenarii-in-loc-de-import.md) Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate; clona Flax rămâne sursă de întrebări; „rotund" primește definiție ca regulă de oprire a PoC-ului; stornoul după reevaluare = inversarea originalului + compensarea efectelor rămase active; proiecțiile persistate doar reconstruibile din postări; triajul restanțelor în patru stări — parțial amendată de 107, 118
- [94](decizii/094-deschidere-prin-detalierea-soldului.md) Deschiderea detaliază soldul contabil, fără dublare
- [95](decizii/095-producatori-completi-inaintea-citirilor.md) DEC și imobilizările complete pe cub înaintea TR-D8
- [97](decizii/097-pif-nominalizare-cu-suport-obligatoriu.md) PIF nominalizează valoarea contabilă cu suport obligatoriu — parțial amendată de 98
- [102](decizii/102-fara-compatibilitate-cu-bazele-de-dezvoltare.md) Fără compatibilitate cu bazele de dezvoltare până la prima producție
- [103](decizii/103-fapte-fiscale-istorice-si-perioade-distincte.md) Fapte fiscale istorice și perioade distincte de declarare
- [104](decizii/104-straturi-izolate-entitati-proprii.md) Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată — parțial amendată de 106
- [106](decizii/106-regim-pe-stare.md) Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile
- [107](decizii/107-deschiderea-de-terti-din-balanta-nivel3.md) Migrarea deschide terții per partidă din `BalantaNivel3`, prin `Materializare.Deschide`; stingerile anului pe partide inițiale; partener generic de migrare
- [108](decizii/108-gate-transversal-tr-d8.md) Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată — parțial amendată de 110
- [109](decizii/109-taxa-nemarcata-pe-document-scrisa-pe-linie.md) Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei; taxa unui document real adusă de o cale fără culegere e marcată
- [115](decizii/115-nota-numeste-partida.md) Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere
- [116](decizii/116-postarea-de-deschidere-departajeaza-unitatea-fara-document.md) Unitatea fără document se departajează pe postarea ei de deschidere; ID-urile postărilor urmează ordinea tranzacției — parțial amendată de 117

### politici-si-fiscalitate (21)

- [20](decizii/020-un-singur-nomenclator-de-tipuri.md) Un singur nomenclator de tipuri, și e codul
- [36](decizii/036-felia-p1-tva-structural.md) Felia P1 — profil privat + TVA structural — parțial amendată de 71
- [42](decizii/042-design-p5-tierul-api.md) Design pasul 5 — tierul API + React — parțial amendată de 104, 106
- [46](decizii/046-felia-1c-a-tipurile-noi.md) Felia 1C-a — tipurile noi de model — parțial amendată de 113
- [56](decizii/056-p5-felia2-fct-nir.md) Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API — parțial amendată de 77, 81
- [66](decizii/066-p5-felia9-raportare.md) Pasul 5, felia 9 — raportarea pe registre
- [69](decizii/069-p5-felia12-d300.md) Pasul 5, felia 12 — D300, decontul de TVA ca proiecție peste `RegistruTva`
- [70](decizii/070-p5-felia13-motor-structura.md) Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)
- [71](decizii/071-p5-felia14-d394.md) Pasul 5, felia 14 — D394, declarația informativă ca proiecție peste `RegistruTva` — parțial amendată de 72
- [72](decizii/072-p5-felia15-partener-anaf.md) Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF
- [73](decizii/073-p5-felia16-saft.md) Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre — parțial amendată de 74, 105
- [74](decizii/074-p5-felia17-saft-s.md) Pasul 5, felia 17 — SAF-T D406 **S (stocuri)** peste `RegistruStoc` — parțial amendată de 105
- [79](decizii/079-p5-felia21-itv.md) Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client: comandă cu cauză + ecran de rezultat — parțial amendată de 80
- [80](decizii/080-refuzuri-acces-toate-usile.md) Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp
- [81](decizii/081-p5-felia23-implicite-politici.md) Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru
- [83](decizii/083-seed-provenienta-goluri-configurator.md) Seed-ul și proveniența: re-seed-ul corectează doar `DinSeed`, golurile privat (NIM / IMP cu poarta spre DVI), rolul `Configurator` — parțial amendată de 84, 86, 104
- [84](decizii/084-p5-felia24-politici-explica.md) Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul `Configurator`, cele șapte ecrane, potrivirea ca funcții pure și „Explică"
- [86](decizii/086-p5-felia25-dvi.md) Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/25 și 7/22, codurile SAF-T de import
- [87](decizii/087-p5-felia26-imobilizari.md) Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp — parțial amendată de 97
- [103](decizii/103-fapte-fiscale-istorice-si-perioade-distincte.md) Fapte fiscale istorice și perioade distincte de declarare
- [113](decizii/113-semnul-e-al-liniei-pastreazasemn-scos.md) Semnul valorii e al liniei; `RegulaContare.PastreazaSemn` a ieșit

### (fără arie) (32)

- [1](decizii/001-nucleu-generic-mostenire.md) Nucleu generic + moștenire, nu tabele complet separate
- [2](decizii/002-testul-apartenentei-campului.md) Testul de apartenență a unui câmp
- [3](decizii/003-mapare-ef-core-tpt.md) Mapare EF Core — parțial amendată de 89
- [4](decizii/004-structura-cod-politica-date.md) Structura devine cod, politica rămâne date
- [5](decizii/005-backend-xaf-frontend-react.md) Backend
- [6](decizii/006-api-fara-ierarhie-polimorfa.md) API-ul NU expune ierarhia polimorfic
- [7](decizii/007-fk-explicite-jsonignore.md) Nav properties în Web API
- [8](decizii/008-metadata-driven-ui.md) Metadata-driven UI
- [9](decizii/009-scope-faza-1.md) Scope faza 1
- [11](decizii/011-sursa-finantare-dimensiune-explicita.md) Dimensiuni pe note
- [12](decizii/012-migrare-green-field.md) Migrare green-field la graniță de ciclu
- [18](decizii/018-migrarea-preia-nomenclatoare-solduri.md) Migrarea preia nomenclatoare + solduri de deschidere
- [19](decizii/019-lista-tipurilor-de-documente.md) Lista țintă de tipuri de documente
- [24](decizii/024-owned-dimensiuni-sub-xaf.md) Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem pe EF Core
- [28](decizii/028-felia-3c3-lista-diferente-inventar.md) Felia 3c-3 — ListaDiferenteInventar
- [29](decizii/029-profil-contabil-privat-first.md) Profil contabil
- [35](decizii/035-pivot-privat-first.md) Pivot privat-first
- [38](decizii/038-felia-p2-descarcare-gestiune.md) Felia P2 — DescarcareGestiune — parțial amendată de 81, 84
- [40](decizii/040-felia-polish-xaf.md) Felia polish XAF
- [44](decizii/044-reordonare-roadmap-1c-inainte-de-p5.md) Reordonarea roadmap-ului
- [51](decizii/051-analiza-post-gate-1c-d-rotunjire-cmp.md) Analiza post-gate 1C-d
- [52](decizii/052-felia-1c-d-final.md) Felia 1C-d-final
- [59](decizii/059-p5-perf-masuratori.md) Măsurarea de perf a proiecțiilor pe baza de import
- [61](decizii/061-p5-restante-f3-f4.md) Restanțele §Închidere F3/F4 golite
- [63](decizii/063-p5-felia6-ldi-bcs.md) Pasul 5, felia 6 — LDI + BCS prin API și client
- [67](decizii/067-p5-felia10-balanta-pliata.md) Pasul 5, felia 10 — balanța pliată pe planul de conturi
- [78](decizii/078-cautare-fara-diacritice-proiectii.md) Căutarea fără diacritice pe proiecții — rescrierea predicatelor `DataSourceLoader` prin normalizatorul comun
- [88](decizii/088-p5-felia27-perioade-solduri.md) Pasul 5, felia 27 — Perioada fiscală ca lanț, închiderea ca comandă cu acceptare conștientă, soldurile și partidele deschise materializate la închidere, data înregistrării ca reper al registrelor, perioada de declarare ca reper fiscal, corecția legată cu motiv — parțial amendată de 103
- [92](decizii/092-identitatea-partidei-include-partenerul.md) Identitatea partidei include partenerul — parțial amendată de 102
- [96](decizii/096-urmarirea-partidelor-separata-de-rolul-comercial.md) Urmărirea partidelor este separată de rolul comercial — parțial amendată de 100, 102
- [99](decizii/099-cauza-diferentei-evidenta-constatare.md) Cauza diferenței dintre evidență și constatarea fizică
- [105](decizii/105-saft-pe-cub.md) SAF-T D406 pe cub: sursă unică, acces complet, certificare cu manifest

## Decizii depășite

Niciuna.

## Restanțe deschise, pe stare

| Stare | Câte |
|---|---|
| activă | 41 |
| după PoC | 205 |
| migrare | 24 |
| depășită de 091 | 2 |
| docs | 6 |
| (în afara vocabularului) | 5 |

### Lista `activă` (41, prag 45)

- `51e` `PoliticaEvaluare` (CMP) — sub 90c devine parametrul „unitatea de evaluare” al unității nominalizate
- `75-r4` `PretEvaluare` 6 zecimale pe cantități mari vs invariantul 46d
- `76-r1` netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei / 36 note amestecă conturi de clasă 4 pe aceeași latură; expunere reală 3 chei)
- `76-r2` `caStins` se scade din AMBELE sensuri — inatacabil azi, **devine real când un tip cu partener pe latură capătă capacitate bidirecțională**
- `76-r3` perf `AsignatFataDe` (entități polimorfe, chemat de 2 × nr. contrapartide); hook-urile de stingere dispar sub 90e
- `86-r1` taxele vamale și accizele în costul de achiziție (ajustare de cost pe lot)
- `86-r2` anularea/stornarea unei FCT legate la o DVI operată nu se refuză
- `86-r11` DVI ca document stins: restul polimorf în `ImperechereService.Total` + ramura în `DocumenteCuRest`
- `F26-r18` anularea unui eveniment PIF din luna unei AMO operate e refuzată deși rândul lunar nu depinde de el (`Data >=` vs „luna >") — refuz fals, pe partea sigură (87)
- `F27-r4` închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88)
- `F27-r16` forma proiecției `DocumenteCuRest`: candidații de la 59 (uniunea tuturor documentelor operate) și forma legăturilor se rezolvă ÎMPREUNĂ — corelarea legăturii duce panoul filtrat la 82 ms, dar calea neplafonată a constatării de rest scadent de la 220 ms la 1,02 s (respinsă motivat, cu cifre; niciun index nu lipsește) (88)
- `F28-r1` simplificarea `CandidatiPereche<T, TOpus>` și a uniunii per tip din `ImperecheriProiectii` (jumătatea „tip” rezolvată de discriminator, „contrapartida per tip” e semantică); împreună cu F27-r16 (89); `ImperecheriProiectii` dispare sub 90e
- `TR-r7` sink-urile bugetare Gratuit/Custodie și injectivitatea SAF-T rămân deschise; Folosință păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI (093, LDI-B3)
- `TR-r8` rulajul brut per partidă nu e sumă sub tranzacția de transfer; fișa partidei se randează ca fereastră (90)
- `TR-r10` deschiderea generică fără document, cu loturi și partide; detalierea înlocuiește soldul bloc și refuză diferențele (094, DES-B1…B4). Conectorul 1C rămâne separat în 091-r4/T-r4
- `TR-r11` `Numar`, `DataScadenta`, `Autogenerat`, `DocumentSursa` rămân atribute ale documentului scrise la operare, sub gardianul (a) (90)
- `N-r5` stornoul unei postări ATRIBUITE (reevaluare) intră în selecție (090i) și poartă cauza altui document, dar contrapartida ei inversată e nedefinită ⇒ tranzacția de storno nu conservă valoarea; se tranșează la TR-D9 odată cu reevaluarea (TR-D6a)
- `N-r6` amendamente de literă ale contractului TR-D6a consemnate în cod: cazurile `Decizie` poartă `Linie`; `Motor.Transfera` ia `Mutare`, nu `Declaratie` (fără decizii/ipoteze pe transfer); ierarhia închisă ține prin constructor `private protected`, copy-constructorul rămâne `protected` (CS8878); `Unitate with { Fel }` și `Declaratie with { Document, Miscari }` nu se re-validează inter-câmp — se construiesc din nou (TR-D6a)
- `N-r7` ordinea FIFO a nucleului e `Guid.CompareTo` (componentă cu componentă) prin `Fifo.Intai`; `ORDER BY uuid` în Postgres are altă ordine ⇒ la TR-D7 candidații se sortează în nucleu, nu în SQL, altfel „ultima ia restul” cade pe alt lot la date egale (TR-D6a)
- `B-r6` `Valuta`/`Curs` sunt câmpuri de frunză pe `FacturaIntrare` fără interfață declarată: operandul le lasă `null`; `IDocumentCuValuta` la primul consumator real (partida în valută, TR-D9) (TR-D6b)
- `B-r8` gestiunile virtuale (`GestiuniVirtuale.Furnizor/Client/Consum`) sunt constante deterministe fără rând de nomenclator: devin rânduri `DinSeed` cu aceleași id-uri dacă un raport le cere nume (TR-D7/D8) (TR-D6b)
- `S-r3` gestiunile virtuale n-au FK pe `Postare` (id-uri fără rând de nomenclator); B-r8 le face rânduri `DinSeed` și atunci FK-ul pe `Gestiune` intră pe ambele partiții (TR-D7a)
- `S-r4` snapshot-ul EF declară cheia `ID` și `FK_Postare_Tranzactie_TranzactieId` pe părinte, baza are cheia `(Spatiu, ID)` și FK-uri per partiție: divergență DECLARATĂ cât timp XAF EF Core nu suportă chei compuse, probată de `STR-SCHEMA` — orice migrație viitoare pe `Postare`/`Tranzactie` se scrie în SQL (TR-D7a)
- `S-r7` un lot născut de o linie „în roșu” (valoare negativă, cantitate pozitivă) se evaluează negativ tăcut, ca în motorul vechi: invariantul „un lot nu se evaluează negativ” e pierdut odată cu admiterea semnului în `Operare` (TR-D7a)
- `S-r8` dry-run-ul nu fixează prețul și data lotului născut de document, deci contractul lui diferă de cel real pe `Unitate.Deschisa`; fără refuz fals azi (TR-D7a)
- `T-r2` reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j); se rezolvă la TR-D9 prin re-evaluarea unității cu reziduul spre 658/758 din politică (TR-D7b)
- `T-r5` DVI deschide partidă pe contrapartida cu RolTert (401 probat; 446 numai dacă este configurat astfel), deși `PoateFiStins = false`; hook-ul e al registrelor până la TR-D9 (TR-D7b, DVI-B3)
- `T-r6` `Custodie` pe bugetar: explicit neacoperită la LDI; de definit contul politicii dintre cele zece 803x și perechea cantitativă/valorică (093d, LDI-B3)
- `091-r2` recensământul pe clona Flax per tip rămas (NTC, ITV, RDC, RLF, ASM, LDI, NIR), o dată, înaintea scenariilor lui; cifrele în coloana „Proveniență” a fișierului tipului (091)
- `107-r4` valuta: partidele inițiale în valută cu curs propriu (azi în lei, plafonate la rest la stingere) (107)
- `107-r6` performanța Import1C: 25 → 50 min/lună pe Flax față de 8–10 la baseline 2026-09-21 (~5×); după gate-ul TR-D8 ianuarie durează 21:27 (2026-10-04, ~2,5× baseline), creșterea pe luni neremăsurată; scara X-D5 nu acoperă calea importului, de profilat înaintea oricărei rulări integrale (107)
- `095-r1` DEC → contract IMO → PIF/AMO/CAS complete pe cub, inclusiv cartea fiscală și istoricul fișei, înaintea TR-D8 (095), PIF cu suport obligatoriu aprobat (097); urmărirea partidelor separată de RolTert (096)
- `097-r2` auditul anulării fizice a suportului eliberat; proveniența fără FK restrictiv (097, 091j)
- `099-r1` LDI adoptă cauza diferenței: minus imputabil pe partener, perisabilitate, neimputabilă; plus (099)
- `103-r2` Intervale TVA și avertismente/raport de impact, cu referință la linia avansului (103h); contract `tr-d8-tva-intervale-contract.md`, SC-CIT-90…94; delimitare rămasă: Proveniența fiscală a ajustărilor RDC/RLF/reduceri (R6-B4)
- `103-r3` Validarea L3 a semnului taxei culese față de baza FCT/FCL și înlocuirea proxy-ului `ILinieCuAvans` pentru linia de factură; detalii în 103
- `106-r5` StateMachine și ViewVariants scoase din `Startup.cs` și `RequiredModuleTypes`; `HCategory` scos din `DbContext` (106g) — partea `HCategory` e făcută la TR-D9a pasul 7 (2026-10-07)
- `X-r3` Raportul de stoc listează capătul de consum al BCS (postarea de debit poartă lotul ca unitate, pe contul de cheltuială); semantica raportului e întrebare pentru owner (108)
- `NG-r1` Balanța și fișa contului filtrate pe gestiune sau pe material nu văd transferurile (BTR, ASM): valoarea rămâne la sursă. Citit din cod, nerulat
- `NG-r2` Contul lotului vine din tipul curent de material al produsului; schimbarea tipului după recepție poate da refuz de stoc insuficient cu stoc existent. Dedus, nedemonstrat
- `118-r2` Unealta `docskit` (repo propriu, dotnet global tool): `gen`, `check` întâi, `build`, `metrics` după; proba pe linkurile cu ancoră

## Felii deschise

Niciun contract deschis declarat cu `Tip: contract`.

## Decizii invocate din cod

| Decizia | Citări | Fișiere |
|---|---|---|
| 1 | 3 | 3 |
| 3 | 2 | 2 |
| 4 | 1 | 1 |
| 5 | 1 | 1 |
| 9 | 1 | 1 |
| 10 | 2 | 2 |
| 13 | 1 | 1 |
| 14 | 2 | 2 |
| 15 | 2 | 2 |
| 17 | 1 | 1 |
| 22 | 1 | 1 |
| 23 | 2 | 2 |
| 24 | 1 | 1 |
| 25 | 1 | 1 |
| 26 | 4 | 4 |
| 28 | 1 | 1 |
| 31 | 1 | 1 |
| 33 | 1 | 1 |
| 34 | 1 | 1 |
| 35 | 1 | 1 |
| 36 | 1 | 1 |
| 38 | 3 | 3 |
| 41 | 2 | 2 |
| 42 | 3 | 3 |
| 43 | 1 | 1 |
| 46 | 2 | 2 |
| 48 | 2 | 2 |
| 49 | 1 | 1 |
| 51 | 1 | 1 |
| 53 | 1 | 1 |
| 55 | 1 | 1 |
| 57 | 6 | 6 |
| 58 | 1 | 1 |
| 63 | 1 | 1 |
| 64 | 1 | 1 |
| 72 | 1 | 1 |
| 73 | 1 | 1 |
| 74 | 1 | 1 |
| 77 | 4 | 4 |
| 79 | 4 | 4 |
| 82 | 3 | 3 |
| 83 | 4 | 3 |
| 85 | 4 | 4 |
| 86 | 1 | 1 |
| 87 | 2 | 2 |
| 89 | 4 | 3 |
| 90 | 4 | 3 |
| 91 | 3 | 2 |
| 92 | 1 | 1 |
| 97 | 1 | 1 |
| 98 | 2 | 2 |
| 99 | 2 | 2 |
| 101 | 1 | 1 |
| 102 | 1 | 1 |
| 103 | 2 | 2 |
| 104 | 7 | 6 |
| 106 | 1 | 1 |
| 107 | 7 | 5 |
| 109 | 1 | 1 |
| 112 | 1 | 1 |
| 115 | 5 | 5 |
| 117 | 1 | 1 |

## Paginile stării curente

| Pagina | Actualizat |
|---|---|
| [README](stare-curenta/README.md) | 2026-10-10 |
| [api-si-client](stare-curenta/api-si-client.md) | 2026-10-10 |
| [dezvoltare-si-validare](stare-curenta/dezvoltare-si-validare.md) | 2026-10-10 |
| [domeniu-si-operare](stare-curenta/domeniu-si-operare.md) | 2026-10-10 |
| [limite-curente](stare-curenta/limite-curente.md) | 2026-10-10 |
| [politici-si-fiscalitate](stare-curenta/politici-si-fiscalitate.md) | 2026-10-10 |
| [termeni](stare-curenta/termeni.md) | 2026-10-10 |

## Ce nu poate ști

Starea mediilor și a bazelor, ramurile nemerse în main, ce e în lucru necomis, și dacă o regulă scrisă chiar e implementată: astea le spun probele, nu docs.
