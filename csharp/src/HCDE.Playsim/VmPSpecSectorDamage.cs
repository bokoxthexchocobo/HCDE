namespace HCDE.Playsim;

/// <summary><c>P_SectorDamage</c> and <c>DoSectorDamage</c> from <c>p_spec.cpp</c> (509–652).</summary>
public static class VmPSpecSectorDamage
{
    public const int MfShootable = 1;
    public const int DamageNonplayers = 1;
    public const int DamagePlayers = 2;
    public const int DamageInAir = 4;
    public const int DamageNoArmor = 8;
    public const int DamageSubclassesProtect = 16;

    public static bool SkipNonShootable(bool shootable) => !shootable;

    public static bool SkipNonPlayersWhenFlag(bool flag, object? player) => flag && player == null;

    public static bool SkipPlayersWhenFlag(bool flag, object? player) => flag && player != null;

    public static bool SkipInAirWhenOnFloor(bool inAirFlag, bool onFloor, bool inWater) =>
        !inAirFlag && !onFloor && !inWater;

    public static bool ProtectedByInventory(bool hasProtectClass, bool found) => hasProtectClass && found;

    public static int DamageFlagsNoArmor(int flags) => (flags & DamageNoArmor) != 0 ? 1 : 0;

    public static bool IterateSectorThings(bool hasThing) => hasThing;

    public static bool Attached3dFloorLoop(bool hasAttached) => hasAttached;

    public static bool VavoomSwapHeights(double z1, double z2, out double low, out double high)
    {
        if (z2 < z1)
        {
            low = z2;
            high = z1;
            return true;
        }
        low = z1;
        high = z2;
        return false;
    }

    public static bool ActorTouches3dFloor(double top, double z1) => top > z1;

    public static bool DamageInAir3dFloor(int flags, bool aboveCeil) =>
        (flags & DamageInAir) != 0 || !aboveCeil;

    public static int OrInAirFlag(int flags) => flags | DamageInAir;

    public static bool TerrainDamageTick(int levelTime, int mask) => levelTime % (mask + 1) == 0;

    public static bool TerrainAllowProtection(bool allow) => allow;

    public static bool IronFeetFound(object? ironfeet) => ironfeet != null;

    public static bool TerrainHasDamage(int amount) => amount != 0;

    public static bool ApplyTerrainDamageWhenUnprotected(bool allowProtection, object? ironfeet) =>
        !allowProtection || ironfeet == null;

    public static bool PlaySplashWhenDamagePositive(int damage, int splashIndex) => damage > 0 && splashIndex != -1;

    public static bool IncrementPlayerSecrets(object? player) => player != null;

    public static bool ShowSecretMessage(bool retval, bool clShow, bool localView) => retval && clShow && localView;

    public static bool PrintSecretSector(bool showSector, int sectornum) => showSector && sectornum >= 0;

    public static bool PlaySecretSound(bool playsound) => playsound;

    public static bool CountFoundSecret() => true;

    public static bool ActorOnSecretSector(bool player, bool isSecret) => player && isSecret;

    public static bool ClearSecretOnTouch() => true;

    public static bool GiveSecretVmCallParams(bool printmessage, bool playsound) => printmessage || playsound;
}
