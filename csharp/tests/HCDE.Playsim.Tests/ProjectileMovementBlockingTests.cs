using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileMovementBlockingTests
{
    [Theory]
    [InlineData(LevelLine.BlockingFlag, true)]
    [InlineData(LevelLine.BlockMonstersFlag, true)]
    [InlineData(LevelLine.BlockFloatersFlag, true)]
    [InlineData(LevelLine.BlockProjectileFlag, false)]
    [InlineData(LevelLine.BlockEverythingFlag, false)]
    public void MissileMovementUsesProjectileRatherThanActorBlocking(int flags, bool expected)
    {
        var sim = Room(flags);
        var missile = Missile();
        Assert.Equal(expected, ActorPhysics.TryMove(sim, missile, 40, 0, out _));
    }

    [Fact]
    public void ProjectileBlockingDoesNotStopOrdinaryActorMovement()
    {
        var sim = Room(LevelLine.BlockProjectileFlag);
        var actor = new Actor { X = Fixed.FromInt(-40), SectorIndex = 0, OnGround = true };
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    private static ProjectileActor Missile() => new(new Actor(), ProjectileKind.ImpBall)
    {
        X = Fixed.FromInt(-40), Z = Fixed.FromInt(32), SectorIndex = 0, Floating = true,
    };

    private static AuthoritySimulation Room(int flags)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1, Flags = flags,
            }).ToArray(),
        });
    }
}
