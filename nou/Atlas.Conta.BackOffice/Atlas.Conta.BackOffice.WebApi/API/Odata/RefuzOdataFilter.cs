using System.Text.Json;
using Atlas.Conta.BackOffice.Module.Api;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Atlas.Conta.BackOffice.WebApi.API.Odata;

/// <summary>
/// F22-D4 (închide 70-r1 și 77-r8 pe ușa OData): pe rutele <c>api/odata/*</c>,
/// ORICE refuz — de acces (403), de subiect invizibil (404) sau de DOMENIU
/// (422) — iese <c>application/json</c> cu ACEEAȘI formă ca pe REST
/// (<see cref="EroriDto"/>), nu <c>text/plain</c> cu textul brut al DevExpress.
/// </summary>
/// <remarks>
/// <para>
/// Problema: pe rutele OData nu există try/catch al nostru (controllerele sunt
/// ale XAF-ului). Filtrul global al DevExpress
/// (<c>DevExpress.ExpressApp.WebApi.Mvc.UserFriendlyExceptionFilter</c>,
/// înregistrat în <c>StartupExtensions.cs:82</c>) face din orice
/// <c>IUserFriendlyException</c> un <c>ContentResult</c> cu textul brut: 403 pe
/// <c>IUserFriendlySecurityException</c>, statusul propriu pe
/// <c>HttpUserFriendlyException</c>, 400 pe rest. Pe sârmă ieșea deci
/// <c>text/plain</c>, iar <c>nucleu/http.ts</c> — care caută <c>Erori[]</c> — le
/// citea pe toate ca „eroare tehnică" și inventa în TS un text pe care serverul
/// nu-l spusese (defectul închis de F22-D8).
/// </para>
/// <para>
/// Mecanica: filtrele de excepție ÎNVELESC acțiunea, deci se execută în ordine
/// INVERSĂ — cel cu <c>Order</c> mai mare e cel mai din interior și primește
/// excepția PRIMUL. Al DevExpress e global cu <c>Order</c> implicit 0 (verificat
/// pe surse: <c>o.Filters.Add(typeof(UserFriendlyExceptionFilter))</c>, fără
/// ordine); al nostru cere <c>int.MaxValue</c>, deci apucă înainte, iar
/// <c>ExceptionHandled = true</c> oprește propagarea (invocatorul nu mai cheamă
/// filtrele exterioare). Filtrul DevExpress rămâne ÎNREGISTRAT și neatins — nu
/// se scoate din pipeline: el continuă să servească rutele XAF care nu sunt ale
/// noastre (prefixul rutei e singurul criteriu de mai jos), iar scoaterea lui
/// ar fi însemnat să răspundem noi pentru un pipeline pe care nu-l controlăm.
/// </para>
/// <para>
/// Ce atinge acum, DELIBERAT (înainte le lăsa pe amândouă filtrului DevExpress):
/// <list type="bullet">
/// <item><c>IUserFriendlySecurityException</c> ⇒ <b>403</b>. Sunt două surse:
/// <see cref="Atlas.Conta.BackOffice.Module.Api.RefuzAcces"/>, ridicat de pasul
/// zero al lui <c>GardianEditare</c> (F22-D3) cu fraza NOASTRĂ, și
/// <c>UserFriendlyEFCoreSecurityException</c>, pe care
/// <c>SecuredEFCoreObjectSpace.DoCommit</c> o împachetează din verificarea de
/// permisiuni a lui <c>SaveChanges</c> — a doua are mesajul DevExpress, în
/// engleză (70-r5), asumat: pe calea normală pasul zero răspunde primul.</item>
/// <item><c>HttpUserFriendlyException</c> ⇒ <b>statusul ei</b>. Ea e felul în
/// care <c>DataService</c> spune „nu am găsit cheia" —
/// <c>HttpUserFriendlyException(404, "Not Found")</c> — iar pe ușa OData
/// „negăsit" înseamnă și „filtrat de securitate" (<c>SecurityQueryCompiler</c>
/// înfășoară orice query, inclusiv <c>Find</c>). Deci 404-ul primește fraza
/// noastră unică (<c>Refuzuri.Invizibil</c>), care nu distinge cele două cazuri;
/// pentru orice alt status rămâne mesajul ei.</item>
/// </list>
/// Ordinea de mai jos e cea de pe sârmă (F22-D1): 404 → 403 → 422. Rutele REST
/// nu trec pe aici — acolo traducerea e în <c>ContaApiController.Domeniu</c>,
/// înaintea oricărui filtru; <c>$metadata</c> și query-urile care nu aruncă
/// rămân, evident, neschimbate.
/// </para>
/// </remarks>
public sealed class RefuzOdataFilter : IExceptionFilter {
    // Prefixul rutei OData, exact cel din `AddRouteComponents` (Startup).
    const string RutaOdata = "/api/odata";

