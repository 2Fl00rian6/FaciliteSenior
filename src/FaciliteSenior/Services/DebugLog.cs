using System.IO;

namespace FaciliteSenior.Services;

/// <summary>
/// TODO temporaire : journal de diagnostic pour l'ecran noir du navigateur integre.
/// A retirer une fois le probleme identifie et corrige.
/// </summary>
internal static class DebugLog
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FaciliteSenior",
        "browser-debug.log");

    private static readonly object Gate = new();

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Journal best-effort : ne doit jamais faire planter l'application.
        }
    }
}
