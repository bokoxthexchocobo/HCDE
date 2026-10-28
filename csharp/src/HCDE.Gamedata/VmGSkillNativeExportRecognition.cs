namespace HCDE.Gamedata;

/// <summary>ZScript/DEFINE export names for <c>g_skill.cpp</c> (engine seam; behavior in conversion chunks).</summary>
public static class VmGSkillNativeExportRecognition
{
    public static IReadOnlyList<string> SkillZScriptExports =>
    [
        "G_SkillPropertyInt",
        "G_SkillPropertyFloat",
        "G_SkillName",
        "FSkillInfo.GetTextColor"
    ];

    public static IReadOnlyList<string> SkillVmFields =>
    [
        "SkillName", "AmmoFactor", "FastMonsters", "MenuName", "Replace", "Replaced"
    ];
}
