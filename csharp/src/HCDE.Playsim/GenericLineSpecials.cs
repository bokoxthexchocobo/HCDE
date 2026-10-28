using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>LS_Generic_*</c> dispatch from <c>p_lnspec.cpp</c>.</summary>
internal static class GenericLineSpecials
{
    internal static bool Execute(AuthoritySimulation sim, int special, int tag, int speed, int a2, int a3, int a4, LevelLine? line)
    {
        return special switch
        {
            200 => ExecuteGenericFloor(sim, tag, speed, a2, a3, a4, line),
            201 => ExecuteGenericCeiling(sim, tag, speed, a2, a3, a4, line),
            202 => ExecuteGenericDoor(sim, tag, speed, a2, a3, a4, line),
            203 => PlatLineSpecials.Execute(sim, MapGenericLift(a3), tag, speed, a2, a3, a4, line),
            204 => LineSpecials.ExecuteStairSpecial(sim, 204, tag, speed, a2, a3, a4, line) == true,
            205 or 169 or 284 => ExecuteGenericCrusher(sim, tag, speed, a2, a3, a4, line, hexen: special != 205),
            _ => false,
        };
    }

    private static int MapGenericLift(int target) => target switch
    {
        1 => 62,
        2 => 172,
        3 => 64,
        4 => 60,
        _ => 65,
    };

    private static bool ExecuteGenericFloor(AuthoritySimulation sim, int tag, int speed, int height, int target, int flags, LevelLine? line)
    {
        var raise = (flags & 8) != 0;
        var crush = (flags & 16) != 0 ? 20 : 0;
        var mapped = raise ? target switch
        {
            1 => 24,
            2 => 257,
            3 => 25,
            4 => 238,
            5 => 259,
            6 => 240,
            _ => 23,
        } : target switch
        {
            1 => 242,
            2 => 21,
            3 => 22,
            4 => 258,
            5 => 260,
            6 => 261,
            _ => 20,
        };
        return LineSpecials.ExecuteFloorSpecial(sim, mapped, tag, speed, height, flags & 7, crush, line) == true;
    }

    private static bool ExecuteGenericCeiling(AuthoritySimulation sim, int tag, int speed, int height, int target, int flags, LevelLine? line)
    {
        var raise = (flags & 8) != 0;
        var crush = (flags & 16) != 0 ? 20 : 0;
        var mapped = raise ? target switch
        {
            1 => 262,
            2 => 265,
            3 => 252,
            4 => 266,
            5 => 254,
            6 => 268,
            _ => 41,
        } : target switch
        {
            1 => 192,
            2 => 253,
            3 => 264,
            4 => 192,
            5 => 254,
            6 => 269,
            _ => 40,
        };
        return LineSpecials.ExecuteCeilingSpecial(sim, mapped, tag, speed, height, flags & 7, crush, line) == true;
    }

    private static bool ExecuteGenericDoor(AuthoritySimulation sim, int tag, int speed, int kind, int delay, int lockId, LevelLine? line)
    {
        var mapped = (kind & 63) switch
        {
            0 => 12,
            1 => 11,
            2 => 249,
            3 => 10,
            4 => 105,
            5 => 106,
            _ => 12,
        };
        return LineSpecials.ExecuteDoorSpecial(sim, mapped, tag, speed, delay, lockId, line) == true;
    }

    private static bool ExecuteGenericCrusher(AuthoritySimulation sim, int tag, int speed, int down, int up, int silent, LevelLine? line, bool hexen)
    {
        var slow = down <= 24 && up <= 24 && !hexen;
        return CeilingActions.Start(sim, line, tag, CeilingTarget.Crush, speed: down / 8.0, upSpeed: up / 8.0,
            crush: silent, returnToStart: true, loop: true, slow: slow, stop: hexen);
    }
}
