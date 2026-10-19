using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimAcsArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);
    private const int GlobalCount = 64;
    private const int MapVarCount = 128;
    private const int WorldVarCount = 256;

    internal static void Validate(SimSaveState state)
    {
        if (state.Acs is not { } saved) return;
        if (state.GeometryHealth is null
            || saved.Globals.Length != GlobalCount
            || saved.MapVariables.Length != MapVarCount
            || saved.WorldVariables.Length != WorldVarCount
            || saved.GlobalStrings.Any(entry => entry is null)
            || saved.Fibers.Any(fiber => fiber.Stack is null || fiber.Locals is null
                || fiber.ScriptNumber < 0 || fiber.Pc < 0 || fiber.Wait < 0))
            throw new InvalidOperationException("Invalid saved ACS state.");
        foreach (var fiber in saved.Fibers)
        {
            if (fiber.ActivatorId != 0
                && state.Actors.All(actor => actor.Id != fiber.ActivatorId))
                throw new InvalidOperationException("Saved ACS fiber activator is missing.");
            if (fiber.TriggerLineIndex >= 0 && fiber.TriggerLineIndex >= state.GeometryHealth!.Lines.Count)
                throw new InvalidOperationException("Saved ACS trigger line is out of range.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.Acs is not { } saved) return archive;
        var body = new List<byte>(4096);
        WriteInt32(body, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        WriteInt32(body, saved.RandomState.HasValue ? 1 : 0);
        if (saved.RandomState is ulong random) WriteUInt64(body, random);
        WriteInt32(body, saved.ScriptResult);
        WriteInts(body, saved.Globals);
        WriteInts(body, saved.MapVariables);
        WriteInts(body, saved.WorldVariables);
        WriteSparse(body, saved.WorldArrays);
        WriteSparse(body, saved.GlobalArrays);
        WriteInt32(body, saved.MapArrays.Length);
        foreach (var array in saved.MapArrays)
        {
            WriteInt32(body, array.Length);
            foreach (var value in array) WriteInt32(body, value);
        }
        WriteInt32(body, saved.GlobalStrings.Count);
        foreach (var entry in saved.GlobalStrings) WriteName(body, entry);
        WriteInt32(body, saved.Fibers.Count);
        foreach (var fiber in saved.Fibers) WriteFiber(body, fiber);
        WriteInt32(body, checked(body.Count + 4));
        var result = new byte[checked(archive.Length + body.Count)];
        archive.CopyTo(result, 0);
        body.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 366);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 32) { error = "save-acs-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 16 || size > bytes.Length - 16) { error = "save-acs-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes.Slice(start, size - 4);
        try
        {
            using var reader = new BinaryReader(new MemoryStream(trailer.ToArray()), Encoding, leaveOpen: true);
            var prior = reader.ReadInt32();
            if (prior < 15 || prior > 365) { error = "save-acs-header"; return false; }
            var hasRandom = reader.ReadInt32();
            ulong? random = hasRandom != 0 ? reader.ReadUInt64() : null;
            var scriptResult = reader.ReadInt32();
            var globals = ReadInts(reader, GlobalCount);
            var mapVars = ReadInts(reader, MapVarCount);
            var worldVars = ReadInts(reader, WorldVarCount);
            var worldArrays = ReadSparse(reader);
            var globalArrays = ReadSparse(reader);
            var mapArrayCount = reader.ReadInt32();
            if (mapArrayCount < 0 || mapArrayCount > 4096) { error = "save-acs-maparray"; return false; }
            var mapArrays = new int[mapArrayCount][];
            for (var i = 0; i < mapArrayCount; i++)
            {
                var length = reader.ReadInt32();
                if (length < 0 || length > 1_048_576) { error = "save-acs-maparray"; return false; }
                mapArrays[i] = new int[length];
                for (var j = 0; j < length; j++) mapArrays[i][j] = reader.ReadInt32();
            }
            var stringCount = reader.ReadInt32();
            if (stringCount < 0 || stringCount > 65536) { error = "save-acs-strings"; return false; }
            var strings = new string[stringCount];
            for (var i = 0; i < stringCount; i++)
                if (!ReadName(reader, out strings[i])) { error = "save-acs-strings"; return false; }
            var fiberCount = reader.ReadInt32();
            if (fiberCount < 0 || fiberCount > 1024) { error = "save-acs-fibers"; return false; }
            var fibers = new SimAcsFiberState[fiberCount];
            for (var i = 0; i < fiberCount; i++)
                if (!ReadFiber(reader, out fibers[i])) { error = "save-acs-fiber"; return false; }
            if (reader.BaseStream.Position != reader.BaseStream.Length) { error = "save-acs-size"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
            state.Acs = new SimAcsState
            {
                Globals = globals,
                MapVariables = mapVars,
                WorldVariables = worldVars,
                WorldArrays = worldArrays,
                GlobalArrays = globalArrays,
                MapArrays = mapArrays,
                GlobalStrings = strings,
                Fibers = fibers,
                RandomState = random,
                ScriptResult = scriptResult,
            };
            Validate(state);
            return true;
        }
        catch (EndOfStreamException)
        {
            state = new(); error = "save-acs-size"; return false;
        }
        catch (InvalidOperationException)
        {
            state = new(); error = "save-acs-state"; return false;
        }
    }

    private static void WriteInt32(List<byte> body, int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        body.AddRange(bytes);
    }

    private static void WriteUInt64(List<byte> body, ulong value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        body.AddRange(bytes);
    }

    private static void WriteInts(List<byte> body, int[] values)
    {
        foreach (var value in values) WriteInt32(body, value);
    }

    private static int[] ReadInts(BinaryReader reader, int count)
    {
        var values = new int[count];
        for (var i = 0; i < count; i++) values[i] = reader.ReadInt32();
        return values;
    }

    private static void WriteSparse(List<byte> body, IReadOnlyList<SimSparseIntArray> arrays)
    {
        WriteInt32(body, arrays.Count);
        foreach (var array in arrays)
        {
            WriteInt32(body, array.ArrayId);
            WriteInt32(body, array.Entries.Count);
            foreach (var entry in array.Entries)
            {
                WriteInt32(body, entry.Index);
                WriteInt32(body, entry.Value);
            }
        }
    }

    private static SimSparseIntArray[] ReadSparse(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count < 0 || count > 4096) throw new EndOfStreamException();
        var arrays = new SimSparseIntArray[count];
        for (var i = 0; i < count; i++)
        {
            var arrayId = reader.ReadInt32();
            var entryCount = reader.ReadInt32();
            if (entryCount < 0 || entryCount > 1_048_576) throw new EndOfStreamException();
            var entries = new SimSparseIntEntry[entryCount];
            for (var j = 0; j < entryCount; j++)
                entries[j] = new(reader.ReadInt32(), reader.ReadInt32());
            arrays[i] = new(arrayId, entries);
        }
        return arrays;
    }

    private static void WriteFiber(List<byte> body, SimAcsFiberState fiber)
    {
        WriteInt32(body, fiber.ScriptNumber);
        WriteInt32(body, unchecked((int)fiber.CodeHash));
        var flags = (fiber.Always ? 1 : 0) | (fiber.Suspended ? 2 : 0) | (fiber.LegacyHexenDelay ? 4 : 0) | (fiber.BackSide ? 8 : 0);
        WriteInt32(body, flags);
        WriteInt32(body, unchecked((int)fiber.ActivatorId));
        WriteInt32(body, fiber.TriggerLineIndex);
        WriteInt32(body, fiber.Pc);
        WriteInt32(body, fiber.Wait);
        WriteInt32(body, fiber.ScriptWaitState);
        WriteInt32(body, fiber.ScriptWaitTarget);
        WriteInt32(body, fiber.TagWait);
        WriteInt32(body, fiber.Stack.Count);
        foreach (var value in fiber.Stack) WriteInt32(body, value);
        WriteInt32(body, fiber.Locals.Count);
        foreach (var value in fiber.Locals) WriteInt32(body, value);
    }

    private static bool ReadFiber(BinaryReader reader, out SimAcsFiberState fiber)
    {
        fiber = null!;
        var script = reader.ReadInt32();
        var hash = reader.ReadUInt32();
        var flags = reader.ReadInt32();
        var activator = reader.ReadUInt32();
        var line = reader.ReadInt32();
        var pc = reader.ReadInt32();
        var wait = reader.ReadInt32();
        var scriptWaitState = reader.ReadInt32();
        var scriptWaitTarget = reader.ReadInt32();
        var tagWait = reader.ReadInt32();
        var stackCount = reader.ReadInt32();
        if (stackCount < 0 || stackCount > 1_048_576) return false;
        var stack = new int[stackCount];
        for (var i = 0; i < stackCount; i++) stack[i] = reader.ReadInt32();
        var localCount = reader.ReadInt32();
        if (localCount < 0 || localCount > 1_048_576) return false;
        var locals = new int[localCount];
        for (var i = 0; i < localCount; i++) locals[i] = reader.ReadInt32();
        fiber = new(script, hash, (flags & 1) != 0, (flags & 2) != 0, (flags & 4) != 0, activator, line, (flags & 8) != 0,
            pc, wait, scriptWaitState, scriptWaitTarget, tagWait, stack, locals);
        return true;
    }

    private static void WriteName(List<byte> body, string name)
    {
        var bytes = Encoding.GetBytes(name);
        WriteInt32(body, bytes.Length);
        body.AddRange(bytes);
    }

    private static bool ReadName(BinaryReader reader, out string name)
    {
        name = "";
        var length = reader.ReadInt32();
        if (length < 0 || length > 1_048_576) return false;
        var bytes = reader.ReadBytes(length);
        try { name = Encoding.GetString(bytes); }
        catch (DecoderFallbackException) { return false; }
        return true;
    }
}
