using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ---------------------------------------------------------------------------
// F23-V6 — funcția LEGII, mutată, dă aceleași cifre
// ---------------------------------------------------------------------------
// `ClasaFiscala.APartenerului` e corpul mutat din `D394Proiectii.TipPartener`
// (F23-D2). Cele 13 aserțiuni de pe `TipPartener` din `VerificaD394` rămân
// neatinse și acoperă cifra formularului; aici se măsoară CUSĂTURA — că enum-ul
// și cifra sunt aceeași funcție, pe toate cele patru clase, în ambele grafii ale
// axei „înregistrat bate tot”.
static class VerificaF23ClasaFiscala {
    public static void Ruleaza(Suita s) {
        (TipPersoana Tip, string Tara, bool Inregistrat, ClasaFiscalaPartener Asteptat)[] combinatii = [
            (TipPersoana.Juridica, "RO", true, ClasaFiscalaPartener.InregistratRo),
            (TipPersoana.Juridica, "RO", false, ClasaFiscalaPartener.NeinregistratRo),
            (TipPersoana.Fizica, "RO", true, ClasaFiscalaPartener.InregistratRo),
            (TipPersoana.Fizica, "RO", false, ClasaFiscalaPartener.NeinregistratRo),
            (TipPersoana.Juridica, "DE", true, ClasaFiscalaPartener.InregistratRo),
            (TipPersoana.Juridica, "DE", false, ClasaFiscalaPartener.Ue),
            (TipPersoana.Juridica, "US", true, ClasaFiscalaPartener.InregistratRo),
            (TipPersoana.Juridica, "US", false, ClasaFiscalaPartener.ExtraUe),
        ];
        var divergente = new List<string>();
        var masurate = new List<string>();
        foreach (var (tip, tara, inregistrat, asteptat) in combinatii) {
            var clasa = ClasaFiscala.APartenerului(tip, tara, inregistrat);
            var cifra = D394Proiectii.TipPartener(tip, tara, inregistrat);
            masurate.Add($"{tip}/{tara}/{(inregistrat ? "înreg" : "neînreg")}→{(int)clasa} ({clasa})");
            if ((int)clasa != cifra || clasa != asteptat)
                divergente.Add($"{tip}/{tara}/{inregistrat}: enum {(int)clasa}, D394 {cifra}, așteptat "
                    + $"{(int)asteptat}");
        }
        Console.WriteLine($"     MĂSURAT (F23-V6): {string.Join(", ", masurate)}.");
        s.Check("F23-V6 `ClasaFiscala.APartenerului` (enum) și `D394Proiectii.TipPartener` (cifra formularului) "
            + "sunt ACEEAȘI funcție a legii, mutată o singură dată: pe cele 8 combinații PF/PJ × RO/DE/US × "
            + "înregistrat/nu valorile coincid — „înregistrat bate tot” (71b) taie prin fel și prin țară, apoi PF "
            + "⇒ 2, RO neînregistrat ⇒ 2, UE ⇒ 3, restul ⇒ 4"
            + (divergente.Count > 0 ? $" — divergențe: {string.Join("; ", divergente)}" : ""),
            divergente.Count == 0);
    }
}
