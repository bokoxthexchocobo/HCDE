namespace HCDE.Playsim;

/// <summary><c>AActor::FallAndSink</c> and <c>P_NightmareRespawn</c> from <c>p_mobj.cpp</c> (3355–3551).</summary>
public static class VmPMobjFallNightmare
{
    public const int MfSpawnceiling = 1;
    public const int Mf2Spawnfloat = 1;
    public const int OnFloorZ = -1;
    public const int OnCeilingZ = -2;
    public const int FloatRandZ = -3;

    public sealed class FallActor
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int WaterLevel { get; set; }
        public double VelZ { get; set; }
        public double StartVelZ { get; set; }
        public bool HasPlayer { get; set; }
        public int Mass { get; set; } = 100;
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double OldFloorZ { get; set; }
    }

    public sealed class NightmareActor
    {
        public int SpawnHealth { get; set; } = 100;
        public int SkillRespawnCount { get; set; }
        public double SpawnX { get; set; }
        public double SpawnY { get; set; }
        public double SpawnZ { get; set; }
        public int SpawnAngle { get; set; }
        public int SpawnFlags { get; set; }
        public int Translation { get; set; }
        public object? DefaultInfo { get; set; }
        public int DefaultFlags { get; set; }
        public int DefaultFlags2 { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class NightmareContext
    {
        public Func<NightmareActor, object?>? StaticSpawn { get; init; }
        public Func<object?, bool>? CheckPosition { get; init; }
        public Action<object?, object?, bool, bool>? SpawnTeleportFog { get; init; }
        public Action<object?, NightmareActor, bool>? CopyFriendliness { get; init; }
        public Action<object?>? ClearCounters { get; init; }
        public Action<object?, object?>? FindFloorCeiling { get; init; }
    }

    public static void FallAndSink(FallActor actor, double grav, double oldfloorz)
    {
        actor.OldFloorZ = oldfloorz;
        if (actor.WaterLevel == 0)
        {
            actor.VelZ += grav;
            return;
        }

        var sinkspeed = -0.5;
        if (actor.WaterLevel > 1)
        {
            if (actor.VelZ < sinkspeed)
                actor.VelZ = Math.Max(actor.StartVelZ, sinkspeed);
            else
                actor.VelZ = actor.StartVelZ + (actor.VelZ - actor.StartVelZ) * 0.75;
        }
        else if (!actor.HasPlayer)
        {
            sinkspeed *= Math.Clamp(actor.Mass, 1, 4000) / 100.0;
            if (actor.VelZ < sinkspeed)
                actor.VelZ = Math.Max(sinkspeed * 2, actor.VelZ - 8);
            else if (actor.VelZ > sinkspeed)
                actor.VelZ = actor.StartVelZ + Math.Max(sinkspeed / 3, -8);
        }
    }

    public static void NightmareRespawn(NightmareContext ctx, NightmareActor mobj)
    {
        mobj.SkillRespawnCount++;
        var info = mobj.DefaultInfo;
        var zMode = (mobj.DefaultFlags & MfSpawnceiling) != 0 ? OnCeilingZ
            : (mobj.DefaultFlags2 & Mf2Spawnfloat) != 0 ? FloatRandZ : OnFloorZ;

        var mo = ctx.StaticSpawn?.Invoke(mobj);
        if (mo == null)
            return;

        _ = zMode;
        ctx.FindFloorCeiling?.Invoke(mo, mobj);

        if (!ctx.CheckPosition?.Invoke(mo) ?? false)
        {
            ctx.ClearCounters?.Invoke(mo);
            mobj.Destroyed = true;
            return;
        }

        ctx.CopyFriendliness?.Invoke(mo, mobj, false);
        ctx.SpawnTeleportFog?.Invoke(mobj, mo, true, true);
        ctx.SpawnTeleportFog?.Invoke(mobj, mo, false, true);
        mobj.Destroyed = true;
    }
}
