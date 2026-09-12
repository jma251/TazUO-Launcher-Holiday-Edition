namespace TazUOLauncher;

internal static class CONSTANTS {
    public const string WEBSITE_URL = "https://tazuo.org";
    public const string GITHUB_URL = "https://github.com/PlayTazUO/TazUO";
    public const string DEV_CHANNEL_RELEASE_URL = "https://api.github.com/repos/PlayTazUO/TazUO/releases/tags/TazUO-BleedingEdge";
    public const string MAIN_CHANNEL_RELEASE_URL = "https://api.github.com/repos/PlayTazUO/TazUO/releases/latest";
    // Self-update points at this fork, not upstream, so a stock upstream launcher
    // release can never overwrite the Holiday Edition changes below.
    //
    // Resolved through the "Latest" badge, never by tag name. This is the one URL
    // that must never break: if it 404s, every installed launcher loses its only
    // update path and can be recovered solely by downloading a zip by hand. A tag
    // name can be renamed, deleted or burned; the badge just follows whatever the
    // newest release is.
    public const string LAUNCHER_RELEASE_URL = "https://api.github.com/repos/jma251/TazUO-Launcher-Holiday-Edition/releases/latest";
    public const string LAUNCHER_LATEST_URL = "https://github.com/jma251/TazUO-Launcher-Holiday-Edition/releases/latest";
    public const string NET472_CHANNEL_RELEASE_URL = "https://api.github.com/repos/PlayTazUO/TazUO/releases/tags/TazUO-Legacy";
    public const string CHANGE_LOG_URL = "https://raw.githubusercontent.com/PlayTazUO/TazUO/refs/heads/{0}/CHANGELOG.md";

    // Holiday Edition channel. Resolved through the repository's "Latest" badge
    // rather than by tag name: the release tag has changed once already, and each
    // time it does a by-name lookup 404s and the launcher shows v0.0.0. /latest
    // names no tag, so it keeps working whatever the release is tagged.
    // The changelog needs its own URL because it lives in a different repo and
    // branch than the CHANGE_LOG_URL format above covers.
    public const string HOLIDAY_CHANNEL_RELEASE_URL = "https://api.github.com/repos/jma251/TazUO-Holiday-Edition/releases/latest";
    // Read from the release branch, which is where stable builds are cut from, so the
    // news panel matches what the stable channel actually installs.
    public const string HOLIDAY_CHANGE_LOG_URL = "https://raw.githubusercontent.com/jma251/TazUO-Holiday-Edition/refs/heads/release/CHANGELOG.md";

    // Holiday Edition dev builds. Replaced on every push to legacy-dev, and cut from
    // the current release plus untested commits - so a dev build reports the SAME
    // version as that release. It is therefore never polled or version-compared; it
    // is installed only when the player explicitly asks for it from the Tools menu.
    // Tagged "dev-build". This one has to be looked up by tag, because a prerelease
    // never carries the "Latest" badge that the stable channel above resolves through.
    public const string HOLIDAY_DEV_CHANNEL_RELEASE_URL = "https://api.github.com/repos/jma251/TazUO-Holiday-Edition/releases/tags/dev-build";

    // Holiday Edition publishes one fixed-name zip per channel, so its assets are
    // matched by exact name instead of by the ZIP_STARTS_WITH fallback below. The dev
    // zip is deliberately named so that fallback can never pick it up by accident.
    public const string HOLIDAY_ZIP_NAME = "TazUO-Holiday-Edition.zip";
    public const string HOLIDAY_DEV_ZIP_NAME = "HolidayEdition-Dev.zip";

    // Must match LAUNCHER_ZIP_NAME in .github/workflows/build-launcher.yml. This zip
    // carries no win-x64 style platform suffix, so the self update has to match it by
    // name - looking only for a platform suffix finds nothing and the update fails.
    public const string LAUNCHER_ZIP_NAME = "TazUOLauncher-HolidayEdition.zip";
    public const string REMOTE_VERSION_FORMAT = "Remote Version: {0}";
    public const string LOCAL_VERSION_FORMAT = "Local Version: {0}";
    public const string CLIENT_DIRECTORY_NAME = "TazUO";
    public const string CLIENT_UPDATE_AVAILABLE = "TazUO update available";
    public const string NO_CLIENT_AVAILABLE = "TazUO not installed";
    public const string EDIT_PROFILES = "[ Edit Profiles ]";
    public const string ZIP_STARTS_WITH = "TazUO";
    public const string CLASSIC_EXE_NAME = "ClassicUO";
    public const string NATIVE_EXECUTABLE_NAME = "TazUO";
    public const string PROCESS_NAME = "TazUO";

    // PR test builds: the TUO-PR-Build action publishes a GitHub release for a PR (named after the PR title)
    // with the same per-platform zips that normal releases provide, tagged "pr-<number>-test-build". These
    // let players test features in open PRs before they are merged. Release assets have public download URLs,
    // so no token is needed. {0} is the PR number.
    public const string PR_LIST_URL = "https://api.github.com/repos/PlayTazUO/TazUO/pulls?state=open&per_page=100";
    public const string RELEASES_URL = "https://api.github.com/repos/PlayTazUO/TazUO/releases?per_page=100";
    public const string PR_BUILD_TAG_FORMAT = "pr-{0}-test-build";
}