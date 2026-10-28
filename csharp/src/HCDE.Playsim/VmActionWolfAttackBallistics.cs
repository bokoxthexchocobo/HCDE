namespace HCDE.Playsim;

/// <summary><c>A_WolfAttack</c> dodge, distance, and hitchance math from <c>p_actionfunctions.cpp</c> (3607–3666).</summary>
public static class VmActionWolfAttackBallistics
{
    public const int WafNorandom = 1;
    public const int WafUsepuff = 2;
    public const int MfNoblood = 1 << 6;
    public const int Mf2Invulnerable = 1 << 7;
    public const int Mf2Dormant = 1 << 9;
    public const int Mf3Ghost = 1 << 0;

    public static bool TargetCanDodge(bool targetSeesAttacker, double angleDegrees) =>
        targetSeesAttacker && angleDegrees < 30.0 * 256.0 / 360.0;

    public static double ChebyshevDistance(double dx, double dy) =>
        dx > dy ? dx : dy;

    public static int BlockDistance(int dist, int blockSize) => (int)(dist / blockSize);

    public static int HitchanceFromSpeed(double speedSquared, double runSpeed) =>
        speedSquared < runSpeed ? 256 : 160;

    public static int AdjustHitchanceForDistance(int hitchance, int blockDist, bool dodge) =>
        hitchance - blockDist * (dodge ? 16 : 8);

    public static int ApplyShadowPenalty(int hitchance) => hitchance >> 2;

    public static bool RollHits(int roll, int hitchance) => roll < hitchance;

    public static int DamageFromFlags(int flags, int maxDamage, int roll) =>
        (flags & WafNorandom) != 0 ? maxDamage : 1 + roll % maxDamage;

    public static int HalveDamageAtRange(int damage, bool atOrBeyondThreshold) =>
        atOrBeyondThreshold ? damage >> 1 : damage;

    public static bool ShouldSpawnBlood(int targetFlags, int targetFlags2) =>
        (targetFlags & MfNoblood) == 0
        && (targetFlags2 & (Mf2Invulnerable | Mf2Dormant)) == 0;

    public static int GhostDamagePenalty(int damage) => damage >> 2;
}
