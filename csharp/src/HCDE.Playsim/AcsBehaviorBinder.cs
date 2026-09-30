using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Binds one old word-format BEHAVIOR lump into an <see cref="AcsVm"/>.
/// Enhanced, compact, and redesigned modules are rejected. This is not library loading or <c>p_acs.cpp</c>.
/// </summary>
public static class AcsBehaviorBinder
{
    public const int ScriptClosed = 0;
    public const int ScriptOpen = 1;
    public const int ScriptRespawn = 2;
    public const int ScriptDeath = 3;
    public const int ScriptEnter = 4;
    private const int LocalSize = 20;
    private const uint AcsEnhancedTag = 0x45534341; // "ACSE"
    private const uint AcsLittleEnhancedTag = 0x65534341; // "ACSe"

    /// <summary>
    /// Registers every script, then queues OPEN scripts in script-number order.
    /// ENTER numbers are returned in that same order and are not started here:
    /// native starts them per player, with ACS_ALWAYS, after the map's OPEN queue.
    /// The VM is left unchanged when binding fails before registration.
    /// </summary>
    public static bool TryBind(AcsVm vm, ReadOnlySpan<byte> behavior, out string? error) =>
        TryBind(vm, behavior, out AcsStartupCatalog _, out error);

    /// <inheritdoc cref="TryBind(AcsVm, ReadOnlySpan{byte}, out string?)"/>
    public static bool TryBind(AcsVm vm, ReadOnlySpan<byte> behavior, out int[] enterScripts, out string? error)
    {
        var bound = TryBind(vm, behavior, out AcsStartupCatalog catalog, out error);
        enterScripts = catalog.Enter;
        return bound;
    }

    /// <inheritdoc cref="TryBind(AcsVm, ReadOnlySpan{byte}, out string?)"/>
    public static bool TryBind(AcsVm vm, ReadOnlySpan<byte> behavior, out AcsStartupCatalog catalog, out string? error)
    {
        catalog = AcsStartupCatalog.Empty;
        ArgumentNullException.ThrowIfNull(vm);
        error = null;
        if (vm.ProgramCount != 0) { error = "acs-vm-not-empty"; return false; }
        if (!MapBehaviorCodec.TryProbe(behavior, out var record, out error))
            return false;
        if (record.Format != MapBehaviorFormat.AcsOld || IsRedesignedOldHeader(record))
        {
            error = "acs-map-format-unsupported";
            return false;
        }

        if (!MapBehaviorDirectoryCodec.TryReadScripts(record.Data, record.Format, record.DirectoryOffset, out var scripts, out error))
            return false;

        if (!MapBehaviorStringTableCodec.TryRead(record.Data, record.Format, record.DirectoryOffset, out var stringTable, out error))
            return false;

        var prepared = new List<AcsProgram>(scripts.Count);
        var openScripts = new List<int>();
        var enter = new List<int>();
        var respawn = new List<int>();
        var death = new List<int>();
        var seen = new HashSet<int>();
        var addresses = new HashSet<uint>();
        for (var i = 0; i < scripts.Count; i++)
        {
            var script = scripts[i];
            if (script.Number is < 0 or > 999) { error = "acs-script-number-invalid"; return false; }
            if (!seen.Add(script.Number)) { error = "acs-duplicate-script"; return false; }
            if (!addresses.Add(script.Address)) { error = "acs-script-address-overlap"; return false; }
            if (!IsKnownType(script.Type)) { error = "acs-script-type-unsupported"; return false; }
            if (script.ArgCount < 0 || script.ArgCount > LocalSize) { error = "acs-script-arguments-unsupported"; return false; }
            if (script.Address > int.MaxValue) { error = "acs-script-address-unsupported"; return false; }
            if (!TryScriptSpan(record, scripts, script.Address, out var code, out error))
                return false;

            prepared.Add(new AcsProgram
            {
                Number = script.Number,
                Code = code,
                CodeBaseOffset = (int)script.Address,
                ArgumentCount = script.ArgCount,
                StringTable = stringTable,
            });
            if (script.Type == ScriptOpen)
                openScripts.Add(script.Number);
            else if (script.Type == ScriptEnter)
                enter.Add(script.Number);
            else if (script.Type == ScriptRespawn)
                respawn.Add(script.Number);
            else if (script.Type == ScriptDeath)
                death.Add(script.Number);
        }

        foreach (var program in prepared)
            vm.Add(program);
        // Native map load queues SCRIPT_Open and runs it on a later tic (maploader/specials.cpp).
        // Locals stay zero. The extra Hexen delay tick applies only to GAME_Hexen, so it stays off here.
        foreach (var number in openScripts.OrderBy(number => number))
        {
            if (!vm.Enqueue(number)) { error = "acs-open-script-start-failed"; return false; }
        }

        catalog = new AcsStartupCatalog(
            enter.OrderBy(number => number).ToArray(),
            respawn.OrderBy(number => number).ToArray(),
            death.OrderBy(number => number).ToArray());
        return true;
    }

    private static bool IsRedesignedOldHeader(MapBehaviorRecord record)
    {
        // Same 24-byte minimum as FBehavior's ACS\0 to ACSE/ACSe upgrade.
        if (record.DirectoryOffset < 24 || record.DirectoryOffset > (uint)record.Data.Length)
            return false;
        var pretag = BinaryPrimitives.ReadUInt32LittleEndian(record.Data.AsSpan((int)record.DirectoryOffset - 4, 4));
        return pretag is AcsEnhancedTag or AcsLittleEnhancedTag;
    }

    // p_acs.h SCRIPT_* values. Types 9-11 are unnamed and rejected.
    private static bool IsKnownType(int type) => type is ScriptClosed or ScriptOpen or 2 or 3 or 4 or 5 or 6 or 7 or 8
        or 12 or 13 or 14 or 15 or 16 or 17 or 18;

    private static bool TryScriptSpan(
        MapBehaviorRecord record,
        IReadOnlyList<MapBehaviorScriptEntry> scripts,
        uint address,
        out byte[] code,
        out string? error)
    {
        code = Array.Empty<byte>();
        error = null;
        var end = (uint)record.Data.Length;
        foreach (var script in scripts)
        {
            if (script.Address > address && script.Address < end)
                end = script.Address;
        }

        // Code compiled ahead of the directory must not treat the directory itself as bytecode.
        if (record.DirectoryOffset > address && record.DirectoryOffset < end)
            end = record.DirectoryOffset;
        if (end > int.MaxValue) { error = "acs-script-address-unsupported"; return false; }
        if (!MapBehaviorBytecodeWalker.TryWalkWordSpan(record.Data, (int)address, (int)end, out var instructions, out error))
            return false;
        if (!instructions.Any(instruction => instruction.Opcode is (int)AcsPcode.Terminate or (int)AcsPcode.Suspend))
        {
            error = "acs-script-unterminated";
            return false;
        }

        code = record.Data.AsSpan((int)address, (int)(end - address)).ToArray();
        return true;
    }
}

/// <summary>
/// Script numbers collected at map bind. OPEN is already queued. ENTER, RESPAWN, and DEATH
/// are returned in script-number order and started later, once per player, with ACS_ALWAYS.
/// </summary>
public readonly record struct AcsStartupCatalog(int[] Enter, int[] Respawn, int[] Death)
{
    public static AcsStartupCatalog Empty { get; } = new([], [], []);
}
