using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>LS_Pillar_*</c> from <c>p_lnspec.cpp</c>.</summary>
internal static class PillarLineSpecials
{
    internal static bool Execute(AuthoritySimulation sim, int special, int tag, int speed, int a2, int a3, int a4, LevelLine? line)
    {
        if (tag == 0 && line is null) return false;
        var platSpeed = speed / 8.0;
        if (platSpeed <= 0 && special != 30) return false;
        var started = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            var floor = sim.Floors[sector];
            var ceiling = sim.Ceilings[sector];
            switch (special)
            {
                case 29:
                case 94:
                    started |= LineSpecials.ExecuteFloorSpecial(sim, 23, tag, speed, a2, 0, special == 94 ? a3 : 0, line) == true;
                    break;
                case 30:
                    started |= LineSpecials.ExecuteFloorSpecial(sim, 20, tag, speed, a2, 0, 0, line) == true
                        && CeilingActions.Start(sim, line, tag, CeilingTarget.RaiseBy, platSpeed, amount: a3);
                    break;
            }
        }
        return started;
    }
}
