using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorTextureChangeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapRaiseChangesMetadataImmediatelyAndPreservesTemplate(bool busy)
    {
        var level = Room(); var sim = AuthoritySimulation.Start(level);
        if (busy) Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        var line = new LevelLine { Special = 239, Arg0 = 7, Arg1 = 8, Arg2 = 4, SideFront = 1, PlayerUse = true };
        Assert.Equal(!busy, LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(busy ? "A" : "B", sim.Level.Sectors[0].FloorPic);
        Assert.Equal(busy ? 26 : 27, sim.Level.Sectors[0].Special);
        Assert.Equal(busy ? 5 : 20, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(busy ? "Slime" : "Fire", sim.Level.Sectors[0].DamageType);
        Assert.Equal(busy ? 32 : 8, sim.Level.Sectors[0].DamageInterval);
        Assert.Equal(busy ? 0 : 256, sim.Level.Sectors[0].Leakiness);
        Assert.Equal(!busy, sim.Level.Sectors[0].DamageEndsGodMode);
        Assert.Equal(!busy, sim.Level.Sectors[0].DamageEndsLevel);
        Assert.Equal(5, level.Sectors[0].DamageAmount);
        Assert.Equal("Slime", level.Sectors[0].DamageType);
        Assert.Equal(32, level.Sectors[0].DamageInterval);
        Assert.Equal(0, level.Sectors[0].Leakiness);
        Assert.Equal(0, sim.FloorOf(0));
        Assert.Equal("A", level.Sectors[0].FloorPic); Assert.Equal(26, level.Sectors[0].Special);
        if (!busy)
        {
            for (var i = 0; i < 4; i++) sim.Tick();
            Assert.Equal(4, sim.FloorOf(0));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptWithoutTriggerClearsSpecialButKeepsTexture(bool stack)
    {
        var sim = AuthoritySimulation.Start(Room());
        sim.Level.Sectors[0].DamageEndsGodMode = true;
        sim.Level.Sectors[0].DamageEndsLevel = true;
        int[] words = stack ? [3, 7, 3, 8, 3, 4, 6, 239, 62, 7, 10, 112, 7, 35, 1]
            : [11, 239, 7, 8, 4, 62, 7, 10, 112, 7, 35, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Level.Sectors[0].Special); Assert.Equal("A", sim.Level.Sectors[0].FloorPic);
        Assert.Equal(0, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal("None", sim.Level.Sectors[0].DamageType);
        Assert.Equal(0, sim.Level.Sectors[0].DamageInterval);
        Assert.Equal(0, sim.Level.Sectors[0].Leakiness);
        Assert.False(sim.Level.Sectors[0].DamageEndsGodMode);
        Assert.False(sim.Level.Sectors[0].DamageEndsLevel);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(4, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DamageMetadataParticipatesInChecksum(int property)
    {
        var original = Room(); var changed = Room();
        var sector = changed.Sectors[0];
        switch (property)
        {
            case 0: sector.DamageAmount++; break;
            case 1: sector.DamageInterval++; break;
            case 2: sector.Leakiness++; break;
            case 3: sector.DamageType = "Fire"; break;
        }
        Assert.NotEqual(AuthoritySimulation.Start(original).Checksum, AuthoritySimulation.Start(changed).Checksum);
    }

    private static PlayLevel Room() => new()
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, Special = 26, FloorPic = "A", CeilingHeight = 128, LightLevel = 128,
                DamageAmount = 5, DamageType = "Slime", DamageInterval = 32 },
            new LevelSector { Index = 1, Special = 27, FloorPic = "B", CeilingHeight = 128,
                DamageAmount = 20, DamageType = "Fire", DamageInterval = 8, Leakiness = 256,
                DamageEndsGodMode = true, DamageEndsLevel = true }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
    };
}
