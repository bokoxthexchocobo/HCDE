using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimDeathTypeArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        var strifeFlags = state.Actors.Any(actor => actor.StrifeDamage);
        var infightFlags = strifeFlags || state.Actors.Any(actor => actor.NoInfightSpecies);
        var pierceFlags = infightFlags || state.Actors.Any(actor => actor.PierceArmor);
        var foilFlags = pierceFlags || state.Actors.Any(actor => actor.FoilInvul);
        var fireFlags = foilFlags || state.Actors.Any(actor => actor.SpecialFireDamage);
        var damageTypes = state.Actors.Any(actor => !string.IsNullOrEmpty(actor.DamageType)
            && !string.Equals(actor.DamageType, "None", StringComparison.OrdinalIgnoreCase)
            && !(actor.DeathFlags == 1 && string.Equals(actor.DamageType, "Massacre", StringComparison.OrdinalIgnoreCase)));
        if (!state.Actors.Any(actor => !string.IsNullOrEmpty(actor.DeathType)
            && !string.Equals(actor.DeathType, "None", StringComparison.OrdinalIgnoreCase)) && !damageTypes && !fireFlags) return archive;
        damageTypes |= fireFlags;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            foreach (var value in damageTypes ? new[] { actor.DeathType, actor.DamageType } : new[] { actor.DeathType })
            {
                var name = Encoding.GetBytes(value ?? string.Empty);
                writer.Write(name.Length);
                writer.Write(name);
            }
            if (fireFlags) writer.Write((actor.SpecialFireDamage ? 1 : 0) | (actor.FoilInvul ? 2 : 0) | (actor.PierceArmor ? 4 : 0) | (actor.NoInfightSpecies ? 8 : 0) | (actor.StrifeDamage ? 16 : 0));
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray();
        var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0);
        trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), strifeFlags ? (ushort)54 : infightFlags ? (ushort)53 : pierceFlags ? (ushort)50 : foilFlags ? (ushort)49 : fireFlags ? (ushort)48 : damageTypes ? (ushort)47 : (ushort)46);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var fireFlags = version >= 48;
        var fields = version >= 47 ? 2 : 1;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16)
        { error = "save-death-type-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 15 || prior > 45 || count < 0 || count > (size - 12) / (4 * fields + (fireFlags ? 4 : 0)))
        { error = "save-death-type-header"; return false; }
        var names = new string?[count * fields];
        var specialFire = new bool[count];
        var foilInvul = new bool[count];
        var pierceArmor = new bool[count];
        var noInfightSpecies = new bool[count];
        var strifeDamage = new bool[count];
        var cursor = start + 8;
        var end = bytes.Length - 4;
        for (var i = 0; i < names.Length; i++)
        {
            if (end - cursor < 4)
            { error = "save-death-type-name"; return false; }
            var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]);
            cursor += 4;
            if (length < 0 || length > end - cursor)
            { error = "save-death-type-name"; return false; }
            try { names[i] = length == 0 ? null : Encoding.GetString(bytes.Slice(cursor, length)); }
            catch (DecoderFallbackException) { error = "save-death-type-encoding"; return false; }
            cursor += length;
            if (fireFlags && i % fields == fields - 1)
            {
                if (end - cursor < 4) { error = "save-special-fire-size"; return false; }
                var flag = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]);
                if (flag < 0 || flag > (version == 54 ? 31 : version == 53 ? 15 : version == 50 ? 7 : version == 49 ? 3 : 1)) { error = "save-special-fire-flags"; return false; }
                specialFire[i / fields] = (flag & 1) != 0;
                foilInvul[i / fields] = (flag & 2) != 0;
                pierceArmor[i / fields] = (flag & 4) != 0;
                noInfightSpecies[i / fields] = (flag & 8) != 0;
                strifeDamage[i / fields] = (flag & 16) != 0;
                cursor += 4;
            }
        }
        if (cursor != end)
        { error = "save-death-type-size"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count)
        { state = new(); error = "save-death-type-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].DeathType = names[i * fields];
            if (fields == 2) state.Actors[i].DamageType = names[i * fields + 1];
            state.Actors[i].SpecialFireDamage = specialFire[i];
            state.Actors[i].FoilInvul = foilInvul[i];
            state.Actors[i].PierceArmor = pierceArmor[i];
            state.Actors[i].NoInfightSpecies = noInfightSpecies[i];
            state.Actors[i].StrifeDamage = strifeDamage[i];
        }
        return true;
    }
}
