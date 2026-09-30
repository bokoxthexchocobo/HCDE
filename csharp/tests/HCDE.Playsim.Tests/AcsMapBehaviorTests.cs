using System.Buffers.Binary;
using System.Text;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMapBehaviorTests
{
    [Fact]
    public void OpenScriptFromBehaviorChangesSectorLight()
    {
        var wad = HexenMap(OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(35))));
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(35, sim!.LightOf(0));
        Assert.Equal(1, sim.Acs.ProgramCount);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var again, out error), error);
        Assert.Equal(sim.Checksum, again!.Checksum);
    }

    [Fact]
    public void OpenScriptsRunInNumberOrderRatherThanDirectoryOrder()
    {
        var wad = HexenMap(OldBehavior(
            (2, AcsBehaviorBinder.ScriptOpen, 0, LightTo(20)),
            (1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(50))));
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(20, sim!.LightOf(0));
    }

    [Fact]
    public void EnterScriptRunsAfterOpenAndOncePerPlayer()
    {
        var wad = HexenMap(OldBehavior(
            (2, AcsBehaviorBinder.ScriptOpen, 0, LightTo(50)),
            (1, AcsBehaviorBinder.ScriptEnter, 0, LightTo(20))), PlayerStart(1));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var sim = AuthoritySimulation.Start(level!);
        Assert.Single(sim.Players);
        Assert.Equal(2, sim.Acs.RunningCount);
        sim.Tick();
        Assert.Equal(20, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);

        var both = HexenMap(OldBehavior((4, AcsBehaviorBinder.ScriptEnter, 0, LightTo(35))),
            PlayerStart(1).Concat(PlayerStart(2)).ToArray());
        Assert.True(LevelBuilder.TryFromWad(both, "MAP01", out level, out error), error);
        sim = AuthoritySimulation.Start(level!);
        Assert.Equal(2, sim.Acs.RunningCount);
        sim.Tick();
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void DeathScriptRunsOnTheKillingTicAndRespawnScriptRunsWhenRevived()
    {
        var wad = HexenMap(OldBehavior(
            (1, AcsBehaviorBinder.ScriptDeath, 0, LightTo(20)),
            (2, AcsBehaviorBinder.ScriptRespawn, 0, LightTo(35))),
            PlayerStart(1, 96, 0x400));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var sim = AuthoritySimulation.Start(level!, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch));
        var player = Assert.Single(sim.Players);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(160, sim.LightOf(0));

        ActorDamage.Apply(player, 1000);
        Assert.Equal(1, sim.Acs.RunningCount);
        sim.Tick();
        Assert.Equal(20, sim.LightOf(0));
        Assert.True(player.IsDead);

        player.X = Fixed.FromDouble(0);
        for (var i = 0; i < 34; i++) sim.Tick();
        Assert.True(player.IsDead);
        Assert.Equal(0, player.X.ToDouble());
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(96, player.X.ToDouble());
        Assert.Equal(100, player.Health);
        Assert.Equal(35, sim.LightOf(0));
        Assert.False(sim.ReloadRequested);
    }

    [Fact]
    public void ClosedAndEnterScriptsAreRegisteredWithoutRunning()
    {
        var wad = HexenMap(OldBehavior(
            (1, AcsBehaviorBinder.ScriptClosed, 0, LightTo(35)),
            (4, 4, 0, LightTo(90))));
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(160, sim!.LightOf(0));
        Assert.Equal(2, sim.Acs.ProgramCount);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void LoadedBranchUsesTheBehaviorModuleAddress()
    {
        // One script: skip a light-90 terminator and land on light-35. The target is a module offset, not a slice offset.
        const int scriptAddress = 40;
        var wad = HexenMap(OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 0,
            [3, 0, 79, scriptAddress + 36, 10, 112, 1, 90, 1, 10, 112, 1, 35, 1])));
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(35, sim!.LightOf(0));
    }

    [Fact]
    public void WideScriptTypeAndArgumentCountAreNotTruncatedIntoKnownValues()
    {
        var wideType = OldBehavior((1, 257, 0, LightTo(35)));
        Assert.False(HeadlessMapBoot.TryBoot(HexenMap(wideType), "MAP01", out var sim, out var error));
        Assert.Null(sim);
        Assert.Equal("acs-script-type-unsupported", error);

        var wideArgs = OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 256, LightTo(35)));
        var vm = new AcsVm();
        Assert.False(AcsBehaviorBinder.TryBind(vm, wideArgs, out error));
        Assert.Equal("acs-script-arguments-unsupported", error);
        Assert.Equal(0, vm.ProgramCount);
    }

    [Fact]
    public void EmptyScriptDirectoryBootsWithoutPrograms()
    {
        var wad = HexenMap(OldBehavior());
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(160, sim!.LightOf(0));
        Assert.Equal(0, sim.Acs.ProgramCount);
    }

    [Fact]
    public void RejectedBehaviorDoesNotRegisterEarlierScripts()
    {
        var lump = OldBehavior(
            (1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(35)),
            (1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(90)));
        var vm = new AcsVm();
        Assert.False(AcsBehaviorBinder.TryBind(vm, lump, out var error));
        Assert.Equal("acs-duplicate-script", error);
        Assert.Equal(0, vm.ProgramCount);

        lump = OldBehavior((1, 9, 0, LightTo(35)));
        Assert.False(AcsBehaviorBinder.TryBind(vm, lump, out error));
        Assert.Equal("acs-script-type-unsupported", error);
        Assert.Equal(0, vm.ProgramCount);

        Assert.False(HeadlessMapBoot.TryBoot(HexenMap(lump), "MAP01", out var sim, out error));
        Assert.Null(sim);
        Assert.Equal("acs-script-type-unsupported", error);
    }

    [Theory]
    [InlineData("enhanced")]
    [InlineData("redesigned")]
    [InlineData("unterminated")]
    [InlineData("arguments")]
    [InlineData("empty-lump")]
    public void UnsupportedBehaviorFailsBoot(string kind)
    {
        var behavior = kind switch
        {
            "enhanced" => EnhancedMagic(OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(35)))),
            "redesigned" => Redesigned(OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 0, LightTo(35)))),
            "unterminated" => OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 0, [3, 1])),
            "arguments" => OldBehavior((1, AcsBehaviorBinder.ScriptOpen, 21, LightTo(35))),
            "empty-lump" => Array.Empty<byte>(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        Assert.False(HeadlessMapBoot.TryBoot(HexenMap(behavior), "MAP01", out var sim, out var error));
        Assert.Null(sim);
        Assert.Equal(kind switch
        {
            "enhanced" or "redesigned" => "acs-map-format-unsupported",
            "unterminated" => "acs-script-unterminated",
            "arguments" => "acs-script-arguments-unsupported",
            "empty-lump" => "behavior-lump-too-small",
            _ => "",
        }, error);
    }

    [Fact]
    public void ThingCountName_UsesBehaviorStringTableFromOldLump()
    {
        var words = new List<int>
        {
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.ThingCountName,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
        };
        words.AddRange(LightTo(35));
        words.Add((int)AcsPcode.Terminate);
        var things = PlayerStart(1).Concat(ImpThing(96)).ToArray();
        var wad = HexenMap(OldBehaviorWithStrings((1, AcsBehaviorBinder.ScriptOpen, 0, words.ToArray()), ["DoomImp"]), things);
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var sim, out var error), error);
        Assert.Equal(35, sim!.LightOf(0));
    }

    [Fact]
    public void BindingRefusesAVmThatAlreadyHasPrograms()
    {
        var vm = new AcsVm();
        vm.Add(new AcsProgram { Number = 7, Code = new byte[4] });
        Assert.False(AcsBehaviorBinder.TryBind(vm, OldBehavior(), out var error));
        Assert.Equal("acs-vm-not-empty", error);
        Assert.Equal(1, vm.ProgramCount);
    }

    private static int[] LightTo(int value) => [10, 112, 1, value, 1];

    private static byte[] EnhancedMagic(byte[] lump)
    {
        lump[3] = (byte)'E';
        return lump;
    }

    private static byte[] Redesigned(byte[] lump)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(20), 0x45534341);
        return lump;
    }

    private static byte[] OldBehaviorWithStrings(
        (int Number, int Type, int ArgCount, int[] Words) script,
        string[] stringTable)
    {
        const int directory = 24;
        var code = new List<byte>();
        foreach (var word in script.Words)
        {
            var bytes = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(bytes, word);
            code.AddRange(bytes);
        }

        var stringTableStart = directory + 4 + 12;
        var stringDataStart = stringTableStart + 4 + stringTable.Length * 4;
        var blob = new List<byte>();
        foreach (var entry in stringTable)
            blob.AddRange(Encoding.ASCII.GetBytes(entry + "\0"));
        var codeStart = stringDataStart + blob.Count;
        var lump = new byte[codeStart + code.Count];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4), directory);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory), 1);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory + 4), script.Number + script.Type * 1000);
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(directory + 8), (uint)codeStart);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory + 12), script.ArgCount);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(stringTableStart), stringTable.Length);
        var cursor = stringTableStart + 4;
        var offset = stringDataStart;
        for (var i = 0; i < stringTable.Length; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(cursor), offset);
            cursor += 4;
            offset += Encoding.ASCII.GetByteCount(stringTable[i]) + 1;
        }

        blob.CopyTo(lump.AsSpan(stringDataStart));
        code.CopyTo(lump.AsSpan(codeStart));
        return lump;
    }

    private static byte[] OldBehavior(params (int Number, int Type, int ArgCount, int[] Words)[] scripts)
    {
        const int directory = 24;
        var code = new List<byte>();
        var entries = new List<(int Packed, uint Address, int Args)>();
        foreach (var script in scripts)
        {
            entries.Add((script.Number + script.Type * 1000, (uint)(directory + 4 + scripts.Length * 12 + code.Count), script.ArgCount));
            foreach (var word in script.Words)
            {
                var bytes = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(bytes, word);
                code.AddRange(bytes);
            }
        }

        var lump = new byte[Math.Max(32, directory + 4 + scripts.Length * 12 + code.Count)];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4), directory);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory), scripts.Length);
        var cursor = directory + 4;
        foreach (var entry in entries)
        {
            BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(cursor), entry.Packed);
            BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(cursor + 4), entry.Address);
            BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(cursor + 8), entry.Args);
            cursor += 12;
        }

        code.CopyTo(lump.AsSpan(cursor));
        return lump;
    }

    private static byte[] ImpThing(short x)
    {
        var thing = new byte[20];
        BinaryPrimitives.WriteInt16LittleEndian(thing.AsSpan(2), x);
        BinaryPrimitives.WriteUInt16LittleEndian(thing.AsSpan(10), 3001);
        return thing;
    }

    private static byte[] PlayerStart(int type, short x = 0, ushort flags = 0x100)
    {
        var thing = new byte[20];
        BinaryPrimitives.WriteInt16LittleEndian(thing.AsSpan(2), x);
        BinaryPrimitives.WriteUInt16LittleEndian(thing.AsSpan(10), (ushort)type);
        // Hexen MTF_GSINGLE is 0x100. Deathmatch starts need MTF_GDEATHMATCH (0x400) as well.
        BinaryPrimitives.WriteUInt16LittleEndian(thing.AsSpan(12), flags);
        return thing;
    }

    private static byte[] HexenMap(byte[] behavior, byte[]? things = null)
    {
        var lines = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(lines.AsSpan(2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(lines.AsSpan(14), ushort.MaxValue);
        var vertices = new byte[8];
        BinaryPrimitives.WriteInt16LittleEndian(vertices.AsSpan(4), 64);
        var sectors = new byte[26];
        BinaryPrimitives.WriteInt16LittleEndian(sectors.AsSpan(2), 128);
        BinaryPrimitives.WriteInt16LittleEndian(sectors.AsSpan(20), 160);
        BinaryPrimitives.WriteInt16LittleEndian(sectors.AsSpan(24), 1);
        return Wad(
            ("MAP01", []),
            ("THINGS", things ?? []),
            ("LINEDEFS", lines),
            ("SIDEDEFS", new byte[30]),
            ("VERTEXES", vertices),
            ("SECTORS", sectors),
            ("BEHAVIOR", behavior));
    }

    private static byte[] Wad(params (string Name, byte[] Data)[] lumps)
    {
        var directory = 12 + lumps.Sum(lump => lump.Data.Length);
        var bytes = new byte[directory + 16 * lumps.Length];
        "PWAD"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), lumps.Length);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), directory);
        var cursor = 12;
        foreach (var lump in lumps)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory), cursor);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory + 4), lump.Data.Length);
            Encoding.ASCII.GetBytes(lump.Name).CopyTo(bytes, directory + 8);
            lump.Data.CopyTo(bytes, cursor);
            cursor += lump.Data.Length;
            directory += 16;
        }

        return bytes;
    }
}
