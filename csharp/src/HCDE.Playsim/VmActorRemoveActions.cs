namespace HCDE.Playsim;

/// <summary><c>A_Remove*</c> and telefog action wrappers from <c>p_actionfunctions.cpp</c> (4471–4635).</summary>
public static class VmActorRemoveActions
{
    public static bool ShouldRemoveWhenPresent(object? victim) => victim is not null;

    public static bool ShouldRemoveMaster(object? master) => master is not null;

    public static bool IsRemoveChildOfMaster(object? actorMaster, object self) =>
        ReferenceEquals(actorMaster, self);

    public static bool ShouldRemoveChildInIterator(object? actorMaster, object self, int health, bool removeAll) =>
        IsRemoveChildOfMaster(actorMaster, self) && (health <= 0 || removeAll);

    public static bool ShouldRunRemoveSiblingsLoop(object? selfMaster) => selfMaster is not null;

    public static bool RemoveSiblingsLoop(
        bool hasMaster,
        bool sameMaster,
        bool notSelf,
        int health,
        bool removeAll) =>
        hasMaster && sameMaster && notSelf && (health <= 0 || removeAll);

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
}
