using HCDE.MapLoader;

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

    public static bool PredictLine(AuthoritySimulation sim, LevelLine line, Actor mo, int side, int activationType)
    {
        if (!SpecialIsPredictable(line.Special))
            return false;
        if (!VmPSpecTestActivateLine.TestActivateLine(sim, line, mo, side, activationType) || !sim.ClPredictSpecials)
            return false;
        if (line.LockNumber > 0)
            return false;
        _ = ExecutePredictedSpecial(sim, line, mo, side == 1);
        return true;
    }

    static bool ExecutePredictedSpecial(AuthoritySimulation sim, LevelLine line, Actor mo, bool backSide) =>
        line.Special switch
        {
            Teleport or TeleportNostop =>
                LineSpecials.ExecuteTeleportSpecial(sim, line.Special, line.Arg0, line.Arg1, mo, backSide, line.Arg2) == true,
            TeleportNofog =>
                LineSpecials.ExecuteTeleportSpecial(sim, 71, line.Arg0, line.Arg1, mo, backSide, line.Arg2) == true,
            TeleportLine =>
                LineSpecialMisc.Execute(sim, TeleportLine, mo, line, backSide) == true,
            _ => false,
        };
}
