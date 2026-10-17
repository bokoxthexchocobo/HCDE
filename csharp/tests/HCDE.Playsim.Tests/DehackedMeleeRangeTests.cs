using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMeleeRangeTests
{
    [Theory]
    [InlineData("0", -1310720)]
    [InlineData("-65536", -1376256)]
    [InlineData("4194304", 2883584)]
    [InlineData("8388608 trailing", 7077888)]
    [InlineData("4194305", 2883585)]
    [InlineData("2148794367", int.MaxValue)]
    public void FixedPointRangeUsesNativeTwentyUnitAdjustmentAndFreshSave(string value, int raw)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMelee range = {value}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var actor = sim.AddBot(0, 0);
        Assert.Equal(raw, actor.MeleeRange.Raw);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(raw, fresh.Actors.Single(a => a.Id == actor.Id).MeleeRange.Raw);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(63, false)]
    [InlineData(64, false)]
    [InlineData(65, true)]
    public void PatchedRangeControlsStrictMeleeBoundaryAfterRestore(int range, bool expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMelee range = {range * 65536}\n");
        var sim = Room(patch); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        target.Radius = Fixed.FromInt(16); actor.Brain!.SetSpecialTarget(target);
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        var random = sim.CombatRandomState;
        Assert.Equal(expected, actor.CheckMeleeRange()); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void DefaultsBindMapActorsPlayersAndProjectilesWithoutChangingUnpatchedClasses()
    {
        var patch = DehackedPatch.Apply("Thing 1\nMelee range = 8388608\n\nThing 2\nMelee range = 8388608\n\nThing 34\nMelee range = 8388608\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }] }, dehacked: patch);
        Assert.Empty(patch.Errors);
        Assert.All(sim.Actors, actor => Assert.Equal(108, actor.MeleeRange.ToDouble()));
        Assert.Equal(108, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket).MeleeRange.ToDouble());
        Assert.Equal(44, sim.AddBot(400, 0, 3001).MeleeRange.ToDouble());
    }

    [Fact]
    public void InheritedDefaultAndSavedRuntimeOverrideRemainIndependent()
    {
        var original = DehackedPatch.Apply("Thing 2\nMelee range = 8388608\n");
        var inherited = DehackedPatch.Apply("Thing 2\nMass = 200\n", original);
        var replacement = DehackedPatch.Apply("Thing 2\nMelee range = 4194304\n", inherited);
        Assert.Equal(108 * 65536, original.Actors.Single(a => a.Index == 2).MeleeRangeRaw);
        Assert.Equal(44 * 65536, replacement.Actors.Single(a => a.Index == 2).MeleeRangeRaw);
        var sim = Room(inherited); var actor = sim.AddBot(0, 0); actor.MeleeRange = Fixed.FromInt(77);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(inherited); SimSavegame.Apply(fresh, saved);
        Assert.Equal(77, fresh.Actors.Single(a => a.Id == actor.Id).MeleeRange.ToDouble());
        Assert.Equal(108, fresh.AddBot(200, 0).MeleeRange.ToDouble());
        actor.MeleeRange = Fixed.FromInt(44); SimSavegame.Apply(sim, saved);
        Assert.Equal(77, actor.MeleeRange.ToDouble());
    }

    [Theory]
    [InlineData("-2147483648")]
    [InlineData("2148794368")]
    [InlineData("4294967295")]
    public void UnrepresentableAdjustedRangeReportsErrorInsteadOfWrapping(string value)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMelee range = {value}\n");
        Assert.Contains(patch.Errors, error => error.Contains("fixed-point range", StringComparison.Ordinal));
        Assert.Null(patch.Actors.Single(a => a.Index == 2).MeleeRangeRaw);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1, X = 60 }], Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
