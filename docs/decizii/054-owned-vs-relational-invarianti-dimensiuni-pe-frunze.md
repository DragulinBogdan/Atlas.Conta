# Decizia 54 — Sesiunea de arhitectură 2026-08-02

- **Data**: 2026-08-06 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §54
- **Docs**: docs/invarianti.md, docs/dim/dim-2-inventar.md

---

**Sesiunea de arhitectură 2026-08-02 („owned vs relational") →
constituția invarianților + dimensiunile coboară pe frunze.** Tranșările:
(a) **`docs/invarianti.md` = constituția proiectului**: cei 6 invarianți
agreați (operația intră ca document; baza=identitate / frunza=culegere /
motorul nu cunoaște frunzele; registrele = singurul adevăr al agregării;
structura cod / politica date / politica nu inventează comportament;
sursele externe = evidență, niciodată canonic; evaluarea = motorul),
fiecare cu clauze de interdicție. Orice propunere arhitecturală se
testează întâi contra lor; jurnalul de față rămâne „de ce e așa".
(b) **Stocarea dimensiunilor rămâne inline** pe tabelele owner-ilor —
normalizarea (tabelă separată / combinații deduplicate) analizată și
RESPINSĂ: join obligatoriu înaintea oricărui GROUP BY de balanță,
lookup-or-create în tranzacția operării (hot path), câștig de spațiu
iluzoriu (NULL-uri = bitmap pe Postgres, volum de sute de mii de
rânduri/an, nu miliarde).
(c) **Maparea owned MOARE; dimensiunile devin caracteristică de frunză**
— amendament la testul bazei (22b): „motorul are nevoie de VALOARE, nu de
coloană"; pe bază intră un câmp doar dacă (1) semantica lui e identică
pentru orice tip purtător și (2) motorul îl consumă direct la postare.
Concret: FK-uri explicite pe detaliile derivatelor care culeg dimensiuni
(reuniunea câmpurilor per TIP, nu per profil), baza expune contractul
`DimensiuniCulese()` → `Dimensiuni` ca value object NE-persistat, consumat
de DimensiuniResolver/motor (idiomul ILinieCu* existent, 32a);
`RegistruContabil` păstrează cele 2×8 coloane ca proprietăți PLATE
(read-only, `HasColumnName` conservă schema), `RegulaContare` cele 3
seturi plate → editarea politicilor devine XAF-nativă. Câștiguri:
UI per tip natural (lookup standard exact pe câmpurile tipului),
AuditTrail redevine posibil (53e — owned-ul era blocajul), DTO-urile
pasului 5 sunt deja plate (aliniere 6/42d), moare regula `CreateProxy`
și dependența de OwnedObjectBase în Conta.
(d) **Clasele per profil (`FacturaIntrarePrivat`) RESPINSE definitiv**
(invariant IV.3): diferența privat/bugetar la dimensiuni e politică +
vizibilitate, nu schemă — reuniunea pe frunză + layout per profil
(`SetareProfil`); profilul rămâne pachet de seed, niciodată ierarhie.
(e) **Consecință de scop asumată**: tipurile care azi culeg dimensiuni pe
detaliul de BAZĂ (NIR — prin clona conexă din FCT; PLT/INC — defalcarea
31a) primesc detaliu derivat propriu cu câmpurile lor; clona conexă și
plata autogenerată copiază prin contract, frunză→frunză. Inventarul
exact „ce dimensiuni culege fiecare tip" (probe: validări, politici seed,
handler-ele Import1C) e primul pas al implementării, nu se ghicește.
(f) Implementarea = feliile DIM-1…DIM-4 (în plan); DIM-2 e candidatul de
re-apropriere: o conduce utilizatorul, linie cu linie.
