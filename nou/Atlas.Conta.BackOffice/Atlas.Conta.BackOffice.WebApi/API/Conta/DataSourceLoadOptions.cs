using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Puntea dintre query string-ul DevExtreme (`?take=20&sort=[...]&filter=[...]`)
// și `DataSourceLoader`.
//
// De ce e scris de mână: pachetul `DevExtreme.AspNet.Data` conține DOAR motorul
// (`DataSourceLoader`, `DataSourceLoadOptionsBase`,
// `Helpers.DataSourceLoadOptionsParser`) — binder-ul de MVC NU e în pachet, e
// fișierul pe care șabloanele DevExpress ASP.NET Core îl COPIAZĂ în proiect
// (verificat pe conținutul lui 5.1.0: namespace-urile publicate sunt
// `DevExtreme.AspNet.Data*`, niciun `DevExtreme.AspNet.Mvc`). Deci lipsa lui nu
// e un blocaj, e forma normală de consum — snippet-ul canonic, verbatim.
[ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))]
public class DataSourceLoadOptions : DataSourceLoadOptionsBase { }

public class DataSourceLoadOptionsBinder : IModelBinder {
    public Task BindModelAsync(ModelBindingContext bindingContext) {
        var loadOptions = new DataSourceLoadOptions();
        DataSourceLoadOptionsParser.Parse(loadOptions, key => bindingContext.ValueProvider.GetValue(key).FirstOrDefault());
        bindingContext.Result = ModelBindingResult.Success(loadOptions);
        return Task.CompletedTask;
    }
}
