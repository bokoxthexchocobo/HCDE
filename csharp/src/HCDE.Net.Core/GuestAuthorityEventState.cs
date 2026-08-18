namespace HCDE.Net.Core;

public sealed class GuestAuthorityEventState : IAuthorityEventSink
{
    public int RoutedCount { get; private set; }

    public uint LastActorId { get; private set; }

    public bool TryApplyInvasionSpawn(AuthorityEventRecord record)
    {
        RoutedCount++;
        LastActorId = record.ActorId;
        return true;
    }

    public bool TryApplyPickupSpawn(AuthorityEventRecord record) => false;

    public bool TryApplyInvasionDespawn(AuthorityEventRecord record) => false;

    public bool TryApplyPickupRetire(AuthorityEventRecord record) => false;

    public bool TryApplyInvasionDamage(AuthorityEventRecord record) => false;

    public bool TryApplyCoopProjectileSpawn(AuthorityEventRecord record) => false;

    public bool TryApplyCoopProjectileRetire(AuthorityEventRecord record) => false;

    public bool TryApplyCoopCosmeticSpawn(AuthorityEventRecord record) => false;
}
