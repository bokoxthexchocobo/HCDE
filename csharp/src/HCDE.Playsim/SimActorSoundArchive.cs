using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimActorSoundArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static void Validate(SimSaveState state)
    {
        foreach (var actor in state.Actors)
            foreach (var (type, name) in actor.ActorSounds)
                if (!Enum.IsDefined(type) || name is null)
                    throw new InvalidOperationException("Invalid saved actor sound.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.ActorSounds.Count != 0)) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            writer.Write(actor.ActorSounds.Count);
            foreach (var (type, name) in actor.ActorSounds.OrderBy(pair => pair.Key))
            {
                var bytes = Encoding.GetBytes(name);
                writer.Write((int)type); writer.Write(bytes.Length); writer.Write(bytes);
            }
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 122);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-actor-sounds-invalid";
        if (bytes.Length < 32) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 121 || count < 0 || (long)count * 4 + 8 > trailer.Length) return false;
        var records = new Dictionary<ActorSoundType, string>[count]; var offset = 8;
        for (var index = 0; index < count; index++)
        {
            if (trailer.Length - offset < 4) return false;
            var entries = BinaryPrimitives.ReadInt32LittleEndian(trailer[offset..]); offset += 4;
            if (entries < 0 || entries > 11) return false;
            var sounds = new Dictionary<ActorSoundType, string>();
            for (var entry = 0; entry < entries; entry++)
            {
                if (trailer.Length - offset < 8) return false;
                var type = (ActorSoundType)BinaryPrimitives.ReadInt32LittleEndian(trailer[offset..]);
                var length = BinaryPrimitives.ReadInt32LittleEndian(trailer[(offset + 4)..]); offset += 8;
                if (!Enum.IsDefined(type) || length < 0 || length > trailer.Length - offset) return false;
                string name;
                try { name = Encoding.GetString(trailer.Slice(offset, length)); }
                catch (DecoderFallbackException) { return false; }
                if (!sounds.TryAdd(type, name)) return false;
                offset += length;
            }
            records[index] = sounds;
        }
        if (offset != trailer.Length) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-actor-sounds-count"; return false; }
        for (var index = 0; index < count; index++) state.Actors[index].ActorSounds = records[index];
        error = null; return true;
    }
}
