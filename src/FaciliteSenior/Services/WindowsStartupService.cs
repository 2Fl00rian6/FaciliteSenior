using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace FaciliteSenior.Services;

public sealed class WindowsStartupService : IWindowsStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupValueName = "FaciliteSenior";

    public void ApplyStartupSetting(bool startWithWindows)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

            if (runKey is null)
            {
                return;
            }

            if (!startWithWindows)
            {
                runKey.DeleteValue(StartupValueName, throwOnMissingValue: false);
                return;
            }

            var executablePath = ResolveExecutablePath();

            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return;
            }

            runKey.SetValue(StartupValueName, $"\"{executablePath}\"", RegistryValueKind.String);
        }
        catch
        {
            // On garde un comportement silencieux : l'application reste utilisable meme si Windows refuse l'ecriture.
        }
    }

    private static string ResolveExecutablePath()
    {
        var processPath = Environment.ProcessPath;

        if (!string.IsNullOrWhiteSpace(processPath)
            && File.Exists(processPath)
            && !processPath.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase)
            && !processPath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
    }
}
