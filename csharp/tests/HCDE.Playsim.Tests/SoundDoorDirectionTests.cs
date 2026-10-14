using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoundDoorDirectionTests
{
    [Theory]
    [InlineData(64, 64, 0, 128, true, false)]
    [InlineData(80, 40, 0, 128, true, false)]
    [InlineData(0, 128, 64, 64, false, true)]
    [InlineData(0, 64, 64, 128, false, false)]
    [InlineData(64, 128, 0, 64, false, false)]
    [InlineData(0, 128, 32, 96, true, true)]
    [InlineData(64, 64, 64, 64, false, false)]
    public void FlatDoorChecksMatchNativeDirection(short floor0, short ceiling0, short floor1, short ceiling1,
        bool forward, bool reverse)
    {
        var sim = Room(floor0, ceiling0, floor1, ceiling1);
        Assert.Equal(forward, SoundPropagation.ReachableSectors(sim, 0).Contains(1));
        Assert.Equal(reverse, SoundPropagation.ReachableSectors(sim, 1).Contains(0));
    }

    [Fact]
    public void NoiseLeavesClosedEmitterSectorAndRestoredPlanesControlNextAlert()
    {
        var sim = Room(64, 64, 0, 128);
        var emitter = sim.Actors[0]; var listener = sim.Actors[1];
        emitter.SectorIndex = 0; listener.SectorIndex = 1; listener.Brain!.Enabled = false;
        SoundPropagation.Alert(sim, emitter);
        Assert.Equal(emitter.Id, listener.LastHeardTargetId);
        Assert.Equal(emitter.Id, sim.SectorSoundTargetId(1));

        var state = sim.CaptureState(); state.Sectors[1] = (64, 64); sim.RestoreState(state);
        emitter.SectorIndex = 0; listener.SectorIndex = 1;
        SoundPropagation.Alert(sim, emitter, null);
        Assert.Null(sim.SectorSoundTargetId(0));
        Assert.Equal(emitter.Id, sim.SectorSoundTargetId(1));
        Assert.Equal(emitter.Id, listener.LastHeardTargetId);

        state.Sectors[1] = (0, 128); sim.RestoreState(state);
        emitter.SectorIndex = 0; listener.SectorIndex = 1;
        SoundPropagation.Alert(sim, emitter, null);
        Assert.Null(listener.LastHeardTargetId);
        Assert.Null(sim.SectorSoundTargetId(1));
    }

    private static AuthoritySimulation Room(short floor0, short ceiling0, short floor1, short ceiling1)
        => AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { FloorHeight = floor0, CeilingHeight = ceiling0 },
                new LevelSector { FloorHeight = floor1, CeilingHeight = ceiling1 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }]
        });
}
