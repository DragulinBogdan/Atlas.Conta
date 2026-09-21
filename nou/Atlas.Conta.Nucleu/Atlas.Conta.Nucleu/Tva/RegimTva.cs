namespace Atlas.Conta.Nucleu;

// Duplicat DECLARAT al enumului din Module (moare la TR-D9): valorile numerice
// sunt cele de acolo, ca maparea consumatorului să rămână o conversie.
public enum RegimTva {
    Normal = 1,
    Capitalizat = 2,
    TaxareInversa = 3,
    Scutit = 4,
    Neimpozabil = 5,
}
