namespace HCDE.Playsim;

/// <summary>SAF flags and <c>FindFModel</c> selection from <c>p_actionfunctions.cpp</c> (6351–6505).</summary>
public static class VmActionModelAnimation
{
    public const int SafInstant = 1 << 0;
    public const int SafLoop = 1 << 1;
    public const int SafNooverride = 1 << 2;

    public static bool SkipSameAnimation(int flags, bool curAnimNone, bool sameStartFrame) =>
        (flags & SafNooverride) != 0 && !curAnimNone && sameStartFrame;

    public static bool HasBaseSpriteModelFrames(bool hasKey) => hasKey;

    public static bool UseModelDefForSpriteFrames(bool hasModelData, bool hasModelDef) =>
        hasModelData && hasModelDef;

    public static int ResolvePrimaryAnimId(bool actorHasAnimationIds, int actorAnimId0, int classAnimId0)
    {
        if (actorHasAnimationIds && actorAnimId0 >= 0)
            return actorAnimId0;

        return classAnimId0;
    }

    public static bool TryPickModelIndex(int animId, int modelCount, int fallbackModelId, out int index)
    {
        if (animId >= 0 && animId < modelCount)
        {
            index = animId;
            return true;
        }

        if (fallbackModelId >= 0 && fallbackModelId < modelCount)
        {
            index = fallbackModelId;
            return true;
        }

        index = -1;
        return false;
    }

    public static bool TryPickBaseSpriteModelIndex(int baseSpriteModelId, int modelCount, out int index)
    {
        if (baseSpriteModelId >= 0 && baseSpriteModelId < modelCount)
        {
            index = baseSpriteModelId;
            return true;
        }

        index = -1;
        return false;
    }

    public static bool AnimationLayerHasPointerFields(bool hasCurAnim, bool hasPrevAnim) =>
        hasCurAnim || hasPrevAnim;

    public static bool AnimationSequenceTracksLoopFrame(int loopFrame) => loopFrame >= 0;

    public static bool ActorHasPerInstanceAnimationIds(int animationIdCount) => animationIdCount > 0;

    public static bool ActorAnimationIdSlotValid(int animationId0) => animationId0 >= 0;

    public static bool ModelDataModelSlotValid(int modelId0) => modelId0 >= 0;

    public static int ResolveFallbackModelId(int modelDataModelId0, int baseSpriteModelId0)
    {
        if (modelDataModelId0 >= 0)
            return modelDataModelId0;

        return baseSpriteModelId0;
    }

    public static bool FindFModelMissingFrames(bool hasBaseSpriteKey) => !hasBaseSpriteKey;

    public static bool SelectModelsArrayIndex(int animId, int modelCount) =>
        animId >= 0 && animId < modelCount;
}
