using HCDE.Gamedata;

namespace HCDE.Playsim;

internal static class DoomDecorationDefaults
{
    // doomitems.txt identities and doomdecorations.zs decoration defaults.
    internal static void Apply(Actor actor, int definitionType, DehackedActor? patch)
    {
        var height = definitionType switch
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
        if (height == 0) return;
        // Skip_Super resets defaults but retains inherited monster state labels.
        actor.HasOnlySpawnLabel = definitionType is not (18 or 19 or 20 or 21 or 22 or 23);
        var hanging = definitionType is >= 49 and <= 53 or >= 59 and <= 63 or >= 73 and <= 78;
        var floorDebris = definitionType is 79 or 80 or 81;
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
