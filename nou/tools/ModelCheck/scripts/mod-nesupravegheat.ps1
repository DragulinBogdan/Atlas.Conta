function Activeaza-ModNesupravegheat {
    if (!$IsWindows) { return $null }
    if (!('Atlas.Verificari.ErrorMode' -as [type])) {
        Add-Type -TypeDefinition @'
using System.Runtime.InteropServices;
namespace Atlas.Verificari {
    public static class ErrorMode {
        [DllImport("kernel32.dll")] public static extern uint GetErrorMode();
        [DllImport("kernel32.dll")] public static extern uint SetErrorMode(uint mode);
    }
}
'@
    }
    $anterior = [Atlas.Verificari.ErrorMode]::GetErrorMode()
    # SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX; moștenite de procesele copil.
    [Atlas.Verificari.ErrorMode]::SetErrorMode($anterior -bor 3) | Out-Null
    return $anterior
}

function Restaureaza-ModErori($Anterior) {
    if ($null -ne $Anterior) {
        [Atlas.Verificari.ErrorMode]::SetErrorMode($Anterior) | Out-Null
    }
}
