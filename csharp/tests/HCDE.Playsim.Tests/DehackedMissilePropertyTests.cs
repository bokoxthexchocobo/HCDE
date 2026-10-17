using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMissilePropertyTests
{
    public static IEnumerable<object[]> Values =>
        from propertyName in new[] { "Melee threshold", "Max target range", "Missile chance multiplier" }
        from raw in new long[] { 0, -65536, 98304, int.MinValue, uint.MaxValue }
        select new object[] { propertyName, raw };

    [Theory]
    [MemberData(nameof(Values))]
    public void NativeFixedPointDefaultsBindAndSurviveFreshSave(string field, long raw)
    {
        var patch = DehackedPatch.Apply($"Thing 2\n{field} = {raw} trailing\n"); Assert.Empty(patch.Errors);
        var (sim, actor) = Room(patch); Assert.Equal(raw / 65536.0, Value(actor, field));
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(raw / 65536.0, Value(loaded, field)); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(107, false)]
    [InlineData(108, true)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void MaximumRangeUsesAdjustedDistanceAndZeroOrNegativeMeansUnlimited(int maximum, bool attacks)
    {
        var (sim, actor) = Room(DehackedPatch.Apply($"Thing 2\nMax target range = {maximum * 65536}\nMissile chance multiplier = 0\n"));
        SimSavegame.Apply(sim, SimSavegame.Write(sim, true));
        Assert.Equal(attacks, actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Equal(attacks, sim.CaptureState().CheckMissileRandomState.HasValue);
    }

    [Theory]
    [InlineData(236, true)]
    [InlineData(237, false)]
    public void MeleeThresholdUsesStrictLowerBoundForMeleeMonster(int minimum, bool attacks)
    {
        var patch = DehackedPatch.Apply($"Thing 12\nMelee threshold = {minimum * 65536}\nMissile chance multiplier = 0\n");
        var (sim, actor) = Room(patch, 3001);
        Assert.Equal(attacks, actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Equal(attacks, sim.CaptureState().CheckMissileRandomState.HasValue);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(32768, 54)]
    [InlineData(65536, 108)]
    [InlineData(131072, 200)]
    public void MultiplierControlsNativeProbabilityAndResumedRandomStream(int raw, int threshold)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMissile chance multiplier = {raw}\n");
        var (sim, actor) = Room(patch); var (reference, _) = Room(patch);
        var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 16; i++)
            Assert.Equal(reference.NextCheckMissileByte() >= threshold, actor.Brain!.CheckMissileRange(sim, actor));
        var expected = sim.CaptureState().CheckMissileRandomState;
        SimSavegame.Apply(sim, saved);
        for (var i = 0; i < 16; i++) actor.Brain!.CheckMissileRange(sim, actor);
        Assert.Equal(expected, sim.CaptureState().CheckMissileRandomState);
    }

    [Fact]
    public void SharedBindingCoversMapPlayerProjectileAndPreservesUnpatchedClasses()
    {
        var body = "Max target range = 65536\nMelee threshold = 131072\nMissile chance multiplier = 32768\n";
        var patch = DehackedPatch.Apply($"Thing 1\n{body}\nThing 2\n{body}\nThing 34\n{body}");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }] }, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.All(sim.Actors, actor => { Assert.Equal(1, actor.MaxTargetRange); Assert.Equal(2, actor.MeleeThreshold); Assert.Equal(0.5, actor.MissileChanceMult); });
        Assert.Equal(1, missile.MaxTargetRange);
        var untouched = sim.AddBot(400, 0, 3001);
        Assert.Null(untouched.MaxTargetRange); Assert.Null(untouched.MeleeThreshold); Assert.Null(untouched.MissileChanceMult);
    }

    [Fact]
    public void LayeredDefaultsAndSavedRuntimeOverrideRemainIndependent()
    {
        var original = DehackedPatch.Apply("Thing 2\nMax target range = 65536\nMelee threshold = 131072\nMissile chance multiplier = 32768\n");
        var inherited = DehackedPatch.Apply("Thing 2\nMass = 200\n", original);
        var replacement = DehackedPatch.Apply("Thing 2\nMax target range = 0\n", inherited);
        Assert.Equal(1, original.Actors.Single(a => a.Index == 2).MaxTargetRange);
        var (sim, actor) = Room(replacement); Assert.Equal(0, actor.MaxTargetRange); Assert.Equal(2, actor.MeleeThreshold);
        actor.MaxTargetRange = 77; actor.MeleeThreshold = -8; actor.MissileChanceMult = 3;
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(replacement); SimSavegame.Apply(fresh, saved);
        Assert.Equal(77, loaded.MaxTargetRange); Assert.Equal(-8, loaded.MeleeThreshold); Assert.Equal(3, loaded.MissileChanceMult);
        var next = fresh.AddBot(400, 0); Assert.Equal(0, next.MaxTargetRange); Assert.Equal(2, next.MeleeThreshold); Assert.Equal(0.5, next.MissileChanceMult);
    }

    private static double? Value(Actor actor, string field) => field switch {
        "Melee threshold" => actor.MeleeThreshold, "Max target range" => actor.MaxTargetRange,
        "Missile chance multiplier" => actor.MissileChanceMult, _ => throw new ArgumentOutOfRangeException(nameof(field)) };

    private static (AuthoritySimulation Sim, Actor Actor) Room(DehackedPatchResult patch, ushort type = 3004)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var actor = sim.AddBot(0, 0, type); actor.ReactionTime = 0; actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
