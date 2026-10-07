**Recomand forma 5, cu separarea explicită a repartitorului de sold, a partenerului fiscal și a rolului cantitativ.** Forma 3 rămâne baza semantică; perechea aduce o corespondență contabilă utilă, dar nu înlocuiește contextul evenimentului. Nu recomand materializarea generală a celor două capete din forma 4 înainte de demonstrarea unui consumator care le justifică. [judecată]

Am făcut numai citiri și căutări, fără modificări, build, ModelCheck sau acces la baze. Folosesc prefixele `M` și `N` din fișă; suplimentar, `D=M/Declaratii/`, `S=M/DatabaseUpdate/SeedData/`. Propunerile și rezultatele exemplelor construite sunt marcate `[judecată]`.

Verdictele sunt:

| Formă | Verdict | Motiv |
|---|---|---|
| **1** | **Se respinge ca țintă.** | Leagă în unele declarații identitatea terțului de existența partidei și amestecă partenerul fiscal cu analiticul. Contraexemplele 1 și 3 de mai jos. |
| **1b** | **Se acceptă cu schimbări**, ca tranziție. | Repară terțul fără partidă, dar trebuie separată cheia soldului de fiscal și de gestiunile virtuale. |
| **2** | **Se respinge dacă ambele coordonate propagate definesc soldul.** | Factură pe `(A,G)`, plată pe `(A,B)` lasă două poziții opuse. Ca atribute de eveniment, cu proiecție distinctă pentru sold, poate funcționa; respingerea nu este universală. |
| **3** | **Se acceptă cu schimbări.** | Repartitor tipizat semantic, politică de completare explicită, fiscal separat și rol cantitativ. Nu rezolvă automat coordonate independente viitoare. |
| **4** | **Se acceptă cu schimbări.** | Gruparea trebuie scrisă explicit; capetele pot fi tehnice, identice, absente sau din afara antetului. Fiscalul rămâne separat. |
| **5** | **Se acceptă cu schimbări; recomandată.** | Forma 3 plus pereche, fără pereche în solduri și fără promisiunea recuperării întregului eveniment. |

Verdictele sunt `[judecată]`; mecanismele relevante sunt `D/Partide.cs:54`, `D/Fiscal.cs:68`, `M/Motor/SolduriService.cs:72` și `docs/nucleu/tr-d9-taierea-amendament-1.md:57`.

Față de runda 1, schimb ordinea **3–5 în 5–3**: există un consumator concret pentru corespondență, fișa contului, care astăzi caută toate conturile de pe sensul opus și poate întoarce contrapartidă nulă (`M/Proiectii/ContabilProiectii.cs:486`). Îmi corectez și exemplele: BCS păstrează lotul pe cheltuială; ASM nu postează obligatoriu prin 601/711; deschiderea nu inventează contraponderi cantitative (`D/DeclarantBonConsum.cs:88`; `N/Motor/Transformare.cs:27`; `M/Cub/Materializare.Deschidere.cs:92`).

La cele șapte fapte care mi-ar fi schimbat verdictul în runda 1 (`docs/consultare/runda1-proprie.md:159`), răspunsurile sunt:

1. **Partener în analiticul contului:** nu există un asemenea mecanism structural în `Cont`; planul declară derivarea analiticelor din dimensiuni (`M/BusinessObjects/Nomenclatoare/PlanConturi.cs:7`).
2. **Două coordonate independente:** cele 22.389 de postări cu ambele coloane sunt raportate ca fiscal+gestiune sau partener+gestiune virtuală, nu demonstrează două identități independente de sold. Custodia rămâne în afara acoperirii (`docs/consultare/fapte.md:66`; `docs/nucleu/tr-d9-taierea-amendament-1.md:210`).
3. **Context integral și imuabil în documente:** există acces prin document, dar nu am demonstrat suficiența lui pentru toate citirile istorice; politica păstrează doar identitatea și versiunea, nu conținutul (`docs/nucleu/tr-d9-taierea-amendament-1.md:167`). `[judecată]` Nu justific deducerea retrospectivă.
4. **Filtre de flux folosite intens:** fișa numără referințe în cod, nu frecvență sau latență (`docs/consultare/fapte.md:79`). Necunoscut.
5. **Perechi suficiente:** nu; perechea TVA–401 nu conține obligatoriu gestiunea recepției, iar transformările rămân fără pereche (`D/Fiscal.cs:136`; `docs/nucleu/tr-d9-taierea-amendament-1.md:61`).
6. **Costul migrării:** resetul actual nu cere migrare de date; costul rescrierii cititorilor rămâne nemăsurat (`docs/nucleu/tr-d9-taierea-contract.md:458`).
7. **Fiscal/context excluse din sold:** fals pentru snapshot-ul contabil; adevărat parțial pentru proiecțiile specializate de lot și partidă (`M/Motor/SolduriService.cs:72`, `:135`, `:268`).

