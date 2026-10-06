using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RailingBlockingPriorityTests
{
    [Theory]
    [InlineData(LevelLine.BlockingFlag)]
    [InlineData(LevelLine.BlockEverythingFlag)]
    [InlineData(LevelLine.BlockMonstersFlag)]
    [InlineData(LevelLine.BlockFloatersFlag)]
    public void OrdinaryActorUsesRailOpeningBeforeBlockingFlags(int flag)
    {
        var sim = Room(flag);
        var actor = sim.AddBot(-40, 80);
        actor.Z = Fixed.FromInt(32);
        actor.Floating = true;
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 40));
        Assert.True(ActorPhysics.TryMove(sim, actor, 1, 80, out _));
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(LevelLine.BlockingFlag, 0, true)]
    [InlineData(LevelLine.BlockEverythingFlag, 0, false)]
    [InlineData(LevelLine.BlockProjectileFlag, 0, false)]
    [InlineData(LevelLine.BlockEverythingFlag, 32, true)]
    [InlineData(LevelLine.BlockProjectileFlag, 32, true)]
    public void MissileBlockingFlagsSelectRailOpening(int flag, int z, bool passes)
    {
        var sim = Room(flag);
        var actor = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        actor.X = Fixed.FromInt(-40); actor.Y = Fixed.FromInt(80); actor.Z = Fixed.FromInt(z);
        var before = SimSavegame.Write(sim);
        Assert.Equal(passes ? null : (int?)2, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 40));
        Assert.Equal(before, SimSavegame.Write(sim));
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, 1, 80, out _));
    }

    [Fact]
    public void OneSidedRailingRemainsWall()
    {
        var sim = Room(LevelLine.BlockEverythingFlag, oneSided: true);
        var actor = sim.AddBot(-40, 80);
        actor.Z = Fixed.FromInt(32);
        Assert.Equal(2, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 40));
        Assert.False(ActorPhysics.TryMove(sim, actor, 1, 80, out _));
    }

    private static AuthoritySimulation Room(int flag, bool oneSided = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0,
                SideBack = oneSided ? -1 : 1, Flags = LevelLine.RailingFlag | flag
            }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -64 }]
        });
    }
}
