using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

public static class ImperechereService {
    public static decimal Total(IObjectSpace os, Guid documentId) =>
        Cub.Citiri.Partide.Total(os, documentId, os.GetObjectByKey<Document>(documentId)?.SensDeStins(os));

    public static decimal Asignat(IObjectSpace os, Guid documentId) => Total(os, documentId) - Ramas(os, documentId);

    public static decimal Ramas(IObjectSpace os, Guid documentId) => Cub.Citiri.Partide.Ramas(os, documentId);

    public static Imperechere Imperecheaza(IObjectSpace os,
        Document stingator, Document document, decimal suma, Guid? contrapartidaId = null,
        DateOnly? data = null) {
        using var tx = TranzactieComanda.Incepe(os);
        Cub.Materializare.BlocheazaDocumente(os, stingator, document);
        var zi = data ?? DateOnly.FromDateTime(DateTime.Today);
        GardianPerioada.VerificaDeschisa(os, zi);
        var imperechere = Creeaza(os, stingator, document, suma, autogenerat: false, contrapartidaId, zi);
        os.CommitChanges();
        tx.Commit();
        return imperechere;
    }

    public static Imperechere Desfa(IObjectSpace os, Guid imperechereId, DateOnly data) {
        using var tx = TranzactieComanda.Incepe(os);
        var original = os.GetObjectByKey<Imperechere>(imperechereId)
            ?? throw new OperareException("Imperecherea nu există.");
        Cub.Materializare.BlocheazaDocumente(os, original.DocumentStingator, original.Document);
        GardianPerioada.VerificaDeschisa(os, data);
        var invers = CreeazaInvers(os, original, data);
        os.CommitChanges();
        tx.Commit();
        return invers;
    }

    internal static Imperechere CreeazaInvers(IObjectSpace os, Imperechere original, DateOnly data, bool inverseazaCub = true) {
        if (original.InverseazaId != null)
            throw new OperareException(
                "Imperecherea e ea însăși un rând invers — nu se desface a doua oară.");
        if (os.GetObjectsQuery<Imperechere>().Any(i => i.InverseazaId == original.ID))
            throw new OperareException("Imperecherea e deja desfăcută printr-un rând invers.");
        if (data < original.Data)
            throw new OperareException("Data desfacerii nu poate preceda data imperecherii.");
        var transfer = inverseazaCub ? DesfaceEfect(os, original, data) : null;
        var invers = os.CreateObject<Imperechere>();
        invers.DocumentStingatorId = original.DocumentStingatorId;
        invers.DocumentId = original.DocumentId;
        invers.Suma = -original.Suma;
        invers.Data = data;
        invers.InverseazaId = original.ID;
        invers.Autogenerat = false;
        invers.TranzactieCubId = transfer;
        return invers;
    }