În nucleu, clasific regulile astfel:

| Regula actuală | Verdict semantic |
|---|---|
| `Capat.Pe` copiază separat `Partener/Gestiune`; `Miscare` produce debitul din `La`, creditul din `DeLa` (`N/Cub/Capat.cs:16`; `N/Cub/Miscare.cs:11`). | Fidelitatea coordonatelor și orientarea mișcării rămân; cele două coloane sunt reprezentare. `[judecată]` |
| Cantitatea cere gestiune și produs; fără unitate este admisă doar într-o gestiune virtuală (`N/Conservare.cs:139`). | Produsul și distincția evidență/exterior rămân. Obligativitatea coloanei `Gestiune` pentru orice contracantitate este artefact. `[judecată]` |
| Lotul cere produs, gestiune și concordanță cu unitatea (`N/Conservare.cs:160`). | Identitatea lotului, contul, produsul și poziția de evidență rămân; numele coloanei nu. `[judecată]` |
| Partida cere partener egal cu titularul ei (`N/Conservare.cs:178`). | Principiu: titularul nu poate fi schimbat printr-o postare. Se verifică împotriva repartitorului de sold, niciodată a celui fiscal. `[judecată]` |
| Transformarea folosește gestiune virtuală, contrapondere fără unitate, valoare, partener, TVA sau valută (`N/Conservare.cs:26`). | Contraponderea pur cantitativă și conservarea pe cont–produs–cauză rămân. GUID-ul virtual și testul `Partener==null` sunt artefacte; restricțiile fiscale/valutare delimitează mecanismul actual. `[judecată]` |

`Coordonate` și `Postare` nu adaugă alte validări asupra acestor două câmpuri (`N/Cub/Coordonate.cs:7`; `N/Cub/Postare.cs:14`). Nucleul ar trebui să recunoască **poziția de evidență, capătul exterior și contraponderea transformării**, nu furnizori sau consumuri deghizate în gestiuni. `[judecată]`

Pentru declaranți, notez `P`=partener, `G`=gestiune/loc/cont propriu real, `Vx`=gestiune virtuală, `R`=grupare. În coloana formei 4, săgeata indică roluri nominale; stornoul schimbă măsurile. `†`=capăt posibil absent din antet; `‡`=debit/credit nu identifică suficient capetele. Toată coloana propusă este `[judecată]`.

