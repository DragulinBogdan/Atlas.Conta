[judecată] **Îmi schimb recomandarea din C în A, cu TR-D9a amendat.** Cubul implementat justifică păstrarea direcției: unifică efecte și localizează calculul mai bine decât anticipasem. Nu justifică însă afirmația că registrele ar fi structural incapabile de aceleași corecții. B rămâne alternativa serioasă pentru raportare; avantajul ei trebuie demonstrat pe o proiecție reală înaintea ștergerii definitive.

Am lucrat numai prin citire, fără build, ModelCheck, baze sau acces la `comunicari/`. „Verificat” din cataloage desemnează rezultatul consemnat de proiect, nu o rulare a mea.

Prescurtări pentru ancore: `M/` = `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`; `N/` = `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`; `D/` = `docs/nucleu/`; `S/` = `D/scenarii/`; `P` = `docs/api/p5-perf-masuratori.md`.

**Verdictul pe cele șapte criterii**

Clasamentele sunt [judecată], pentru continuarea proiectului de azi, nu pentru trei implementări imaginare pornite simultan.

| Criteriu | Verdict și dovadă |
|---|---|
| Coerență | **A = B > C actual.** Lotul și valoarea contabilă folosesc aceeași postare. Dar nota pe 302 fără lot rămâne acceptată: concordanța tuturor pozițiilor contului cu stocul nu rezultă automat. `M/Cub/Citiri/Loturi.cs:21–34`; `nou/tools/ModelCheck/ScenariiNtc.cs:46–48`. |
| Implementare și întreținere | **A > B > revenirea la C.** A poate elimina planul paralel; B adaugă derivare și mută din nou cititorii; C cere și refacerea comportamentelor câștigate. Eliminarea planului este concretă, dar hook-urile de pregătire și validare rămân. `D/tr-d9-taierea-contract.md:176–210`. |
| Lizibilitatea unui document | **A = B, rezultat mixt față de C.** Declarantul arată efectele într-un loc, dar NIR și ASM introduc semantică suplimentară. Nu susțin „mai puțin cod per tip” universal. `M/Declaratii/DeclarantNir.Diferenta.cs:12–95`; `DeclarantAsamblare.cs:61–119`. |
| Portița manuală | **Egalitate posibilă, incompletitudine actuală.** Motorul este comun, însă NTC nu oferă nominalizarea unui lot; terțul fără partener rămâne nenominalizat. `M/Declaratii/DeclarantNotaContabila.cs:43–44,56–82`. |
| Optimizări ulterioare | **B ≥ C > A strict.** B permite forme specializate principale. A păstrează indexarea și agregatele; chiar azi are snapshot-uri contabile, de stoc și partide distincte. `M/BusinessObjects/Registre/SolduriPerioada.cs:7,56,81`. |
| Audit | **A = B > registrele actuale**, pentru explicația FIFO/evaluării și legăturile explicite. Nu există însă versiune istorică completă a politicii: operandul primește `"seed"` și data înregistrării. `M/Cub/Explicatie.cs:25–34,61–77`; `M/Motor/Fapte.cs:144`. |
| Reversibilitate | **B are avantaj numai cu autoritate suficientă.** Cubul nu păstrează perechea contabilă originală ca identitate; fișa afișează conturile opuse ale tranzacției. A și C nu au ordine universală. `M/Cub/Postare.cs:8–64`; `M/Proiectii/ContabilProiectii.cs:486–489`. |

**Verificarea întrebărilor mele din runda 1**

1. **Este C mai simplu?** Nu am găsit dovada presupunerii mele. Planul registrelor combină pregătire, reguli de stoc, evaluare, contare și fiscalitate; declarantul separă calculul de citirea faptelor. Totuși, factura păstrează tratamente speciale, iar fiscalitatea comună nu devine gratuită. `M/Motor/MotorOperare.cs:63–298`; `M/Declaratii/Contractare.cs:18–43`; `DeclarantFacturaIntrare.cs:107–192`. [judecată] Retrag preferința generală pentru C la întreținere.

2. **Este cubul suficient informațional?** Pentru agregatele implementate păstrează baza/taxa prin rol, cota, regimul și reperele fiscale. Pentru fișa imobilizării citește suplimentar metoda, durata și alte atribute din documentele eveniment. **Postările singure nu reconstruiesc tot.** `M/Cub/Postare.cs:34–48`; `M/Cub/Citiri/Imobilizari.cs:45–70`.

