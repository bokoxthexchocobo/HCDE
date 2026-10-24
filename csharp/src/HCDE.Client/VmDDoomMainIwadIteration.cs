namespace HCDE.Client;

/// <summary>One <c>D_DoomMain_Internal</c> restart-loop IWAD/mod pass from <c>d_main.cpp</c> (4666–4761).</summary>
public static class VmDDoomMainIwadIteration
{
    public static bool RecreateIwadManagerWhenNull(bool isNull) => isNull;

    public static bool UseLastIwadWhenEmpty(string? iwad, string? lastIwad) =>
        string.IsNullOrEmpty(iwad) && !string.IsNullOrEmpty(lastIwad);

    public static bool CancelWhenNoIwadSelected(bool iwadInfoNull) => iwadInfoNull;

    public static int ExitOkWhenUserCancelledIwad() => 0;

    public static bool SharewareBlocksPwads(bool sharewareFlag, int pwadCount) =>
        sharewareFlag && pwadCount > 0;

    public static bool SteamAppIdDigit(char c) => c >= '0' && c <= '9';

    public static bool SteamAppIdInvalidChar(char c) => !SteamAppIdDigit(c);

    public static bool ClearSteamAppIdWhenInvalid(bool failedcheck) => failedcheck;

    public static bool ShouldCallInitGame(bool hasIwadInfo) => hasIwadInfo;

    public static bool AbortLoopWhenInitGameFailed(int ret) => ret != 0;

    public static int ReturnInitGameStatus(int ret) => ret;

    public static bool ClearPwadsAfterInit() => true;

    public static bool DeleteIwadManagerAfterInit() => true;

    public static bool ResetSpriteRenameFlag() => false;

    public static bool RestartReinitsConsole(bool restart) => restart;

    public static bool FirstGetCmdLineFilesForZdl(bool launcherPass) => launcherPass;

    public static bool SecondGetCmdLineFilesForLauncherExtras(bool updatePass) => updatePass;

    public static bool ThirdGetCmdLineFilesForNetVerify(bool verifyPass) => verifyPass;

    public static bool AppendModCompatFiles(bool compatEnabled) => compatEnabled;

    public static bool StoreLastIwadAfterSelection(string? iwad) => !string.IsNullOrEmpty(iwad);

    public static bool FatalSharewareMods(bool blocked) => blocked;

    public static bool ValidateSteamAppIdString(string? id, out bool failed)
    {
        failed = false;
        if (string.IsNullOrEmpty(id)) return true;
        foreach (var c in id)
        {
            if (SteamAppIdInvalidChar(c))
            {
                failed = true;
                return false;
            }
        }
        return true;
    }

    public static bool StartupStatusBeforeInitGame() => true;

    public static bool LogInitGameResult(int ret) => true;

    public static bool LauncherPassAddsZdlFiles(bool launcher) => launcher;

    public static bool UpdatePassAddsLauncherExtras(bool update) => update;

    public static bool NetVerifyPassAddsFiles(bool verify) => verify;

    public static bool CompatModAppendEnabled(bool compat) => compat;

    public static bool IwadManagerRecreateWhenMissing(bool missing) => missing;

    public static bool UseStoredLastIwadWhenUnset(string? iwad, string? last) =>
        string.IsNullOrWhiteSpace(iwad) && !string.IsNullOrWhiteSpace(last);

    public static bool FatalWhenSharewareWithPwads(bool shareware, int pwads) =>
        shareware && pwads > 0;

    public static bool SteamAppIdAllDigits(string? id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        foreach (var c in id)
        {
            if (c < '0' || c > '9') return false;
        }
        return true;
    }

    public static bool ClearSteamAppIdOnBadDigits(bool bad) => bad;

    public static bool InitGameWhenIwadResolved(bool hasIwad) => hasIwad;

    public static bool BreakRestartLoopOnInitFailure(int status) => status != 0;

    public static bool ClearPwadListAfterSuccessfulInit() => true;

    public static bool DeleteIwadManagerAfterPass() => true;

    public static bool StatusMessageBeforeInit() => true;

    public static bool StatusMessageAfterInit(int code) => code == 0 || code != 0;
}