| Declarant | Ce poartă azi picioarele | Forma 4: ieșire → intrare; grupare |
|---|---|---|
| FacturaIntrare | Intern `G+P fiscal`; terț `P` cu partidă, plus `VFurnizor` la stoc; taxa `G+P fiscal` (`D/DeclarantFacturaIntrare.cs:139`; `D/Fiscal.cs:120`). | Furnizor→G; R=G pe intern, furnizor pe datorie, nul pe TVA. |
| FacturaIesire | Venit `G+P fiscal`; creanță `P` cu partidă; TVA după politica fiscală (`D/DeclarantFacturaIesire.cs:95`). | G→client; R=G/client, nul pe TVA. |
| Trezorerie | Plata: G numai pe piciorul propriu, P pe partida terțului; viramentul: același cont propriu pe ambele picioare ale etapei (`D/DeclarantTrezorerie.cs:158`). | Bancă→terț sau invers; virament B1→B2, R=B pe 5121, nul pe 581. |
| BonConsum | G și lot pe **ambele** picioare, fără P (`D/DeclarantBonConsum.cs:75`). | G→loc consum; R propriu fiecărui picior. |
| NotaTransfer | G1/G2, același lot, ambele postări debit (`D/DeclarantNotaTransfer.cs:73`). | G1→G2; R=G1/G2. ‡ |
| NotaContabila | G pentru intern/propriu; P numai pentru extern explicit; partide condiționate (`D/DeclarantNotaContabila.cs:54`). | Repartitor credit→repartitor debit, explicit pe linie; R propriu. † |
| Decont | Fie P, fie G după repartitorul rezolvat; fiscalul poate suprascrie P; creditul taxei revine titularului (`D/DeclarantDecont.cs:46`, `:62`, `:85`). | Repartitor credit→repartitor debit; implicit pot coincide; fiscal separat. †‡ |
| Nir | G+lot versus VFurnizor și P condiționat de partidă (`D/DeclarantNir.cs:48`). | Furnizor→G; R=furnizor/G. |
| Nir.Diferenta | Stoc G; diferență VInventar, eventual P=furnizor sau imputat (`D/DeclarantNir.Diferenta.cs:67`). | Plus: furnizor/exterior→G; lipsă: G→imputat/exterior; R propriu. † |
| ReturFurnizor | Stoc G+P fiscal versus VFurnizor+P cu partidă; valori semnate (`D/DeclarantReturFurnizor.cs:48`). | Furnizor→G pentru inversarea recepției; R=furnizor/G. |
| ReturClient | Cost: G+lot versus VClient+P; venit/taxă: fiscal și partidă (`D/DeclarantReturClient.cs:53`). | G→client, cu măsuri inverse; R propriu. |
| DescarcareGestiune | G+lot versus VClient+P extern (`D/DeclarantDescarcareGestiune.cs:74`). | G→client; R=G/client, dacă analiza costului cere clientul. |
| DiferenteInventar | G+lot versus VInventar/VConsum, fără P (`D/DeclarantDiferenteInventar.cs:80`). | Exterior→G sau G→exterior; comisia nu este destinația bunului. † |
| Asamblare | Loturi în G predător; contraponderi VTransformare (`D/DeclarantAsamblare.cs:112`; `N/Motor/Transformare.cs:30`). | G→transformare→G, relație de grup; R=G pe real, nul pe tehnic. †‡ |
| Imobilizari | G din fișă pe notele ordinare; PIF copiază coordonatele suportului, adăugând fișa (`D/DeclarantImobilizari.cs:38`, `:90`). | Loc→același loc; nominalizare suport→fișă; R păstrat. †‡ |

Forma 4 nu poate fi definită drept „cele două laturi ale antetului copiate pe fiecare postare”. Tabelul conține contraexemple implementate. `[judecată]`

Clasificarea cititorilor și impactul formelor 3/4/5 sunt:

| Folosire actuală | Sens | Schimbarea necesară |
|---|---|---|
| `ContabilProiectii` și snapshot contabil (`M/Proiectii/ContabilProiectii.cs:190`; `M/Motor/SolduriService.cs:72`) | Cheie de sold și filtre analitice | Toate trei trebuie portate pe R/Grupare. Se elimină fragmentarea fiscală; filtrele vechi își schimbă sensul. |
| `Partide`, `Plati`, `ImperecheriProiectii`, `Transferuri` (`M/Cub/Citiri/Partide.cs:150`; `M/Cub/Citiri/Plati.cs:131`; `M/Proiectii/ImperecheriProiectii.cs:175`; `M/Cub/Transferuri.cs:61`) | Identitate de partidă și titular | R înlocuiește P fără schimbarea hash-ului partidei. Perechea nu înlocuiește partida. |
| `Loturi`, `StocProiectii`, SAF-T stoc (`M/Cub/Citiri/Loturi.cs:21`; `M/Proiectii/StocProiectii.cs:33`; `M/Saft/SaftProiectii.PeCub.Stocuri.cs:115`) | Poziție de lot | G devine R pe pozițiile de lot; rolul exclude exteriorul. |
| `Fiscale`, D394, TVA, invarianții fiscali (`M/Cub/Citiri/Fiscale.cs:74`; `M/Proiectii/D394Proiectii.cs:160`; `M/Proiectii/TvaProiectii.cs:98`; `M/Cub/Citiri/Invarianti.cs:117`) | Fapt fiscal | Citesc exclusiv `PartenerFiscalId`; deducerea prin capete/pereche ar pierde cazuri. |
| SAF-T terți și plăți (`M/Saft/SaftProiectii.PeCub.cs:185`; `M/Saft/SaftProiectii.PeCub.Plati.cs:95`) | Sold de terț plus atribut de eveniment | Rolul comercial rămâne separat; „mai mulți parteneri” trebuie verificat pe titularii contrapartidei, nu pe orice R. |
| `Randuri`, deschidere, explicații (`M/Cub/Randuri.cs:109`; `M/Cub/Materializare.Deschidere.cs:92`; `M/Cub/Citiri/Explicatii.cs:147`) | Transportul identității de partidă/lot | Mapări portate explicit, fără coalescență fiscală. |
| `GardAnaliza`, `Transformare`, `ReceptiiConexe` (`M/Cub/GardAnaliza.cs:52`; `M/Cub/Citiri/Transformare.cs:8`; `M/Cub/ReceptiiConexe.cs:96`) | Obligativitate analitică, rol tehnic, poziție istorică de lot | Gard pe R; rol tehnic explicit; coordonatele istorice păstrate. |

