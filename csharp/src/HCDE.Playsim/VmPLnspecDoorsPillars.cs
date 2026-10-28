namespace HCDE.Playsim;

/// <summary>Door, waggle, ceiling, and thing-destroy gap specials from <c>p_lnspec.cpp</c> (241–304, 508–531, 842–883, 1515–1550).</summary>
public static class VmPLnspecDoorsPillars
{
    public const int MfShootable = 1;
    public const int TelefragDamage = 1000000;
    public const int Level2HexenHack = 1;

    public enum DoorKind
    {
        Raise = 0,
        Open = 1,
        CloseWaitOpen = 2,
        Close = 3,
    }

    public readonly record struct DoorCall(int Tag, int Speed, int DelayTics, int Lock, int LightTag);

    public static DoorCall PackDoorClose(int tag, int speed, int lightTag) => new(tag, speed, 0, 0, lightTag);

    public static DoorCall PackDoorOpen(int tag, int speed, int lightTag) => new(tag, speed, 0, 0, lightTag);

    public static DoorCall PackDoorRaise(int tag, int speed, int delay, int lightTag) => new(tag, speed, delay, 0, lightTag);

    public static DoorCall PackDoorCloseWaitOpen(int tag, int speed, int delay, int lightTag) =>
        new(tag, speed, delay, 0, lightTag);

    public static bool LockedRaiseUsesRaise(int delayArg, int levelFlags) =>
        delayArg != 0 || (levelFlags & Level2HexenHack) != 0;

    public static DoorKind LockedRaiseKind(int delayArg, int levelFlags) =>
        LockedRaiseUsesRaise(delayArg, levelFlags) ? DoorKind.Raise : DoorKind.Open;

    public static DoorCall PackDoorWaitRaise(int tag, int speed, int delay, int wait, int lightTag) =>
        new(tag, speed, delay, 0, lightTag);

    public static DoorCall PackDoorWaitClose(int tag, int speed, int wait, int lightTag) =>
        new(tag, speed, wait, 0, lightTag);

    public static bool AnimatedDoorRequiresKeys(int lockArg, bool hasKeys) =>
        lockArg == 0 || hasKeys;

    public static bool AnimatedDoorOpenClose(int lockArg, bool keysOk) =>
        lockArg == 0 || keysOk;

    public static int AnimatedCloseSpeedArg(int speed) => speed;

    public readonly record struct GenericDoorPlan(DoorKind Kind, bool BoomGen, int Tag, int LightTag, int Lock);

    public static GenericDoorPlan PlanGenericDoor(int arg0, int arg1, int arg2, int arg3, int arg4)
    {
        var kindBits = arg2 & 63;
        var kind = kindBits switch
        {
            0 => DoorKind.Raise,
            1 => DoorKind.Open,
            2 => DoorKind.CloseWaitOpen,
            3 => DoorKind.Close,
            _ => (DoorKind)(-1),
        };
        var boomGen = (arg2 & 64) != 0;
        var localLight = (arg2 & 128) != 0;
        var tag = localLight ? 0 : arg0;
        var lightTag = localLight ? arg0 : 0;
        return new(kind, boomGen, tag, lightTag, arg4);
    }

    public static bool GenericDoorKindValid(GenericDoorPlan plan) => (int)plan.Kind >= 0;

    public static int GenericDoorCrushArg(int arg4) => arg4;

    public readonly record struct WaggleCall(int Tag, int Amplitude, int Frequency, int Delay, int Time, bool Ceiling);

    public static WaggleCall PackFloorWaggle(int tag, int a1, int a2, int a3, int a4) =>
        new(tag, a1, a2, a3, a4, false);

    public static WaggleCall PackCeilingWaggle(int tag, int a1, int a2, int a3, int a4) =>
        new(tag, a1, a2, a3, a4, true);

    public readonly record struct TransferChangeCall(int Tag, bool NumericOnly);

    public static TransferChangeCall PackTransferTrigger(int tag) => new(tag, false);

    public static TransferChangeCall PackTransferNumeric(int tag) => new(tag, true);

    public readonly record struct CeilingLowerCall(int Tag, int Speed, int Change, int Crush, int Gap);

