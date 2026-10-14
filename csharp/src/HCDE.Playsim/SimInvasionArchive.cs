using System.Buffers.Binary;

namespace HCDE.Playsim;

public sealed record SimInvasionState(bool Enabled, InvasionPhase Phase, int Wave, int Cooldown,
    int Spawned, int ActiveMonsters, int MaxWaves, int CountdownTics, int IntermissionTics,
    int SpawnSpotCount, IReadOnlyList<uint> WaveActors);

internal static class SimInvasionArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Invasion is not { } saved) return;
        if (!state.IncludesBotIdentities || state.GeometryHealth is null || !Enum.IsDefined(saved.Phase)
            || saved.Wave < 0 || saved.Cooldown < 0 || saved.Spawned < 0 || saved.ActiveMonsters < 0
            || saved.MaxWaves < 0 || saved.CountdownTics < 0 || saved.IntermissionTics < 0 || saved.SpawnSpotCount < 0
            || saved.WaveActors is null || saved.Spawned != saved.WaveActors.Count || saved.ActiveMonsters > saved.Spawned
            || saved.WaveActors.Any(id => id is 0 or uint.MaxValue || state.NextActorId is { } next && id >= next)
            || saved.WaveActors.Distinct().Count() != saved.WaveActors.Count)
            throw new InvalidOperationException("Invalid saved invasion state.");
        if (saved.MaxWaves > 0 && saved.Wave > saved.MaxWaves
            || saved.Phase is InvasionPhase.Waiting or InvasionPhase.Countdown
                && (saved.Wave != 0 || saved.Spawned != 0 || saved.SpawnSpotCount != 0)
            || saved.Phase is InvasionPhase.Wave or InvasionPhase.Intermission or InvasionPhase.Victory && saved.Wave == 0
            || saved.Phase is InvasionPhase.Waiting or InvasionPhase.Wave && saved.Cooldown != 0
            || saved.Phase == InvasionPhase.Countdown && (saved.Cooldown == 0 || saved.Cooldown > saved.CountdownTics)
            || saved.Phase == InvasionPhase.Intermission && (saved.ActiveMonsters != 0 || saved.Cooldown > saved.IntermissionTics)
            || saved.Phase == InvasionPhase.Victory && (saved.ActiveMonsters != 0 || saved.Cooldown != 0
                || saved.MaxWaves == 0 || saved.Wave != saved.MaxWaves))
            throw new InvalidOperationException("Saved invasion phase conflicts with its counters.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.Invasion is not { } saved) return archive;
        var size = checked(52 + saved.WaveActors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        var fields = new[] { BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)), saved.Enabled ? 1 : 0,
            (int)saved.Phase, saved.Wave, saved.Cooldown, saved.Spawned, saved.ActiveMonsters,
            saved.MaxWaves, saved.CountdownTics, saved.IntermissionTics, saved.SpawnSpotCount, saved.WaveActors.Count };
        for (var i = 0; i < fields.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(trailer[(i * 4)..], fields[i]);
        for (var i = 0; i < saved.WaveActors.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(trailer[(48 + i * 4)..], saved.WaveActors[i]);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 215);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 68) { error = "save-invasion-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 52 || size > bytes.Length - 16) { error = "save-invasion-size"; return false; }
        var trailer = bytes[^size..]; var fields = new int[12];
        for (var i = 0; i < fields.Length; i++) fields[i] = BinaryPrimitives.ReadInt32LittleEndian(trailer[(i * 4)..]);
        if (fields[0] is < 199 or > 214 || fields[11] < 0 || (long)fields[11] * 4 + 52 != size)
        { error = "save-invasion-header"; return false; }
        if (fields[1] is not (0 or 1)) { error = "save-invasion-value"; return false; }
        var members = new uint[fields[11]];
        for (var i = 0; i < members.Length; i++) members[i] = BinaryPrimitives.ReadUInt32LittleEndian(trailer[(48 + i * 4)..]);
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)fields[0]);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.Invasion = new(fields[1] != 0, (InvasionPhase)fields[2], fields[3], fields[4], fields[5], fields[6],
            fields[7], fields[8], fields[9], fields[10], members);
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-invasion-state"; return false; }
        return true;
    }
}
