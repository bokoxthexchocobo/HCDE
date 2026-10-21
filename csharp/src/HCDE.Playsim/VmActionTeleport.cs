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

    public static bool BlockedByNoteleport(int flags2, int tfFlags) =>
        (flags2 & Mf2Noteleport) != 0 && (tfFlags & TfOverride) == 0;

    public static int RandomDecideIndex(int health, int spawnHealthEighth)
    {
        var denom = spawnHealthEighth == 0 ? 1 : spawnHealthEighth;
        var idx = health / denom;
        return Math.Clamp(idx, 0, RandomDecideChance.Length - 1);
    }

    public static bool RandomDecidePasses(int roll, int index) => roll < RandomDecideChance[index];

    public static bool ShouldAbortRandomDecide(int flags, int roll, int chanceIndex) =>
        (flags & TfRandomdecide) != 0 && !RandomDecidePasses(roll, chanceIndex);

    public static bool SensitiveZFails(double posZ, double height, double ceilingZ, double floorZ, int tfFlags)
    {
        if ((tfFlags & TfSensitivez) == 0) return false;
        var z = (tfFlags & TfUsespotz) != 0 ? posZ : floorZ;
        return z + height > ceilingZ || z < floorZ;
    }

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

    public static bool ShouldForceOrigin(bool teleResult, int tfFlags) =>
        !teleResult && (tfFlags & TfForced) != 0;

    public static bool WantsFogBlock(bool teleResult, bool fogTypeNull, int tfFlags) =>
        teleResult && (!fogTypeNull || (tfFlags & TfUseactorfog) != 0);

    public static bool EmitSourceFog(int tfFlags) => (tfFlags & TfNosrcfog) == 0;

    public static bool EmitDestFog(int tfFlags) => (tfFlags & TfNodestfog) == 0;

    public static bool SourceFogUsesActor(int tfFlags) => (tfFlags & TfUseactorfog) != 0;

    public static bool DestFogUsesActor(int tfFlags) => (tfFlags & TfUseactorfog) != 0;

    public static bool SourceFogUsesSpawnedClass(int tfFlags, bool fogTypeNull) =>
        EmitSourceFog(tfFlags) && !SourceFogUsesActor(tfFlags) && !fogTypeNull;

    public static double PostMoveZ(int tfFlags, double spotZ, double floorZ) =>
        (tfFlags & TfUsespotz) != 0 ? spotZ : floorZ;

    public static double YawAfterTeleport(int tfFlags, double spotYaw, double currentYaw) =>
        (tfFlags & TfKeepangle) != 0 ? currentYaw : spotYaw;

    public static bool ZeroVelocityAfterTeleport(int tfFlags) => (tfFlags & TfKeepvelocity) == 0;

    public static bool StateJumpAllowed(int tfFlags) => (tfFlags & TfNojump) == 0;

    public static bool TelefragMove(int tfFlags) => (tfFlags & TfTelefrag) != 0;

    public readonly record struct AActorRef(int Health, int SpawnHealth, int Flags2, double Height);
}
