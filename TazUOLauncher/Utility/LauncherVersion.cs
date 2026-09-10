using System;
using System.Reflection;

namespace TazUOLauncher;

internal static class LauncherVersion
{
    public static Version GetLauncherVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        if (version != null)
            return version;

        return new Version(0, 0, 0);
    }

    public static string ToHumanReable(this Version v, bool prependv = true)
    {
        if (v == null)
            return string.Empty;

        string pv = prependv ? "v" : string.Empty;

        // Include the fourth part when there is one: CI stamps the build number
        // there, and without it two different builds display as the same version.
        // Client versions are three-part, so their Revision is -1 and nothing is
        // appended for them.
        string revision = v.Revision > 0 ? $".{v.Revision}" : string.Empty;

        return $"{pv}{v.Major}.{v.Minor}.{v.Build}{revision}";
    }
}