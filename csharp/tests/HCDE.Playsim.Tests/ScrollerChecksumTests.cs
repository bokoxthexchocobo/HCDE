using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ScrollerChecksumTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
    });

    [Fact]
    public void DifferentControlRatesAreDetectedBeforeControllerMoves()
    {
        var first = Room(); var second = Room();
        first.RegisterControlSectorCarry(0, 1, 1, 0, 128);
        second.RegisterControlSectorCarry(0, 1, 2, 0, 128);
        first.Tick(); second.Tick();
        Assert.Equal(first.SectorScrollX, second.SectorScrollX);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void AffectMasksAreDetectedWithoutActors()
    {
        var first = Room(); var second = Room();
        first.AppendSectorCarryScroll(0, 1, 0, ScrollCarryAffect.Players);
        second.AppendSectorCarryScroll(0, 1, 0, ScrollCarryAffect.Monsters);
        first.Tick(); second.Tick();
        Assert.Equal(first.SectorScrollX, second.SectorScrollX);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void ControlAccelerationFlagIsDetectedAtRest()
    {
        var first = Room(); var second = Room();
        first.RegisterControlSectorCarry(0, 1, 1, 0, 128, false);
        second.RegisterControlSectorCarry(0, 1, 1, 0, 128, true);
        first.Tick(); second.Tick();
        Assert.Equal(first.SectorScrollX, second.SectorScrollX);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void SavedCarryStateReproducesChecksumAfterNextTick()
    {
        var original = Room();
        original.AppendAcceleratingSectorCarryScroll(0, 0.25, -0.5);
        original.RegisterControlSectorCarry(0, 1, 1, 0, 128, true);
        original.Floors[1] = 2; original.Tick();
        var bytes = SimSavegame.Write(original);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        for (var i = 0; i < 5; i++)
        {
            original.Tick(); restored.Tick();
            Assert.Equal(original.Checksum, restored.Checksum);
        }
    }
}
