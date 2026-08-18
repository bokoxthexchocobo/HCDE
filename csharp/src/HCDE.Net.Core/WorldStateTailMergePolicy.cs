namespace HCDE.Net.Core;

public static class WorldStateTailMergePolicy
{
    public static bool ShouldMergeCoopIntoInvasion(
        InvasionSnapshotHeader? invasionSnapshot,
        GuestWorldStateStore? store) =>
        invasionSnapshot is not null
            && store is not null
            && WorldStateTailBuilder.HasWorldDeltaPayload(store);
}