3. **Cum reconstruiește B perechile?** Nu le reconstruiește unic în general. `Miscare` produce două postări, dar identitatea mișcării nu ajunge în schema persistentă; ASM produce grupuri. Aș proiecta un jurnal pe laturi, fără corespondențe inventate. `N/Cub/Miscare.cs:3–23`; `M/Cub/Postare.cs:8–64`; `N/Motor/Transformare.cs:13–32`.

4. **Concurența?** Există blocaj tranzacțional serial per bază și probe pe conexiuni distincte pentru consumuri, stingeri și închidere. Rezultatele consemnate includ două consumuri evaluate succesiv la 0,33 și 0,34. Nu este probat debitul cu mulți operatori. `M/Motor/TranzactieComanda.cs:14–52`; `nou/tools/ModelCheck/ScenariiConcurenta.cs:34–46,89–155,203–214`; `D/tr-d8-transversal-contract.md:597–626`.

5. **Ce păstrează unitatea?** Identitatea și felul, plus suportul, inversa și atribuirea sunt persistate; compatibilitatea lot/produs și partidă/partener este validată. Acestea nu constituie automat un graf complet al repartizării costurilor ulterioare. `M/Cub/Postare.cs:25–32,60`; `N/Conservare.cs:158–184`. Ultima apreciere: [judecată].

6. **Cazurile-limită?** NIR diferență cantitativă, nominalizare, storno și închidere au scenarii concrete. Reevaluarea după consum rămâne amânată; custodia este refuzată. `S/NIR.md:60–74`; `S/NTC.md:31–43`; `S/BCS.md:95`; `S/LDI.md:38`.

7. **Costul real?** Avem mediane, maxime, SQL, alocări și explicații persistate; scena recentă privată are numai 7.951 postări. Nu avem aici comparația completă A/B/C, p95/p99 concurente sau reconstrucția registrelor B. `P:1023–1035,1113–1127,1219–1220`. Absența unei comparații suficiente: [judecată].

8. **Audit reproductibil?** Alegerea unităților și evaluarea sunt explicabile; reproducerea integrală a deciziei cu politica istorică nu este demonstrată de eticheta `"seed"`. `M/Cub/Explicatie.cs:44–77`; `M/Motor/Fapte.cs:144`.

9. **A câștigă fără artificii?** Nu îndeplinește condiția mea maximală „toate cazurile, mai puțin cod semantic”. Are contraponderi cantitative pentru transformare și cititori care le elimină. [judecată] Câștigul justifică totuși continuarea, fără a transforma convențiile în virtuți universale. `N/Motor/Transformare.cs:26–32`; `M/Cub/Citiri/Transformare.cs:7–14`.

**Predicțiile din `nucleu-bilant.md`, rând cu rând**

Tabelul original este la `D/nucleu-bilant.md:15–29`. „Neverificabil” privește rezultatul implementării actuale, nu existența măsurării istorice.