Impactul propus este `[judecată]`. Forma 3 simplifică soldul; forma 4 simplifică interogările directe de flux, cu redundanță; forma 5 simplifică identificarea contrapărții, cu join. Niciuna nu justifică schimbarea automată a sensului filtrelor existente.

Snapshot-ul contabil are **cont + nouă dimensiuni**, între care P și G; `UnitateId` de aici este unitatea organizatorică, nu lotul/partida (`M/Motor/SolduriService.cs:71`, `:227`). Fie K celelalte șapte dimensiuni plus cont, în cadrul perioadei.

| Formă | Cheie contabilă | Cardinalitate |
|---|---|---|
| 1 | K+P+G | Combinațiile observate actuale. |
| 1b | K+P+G | Poate crește prin nominalizarea terților anterior nuli. |
| 2 | K+P+G propagat | Poate fragmenta după participanții evenimentului; nu obligatoriu produs cartezian. |
| 3 | K+R | Elimină separările numai fiscale/virtuale; poate adăuga terții lipsă. |
| 4 | K+Grupare | Aceeași cardinalitate semantică ca 3; capetele nu intră. |
| 5 | K+R | Ca 3; perechea nu intră. |

Predicțiile sunt `[judecată]`. Snapshot-ul de stoc rămâne semantic `(lot,cont,produs,poziție)`; cel de partide `(unitate,cont,titular)` (`M/Motor/SolduriService.cs:135`, `:268`). Cele 81.836 de postări nu permit calcularea numărului noilor grupe (`docs/consultare/fapte.md:48`). În plus, citirea contabilă exclude transferurile: schimbarea cheii singură nu transformă balanța filtrată pe gestiune într-o poziție curentă de stoc (`M/Cub/Citiri/Contabil.cs:42`). `[judecată]`

**Partenerul fiscal poate și trebuie separat**, fixat la postare. Astăzi `CuFapt` îl ia din antet și suprascrie `Partener`; nu are prioritate fiscală de linie (`D/Fiscal.cs:68`). DEC folosește predătorul, inclusiv titularul angajat; D394 raportează repartitorii care nu sunt `Partener` ca neincluși (`M/DatabaseUpdate/ProfilPrivat.cs:519`; `M/Proiectii/D394Proiectii.cs:256`). Mutarea coloanei nu inventează furnizorul lipsă: trebuie și culegerea lui. `[judecată]`

Stornoul păstrează coordonatele și inversează măsurile; corecția creează un draft nou; inversa tehnică schimbă astăzi perioadele și marcajul înaintea commit-ului (`N/Cub/Storno.cs:13`; `M/Motor/CorectieService.cs:72`; `M/Cub/Materializare.cs:213`). În noua formă, inversa păstrează **partenerul fiscal original**, iar corecția A→B înregistrează minus la A și plus la B. Atribuirile D300/D394 rămân distincte. `[judecată]`

Invariantul inversei verifică tipul, rolul, sensul, regimul, cota, importul, documentul fiscal și suma, **dar nu partenerul** (`M/Cub/Citiri/Invarianti.cs:127`). Trebuie întărit. D394 citește și calificarea curentă a partenerului; simpla separare a GUID-ului nu rezolvă reproducerea istorică integrală (`M/Proiectii/D394Proiectii.cs:175`). `[judecată]`

Următoarele cazuri sunt construite `[judecată]`. Notația actuală este `cont(P,G)[unitate]`; în forma recomandată scriu `cont/R`, păstrând unitatea și măsurile. TVA=21 este doar parametrul exemplului.

