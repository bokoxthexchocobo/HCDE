namespace HCDE.Playsim;

/// <summary>Player think cluster from <c>p_user.cpp</c> (1682–1912).</summary>
public static class VmPUserPlayerThink
{
    public const int CfPredicting = 1;
    public const int CfInterpview = 1 << 1;
    public const int PstLive = 0;
    public const int Pspb2D = 0;
    public const int Pspb3D = 1;
    public const int MfNogravity = 1;

    public static bool ApplyAngleOffset(double degrees) => Math.Abs(degrees) >= 1e-6;

    public static bool SubtitleTick(int counter) => counter > 0;

    public static int DecrementSubtitle(int counter) => counter - 1;

    public static bool UpdateSafePosWhenLive(int playerstate, int pstLive) => playerstate == pstLive;

    public static bool SkipBobTimerWhenPredicting(int cheats, int cfPredicting) =>
        (cheats & cfPredicting) == 0;

    public static bool BotFrozenSkip(bool frozen, bool hasBot) => frozen && hasBot;

    public static bool DebugLogTick(bool debugfile, bool predicting) => debugfile && !predicting;

    public static bool ClearInterpCheats(int cheats) => cheats != 0;

    public static bool BobWeapon2D(int bobType, int pspb2D) => bobType == pspb2D;

    public static bool BobWeapon3D(int bobType, int pspb3D) => bobType == pspb3D;

    public static bool RecordVelocityWhenNotPredicting(bool predicting, bool hasMo) =>
        !predicting && hasMo;

    public static bool VelocityUsesFull3D(int flags, int mfNogravity) =>
        (flags & mfNogravity) != 0;

    public readonly record struct AngleOffsetApply(double Degrees, double Current);

    public static double ApplyAngleOffsetValue(AngleOffsetApply ctx) =>
        ApplyAngleOffset(ctx.Degrees) ? ctx.Current + ctx.Degrees : ctx.Current;

    public static bool MusicCdTrackChange(int current, int next) => current != next;

    public static bool UseHoldButton(int buttons, int useMask) => (buttons & useMask) != 0;

    public static bool SafePosTracksLive(bool live, bool force) => live || force;

    public static int ClearInterpFlag(int cheats, int flag) => cheats & ~flag;

    public readonly record struct VelocityRecordGate(bool Predicting, bool HasMo, int Flags, int MfNogravity);

    public static bool ShouldRecordVelocity(VelocityRecordGate g) =>
        RecordVelocityWhenNotPredicting(g.Predicting, g.HasMo);

    public static bool UseXYVelocity(VelocityRecordGate g) =>
        !VelocityUsesFull3D(g.Flags, g.MfNogravity);

    public readonly record struct MusicChangeContext(bool Ready, int Track, int NextTrack);

    public static bool ShouldSwapMusic(MusicChangeContext ctx) =>
        ctx.Ready && MusicCdTrackChange(ctx.Track, ctx.NextTrack);

    public readonly record struct EnvironmentContext(bool Frozen, bool HasBot, bool Underwater, bool Secret);

    public static bool ShouldSkipThink(EnvironmentContext ctx) =>
        BotFrozenSkip(ctx.Frozen, ctx.HasBot);

    public static bool EnvironmentTriggersFx(EnvironmentContext ctx) =>
        ctx.Underwater || ctx.Secret;

    public readonly record struct PlayerThinkLoop(int Index, double OffsetDegrees);

    public static bool ProcessAngleSlot(PlayerThinkLoop slot) =>
        ApplyAngleOffset(slot.OffsetDegrees);

    public static bool WeaponBobBranch(int bobType) => bobType == Pspb2D || bobType == Pspb3D;

    public static bool LevelVelocitySlotValid(int playerIndex, int maxPlayers) =>
        playerIndex >= 0 && playerIndex < maxPlayers;

    public readonly record struct ThinkCheatsGate(int Cheats, int Predicting, int InterpView);

    public static int StripPresentationCheats(ThinkCheatsGate g) =>
        ClearInterpFlag(ClearInterpFlag(ClearInterpFlag(g.Cheats, CfInterpview), g.InterpView), g.InterpView);

    public const int CfPredictingUse = CfPredicting;
    public const int BtUse = 1;

    public static bool CheckUseSkipsWhenPredicting(int cheats) => (cheats & CfPredictingUse) != 0;

    public static bool CheckUseEdge(int buttons, int btUse, bool usedown) =>
        UseHoldButton(buttons, btUse) && !usedown;

    public static bool CheckUseRelease(int buttons, int btUse) => (buttons & btUse) == 0;

    public readonly record struct MusicInfoContext(int Tics, bool HasActor, bool IsConsolePlayer, int Arg0);

    public static bool MusicInfoTickActive(MusicInfoContext ctx) => ctx.Tics >= 0 && ctx.HasActor;

    public static bool MusicInfoFired(MusicInfoContext ctx) => ctx.Tics > 0 && ctx.Tics - 1 < 0;

    public static bool MusicInfoUsesLookup(MusicInfoContext ctx) => ctx.IsConsolePlayer && ctx.Arg0 != 0;

    public readonly record struct EnvironmentSoundContext(double VelZ, double MinSpeed, double MaxSpeed, bool Underwater, bool HasAlternative);

    public static bool EnvironmentFallingScream(EnvironmentSoundContext ctx) =>
        ctx.VelZ <= -ctx.MinSpeed && ctx.VelZ >= -ctx.MaxSpeed && !ctx.HasAlternative && !ctx.Underwater;

    public readonly record struct SafePosSample(object? Sector, double Z, double DropoffZ, double MaxStep, double Height, bool Valid);

    public static bool SafePosLiveUpdate(int playerstate, int pstLive) => playerstate == pstLive;

    public static bool SafePosValidOnFloor(SafePosSample s) =>
        s.Valid && s.Z <= s.Z && s.Z - s.DropoffZ <= s.MaxStep;

    public static bool SafePosForceCopy(bool force, bool live) => force || live;

    public static bool SafePosHeightClear(double ceiling, double floor, double height) => ceiling - floor >= height;

    public static bool PlayerThinkNullMoGuard(object? mo) => mo == null;

    public static bool PlayerThinkAngleLoop(int count) => count < 3;

    public static bool PlayerThinkVelocitySlot(bool predicting, bool hasMo) =>
        RecordVelocityWhenNotPredicting(predicting, hasMo);
}