| Predicție | Verdict față de azi |
|---|---|
| Corectitudinea modelului | **Confirmat, limitat:** partide nominalizate și efect unificat; pretenția eliminării tuturor diferențelor este prea tare, deoarece există NTC fără unitate. `M/Declaratii/DeclarantNotaContabila.cs:63–82`; `S/NTC.md:41–42`. |
| Un mecanism în loc de patru | **Infirmat în forma literală:** inversarea pură este comună, dar persistă snapshot-uri specializate și mecanisme de domeniu. `N/Cub/Storno.cs:8–30`; `M/BusinessObjects/Registre/SolduriPerioada.cs:7–99`; `D/tr-d9-inventar.md:22`. |
| Izolarea motorului | **Confirmat pentru calcul**, nu pentru dispariția hook-urilor: operand închis → declarant → nucleu; coaja păstrează pregătirea și validarea frunzei. `M/Declaratii/Contractare.cs:18–43`; `D/tr-d9-taierea-contract.md:178–184`. |
| Scrierea documentului | **Neverificabil postimplementare:** cifra 1,7/5,3 ms este a prototipului; cu FK echivalente avantajul aproape dispare. `D/nucleu-fizica.md:110–115`. |
| Citirile lunii | **Neverificabil comparativ azi:** există timpi recenți ai cubului, fără martor contemporan pe registre. `P:1037–1072`. |
| Citirile integrale | **Neverificabil comparativ azi:** pierderea prototipului este documentată; implementarea folosește alte trasee și snapshot-uri. `D/nucleu-fizica.md:71–81`; `P:1200–1208`. |
| Balanța pe snapshot | **Neverificabil în raport cu C actual:** 57/99 ms este comparația veche; 8,1/12,9 ms compară recitirea cubului cu snapshot-ul lui. `D/nucleu-fizica.md:86–87`; `P:1204–1208`. |
| Disc | **Neverificabil ca raport actual:** 1,5–1,8× privește forma prototipului; explicațiile adaugă alt cost. `D/nucleu-fizica.md:289–290`; `P:1219–1220`. |
| Coordonate noi | **Confirmat ca structură**, nu ca funcționalitate livrată integral. `M/Cub/Postare.cs:50–64`; `D/tr-d9-taierea-contract.md:764–766`. |
| Continuitate semantică | **Confirmat:** recepția aparține FCT; NIR nemodificat nu postează. `M/Declaratii/DeclarantFacturaIntrare.cs:130–154`; `S/NIR.md:40`. |
| Disciplina Transfer | **Confirmat**, inclusiv tratarea inversei transferului și excluderea contraponderilor. `M/Cub/Citiri/Contabil.cs:33–45`. |
| Rescriere mare | **Confirmat ca întindere**, nu ca productivitate măsurată: mai rămân portări și eliminări nominale. `D/tr-d9-taierea-contract.md:660–667`. |
| Forma declarației fluxului | **Neverificabil ca superioritate universală; necunoscuta tehnică este rezolvată.** Analiza concretă urmează mai jos. `M/Declaratii/Contractare.cs:13–43`. |
| Ce nu se putea proba | **Confirmat ca risc rezidual**, redus pentru storno/concurență, încă deschis pentru reevaluare. `S/NTC.md:30–43`; `S/BCS.md:95`. |
| Costul de oportunitate | **Neverificabil ca valoare pierdută:** înghețarea este consemnată, lipsesc estimări independente ale funcțiilor amânate. `docs/instructiuni-proiect/CLAUDE.md:113–129`; `D/nucleu-bilant.md:219–224`. |

**Cazurile reale și declarația fluxului**

Liniile de mai jos sunt fizice, inclusiv comentarii. Clasele documentelor includ culegere/UI, deci **nu sunt o măsurare comparabilă a efortului**. Numărul conceptelor și al tratamentelor speciale este inventarul meu semantic [judecată], nu complexitate ciclomatică.

- **1 — Factură cu recepție.** Registre: factura, conexul NIR, regulile de stoc/contare, TVA, lotul și stingerea — aproximativ șapte concepte. Cub: operand, mișcare/capete, unități, fiscalitate, partidă, decizii/ipoteze — aproximativ șapte. Frunza FCT are 237 linii; declarantul 194, peste infrastructura comună. Registrele primesc recepția prin politica NIR, cubul direct din FCT. `M/BusinessObjects/Documente/FacturaIntrare.cs:1–237`; `M/Declaratii/DeclarantFacturaIntrare.cs:1–194`; `M/DatabaseUpdate/ProfilPrivat.cs:1002–1030`. Trei tratamente speciale vizibile în declarant: stoc versus net, contrapartidă fără regulă proprie, capitalizarea TVA. Politicile aleg conturi/regimuri; codul stabilește când și cum se postează. Riscul tăcut: fallback-ul contrapartidei sau analiza recepției nevalidată. `DeclarantFacturaIntrare.cs:44–46,110–126,179–192`; `D/tr-d9-inventar.md:23`.

- **2 — NIR ulterior.** Registre: recepție completă, lot, regulă, conex — aproximativ patru concepte; cub: încă recepție istorică, delta, cauză și politică de diferență — aproximativ opt. Clasa NIR ocupă `DocumenteGestiune.cs:17–136`; declarația are 65+97 linii, iar adaptorul recepției 235. `M/Declaratii/DeclarantNir.cs:1–65`; `DeclarantNir.Diferenta.cs:1–97`; `M/Cub/ReceptiiConexe.cs:1–235`. Cele trei ramuri importante sunt linia originală, lotul suplimentar și delta zero. Pentru 4/100→3/75, cubul păstrează furnizorul la 100 și pune 25 pe contul cauzei; registrul recepției nu exprimă același rezultat economic. `S/NIR.md:60–64,74`. **Corecția de preț după consum nu este acest caz implementat:** pe original, cantitate neschimbată produce delta valorică zero. `DeclarantNir.Diferenta.cs:43–45`.

