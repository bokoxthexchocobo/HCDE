namespace HCDE.Gamedata;

/// <summary>Global skill table declarations from <c>g_skill.cpp</c>.</summary>
public static class VmGSkillGlobals
{
    public const int DefaultSkillUnset = -1;
    public const int TeamMaximumPlaceholder = 16;

    public static List<VmSkillInfo> CreateEmptySkillList() => [];
}
