namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Clasificarea fiscală a partenerului — FUNCȚIA LEGII, o singură dată (felia
// 23, F23-D2). Locuia în `D394Proiectii.TipPartener`, unde era o cifră a
// formularului; e de fapt un fapt al nomenclatorului, pe care îl citesc acum
// DOUĂ formulare și o politică (`PoliticaTvaImplicit.ClasaFiscala`), deci se
// mută aici, lângă `TariUe`, ca funcție de nomenclator.
//
// `D394Proiectii.TipPartener` rămâne cu semnătura `int` (formularul scrie
// cifra 1–4 și niciun apelant al lui nu se schimbă) și cheamă de aici.
public static class ClasaFiscala {
    /// <summary>
    /// `tip_partener` (1–4) din identitatea fiscală a partenerului (D4-D1, cu
    /// fixurile 2/3 ale review-ului advers): **ÎNREGISTRAT BATE TOT** —
    /// înregistrat în scopuri de TVA în România ⇒ 1, indiferent de felul
    /// persoanei (PFA/II înregistrate) sau de țară (străin cu cod RO, art.
    /// 316); apoi PF ⇒ 2; RO neînregistrat ⇒ 2; UE ⇒ 3; altfel 4. §4.2: tip 1 =
    /// „persoane impozabile înregistrate în scopuri de TVA în România", tip 3 =
    /// „neînregistrate și care nu sunt obligate să se înregistreze".
    /// `Tara` goală se citește ca RO (default-ul nomenclatorului), nu ca 4.
    /// </summary>
    public static ClasaFiscalaPartener APartenerului(TipPersoana tipPersoana, string tara, bool inregistratTva) {
        if (inregistratTva)
            return ClasaFiscalaPartener.InregistratRo;
        if (tipPersoana == TipPersoana.Fizica)
            return ClasaFiscalaPartener.NeinregistratRo;
        var cod = Partener.NormalizeazaTara(tara);
        if (cod == "RO")
            return ClasaFiscalaPartener.NeinregistratRo;
        return TariUe.Contine(cod) ? ClasaFiscalaPartener.Ue : ClasaFiscalaPartener.ExtraUe;
    }
}
