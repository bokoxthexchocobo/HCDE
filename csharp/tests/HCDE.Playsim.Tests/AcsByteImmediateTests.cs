using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsByteImmediateTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void EverySpecialWidthMatchesWordForm(int count)
    {
        // Sector_SetDamage exposes all five arguments; absent arguments must be zero.
        int[] args = [7, 200, 14, 9, 255];
        var packed = new List<byte>(); Word(packed, 167 + count); packed.Add(214);
        packed.AddRange(args.Take(count).Select(value => (byte)value));
        Word(packed, 1);
        var words = new List<byte>(); Word(words, 8 + count); Word(words, 214);
        foreach (var arg in args.Take(count)) Word(words, arg);
        Word(words, 1);
        var actual = Room(); var expected = Room();
        Start(actual, packed); Start(expected, words); actual.Acs.Tick(actual); expected.Acs.Tick(expected);
        var a = actual.Level.Sectors[0]; var e = expected.Level.Sectors[0];
        Assert.Equal(e.DamageAmount, a.DamageAmount); Assert.Equal(e.DamageType, a.DamageType);
        Assert.Equal(e.DamageInterval, a.DamageInterval); Assert.Equal(e.Leakiness, a.Leakiness);
        Assert.Equal(count >= 2 ? 200 : 0, a.DamageAmount);
        Assert.Equal(0, actual.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 255)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 255)]
    public void ByteDelayUsesUnsignedDurationAndLegacyExtraTic(bool legacy, byte duration)
    {
        var code = new List<byte>(); Word(code, 3); Word(code, 7);
        Word(code, 173); code.Add(duration);
        Word(code, 3); Word(code, 35); Word(code, 5); Word(code, 112); Word(code, 1);
        var sim = Room(); Start(sim, code, legacy); sim.Acs.Tick(sim);
        var wait = duration + (legacy ? 1 : 0);
        for (var i = 0; i < wait; i++)
        {
            Assert.Equal(128, sim.LightOf(0)); sim.Acs.Tick(sim);
        }
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(168, 2)]
    [InlineData(169, 3)]
    [InlineData(170, 4)]
    [InlineData(171, 5)]
    [InlineData(172, 6)]
    [InlineData(173, 1)]
    public void EveryTruncationStopsWithoutSpecialMutation(int opcode, int payloadLength)
    {
        for (var length = 0; length < payloadLength; length++)
        {
            var code = new List<byte>(); Word(code, opcode);
            code.AddRange(new byte[] { 214, 7, 200, 14, 9, 255 }.Take(length));
            var sim = Room(); Start(sim, code); sim.Acs.Tick(sim);
            Assert.Equal(0, sim.Level.Sectors[0].DamageAmount);
            Assert.Equal(0, sim.Acs.RunningCount);
        }
    }

    [Fact]
    public void ByteSpecialKeepsActivatorForExitRestrictions()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch), noExit: true);
        var code = new List<byte>(); Word(code, 168); code.AddRange([243, 0]); Word(code, 1);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code.ToArray() });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.False(sim.Exited); Assert.True(sim.Players.Single().IsDead);
    }

    private static void Word(List<byte> code, int value)
    {
        var bytes = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value); code.AddRange(bytes);
    }
    private static void Start(AuthoritySimulation sim, List<byte> code, bool legacy = false)
    {
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code.ToArray(), LegacyHexenDelay = legacy });
        Assert.True(sim.Acs.TryExecute(1, []));
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
