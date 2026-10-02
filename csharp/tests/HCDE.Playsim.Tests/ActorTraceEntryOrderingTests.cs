using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTraceEntryOrderingTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void HorizontalEntryOrderPrecedesAdjustedTopBottomDistance(bool reverse, bool descending, bool laterActorAddedFirst)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(descending ? 128 : 0);
        var sign = reverse ? -1 : 1;
        Actor first, later;
        if (laterActorAddedFirst) { later = sim.AddBot(sign * 70, 0); first = sim.AddBot(sign * 80, 0); }
        else { first = sim.AddBot(sign * 80, 0); later = sim.AddBot(sign * 70, 0); }
        first.Radius = Fixed.FromInt(60); first.Z = Fixed.FromInt(descending ? 28 : 100); first.Height = Fixed.FromInt(56);
        later.Radius = Fixed.FromInt(10); later.Z = Fixed.FromInt(0); later.Height = Fixed.FromInt(256);
        var yaw = BamAngle.FromDegrees(reverse ? 180 : 0);
        var pitch = BamAngle.FromDegrees(descending ? 45 : -45);
        var hit = CombatTrace.TraceLineAttack(sim, source, yaw, pitch, 256);
        Assert.Same(first, hit.Victim);
        Assert.Equal(descending ? 84 : 100, hit.Z, 5);
        Assert.Same(first, CombatTrace.PickActor(sim, source, yaw, pitch, 256));
        var health = first.Health;
        AcsLineAttack.Attack(sim, source, unchecked((int)yaw.Raw), unchecked((int)pitch.Raw), 7, "None", 256,
            absoluteAngles: true);
        Assert.Equal(health - 7, first.Health);
        Assert.Equal(30, later.Health);
    }
}
