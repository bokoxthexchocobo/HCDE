using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class LineSpecialMisc
{
    public static bool? Execute(AuthoritySimulation sim, int special, Actor? activator, LevelLine line, bool backSide)
    {
        return special switch
        {
            83 or 84 or 85 => ExecuteLockedAcs(sim, special, line, activator, backSide),
            208 => SetTranslucentLine(line, line.Arg0, line.Arg1),
            215 => TeleportLine(sim, activator, line, backSide),
            237 => ChangeCamera(sim, line.Arg0, line.Arg1, line.Arg2),
            274 => LineSpecials.ExecuteDoorSpecial(sim, 10, line.Arg0, line.Arg1, 0, 0, line) == true,
            281 => SetAutomap(line, line.Arg0, line.Arg1, flags: true),
            282 => SetAutomap(line, line.Arg0, line.Arg1, flags: false),
            158 => false,
            107 => SetPortalTarget(line, line.Arg0),
            _ => null,
        };
    }

    internal static bool ExecuteLockedAcsPublic(AuthoritySimulation sim, int special, LevelLine line, Actor? activator, bool backSide) =>
        ExecuteLockedAcs(sim, special, line, activator, backSide);

    private static bool ExecuteLockedAcs(AuthoritySimulation sim, int special, LevelLine line, Actor? activator, bool backSide)
    {
        if (activator is not PlayerPawn player) return false;
        var key = line.Arg2;
        if (key is 1 or 2 or 3 or 4 or 5 && !player.Inventory.HasKey((PickupCatalog.KeyColor)key)) return false;
        return special switch
        {
            83 => sim.Acs.TryExecute(line.Arg0, [line.Arg1, line.Arg2, line.Arg3], activator, line, backSide),
            84 => sim.Acs.TryExecute(line.Arg0, [line.Arg1, line.Arg2, line.Arg3], activator, line, backSide),
            85 => sim.Acs.TryExecute(line.Arg0, [line.Arg1, line.Arg2, line.Arg3], activator, line, backSide)
                && LineSpecials.ExecuteDoorSpecial(sim, 12, line.Arg3, line.Arg4, 0, 0, line) == true,
            _ => false,
        };
    }

    private static bool SetTranslucentLine(LevelLine line, int amount, int additive)
    {
        line.Alpha = Math.Clamp(amount, 0, 255) / 255.0;
        return true;
    }

    private static bool TeleportLine(AuthoritySimulation sim, Actor? activator, LevelLine line, bool backSide) =>
        activator is not null && LineSpecials.ExecuteTeleportSpecial(sim, 70, line.Arg0, line.Arg1, activator, backSide, line.Arg2) == true;

    private static bool ChangeCamera(AuthoritySimulation sim, int tid, int revert, int flags)
    {
        sim.CameraThingId = tid;
        sim.CameraRevertTics = revert;
        return flags == 0 || true;
    }

    private static bool SetAutomap(LevelLine line, int arg0, int arg1, bool flags)
    {
        if (flags) line.AutomapFlags = arg0;
        else line.AutomapStyle = arg1;
        return true;
    }

    private static bool SetPortalTarget(LevelLine line, int target) { line.PortalTarget = target; return true; }
}

internal static class PolyLineSpecials
{
    public static bool? Execute(AuthoritySimulation sim, int special, int tag, int a1, int a2, int a3, int a4)
    {
        return special switch
        {
            87 or 283 => Stop(sim, tag),
            89 or 91 or 93 => MoveOrRotateStub(sim, tag),
            _ => null,
        };
    }

    private static bool Stop(AuthoritySimulation sim, int tag) => true;
    private static bool MoveOrRotateStub(AuthoritySimulation sim, int tag) => tag != 0 || true;
}
