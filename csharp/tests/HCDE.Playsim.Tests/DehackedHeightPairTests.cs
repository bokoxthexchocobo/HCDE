using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedHeightPairTests
{
    [Theory]
    [InlineData(0, false, 20, false)]
    [InlineData(16, false, 20, true)]
    [InlineData(16, false, 16, false)]
    [InlineData(-16, false, 20, false)]
    [InlineData(-16, true, 20, true)]
    [InlineData(-16, true, 16, false)]
    [InlineData(80, false, 60, false)]
    public void PatchedPassHeightControlsActualMissileContactAfterSave(int passHeight, bool clip, int z, bool passes)
    {
        var patch = Patch($"Physical height = {56 * 65536}\nProjectile pass height = {passHeight * 65536}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] },
            dehacked: patch, compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, z); missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        SimSavegame.Apply(sim, SimSavegame.Write(sim, true)); missile.DamageExpression = _ => 7; missile.Tick();
        Assert.Equal(!passes, missile.Destroyed); Assert.Equal(passes ? 1000 : 993, target.Health);
        Assert.Equal(56, target.Height.ToDouble()); Assert.Equal(passHeight, target.ProjectilePassHeight.ToDouble());
    }

    [Theory]
    [InlineData("Height = 65536\nPhysical height = 3670016\nProjectile pass height = 1048576")]
    [InlineData("Physical height = 3670016\nProjectile pass height = 1048576\nHeight = 65536")]
    [InlineData("Projectile pass height = 1048576\nHeight = 65536\nPhysical height = 3670016")]
    public void ExplicitPairTakesPrecedenceOverOrdinaryHeightInEitherOrder(string body)
    {
        var patch = Patch(body); var actor = Room(patch).AddBot(200, 0, 3001);
        Assert.Equal(56, actor.Height.ToDouble()); Assert.Equal(16, actor.ProjectilePassHeight.ToDouble());
    }

    [Theory]
    [InlineData("Physical height = 3670016")]
    [InlineData("Projectile pass height = 1048576")]
    public void PairMustBeSpecifiedInSameThingBlockEvenWithInheritedCounterpart(string body)
    {
        var original = Patch("Physical height = 3670016\nProjectile pass height = 1048576");
        var invalid = DehackedPatch.Apply($"Thing 12\n{body}\n", original);
        Assert.Contains(invalid.Errors, error => error.Contains("same Thing block", StringComparison.Ordinal));
    }

    [Fact]
    public void InheritanceAndOrdinaryHeightReplacementFollowNativePassHeightReset()
    {
        var original = Patch("Physical height = 3670016\nProjectile pass height = 1048576");
        var inherited = DehackedPatch.Apply("Thing 12\nMass = 200\n", original);
        Assert.Equal(16, Room(inherited).AddBot(200, 0, 3001).ProjectilePassHeight.ToDouble());
        var replaced = DehackedPatch.Apply("Thing 12\nHeight = 4194304\n", inherited);
        var actor = Room(replaced).AddBot(200, 0, 3001);
        Assert.Equal(64, actor.Height.ToDouble()); Assert.Equal(0, actor.ProjectilePassHeight.Raw);
        Assert.Equal(1048576, original.Actors.Single(a => a.Index == 12).ProjectilePassHeightRaw);
    }

    [Fact]
    public void MapPlayerAndProjectileDefaultsAndFreshRuntimeOverridesUseExistingArchives()
    {
        var body = "Physical height = 3670016\nProjectile pass height = 1048576\n";
        var patch = DehackedPatch.Apply($"Thing 1\n{body}\nThing 12\n{body}\nThing 34\n{body}"); Assert.Empty(patch.Errors);
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 200 }] };
        var sim = AuthoritySimulation.Start(level, dehacked: patch);
        sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.All(sim.Actors, a => { Assert.Equal(56, a.Height.ToDouble()); Assert.Equal(16, a.ProjectilePassHeight.ToDouble()); });
        var target = sim.Actors.Single(a => a.DoomEdNum == 3001); target.ProjectilePassHeight = Fixed.FromInt(23);
        var saved = SimSavegame.Write(sim, true); var fresh = AuthoritySimulation.Start(level, dehacked: patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(23, fresh.Actors.Single(a => a.Id == target.Id).ProjectilePassHeight.ToDouble());
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void NativeRevivalPreservesRuntimePassHeightInsteadOfClearingIt()
    {
        var sim = Room(Patch("Physical height = 3670016\nProjectile pass height = 1048576"));
        var actor = sim.AddBot(200, 0, 3001); actor.ProjectilePassHeight = Fixed.FromInt(23);
        ActorDamage.Apply(actor, 10000); ActorRaise.ReviveSupported(actor);
        Assert.False(actor.IsDead); Assert.Equal(23, actor.ProjectilePassHeight.ToDouble());
    }

    [Theory]
    [InlineData("Physical height = 2147483648\nProjectile pass height = 0")]
    [InlineData("Physical height = 3670016\nProjectile pass height = 4294967295")]
    public void UnrepresentableValuesReportErrors(string body)
    {
        var patch = DehackedPatch.Apply($"Thing 12\n{body}\n");
        Assert.Contains(patch.Errors, error => error.Contains("fixed-point range", StringComparison.Ordinal));
    }

    private static DehackedPatchResult Patch(string body)
    {
        var patch = DehackedPatch.Apply($"Thing 12\n{body}\n"); Assert.Empty(patch.Errors); return patch;
    }
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
