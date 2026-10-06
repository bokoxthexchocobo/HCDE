using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimArmorTypeArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        var actorArmor = state.Actors.Any(a => a.ActorArmor.HasValue);
        var spares = actorArmor || state.Actors.Any(a => a.SpareArmor is { Count: > 0 });
        var metadata = spares || state.Actors.Any(a => a.WornArmor.HasValue);
        if (!metadata && !state.Actors.Any(a => a.WornArmorType is not null)) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            var name = Encoding.GetBytes(actor.WornArmorType ?? string.Empty);
            writer.Write(name.Length); writer.Write(name);
            writer.Write(actor.WornArmorAmount);
            if (metadata)
            {
                var armor = actor.WornArmor ?? SimWornArmor.Default;
                writer.Write(armor.Maximum); writer.Write(armor.ActualSaveAmount); writer.Write(armor.SavePercent);
                writer.Write(armor.MaxAbsorb); writer.Write(armor.MaxFullAbsorb); writer.Write(armor.AbsorbCount);
            }
            if (spares)
            {
                writer.Write(actor.SpareArmor?.Count ?? 0);
                foreach (var spare in actor.SpareArmor ?? Array.Empty<SpareArmor>())
                {
                    if (string.IsNullOrEmpty(spare.ArmorType))
                        throw new ArgumentException("Spare armor must have a type name.", nameof(state));
                    var type = Encoding.GetBytes(spare.ArmorType);
                    writer.Write(type.Length); writer.Write(type);
                    writer.Write(spare.SaveAmount); writer.Write(spare.SavePercent);
                    writer.Write(spare.MaxAbsorb); writer.Write(spare.MaxFullAbsorb);
                    writer.Write(spare.IgnoreSkill ? 1 : 0);
                }
            }
            if (actorArmor)
            {
                var armor = actor.ActorArmor ?? default;
                writer.Write(armor.Amount); writer.Write(armor.SavePercent);
                writer.Write(armor.MaxAbsorb); writer.Write(armor.MaxFullAbsorb); writer.Write(armor.AbsorbCount);
            }
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray();
        var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), actorArmor ? (ushort)66 : spares ? (ushort)65 : metadata ? (ushort)64 : (ushort)63);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var actorArmor = version == 66;
        var spares = version >= 65;
        var metadata = version >= 64;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-armor-type-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 15 || prior > 62 || count < 0 || count > (size - 12) / (actorArmor ? 56 : spares ? 36 : metadata ? 32 : 8))
        { error = "save-armor-type-header"; return false; }
        var names = new string?[count]; var amounts = new int[count];
        var armorValues = new SimWornArmor?[count];
        var spareValues = new IReadOnlyList<SpareArmor>?[count];
        var actorValues = new SimActorArmor?[count];
        var cursor = start + 8; var end = bytes.Length - 4;
        for (var i = 0; i < count; i++)
        {
            if (end - cursor < 4) { error = "save-armor-type-name"; return false; }
            var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
            if (length < 0 || length > end - cursor) { error = "save-armor-type-name"; return false; }
            try { names[i] = length == 0 ? null : Encoding.GetString(bytes.Slice(cursor, length)); }
            catch (DecoderFallbackException) { error = "save-armor-type-encoding"; return false; }
            cursor += length;
            if (end - cursor < 4) { error = "save-armor-type-amount"; return false; }
            amounts[i] = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
            if (metadata)
            {
                if (end - cursor < 24) { error = "save-armor-type-metadata"; return false; }
                armorValues[i] = new SimWornArmor(
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 12)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 16)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 20)..]));
                cursor += 24;
            }
            if (spares)
            {
                if (end - cursor < 4) { error = "save-spare-armor-count"; return false; }
                var spareCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
                if (spareCount < 0 || spareCount > (end - cursor) / 24)
                { error = "save-spare-armor-count"; return false; }
                var list = new SpareArmor[spareCount];
                for (var j = 0; j < spareCount; j++)
                {
                    if (end - cursor < 4) { error = "save-spare-armor-name"; return false; }
                    var lengthSpare = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]); cursor += 4;
                    if (lengthSpare <= 0 || lengthSpare > end - cursor - 20)
                    { error = "save-spare-armor-name"; return false; }
                    string type;
                    try { type = Encoding.GetString(bytes.Slice(cursor, lengthSpare)); }
                    catch (DecoderFallbackException) { error = "save-spare-armor-encoding"; return false; }
                    cursor += lengthSpare;
                    var ignore = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 16)..]);
                    if (ignore is not (0 or 1)) { error = "save-spare-armor-flags"; return false; }
                    list[j] = new SpareArmor(BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]),
                        BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]),
                        BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]),
                        BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 12)..]), type, ignore != 0);
                    cursor += 20;
                }
                spareValues[i] = list;
            }
            if (actorArmor)
            {
                if (end - cursor < 20) { error = "save-actor-armor-metadata"; return false; }
                actorValues[i] = new SimActorArmor(BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 12)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 16)..]));
                cursor += 20;
            }
        }
        if (cursor != end) { error = "save-armor-type-size"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-armor-type-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].WornArmorType = names[i];
            state.Actors[i].WornArmorAmount = amounts[i];
            state.Actors[i].WornArmor = armorValues[i];
            state.Actors[i].SpareArmor = spareValues[i];
            state.Actors[i].ActorArmor = actorValues[i] == default(SimActorArmor) ? null : actorValues[i];
        }
        return true;
    }
}