- **3 — Asamblare.** Registre: consum/produs, lot, preț, reguli, conservarea valorii — aproximativ cinci concepte. Cub: aceleași plus evaluarea curentă, grupuri pe cont, Transfer/Operare, contrapondere și absorbție duală — aproximativ zece. Frunza are 237 linii, declarantul 124, transformarea pură 34. `M/BusinessObjects/Documente/Asamblare.cs:1–237`; `M/Declaratii/DeclarantAsamblare.cs:1–124`; `N/Motor/Transformare.cs:1–34`. Trei tratamente suplimentare: clasificarea grupurilor, ancora absorbției, produsul devenit nepozitiv. Seed-ul vechi nu pune note; cubul reprezintă și transformarea între conturi. `M/DatabaseUpdate/ProfilPrivat.cs:1248–1255`; `S/ASM.md:30–35`. [judecată] Aici cubul este mai complet, dar astăzi mai greu de citit. Eliminarea absorbției simplifică; contraponderea rămâne.

- **4 — Nota manuală.** Frunza are 142 linii, declarantul 87; vechiul plan rezolvă perechea explicită, cubul nominalizează separat ambele laturi și le împarte în mișcări. `M/BusinessObjects/Documente/NotaContabila.cs:1–142`; `M/Motor/MotorOperare.cs:159–212`; `M/Declaratii/DeclarantNotaContabila.cs:35–82`. Aproximativ trei concepte pentru nota simplă, șase cu partide/FIFO. Cele trei ramuri sunt fără partener, stingere și rest pe partidă proprie. [judecată] Complexitatea suplimentară cumpără comportament, nu simplificare textuală.

- **5 — Storno.** Secvența veche are 87 linii (`MotorOperare.cs:637–723`), inversarea pură 48; însă coaja, dependențele și materializarea rămân. `N/Cub/Storno.cs:1–48`; `M/Cub/Materializare.cs:70–114`. Aproximativ șase concepte pe ambele căi: perioadă, original, inversă, dependenți, stingeri, fiscalitate. Cubul elimină copierea separată a câmpurilor celor trei registre, nu analiza dependențelor. Storno după consum poate refuza până la inversarea consumatorilor; nu repară automat lanțul. `S/NIR.md:38`; `nou/tools/ModelCheck/ScenariiAsm.cs:405–412`.

- **7 — Custodie.** Nu există două implementări complete de comparat. Registrul are `TipStoc`, cantitate și valoare; declarantul LDI acceptă numai Magazie/Marfuri/Folosință. `M/BusinessObjects/Registre/Registre.cs:20–37`; `M/Declaratii/DeclarantDiferenteInventar.cs:52–55`; `S/LDI.md:38`. [judecată] Extensia cere minimum proprietar, natură proprie/custodie, lot, gestiune și semantica extrabilanțieră. Enum-ul existent nu dovedește acoperirea; linii și cazuri speciale pentru implementarea cerută: **nemăsurabile**.

[judecată] Declarația este un câștig de localizare și testabilitate, nu dispariția cuplajului. Infrastructura vizibilă include `Contractare` 60 linii, `Operand` 130 și `Fapte` 423; un cititor urmărește în continuare colectarea faptelor, helper-ele și materializarea. `M/Declaratii/Contractare.cs:1–60`; `Operand.cs:1–130`; `M/Motor/Fapte.cs:1–423`. Politicile ignorate de un declarant sunt un risc concret: gardianul validează forma regulii, fără să verifice dacă tipul o consumă. `M/Motor/GardianEditare.cs:1165–1188`; `D/tr-d9-inventar.md:26`.

**B, concret**

[judecată] Aș construi cinci proiecții principale de raportare:

| Proiecție | Formă |
|---|---|
| Contabil | O latură per rând, cont, analiză, debit/credit, document, tranzacție; Transfer și inversele lui clasificate la derivare. |
| Stoc | Lot–produs–gestiune–cont, cantitate/valoare semnate; consumul separat de stocul disponibil. |
| Fiscal | Document/linie/fapt fiscal, bază și taxă distincte, cotă/regim și repere istorice. |
| Partide | Identitate cont–partener–unitate, origine și mișcări de stingere; solduri reconstruibile. |
| Imobilizări | Eveniment, fișă, valori contabile/fiscale și parametrii istorici ai evenimentului. |

