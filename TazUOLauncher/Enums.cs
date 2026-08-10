namespace TazUOLauncher;

/// <summary>
/// Persisted to launcherdata.json by ordinal, not by name. New entries must be
/// APPENDED - inserting one in the middle silently repoints every saved channel
/// choice at the wrong channel.
/// </summary>
public enum ReleaseChannel
{
    INVALID,
    MAIN,
    DEV,
    LAUNCHER,
    NET472,
    HOLIDAY
}

public enum ClientStatus
{
    INITIALIZING,
    DOWNLOAD_IN_PROGRESS,
    NO_LOCAL_CLIENT,
    READY
}