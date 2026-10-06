using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendlyLineBlockingTests
{
    [Theory]
    [InlineData(false, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, true, false)]
    public void FriendshipCompatibilityControlsMonsterLineBlocking(bool enabled, bool friendly,
        bool landOnly, bool blocked)
    {
        var sim = Room(enabled, landOnly ? 0 : LevelLine.BlockMonstersFlag,
            landOnly ? LevelLine.BlockLandMonstersFlag2 : 0);
        var actor = sim.AddBot(-40, 0);
        actor.Friendly = friendly;
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, xOffset: 40));
        Assert.Equal(-40, actor.X.ToDouble());
        Assert.Equal(!blocked, ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    [Theory]
    [InlineData(LevelLine.BlockingFlag, false)]
    [InlineData(LevelLine.BlockEverythingFlag, false)]
    [InlineData(LevelLine.BlockFloatersFlag, true)]
    public void FriendshipDoesNotBypassOtherBlockers(int flags, bool floating)
    {
        var sim = Room(true, flags | LevelLine.BlockMonstersFlag, 0);
        var actor = sim.AddBot(-40, 0);
        actor.Friendly = true;
        actor.Floating = floating;
        Assert.Equal(2, ActorJumpActions.CheckBlock(actor, 2, xOffset: 40));
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    private static AuthoritySimulation Room(bool enabled, int flags, int flags2)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags = flags, Flags2 = flags2
            }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -40, Y = 100 }]
        }, compat: CompatSurface.Mbf21 | (enabled ? CompatSurface.NoBlockFriends : CompatSurface.None));
    }
}
