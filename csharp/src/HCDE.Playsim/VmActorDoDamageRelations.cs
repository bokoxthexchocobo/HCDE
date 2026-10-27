namespace HCDE.Playsim;

/// <summary><c>A_DamageMaster</c>, <c>A_DamageChildren</c>, and sibling loop guards from <c>p_actionfunctions.cpp</c> (4158–4238).</summary>
public static class VmActorDoDamageRelations
{
    public static bool ShouldDamageMaster(object? master) => master is not null;

    public static bool IsChildOfMaster(object? actorMaster, object self) =>
        ReferenceEquals(actorMaster, self);

    public static bool ShouldRunSiblingDamageLoop(object? selfMaster) => selfMaster is not null;

    public static bool SiblingDamageCandidate(bool hasMaster, bool sameMaster, bool notSelf) =>
        hasMaster && sameMaster && notSelf;

    public static bool ShouldDamageChildInIterator(object? actorMaster, object self) =>
        IsChildOfMaster(actorMaster, self);
}
