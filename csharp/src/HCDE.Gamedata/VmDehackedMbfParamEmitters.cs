namespace HCDE.Gamedata;

/// <summary>Additional MBF pointer helpers from <c>d_dehacked.cpp</c> (936–1085).</summary>
public static class VmDehackedMbfParamEmitters
{
    public static int JumpStateLabelOffset(int stateIndex) => stateIndex;

    public static bool ShouldReturnPointerForJump() => true;

    public static float TurnAngleDegrees(int misc1) => misc1;

    public static float FaceAngleDegrees(int misc1) => misc1;

    public static int ScratchDamage(int misc1) => misc1;

    public static int ScratchSoundIndex(int misc2) => VmDehackedMbfParamDefaults.DehSoundIndex(misc2, true);
}
