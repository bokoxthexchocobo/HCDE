namespace HCDE.Playsim;

public readonly record struct SimMapPickupIdentity(int ThingIndex, int DoomEdNum, int ThingId, bool JustSpawned, int StatNum);

internal static class SimMapPickupArchive
{
    internal static void Validate(SimSaveState state) => SimMapOriginArchive.Validate(state);
    internal static byte[] Write(SimSaveState state, byte[] archive) => SimMapOriginArchive.Write(state, archive);
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
        => SimMapOriginArchive.TryRead(bytes, out state, out error);
}
