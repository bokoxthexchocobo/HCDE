using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMinMissileChanceTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("200", 0)]
    [InlineData("65535", 0)]
    [InlineData("65536", 1)]
    [InlineData("98304 trailing", 1)]
    [InlineData("-1", 0)]
    [InlineData("-65535", 0)]
    [InlineData("10485760", 160)]
    [InlineData("13107200", 200)]
    [InlineData("16777215", 255)]
    public void NativeFixedPointAssignmentTruncatesToByteAndSurvivesFreshSave(string value, byte expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMin missile chance = {value}\n");
        var (sim, actor) = Room(patch); Assert.Equal(expected, actor.MinMissileChance);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, loaded.MinMissileChance); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(80)]
    [InlineData(160)]
    [InlineData(255)]
    public void PatchedCapControlsActualProbabilityAndResumedRandomSequence(int cap)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMin missile chance = {cap * 65536}\n");
        var (sim, actor) = Room(patch); var (reference, _) = Room(patch);
        var saved = SimSavegame.Write(sim, true); var outcomes = new List<bool>();
        for (var i = 0; i < 32; i++)
        {
            var expected = reference.NextCheckMissileByte() >= Math.Min(108, cap);
            var actual = actor.Brain!.CheckMissileRange(sim, actor); Assert.Equal(expected, actual); outcomes.Add(actual);
        }
        var state = sim.CaptureState().CheckMissileRandomState;
        SimSavegame.Apply(sim, saved);
        foreach (var expected in outcomes) Assert.Equal(expected, actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Equal(state, sim.CaptureState().CheckMissileRandomState);
    }

    [Theory]
    [InlineData("-65536")]
    [InlineData("16777216")]
    [InlineData("-2147483648")]
    [InlineData("4294967295")]
    public void UndefinedNativeFloatToByteDomainReportsErrorAndRetainsInheritedValue(string value)
    {
        var original = DehackedPatch.Apply("Thing 2\nMin missile chance = 10485760\n");
        var patch = DehackedPatch.Apply($"Thing 2\nMin missile chance = {value}\n", original);
        Assert.Contains(patch.Errors, error => error.Contains("byte range", StringComparison.Ordinal));
        Assert.Equal((byte)160, patch.Actors.Single(a => a.Index == 2).MinMissileChance);
        Assert.Equal((byte)160, original.Actors.Single(a => a.Index == 2).MinMissileChance);
    }

    [Fact]
    public void SharedDefaultsBindMapPlayersProjectilesAndLeaveOtherClassesUnchanged()
    {
        var patch = DehackedPatch.Apply("Thing 1\nMin missile chance = 10485760\n\nThing 2\nMin missile chance = 10485760\n\nThing 34\nMin missile chance = 10485760\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }] }, dehacked: patch);
        sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.All(sim.Actors, actor => Assert.Equal((byte)160, actor.MinMissileChance));
        Assert.Null(sim.AddBot(400, 0, 3001).MinMissileChance);
    }

    [Fact]
    public void InheritedDefaultsAndSavedRuntimeOverridesRemainIndependent()
    {
        var original = DehackedPatch.Apply("Thing 2\nMin missile chance = 10485760\n");
        var inherited = DehackedPatch.Apply("Thing 2\nMass = 200\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nMin missile chance = 0\n", inherited);
        Assert.Equal((byte)160, original.Actors.Single(a => a.Index == 2).MinMissileChance);
        var (sim, actor) = Room(replaced); Assert.Equal((byte)0, actor.MinMissileChance); actor.MinMissileChance = 77;
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(replaced); SimSavegame.Apply(fresh, saved);
        Assert.Equal((byte)77, loaded.MinMissileChance); Assert.Equal((byte)0, fresh.AddBot(400, 0).MinMissileChance);
        Assert.Equal((byte)160, Room(inherited).Actor.MinMissileChance);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var actor = sim.AddBot(0, 0); actor.ReactionTime = 0; actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
