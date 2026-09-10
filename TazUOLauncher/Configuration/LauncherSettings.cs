using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TazUOLauncher;

internal class LauncherSettings
{
    public static LauncherSaveFile GetLauncherSaveFile { get; } = LauncherSaveFile.Get();
    public static Version LocalLauncherVersion { get; } = LauncherVersion.GetLauncherVersion();

    internal class LauncherSaveFile
    {
        public string LastSelectedProfileName { get; set; } = string.Empty;
        public ReleaseChannel DownloadChannel { get; set; } = ReleaseChannel.MAIN;
        public bool AutoDownloadUpdates { get; set; } = false;

        // Which channel actually produced the installed client. Distinct from
        // DownloadChannel, which is the channel being followed: a one-shot install
        // from the Tools menu changes this but not that. A Holiday dev build reports
        // the same version as the release it was cut from, so this is the only way to
        // tell the two apart once installed.
        public ReleaseChannel InstalledChannel { get; set; } = ReleaseChannel.INVALID;

        // Where the launcher window was last placed, so it reopens where it was left
        // instead of wherever the OS decides to put it. Null until the first close,
        // and null rather than 0 so "never saved" is distinct from "saved at 0,0".
        public int? WindowPositionX { get; set; }
        public int? WindowPositionY { get; set; }

        public static LauncherSaveFile Get()
        {
            try
            {
                var p = Path.Combine(PathHelper.LauncherPath, "launcherdata.json");
                if (File.Exists(p))
                {
                    return JsonSerializer.Deserialize<LauncherSaveFile>(File.ReadAllText(p)) ?? new LauncherSaveFile();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new LauncherSaveFile();
        }

        public async Task Save()
        {
            await Task.Run(SaveSync);
        }

        /// <summary>
        /// Writes on the calling thread. Use this while shutting down: the async save
        /// hands the work to a background thread that the exiting process can cut off
        /// before it ever reaches disk.
        /// </summary>
        public void SaveSync()
        {
            try
            {
                var targetPath = Path.Combine(PathHelper.LauncherPath, "launcherdata.json");
                var tempPath = targetPath + ".tmp";

                File.WriteAllText(tempPath, JsonSerializer.Serialize<LauncherSaveFile>(this));
                File.Move(tempPath, targetPath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public LauncherSaveFile() { }
    }
}
