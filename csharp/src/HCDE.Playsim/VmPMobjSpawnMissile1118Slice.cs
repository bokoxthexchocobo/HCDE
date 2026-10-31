namespace HCDE.Playsim;

/// <summary>
/// <c>P_PlaySpawnSound</c>, <c>GetDefaultSpeed</c>, and <c>P_SpawnMissileXYZ</c> aim policy from
/// <c>p_mobj.cpp</c> (**7877–8026**). Chunk **1118**.
/// </summary>
public static class VmPMobjSpawnMissile1118Slice
{
    public const int NoSound = -1;
    public const int MfSpawnSoundSource = 1;
    public const int SecSilent = 2;
    public const int Mf3FloorHugger = 4;
    public const int Mf3CeilingHugger = 8;
    public const int Mf4Spectral = 16;

    public enum SpawnSoundSite
    {
        None,
        Missile,
        Spawner,
        SpawnPosition
    }

    public static SpawnSoundSite ResolveSpawnSound(int seeSound, int flags, bool hasSpawner, int sectorFlags)
    {
        if (seeSound == NoSound)
            return SpawnSoundSite.None;
        if ((flags & MfSpawnSoundSource) == 0)
            return SpawnSoundSite.Missile;
        if (hasSpawner)
            return SpawnSoundSite.Spawner;
        if ((sectorFlags & SecSilent) != 0)
            return SpawnSoundSite.None;
        return SpawnSoundSite.SpawnPosition;
    }

    public static double GetDefaultSpeed(bool hasType, bool fastMonsters, double fastSpeed, double speed)
    {
        if (!hasType)
            return 0;
        if (fastMonsters && fastSpeed >= 0)
            return fastSpeed;
        return speed;
    }

    public enum MissileAbort
    {
        None,
        MissingSourceOrType,
        MissingDestination,
        FailedSpawnCheck
    }

    public sealed class MissileAimContext
    {
        public bool HasSource { get; init; }
        public bool HasType { get; init; }
        public bool HasDestination { get; init; }
        public double PosZ { get; set; }
        public bool ZIsFloorSentinel { get; init; }
        public bool ZIsCeilingSentinel { get; init; }
        public double FloorClip { get; init; }
        public bool HasOwner { get; init; }
        public bool OwnerDefaultsToSource { get; set; }
        public int Flags3 { get; init; }
        public int Flags4 { get; init; }
        public double SourceZ { get; init; }
        public double DestHeight { get; init; }
        public double VelocityZ { get; set; }
        public bool ZeroVertical { get; set; }
        public bool RaisedOverHead { get; set; }
        public bool SpectralFriend { get; set; }
        public bool CheckSpawn { get; init; }
        public bool SpawnCheckPassed { get; init; } = true;
        public double LaunchZ { get; set; }
        public MissileAbort Abort { get; set; }
    }

    public static bool RunSpawnMissileXyz(MissileAimContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasSource || !ctx.HasType)
        {
            ctx.Abort = MissileAbort.MissingSourceOrType;
            return false;
        }

        if (!ctx.HasDestination)
        {
            ctx.Abort = MissileAbort.MissingDestination;
            return false;
        }

        if (!ctx.ZIsFloorSentinel && !ctx.ZIsCeilingSentinel)
            ctx.PosZ -= ctx.FloorClip;

        if (!ctx.HasOwner)
            ctx.OwnerDefaultsToSource = true;

        if ((ctx.Flags3 & (Mf3FloorHugger | Mf3CeilingHugger)) != 0)
        {
            ctx.VelocityZ = 0;
            ctx.ZeroVertical = true;
        }
        else if (ctx.PosZ - ctx.SourceZ >= ctx.DestHeight)
        {
            ctx.VelocityZ += ctx.DestHeight - ctx.PosZ + ctx.SourceZ;
            ctx.RaisedOverHead = true;
        }

        if ((ctx.Flags4 & Mf4Spectral) != 0)
            ctx.SpectralFriend = true;

        if (ctx.CheckSpawn && !ctx.SpawnCheckPassed)
        {
            ctx.Abort = MissileAbort.FailedSpawnCheck;
            return false;
        }

        return true;
    }

    public static double SpawnMissileLaunchZ(double sourceZ, double bobOffset) => sourceZ + 32 + bobOffset;

    public static bool TrySpawnMissileZ(bool hasSource, double z, out double launchZ)
    {
        launchZ = z;
        return hasSource;
    }

    public static bool OldSpawnMissileMissingArgs(bool hasSource, bool hasType) => !hasSource || !hasType;
}
