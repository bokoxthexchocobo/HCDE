using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimHealthPickupArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.PlayerKeyBits is < 0 or > 7))
            throw new InvalidOperationException("Invalid player key ownership.");
        if (state.GeometryHealth is null && state.Actors.Any(a => a.PlayerKeyBits != 0))
            throw new InvalidOperationException("Player key ownership requires a full simulation save.");
        if (state.Actors.Any(a => a.HealthPickups?.Any(i => i.Health == 0 || i.Health < 0 && i.AutoUseMode != 0 || i.Amount <= 0 || i.AutoUseMode is < 0 or > 3 || i.TypeName == "") == true))
            throw new InvalidOperationException("Invalid health pickup stack.");
    }
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        var keys = state.Actors.Any(a => a.PlayerKeyBits != 0);
        if (!keys && !state.Actors.Any(a => a.HealthPickups is { Count: > 0 })) return archive;
        var named = keys || state.Actors.Any(a => a.HealthPickups?.Any(item => item.TypeName is not null) == true);
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4))); writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            if (keys) writer.Write(actor.PlayerKeyBits);
            writer.Write(actor.HealthPickups?.Count ?? 0);
            if (actor.HealthPickups is null) continue;
            foreach (var item in actor.HealthPickups)
            {
                writer.Write(item.Health); writer.Write(item.Amount); writer.Write(item.AutoUseMode);
                if (named)
                {
                    var name = item.TypeName is null ? null : Encoding.GetBytes(item.TypeName);
                    writer.Write(name?.Length ?? -1);
                    if (name is not null) writer.Write(name);
                }
            }
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), keys ? (ushort)164 : named ? (ushort)163 : (ushort)162); return result;
    }
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-health-pickups-invalid";
        if (bytes.Length < 28) return false;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var keys = version == 164;
        var named = version >= 163;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var data = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(data); var count = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        if (prior < 15 || prior > 161 || count < 0 || (long)count * (keys ? 8 : 4) + 8 > data.Length) return false;
        var keyBits = new int[count];
        var lists = new IReadOnlyList<HealthPickupStack>[count]; var offset = 8;
        for (var index = 0; index < count; index++)
        {
            if (keys)
            {
                if (data.Length - offset < 4) return false;
                keyBits[index] = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
                if (keyBits[index] is < 0 or > 7) return false;
            }
            if (data.Length - offset < 4) return false;
            var items = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
            if (items < 0 || (long)items * (named ? 16 : 12) > data.Length - offset) return false;
            var list = new HealthPickupStack[items];
            for (var item = 0; item < items; item++)
            {
                if (data.Length - offset < (named ? 16 : 12)) return false;
                var health = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
                var amount = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 4)..]);
                var mode = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 8)..]); offset += 12;
                if (health == 0 || health < 0 && mode != 0 || amount <= 0 || mode is < 0 or > 3) return false;
                string? name = null;
                if (named)
                {
                    var length = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
                    if (length < -1 || length == 0 || length > data.Length - offset) return false;
                    if (length > 0)
                    {
                        try { name = Encoding.GetString(data.Slice(offset, length)); }
                        catch (DecoderFallbackException) { return false; }
                        offset += length;
                    }
                }
                list[item] = new(health, amount, mode, name);
            }
            lists[index] = list;
        }
        if (offset != data.Length) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-health-pickups-count"; return false; }
        for (var index = 0; index < count; index++)
        {
            state.Actors[index].HealthPickups = lists[index];
            state.Actors[index].PlayerKeyBits = keyBits[index];
        }
        error = null; return true;
    }
}
