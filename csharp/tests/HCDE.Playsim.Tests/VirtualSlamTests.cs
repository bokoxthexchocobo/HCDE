using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VirtualSlamTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallbackMaySpawnActorsAndDestroyedChargerStopsMoving(bool destroy)
    {
        var sim = Room(); var target = sim.Players.Single();
        var actor = new MutatingSlamActor(destroy)
        {
            Id = 50000, Simulation = sim, Level = sim.Level, Brain = MonsterBrain.ForType(3006),
            SectorIndex = 0, Solid = true, Floating = true, NoGravity = true,
            Radius = Fixed.FromInt(16), Height = Fixed.FromInt(56),
        };
        actor.States.Configure(actor, [new ActorFrame(-1, 0)], 0);
        actor.Brain!.StartCharge(actor, target);
        for (var i = 0; i < 4 && !actor.Destroyed; i++) actor.Tick();
        Assert.True(actor.Spawned); Assert.Equal(2, sim.Actors.Count);
        Assert.Equal(destroy, actor.Destroyed);
        Assert.Equal(!destroy, actor.X.ToDouble() > target.X.ToDouble());
        Assert.Equal(100, target.Health); Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ChargeContactDispatchesOverrideAndHonorsPassResult(bool pass)
    {
        var sim = Room(); var target = sim.Players.Single();
        var actor = new CustomSlamActor(pass)
        {
            Id = 50000, Simulation = sim, Level = sim.Level, Brain = MonsterBrain.ForType(3006),
            SectorIndex = 0, Solid = true, Floating = true, NoGravity = true,
            Radius = Fixed.FromInt(16), Height = Fixed.FromInt(56),
        };
        actor.States.Configure(actor, [new ActorFrame(-1, 0)], 0);
        actor.Brain!.StartCharge(actor, target);
        for (var i = 0; i < 4 && actor.Brain.Charging; i++) actor.Tick();
        Assert.True(actor.Calls > 0); Assert.Same(target, actor.LastVictim);
        Assert.Equal(pass, actor.Brain.Charging);
        Assert.Equal(pass, actor.X.ToDouble() > target.X.ToDouble());
        Assert.Equal(100, target.Health); Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void DirectDefaultSlamHonorsOnlySolidBeforeClearingCharge()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3006); var target = sim.Players.Single();
        actor.Brain!.StartCharge(actor, target); actor.OnlySlamSolid = true;
        target.Solid = target.Shootable = false;
        Assert.True(actor.Slam(target)); Assert.True(actor.Brain.Charging);
        Assert.Equal(20, actor.VelocityX.ToDouble()); Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    private sealed class CustomSlamActor(bool pass) : Actor
    {
        public int Calls { get; private set; }
        public Actor? LastVictim { get; private set; }
        public override bool Slam(Actor victim)
        {
            Calls++; LastVictim = victim;
            if (!pass) Brain!.StopCharge(this);
            return pass;
        }
    }

    private sealed class MutatingSlamActor(bool destroy) : Actor
    {
        public bool Spawned { get; private set; }
        public override bool Slam(Actor victim)
        {
            if (!Spawned) { Simulation!.AddBot(1000, 0); Spawned = true; }
            if (destroy) Destroy();
            return true;
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1, X = 64 }] });
}
