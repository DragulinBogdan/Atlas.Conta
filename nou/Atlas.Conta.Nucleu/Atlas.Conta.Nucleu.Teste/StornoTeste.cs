using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class StornoTeste {
    [Fact]
    public void StornoulScoateExactCauzatulSiAtribuitul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scena = Scena.Construieste(aleator);
            var selectate = Storno.Selecteaza(scena.Toate, scena.Document);
            var stornata = Storno.Inverseaza(selectate, scena.Document, Gen.Data(aleator));
            var cuStorno = scena.Tranzactii.Append(stornata).ToList();
            var faraNimic = scena.Baza
                .Append(scena.Reevaluarea with {
                    Postari = scena.Reevaluarea.Postari.Where(p => p.Atribuit is null).ToList(),
                })
                .ToList();
            foreach (var (nume, cheie) in Proiectii.Toate)
                Proiectii.Egale(
                    Cub.Sold(cuStorno, cheie),
                    Cub.Sold(faraNimic, cheie),
                    $"{nume} (documentul {scena.Document})");
        });

    [Fact]
    public void SelectiaIaCauzatulSiUnSingurSaltSpreAtribuit() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scena = Scena.Construieste(aleator);
            var selectate = Storno.Selecteaza(scena.Toate, scena.Document);
            var asteptate = scena.Stornatul.Postari
                .Concat(scena.Reevaluarea.Postari.Where(p => p.Atribuit is not null))
                .ToList();
            Assert.Equal(asteptate.Count, selectate.Count);
            foreach (var postare in selectate)
                Assert.True(
                    postare.Cauza.Document == scena.Document || postare.Atribuit is not null,
                    "selecția a luat o postare nici cauzată, nici atribuită");
            Assert.Equal(asteptate, selectate);
        });

    [Fact]
    public void StornoulPastreazaLaturaSiConservareaCauzatului() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            Assert.Empty(Conservare.Verifica(tranzactie));
            var data = Gen.Data(aleator);
            var stornata = Storno.Inverseaza(tranzactie.Postari, tranzactie.Document!.Value, data);
            Assert.Equal(FelTranzactie.Storno, stornata.Fel);
            Assert.Equal(data, stornata.Data);
            Assert.Empty(Conservare.Verifica(stornata));
            for (var i = 0; i < tranzactie.Postari.Count; i++) {
                var inainte = tranzactie.Postari[i];
                var dupa = stornata.Postari[i];
                Assert.Equal(inainte.Coordonate.Latura, dupa.Coordonate.Latura);
                Assert.Equal(inainte.Cauza, dupa.Cauza);
                Assert.Equal(inainte.Atribuit, dupa.Atribuit);
                Assert.Equal(-inainte.Valoare, dupa.Valoare);
                Assert.Equal(-inainte.Cantitate, dupa.Cantitate);
                Assert.Equal(-inainte.ValoareValuta, dupa.ValoareValuta);
                Assert.Equal(data, dupa.Coordonate.Data);
            }
        });

    [Fact]
    public void StornoulStornouluiEOriginalulMaiPutinData() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var document = tranzactie.Document!.Value;
            var intai = Storno.Inverseaza(tranzactie.Postari, document, Gen.Data(aleator));
            var data = Gen.Data(aleator);
            var apoi = Storno.Inverseaza(intai.Postari, document, data);
            Assert.Equal(
                tranzactie.Postari.Select(p => p with { Coordonate = p.Coordonate with { Data = data } }).ToList(),
                apoi.Postari);
        });

    // Amendament de literă cerut la TR-D6b: postarea atribuită vine de pe ALT document, deci
    // stornoul „cauzat ∪ atribuit" încalcă prin construcție regula N-D3 „o tranzacție, un document".
    [Fact]
    public void StornoulCuAtribuitDinAltDocumentNuECauzaStrainaDarNuEBalansat() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scena = Scena.Construieste(aleator, celPutinOReevaluare: true);
            var selectate = Storno.Selecteaza(scena.Toate, scena.Document);
            var stornata = Storno.Inverseaza(selectate, scena.Document, Gen.Data(aleator));
            var refuzuri = Conservare.Verifica(stornata);
            Assert.DoesNotContain(refuzuri, refuz => refuz.Cod == Coduri.CauzaStraina);
            // Contrapartida atribuitului inversat e nedefinită până la TR-D9 (N-D10, restanță).
            Assert.Contains(refuzuri, refuz => refuz.Cod == Coduri.ConservareValoare);
        });

    [Fact]
    public void CauzaStrainaFaraAtribuitRamaneRefuzSiInStorno() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scena = Scena.Construieste(aleator);
            var strain = Gen.Documente.First(d => d != scena.Document);
            var postari = scena.Stornatul.Postari
                .Select((p, i) => i == 0 ? p with { Cauza = p.Cauza with { Document = strain } } : p)
                .ToList();
            var stornata = Storno.Inverseaza(postari, scena.Document, Gen.Data(aleator));
            Assert.Contains(Conservare.Verifica(stornata), refuz => refuz.Cod == Coduri.CauzaStraina);
        });

    [Fact]
    public void StornoulFaraPostariERefuzat() =>
        Assert.Throws<ArgumentException>(() =>
            Storno.Inverseaza([], Gen.Documente[0], new DateOnly(2026, 1, 1)));

    [Fact]
    public void SelectiaFaraPotrivireEGoala() =>
        Assert.Empty(Storno.Selecteaza([], Gen.Documente[0]));

    sealed record Scena(
        Guid Document,
        IReadOnlyList<Tranzactie> Baza,
        IReadOnlyList<Tranzactie> Tranzactii,
        Tranzactie Stornatul,
        Tranzactie Reevaluarea,
        IReadOnlyList<(Guid Id, Postare Postare)> Toate) {

        public static Scena Construieste(Random aleator, bool celPutinOReevaluare = false) {
            var document = Gen.Documente[0];
            var altul = Gen.Documente[1];
            var baza = new List<Tranzactie>();
            var cate = aleator.Next(3);
            for (var i = 0; i < cate; i++)
                baza.Add(aleator.Next(3) switch {
                    0 => Gen.Transfer(aleator, altul),
                    1 => Gen.Deschidere(aleator, altul),
                    _ => Gen.Operare(aleator, peDocument: altul),
                });
            var tranzactii = new List<Tranzactie>(baza);
            var stornatul = Gen.Operare(aleator, cuCantitate: true, peDocument: document);
            tranzactii.Add(stornatul);
            var urmator = 0;
            Guid Id() => new(100, (short)urmator++, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var toate = new List<(Guid Id, Postare Postare)>();
            foreach (var tranzactie in tranzactii)
                foreach (var postare in tranzactie.Postari)
                    toate.Add((Id(), postare));
            var tinte = toate
                .Where(pereche => pereche.Postare.Coordonate.Unitate?.Fel == FelUnitate.Lot
                    && pereche.Postare.Cauza.Document == document)
                .Select(pereche => pereche.Id)
                .ToList();
            var data = Gen.Data(aleator);
            var postari = new List<Postare>();
            var cateReevaluari = celPutinOReevaluare ? 1 + aleator.Next(3) : aleator.Next(4);
            for (var i = 0; i < cateReevaluari && tinte.Count > 0; i++) {
                var cauza = new Cauza(altul, Gen.Unul(aleator, Gen.Linii));
                var valoare = Gen.Zecimal(aleator, 0.01m, 999.99m, Scara.Bani);
                var tinta = tinte[aleator.Next(tinte.Count)];
                postari.Add(new Postare(
                    Gen.CapatContabil(aleator).Pe(Latura.Debit, data),
                    0m,
                    0m,
                    valoare,
                    cauza,
                    tinta));
                postari.Add(new Postare(
                    Gen.CapatContabil(aleator).Pe(Latura.Credit, data),
                    0m,
                    0m,
                    valoare,
                    cauza));
            }
            var reevaluarea = new Tranzactie(FelTranzactie.Operare, data, altul, postari);
            tranzactii.Add(reevaluarea);
            foreach (var postare in reevaluarea.Postari)
                toate.Add((Id(), postare));
            return new Scena(document, baza, tranzactii, stornatul, reevaluarea, toate);
        }
    }
}
