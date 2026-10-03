using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWallEntryOrderingTests
{
    [Theory]
    [InlineData(false, 10, false)]
    [InlineData(false, 20, false)]
    [InlineData(false, 60, true)]
    [InlineData(false, 80, true)]
    [InlineData(true, 10, false)]
    [InlineData(true, 20, false)]
    [InlineData(true, 60, true)]
    [InlineData(true, 80, true)]
    public void ActorEntryBeforeWallCompletesHeightAdjustedHit(bool reverse, int wallDistance, bool actorFirst)
    {
        var sign = reverse ? -1 : 1;
        var wallX = sign * wallDistance;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = wallX, X2 = wallX, Y1 = sign * 128, Y2 = -sign * 128,
                SideFront = 0, SideBack = -1, Health = 100 }],
            Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
        });
        var source = CenterOrigin(sim); var target = sim.AddBot(sign * 80, 0);
        target.Radius = Fixed.FromInt(60); target.Z = Fixed.FromInt(100); target.Height = Fixed.FromInt(56);
        var pitch = BamAngle.FromDegrees(-45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 256);
        Assert.Equal(actorFirst, ReferenceEquals(target, hit.Victim));
        if (actorFirst) { Assert.Equal(100, hit.Z, 5); Assert.Equal(72, Math.Abs(hit.X), 5); }
        else Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(actorFirst ? target : null, CombatTrace.PickActor(sim, source, source.Angle, pitch, 256));
        var health = target.Health;
        AcsLineAttack.Attack(sim, source, unchecked((int)source.Angle.Raw), unchecked((int)pitch.Raw), 7, "None", 256,
            absoluteAngles: true);
        Assert.Equal(actorFirst ? health - 7 : health, target.Health);
        Assert.Equal(actorFirst ? 100 : 93, sim.Level.Lines[0].Health);
    }
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
