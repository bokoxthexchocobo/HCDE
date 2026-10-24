namespace HCDE.Client;

/// <summary><c>CheckDefaultSkill</c> and <c>NewFailure</c> from <c>d_main.cpp</c> (2893–2913).</summary>
public static class VmDMainCheckDefaultSkill
{
    public static bool ShouldApplyDefaultSkillCvar(int defaultSkill) => defaultSkill >= 0;

    public static int ResolveGameskill(int setskillFromCmdline, int currentGameskill) =>
        setskillFromCmdline >= 0 ? setskillFromCmdline : currentGameskill;

    public static bool ShouldOverrideWithCmdlineSkill(int setskill) => setskill >= 0;

    public static string HeapAllocationFailureMessage() =>
        "Failed to allocate memory from system heap";
}
