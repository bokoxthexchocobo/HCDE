namespace HCDE.Playsim;

/// <summary><c>AActor::A_Teleport</c> from <c>p_actionfunctions.cpp</c> (3255–3423).</summary>
public static class VmActionTeleport
{
    public const int TfOverride = 0x00000001;
    public const int TfRandomdecide = 0x00000002;
    public const int TfSensitivez = 0x00000800;
    public const int TfUsespotz = 0x00000040;
    public const int TfTelefrag = 0x00000008;
    public const int TfForced = 0x00000010;
    public const int TfUseactorfog = 0x00000080;
    public const int TfNosrcfog = 0x00000100;
    public const int TfNodestfog = 0x00000200;
    public const int TfKeepangle = 0x00000004;
    public const int TfKeepvelocity = 0x00000020;
    public const int TfNojump = 0x00000400;

    public const int Mf2Noteleport = 0x8000000;

    private static readonly int[] RandomDecideChance = [192, 120, 120, 120, 64, 64, 32, 16, 0];

    public static bool RefNull(AActorRef? refActor) => refActor is null;

    public static bool BlockedByNoteleport(int flags2, int tfFlags) =>
        (flags2 & Mf2Noteleport) != 0 && (tfFlags & TfOverride) == 0;

    public static int RandomDecideIndex(int health, int spawnHealthEighth)
    {
        var denom = spawnHealthEighth == 0 ? 1 : spawnHealthEighth;
        var idx = health / denom;
        return Math.Clamp(idx, 0, RandomDecideChance.Length - 1);
    }

    public static bool RandomDecidePasses(int roll, int index) => roll < RandomDecideChance[index];

    public static bool SpotStateMissing(bool hasState) => !hasState;

    public static bool DefaultBossSpotType(bool targetNull) => targetNull;

    public static bool SpotMissing(bool hasSpot) => !hasSpot;

    public static bool SensitiveZFails(double posZ, double height, double ceilingZ, double floorZ, int tfFlags)
    {
        if ((tfFlags & TfSensitivez) == 0) return false;
        return posZ + height > ceilingZ || posZ < floorZ;
    }

    public static double PosZForSensitive(bool useSpotZ, double spotZ, double floorZ) =>
        useSpotZ ? spotZ : floorZ;

    public static double ComputeFinalZ(double spotZ, double floorZ, double ceilingZ, double refHeight, double spotTop)
    {
        var aboveFloor = spotZ - floorZ;
        var finalZ = floorZ + aboveFloor;
        if (spotTop > ceilingZ)
            finalZ = ceilingZ - refHeight;
        else if (spotZ < floorZ)
            finalZ = floorZ;
        return finalZ;
    }

    public static bool TelefragFlag(int tfFlags) => (tfFlags & TfTelefrag) != 0;

    public static bool ShouldForceOrigin(bool teleResult, int tfFlags) =>
        !teleResult && (tfFlags & TfForced) != 0;

    public static bool ShouldSpawnFog(bool teleResult) => teleResult;

    public static bool FogUsesActor(bool fogTypeNull, int tfFlags) =>
        fogTypeNull && (tfFlags & TfUseactorfog) != 0;

    public static bool SkipSourceFog(int tfFlags) => (tfFlags & TfNosrcfog) != 0;

    public static bool SkipDestFog(int tfFlags) => (tfFlags & TfNodestfog) != 0;

    public static bool UseSpotZAfterMove(int tfFlags) => (tfFlags & TfUsespotz) != 0;

    public static bool KeepAngle(int tfFlags) => (tfFlags & TfKeepangle) != 0;

    public static bool KeepVelocity(int tfFlags) => (tfFlags & TfKeepvelocity) != 0;

    public static bool ShouldStateJump(int tfFlags) => (tfFlags & TfNojump) == 0;

    public static bool TeleportStateNull(bool stateNull) => stateNull;

    public static bool InventoryChainResult(int numRet) => numRet > 1;

    public static bool SetTeleResult(int numRet) => numRet > 1;

    public readonly record struct AActorRef(int Health, int SpawnHealth, int Flags2, double Height);
}
