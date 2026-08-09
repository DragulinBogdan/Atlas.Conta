using Atlas.DXF.EfCore.Database.Exceptions;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Template-urile în ROMÂNĂ ale traducerii violărilor de constraint DB (39a,
// Atlas.DXF.EfCore). O SINGURĂ sursă pentru ambele host-uri: Blazor le consumă
// prin `AtlasDxfExceptionService` (mesajul prietenos în UI), WebApi prin
// catch-ul din `ContaApiController.Domeniu` (422 cu mesaj de domeniu, F4-M2).
// Template-urile sunt statice pe proces — aplicarea e idempotentă.
public static class MesajeConstraintRo {
    public static void Aplica() {
        ConstraintViolationMessages.ForeignKeyDeleteTemplate =
            "Nu se poate șterge înregistrarea „{0}”: există înregistrări „{1}” care o referă.";
        ConstraintViolationMessages.ForeignKeyTemplate =
            "Operația intră în conflict cu o referință între înregistrările „{0}” și „{1}”.";
        ConstraintViolationMessages.UniqueTemplate =
            "Există deja o înregistrare „{0}” cu aceleași valori pentru {1}.";
        ConstraintViolationMessages.NotNullTemplate =
            "„{1}” este obligatoriu pe „{0}”.";
        ConstraintViolationMessages.CheckTemplate =
            "Înregistrarea „{0}” încalcă regula „{1}”.";
        ConstraintViolationMessages.FallbackTemplate =
            "Operația încalcă restricția de bază de date „{0}”.";
    }
}
