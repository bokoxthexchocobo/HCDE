using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimDamageTypeArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static bool HasCustomDefinitions(SimSaveState state) => state.DamageTypes is { } definitions
        && (state.IncludesDamageTypeDefinitions || definitions.Count != 1 || !definitions.TryGetValue("Drowning", out var drowning)
            || drowning != new DamageTypeFactor(1, false, true));

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!HasCustomDefinitions(state)) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.DamageTypes!.Count);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in state.DamageTypes.OrderBy(p => p.Key.ToUpperInvariant(), StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(entry.Key) || !double.IsFinite(entry.Value.Factor) || !names.Add(entry.Key))
                throw new ArgumentException("Invalid damage definition.", nameof(state));
            var name = Encoding.GetBytes(entry.Key);
            writer.Write(name.Length); writer.Write(name); writer.Write(entry.Value.Factor);
            writer.Write((entry.Value.ReplaceFactor ? 1 : 0) | (entry.Value.NoArmor ? 2 : 0));
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray();
        var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 57);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-damage-type-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 15 || prior > 56 || count < 0 || count > (size - 12) / 17)
        { error = "save-damage-type-header"; return false; }
        var definitions = new Dictionary<string, DamageTypeFactor>(StringComparer.OrdinalIgnoreCase);
        var cursor = start + 8; var end = bytes.Length - 4;
        for (var i = 0; i < count; i++)
        {
            if (end - cursor < 4) { error = "save-damage-type-name"; return false; }
            var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
            if (length <= 0 || length > end - cursor - 12) { error = "save-damage-type-name"; return false; }
            string name;
            try { name = Encoding.GetString(bytes.Slice(cursor, length)); }
            catch (DecoderFallbackException) { error = "save-damage-type-encoding"; return false; }
            cursor += length;
            var factor = BinaryPrimitives.ReadDoubleLittleEndian(bytes[cursor..]); cursor += 8;
            var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
            if (!double.IsFinite(factor) || flags < 0 || flags > 3
                || !definitions.TryAdd(name, new(factor, (flags & 1) != 0, (flags & 2) != 0)))
            { error = "save-damage-type-value"; return false; }
        }
        if (cursor != end) { error = "save-damage-type-size"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.DamageTypes = definitions;
        state.IncludesDamageTypeDefinitions = true;
        return true;
    }
}
