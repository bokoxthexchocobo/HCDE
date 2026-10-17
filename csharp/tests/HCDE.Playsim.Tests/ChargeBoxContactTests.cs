using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChargeBoxContactTests
{
    [Theory]
    [InlineData(30, 30, 2, 0, true)]
    [InlineData(30, 0, -2, 0, true)]
    [InlineData(30, 0, 0, 2, true)]
    [InlineData(30, 36, 2, 0, false)]
    public void ChargeDispatchUsesStrictDestinationBox(int x, int y, int vx, int vy, bool hits)
    {
        var (sim, mover, target) = Room(); target.X = Fixed.FromInt(x); target.Y = Fixed.FromInt(y);
        mover.VelocityX = Fixed.FromInt(vx); mover.VelocityY = Fixed.FromInt(vy);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(hits ? 1 : 0, mover.Calls); Assert.Equal(!hits, mover.Brain!.Charging);
        Assert.Equal(hits ? 0 : vx, mover.X.ToDouble()); Assert.Equal(hits ? 0 : vy, mover.Y.ToDouble());
        Assert.Equal(20, target.Health);
    }

    [Fact]
    public void UnarmedTouchyOnlyTargetStillReachesSlam()
    {
        var (sim, mover, target) = Room(); target.Solid = target.Shootable = false;
        target.Touchy = true; target.Armed = false; target.SeeState = -1;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(1, mover.Calls); Assert.False(target.IsDead); Assert.Same(target, mover.Victim);
    }

    [Fact]
    public void NonsolidChargerStillDispatchesCornerSlam()
    {
        var (sim, mover, target) = Room(); mover.Solid = false;
        target.X = target.Y = Fixed.FromInt(30);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(1, mover.Calls); Assert.Same(target, mover.Victim);
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(false, true, 1)]
    public void DontOverlapReturnPrecedesSlamUnlessCompatibilityOrSpecialBypassesIt(bool legacy, bool special, int calls)
    {
        var (sim, mover, target) = Room(legacy);
        mover.CanPass = true; mover.DontOverlap = target.DontOverlap = true; target.SpecialPickup = special;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(calls, mover.Calls); Assert.Equal(0, mover.X.Raw);
    }

    private sealed class ProbeCharger : Actor
    {
        public int Calls { get; private set; }
        public Actor? Victim { get; private set; }
        public override bool Slam(Actor victim)
        {
            Calls++; Victim = victim; Brain!.StopCharge(this); return false;
        }
    }

    private static (AuthoritySimulation Sim, ProbeCharger Mover, Actor Target) Room(bool legacy = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] },
            compat: legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
        var target = sim.AddBot(30, 0); target.Brain = null; target.Solid = false;
        var mover = new ProbeCharger
        {
            Id = 50000, Simulation = sim, Level = sim.Level, Brain = MonsterBrain.ForType(3006),
            SectorIndex = 0, Solid = true, NoGravity = true, Floating = true,
            Radius = Fixed.FromInt(16), Height = Fixed.FromInt(56),
        };
        mover.States.Configure(mover, [new ActorFrame(-1, 0)], 0);
        mover.Brain!.StartCharge(mover, target); mover.VelocityX = Fixed.FromInt(2); mover.VelocityY = default;
        mover.VelocityZ = default;
        return (sim, mover, target);
    }
}
