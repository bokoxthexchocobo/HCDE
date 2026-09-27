using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombinedSectorEffectTests
{
    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 4, 20)]
    [InlineData(MapDataFormat.HexenBinary, 68, 20)]
    [InlineData(MapDataFormat.UdmfText, 104, 5)]
    public void LightAndDamageKeepIndependentCadences(MapDataFormat format, short special, int damage)
    {
        var level = Room(format, special); var sim = AuthoritySimulation.Start(level);
        var player = sim.Players.Single();
        var sawDark = false; var sawBrightAfterDark = false;
        for (var tic = 0; tic < 65; tic++)
        {
            sim.Tick();
            Assert.Equal(100 - (tic / 32 + 1) * damage, player.Health);
            Assert.Contains(sim.LightOf(0), new short[] { 0, 160 });
            if (sim.LightOf(0) == 0) sawDark = true;
            else if (sawDark) sawBrightAfterDark = true;
        }
        Assert.True(sawDark); Assert.True(sawBrightAfterDark);
        Assert.Equal(0, sim.Level.Sectors[0].Special);
        Assert.Equal(special, level.Sectors[0].Special); Assert.Equal(160, level.Sectors[0].LightLevel);
    }

    [Theory]
    [InlineData(68)]
    [InlineData(104)]
    public void ClearingDamageRetainsExistingStrobe(short special)
    {
        var sim = AuthoritySimulation.Start(Room(MapDataFormat.HexenBinary, special));
        var player = sim.Players.Single(); sim.Tick(); var health = player.Health;
        int[] words = [13, 214, 7, 0, 0, 1, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var levels = new HashSet<short>();
        for (var i = 0; i < 64; i++) { sim.Tick(); levels.Add(sim.LightOf(0)); Assert.Equal(health, player.Health); }
        Assert.Equal(2, levels.Count);
    }

    [Fact]
    public void StoppingLightRetainsDamage()
    {
        var sim = AuthoritySimulation.Start(Room(MapDataFormat.HexenBinary, 104));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
            { Special = 117, Arg0 = 7, PlayerUse = true }, true));
        for (var i = 0; i < 33; i++) { sim.Tick(); Assert.Equal(160, sim.LightOf(0)); }
        Assert.Equal(90, sim.Players.Single().Health);
    }

    private static PlayLevel Room(MapDataFormat format, short special) => new()
    {
        Format = format,
        Sectors = [new LevelSector { Tag = 7, Special = special, CeilingHeight = 128, LightLevel = 160 }],
        Things = [new LevelThing { Type = 1 }],
    };
}
