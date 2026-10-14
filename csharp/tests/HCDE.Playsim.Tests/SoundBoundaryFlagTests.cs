using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoundBoundaryFlagTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(LevelLine.BlockSoundFlag, false)]
    [InlineData(LevelLine.TwoSidedFlag, true)]
    [InlineData(LevelLine.TwoSidedFlag | LevelLine.BlockSoundFlag, true)]
    public void OrdinarySoundBoundaryRequiresTwoSidedFlag(int flags, bool crosses)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 1, Flags = flags }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }]
        });
        var emitter = sim.Actors[0]; var listener = sim.Actors[1];
        emitter.SectorIndex = 0; listener.SectorIndex = 1; listener.Brain!.Enabled = false;

        Assert.Equal(crosses, SoundPropagation.ReachableSectors(sim, 0).Contains(1));
        Assert.Equal(crosses, SoundPropagation.ReachableSectors(sim, 1).Contains(0));
        SoundPropagation.Alert(sim, emitter);
        Assert.Equal(crosses ? emitter.Id : (uint?)null, listener.LastHeardTargetId);
        Assert.Equal(crosses ? emitter.Id : (uint?)null, sim.SectorSoundTargetId(1));
        Assert.Equal(emitter.Id, sim.SectorSoundTargetId(0));
    }
}
