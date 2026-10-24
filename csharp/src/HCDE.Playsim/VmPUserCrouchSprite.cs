namespace HCDE.Playsim;

/// <summary><c>SetupCrouchSprite</c> and <c>P_CheckPlayerSprite</c> from <c>p_user.cpp</c> (1379–1427, 1531–1572).</summary>
public static class VmPUserCrouchSprite
{
    public const int Mf4Noskin = 1;
    public const int PstDead = 4;
    public const double CrouchSpriteThreshold = 0.75;

    public static bool CrouchSpriteDisabledWhenNonPositive(int crouchSprite) => crouchSprite <= 0;

    public static string SpriteFrameName(string spriteName, string frame) => spriteName + frame;

    public static bool SpriteMissing(int lump) => lump < 0;

    public static bool DisableWhenEitherSpriteMissing(int normLump, int crouchLump) =>
        SpriteMissing(normLump) || SpriteMissing(crouchLump);

    public static bool PwadNormIwadCrouchMismatch(int wadNorm, int wadCrouch, int maxIwadNum) =>
        wadNorm > maxIwadNum && wadCrouch <= maxIwadNum;

    public readonly record struct SetupCrouchSpriteContext(
        int CrouchSprite,
        string NormSpriteName,
        string CrouchSpriteName,
        Func<string, int> CheckSpriteLump,
        Func<int, int> FileContainer,
        int MaxIwadNum);

    /// <summary>Faithful to native <c>SetupCrouchSprite</c> (including <c>GetFileContainer(spritenorm)</c> for crouch WAD id).</summary>
    public static bool EvaluateSetupCrouchSprite(SetupCrouchSpriteContext ctx)
    {
        if (ctx.CrouchSprite <= 0) return true;

        var spritenorm = ctx.CheckSpriteLump(SpriteFrameName(ctx.NormSpriteName, "A1"));
        if (SpriteMissing(spritenorm))
            spritenorm = ctx.CheckSpriteLump(SpriteFrameName(ctx.NormSpriteName, "A0"));

        var spritecrouch = ctx.CheckSpriteLump(SpriteFrameName(ctx.CrouchSpriteName, "A1"));
        if (SpriteMissing(spritecrouch))
            spritecrouch = ctx.CheckSpriteLump(SpriteFrameName(ctx.CrouchSpriteName, "A0"));

        if (DisableWhenEitherSpriteMissing(spritenorm, spritecrouch)) return false;

        var wadnorm = ctx.FileContainer(spritenorm);
        var wadcrouch = ctx.FileContainer(spritenorm);

        if (PwadNormIwadCrouchMismatch(wadnorm, wadcrouch, ctx.MaxIwadNum)) return false;
        return true;
    }

    public static bool ApplySkinScale(int skinIndex, int actorFlags4) =>
        skinIndex != 0 && (actorFlags4 & Mf4Noskin) == 0;

    public static double ScaleSkinAxis(double scale, double skinScale, double defaultScale) =>
        defaultScale == 0 ? scale : scale * (skinScale / defaultScale);

    public static bool ActorIsCrouchingMo(double crouchFactor) => crouchFactor < CrouchSpriteThreshold;

    public static bool UseMoCrouchSprite(int spritenum, int spawnSprite, int crouchSprite) =>
        spritenum == spawnSprite || spritenum == crouchSprite;

    public static bool UseSkinCrouchSprite(int spritenum, int skinSprite, int skinCrouchSprite, int actorFlags4) =>
        (actorFlags4 & Mf4Noskin) == 0 && (spritenum == skinSprite || spritenum == skinCrouchSprite);

    public readonly record struct CheckPlayerSpriteContext(
        int SkinIndex,
        int ActorFlags4,
        double DefaultScaleX,
        double DefaultScaleY,
        double SkinScaleX,
        double SkinScaleY,
        double ScaleX,
        double ScaleY,
        bool MoMatchesActor,
        double CrouchFactor,
        int Spritenum,
        int SpawnSprite,
        int CrouchSprite,
        int SkinSprite,
        int SkinCrouchSprite,
        int PlayerState);

    public readonly record struct CheckPlayerSpriteResult(int Spritenum, double ScaleX, double ScaleY);

    public static CheckPlayerSpriteResult EvaluateCheckPlayerSprite(CheckPlayerSpriteContext ctx)
    {
        var scaleX = ctx.ScaleX;
        var scaleY = ctx.ScaleY;
        if (ApplySkinScale(ctx.SkinIndex, ctx.ActorFlags4))
        {
            scaleX = ScaleSkinAxis(scaleX, ctx.SkinScaleX, ctx.DefaultScaleX);
            scaleY = ScaleSkinAxis(scaleY, ctx.SkinScaleY, ctx.DefaultScaleY);
        }

        var spritenum = ctx.Spritenum;
        if (ctx.MoMatchesActor && ActorIsCrouchingMo(ctx.CrouchFactor))
        {
            int crouchspriteno;
            if (UseMoCrouchSprite(spritenum, ctx.SpawnSprite, ctx.CrouchSprite))
                crouchspriteno = ctx.CrouchSprite;
            else if (UseSkinCrouchSprite(spritenum, ctx.SkinSprite, ctx.SkinCrouchSprite, ctx.ActorFlags4))
                crouchspriteno = ctx.SkinCrouchSprite;
            else
                crouchspriteno = -1;

            if (crouchspriteno > 0)
                spritenum = crouchspriteno;
            else if (ctx.PlayerState != PstDead && ctx.CrouchFactor < CrouchSpriteThreshold)
                scaleY *= 0.5;
        }

        return new CheckPlayerSpriteResult(spritenum, scaleX, scaleY);
    }
}
