using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace TazUOLauncher;

internal static class UpdateHelper
{
    public static ConcurrentDictionary<ReleaseChannel, GitHubReleaseData> ReleaseData = new();

    public static bool HaveData(ReleaseChannel channel) { return ReleaseData.ContainsKey(channel) && ReleaseData[channel] != null; }

    public static async Task GetAllReleaseData(ReleaseChannel priorityChannel)
    {
        if (BuildInfo.IsDebug)
        {
            var d = new GitHubReleaseData() { name = "Version v10.10.10.0"};
            if (!ReleaseData.TryAdd(priorityChannel, d))
                ReleaseData[priorityChannel] = d;
            return;
        }
        
        await TryGetReleaseData(priorityChannel);
        await Task.Delay(1000);
        await TryGetReleaseData(ReleaseChannel.LAUNCHER);
    }

    private static async Task<GitHubReleaseData?> TryGetReleaseData(ReleaseChannel channel)
    {
        string url;
        
        Console.WriteLine($"Grabbing release data for {channel}...");

        switch (channel)
        {
            case ReleaseChannel.MAIN:
                url = CONSTANTS.MAIN_CHANNEL_RELEASE_URL;
                break;
            case ReleaseChannel.DEV:
                url = CONSTANTS.DEV_CHANNEL_RELEASE_URL;
                break;
            case ReleaseChannel.LAUNCHER:
                url = CONSTANTS.LAUNCHER_RELEASE_URL;
                break;
            case ReleaseChannel.NET472:
                url = CONSTANTS.NET472_CHANNEL_RELEASE_URL;
                break;
            case ReleaseChannel.HOLIDAY:
                url = CONSTANTS.HOLIDAY_CHANNEL_RELEASE_URL;
                break;
            case ReleaseChannel.HOLIDAY_DEV:
                url = CONSTANTS.HOLIDAY_DEV_CHANNEL_RELEASE_URL;
                break;
            default:
                url = CONSTANTS.MAIN_CHANNEL_RELEASE_URL;
                break;
        }

        return await Task.Run(async () =>
        {
            var d = await TryGetReleaseData(url);

            if (d != null)
                if (!ReleaseData.TryAdd(channel, d))
                    ReleaseData[channel] = d;

            return d;
        });
    }

    private static async Task<GitHubReleaseData?> TryGetReleaseData(string url)
    {
        HttpRequestMessage restApi = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
        };
        restApi.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        restApi.Headers.Add("User-Agent", "Public");

