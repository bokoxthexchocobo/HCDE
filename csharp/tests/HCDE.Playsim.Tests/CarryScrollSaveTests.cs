using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CarryScrollSaveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
    });

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void RestoredCarryContinuesWithSameRatesAndVelocity(int kind)
    {
        var original = Room();
        if (kind == 9) original.AppendSectorCarryScroll(0, 0.25, -0.5, ScrollCarryAffect.Monsters);
        else if (kind == 10) original.AppendAcceleratingSectorCarryScroll(0, 0.25, -0.5, ScrollCarryAffect.Players);
        else original.RegisterControlSectorCarry(0, 1, 0.25, -0.5, 128, kind == 12);
        original.Floors[1] = 2; original.Tick(); original.Tick();
        var bytes = SimSavegame.Write(original);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        for (var i = 0; i < 10; i++)
        {
            original.Floors[1] += i % 3 - 1; restored.Floors[1] = original.Floors[1];
            original.Tick(); restored.Tick();
            Assert.Equal(original.SectorScrollX, restored.SectorScrollX);
            Assert.Equal(original.SectorScrollY, restored.SectorScrollY);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
    }

    [Fact]
    public void VersionNineDoesNotClearExistingCarry()
    {
        var sim = Room(); sim.AppendSectorCarryScroll(0, 1, 0);
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [] });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Tick();
        Assert.Equal(1, sim.SectorScrollX[0]);
    }

    [Fact]
    public void InvalidControllerRejectedBeforeClockChanges()
    {
        var sim = Room();
        var state = new SimSaveState { Tic = 99, IncludesCarryScrolls = true,
            TextureScrolls = [new SimTextureScroll(0, 11, 1, 0, Control: 99)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }
}