    /// <summary>
    /// Ordinea cu care se ÎNREGISTREAZĂ filtrul (`Filters.Add&lt;T&gt;(order)`).
    /// Trăiește aici, nu ca literal în `Startup`, fiindcă e jumătate din
    /// mecanica descrisă mai sus — iar la înregistrarea prin `TypeFilterAttribute`
    /// ordinea o dă FABRICA, nu instanța: un `IOrderedFilter` implementat pe
    /// clasă ar fi fost ignorat tăcut.
    /// </summary>
    public const int OrdineInterioara = int.MaxValue;

    readonly JsonSerializerOptions json;

    public RefuzOdataFilter(IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> optiuni) {
        // ACELEAȘI opțiuni ca răspunsurile REST (`JsonApi.Configureaza`:
        // `PropertyNamingPolicy = null`). Hand-serializarea cu opțiuni proprii
        // ar fi produs `erori` în loc de `Erori` — exact câmpul pe care îl caută
        // clientul, deci un refuz pe care l-ar fi ratat la fel ca pe 400-ul
        // text/plain de dinainte.
        json = optiuni.Value.JsonSerializerOptions;
    }

    public void OnException(ExceptionContext context) {
        if (!context.HttpContext.Request.Path.StartsWithSegments(
                RutaOdata, StringComparison.OrdinalIgnoreCase))
            return;

        var refuz = Refuz(context.Exception);
        if (refuz == null)
            return;

        // `ContentResult`, nu `ObjectResult`: pe o rută OData formatterele de
        // ieșire sunt cele ale OData, iar un tip care nu e în modelul EDM n-are
        // formatter garantat. Conținutul serializat de mână ocolește negocierea
        // și fixează `application/json` — contractul e al nostru, nu al
        // formatterului care se întâmplă să fie selectat.
        var (status, dto) = refuz.Value;
        context.Result = new ContentResult {
            StatusCode = status,
            ContentType = "application/json; charset=utf-8",
            Content = JsonSerializer.Serialize(dto, json),
        };
        context.ExceptionHandled = true;
    }

    // `null` = nu e un refuz pe care să-l traducem — excepția merge mai departe
    // (filtrul DevExpress, apoi handler-ul de erori al host-ului).
    static (int Status, EroriDto Dto)? Refuz(Exception ex) {
        // 404 ÎNAINTEA lui 403: `HttpUserFriendlyException` nu e o excepție de
        // securitate, dar poartă tocmai statusul cu care ușa OData spune „cheia
        // asta nu-ți e vizibilă". Testul e pe TIP, nu pe interfață, fiindcă ea
        // implementează `IUserFriendlyException` și ar fi căzut altfel în 422.
        if (ex is HttpUserFriendlyException http) {
            var status = (int)http.HttpStatusCode;
            return (status, EroriDto.DinMesaj(
                status == StatusCodes.Status404NotFound ? Refuzuri.Invizibil : http.Message));
        }
        if (ex is IUserFriendlySecurityException)
            return (StatusCodes.Status403Forbidden, EroriDto.DinMesaj(ex.Message));
        if (ex is IUserFriendlyException)
            return (StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(ex.Message));
        // Violările de constraint DB (60a): aceeași traducere ca pe REST
        // (`ContaApiController.RefuzDeDomeniu`) și ca în Blazor (39a) —
        // template-urile RO se aplică la pornire (`MesajeConstraintRo.Aplica`).
        var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(ex);
        return violare == null
            ? null
            : (StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(
                Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)));
    }
}
