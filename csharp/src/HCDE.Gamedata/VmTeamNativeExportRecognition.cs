namespace HCDE.Gamedata;

/// <summary>ZScript/DEFINE export names for <c>teaminfo.cpp</c> (engine seam).</summary>
public static class VmTeamNativeExportRecognition
{
    public static IReadOnlyList<string> TeamZScriptExports =>
    [
        "FTeam.IsValid",
        "FTeam.ChangeTeam",
        "FTeam.GetPlayerColor",
        "FTeam.GetTextColor",
        "FTeam.GetLogo",
        "FTeam.GetLogoName",
        "FTeam.AllowsCustomPlayerColor"
    ];
}
