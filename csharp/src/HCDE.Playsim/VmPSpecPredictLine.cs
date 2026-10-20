namespace HCDE.Playsim;

/// <summary><c>P_PredictLine</c> from <c>p_spec.cpp</c> (364–399).</summary>
public static class VmPSpecPredictLine
{
    public const int TeleportLine = 215;
    public const int Teleport = 70;
    public const int TeleportNofog = 71;
    public const int TeleportNostop = 154;

    public static bool SpecialIsPredictable(int special) =>
        special is TeleportLine or Teleport or TeleportNofog or TeleportNostop;

    public static bool RequiresPredictCvar(bool clPredictSpecials) => clPredictSpecials;

    public static bool RejectWhenTestFails(bool testOk) => !testOk;

    public static bool RejectWhenLocked(int lockNumber) => lockNumber > 0;

    public static bool SideIsBack(int side) => side == 1;

    public static bool LogPredictSpam(bool developerSpam, bool success) => developerSpam && success;

    public static bool TracePredictDiag(bool success) => success;
}
