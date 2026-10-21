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

    public static bool PlayerThinkRequiresMo(bool moNull) => moNull;

    public static bool ApplyAngleOffset(double degrees) => Math.Abs(degrees) >= 1e-6;

    public static bool SubtitleTick(int counter) => counter > 0;

    public static int DecrementSubtitle(int counter) => counter - 1;

    public static bool UpdateSafePosWhenLive(int playerstate, int pstLive) => playerstate == pstLive;

    public static bool SkipBobTimerWhenPredicting(int cheats, int cfPredicting) =>
        (cheats & cfPredicting) == 0;

    public static bool BotFrozenSkip(bool frozen, bool hasBot) => frozen && hasBot;

    public static bool DebugLogTick(bool debugfile, bool predicting) => debugfile && !predicting;

    public static bool ClearInterpCheats(int cheats) => cheats != 0;

    public static bool CallVirtualPlayerThink(bool hasVirtual) => hasVirtual;

    public static bool BobWeapon2D(int bobType, int pspb2D) => bobType == pspb2D;

    public static bool BobWeapon3D(int bobType, int pspb3D) => bobType == pspb3D;

    public static bool RecordVelocityWhenNotPredicting(bool predicting, bool hasMo) =>
        !predicting && hasMo;

    public static bool VelocityUsesFull3D(int flags, int mfNogravity) =>
        (flags & mfNogravity) != 0;

    public static bool MusicChangeReady(bool ready) => ready;

    public static bool EnvironmentHazard(bool hazard) => hazard;

    public static bool UseLineCross(bool cross) => cross;

    public static bool SafePositionForced(bool force) => force;

    public readonly record struct AngleOffsetApply(double Degrees, double Current);

    public static double ApplyAngleOffsetValue(AngleOffsetApply ctx) =>
        ApplyAngleOffset(ctx.Degrees) ? ctx.Current + ctx.Degrees : ctx.Current;

    public static bool MusicSecretFound(bool found) => found;

    public static bool MusicCdTrackChange(int current, int next) => current != next;

    public static bool EnvironmentUnderwater(bool water) => water;

    public static bool EnvironmentSecret(bool secret) => secret;

    public static bool UseHoldButton(int buttons, int useMask) => (buttons & useMask) != 0;

    public static bool UseConsumesTick(bool used) => used;

    public static bool SafePosTracksLive(bool live, bool force) => live || force;

    public static bool CopyCmdForAcs(bool predicting) => true;

    public static int ClearInterpFlag(int cheats, int flag) => cheats & ~flag;

    public readonly record struct VelocityRecordGate(bool Predicting, bool HasMo, int Flags, int MfNogravity);

    public static bool ShouldRecordVelocity(VelocityRecordGate g) =>
        RecordVelocityWhenNotPredicting(g.Predicting, g.HasMo);

    public static bool UseXYVelocity(VelocityRecordGate g) =>
        !VelocityUsesFull3D(g.Flags, g.MfNogravity);

    public readonly record struct MusicChangeContext(bool Ready, int Track, int NextTrack);

    public static bool ShouldSwapMusic(MusicChangeContext ctx) =>
        MusicChangeReady(ctx.Ready) && MusicCdTrackChange(ctx.Track, ctx.NextTrack);

    public readonly record struct EnvironmentContext(bool Frozen, bool HasBot, bool Underwater, bool Secret);

    public static bool ShouldSkipThink(EnvironmentContext ctx) =>
        BotFrozenSkip(ctx.Frozen, ctx.HasBot);

    public static bool EnvironmentTriggersFx(EnvironmentContext ctx) =>
        EnvironmentUnderwater(ctx.Underwater) || EnvironmentSecret(ctx.Secret);

    public readonly record struct PlayerThinkLoop(int Index, double OffsetDegrees);

    public static bool ProcessAngleSlot(PlayerThinkLoop slot) =>
        ApplyAngleOffset(slot.OffsetDegrees);

    public static double ResetAngleTarget() => 0;

    public static bool StorePrevAngle(bool applied) => applied;

    public static bool FloatBobPrevSet(bool hasMo) => hasMo;

    public static bool WeaponBobBranch(int bobType) => bobType == Pspb2D || bobType == Pspb3D;

    public static int PlayerIndexFromPtr(int index) => index;

    public static bool LevelVelocitySlotValid(int playerIndex, int maxPlayers) =>
        playerIndex >= 0 && playerIndex < maxPlayers;

    public readonly record struct ThinkCheatsGate(int Cheats, int Predicting, int InterpView);

    public static int StripPresentationCheats(ThinkCheatsGate g) =>
        ClearInterpFlag(ClearInterpFlag(ClearInterpFlag(g.Cheats, CfInterpview), g.InterpView), g.InterpView);

    public static bool OriginalCmdCopied(bool always) => always;

    public static bool PitchLimitsSent(bool sent) => sent;

    public static bool ResurrectGate(bool dead) => dead;
}
