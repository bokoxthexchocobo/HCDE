using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimActorDamageFactorArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.DamageFactor != 65536 || a.DamageMultiplier != 65536 || a.DamageFactors is { Count: > 0 })) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            writer.Write(actor.DamageFactor); writer.Write(actor.DamageMultiplier);
            writer.Write(actor.DamageFactors?.Count ?? 0);
            if (actor.DamageFactors is not { } factors) continue;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in factors.OrderBy(p => p.Key.ToUpperInvariant(), StringComparer.Ordinal))
            {
                if (string.IsNullOrEmpty(entry.Key) || !double.IsFinite(entry.Value) || !names.Add(entry.Key))
                    throw new ArgumentException("Invalid actor damage factor.", nameof(state));
                var name = Encoding.GetBytes(entry.Key);
                writer.Write(name.Length); writer.Write(name); writer.Write(entry.Value);
            }
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray();
        var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 56);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-actor-factor-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 15 || prior > 55 || count < 0 || count > (size - 12) / 12)
        { error = "save-actor-factor-header"; return false; }
        var scalars = new (int Factor, int Multiplier)[count];
        var tables = new Dictionary<string, double>[count];
        var cursor = start + 8; var end = bytes.Length - 4;
        for (var i = 0; i < count; i++)
        {
            if (end - cursor < 12) { error = "save-actor-factor-size"; return false; }
            scalars[i] = (BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]), BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]));
            var entries = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]); cursor += 12;
            if (entries < 0 || entries > (end - cursor) / 13) { error = "save-actor-factor-entries"; return false; }
            var table = tables[i] = new(StringComparer.OrdinalIgnoreCase);
            for (var j = 0; j < entries; j++)
            {
                if (end - cursor < 4) { error = "save-actor-factor-name"; return false; }
                var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
                if (length <= 0 || length > end - cursor - 8) { error = "save-actor-factor-name"; return false; }
                string name;
                try { name = Encoding.GetString(bytes.Slice(cursor, length)); }
                catch (DecoderFallbackException) { error = "save-actor-factor-encoding"; return false; }
                cursor += length;
                var factor = BinaryPrimitives.ReadDoubleLittleEndian(bytes[cursor..]); cursor += 8;
                if (!double.IsFinite(factor) || !table.TryAdd(name, factor)) { error = "save-actor-factor-value"; return false; }
            }
        }
        if (cursor != end) { error = "save-actor-factor-size"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-actor-factor-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].DamageFactor = scalars[i].Factor;
            state.Actors[i].DamageMultiplier = scalars[i].Multiplier;
            state.Actors[i].DamageFactors = tables[i];
        }
        return true;
    }
}
