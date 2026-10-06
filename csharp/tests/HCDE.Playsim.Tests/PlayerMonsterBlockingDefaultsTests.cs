using HCDE.MapLoader;
using HCDE.Gamedata;

namespace HCDE.Playsim.Tests;

public class PlayerMonsterBlockingDefaultsTests
{
    [Fact]
    public void PlayerClassDefaultsIncludeMonsterBlockingExemption()
    {
        Assert.True(new PlayerPawn().NoBlockMonsters);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("SOLID+SHOOTABLE")]
    public void DehackedPrimaryBitsPreservePlayerExemption(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 1\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }]
        }, dehacked: patch);
        Assert.True(sim.Players.Single().NoBlockMonsters);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PlayerLineBlockingUsesExemptionFlag(bool landOnly, bool clearExemption)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags = landOnly ? 0 : LevelLine.BlockMonstersFlag,
                Flags2 = landOnly ? LevelLine.BlockLandMonstersFlag2 : 0
            }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -40 }]
        }, compat: CompatSurface.Mbf21);
        var player = sim.Players.Single();
        Assert.True(player.NoBlockMonsters);
        if (clearExemption) player.NoBlockMonsters = false;
        Assert.Equal(clearExemption ? 2 : (int?)null, ActorJumpActions.CheckBlock(player, 2, xOffset: 40));
        Assert.Equal(-40, player.X.ToDouble());
        Assert.Equal(!clearExemption, ActorPhysics.TryMove(sim, player, 40, 0, out _));
    }
}
