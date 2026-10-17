using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedContactPushTests
{
    [Theory]
    [InlineData("boss")]
    [InlineData("dontblast")]
    [InlineData("notmonster")]
    [InlineData("notshootable")]
    public void IneligibleBlastTargetFallsThroughToOrdinaryPush(string exclusion)
    {
        var (sim, mover, target) = Room();
        if (exclusion == "boss") target.Boss = true;
        if (exclusion == "dontblast") target.DontBlast = true;
        if (exclusion == "notmonster") target.IsMonster = false;
        if (exclusion == "notshootable") { target.Shootable = false; target.SpecialPickup = true; }
        ActorPhysics.Step(sim, mover);
        Assert.Equal(2, target.VelocityX.ToDouble()); Assert.Equal(8, mover.X.ToDouble());
        Assert.Equal(1000, target.Health); Assert.True(target.LastPushFromMovement);
        Assert.Equal("push/test", Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Fact]
    public void CannotPushSuppressesFallbackWithoutCreatingBlastImpact()
    {
        var (sim, mover, target) = Room(); target.Boss = true; mover.CannotPush = true;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(1000, target.Health);
        Assert.Empty(sim.PendingSoundRequests); Assert.False(target.LastPushFromMovement);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EligibleBlastImpactDoesNotAlsoApplyOrdinaryPush(bool cannotPush)
    {
        var (sim, mover, target) = Room(); mover.CannotPush = cannotPush;
        mover.MovementPushContacts = new HashSet<Actor>();
        Assert.False(ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(8, target.VelocityX.ToDouble()); Assert.Equal(997, target.Health);
        Assert.Equal(999, mover.Health); Assert.False(target.LastPushFromMovement);
        Assert.Empty(sim.PendingSoundRequests); Assert.Empty(mover.MovementPushContacts);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(40, 0);
        mover.Brain = target.Brain = null; mover.Health = target.Health = 1000;
        mover.PainChance = target.PainChance = 0; mover.DoHarmSpecies = target.DoHarmSpecies = true;
        mover.Mass = 200; target.Mass = 300; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(8);
        target.Solid = false; target.Pushable = true;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
