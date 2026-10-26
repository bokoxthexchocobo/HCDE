namespace HCDE.Playsim;

/// <summary>Player think cluster from <c>p_user.cpp</c> (1682–1912).</summary>
public static class VmPUserPlayerThink
{
    public const int CfPredicting = 1;
    public const int CfInterpview = 1 << 1;
    public const int CfInterpviewangles = 1 << 2;
    public const int CfScalednolerp = 1 << 3;
    public const int CfNofovinterp = 1 << 4;
    public const int CfNoviewposinterp = 1 << 5;
    public const int PstLive = 0;
    public const int Pspb2D = 0;
    public const int Pspb3D = 1;
    public const int MfNogravity = 1;
    public const int Mf2Onmobj = 1;
    public const double EqualEpsilon = 1e-6;

    public const int CfPredictingUse = CfPredicting;
    public const int BtUse = 1;

    public readonly record struct AngleOffsetApply(double Degrees, double Current);

    public readonly record struct VelocityRecordGate(bool Predicting, bool HasMo, int Flags, int MfNogravity);

    public readonly record struct MusicChangeContext(bool Ready, int Track, int NextTrack);

    public readonly record struct MusicInfoContext(int Tics, bool HasActor, bool IsConsolePlayer, int Arg0, int Arg1);

    public readonly record struct EnvironmentContext(bool Frozen, bool HasBot, bool Underwater, bool Secret);

    public readonly record struct EnvironmentSoundContext(
        double VelZ, double MinSpeed, double MaxSpeed, bool Underwater, bool HasAlternative);

    public readonly record struct SafePosSample(
        object? Sector,
        double X,
        double Y,
        double Z,
        double FloorZ,
        double DropoffZ,
        double MaxDropOff,
        double MaxStep,
        double Height,
        bool OnMobj,
        int WaterLevel,
        bool Valid);

    public readonly record struct ThinkCheatsGate(int Cheats, int Predicting, int InterpView);

    public readonly record struct PlayerThinkLoop(int Index, double OffsetDegrees);

    public static bool ApplyAngleOffset(double degrees) => Math.Abs(degrees) >= EqualEpsilon;

    public static double ApplyAngleOffsetValue(AngleOffsetApply ctx) =>
        ApplyAngleOffset(ctx.Degrees) ? ctx.Current + ctx.Degrees : ctx.Current;

    public static bool PlayerThinkAngleLoop(int count) => count < 3;

    public static bool ProcessAngleSlot(PlayerThinkLoop slot) => ApplyAngleOffset(slot.OffsetDegrees);

    public static bool SubtitleTick(int counter) => counter > 0;

    public static int DecrementSubtitle(int counter) => counter - 1;

    public static bool UpdateSafePosWhenLive(int playerstate, int pstLive) => playerstate == pstLive;

    public static bool SkipBobTimerWhenPredicting(int cheats, int cfPredicting) =>
        (cheats & cfPredicting) == 0;

    public static bool BotFrozenSkip(bool frozen, bool hasBot) => frozen && hasBot;

    public static bool DebugLogTick(bool debugfile, bool predicting) => debugfile && !predicting;

    public static bool BobWeapon2D(int bobType, int pspb2D) => bobType == pspb2D;

    public static bool BobWeapon3D(int bobType, int pspb3D) => bobType == pspb3D;

    public static bool WeaponBobBranch(int bobType) => bobType == Pspb2D || bobType == Pspb3D;

    public static bool RecordVelocityWhenNotPredicting(bool predicting, bool hasMo) =>
        !predicting && hasMo;

    public static bool VelocityUsesFull3D(int flags, int mfNogravity) =>
        (flags & mfNogravity) != 0;

    public static bool ShouldRecordVelocity(VelocityRecordGate g) =>
        RecordVelocityWhenNotPredicting(g.Predicting, g.HasMo);

