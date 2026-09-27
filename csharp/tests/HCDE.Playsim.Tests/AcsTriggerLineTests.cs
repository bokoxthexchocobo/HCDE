using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTriggerLineTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MapStartedRaiseAndChangeUsesTriggerFrontAndBack(bool stack, bool nested)
    {
        var sim = Room();
        var words = stack ? new[] { 3, 0, 3, 8, 3, 4, 6, 239, 1 } : [11, 239, 0, 8, 4, 1];
        sim.Acs.Add(Program(nested ? 2 : 1, words));
        if (nested) sim.Acs.Add(Program(1, [13, 226, 2, 0, 0, 0, 0, 1]));
        var trigger = new LevelLine { Special = 80, Arg0 = 1, PlayerUse = true, SideFront = 0, SideBack = 1 };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), trigger, true));
        Assert.Equal(0, trigger.Special);
        sim.Acs.Tick(sim); if (nested) sim.Acs.Tick(sim);
        Assert.Equal("MODEL", sim.Level.Sectors[1].FloorPic);
        Assert.Equal(7, sim.Level.Sectors[1].DamageAmount);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(4, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(0));
    }

    [Fact]
    public void ResumeKeepsOriginalTrigger()
    {
        var sim = Room(); sim.Acs.Add(Program(1, [2, 11, 239, 0, 8, 4, 1]));
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideFront = 0, SideBack = 1 }));
        sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideFront = 1, SideBack = 0 }));
        sim.Acs.Tick(sim);
        Assert.Equal("MODEL", sim.Level.Sectors[1].FloorPic);
        Assert.Equal(7, sim.Level.Sectors[1].DamageAmount);
    }

    [Fact]
    public void TriggerTargetParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); var program = Program(1, [2, 1]);
        first.Acs.Add(program); second.Acs.Add(program);
        first.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideBack = 0 });
        second.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideBack = 1 });
        Assert.NotEqual(first.Acs.Checksum, second.Acs.Checksum);
    }

    private static AcsProgram Program(int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "MODEL", DamageAmount = 7 },
            new LevelSector { Index = 1, CeilingHeight = 128, FloorPic = "OLD" }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
    });
}
