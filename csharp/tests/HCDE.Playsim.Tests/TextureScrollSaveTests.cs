using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TextureScrollSaveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide()],
        Lines = [new LevelLine { Tag = 7, SideFront = 0, SideBack = -1 }],
    });

    [Fact]
    public void RestoredPlaneAndWallScrollsContinueIdentically()
    {
        var original = Room();
        original.SetSectorTextureScroll(0, 0.25, -0.5, SectorTextureScrollPlane.Floor);
        original.SetSectorTextureScroll(0, -1, 2, SectorTextureScrollPlane.Ceiling);
        original.SetWallTextureScroll(7, 0, 0.5, -0.25, WallScrollParts.All);
        original.Tick();
        var bytes = SimSavegame.Write(original);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.SetSectorTextureScroll(0, 99, 99, SectorTextureScrollPlane.Floor);
        restored.RestoreState(state);
        for (var i = 0; i < 20; i++)
        {
            original.Tick(); restored.Tick();
            Assert.Equal(original.CaptureState().Walls, restored.CaptureState().Walls);
            Assert.Equal(original.CaptureState().Planes, restored.CaptureState().Planes);
        }
    }

    [Fact]
    public void ExplicitEmptyScrollStateClearsExistingRates()
    {
        var sim = Room();
        sim.SetSectorTextureScroll(0, 1, 0, SectorTextureScrollPlane.Floor);
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [] });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Tick();
        Assert.Equal(0, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void InvalidTargetFailsBeforeRestoreMutation()
    {
        var sim = Room();
        var state = new SimSaveState { Tic = 99, TextureScrolls = [new SimTextureScroll(99, 0, 1, 0)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void ScrollerArchiveRejectsAllTruncationsAndInvalidRates()
    {
        var sim = Room(); sim.SetSectorTextureScroll(0, 1, 0, SectorTextureScrollPlane.Floor);
        var bytes = SimSavegame.Write(sim);
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(new SimSaveState
            { TextureScrolls = [new SimTextureScroll(0, 0, double.NaN, 0)] }));
    }
}
