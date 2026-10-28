namespace HCDE.Playsim;

/// <summary><c>A_Jump</c>, <c>A_RadiusDamageSelf</c>, <c>A_CustomRailgun</c> from <c>p_actionfunctions.cpp</c>.</summary>
public static class VmActionJumpRadiusRail
{
    public const int RdsfBfgDamage = 1;
    public const int CrfExplicitAngle = 4;
    public const int MfStealth = 1 << 4;

    public static bool JumpShouldChangeState(int maxChance, int randomRoll) =>
        maxChance >= 256 || randomRoll < maxChance;

    public static int RadiusDamageSteps(int damage, double actualDistance, double distanceLimit) =>
        damage - (int)(damage * actualDistance / distanceLimit);

    public static int BfgDamagePerRoll(int prValue) => (prValue & 7) + 1;

    public static int SumBfgDamage(int damageSteps, Func<int> prBfgSelfDamage)
    {
        var total = 0;
        for (var i = 0; i < damageSteps; i++)
            total += BfgDamagePerRoll(prBfgSelfDamage());
        return total;
    }

    public static int SelectActualDamage(int damageSteps, int flags, Func<int> prBfgSelfDamage) =>
        (flags & RdsfBfgDamage) != 0 ? SumBfgDamage(damageSteps, prBfgSelfDamage) : damageSteps;

    public static bool CustomRailgunShouldAbort(bool aim, bool hasTarget) => aim && !hasTarget;

    public static double NormalizeRailRange(double range) => range == 0 ? 8192.0 : range;

    public static double NormalizeRailSparsity(double sparsity) => sparsity == 0 ? 1.0 : sparsity;

    public static int StealthVisDirOnFire(int actorFlags) => (actorFlags & MfStealth) != 0 ? 1 : 0;

    public static (double spreadXy, double spreadZ) SpreadJitter(bool explicitAngle, double spreadXy, double spreadZ, int prRandom2) =>
        explicitAngle ? (spreadXy, spreadZ) : (spreadXy * prRandom2 / 255.0, spreadZ * prRandom2 / 255.0);

    public static int FlashDamageFlags(bool puffGetsOwner, bool foilInvul, bool foilBuddha)
    {
        var flags = 0;
        if (foilInvul)
            flags |= 1;
        if (foilBuddha)
            flags |= 2;
        return flags;
    }

    public sealed class RailParams
    {
        public int Damage;
        public int OffsetXy;
        public int OffsetZ;
        public int Color1;
        public int Color2;
        public double MaxDiff;
        public int Flags;
        public double AngleOffset;
        public double PitchOffset;
        public double Distance;
        public int Duration;
        public double Sparsity;
        public double Drift;
        public int SpiralOffset;
        public int Limit;
    }

    public static RailParams FillRailParams(
        int damage,
        int offsetXy,
        int offsetZ,
        int color1,
        int color2,
        double maxDiff,
        int flags,
        double angleOffset,
        double pitchOffset,
        double distance,
        int duration,
        double sparsity,
        double drift,
        int spiralOffset,
        int limit) =>
        new()
        {
            Damage = damage,
            OffsetXy = offsetXy,
            OffsetZ = offsetZ,
            Color1 = color1,
            Color2 = color2,
            MaxDiff = maxDiff,
            Flags = flags,
            AngleOffset = angleOffset,
            PitchOffset = pitchOffset,
            Distance = distance,
            Duration = duration,
            Sparsity = sparsity,
            Drift = drift,
            SpiralOffset = spiralOffset,
            Limit = limit,
        };
}