[judecată] Derivare **sincronă**, în tranzacția comenzii, exclusiv din efectele finale și atributele istorice necesare. Motorul continuă să citească autoritatea cubului; rapoartele citesc predominant registrele. Reconstrucție într-o generație nouă, identificatori deterministici, verificare numerică, apoi comutare atomică. Politicile economice curente nu se reaplică.

[judecată] Din TR-D9a aș păstra temporar schemele și grilele ca referință, contractele rapoartelor și probele independente. Aș rescrie formele de mai sus, autorizarea și cititorii; **nu** aș păstra scriitorii vechi, absorbția ASM, `TotalStingere` sau regulile economice duplicate. Tabelele actuale și oracolul sunt exact printre eliminările D9-D6/D7. `D/tr-d9-taierea-contract.md:423–448`.

[judecată] Beneficiul B este real conceptual: raportul nu mai interpretează contraponderi, originea stornoului și forma generică. Costul este încă o materializare verificabilă și o a doua portare a citirilor. Nu rezultă din „registrele există deja” că B este aproape terminată.

**C, concret**

[judecată] Revenirea nu este un comutator: dezactivarea este refuzată după existența tranzacțiilor, iar codul vechi folosește deja citiri ale cubului. `M/Motor/GardianEditare.cs:1043–1045`; `M/Motor/MotorOperare.cs:786–788`. Estimez **20–35 zile de inginer**, pentru adaptarea motorului pur la efecte tipate, restaurarea rapoartelor și portarea scenariilor; fără ALOP și migrare istorică.

[judecată] Toate cele patru defecte din bilanț §2.1 sunt reparabile pe registre:

- partidă explicită cont–partener, independentă de antet;
- efect valoric unic în plan, proiectat atomic în contabil și stoc, cu egalitate validată;
- partener explicit, distinct de repartitor;
- fapte fiscale finale înainte de commit și ajustări valorice append-only.

Afirmația contrară din `D/nucleu-bilant.md:40–66` este prea categorică. Repartizarea exactă a taxei și evaluarea ultimei ieșiri sunt algoritmi reutilizabili, nu proprietăți exclusive ale tabelei `Postare`; chiar registrul are `ValoareGolire`. `M/Motor/StocService.cs:50–51`; `N/Masura/Evaluare.cs:14–17`.

**Portița**

[judecată] Regula owner-ului este necesară, dar trebuie spus dacă „numește unitatea” înseamnă alegere explicită sau nominalizare deterministă a motorului.

- **Simplă:** în A și B, mișcare contabilă normală; în C, pereche contabilă prin același plan validat.
- **Terț:** A/B pot nominaliza prin FIFO și deschide rest propriu, cum face declarantul. C poate scrie aceleași alocări într-un registru de partide. Fără partener, azi nota nu stinge factura existentă. `M/Declaratii/DeclarantNotaContabila.cs:63–82`; `nou/tools/ModelCheck/ScenariiNtc.cs:129–131`.
- **Stoc:** A/B necesită o extensie a notei care cere lot, produs, gestiune și tipul ajustării; C cere același sens economic și efecte corelate contabil/stoc. Azi NTC emite cantitate zero și nu nominalizează lotul. `M/Declaratii/DeclarantNotaContabila.cs:43–44,56–60`.

[judecată] Aș refuza implicit nota nenominalizată pe conturi controlate sau aș reprezenta explicit excepția într-o evidență reconciliabilă. Acceptarea tăcută de azi infirmă lectura tare „diferența nu mai poate exista”. Gardul nucleului cere unitate pentru cantitate nenulă, nu pentru orice valoare pe un cont de stoc. `N/Conservare.cs:139–159`.

**Viteza**

Prototipul arată pierderi reale: fișă integrală 1.017→1.757 ms, balanță cu referință 57→99 ms; arată și câștiguri lunare. Dar compară uneori agregatul cubului cu raportul complet și repetă aceleași coordonate pe zece ani. `D/nucleu-fizica.md:28–55,71–87`.

Scara recentă demonstrează îmbunătățirea interogărilor cubului, nu victoria asupra registrelor. `PREST-NI` rămâne dependent de istoric; API-ul recitește istoricul; raportul stoc include capătul de consum. `P:1168–1173,1179–1187,1224–1231`.

