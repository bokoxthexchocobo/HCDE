namespace HCDE.Gamedata;

/// <summary>
/// <c>PClassActor::InitializeDefaults</c> tail, <c>RegisterIDs</c>, and <c>GetReplacement</c> skill gate from <c>info.cpp</c> (**500–649**).
/// Chunk **1037**.
/// </summary>
public static class VmPClassActorInfo1037Slice
{
    public const int GameFilterAny = -1;
    public const int DoomEdNumUnset = -1;
    public const int DoomEdSpecialDecorateMarker = -2;
    public const uint OfTransferrableFlags = 0x00000007;

    public readonly record struct DoomEdRegistryEntry(string TypeName, int Special);

    public readonly record struct SkillReplacementFailure(
        bool Failed,
        bool ClearLookSkill,
        string ClearedReplacee,
        string ClearedReplacedBy);

    public static int ParentDefaultsCopyLength(int parentClassSize, int objectHeaderSize) =>
        Math.Max(0, parentClassSize - objectHeaderSize);

    public static int ExtendedDefaultsZeroLength(int classSize, int parentClassSize) =>
        classSize > parentClassSize ? classSize - parentClassSize : 0;

    public static void CopyParentDefaultsBlock(
        Span<byte> defaults,
        ReadOnlySpan<byte> parentDefaults,
        int objectHeaderSize,
        int parentClassSize)
    {
        var copyLen = ParentDefaultsCopyLength(parentClassSize, objectHeaderSize);
        if (copyLen > 0)
            parentDefaults.Slice(objectHeaderSize, copyLen).CopyTo(defaults.Slice(objectHeaderSize, copyLen));
        var zeroLen = ExtendedDefaultsZeroLength(defaults.Length, parentClassSize);
        if (zeroLen > 0)
            defaults.Slice(parentClassSize, zeroLen).Clear();
    }

    public static uint TransferrableObjectFlags(uint parentObjectFlags) =>
        parentObjectFlags & OfTransferrableFlags;

    public static void ZeroDefaultsTail(Span<byte> defaults, int objectHeaderSize)
    {
        if (defaults.Length > objectHeaderSize)
            defaults.Slice(objectHeaderSize).Clear();
    }

    public static void CopyMetaBlock(Span<byte> meta, ReadOnlySpan<byte> parentMeta, int parentMetaSize, int metaSize)
    {
        if (metaSize == 0)
            return;
        if (parentMetaSize > 0)
        {
            parentMeta.Slice(0, Math.Min(parentMetaSize, metaSize)).CopyTo(meta.Slice(0, Math.Min(parentMetaSize, metaSize)));
            if (metaSize > parentMetaSize)
                meta.Slice(parentMetaSize, metaSize - parentMetaSize).Clear();
        }
        else
        {
            meta.Clear();
        }
    }

    public static bool ShouldRegisterConversationId(int conversationId) => conversationId > 0;

    public static bool ShouldRegisterSpawnOrDoomEd(int actorGameFilter, int currentGameType) =>
        actorGameFilter == GameFilterAny || (actorGameFilter & currentGameType) != 0;

    public static bool ShouldRegisterSpawnId(int spawnId) => spawnId > 0;

    public static bool ShouldRegisterDoomEdNum(int doomEdNum) => doomEdNum != DoomEdNumUnset;

    public static DoomEdRegistryEntry CreateDecorateDoomEdEntry(string typeName) =>
        new(typeName, DoomEdSpecialDecorateMarker);

    public static bool IsDuplicateDecorateDoomEdEntry(DoomEdRegistryEntry? existing) =>
        existing != null && existing.Special == DoomEdSpecialDecorateMarker;

    public static bool HiddenClassRefersToSelf(string registeredType, string definingType) =>
        !registeredType.Equals(definingType, StringComparison.OrdinalIgnoreCase);

    public static bool VerifyClientSideReplacement(bool replaceeClientSide, bool replacementClientSide) =>
        replaceeClientSide == replacementClientSide;

    public static SkillReplacementFailure EvaluateSkillReplacement(
        bool lookSkill,
        int skillCount,
        int currentSkill,
        string typeName,
        string skillReplaceName,
        bool replaceeExists,
        bool clientSideMatch)
    {
        if (!lookSkill || skillCount <= currentSkill || skillReplaceName.Length == 0)
            return new SkillReplacementFailure(false, lookSkill, typeName, skillReplaceName);

        if (!replaceeExists || !clientSideMatch)
        {
            return new SkillReplacementFailure(
                true,
                ClearLookSkill: false,
                typeName,
                skillReplaceName);
        }

        return new SkillReplacementFailure(false, lookSkill, typeName, skillReplaceName);
    }

    public static SkillReplacementFailure ApplySkillReplacementFailureClear(
        SkillReplacementFailure failure,
        string typeName,
        string skillReplaceName)
    {
        if (!failure.Failed)
            return failure;
        return failure with
        {
            ClearLookSkill = true,
            ClearedReplacee = typeName,
            ClearedReplacedBy = skillReplaceName
        };
    }
}
