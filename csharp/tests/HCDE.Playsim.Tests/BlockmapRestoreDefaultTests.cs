using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockmapRestoreDefaultTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOverrideRestoresClassParticipationAndCombatPicking(bool excluded)
    {
        var sim = Room(excluded); var target = Assert.Single(sim.Actors);
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.ChangeFlag(target, "NOBLOCKMAP", !excluded);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(excluded, target.NoBlockmap);
        Assert.False(target.HasBlockmapOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        var picked = CombatTrace.PickActor(sim, new Actor { Health = 100 },
            BamAngle.FromDegrees(0), BamAngle.FromDegrees(0), 128);
        Assert.Equal(!excluded, ReferenceEquals(target, picked));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitFlagRestoresItsValueAndOverridePresence(bool excluded)
    {
        var sim = Room(false); var target = Assert.Single(sim.Actors);
        ActorPropertyActions.ChangeFlag(target, "NOBLOCKMAP", excluded);
        var bytes = SimSavegame.Write(sim);
        target.NoBlockmap = !excluded;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(excluded, target.NoBlockmap);
        Assert.True(target.HasBlockmapOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room(bool excluded) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 64 }],
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = " + (excluded ? "22" : "6") + "\n"));
}
