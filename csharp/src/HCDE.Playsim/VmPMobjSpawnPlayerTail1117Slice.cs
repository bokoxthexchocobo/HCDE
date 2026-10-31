namespace HCDE.Playsim;

/// <summary>
/// <c>P_SpawnPlayer</c> tail and <c>SpawnMapThing</c> editor-number reject from <c>p_mobj.cpp</c> (**6427–6576**).
/// Chunk **1117**.
/// </summary>
public static class VmPMobjSpawnPlayerTail1117Slice
{
    public const int PstLive = 0;
    public const int PstReborn = 1;
    public const int PstEnter = 2;
    public const int SpfTempPlayer = 1;
    public const int SpfWeaponFullyUp = 2;
    public const int CfChaseCam = 4;
    public const int Level2AllowRespawn = 8;
    public const int OfEuthanizeMe = 16;

    public sealed class SpawnPlayerTailContext
    {
        public int Refire { get; set; } = 1;
        public int DamageCount { get; set; } = 1;
        public int BonusCount { get; set; } = 1;
        public int MorphTics { get; set; } = 1;
        public int ExtraLight { get; set; } = 1;
        public int FixedLightLevel { get; set; }
        public float BlendA { get; set; } = 1;
        public int MusInfoTics { get; set; }
        public bool Uncrouch { get; set; }
        public bool ZeroVelocity { get; set; }
        public bool ResetAirSupply { get; set; }
        public int DemoCheats { get; set; }
        public bool SetupPsprites { get; set; }
        public bool WeaponFullyUp { get; set; }
        public bool GiveDeathmatchInventory { get; set; }
        public bool FilterCoopRespawn { get; set; }
        public bool DestroyOldInventory { get; set; }
        public bool OnRespawn { get; set; }
        public bool AttachStatusBar { get; set; }
        public bool SpawnTeleportFog { get; set; }
        public bool DropBelowCeiling { get; set; }
        public bool StartEnterScripts { get; set; }
        public bool ClearSoundTargets { get; set; }
        public bool SubstitutePlayerPointers { get; set; }
        public bool StartRespawnScripts { get; set; }
        public double SpawnZ { get; set; }
    }

    public static void ResetLiveCounters(SpawnPlayerTailContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.Refire = 0;
        ctx.DamageCount = 0;
        ctx.BonusCount = 0;
        ctx.MorphTics = 0;
        ctx.ExtraLight = 0;
        ctx.FixedLightLevel = -1;
        ctx.BlendA = 0;
        ctx.MusInfoTics = -1;
        ctx.Uncrouch = true;
        ctx.ZeroVelocity = true;
        ctx.ResetAirSupply = true;
    }

    public static int RetargetCamera(int watchingPlayer, object? watched, object? oldActor, object? spawned)
    {
        if (watched != null && ReferenceEquals(watched, oldActor))
            return watchingPlayer;
        return -1;
    }

    public static void ApplyDemoChaseCam(SpawnPlayerTailContext ctx, bool demoPlayback, bool demoNew, bool chaseDemo)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((demoPlayback || demoNew) && chaseDemo)
            ctx.DemoCheats |= CfChaseCam;
    }

    public static void RunSpawnPlayerTail(SpawnPlayerTailContext ctx, SpawnPlayerInputs input)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(input);
        ResetLiveCounters(ctx);
        ApplyDemoChaseCam(ctx, input.DemoPlayback, input.DemoNew, input.ChaseDemo);

        var tempWithOld = (input.Flags & SpfTempPlayer) != 0 && input.HasOldActor;
        if (!tempWithOld)
        {
            ctx.SetupPsprites = true;
            ctx.WeaponFullyUp = (input.Flags & SpfWeaponFullyUp) != 0;
        }

        if (input.Deathmatch)
        {
            ctx.GiveDeathmatchInventory = true;
        }
        else if (input.State == PstReborn && input.HasOldActor
                 && (input.Multiplayer || input.AllowRespawn || input.SinglePlayerRespawn || input.SkillPlayerRespawn))
        {
            ctx.FilterCoopRespawn = true;
        }

        if (input.HasOldActor)
            ctx.DestroyOldInventory = true;

        if (input.State is PstReborn or PstEnter)
            ctx.OnRespawn = true;

        if (input.HasStatusBar && (input.PlayerNum == input.ConsolePlayer || input.StatusBarPlayer == input.PlayerNum))
            ctx.AttachStatusBar = true;

        if (input.Multiplayer)
            ctx.SpawnTeleportFog = true;

        if (input.Top > input.CeilingZ)
        {
            ctx.DropBelowCeiling = true;
            ctx.SpawnZ = input.CeilingZ - input.Height;
        }

        if (!tempWithOld)
        {
            if (input.State == PstEnter || (input.State == PstLive && !input.SaveGameRestore))
            {
                ctx.StartEnterScripts = true;
            }
            else if (input.State == PstReborn)
            {
                ctx.ClearSoundTargets = true;
                ctx.SubstitutePlayerPointers = true;
                ctx.StartRespawnScripts = true;
            }
        }
    }

    public static bool RejectMapThingEditorNumber(int edNum) => edNum is 0 or -1;

    public static bool SpawnMultiEnabled(bool skillSpawnMulti, bool alwaysSpawnMulti) =>
        skillSpawnMulti || alwaysSpawnMulti;

    public sealed class SpawnPlayerInputs
    {
        public int Flags { get; init; }
        public bool HasOldActor { get; init; }
        public bool DemoPlayback { get; init; }
        public bool DemoNew { get; init; }
        public bool ChaseDemo { get; init; }
        public bool Deathmatch { get; init; }
        public bool Multiplayer { get; init; }
        public bool AllowRespawn { get; init; }
        public bool SinglePlayerRespawn { get; init; }
        public bool SkillPlayerRespawn { get; init; }
        public int State { get; init; }
        public bool HasStatusBar { get; init; }
        public int PlayerNum { get; init; }
        public int ConsolePlayer { get; init; }
        public int StatusBarPlayer { get; init; } = -1;
        public double Top { get; init; }
        public double CeilingZ { get; init; }
        public double Height { get; init; }
        public bool SaveGameRestore { get; init; }
    }
}
