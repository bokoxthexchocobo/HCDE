namespace HCDE.Playsim;

/// <summary>Door, pillar, and thing-destroy extension specials from <c>p_lnspec.cpp</c> (240–334, 472–487, 652–668, 1515–1563).</summary>
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

    public readonly record struct TextureFloorCall(int Tag, int Speed, int Change, int Crush);

    public static TextureFloorCall PackRaiseByTexture(int tag, int speed, int change, int crush) =>
        new(tag, speed, change, crush);

    public static TextureFloorCall PackLowerByTexture(int tag, int speed, int change, int crush) =>
        new(tag, speed, change, -1);

    public static TextureFloorCall PackRaiseToCeiling(int tag, int speed, int change, int crush, int gap) =>
        new(tag, speed, change, crush);

    public enum PillarKind
    {
        Build,
        Open,
    }

    public readonly record struct PillarCall(PillarKind Kind, int Tag, int Speed, int Height, int Crush, int CrushType);

    public static PillarCall PackPillarBuild(int tag, int speed, int height) =>
        new(PillarKind.Build, tag, speed, height, 0, -1);

    public static PillarCall PackPillarBuildAndCrush(int tag, int speed, int height, int crush, int crushType) =>
        new(PillarKind.Build, tag, speed, height, crush, crushType);

    public static PillarCall PackPillarOpen(int tag, int speed, int floorHeight, int ceilHeight) =>
        new(PillarKind.Open, tag, speed, floorHeight, ceilHeight, -1);

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

    public static bool ThingDamageDelegates(int tid) => true;

    public static int ThingDamageMod(int modArg) => modArg;

    public readonly record struct ThingProjectileCall(int Tid, int Type, int ByteAngle, int Speed, int VSpeed, bool Gravity);

    public static ThingProjectileCall PackProjectile(int tid, int type, int angle, int speed, int vSpeed, bool gravity) =>
        new(tid, type, angle, speed, vSpeed, gravity);

    public static double ByteAngleToDegrees(int byteAngle) => byteAngle * (360.0 / 256.0);

    public static int SpeedArg(int raw) => raw;

    public static bool ProjectileUsesGravity(ThingProjectileCall call) => call.Gravity;

    public static bool DoorEvCallReturnsTrue() => true;

    public static bool PillarEvCallReturnsTrue() => true;

    public static bool DestroyReturnsTrue() => true;

    public static int OcticsDelay(int arg) => arg;

    public static int TicsDelay(int arg) => arg;

    public static bool KeysCheckSide(bool sameSide) => sameSide;
}
