namespace HCDE.Playsim;

/// <summary><c>A_Face</c>, <c>A_FaceTarget</c>, and <c>A_Pain</c> from <c>p_enemy.cpp</c> (3046–3269).</summary>
public static class VmPEnemyFacePain
{
    public const int MfStealth = 1;
    public const int MfAmbush = 1;
    public const int FafBottom = 1;
    public const int FafMiddle = 1 << 1;
    public const int FafTop = 1 << 2;
    public const double BobOffsetZ = 32;

    public static bool FaceRequiresOther(bool otherNull) => !otherNull;

    public static bool StealthVisdirOnFace(int flags) => (flags & MfStealth) != 0;

    public static bool ClearsAmbush(int flags) => (flags & MfAmbush) != 0;

    public static bool YawTurnLimited(double maxTurn, double delta) =>
        maxTurn != 0 && maxTurn < Math.Abs(delta);

    public static double TurnTowardNegative(double yaw, double maxTurn, double offset) => yaw - maxTurn + offset;

    public static double TurnTowardPositive(double yaw, double maxTurn, double offset) => yaw + maxTurn + offset;

    public static bool PitchClampEnabled(double maxPitch) => maxPitch <= 180;

    public static bool RetargetZToMiddle(double targetZ, double top) => targetZ >= top;

    public static bool RetargetZToCenter(double sourceZ, double top) => sourceZ >= top;

    public static double ApplyFafBottom(double z, double bob) => z + bob;

    public static double ApplyFafMiddle(double center, double bob) => center + bob;

    public static double ApplyFafTop(double top, double bob) => top + bob;

    public static bool PitchLimitedTurn(bool maxPitchSet, double selfPitch, double otherPitch) => maxPitchSet;

    public static bool PainUsesPlayerBranch(bool hasPlayer, bool altNull) => hasPlayer && altNull;

    public static bool PickLowHealthPain(int health) => health < 25;

    public static bool PickMidHealthPain(int health) => health < 50;

    public static bool PickHighMidPain(int health) => health < 75;

    public static string PainAmountSuffix(int health) =>
        health < 25 ? "*pain25" : health < 50 ? "*pain50" : health < 75 ? "*pain75" : "*pain100";

    public static bool TypedPainSoundFound(int sfx) => sfx != 0;

    public static bool FallbackMonsterPain(bool painSoundValid) => painSoundValid;

    public static double SourceAimZ(double z, double bob, double floorclip) => z + BobOffsetZ + bob - floorclip;

    public static double DeltaAngleToward(double yaw, double other) => other - yaw;

    public static bool UseFullPitchSnap(double maxPitch) => maxPitch == 0;

    public static double AddPitchOffset(double pitch, double offset) => pitch + offset;

    public readonly record struct FacePitchContext(
        double MaxPitch,
        double SelfPitch,
        double OtherPitch,
        bool UseLimit);

    public static double AdjustPitch(FacePitchContext ctx)
    {
        if (!ctx.UseLimit)
            return ctx.OtherPitch;
        if (ctx.SelfPitch > ctx.OtherPitch)
            return ctx.SelfPitch - Math.Min(ctx.MaxPitch, ctx.SelfPitch - ctx.OtherPitch);
        return ctx.SelfPitch + Math.Min(ctx.MaxPitch, ctx.OtherPitch - ctx.SelfPitch);
    }

    public static bool ShadowHandlingAfterPitch(bool afterPitch) => afterPitch;

    public static bool ShadowHandlingAfterYaw(bool afterYaw) => !afterYaw;

    public readonly record struct PainSoundPick(int Health, int LastDamageType, int SfxId);

    public static bool TryDamageTypedPain(PainSoundPick ctx) =>
        ctx.LastDamageType != 0 && ctx.SfxId != 0;

    public static string BuildTypedPainKey(string amount, string damageType) => amount + "-" + damageType;
}
