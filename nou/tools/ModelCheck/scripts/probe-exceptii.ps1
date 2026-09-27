#requires -Version 7.0
param([Parameter(Mandatory)][string]$Director)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
if (!$IsWindows) { throw 'Proba dialogurilor de eroare cere Windows.' }
$proba = Join-Path $Director 'proba-exceptii'
New-Item -ItemType Directory -Path $proba -Force | Out-Null
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $proba 'Proba.csproj') -Encoding utf8
@'
using System;
using System.Runtime.InteropServices;
class Program {
    [DllImport("kernel32.dll")] static extern uint GetErrorMode();
    static int Main(string[] args) {
        uint mode = GetErrorMode();
        Console.WriteLine($"MOD_ERORI={mode}");
        if ((mode & 3) != 3) return 2;
        if (args.Length > 0 && args[0] == "cadere")
            throw new InvalidOperationException("PROBA_EXCEPTIE_NECAPTURATA_FARA_BAZA");
        Console.WriteLine("PROBA_CONTINUA_DUPA_CADERE");
        return 0;
    }
}
'@ | Set-Content -LiteralPath (Join-Path $proba 'Program.cs') -Encoding utf8
& dotnet build (Join-Path $proba 'Proba.csproj') --nologo 2>&1 | Out-Host
if ($LASTEXITCODE -ne 0) { throw 'Compilarea probei de infrastructură a eșuat.' }
$dll = Join-Path $proba 'bin/Debug/net10.0/Proba.dll'
foreach ($pas in @('cadere', 'normal')) {
    $start = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $start.ArgumentList.Add($dll)
    $start.ArgumentList.Add($pas)
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $proces = [Diagnostics.Process]::Start($start)
    try {
        $stdout = $proces.StandardOutput.ReadToEndAsync()
        $stderr = $proces.StandardError.ReadToEndAsync()
        if (!$proces.WaitForExit(30000)) {
            $proces.Kill($true)
            $proces.WaitForExit()
            throw "Proba $pas nu s-a încheiat în 30 s; procesul de probă a fost oprit."
        }
        $iesire = $stdout.GetAwaiter().GetResult()
        $eroare = $stderr.GetAwaiter().GetResult()
        $iesire | Set-Content -LiteralPath (Join-Path $proba "$pas.stdout.log") -Encoding utf8
        $eroare | Set-Content -LiteralPath (Join-Path $proba "$pas.stderr.log") -Encoding utf8
        if ($pas -eq 'cadere') {
            if ($proces.ExitCode -ne -532462766 -or $eroare -notmatch 'PROBA_EXCEPTIE_NECAPTURATA_FARA_BAZA' -or
                $eroare -notmatch 'Program.Main') {
                throw "Căderea nu a produs excepția și stack trace-ul așteptate (exit $($proces.ExitCode))."
            }
        }
        elseif ($proces.ExitCode -ne 0 -or $iesire -notmatch 'PROBA_CONTINUA_DUPA_CADERE') {
            throw 'Execuția normală de după cădere a eșuat.'
        }
        Write-Host "OK infrastructură $pas`: exit $($proces.ExitCode), proces încheiat fără intervenție."
    }
    finally { $proces.Dispose() }
}
