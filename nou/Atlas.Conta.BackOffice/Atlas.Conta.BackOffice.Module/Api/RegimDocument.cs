using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>Regimul pe stare al unui document (106): editabilitatea și, pentru fiecare comandă cunoscută, motivul indisponibilității sau null.</summary>
public sealed class RegimDocument {
    public const string Sterge = "Sterge";
    public const string Regenereaza = "Regenereaza";
    public const string Distribuie = "Distribuie";
    public const string Stinge = "Stinge";
    public const string GenereazaDescarcarea = "GenereazaDescarcarea";

    /// <summary>Vocabularul comenzilor: cele ale coajei plus cele contribuite de tipuri.</summary>
    public static readonly IReadOnlySet<string> ComenziCunoscute = new HashSet<string>(
        Enum.GetNames<ComandaDocument>().Concat([Sterge, Regenereaza, Distribuie, Stinge, GenereazaDescarcarea]));

    public StareDocument Stare { get; }
    public bool Editabil { get; }
    public IReadOnlyDictionary<string, string> Comenzi { get; }

    RegimDocument(StareDocument stare, bool editabil, IReadOnlyDictionary<string, string> comenzi) {
        Stare = stare;
        Editabil = editabil;
        Comenzi = comenzi;
    }

    public bool Poate(string comanda) => Comenzi.TryGetValue(comanda, out var motiv) && motiv == null;
    public bool Poate(ComandaDocument comanda) => Poate(comanda.ToString());
    public string Motiv(string comanda) => Comenzi.GetValueOrDefault(comanda);

    /// <summary>Componenta ieftină a regimului, pe entitate: antetul și liniile se culeg doar în Draft.</summary>
    public static bool EsteEditabil(Document doc) => doc == null || doc.Stare == StareDocument.Draft;

    /// <summary>Componenta ieftină a regimului pentru ștergere, pe entitate (106j): null = se poate șterge.</summary>
    public static string MotivStergere(Document doc) => doc?.Stare switch {
        null or StareDocument.Draft => null,
        StareDocument.Operat => "Documentul operat se stornează, nu se șterge.",
        _ => "Documentul e stornat.",
    };

    public static RegimDocument Calculeaza(IObjectSpace os, Guid documentId) =>
        Calculeaza(os, Rezolva.Cere<Document>(os, documentId, "Documentul"));

    public static RegimDocument Calculeaza(IObjectSpace os, Document doc) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(doc);
        var regim = new Constructor(os, doc);
        regim.Decide(Sterge, MotivStergere(doc));
        switch (doc.Stare) {
            case StareDocument.Draft:
                regim.Permite(ComandaDocument.Opereaza);
                regim.Permite(ComandaDocument.Valideaza);
                regim.Refuza(ComandaDocument.AnuleazaOperarea, "Doar un document Operat poate fi anulat.");
                regim.Refuza(ComandaDocument.Storneaza, "Doar un document Operat poate fi stornat.");
                regim.Refuza(ComandaDocument.Corecteaza, "Se corectează doar un document operat.");
                break;
            case StareDocument.Operat:
                regim.Refuza(ComandaDocument.Opereaza, "Documentul e deja operat.");
                regim.Refuza(ComandaDocument.Valideaza, "Documentul e deja operat.");
                var dependenti = regim.Dependenti();
                regim.Decide(ComandaDocument.AnuleazaOperarea,
                    GardianPerioada.MotivInchisa(os, doc.DataInregistrare) ?? dependenti);
                regim.Decide(ComandaDocument.Storneaza, dependenti);
                regim.Decide(ComandaDocument.Corecteaza, dependenti);
                break;
            default:
                foreach (var comanda in Enum.GetNames<ComandaDocument>())
                    regim.Refuza(comanda, "Documentul e stornat.");
                break;
        }
        doc.ContribuieRegim(os, regim);
        if (regim.Comenzi[Sterge] != MotivStergere(doc))
            throw new InvalidOperationException("Ștergerea se decide numai pe stare (106j); tipul nu o poate schimba.");
        return new RegimDocument(doc.Stare, EsteEditabil(doc), regim.Comenzi);
    }

    /// <summary>Contribuția tipului la regim: comenzile proprii, pe starea și datele documentului.</summary>
    public sealed class Constructor {
        readonly Dictionary<string, string> comenzi = [];
        string dependenti;
        bool dependentiCititi;

        internal Constructor(IObjectSpace os, Document doc) {
            ObjectSpace = os;
            Document = doc;
        }

        public IObjectSpace ObjectSpace { get; }
        public Document Document { get; }
        public bool Draft => Document.Stare == StareDocument.Draft;
        public bool Operat => Document.Stare == StareDocument.Operat;
        internal IReadOnlyDictionary<string, string> Comenzi => comenzi;

        public void Permite(string comanda) => Decide(comanda, null);
        public void Permite(ComandaDocument comanda) => Permite(comanda.ToString());
        public void Refuza(string comanda, string motiv) => Decide(comanda, motiv ?? throw new ArgumentNullException(nameof(motiv)));
        public void Refuza(ComandaDocument comanda, string motiv) => Refuza(comanda.ToString(), motiv);
        public void Decide(ComandaDocument comanda, string motiv) => Decide(comanda.ToString(), motiv);

        public void Decide(string comanda, string motiv) {
            if (!ComenziCunoscute.Contains(comanda))
                throw new ArgumentException($"Comanda „{comanda}” nu e în vocabularul regimului.", nameof(comanda));
            comenzi[comanda] = motiv;
        }

        /// <summary>Primul motiv al gardienilor de dependenți (latura pereche, conexele, împerecherile), citit o singură dată.</summary>
        public string Dependenti() {
            if (!dependentiCititi) {
                dependenti = MotorOperare.MotivLaturaPerecheOperata(ObjectSpace, Document)
                    ?? MotorOperare.MotivConexeOperate(ObjectSpace, Document)
                    ?? MotorOperare.MotivImperecheri(ObjectSpace, Document);
                dependentiCititi = true;
            }
            return dependenti;
        }
    }
}
