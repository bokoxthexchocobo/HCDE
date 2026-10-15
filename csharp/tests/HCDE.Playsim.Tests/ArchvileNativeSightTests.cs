using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileNativeSightTests
{
    [Fact]
    public void DirectAttackDamagesPartiallyVisibleTarget()
    {
        var (sim, vile) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        Assert.False(CombatTrace.HasLineOfSight(sim, vile, target));
        ArchvileActions.Attack(sim, vile, target, false);
        Assert.Equal(980, target.Health); Assert.Equal(Fixed.FromInt(10), target.VelocityZ);
        Assert.Contains(sim.DrainSoundRequests(), s => s.Sound == "vile/stop");
    }

    [Fact]
    public void MvisBlockedRejectsDamageSoundAndThrustAfterFacing()
    {
        var (sim, vile) = Room(); var target = sim.Players.Single();
        target.Health = 1000; target.MVisBlocked = true; target.VelocityZ = Fixed.FromInt(-3);
        vile.Angle = BamAngle.FromDegrees(180); vile.Ambush = true;
        ArchvileActions.Attack(sim, vile, target, true);
        Assert.Equal(BamAngle.FromDegrees(0), vile.Angle); Assert.False(vile.Ambush);
        Assert.Equal(1000, target.Health); Assert.Equal(Fixed.FromInt(-3), target.VelocityZ);
        Assert.DoesNotContain(sim.DrainSoundRequests(), s => s.Sound == "vile/stop");
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DirectAttackUsesNativeVisibilityProbabilityAndOneDraw(int kind)
    {
        var (sim, vile) = Room(); var (reference, _) = Room(); var target = sim.Players.Single();
        target.NoPain = true; target.Invisible = kind == 0; target.MinVisible = kind == 1;
        if (kind == 2) { target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0; }
        for (var i = 0; i < 256; i++)
        {
            var attacks = reference.NextCheckSightByte() <= 50;
            target.Health = 1000; target.VelocityZ = default;
            ArchvileActions.Attack(sim, vile, target, false);
            Assert.Equal(attacks ? 980 : 1000, target.Health);
            Assert.Equal(Fixed.FromInt(attacks ? 10 : 0), target.VelocityZ);
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
            sim.DrainSoundRequests();
        }
    }

    [Fact]
    public void SplashUsesVictimEyeTowardInheritedFireHeight()
    {
        var (sim, vile) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        var victim = sim.AddBot(-40, 40); victim.Health = 1000; victim.NoPain = true;
        ArchvileActions.Attack(sim, vile, target, true, initialDamage: 0, blastDamage: 140, blastRadius: 140);
        Assert.Equal(920, victim.Health);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedVisibilityDecisionReplaysAttackEffects()
    {
        var (sim, vile) = Room(); var target = sim.Players.Single();
        target.Health = 1000; target.NoPain = true; target.Invisible = true;
        var saved = SimSavegame.Write(sim, true);
        ArchvileActions.Attack(sim, vile, target, false); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); ArchvileActions.Attack(sim, vile, target, false);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var vile = sim.AddBot(-64, 0, 64); vile.Brain = null;
        sim.DrainSoundRequests();
        return (sim, vile);
    }
}