    public static CeilingLowerCall PackCeilingLowerToLowest(int tag, int speed, int change, int crush) =>
        new(tag, speed, change, crush, 0);

    public static CeilingLowerCall PackCeilingLowerToFloor(int tag, int speed, int change, int crush, int gap) =>
        new(tag, speed, change, crush, gap);

    public static CeilingLowerCall PackCeilingLowerByTexture(int tag, int speed, int change, int crush) =>
        new(tag, speed, change, crush, 0);

    public static bool CeilingInstantUsesSpeedTwo() => true;

    public readonly record struct CeilingInstantCall(int Tag, int Change, int Crush, int Gap);

    public static CeilingInstantCall PackCeilingToHighestInstant(int tag, int change, int crush) =>
        new(tag, change, crush, 0);

    public static CeilingInstantCall PackCeilingToFloorInstant(int tag, int change, int crush, int gap) =>
        new(tag, change, crush, gap);

    public static CeilingLowerCall PackCeilingLowerToNearest(int tag, int speed, int change, int crush) =>
        new(tag, speed, change, crush, 0);

    public static int CeilingInstantSpeed() => 2;

    public static int CeilingCrushArg(int arg3) => arg3;

    public static int CeilingChangeArg(int arg2) => arg2;

    public static bool CeilingLowerIncludesCrush(CeilingLowerCall call) => call.Crush != 0;

    public static bool WaggleCeilingFlag(WaggleCall call) => call.Ceiling;

    public static bool TransferNumericOnly(TransferChangeCall call) => call.NumericOnly;

    public static int DonutPillarTagFromWaggleBand(int tag) => tag;

    public static bool DestroyMassacreSkipped(int tid, int tag) => tid != 0 || tag != 0;

    public static bool DestroyIteratorAdvance(bool hasNext) => hasNext;

    public static int DestroyTelefragWhenExtreme(bool extreme) => extreme ? TelefragDamage : 0;

    public static bool DestroySectorScanUsesTag(int tag) => tag != 0;

    public static bool DestroyTidScanAllowsUntagged(int tagArg) => tagArg == 0;

    public static DoorKind DoorCloseKind() => DoorKind.Close;

    public static DoorKind DoorOpenKind() => DoorKind.Open;

    public static DoorKind DoorRaiseKind() => DoorKind.Raise;

    public static bool SlidingDoorAnimatedCloseUsesMinusOneDelay() => true;

    public static int SlidingDoorCloseDelay() => -1;

    public readonly record struct DestroyIteratorPlan(bool BySectorTag, int Tag, bool Extreme, bool ByTid, int Tid);

    public static DestroyIteratorPlan PlanDestroy(int tid, int extreme, int tag) =>
        tid == 0 && tag != 0
            ? new(true, tag, extreme != 0, false, 0)
            : new(false, tag, extreme != 0, true, tid);

    public static bool DestroySectorCandidate(bool shootable, bool sectorHasTag) =>
        shootable && sectorHasTag;

    public static bool DestroyTidCandidate(bool shootable, int tagArg, bool sectorHasTag) =>
        shootable && (tagArg == 0 || sectorHasTag);

    public static int DestroyDamageAmount(bool extreme, int health) => extreme ? TelefragDamage : health;

    public static bool AnimatedDoorRequiresKeysWhenLockSet(int lockArg, bool hasKeys) =>
        lockArg == 0 || hasKeys;

    public static bool DoorEvCallReturnsTrue() => true;

    public static bool WaggleEvCallReturnsTrue() => true;

    public static bool CeilingEvCallReturnsTrue() => true;

    public static bool DestroyReturnsTrue() => true;

    public static bool StairsRepeatableToggle(bool res) => res;

    public static int OcticsDelay(int arg) => arg;

    public static int TicsDelay(int arg) => arg;

    public static bool KeysCheckSide(bool sameSide) => sameSide;

    public static int SpeedScaled(int raw) => raw;

    public static int CrushScaled(int raw) => raw;

    public static int ChangeEncoded(int raw) => raw;

    public static bool RepeatStairsToggleOnSuccess(bool res, bool repeatSpecial) => res && repeatSpecial;

    public static int RepeatStairsArgToggle(int arg3) => arg3 ^ 1;
}
