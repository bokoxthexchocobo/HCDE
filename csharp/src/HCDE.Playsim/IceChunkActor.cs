namespace HCDE.Playsim;

/// <summary>Native <c>IceChunk</c> debris from <c>A_FreezeDeathChunks</c>. Ice chunk heads are absent.</summary>
public sealed class IceChunkActor : Actor
{
    public const int DoomEdNumMarker = 65540;

    public int RemainingTics => Destroyed ? 0 : States.RemainingTics;

    internal IceChunkActor(int remainingTics)
    {
        if (remainingTics <= 0) throw new ArgumentOutOfRangeException(nameof(remainingTics));
        DoomEdNum = DoomEdNumMarker;
        ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "misc/ripslop" };
        SpawnActorSounds = new Dictionary<ActorSoundType, string>(ActorSounds);
        HasOnlySpawnLabel = true;
        PainState = DeathState = -1;
        Health = ResurrectionHealth = 1000;
        GibHealth = DefaultGibHealth = int.MinValue;
        MovementSpeed = default;
        SpawnTuning = new(MovementSpeed.Raw, FloatSpeed, PainThreshold);
        ReactionTime = 8;
        SpawnReactionTime = ReactionTime;
        PainChance = 0;
        SpawnPainChance = PainChance;
        WoundHealth = 6;
        Solid = false;
        Shootable = false;
        AllowDropOff = true;
        CannotPush = true;
        NoTeleport = true;
        NoBlockmap = true;
        MoveWithSector = true;
        FloorClipEnabled = DefaultFloorClip = true;
        Radius = Fixed.FromInt(3);
        Height = Fixed.FromInt(4);
        Mass = 5;
        SpawnMass = Mass;
        ResurrectionRadius = Radius;
        ResurrectionHeight = Height;
        ResurrectionCollisionFlags = ActorSpawner.CollisionFlagsOf(this);
        ResurrectionMovementFlags = ActorSpawner.MovementFlagsOf(this);
        DefaultPushFlags = 2;
        Gravity = Fixed.FromDouble(0.125);
        NullState = 4;
        GenericCrushState = 5;
        DieFromSpawnState = 6;
        GenericFreezeDeath = 8;
        States.ConfigureSpawn(this, [new ActorFrame(remainingTics, 1, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, 2, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, 3, _ => SetFrameDuration()),
            new ActorFrame(remainingTics, -1, _ => SetFrameDuration()),
            new ActorFrame(1, -1, FullBright: false),
            new ActorFrame(-1, -1, FullBright: false),
            new ActorFrame(1, 7, FullBright: false),
            new ActorFrame(1, -1, actor => actor.DieFromSpawn(), FullBright: false),
            new ActorFrame(5, 9, ActorFreezeActions.FreezeDeath, FullBright: false),
            new ActorFrame(1, 9, actor => (actor.Simulation
                ?? throw new InvalidOperationException("FreezeDeathChunks requires an attached simulation."))
                .SpawnIceChunks(actor), FullBright: false)], 0);
    }

    private void SetFrameDuration()
    {
        if (Simulation is { } sim)
        {
            var tics = sim.NextIceTics();
            var sector = ActorPhysics.SectorAt(sim.Level, X.ToDouble(), Y.ToDouble());
            if ((uint)sector < (uint)sim.Level.Sectors.Count)
            {
                var type = sim.Level.FloorTerrainDamageType(sector);
                if (string.Equals(type, "Fire", StringComparison.OrdinalIgnoreCase)) tics >>= 2;
                else if (string.Equals(type, "Ice", StringComparison.OrdinalIgnoreCase)) tics <<= 1;
            }
            States.ForceRemainingTics(tics);
        }
    }
}
