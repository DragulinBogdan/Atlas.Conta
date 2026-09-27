using System.ComponentModel;
using System.Runtime.CompilerServices;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Conta.BackOffice.Module.Culegere;

/// <summary>
/// Adaptorul XAF al salvării: pe ObjectSpace-urile securizate ale ecranelor, culegerea se
/// normalizează la Committing, înaintea gardianului. `Api/*Apply` cheamă explicit același pas.
/// </summary>
public sealed class CulegereLaCommitXaf : IObjectSpaceCustomizer {
    static readonly ConditionalWeakTable<IObjectSpace, object> abonate = new();

    public void OnObjectSpaceCreated(IObjectSpace objectSpace) {
        if (objectSpace == null)
            return;
        lock (abonate) {
            if (abonate.TryGetValue(objectSpace, out _))
                return;
            abonate.Add(objectSpace, abonate);
        }
        objectSpace.Committing += OnCommitting;
    }

    static void OnCommitting(object sender, CancelEventArgs e) {
        if (sender is IObjectSpace os)
            CulegereDocument.InainteDeSalvare(os);
    }
}

public static class CulegereLaCommitXafExtensions {
    /// <summary>Se înregistrează înaintea gardianului de editare: customizerii rulează în ordinea înregistrării.</summary>
    public static IServiceCollection AddContaCulegereXaf(this IServiceCollection services) {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IObjectSpaceCustomizer, CulegereLaCommitXaf>());
        return services;
    }
}
