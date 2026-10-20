namespace HCDE.Playsim;

/// <summary><c>CalcBones</c> and bone query helpers from <c>p_mobj.cpp</c> (4308–4474).</summary>
public static class VmPMobjCalcBones
{
    public const int ModeldataGetBoneInfo = 1;
    public const int ModeldataGetBoneInfoRecalc = 1;
    public const int Mf9Decoupledanimations = 1;

    public static bool HasModelData(bool has) => has;

    public static bool RecalcOrBoneFlag(bool recalc, int flags, int recalcFlag, int boneFlag) =>
        (!recalc || (flags & recalcFlag) != 0) && (flags & boneFlag) != 0;

    public static bool PicnumOverridesModel(bool picnumValid) => picnumValid;

    public static bool MissingModelFrame(bool found) => !found;

    public static bool DecoupledAnimations(int flags9) => (flags9 & Mf9Decoupledanimations) != 0;

    public static double ModelTimerTic(int timer) => timer + 1;

    public static bool CalcOverridesPass(bool pass) => pass;

    public static bool EvaluateSingleModel(bool attachments, bool decoupled) =>
        attachments || decoupled;

    public static bool NextFrameForProcess(bool hasNext, int frame, int nextFrame) =>
        hasNext && frame != nextFrame;

    public static bool BoneIndexValid(int modelIndex, int boneIndex) => modelIndex >= 0 && boneIndex >= 0;

    public static bool BoneArraySized(int modelCount, int modelIndex, int boneCount, int boneIndex) =>
        modelCount > modelIndex && boneCount > boneIndex;

    public static bool WithOverrideMatrix(bool withOverride) => withOverride;

    public static bool PicnumBlocksBoneQuery(bool picnumValid) => picnumValid;

    public readonly record struct CalcBonesContext(
        bool HasModelData,
        bool Recalc,
        int Flags,
        bool PicnumValid,
        bool FoundModelFrame,
        int Flags9,
        int ModelTimer,
        bool OverridePass,
        bool AttachmentsFlag,
        bool HasNextFrame,
        int Frame,
        int NextFrame);

    public static bool EvaluateCalcBones(CalcBonesContext ctx)
    {
        if (!HasModelData(ctx.HasModelData)) return false;
        if (!RecalcOrBoneFlag(ctx.Recalc, ctx.Flags, ModeldataGetBoneInfoRecalc, ModeldataGetBoneInfo)) return false;
        if (PicnumOverridesModel(ctx.PicnumValid)) return false;
        if (MissingModelFrame(ctx.FoundModelFrame)) return false;
        if (!CalcOverridesPass(ctx.OverridePass)) return false;
        if (EvaluateSingleModel(ctx.AttachmentsFlag, DecoupledAnimations(ctx.Flags9))) { }
        return NextFrameForProcess(ctx.HasNextFrame, ctx.Frame, ctx.NextFrame) || true;
    }

    public readonly record struct BoneQueryContext(
        bool HasModelData,
        int Flags,
        int ModelIndex,
        int BoneIndex,
        int ModelSlots,
        int BoneSlots,
        bool PicnumValid);

    public static bool EvaluateBoneQuery(BoneQueryContext ctx) =>
        HasModelData(ctx.HasModelData)
        && (ctx.Flags & ModeldataGetBoneInfo) != 0
        && BoneIndexValid(ctx.ModelIndex, ctx.BoneIndex)
        && BoneArraySized(ctx.ModelSlots, ctx.ModelIndex, ctx.BoneSlots, ctx.BoneIndex)
        && !PicnumBlocksBoneQuery(ctx.PicnumValid);
}
