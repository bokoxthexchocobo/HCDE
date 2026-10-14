namespace HCDE.Playsim;

public readonly record struct SimMapDecorationIdentity(int ThingIndex, int DoomEdNum, int ThingId, bool JustSpawned, int StatNum);

internal static class SimMapDecorationArchive
{
    internal static void Validate(SimSaveState state) => SimMapOriginArchive.Validate(state, decoration: true);
    internal static byte[] Write(SimSaveState state, byte[] archive) => SimMapOriginArchive.Write(state, archive, decoration: true);
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        var success = SimMapOriginArchive.TryRead(bytes, out state, out error, decoration: true);
        if (error is not null && error.StartsWith("save-map-pickup-", StringComparison.Ordinal))
            error = "save-map-decoration-" + error[16..];
        return success;
    }
}
