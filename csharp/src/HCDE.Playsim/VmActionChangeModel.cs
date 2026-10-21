namespace HCDE.Playsim;

/// <summary>Model flag overrides and <c>ChangeModelNative</c> policy from <c>p_actionfunctions.cpp</c> (6740–6989).</summary>
public static class VmActionChangeModel
{
    public const int CmdlWeapontoplayer = 1 << 0;
    public const int CmdlHidemodel = 1 << 1;
    public const int CmdlUsesurfaceskin = 1 << 2;
    public const int ModeldataIqmflagsMask = int.MaxValue;

    public static int MaskIqmFlags(int iqmFlag) => iqmFlag & ModeldataIqmflagsMask;

    public static bool ShouldSetOverrideFlags(int flag) => flag != 0;

    public static int ApplySetModelFlag(int flags, int flag, int overrideFlagsSet, int overrideFlagsClear) =>
        ShouldSetOverrideFlags(flag)
            ? flags | flag | (overrideFlagsSet | flag) & ~overrideFlagsClear
            : flags | flag;

    public static int ApplyClearModelFlag(int flags, int flag, int overrideFlagsSet, int overrideFlagsClear) =>
        ShouldSetOverrideFlags(flag)
            ? (flags & ~flag) | (overrideFlagsClear | flag) & ~overrideFlagsSet
            : flags & ~flag;

    public static bool ShouldResetModelOverrides(int resetModel) => resetModel != 0;

    public static bool ShouldResetIqmFlags(int resetIqm) => resetIqm != 0;

    public static int ClearOverrideFlagBits(int flags) => flags & ~1;

    public static int ClearIqmFlagBits(int flags, int iqmMask) => flags & ~iqmMask;

    public static bool UseWeaponToPlayer(int flags) => (flags & CmdlWeapontoplayer) != 0;

    public static bool HideModelQuery(int flags) => (flags & CmdlHidemodel) != 0;

    public static bool UseSurfaceSkinPath(int flags) => (flags & CmdlUsesurfaceskin) != 0;

    public static int NormalizeModelIndex(int modelIndex) => modelIndex < 0 ? 0 : modelIndex;

    public static int NormalizeSkinIndex(int skinIndex) => skinIndex < 0 ? 0 : skinIndex;

    public static int NormalizeAnimationIndex(int animationIndex) => animationIndex < 0 ? 0 : animationIndex;

    public static bool SelectInvokerAsMobj(bool fromPsprite, bool fromInventory, int flags, bool useSelf) =>
        (fromPsprite && UseWeaponToPlayer(flags)) || fromInventory ? useSelf : !useSelf;

    public static string EnsureTrailingSlash(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;
        return path.EndsWith('/') ? path : path + '/';
    }

    public static int QueryModelWhenVisible(int flags, bool modelNameSet) =>
        HideModelQuery(flags) ? -2 : modelNameSet ? 0 : -1;

    public static int QueryAnimationWhenNamed(bool animationNameSet) => animationNameSet ? 0 : -1;

    public static bool NeedsGrowModels(int modelCount, int modelIndex) => modelCount < modelIndex;

    public static int GrowModelSlots(int modelCount, int modelIndex) => modelIndex - modelCount;

    public static bool NeedsGrowAnimationIds(int animIdCount, int animationIndex) => animIdCount < animationIndex;

    public static bool AppendNewModelSlot(int modelCount, int modelIndex) => modelCount == modelIndex;

    public static bool ShouldPushSurfaceSkin(int flags, int skinIndex) => UseSurfaceSkinPath(flags) && skinIndex >= 0;

    public static int SurfaceSkinPadCount(int skinIndex) => skinIndex > 0 ? skinIndex : 0;

    public static bool ShouldClearBoneOverrides(int queryModel, int boneOverrideCount, int modelIndex) =>
        queryModel != -1 && boneOverrideCount > modelIndex;

    public static bool ShouldUpdateExistingModelId(int queryModel) => queryModel != -1;

    public static bool ShouldUpdateGenerator(int generatorIndex) => generatorIndex != -1;

    public static bool ShouldResetCurAnimOnAnimationChange(
        int curAnimFlags,
        int modelAnimNone,
        int animationIndex,
        int animationId0,
        int queryAnimation) =>
        (curAnimFlags & modelAnimNone) == 0 && animationIndex == 0 && (animationId0 != queryAnimation);

    public static bool AppendAnimationIdSlot(int animIdCount, int animationIndex) => animIdCount == animationIndex;

    public static bool UseGlobalSkinArray(int flags) => !UseSurfaceSkinPath(flags);

    public static bool NeedsGrowSkinIds(int skinIdCount, int skinIndex) => skinIdCount < skinIndex;

    public static bool AppendSkinIdSlot(int skinIdCount, int skinIndex) => skinIdCount == skinIndex;

    public static bool ShouldCalcBonesAfterChange(bool animationNamed, bool modelDefSet) => animationNamed || modelDefSet;

    public static bool InvalidModelDefWhenNamed(bool modelDefNamed, bool classFound) => modelDefNamed && !classFound;

    public static bool ModelDefNameIsNone(int modelDefIndex) => modelDefIndex == 0;

    public static bool ModelNameIsNone(int modelIndex) => modelIndex == 0;

    public static bool SkinNameIsNone(int skinIndex) => skinIndex == 0;

    public static bool AnimationNameIsNone(int animationIndex) => animationIndex == 0;

    public static bool ShouldGrowSurfaceSkins(int existingCount, int skinIndex) => skinIndex > existingCount;

    public static bool AssignSurfaceSkinAtEnd(int existingCount, int skinIndex) => skinIndex == existingCount;

    public static bool AssignSurfaceSkinInPlace(int existingCount, int skinIndex) => skinIndex < existingCount;

    public static bool ShouldAssignSkinAtEnd(int skinIdCount, int skinIndex) => skinIdCount == skinIndex;

    public static bool ShouldAssignSkinInPlace(int skinIdCount, int skinIndex) => skinIdCount > skinIndex;

    public static int HideModelQuerySentinel() => -2;

    public static int MissingModelQuerySentinel() => -1;

    public static bool HasModelData(bool modelDataNonNull) => modelDataNonNull;

    public static bool GeneratorIndexValid(int generatorIndex) => generatorIndex != -1;
}