    internal static void InverseazaLaStorno(IObjectSpace os, Document doc, DateOnly dataStorno) {
        var legaturi = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == doc.ID || i.DocumentId == doc.ID)
            .ToList();
        if (legaturi.Count == 0)
            return;
        var inversate = legaturi.Where(i => i.InverseazaId != null)
            .Select(i => i.InverseazaId.Value).ToHashSet();
        var vii = legaturi
            .Where(i => i.InverseazaId == null && !inversate.Contains(i.ID))
            .ToList();
        if (vii.Any(i => EstePerioadaDeschisa(os, i.Data)))
            throw new OperareException(
                "Documentul are imperecheri (stingeri) — ștergeți-le întâi, apoi anulați/stornați.");
        foreach (var legatura in vii)
            CreeazaInvers(os, legatura, dataStorno,
                !(legatura.Autogenerat && legatura.DocumentStingatorId == doc.ID));
    }

    public static void Sterge(IObjectSpace os, Guid imperechereId) {
        using var tx = TranzactieComanda.Incepe(os);
        var original = os.GetObjectByKey<Imperechere>(imperechereId)
            ?? throw new OperareException("Imperecherea nu există.");
        Cub.Materializare.BlocheazaDocumente(os, original.DocumentStingator, original.Document);
        GardianPerioada.VerificaDeschisa(os, original.Data);
        if (original.InverseazaId != null || os.GetObjectsQuery<Imperechere>().Any(i => i.InverseazaId == original.ID))
            throw new OperareException("Împerecherea desfăcută sau rândul invers nu se șterge.");
        DesfaceEfect(os, original, original.Data);
        os.Delete(original);
        os.CommitChanges();
        tx.Commit();
    }

    static Guid? DesfaceEfect(IObjectSpace os, Imperechere original, DateOnly data) {
        if (original.TranzactieCubId is Guid transfer)
            return Cub.Materializare.DesfaceTransfer(os, original.DocumentStingatorId, transfer, data);
        if (!original.Autogenerat) return null;
        return Cub.Materializare.Imperecheaza(os, original.DocumentStingator, original.Document,
            -original.Suma, data);
    }

    static bool EstePerioadaDeschisa(IObjectSpace os, DateOnly data) {
        try {
            GardianPerioada.VerificaDeschisa(os, data);
            return true;
        }
        catch (OperareException) {
            return false;
        }
    }

    internal static void CreeazaAutomataLaOperare(IObjectSpace os, Document document) {
        if (document.SursaStingeriiAutomate(os) is not Guid sursaId)
            return;
        var sursa = os.GetObjectByKey<Document>(sursaId);
        if (!sursa.PoateFiStins(os))
            return;
        var suma = Math.Min(
            Total(os, document.ID),
            Ramas(os, sursa.ID));
        if (suma <= 0) return;
        ValideazaCreare(os, document, sursa, suma, data: document.DataInregistrare, autogenerat: true);
        var tinte = Cub.Citiri.Partide.Origini(os).Where(o => o.DocumentId == sursaId)
            .Select(o => o.UnitateId).ToHashSet();
        var efect = os.ModifiedObjects.OfType<Cub.Postare>().Where(p => os.IsNewObject(p)
            && p.DocumentId == document.ID && p.Carte == Atlas.Conta.Nucleu.Carte.Contabil
            && p.Unitate != null && tinte.Contains(p.Unitate.Value))
            .GroupBy(p => new { p.Unitate, p.Cont, p.Partener })
            .Sum(g => Math.Abs(g.Sum(p => p.Latura == Atlas.Conta.Nucleu.Latura.Debit ? p.Valoare : -p.Valoare)));
        if (efect <= 0m || efect > suma)
            throw new OperareException($"IMPERECHERE_FARA_EFECT: nominalizarea automată cere {suma}, cubul dovedește {efect}.");
        Creeaza(os, document, sursa, efect, autogenerat: true, data: document.DataInregistrare);
    }

    internal static Imperechere Creeaza(IObjectSpace os,
        Document stingator, Document document, decimal suma, bool autogenerat,
        Guid? contrapartidaId = null, DateOnly? data = null) {
        suma = Scara.RotunjesteBani(suma);
        var zi = data ?? DateOnly.FromDateTime(DateTime.Today);
        contrapartidaId = ValideazaCreare(os, stingator, document, suma, contrapartidaId, zi, autogenerat);
        var transfer = autogenerat ? null : Cub.Materializare.Imperecheaza(os, stingator, document, suma, zi, contrapartidaId: contrapartidaId);
        var imperechere = os.CreateObject<Imperechere>();
        imperechere.DocumentStingator = stingator;
        imperechere.Document = document;
        imperechere.Suma = suma;
        imperechere.Data = zi;
        imperechere.Autogenerat = autogenerat;
        imperechere.TranzactieCubId = transfer;
        return imperechere;
    }

    internal static Guid ValideazaCreare(IObjectSpace os,
        Document stingator, Document document, decimal suma, Guid? contrapartidaId = null,
        DateOnly? data = null, bool autogenerat = false) {
        if (stingator == null || document == null)
            throw new OperareException(
                "Imperecherea leagă un document care stinge de un document stins — ambele sunt obligatorii.");
        if (data is DateOnly zi && zi != default
                && (zi < stingator.DataInregistrare || zi < document.DataInregistrare))
            throw new OperareException(
                "Data imperecherii nu poate preceda data înregistrării documentelor.");
        if (stingator.Stare != StareDocument.Operat || document.Stare != StareDocument.Operat)
            throw new OperareException("Imperecherea leagă două documente operate (registrele lor există).");
        if (document.ID == stingator.ID)
            throw new OperareException("Un document nu se poate stinge pe el însuși.");
        if ((stingator is Plata && document is Plata)
            || (stingator is Incasare && document is Incasare))
            throw new OperareException(
                "Două documente de trezorerie de același sens nu se imperechează — imperecherea stinge un document de sens opus.");
        if (suma <= 0)
            throw new OperareException("Suma imperecheată trebuie să fie pozitivă.");

        var capacitati = stingator.CapacitateStingere(os)
            ?? throw new OperareException(
                "Doar plățile/încasările și notele contabile pot stinge un document (compensarea e notă contabilă).");
        if (capacitati.Count == 0)
            throw new OperareException(
                "Documentul care stinge nu poartă nicio contrapartidă (nota contabilă cere repartitori expliciți pe linii).");

        if (!document.PoateFiStins(os))
            throw new OperareException(
                "Documentul nu poate fi stins: un virament intern nu închide nicio datorie sau creanță "
                + "(contul de tranzit se închide singur când ambele picioare sunt operate).");

        var peLaturi = capacitati.Keys
            .Where(k => k == document.PredatorId || k == document.PrimitorId)
            .ToList();
        if (peLaturi.Count == 0)
            throw new OperareException(
                "Documentul care stinge și documentul stins nu împart aceeași contrapartidă (partener/angajat).");
        if (contrapartidaId is Guid ceruta) {
            if (!peLaturi.Contains(ceruta))
                throw new OperareException(
                    "Contrapartida cerută nu apare și pe documentul care stinge, și pe laturile documentului stins.");
            peLaturi = new List<Guid> { ceruta };
        }

        var sensCerut = document.SensDeStins(os);
        var perechi = new List<(Guid Contrapartida, SensStingere Sens)>();
        foreach (var cp in peLaturi) {
            var plafon = capacitati[cp];
            if (sensCerut is SensStingere cerut) {
                if (plafon[cerut] != 0m)
                    perechi.Add((cp, cerut));
            } else {
                foreach (var sens in plafon.Sensuri)
                    perechi.Add((cp, sens));
            }
        }
        if (perechi.Count == 0)
            throw new OperareException(
                $"Documentul care stinge n-are capacitate pe sensul cerut de documentul stins ({Eticheta(sensCerut)}): "
                + "plata stinge datorii, încasarea stinge creanțe, iar nota de compensare le face pe amândouă — "
                + "fiecare jumătate pe latura ei.");
        if (perechi.Select(p => p.Contrapartida).Distinct().Count() > 1)
            throw new OperareException(
                "Documentul stins poartă MAI MULTE dintre contrapartidele documentului care stinge, pe laturi "
                + "diferite — alegeți explicit contrapartida (grupul de plafon), altfel stingerea ar consuma un "
                + "plafon la întâmplare.");
        if (perechi.Count > 1)
            throw new OperareException(
                "Documentul stins nu declară ce fel de sold poartă pe contul contrapartidei, iar documentul care "
                + "stinge are capacitate pe AMBELE sensuri față de ea (și datorie, și creanță) — jumătatea "
                + "consumată ar fi arbitrară. Stingeți-l cu un document care are o singură jumătate față de "
                + "contrapartida asta (plata stinge datorii, încasarea stinge creanțe), sau declarați natura "
                + "soldului pe tipul documentului stins.");

        var (contrapartida, sensAles) = perechi[0];
        var nominalizata = autogenerat ? 0m : Cub.Citiri.Partide.NominalizataLibera(os, stingator.ID, document.ID, data, contrapartida);
        var ramasStingator = autogenerat ? capacitati[contrapartida][sensAles]
            : Cub.Citiri.Partide.Disponibil(os, stingator.ID, contrapartida, sensAles) + nominalizata;
        if (suma > ramasStingator)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul neasignat al documentului care stinge, "
                + $"pe sensul {Eticheta(sensAles)} ({ramasStingator:0.##}).");
        var ramasDocument = Ramas(os, document.ID) + nominalizata;
        if (suma > ramasDocument)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul nestins al documentului ({ramasDocument:0.##}).");
        return contrapartida;
    }

    public static decimal AsignatFataDe(IObjectSpace os, Guid stingatorId, Guid contrapartidaId,
        SensStingere sens) => Cub.Citiri.Partide.Capacitate(os, stingatorId, contrapartidaId, sens)
            - Cub.Citiri.Partide.Disponibil(os, stingatorId, contrapartidaId, sens);

    static string Eticheta(SensStingere? sens) => sens switch {
        SensStingere.Datorie => "datorie",
        SensStingere.Creanta => "creanță",
        _ => "nedeclarat"
    };
}
