using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNoBlockmapTests
{
    [Theory]
    [InlineData("6", true)]
    [InlineData("22", false)]
    [InlineData("NOBLOCKMAP+SOLID+SHOOTABLE", false)]
    public void BlockmapFlagControlsCombatActorPickingWithoutClearingShootability(string bits, bool picked)
    {
        var sim = Room(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
        var target = Assert.Single(sim.Actors);
        Assert.True(target.Shootable);
        Assert.Equal(picked, target.IsBlockmapActor);
        var source = new Actor { Health = 100 };
        var result = CombatTrace.PickActor(sim, source, BamAngle.FromDegrees(0), BamAngle.FromDegrees(0), 128);
        Assert.Equal(picked, ReferenceEquals(target, result));
    }

    [Fact]
    public void ReplacingBitsRestoresBlockmapParticipationWithoutMutatingBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = NOBLOCKMAP+6\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = 6\n", first);
        Assert.True(Assert.Single(Room(first).Actors).NoBlockmap);
        Assert.False(Assert.Single(Room(second).Actors).NoBlockmap);
    }

    [Fact]
    public void BlockmapExclusionParticipatesInSimulationHash()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = 6\n");
        var first = Room(patch); var second = Room(patch);
        Assert.Single(first.Actors).NoBlockmap = true;
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 64 }],
    }, dehacked: patch);
}
