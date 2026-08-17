using HCDE.MapLoader;

namespace HCDE.Net.Core;

public static class AuthorityMapLoadBootstrap
{
    public static bool TryBootstrapAuthorityWorldState(
        LiveAuthoritySession authority,
        ReadOnlySpan<byte> wad,
        string mapName,
        out string? rejectReason,
        int rngSeed,
        bool replicateSectorMetadata)
    {
        rejectReason = null;
        var store = new GuestWorldStateStore();
        if (!MapLoadBootstrap.TrySeedGuestWorldState(wad, mapName, store, out rejectReason))
            return false;

        var checksum = new SnapshotChecksumSession();
        authority.SetAuthorityWorldState(store, checksum, rngSeed, replicateSectorMetadata);
        return true;
    }
}
