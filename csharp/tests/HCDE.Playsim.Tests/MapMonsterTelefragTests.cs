using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapMonsterTelefragTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void MapFlagEnablesOrdinaryMonsterStompUnlessNoTeleStomp(bool enabled, bool noStomp, bool succeeds)
    {
        var sim = Room(enabled); var mover = sim.AddBot(-80, 0); var target = sim.AddBot(200, 0);
        mover.NoTeleStomp = noStomp;
        Assert.False(mover.TeleStomp);
        Assert.Equal(succeeds, LineSpecials.Execute(sim, mover, LineSpecials.Teleport, 0));
        Assert.Equal(succeeds, target.IsDead);
    }

    [Fact]
    public void MapFlagParticipatesInChecksumAndFreshSaveContinuation()
    {
        var baseline = Room(false); var sim = Room(true);
        Assert.NotEqual(baseline.Checksum, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); var restored = Room(true); SimSavegame.Apply(restored, saved);
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static AuthoritySimulation Room(bool enabled) => AuthoritySimulation.Start(new PlayLevel
    {
        AllowMonsterTelefrags = enabled, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = -500 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