        try
        {
            using var httpClient = new HttpClient();
            string jsonResponse = await httpClient.Send(restApi).Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubReleaseData>(jsonResponse);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public static async Task<string> GetNews(ReleaseChannel channel)
    {
        string url;

        if (channel is ReleaseChannel.HOLIDAY or ReleaseChannel.HOLIDAY_DEV)
        {
            // Holiday Edition keeps its changelog in its own repo, so it does not fit
            // the branch-name substitution the other channels use.
            url = CONSTANTS.HOLIDAY_CHANGE_LOG_URL;
        }
        else
        {
            string chan = "dev";
            switch (channel)
            {
                case ReleaseChannel.MAIN:
                    chan = "main";
                    break;
                case ReleaseChannel.NET472:
                    chan = "legacy";
                    break;
            }
            url = string.Format(CONSTANTS.CHANGE_LOG_URL, chan);
        }

        Console.WriteLine($"Grabbing changelog from {channel} channel...");

        try
        {
            using var client = new HttpClient();
            string rawResponse = await client.GetStringAsync(url);
            
            if (rawResponse.Length > 15000)
                rawResponse = rawResponse.Substring(0, 15000) + $"<br><br>... \n For more see {url}";
            
            return rawResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Unable to retrieve news..";
        }
    }

    /// <summary>Downloads the launcher ZIP for the current platform to a temp file. Returns the temp file path, or null on failure.</summary>
    public static async Task<string?> DownloadLauncherZip(DownloadProgress downloadProgress)
    {
        if (!HaveData(ReleaseChannel.LAUNCHER)) return null;

        GitHubReleaseData releaseData = ReleaseData[ReleaseChannel.LAUNCHER];

        if (releaseData == null || releaseData.assets == null) return null;

        // The launcher zip is published under a fixed name rather than with a
        // win-x64 style suffix, so match it by name first. The suffix rule is kept as
        // a fallback in case per-platform launcher builds are published later.
        GitHubReleaseData.Asset? selectedAsset = FindAssetByName(releaseData, CONSTANTS.LAUNCHER_ZIP_NAME);

        if (selectedAsset == null)
        {
            string platformZipName = PlatformHelper.GetPlatformZipName();

            foreach (GitHubReleaseData.Asset asset in releaseData.assets)
            {
                if (asset.name != null && asset.name.EndsWith(platformZipName) && asset.browser_download_url != null)
                {
                    selectedAsset = asset;
                    break;
                }
            }
        }

        if (selectedAsset == null)
        {
            Console.WriteLine("No launcher zip found on the release; cannot self update.");
            return null;
        }

        try
        {
            string tempFilePath = Path.GetTempFileName();
            using (var file = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                HttpClient httpClient = new HttpClient();
                await httpClient.DownloadAsync(selectedAsset.browser_download_url, file, downloadProgress);
            }
            return tempFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Finds a release asset by exact filename.
    /// </summary>
    private static GitHubReleaseData.Asset? FindAssetByName(GitHubReleaseData releaseData, string name)
    {
        if (releaseData.assets == null) return null;

        foreach (GitHubReleaseData.Asset asset in releaseData.assets)
        {
            if (string.Equals(asset.name, name, StringComparison.OrdinalIgnoreCase) && asset.browser_download_url != null)
                return asset;
        }

        return null;
    }

    /// <summary>
    /// Picks the zip to install for a channel. Channels that publish a single
    /// fixed-name asset are matched by that name and nothing else, so a differently
    /// named zip in the same release can never be installed by mistake. The other
    /// channels keep the platform-suffix rule with the ZIP_STARTS_WITH fallback.
    /// </summary>
    private static GitHubReleaseData.Asset? FindAssetForChannel(GitHubReleaseData releaseData, ReleaseChannel channel)
    {
        if (releaseData.assets == null) return null;

        string? exactName = channel switch
        {
            ReleaseChannel.HOLIDAY => CONSTANTS.HOLIDAY_ZIP_NAME,
            ReleaseChannel.HOLIDAY_DEV => CONSTANTS.HOLIDAY_DEV_ZIP_NAME,
            _ => null
        };

        if (exactName != null)
            return FindAssetByName(releaseData, exactName);

        string platformZipName = PlatformHelper.GetPlatformZipName();

        // First, try to find platform-specific zip
        foreach (GitHubReleaseData.Asset asset in releaseData.assets)
        {
            if (asset.name != null && asset.name.EndsWith(platformZipName) && asset.browser_download_url != null)
                return asset;
        }

        // Fallback to current method if platform-specific zip not found
        foreach (GitHubReleaseData.Asset asset in releaseData.assets)
        {
            if (asset.name != null && asset.name.EndsWith(".zip") && asset.name.StartsWith(CONSTANTS.ZIP_STARTS_WITH) && asset.browser_download_url != null)
                return asset;
        }

        return null;
    }

    /// <summary>
    /// Downloads and installs a client channel into the client folder. Not for the
    /// launcher channel; see DownloadLauncherZip for that.
    /// </summary>
    /// <param name="onCompleted">
    /// Always invoked, with true only if a zip was actually installed. It must run on
    /// every exit path: it is what re-enables the play button and hides the progress
    /// bar, so returning without it leaves the launcher stuck mid-download.
    /// </param>
    public static async void DownloadAndInstallZip(ReleaseChannel channel, DownloadProgress downloadProgress, Action<bool> onCompleted)
    {
        bool installed = false;

        try
        {
            // Only the selected channel and the launcher's own data are fetched up
            // front, so a one-shot install from the Tools menu normally has none yet.
            if (!HaveData(channel))
                await TryGetReleaseData(channel);

            GitHubReleaseData? releaseData = HaveData(channel) ? ReleaseData[channel] : null;

            if (releaseData?.assets == null)
            {
                Console.WriteLine($"No release data available for {channel}, nothing to install.");
                return;
            }

            string extractTo = PathHelper.ClientPath;

            installed = await Task.Run(() =>
            {
                GitHubReleaseData.Asset? selectedAsset = FindAssetForChannel(releaseData, channel);

                if (selectedAsset == null)
                {
                    Console.WriteLine($"No matching asset on the {channel} release.");
                    return false;
                }

                Console.WriteLine($"Picked for download: {selectedAsset.name} from {selectedAsset.browser_download_url}");

                try
                {
                    string tempFilePath = Path.GetTempFileName();
                    using (var file = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        HttpClient httpClient = new HttpClient();
                        httpClient.DownloadAsync(selectedAsset.browser_download_url, file, downloadProgress).Wait();
                    }

                    Directory.CreateDirectory(extractTo);
                    ZipFile.ExtractToDirectory(tempFilePath, extractTo, true);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            });
        }
        finally
        {
            onCompleted?.Invoke(installed);
        }
    }

    public static async Task<bool> ProcessRunningShouldWeProceed(Window parentWindow)
    {
        if (Process.GetProcessesByName(CONSTANTS.PROCESS_NAME).Length > 0)
        {
            return await Utility.ShowConfirmationDialog(
                parentWindow,
                $"{CONSTANTS.PROCESS_NAME} is running",
                $"{CONSTANTS.PROCESS_NAME} appears to be running. Updating while running may cause issues.\n\nDo you want to proceed with the update anyway?"
            );
        }
        
        return true;
    }
}