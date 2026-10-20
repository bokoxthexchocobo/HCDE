namespace HCDE.Playsim;

/// <summary>Bone world transform and <c>AActor::Tick</c> prologue from <c>p_mobj.cpp</c> (4475–4637).</summary>
public static class VmPMobjBoneWorldTick
{
    public const int ModeldataGetBoneInfo = 1;
    public const int MfUnmorphed = 1;
    public const int Mf5Nointeraction = 1;
    public const int Mf6Bosscube = 1;
    public const int Mf9Decoupledanimations = 1;
    public const int MfNoblockmap = 1;
    public const int Mf8Inscrollsec = 1;
    public const int ModelanimNone = 1;

    public static readonly int[] HexenCompatSpeeds = { -25, 0, -10, -5, 0, 5, 10, 0, 25 };

    public static readonly (int X, int Y)[] HexenScrollies =
    {
        (0, 1), (0, 2), (0, 4),
        (-1, 0), (-2, 0), (-4, 0),
        (0, -1), (0, -2), (0, -4),
        (1, 0), (2, 0), (4, 0),
        (1, 1), (2, 2), (4, 4),
        (-1, 1), (-2, 2), (-4, 4),
        (-1, -1), (-2, -2), (-4, -4),
        (1, -1), (2, -2), (4, -4),
    };

    public static readonly int[] HereticScrollDirs = { 6, 9, 1, 4 };
    public static readonly int[] HereticSpeedMuls = { 5, 10, 25, 30, 35 };

    public static (double X, double Y, double Z) SwizzlePosition(double x, double y, double z) => (x, z, y);

    public static (double X, double Y, double Z) UnswizzlePosition(double x, double y, double z) => (x, z, y);

    public static (double X, double Y, double Z) NormalizeVector(double x, double y, double z)
    {
        var len = Math.Sqrt(x * x + y * y + z * z);
        if (len == 0) return (0, 0, 0);
        return (x / len, y / len, z / len);
    }

    public static (double X, double Y, double Z, double W) Homogenize(double x, double y, double z, double w) =>
        w == 0 ? (x, y, z, 1) : (x / w, y / w, z / w, 1);

    public readonly record struct BoneWorldTransformContext(
        bool UseOverrideMatrix,
        double PosX,
        double PosY,
        double PosZ,
        double FwdX,
        double FwdY,
        double FwdZ,
        double UpX,
        double UpY,
        double UpZ);

    public static bool EvaluateBonePositionGate(
        bool hasModelData,
        int flags,
        int modelIndex,
        int boneIndex,
        int modelSlots,
        int boneSlots,
        bool picnumValid) =>
        hasModelData
        && (flags & ModeldataGetBoneInfo) != 0
        && modelIndex >= 0
        && boneIndex >= 0
        && modelSlots > modelIndex
        && boneSlots > boneIndex
        && !picnumValid;

    public static (double Px, double Py, double Pz, double Fx, double Fy, double Fz, double Ux, double Uy, double Uz)
        ApplyBoneThenWorldSwizzle(BoneWorldTransformContext ctx)
    {
        var (px, py, pz) = SwizzlePosition(ctx.PosX, ctx.PosY, ctx.PosZ);
        var (fx, fy, fz) = SwizzlePosition(ctx.FwdX, ctx.FwdY, ctx.FwdZ);
        var (ux, uy, uz) = SwizzlePosition(ctx.UpX, ctx.UpY, ctx.UpZ);
        var (nfx, nfy, nfz) = NormalizeVector(fx, fy, fz);
        var (nux, nuy, nuz) = NormalizeVector(ux, uy, uz);
        var worldPos = UnswizzlePosition(px, py, pz);
        var worldFwd = UnswizzlePosition(nfx, nfy, nfz);
        var worldUp = UnswizzlePosition(nux, nuy, nuz);
        return (worldPos.X, worldPos.Y, worldPos.Z, worldFwd.X, worldFwd.Y, worldFwd.Z, worldUp.X, worldUp.Y, worldUp.Z);
    }

    public static bool GetObjectToWorldMatrixGate(bool hasModelData, int flags, bool picnumValid) =>
        hasModelData && (flags & ModeldataGetBoneInfo) != 0 && !picnumValid;

