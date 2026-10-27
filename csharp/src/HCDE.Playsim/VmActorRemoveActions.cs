namespace HCDE.Playsim;

/// <summary><c>A_Remove*</c> and telefog action wrappers from <c>p_actionfunctions.cpp</c> (4466–4635).</summary>
public static class VmActorRemoveActions
{
    [Flags]
    public enum RmvfFlags
    {
        Missiles = 1 << 0,
        NoMonsters = 1 << 1,
        Misc = 1 << 2,
        Everything = 1 << 3,
        ExFilter = 1 << 4,
        ExSpecies = 1 << 5,
        Either = 1 << 6,
    }

    public static bool ShouldRemoveWhenPresent(object? victim) => victim is not null;

    public static bool ShouldRemoveMaster(object? master) => master is not null;

    public static bool ShouldRemoveTarget(object? target) => ShouldRemoveWhenPresent(target);

    public static bool ShouldRemoveTracer(object? tracer) => ShouldRemoveWhenPresent(tracer);

    public static bool IsRemoveChildOfMaster(object? actorMaster, object self) =>
        ReferenceEquals(actorMaster, self);

    public static bool ChildHealthAllowsRemove(int health, bool removeAll) => health <= 0 || removeAll;

    public static bool ShouldRemoveChildInIterator(object? actorMaster, object self, int health, bool removeAll) =>
        IsRemoveChildOfMaster(actorMaster, self) && ChildHealthAllowsRemove(health, removeAll);

    public static bool ShouldRunRemoveChildrenLoop(object? nextActor) => nextActor is not null;

    public static bool ShouldRunRemoveSiblingsLoop(object? selfMaster) => selfMaster is not null;

    public static bool RemoveSiblingsLoop(
        bool hasMaster,
        bool sameMaster,
        bool notSelf,
        int health,
        bool removeAll) =>
        hasMaster && sameMaster && notSelf && ChildHealthAllowsRemove(health, removeAll);

    public static bool ShouldRemoveSiblingCandidate(
        object? candidateMaster,
        object? selfMaster,
        object candidate,
        object self,
        int health,
        bool removeAll) =>
        RemoveSiblingsLoop(
            selfMaster is not null,
            ReferenceEquals(candidateMaster, selfMaster),
            !ReferenceEquals(candidate, self),
            health,
            removeAll);

    public static bool ShouldRemoveReference(object? reference) => reference is not null;

    public static (object? SourceType, object? DestType) AssignTeleFogTypes(object? sourceType, object? destType) =>
        (sourceType, destType);

    public static bool ShouldSwapTeleFog(object? sourceType, object? destType) =>
        !Equals(sourceType, destType);

    public static (object? SourceType, object? DestType) SwapTeleFogWhenDistinct(
        object? sourceType,
        object? destType)
    {
        if (!ShouldSwapTeleFog(sourceType, destType))
            return (sourceType, destType);
        return (destType, sourceType);
    }

    public static (object? Source, object? Dest) ApplySwappedTeleFogFields(
        object? sourceType,
        object? destType)
    {
        var swapped = SwapTeleFogWhenDistinct(sourceType, destType);
        return (swapped.SourceType, swapped.DestType);
    }
}
