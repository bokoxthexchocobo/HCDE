namespace HCDE.Playsim;

/// <summary>
/// <c>LS_GlassBreak</c> junk loop through <c>LS_SetGlobalFogParameter</c> from <c>p_lnspec.cpp</c> (**3305–3454**); chunk **933**.
/// </summary>
public static class VmPLnspecGlassConversation933Slice
{
    public const int GlassShardIterations = 7;
    public const double GlassSpawnHeightOffset = 24.0;
    public const double GlassYawScale = 360.0 / 256.0;

    public static readonly string[] QuestGrantInventory =
    [
        "QuestItem29",
        "UpgradeAccuracy",
        "UpgradeStamina",
    ];

    public static bool GlassUsesCustomType(int junkId) => junkId > 0;

    public static bool GlassSpawnTypeResolved(object? spawnableType) => spawnableType != null;

    public static int GlassStateOffset(int prGlass, int health) =>
        health <= 0 ? 0 : prGlass % health;

    public static double GlassShardYawDegrees(int prGlass) => prGlass * GlassYawScale;

    public static int GlassVelocityAngleIndex(int prGlass) => prGlass & 3;

    public static int GlassVelocityZ(int prGlass) => prGlass & 7;

    public static bool ShouldGrantQuestItems(bool quest1, bool quest2) => quest1 || quest2;

    public static object? ResolveQuestActivator(object? activator, object? firstInGamePlayerMo) =>
        activator ?? firstInGamePlayerMo;

    public static bool StartConversationHasTarget(object? target) => target != null;

    public static bool StartConversationPlayerEligible(
        object? activator,
        object? player,
        bool moIsActivator,
        int health) =>
        activator != null && player != null && moIsActivator && health > 0;

    public static int ConversationDialogIndex(int dialogId, int resolvedIndex) =>
        dialogId == 0 ? -1 : resolvedIndex;

    public static bool ConversationDialogMissing(int dialogId, int resolvedIndex) =>
        dialogId != 0 && resolvedIndex == -1;

    public static bool ConversationApplyToIterator(int tid) => tid != 0;

    public static float PlaneReflectionScale(int raw) => raw / 255f;

    public enum GlobalFogKind
    {
        Density = 0,
        OutsideDensity = 1,
        SkyFog = 2,
    }

    public static GlobalFogKind? ParseGlobalFogKind(int type) => type switch
    {
        0 => GlobalFogKind.Density,
        1 => GlobalFogKind.OutsideDensity,
        2 => GlobalFogKind.SkyFog,
        _ => null,
    };

    public static int ApplyGlobalFogDensity(int value) => value >> 1;

    public static int ApplyGlobalFogSky(int value) => value;

    public readonly record struct GlassShardInvoke(
        bool SpawnCustom,
        bool ResolvedType,
        double YawDegrees,
        int VelAngleIndex,
        int VelZ,
        int ExtraTics,
        int StateOffset);

    public static GlassShardInvoke BuildGlassShard(int junkId, object? spawnableType, int prGlass, int health)
    {
        var custom = GlassUsesCustomType(junkId);
        var resolved = !custom || GlassSpawnTypeResolved(spawnableType);
        return new(
            custom,
            resolved,
            GlassShardYawDegrees(prGlass),
            GlassVelocityAngleIndex(prGlass),
            GlassVelocityZ(prGlass),
            prGlass,
            custom ? 0 : GlassStateOffset(prGlass, health));
    }

    public readonly record struct QuestGrantInvoke(bool Grant, IReadOnlyList<string> Items);

    public static QuestGrantInvoke BuildQuestGrant(
        bool switched,
        bool noJunkArg,
        bool quest1,
        bool quest2,
        object? activator,
        object? firstPlayerMo)
    {
        if (!switched || noJunkArg)
            return new(false, Array.Empty<string>());
        if (!ShouldGrantQuestItems(quest1, quest2))
            return new(false, Array.Empty<string>());
        var who = ResolveQuestActivator(activator, firstPlayerMo);
        return who == null ? new(false, Array.Empty<string>()) : new(true, QuestGrantInventory);
    }

    public readonly record struct StartConversationInvoke(bool FaceTalker);

    public static StartConversationInvoke? TryStartConversation(
        object? target,
        object? activator,
        object? player,
        bool moIsActivator,
        int health,
        int faceTalkerArg)
    {
        if (!StartConversationHasTarget(target))
            return null;
        if (!StartConversationPlayerEligible(activator, player, moIsActivator, health))
            return null;
        return new(faceTalkerArg != 0);
    }

    public readonly record struct ConversationAssignInvoke(int DialogIndex, bool HasNode);

    public static ConversationAssignInvoke? BuildConversationAssign(int dialogId, int resolvedIndex)
    {
        if (ConversationDialogMissing(dialogId, resolvedIndex))
            return null;
        var index = ConversationDialogIndex(dialogId, resolvedIndex);
        return new(index, index >= 0);
    }

    public readonly record struct PlaneReflectionInvoke(float Floor, float Ceiling, bool ApplyFloor, bool ApplyCeiling);

    public static PlaneReflectionInvoke BuildPlaneReflection(
        int floorArg,
        int ceilingArg,
        bool floorSlope,
        bool ceilingSlope) =>
        new(
            PlaneReflectionScale(floorArg),
            PlaneReflectionScale(ceilingArg),
            !floorSlope,
            !ceilingSlope);

    public readonly record struct GlobalFogInvoke(GlobalFogKind Kind, int Value);

    public static GlobalFogInvoke? BuildGlobalFog(int type, int value)
    {
        var kind = ParseGlobalFogKind(type);
        if (kind is null)
            return null;
        var applied = kind switch
        {
            GlobalFogKind.SkyFog => ApplyGlobalFogSky(value),
            _ => ApplyGlobalFogDensity(value),
        };
        return new(kind.Value, applied);
    }
}