    public static double SelectVelocitySpeed(VelocityRecordGate g, double velLength, double velXyLength) =>
        VelocityUsesFull3D(g.Flags, g.MfNogravity) ? velLength : velXyLength;

    public static bool LevelVelocitySlotValid(int playerIndex, int maxPlayers) =>
        playerIndex >= 0 && playerIndex < maxPlayers;

    public static bool MusicCdTrackChange(int current, int next) => current != next;

    public static bool ShouldSwapMusic(MusicChangeContext ctx) =>
        ctx.Ready && MusicCdTrackChange(ctx.Track, ctx.NextTrack);

    public static bool MusicInfoTickActive(MusicInfoContext ctx) => ctx.Tics >= 0 && ctx.HasActor;

    public static int DecrementMusicInfoTics(int tics) => tics - 1;

    public static bool MusicInfoFiredAfterDecrement(int ticsBefore) => ticsBefore - 1 < 0;

    public static bool MusicInfoUsesLookup(MusicInfoContext ctx) => ctx.IsConsolePlayer && ctx.Arg0 != 0;

    public static bool MusicInfoUsesWildcard(MusicInfoContext ctx) => ctx.IsConsolePlayer && ctx.Arg0 == 0;

    public static bool EnvironmentTriggersFx(EnvironmentContext ctx) => ctx.Underwater || ctx.Secret;

    public static bool ShouldSkipThink(EnvironmentContext ctx) => BotFrozenSkip(ctx.Frozen, ctx.HasBot);

    public static bool EnvironmentFallingScream(EnvironmentSoundContext ctx) =>
        ctx.VelZ <= -ctx.MinSpeed && ctx.VelZ >= -ctx.MaxSpeed && !ctx.HasAlternative && !ctx.Underwater;

    public static bool CheckUseSkipsWhenPredicting(int cheats) => (cheats & CfPredictingUse) != 0;

    public static bool UseHoldButton(int buttons, int useMask) => (buttons & useMask) != 0;

    public static bool CheckUseEdge(int buttons, int btUse, bool usedown) =>
        UseHoldButton(buttons, btUse) && !usedown;

    public static bool CheckUseRelease(int buttons, int btUse) => (buttons & btUse) == 0;

    public static bool SafePosLiveUpdate(int playerstate, int pstLive) => playerstate == pstLive;

    public static bool SafePosTracksLive(bool live, bool force) => live || force;

    public static bool SafePosValidOnFloor(SafePosSample s) =>
        s.Sector != null && (
            s.WaterLevel >= 3 ||
            (!s.OnMobj && s.Z <= s.FloorZ && s.Z - s.DropoffZ <= s.MaxDropOff));

    public static bool SafePosForceCopy(bool force, bool live) => force || live;

    public static bool SafePosHeightClear(double ceiling, double floor, double height) => ceiling - floor >= height;

    public static bool SafePosIsSafe(
        SafePosSample s,
        Func<object?, double, double, double, double, double>? nextHighestCeiling,
        Func<object?, double, double, double, double, int, bool>? isDangerous,
        int tid) =>
        s.Valid
        && nextHighestCeiling?.Invoke(s.Sector, s.X, s.Y, s.Z, s.Z + s.Height) - s.Z >= s.Height
        && isDangerous?.Invoke(s.Sector, s.X, s.Y, s.Z, s.Height, tid) != true;

    public static bool PlayerThinkNullMoGuard(object? mo) => mo == null;

    public static int StripPresentationCheats(ThinkCheatsGate g)
    {
        var c = g.Cheats;
        c &= ~CfInterpview;
        c &= ~CfInterpviewangles;
        c &= ~CfScalednolerp;
        c &= ~CfNofovinterp;
        c &= ~CfNoviewposinterp;
        c &= ~g.InterpView;
        return c;
    }

    public static bool PlayerThinkVelocitySlot(bool predicting, bool hasMo) =>
        RecordVelocityWhenNotPredicting(predicting, hasMo);
}