| Caz | Postările actuale → forma recomandată; rezultat/risc |
|---|---|
| **1. Recepție, plată, descărcare** | D371(A,G)[L] 100, q+10; C401(A,VF)[p] 100, q−10; D4426(A,G) 21; C401(A,∅)[p] 21; plata D401(A,∅)[p]121/C5121(∅,B)121; descărcare D607(C,VC)100,q+10/C371(∅,G)[L]100,q−10. → R: G/A/∅/A/A/B/C/G; fiscal A pe bază și taxă. Azi 371 analitic: +100/A, −100/nul; R=G închide poziția. 401 total/A se închide deja, dar snapshot-ul păstrează fragmentarea după VF. Surse: `D/DeclarantFacturaIntrare.cs:139`; `D/Fiscal.cs:120`; `D/DeclarantDescarcareGestiune.cs:74`. |
| **2. Transfer 4 bucăți, 40 lei** | D371(∅,G1)[L]−40,q−4; D371(∅,G2)[L]+40,q+4 → R=G1/G2. Soldurile de lot coincid. Deducerea destinației numai din debit e imposibilă. `D/DeclarantNotaTransfer.cs:79`; `N/Motor/Mutare.cs:21`. |
| **3. Terți fără partide** | Două servicii nefiscale: D628(∅,G)100/C401.02.00(∅,∅)100 pentru A și analog 200 pentru B → credite R=A100, R=B200. Azi datoria 300 nu distinge creditorii. `D/DeclarantFacturaIntrare.cs:157`; `D/Partide.cs:54`; `S/plan-conturi.csv:1150`. |
| **4. Compensare explicită** | D401(A,∅)[pA]70/C4111(B,∅)[pB]70 → R=A/B. Soldurile respective se sting; un singur „partener al documentului” aplicat ambelor picioare stinge persoana greșită. `D/DeclarantNotaContabila.cs:35`. |
| **5. Virament 100** | D581(∅,B1)100/C5121(∅,B1)100; D5121(∅,B2)100/C581(∅,B2)100 → R581=∅, R5121=B1/B2. Totalul 581 este zero în ambele forme; filtrul actual pe B1/B2 arată ±100. `D/DeclarantTrezorerie.cs:161`. |
| **6. Decont E, furnizor S** | Cu U explicit pe debit: D628(E,U)100/C542(E,∅)[p]100; D4426(E,U)21/C542(E,∅)[p]21 → R=U/E/∅/E, fiscal S. Soldul 542 nu se schimbă; raportarea fiscală se schimbă justificat numai dacă S este cules. Perechea nu îl poate inventa. `D/DeclarantDecont.cs:46`; `D/Fiscal.cs:68`. |
| **7. Transformare între conturi** | C301(∅,G)[L1]100,q−2 și C301(∅,VT)0,q+2; D345(∅,G)[L2]100,q+1 și D345(∅,VT)0,q−1 → R=G/∅/G/∅, roluri explicite, pereche nulă. Aceleași solduri; cantitățile se conservă separat pe produs. `N/Motor/Transformare.cs:27`. |
| **8. Deschidere** | D371(∅,G)[L]100,q10; C401(A,∅)[p]100 → R=G/A, pereche nulă. Fără 890 și fără contracantitate inventată. `M/Cub/Materializare.Deschidere.cs:92`. |

Cele trei declarații ale contului **nu se pot comprima într-un singur enum fără pierdere**. Obligatoriu, feluri admise, urmărire pe partide și rol SAF-T sunt axe diferite. Pot fi reunite într-un obiect de configurare, păstrând axe separate. `[judecată]`

Privat: 401 și 4111 urmăresc partide cu roluri comerciale diferite; 542 și 461 urmăresc partide fără rol comercial; 462 nu urmărește partide. Dimensiunile obligatorii pornesc goale (`M/DatabaseUpdate/ProfilPrivat.cs:233`, `:336`, `:344`, `:364`).

Bugetar: 542.01.00, 542.02.00, 408.00.00 și 428.01.02 urmăresc partide fără flag R (`M/DatabaseUpdate/ProfilBugetar.cs:258`; `S/plan-conturi.csv:1157`, `:1185`, `:1459`). Invers, cele **16 conturi R fără partide** sunt: 774, 803.00.01, 804.90.00, 805.00.00, 442.08, 401.02.00, 411.02.08, 437.01.00, 437.02.00, 442.08.01, 458.01.00, 458.05.01, 458.05.02, 462.01.01, 462.01.09, 552.00.00 (`S/plan-conturi.csv:327`, `:336`, `:354`, `:594`, `:1150`, `:1163`, `:1196`, `:1206`, `:1282`, `:1287`, `:1297`, `:1463`). Unirea ar schimba aceste contracte.