[judecată] Pentru **toate** rămân deschise indexi per acces, predicate indexabile, SQL specializat, batching, streaming, snapshot-uri și blocaje mai fine. Pentru **A** rămân agregatele cubului, dar registrele principale specializate ar schimba opțiunea în B. **B** permite cache-uri cu granularitate și securitate per raport, plătind derivarea și reconstrucția. **C** permite aceleași optimizări specializate fără dublarea stocării, dar pierde reconstrucția dintr-o autoritate comună alternativă. Eliminarea blocajului global cere probe de concurență în toate trei; nu cere schimbarea modelului.

**ALOP**

Vechiul flux nu este doar contabil: angajamentul verifică disponibilul și admite avertisment/refuz; lichidarea leagă ordonanțarea de document și linie; ordonanțarea verifică disponibilul angajamentului, trimestrial și anual, apoi are validare proprie. `legacy/Buget/AlopAngajamente.pas:503–521,590`; `AlopObligatii.pas:319–328`; `AlopLichidare.pas:659–680,1291–1297`. Evidența distinge „Plătit” și „De plătit”. `legacy/Buget/AlopDisponibil.dfm:1265,1277`.

[judecată] În toate opțiunile trebuie documente/evenimente pentru credit, angajament, lichidare, ordonanțare și plată, legături de consum parțial între faze, exercițiu, limite și aprobări. Coordonatele bugetare existente ajută, dar nu exprimă aceste relații. `M/Cub/Postare.cs:53–58`.

[judecată] **A:** unități bugetare tipate și efecte pe faze, plus structură explicită de alocări/aprobări. **B:** aceeași autoritate, cu registru principal de execuție/disponibil pe faze. **C:** registru specializat de rezervări/consumuri și note corelate prin identificator comun. Niciuna nu obține ALOP numai din seed sau din trei conturi.

**Ce înclină dosarul și ce schimb în TR-D9a**

[judecată] Dosarul este util ca hartă, dar „ambele căi probate” ascunde comparații semantic inegale: NIR poate lăsa registrul negativ, ASM postează contabil numai în cub, iar custodia este un refuz, nu o funcție acoperită. `S/FCT.md:33`; `S/ASM.md:33`; `S/LDI.md:38`. Eticheta măsurătorilor „A/B” poate sugera greșit comparația opțiunilor; închiderea FZ-r1 se bazează pe numai 7.951 postări, iar versiunea reală a politicii și pierderea perechilor nu primesc greutatea necesară. `P:1023–1024,1204–1208`; `M/Motor/Fapte.cs:144`.

[judecată] **TR-D9a se amendează astfel:**

1. Integrează I1–I8; păstrează hook-urile de stare/refuz ale imobilizărilor și mută gardul analizei recepției pe FCT. `D/tr-d9-inventar.md:22–29`.
2. Închide **în această felie** politicile fără efect: refuz la configurare pentru tipurile care nu le consumă; nu amână I5 ca tăcere acceptată.
3. Fixează contractual portița pe conturi controlate și probează cele trei variante; schimbarea față de SC-NTC-14/15 trebuie declarată.
4. Decide explicit dacă perechile originale sunt informație de audit obligatorie. Dacă da, persistă identificatorul mișcării pentru operațiile pereche; ASM rămâne grup. Nu pretinde reconstrucție retrospectivă exactă.
5. Pune comparația A/B și verificarea informației reconstruibile **înaintea pasului 6**, care elimină martorii; păstrează așteptările numerice independente după eliminarea lor. Pasul actual îi șterge la 6. `D/tr-d9-taierea-contract.md:665`.
6. Nu condiționa această tăiere de livrarea întregii reevaluări sau ALOP; păstrează explicit limitele lor.

[judecată] Înaintea deciziei finale de ștergere: **2–3 zile** pentru prototip B contabil+stoc+partide, **2 zile** pentru comparație pe aceeași populație mare, cu rapoarte complete, securitate, p95/p99, disc, scriere și reconstrucție; **1–2 zile** pentru probele portiței, politicilor ignorate și conservării informației. Total **5–7 zile de inginer**, fără implementarea completă B.

[judecată] **Încredere: 3/5.** Am dovezi suficiente să retrag C ca recomandare imediată și să resping ștergerea neamendată. Nu am suficiente să declar B inutilă sau cubul suficient pentru toate extensiile. Limitele decisive sunt lipsa comparației contemporane la volum mare, auditul politicilor și cazurile economice încă amânate.