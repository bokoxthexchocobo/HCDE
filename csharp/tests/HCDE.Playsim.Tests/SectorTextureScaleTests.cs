using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorTextureScaleTests
{
    [Theory]
    [InlineData(170, false)]
    [InlineData(171, true)]
    [InlineData(188, false)]
    [InlineData(189, true)]
    public void MapActionInvertsFactorsAndPreservesZeroAxis(int special, bool floor)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Sectors = [new LevelSector { Tag = 7, FloorTextureScaleY = 3, CeilingTextureScaleY = 4 },
                new LevelSector { Index = 1, Tag = 7 }, new LevelSector { Index = 2 }],
        });
        var fixedPoint = special < 180;
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = fixedPoint ? -131072 : -2,
            Arg2 = fixedPoint ? 0 : 50, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        foreach (var sector in sim.Level.Sectors.Take(2))
            Assert.Equal(fixedPoint ? -0.5 : -2.0 / 3, floor ? sector.FloorTextureScaleX : sector.CeilingTextureScaleX);
        Assert.Equal(floor ? 3 : 4, floor ? sim.Level.Sectors[0].FloorTextureScaleY : sim.Level.Sectors[0].CeilingTextureScaleY);
        Assert.Equal(1, floor ? sim.Level.Sectors[0].CeilingTextureScaleX : sim.Level.Sectors[0].FloorTextureScaleX);
        Assert.Equal(1, sim.Level.Sectors[2].FloorTextureScaleX);
    }

    [Theory]
    [InlineData(170, false)]
    [InlineData(170, true)]
    [InlineData(171, false)]
    [InlineData(171, true)]
    [InlineData(188, false)]
    [InlineData(188, true)]
    [InlineData(189, false)]
    [InlineData(189, true)]
    public void AcsPathsUseCorrectFactorEncodingAndTagZero(int special, bool stack)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector(), new LevelSector { Index = 1, Tag = 7 }] });
        int[] args = special < 180 ? [0, 131072, 32768, 0, 0] : [0, 2, 0, 0, 50];
        var words = new List<int>();
        if (stack)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, special, 1]);
        }
        else words.AddRange([13, special, ..args, 1]);
        var code = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        var sector = sim.Level.Sectors[0];
        var floor = special is 171 or 189;
        Assert.Equal(0.5, floor ? sector.FloorTextureScaleX : sector.CeilingTextureScaleX);
        Assert.Equal(2, floor ? sector.FloorTextureScaleY : sector.CeilingTextureScaleY);
        Assert.Equal(1, sim.Level.Sectors[1].FloorTextureScaleX);
        Assert.Equal(1, sim.Level.Sectors[1].CeilingTextureScaleX);
    }
}