    public static int CopyMatrixFlat(double[] source, double[] dest)
    {
        if (source.Length < 16 || dest.Length < 16) return 0;
        for (var i = 0; i < 16; i++) dest[i] = source[i];
        return 16;
    }

    public readonly record struct UnmorphTickContext(
        bool HasAlternative,
        bool IsPlayer,
        int Flags,
        bool VirtualCheckUnmorph);

    public static bool TickStopsOnUnmorph(UnmorphTickContext ctx)
    {
        if (!ctx.HasAlternative || ctx.IsPlayer) return false;
        if ((ctx.Flags & MfUnmorphed) != 0) return true;
        return ctx.VirtualCheckUnmorph;
    }

    public static bool FreezeTickDecrement(ref int freezetics) =>
        freezetics > 0 && --freezetics >= 0;

    public static bool ShouldCalcBonesOnTick(int flags9) => (flags9 & Mf9Decoupledanimations) != 0;

    public static bool TickRequiresDestroy(bool stateNull) => stateNull;

    public readonly record struct NointeractionFrozenContext(
        bool Frozen,
        int Flags6,
        int Flags9,
        bool HasModelData,
        int AnimFlags);

    public static bool NointeractionFrozenBossCube(NointeractionFrozenContext ctx) =>
        ctx.Frozen && (ctx.Flags6 & Mf6Bosscube) != 0;

    public static bool NointeractionAdvanceDecoupledAnim(NointeractionFrozenContext ctx) =>
        ctx.Frozen
        && (ctx.Flags9 & Mf9Decoupledanimations) != 0
        && ctx.HasModelData
        && (ctx.AnimFlags & ModelanimNone) == 0;

    public static bool CoopAuthorityVisualSkip(bool isCoopVisual) => isCoopVisual;

    public readonly record struct NointeractionMoveContext(
        bool VelocityZero,
        int Flags,
        bool InvasionMirrorKeepBlockmap);

    public static bool ShouldIntegrateNointeractionVelocity(NointeractionMoveContext ctx) =>
        !ctx.VelocityZero || (ctx.Flags & MfNoblockmap) == 0;

    public static bool UnlinkBlockmapForMirror(bool keepBlockmap) => !keepBlockmap;

    public static int ClearInScrollSecFlag(int flags8) => flags8 & ~Mf8Inscrollsec;

    public static int HexenScrollSpeed(int index) =>
        index >= 0 && index < HexenCompatSpeeds.Length ? HexenCompatSpeeds[index] : 0;

    public static (int Dx, int Dy) HexenScrollDelta(int index) =>
        index >= 0 && index < HexenScrollies.Length ? HexenScrollies[index] : (0, 0);

    public static int HereticScrollDir(int index) =>
        index >= 0 && index < HereticScrollDirs.Length ? HereticScrollDirs[index] : 0;

    public static int HereticSpeedMul(int index) =>
        index >= 0 && index < HereticSpeedMuls.Length ? HereticSpeedMuls[index] : 0;

    public readonly record struct TickPrologueResult(
        bool StopTick,
        bool RunCalcBones,
        bool DestroyActor,
        bool ClearScrollFlag);

    public static TickPrologueResult EvaluateTickPrologue(
        UnmorphTickContext unmorph,
        ref int freezetics,
        int flags9,
        bool stateNull,
        int flags5,
        bool frozen,
        bool coopVisual,
        NointeractionMoveContext move)
    {
        if (TickStopsOnUnmorph(unmorph)) return new TickPrologueResult(true, false, false, false);
        if (FreezeTickDecrement(ref freezetics)) return new TickPrologueResult(true, false, false, false);
        var calcBones = ShouldCalcBonesOnTick(flags9);
        if (TickRequiresDestroy(stateNull)) return new TickPrologueResult(true, calcBones, true, false);
        if ((flags5 & Mf5Nointeraction) == 0)
            return new TickPrologueResult(false, calcBones, false, false);
        if (CoopAuthorityVisualSkip(coopVisual))
            return new TickPrologueResult(true, calcBones, false, true);
        if (ShouldIntegrateNointeractionVelocity(move)) { }
        return new TickPrologueResult(false, calcBones, false, true);
    }
}
