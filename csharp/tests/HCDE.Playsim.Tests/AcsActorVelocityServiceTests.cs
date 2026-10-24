using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorVelocityServiceTests
{
    [Theory]
    [InlineData(0, 4.25, 0, 0)]
    [InlineData(1, 0, -3.5, 0)]
    [InlineData(2, 0, 0, 12.125)]
    public void Get_ReturnsDoubleToAcsForEachAxis(int axis, double vx, double vy, double vz)
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.VelocityX = Fixed.FromDouble(vx);
        player.VelocityY = Fixed.FromDouble(vy);
        player.VelocityZ = Fixed.FromDouble(vz);
        var expected = (int)Math.Floor((axis switch { 0 => vx, 1 => vy, _ => vz }) * 65536.0);
        Assert.Equal(expected, AcsActorVelocityService.Get(sim, 0, player, axis));
    }

    [Fact]
    public void Get_ResolvesTidAndReturnsZeroWhenMissing()
    {
        var sim = Room();
        var imp = sim.Actors.First(a => a.DoomEdNum == 3001);
        imp.VelocityZ = Fixed.FromInt(5);
        Assert.Equal(0, AcsActorVelocityService.Get(sim, 99, null, 2));
        Assert.Equal((int)Math.Floor(5.0 * 65536), AcsActorVelocityService.Get(sim, imp.ThingId, null, 2));
    }

    [Fact]
    public void CallFunc_GetActorVelY_MatchesService()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.VelocityY = Fixed.FromDouble(1.5);
        var stack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = player }, [],
            new AcsGlobalStrings(), AcsCallFunctions.GetActorVelY, 1, out var result));
        Assert.Equal(AcsActorVelocityService.Get(sim, 0, player, 1), result);
    }

    private static AuthoritySimulation Room()
        => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 64, Y = 64, Id = 5 }],
        });
}