Forma exactă recomandată: din `M/Cub/Postare.cs:8` păstrez toate coloanele existente **exceptând `Partener` și `Gestiune`**, înlocuite cu `Guid? RepartitorSoldId`; adaug `Guid? PartenerFiscalId`, `int? Pereche` și `RolCantitativ RolCantitativ`, cu valorile `Fara`, `EvidentaLot`, `ExteriorFurnizor`, `ExteriorClient`, `ExteriorConsum`, `ExteriorInventar`, `ContrapondereTransformare`. Declarantul completează R după identitatea analitică a piciorului și politica contului, independent de partidă; lotul cere poziție de evidență, partida cere titular concordant; fiscalul se fixează separat din sursa fiscală explicită; perechea identifică numai mișcarea/mutarea; rolul tehnic nu intră în sold. Nul înseamnă neaplicabil, iar lipsa unei identități obligatorii se refuză. `[judecată]`

Pasul **2c** susține 1b și semantic formele 3/4/5; contrazice propagarea nediferențiată din 2. Pasul **7b** este compatibil cu toate și susține direct 5, fără să decidă cheia soldului (`docs/nucleu/tr-d9-taierea-amendament-2.md:35`; `docs/nucleu/tr-d9-taierea-amendament-1.md:57`). Implementarea literală a lui 2c acum ar cere ulterior refacerea asignărilor, gardului și cititorilor; regula „terț și fără partidă” rămâne reutilizabilă. `[judecată]`

Mai cer o precizare la 7b: stornoul poate reuni postări din mai multe tranzacții (`M/Cub/Materializare.cs:87`). Copierea ordinalelor locale poate produce coliziuni; trebuie remapate perechile originale distincte sau păstrat domeniul original al cheii. `[judecată]`

Ordinea cerută a schimbărilor este `[judecată]`:

1. Contractul cheilor, al nulurilor și al filtrelor de sold versus eveniment.
2. Separarea fiscalului și culegerea furnizorului la decont; invariantul inversei.
3. R și rolul cantitativ în nucleu, declaranți, deschideri și mapări.
4. Portarea cititorilor, snapshot-urilor și SAF-T.
5. Perechea, inclusiv stornoul; apoi folosirea ei în fișa contului.

Aș plasa schimbarea **în TR-D9a, înaintea pasului 7**, prin amendament explicit. Riscul este lărgirea tăierii; avantajul este resetul comun. Trebuie modificate D9-D1, D4, D6, D12, D15, B-D8 punctele 4–6/9, contractul fiscal D8-B8 și D9-A2. Contractul actual interzice schimbări în afara listei închise (`docs/nucleu/tr-d9-taierea-contract.md:163`). În **TR-D9b**, riscul este suprapunerea cu reevaluarea și unitățile; **după**, riscul este migrarea istoriei și compatibilitatea cititorilor. Economia DDL de acum nu elimină costul semantic. `[judecată]`

Fișa este utilă, dar înclină argumentul: §2 spune greșit că trezoreria pune G pe ambele capete în general; §4 confundă cheia snapshot-ului cu gruparea balanței, care grupează pe cont și partener, G fiind filtru (`D/DeclarantTrezorerie.cs:158`; `M/Proiectii/ContabilProiectii.cs:221`). Omite fiscalul DEC, BCS cu lot pe cheltuială, nominalizarea PIF și verificarea incompletă a inversei. Numărătorile de referințe nu măsoară costul portării. Cele 22.389 de rânduri demonstrează suprapunerea sensurilor, nu necesitatea a două coordonate de sold. `[judecată]`

Înaintea deciziei definitive aș aloca **7–10 zile-om**: 2 zile pentru recensământul cheilor și filtrelor; 2–3 pentru prototipul semantic cu cazurile de mai sus și ciclurile fiscale; 2–3 pentru măsurători comparative de volum, indexuri și reconstrucție; 1–2 pentru auditul migrării și al perechilor. Sunt estimări `[judecată]`, nu rezultate măsurate.

**Încredere: 4/5 în separarea semantică, 3/5 în alegerea formei 5 și în calendar.** Limitele sunt absența bazei, lipsa măsurătorilor de utilizare și performanță și acoperirea incompletă a coordonatelor independente, în special custodia. `[judecată]`