# Consultare: cum poartă postarea repartitorul

- Data: 2026-10-06. Stare: **ambele runde rulate; răspunsurile în
  `raspunsuri/r1-*.md` și `r2-*.md`; decizia e la owner.** Nimic din această
  consultare nu e transcris în contract sau în decizii.
- Întrebarea owner-ului: cubul poartă azi două coloane tipate, `Partener` și
  `Gestiune`, completate selectiv. Ar fi mai bun un singur `Repartitor`
  generic, tipat de cont, sau fluxul pe postare (`RepartitorIesire`,
  `RepartitorIntrare`) plus o coloană de grupare cu semantică pe natura
  contului?
- Analiști: Claude Fable 5.1 și GPT-6 Astra (Codex), independent, efort
  ridicat. Fără critică încrucișată.
- Bază: `tr-d9-taierea` la `717c3b5`. Clona: `D:\Temp\conta-analiza`.

## Formele puse în discuție

| Forma | Pe scurt |
|---|---|
| 1 | azi: `Partener` și `Gestiune`, selectiv; gestiuni virtuale pe capătul din afara evidenței; partenerul numai cu partida și pe faptul fiscal |
| 1b | 1 plus partenerul pe piciorul de terț al conturilor care cer repartitor (D9-A10, aprobat, neimplementat) |
| 2 | ambele coloane pe toate postările documentului cu terț |
| 3 | un singur `Repartitor`, tipat de cont; partenerul fiscal în blocul fiscal |
| 4 | fluxul și gruparea: `RepartitorIesire`, `RepartitorIntrare`, `RepartitorGrupare` |
| 5 | 3 plus cheia de pereche (D9-A2, aprobată, neimplementată) |

## Protocolul

| Runda | Ce primește analistul | Unde rulează | Fișier |
|---|---|---|---|
| 1, oarbă | numai promptul | director gol, în afara repo-ului | `1-runda-oarba.md` |
| 2, informată | răspunsul lui din runda 1, fișa de fapte, repo în citire | clona, la `717c3b5` | `2-runda-informata.md`, `fapte.md` |

Mecanica e cea din `../2026-10-05-cub-vs-registre/README.md`. Rularea:
`run.sh`, pe pași, cu jurnalul în `raspunsuri/run.log`. Durate: runda oarbă
4–5 minute per analist, runda pe cod 7–13 minute.

## Rezultatul

**Converg amândoi, în ambele runde, pe aceeași direcție:**

- o singură coordonată de sold pentru „cine sau unde", `Repartitor`, cu felul
  admis declarat de cont ca mulțime, nu ca fel unic;
- partenerul fiscal e atribut al faptului fiscal, persistat pe postare, în
  blocul fiscal; nu se deduce din document, din pereche sau din capete;
- gestiunile virtuale nu sunt necesare ca gestiuni; capătul din afara
  evidenței și contraponderea transformării cer un marcaj explicit;
- forma 2 respinsă; forma 4 respinsă ca formă finală: gruparea ei e corectă și
  trebuie scrisă, dar capetele nu pot fi completate onest (decontul,
  viramentul, TVA-ul, locul fișei, imputatul de pe linia NIR);
- recomandarea amândurora e forma 5: 3 plus cheia de pereche;
- locul schimbării: în TR-D9a, la pașii 7 și 7b, cât `InitialCreate` și bazele
  se recreează oricum; după tăiere costul ar fi al unei migrări;
- pasul 2c, în forma aprobată, adâncește forma 1b și ar fi refăcut; amândoi
  cer hotărârea formei înaintea lui 2c.

| | Runda 1, clasament | Runda 2, verdict |
|---|---|---|
| Codex | 3, 5, 1b, 4, 1, 2 | 5 recomandată; 3 și 4 cu schimbări; 1b tranziție; 1 respinsă; 2 respinsă dacă ambele coloane intră în sold |
| Fable | 5, 3, 4, 1b, 1, 2 | 5 recomandată, „F3+"; 3 cu schimbări; 1, 1b, 2, 4 respinse cu contraexemple din cod |

**Diferă pe:**

- coloanele exacte: Fable cere `FelRepartitor` pe postare, ca nucleul să
  deosebească un declarant care a uitat lotul de o contracantitate legitimă;
  Codex cere un `RolCantitativ` explicit (evidență, exterior pe feluri,
  contrapondere) și păstrează `RepartitorSold` plus `PartenerFiscal`;
- declarația contului: Fable unește flag-ul `Repartitor` cu felul într-un
  `RepartitorCerut` cu valori multiple și ține `UrmarestePartide` și `RolTert`
  separate; Codex spune că cele trei axe nu se comprimă fără pierdere;
- costul înaintea deciziei: Fable 4–5 zile, Codex 7–10 zile-om;
- Codex nu respinge universal forma 2, dacă ambele coloane ar fi atribute de
  eveniment, nu de sold.

**Constatări din cod care nu erau în fișă** (de verificat înaintea oricărei
decizii):

- 3xx la recepție poartă partenerul fiscal, deci balanța analitică pe
  repartitor a stocurilor rămâne pe furnizor după consum (Fable, cazul A);
- viramentul pune aceeași bancă pe ambele picioare ale fiecărei etape, deci
  581 pe bancă nu se închide (Fable, cazul D); fișa spunea greșit că
  trezoreria pune gestiunea pe ambele capete în general (Codex);
- DSC pune virtuala Client și la primitor intern; LDI pierde comisia; BCS
  pune lotul și locul pe 6xx (Fable);
- la decont, partenerul fiscal e angajatul, scos de D394 la „neincluse";
  furnizorul bonului nu există în model în nicio formă (amândoi);
- invariantul inversei fiscale nu verifică partenerul (Codex);
- stornoul poate reuni postări din mai multe tranzacții, deci ordinalul
  perechii din D9-A2 cere remapare (Codex);
- textul D9-A10 „partenerul = latura externă a documentului" e greșit pentru
  DEC și NTC, unde terțul e al liniei; corect e „terțul capătului" (Fable);
- fișa §2 spunea că partenerul vine numai cu partida; DSC, RDC, DEC, NTC și
  diferența NIR îl scriu fără partidă (Fable). §4 numără linii, nu semantici.

## Ce rămâne al owner-ului

- dacă direcția se adoptă, și în ce felie: TR-D9a la pasul 7 (amendament
  nou, a noua schimbare de comportament), TR-D9b sau după;
- soarta pasului 2c: implementat cum e aprobat, redus la un helper al
  capătului de terț, sau absorbit în forma nouă;
- lista conturilor bugetare cu flag `Repartitor` și felul lor, pe care
  Fable o cere confirmată de owner;
- măsurătorile cerute de amândoi înaintea deciziei: recensământul perechilor
  de feluri pe cont pe Flax, D394 regenerat cu partenerul mutat, prototipul
  nucleului cu repartitor și fel, fișa contului prin pereche.
