using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathHeightTests
{
    [Theory]
    [InlineData(56, 14)]
    [InlineData(80, 20)]
    [InlineData(3.5, 0.875)]
    [InlineData(0, 0)]
    public void LethalDamageQuartersCurrentHeight(double height, double expected)
    {
        var actor = new Actor { Height = Fixed.FromDouble(height) };
        ActorDamage.Apply(actor, 1000);
        Assert.Equal(Fixed.FromDouble(expected), actor.Height);
        ActorDamage.Apply(actor, 1000);
        Assert.Equal(Fixed.FromDouble(expected), actor.Height);
    }

    [Fact]
    public void SurvivingDamagePreservesHeight()
    {
        var actor = new Actor(); var height = actor.Height;
        ActorDamage.Apply(actor, 1); Assert.Equal(height, actor.Height);
    }

    [Fact]
    public void DeathStateActionObservesReducedHeight()
    {
        var actor = new Actor(); Fixed? observed = null;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, a => observed = a.Height)], 0);
        actor.DeathState = 1;
        ActorDamage.Apply(actor, 1000);
        Assert.Equal(Fixed.FromInt(14), observed);
    }

    [Fact]
    public void VirtualDeathHeightIsAppliedOnceBeforeStateAction()
    {
        var actor = new CustomHeightActor();
        ActorDamage.Apply(actor, 1000, damageType: "Fire");
        Assert.Equal(Fixed.FromInt(9), actor.Height); Assert.Equal(1, actor.Calls);
        Assert.Equal("Fire", actor.ObservedType);
    }

    [Fact]
    public void PlayerDeadTicksPreserveCorpseHeight()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorDamage.Apply(player, 1000);
        Assert.Equal(Fixed.FromInt(14), player.Height);
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.Equal(Fixed.FromInt(14), player.Height);
    }

    [Fact]
    public void ResurrectionRestoresLivingDimensions()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster); var height = actor.Height;
        ActorDamage.Apply(actor, 1000); Assert.Equal(Fixed.FromDouble(height.ToDouble() / 4), actor.Height);
        ActorRaise.ReviveSupported(actor); Assert.Equal(height, actor.Height);
    }

    [Fact]
    public void DelayedSpawnDeathReducesHeightOnFirstTickOnly()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3004, Health = 0 }]
        });
        var actor = sim.Actors.Single(); var height = actor.Height;
        sim.Tick(); Assert.Equal(Fixed.FromDouble(height.ToDouble() / 4), actor.Height);
        sim.Tick(); Assert.Equal(Fixed.FromDouble(height.ToDouble() / 4), actor.Height);
    }

    [Fact]
    public void SaveReconstructsCorpseDimensionsWithoutShrinkingAgain()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster);
        ActorDamage.Apply(actor, 1000); var height = actor.Height;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(height, sim.Actors.Single(a => a.IsMonster).Height);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    private sealed class CustomHeightActor : Actor
    {
        public int Calls { get; private set; }
        public string? ObservedType { get; private set; }
        public override Fixed GetDeathHeight()
        {
            Calls++; ObservedType = DamageType; return Fixed.FromInt(9);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }]
    });
}
