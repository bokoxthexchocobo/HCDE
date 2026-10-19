namespace HCDE.Playsim;

/// <summary>Native <c>SpawnMapThing</c> actor placement after type resolution (Z height, TID, angle, args).</summary>
public static class MapThingActorSpawnComposer
{
    public enum SpawnHeightMode
    {
        OnFloor,
        OnCeiling,
        FloatRandom,
    }

    public sealed class MapThingSpawnPlacement
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double ZOffset { get; init; }
        public int AngleDegrees { get; init; }
        public uint ThingId { get; init; }
        public int Special { get; init; }
        public int[] Args { get; init; } = new int[5];
        public ushort SpawnFlags { get; init; }
        public bool SpawnCeiling { get; init; }
        public bool SpawnFloat { get; init; }
        public bool FloatBob { get; init; }
        public bool FloorClip { get; init; }
        public bool ArgsDefinedOnActor { get; init; }
        public int FloatBobPhase { get; init; } = -1;
        public double GravityScale { get; init; } = 1;
        public bool NoGravity { get; init; }
        public int FriendlySeeBlocks { get; init; }
    }

    public sealed class ComposedSpawn
    {
        public SpawnHeightMode HeightMode { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
        public double SpawnPointZ { get; init; }
        public int AngleDegrees { get; init; }
        public uint ThingId { get; init; }
        public int Special { get; set; }
        public int[] Args { get; } = new int[5];
        public ushort SpawnFlags { get; init; }
        public double FloatBobAnchorZ { get; init; }
        public bool ApplyFloorCeilingProbe { get; set; }
    }

    public static ComposedSpawn Compose(MapThingSpawnPlacement placement, bool hexenFloatBobCompat)
    {
        var heightMode = placement.SpawnCeiling
            ? SpawnHeightMode.OnCeiling
            : placement.SpawnFloat
                ? SpawnHeightMode.FloatRandom
                : SpawnHeightMode.OnFloor;

        var z = heightMode switch
        {
            SpawnHeightMode.OnCeiling => -placement.ZOffset,
            _ => placement.ZOffset,
        };

        var composed = new ComposedSpawn
        {
            HeightMode = heightMode,
            X = placement.X,
            Y = placement.Y,
            Z = z,
            SpawnPointZ = placement.ZOffset,
            AngleDegrees = placement.AngleDegrees,
            ThingId = placement.ThingId,
            SpawnFlags = placement.SpawnFlags,
            FloatBobAnchorZ = placement.FloatBob && hexenFloatBobCompat ? placement.ZOffset : 0,
        };

        composed.ApplyFloorCeilingProbe = composed.FloatBobAnchorZ == 0
            || !placement.FloatBob
            || !hexenFloatBobCompat;

        if (!placement.ArgsDefinedOnActor)
        {
            composed.Special = placement.Special;
            Array.Copy(placement.Args, composed.Args, 5);
        }

        if (placement.NoGravity)
            composed.ApplyFloorCeilingProbe = true;

        if (placement.FloatBobPhase >= 0 && placement.FloatBobPhase < 64)
            composed.ApplyFloorCeilingProbe = true;

        return composed;
    }
}
