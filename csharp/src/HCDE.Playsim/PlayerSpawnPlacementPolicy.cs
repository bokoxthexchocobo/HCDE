namespace HCDE.Playsim;

/// <summary>Native <c>FLevelLocals::SpawnPlayer</c> spawn origin selection (start spot vs same-spot respawn).</summary>
public static class PlayerSpawnPlacementPolicy
{
    public sealed class PlayerSpawnContext
    {
        public bool Deathmatch { get; init; }
        public bool SameSpawnSpotDmFlag { get; init; }
        public bool SectorNoRespawn { get; init; }
        public bool WorldDone { get; init; }
        public bool LastDamageWasSuicide { get; init; }
        public bool LastSafePosValid { get; init; }
        public bool PlayerRebornState { get; init; }
        public bool SpawnOnCeiling { get; init; }
        public bool SpawnFloat { get; init; }
        public bool UsePlayerStartZ { get; init; }
        public double LastSafeX { get; init; }
        public double LastSafeY { get; init; }
        public double LastSafeZ { get; init; }
        public int LastSafeAngle { get; init; }
    }

    public sealed class PlayerSpawnOrigin
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double ZOffset { get; init; }
        public int AngleDegrees { get; init; }
        public MapThingActorSpawnComposer.SpawnHeightMode HeightMode { get; init; }
        public bool UsedLastSafePos { get; init; }
    }

    public static PlayerSpawnOrigin Resolve(
        PlayerSpawnContext context,
        double startX,
        double startY,
        double startZ,
        int startAngle,
        bool badAnglesCompat)
    {
        if (!context.Deathmatch
            && context.SameSpawnSpotDmFlag
            && context.PlayerRebornState
            && !context.WorldDone
            && !context.SectorNoRespawn
            && !context.LastDamageWasSuicide
            && context.LastSafePosValid)
        {
            return new PlayerSpawnOrigin
            {
                X = context.LastSafeX,
                Y = context.LastSafeY,
                ZOffset = context.LastSafeZ,
                AngleDegrees = context.LastSafeAngle,
                HeightMode = ResolveHeightMode(context),
                UsedLastSafePos = true,
            };
        }

        var angle = startAngle;
        if (badAnglesCompat)
            angle += 1;

        return new PlayerSpawnOrigin
        {
            X = startX,
            Y = startY,
            ZOffset = context.UsePlayerStartZ ? startZ : 0,
            AngleDegrees = angle,
            HeightMode = ResolveHeightMode(context),
        };
    }

    private static MapThingActorSpawnComposer.SpawnHeightMode ResolveHeightMode(PlayerSpawnContext context)
    {
        if (context.SpawnOnCeiling)
            return MapThingActorSpawnComposer.SpawnHeightMode.OnCeiling;
        if (context.SpawnFloat)
            return MapThingActorSpawnComposer.SpawnHeightMode.FloatRandom;
        return MapThingActorSpawnComposer.SpawnHeightMode.OnFloor;
    }
}
