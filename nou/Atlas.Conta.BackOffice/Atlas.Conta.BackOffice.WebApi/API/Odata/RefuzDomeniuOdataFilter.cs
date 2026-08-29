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
/// F20-D7 (închide 70-r1): pe rutele <c>api/odata/*</c>, refuzurile de DOMENIU
/// ies <c>422 application/json</c> cu ACEEAȘI formă ca pe REST
/// (<see cref="EroriDto"/>), nu <c>400 text/plain</c>.
/// </summary>
/// <remarks>
/// <para>
/// Problema: <c>GardianEditare</c> aruncă <c>OperareException :
/// UserFriendlyException</c> din <c>ObjectSpace.Committing</c>, iar pe rutele
/// OData nu există try/catch al nostru (controllerele sunt ale XAF-ului).
/// Filtrul global al DevExpress
/// (<c>DevExpress.ExpressApp.WebApi.Mvc.UserFriendlyExceptionFilter</c>,
/// înregistrat în <c>StartupExtensions.cs:82</c>) transformă orice
/// <c>IUserFriendlyException</c> într-un <c>ContentResult</c> cu textul brut și
/// status 400 — pe care <c>nucleu/http.ts</c> îl citește ca „eroare tehnică",
/// fiindcă el caută <c>Erori[]</c>. Blocantul latent al oricărui ecran de
/// nomenclator (F20-D8).
/// </para>
/// <para>
/// Mecanica: filtrele de excepție ÎNVELESC acțiunea, deci se execută în ordine
/// INVERSĂ — cel cu <c>Order</c> mai mare e cel mai din interior și primește
/// excepția PRIMUL. Al DevExpress e global cu <c>Order</c> implicit 0; al
/// nostru cere <c>int.MaxValue</c>, deci apucă înainte, iar
/// <c>ExceptionHandled = true</c> oprește propagarea (invocatorul nu mai cheamă
/// filtrele exterioare). Filtrul DevExpress rămâne ÎNREGISTRAT și neatins —
/// nu se scoate din pipeline: el continuă să servească rutele XAF care nu sunt
/// ale noastre.
/// </para>
/// <para>
/// Ce NU atinge, deliberat: <c>IUserFriendlySecurityException</c> (403 e al
/// securității, iar clientul îl tratează separat) și
/// <c>HttpUserFriendlyException</c> (își poartă singură statusul). Rutele REST
/// nu trec pe aici — acolo traducerea e în <c>ContaApiController.Domeniu</c>,
/// înaintea oricărui filtru; <c>$metadata</c> și query-urile care nu aruncă
/// rămân, evident, neschimbate.
/// </para>
/// </remarks>
public sealed class RefuzDomeniuOdataFilter : IExceptionFilter {
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

    public RefuzDomeniuOdataFilter(IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> optiuni) {
        // ACELEAȘI opțiuni ca răspunsurile REST (`JsonApi.Configureaza`:
        // `PropertyNamingPolicy = null`). Hand-serializarea cu opțiuni proprii
        // ar fi produs `erori` în loc de `Erori` — exact câmpul pe care îl caută
        // clientul, deci un 422 pe care l-ar fi ratat la fel ca pe 400-ul de azi.
        json = optiuni.Value.JsonSerializerOptions;
    }

    public void OnException(ExceptionContext context) {
        if (!context.HttpContext.Request.Path.StartsWithSegments(
                RutaOdata, StringComparison.OrdinalIgnoreCase))
            return;

        var dto = Refuz(context.Exception);
        if (dto == null)
            return;

        // `ContentResult`, nu `ObjectResult`: pe o rută OData formatterele de
        // ieșire sunt cele ale OData, iar un tip care nu e în modelul EDM n-are
        // formatter garantat. Conținutul serializat de mână ocolește negocierea
        // și fixează `application/json` — contractul e al nostru, nu al
        // formatterului care se întâmplă să fie selectat.
        context.Result = new ContentResult {
            StatusCode = StatusCodes.Status422UnprocessableEntity,
            ContentType = "application/json; charset=utf-8",
            Content = JsonSerializer.Serialize(dto, json),
        };
        context.ExceptionHandled = true;
    }

    // `null` = nu e refuz de domeniu — excepția merge mai departe (filtrul
    // DevExpress, apoi handler-ul de erori al host-ului).
    static EroriDto Refuz(Exception ex) {
        if (ex is IUserFriendlySecurityException || ex is HttpUserFriendlyException)
            return null;
        if (ex is IUserFriendlyException)
            return EroriDto.DinMesaj(ex.Message);
        // Violările de constraint DB (60a): aceeași traducere ca pe REST
        // (`ContaApiController.RefuzDeDomeniu`) și ca în Blazor (39a) —
        // template-urile RO se aplică la pornire (`MesajeConstraintRo.Aplica`).
        var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(ex);
        return violare == null
            ? null
            : EroriDto.DinMesaj(Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare));
    }
}
