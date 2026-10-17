using HCDE.Gamedata;

namespace HCDE.Playsim;

internal static class DoomDecorationDefaults
{
    private static readonly Dictionary<int, string> ClassNames = new()
    {
        [PatrolMarkers.PointType] = "PatrolPoint",
        [PatrolMarkers.SpecialType] = "PatrolSpecial",
        [10] = "GibbedMarine",
        [12] = "GibbedMarineExtra",
        [15] = "DeadMarine",
        [18] = "DeadZombieMan",
        [19] = "DeadShotgunGuy",
        [20] = "DeadDoomImp",
        [21] = "DeadDemon",
        [22] = "DeadCacodemon",
        [23] = "DeadLostSoul",
        [24] = "Gibs",
        [25] = "DeadStick",
        [26] = "LiveStick",
        [27] = "HeadOnAStick",
        [28] = "HeadsOnAStick",
        [29] = "HeadCandles",
        [30] = "TallGreenColumn",
        [31] = "ShortGreenColumn",
        [32] = "TallRedColumn",
        [33] = "ShortRedColumn",
        [34] = "Candlestick",
        [35] = "Candelabra",
        [36] = "HeartColumn",
        [37] = "SkullColumn",
        [41] = "EvilEye",
        [42] = "FloatingSkull",
        [43] = "TorchTree",
        [44] = "BlueTorch",
        [45] = "GreenTorch",
        [46] = "RedTorch",
        [47] = "Stalagtite",
        [48] = "TechPillar",
        [49] = "BloodyTwitch",
        [50] = "Meat2",
        [51] = "Meat3",
        [52] = "Meat4",
        [53] = "Meat5",
        [54] = "BigTree",
        [55] = "ShortBlueTorch",
        [56] = "ShortGreenTorch",
        [57] = "ShortRedTorch",
        [59] = "NonsolidMeat2",
        [60] = "NonsolidMeat4",
        [61] = "NonsolidMeat3",
        [62] = "NonsolidMeat5",
        [63] = "NonsolidTwitch",
        [70] = "BurningBarrel",
        [73] = "HangNoGuts",
        [74] = "HangBNoBrain",
        [75] = "HangTLookingDown",
        [76] = "HangTSkull",
        [77] = "HangTLookingUp",
        [78] = "HangTNoBrain",
        [79] = "ColonGibs",
        [80] = "SmallBloodPool",
        [81] = "BrainStem",
        [85] = "TechLamp",
        [86] = "TechLamp2",
        [2028] = "Column",
        [5050] = "Stalagmite",
    };
    private static readonly HashSet<string> KnownClassNames = ClassNames.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);
    internal static bool TryClassName(int type, out string name) => ClassNames.TryGetValue(type, out name!);
    internal static bool IsKnownClassName(string name) => KnownClassNames.Contains(name);
    internal static bool IsKindOfClassName(int type, string name)
    {
        if (TryClassName(type, out var concrete) && concrete.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        var parent = type switch
        {
            12 => 10, 18 => 3004, 19 => 9, 20 => 3001, 21 => 3002, 22 => 3005, 23 => 3006,
            59 => 50, 60 => 52, 61 => 51, 62 => 53, 63 => 49, _ => 0,
        };
        return parent != 0 && (TryClassName(parent, out var decoration) && decoration.Equals(name, StringComparison.OrdinalIgnoreCase)
            || DoomActorCatalog.IsKindOfClassName(parent, name));
    }

    internal static bool IsDecoration(int type) => HeightOf(type) != 0 || PatrolMarkers.IsMarker(type);
    internal static bool MovesWithSector(int type) => type is 79 or 80 or 81;

    private static int HeightOf(int definitionType) => definitionType switch
    {
        85 => 80,
        86 => 60,
        2028 => 48,
        5050 => 48,
        10 or 12 or 15 or 18 or 19 or 20 or 21 or 22 or 23 or 24 => 16,
        25 or 26 or 28 => 64,
        27 => 56,
        29 => 42,
        70 => 32,
        30 or 32 => 52,
        31 or 33 or 36 or 37 => 40,
        34 => 14,
        35 => 60,
        41 => 54,
        42 => 26,
        43 => 56,
        44 or 45 or 46 => 68,
        47 => 40,
        48 => 128,
        54 => 108,
        55 or 56 or 57 => 37,
        49 or 52 or 60 or 63 => 68,
        50 or 51 or 59 or 61 => 84,
        53 or 62 => 52,
        73 or 74 => 88,
        75 or 76 or 77 or 78 => 64,
        79 or 81 => 4,
        80 => 1,
        _ => 0,
    };

    // doomitems.txt identities and doomdecorations.zs decoration defaults.
    internal static void Apply(Actor actor, int definitionType, DehackedActor? patch)
    {
        var height = HeightOf(definitionType);
        if (height == 0) return;
        // Skip_Super resets defaults but retains inherited monster state labels.
        actor.HasOnlySpawnLabel = definitionType is not (18 or 19 or 20 or 21 or 22 or 23);
        var hanging = definitionType is >= 49 and <= 53 or >= 59 and <= 63 or >= 73 and <= 78;
        var floorDebris = MovesWithSector(definitionType);
        if (floorDebris) actor.MoveWithSector = true;
        var corpseDecoration = definitionType is 10 or 12 or 15 or 18 or 19 or 20 or 21 or 22 or 23 or 24;
        if (patch is not { WidthPatched: true }) actor.Radius = Fixed.FromInt(definitionType switch
        {
            10 or 12 or 15 or 18 or 19 or 20 or 21 or 22 or 23 or 24 or 34 or 59 or 60 or 61 or 62 or 63 or 79 or 80 or 81 => 20,
            54 => 32,
            _ => 16,
        });
        if (patch is not { HeightPatched: true }) actor.Height = Fixed.FromInt(height);
        actor.ProjectilePassHeight = hanging || floorDebris || corpseDecoration || definitionType == 5050
            ? default : Fixed.FromInt(-16);
        if (patch is not { BitsPatched: true })
        {
            actor.Solid = definitionType is not (10 or 12 or 15 or 18 or 19 or 20 or 21 or 22 or 23 or 24 or 34 or 59 or 60 or 61 or 62 or 63 or 79 or 80 or 81);
            if (floorDebris) actor.NoBlockmap = true;
            if (hanging) actor.NoGravity = actor.SpawnCeiling = true;
            actor.Shootable = false;
        }
        if (definitionType == 23)
        {
            actor.GenericCrushState = 1;
            actor.NullState = 2;
            actor.States.ConfigureSpawn(actor, [new ActorFrame(6, -1, FullBright: false),
                new ActorFrame(-1, 1, FullBright: false), new ActorFrame(1, -1, FullBright: false)], ActorStateMachine.Spawn);
        }
        else
            ConfigureTiming(actor, definitionType);
    }

    private static void ConfigureTiming(Actor actor, int definitionType)
    {
        int[] durations = definitionType switch
        {
            85 or 86 or 44 or 45 or 46 or 55 or 56 or 57 => [4, 4, 4, 4],
            36 => [14, 14],
            41 => [6, 6, 6, 6],
            42 => [6, 6, 6],
            29 => [6, 6],
            26 => [6, 8],
            49 or 63 => [10, 15, 8, 6],
            70 => [4, 4, 4],
            _ => [-1],
        };
        actor.GenericCrushState = durations.Length;
        actor.NullState = durations.Length + 1;
        var fullBright = definitionType is 85 or 86 or 2028 or 41 or 42
            or 44 or 45 or 46 or 55 or 56 or 57 or 34 or 35 or 29 or 70;
        var frames = durations.Select((tics, index) =>
            new ActorFrame(tics, (index + 1) % durations.Length, FullBright: fullBright))
            .Append(new ActorFrame(-1, actor.GenericCrushState, FullBright: false))
            .Append(new ActorFrame(1, -1, FullBright: false));
        // Native map Gibs aliases Spawn to the inherited GenericCrush frame.
        actor.SpawnState = definitionType == 24 ? actor.GenericCrushState : ActorStateMachine.Spawn;
        actor.States.ConfigureSpawn(actor, frames, actor.SpawnState);
    }
}
