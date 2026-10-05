using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialWallScrollTests
{
    [Theory]
    [InlineData(52, false)]
    [InlineData(52, true)]
    [InlineData(221, false)]
    [InlineData(221, true)]
    public void ActivationStartsWallScrollerWithAllArguments(int special, bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Special = special;
        int[] args = special == 52 ? [7, 65536, 131072, 0, 7] : [7, 128, 64, 64, 192];
        Array.Copy(args, actor.SpecialArgs, 5);
        if (death) ActorDamage.Apply(actor, 1000, null);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(death ? 0 : special, actor.Special);
        sim.Tick(); var side = sim.Level.Sides[0];
        Assert.Equal(1, side.TopTextureOffsetX); Assert.Equal(2, side.TopTextureOffsetY);
        Assert.Equal(1, side.MidTextureOffsetX); Assert.Equal(2, side.BottomTextureOffsetY);
        Assert.False(sim.Exited);
    }

    [Theory]
    [InlineData(52)]
    [InlineData(221)]
    public void ZeroLineIdRejectsAndPreservesSpecial(int special)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Special = special; actor.ActivationType = 32;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(special, actor.Special); sim.Tick();
        Assert.Equal(0, sim.Level.Sides[0].TopTextureOffsetX);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText, Namespace = "ZDoom",
        Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { Tag = 7, X1 = 10, Y1 = 20, X2 = 10, Y2 = 84, SideFront = 0, SideBack = -1 }],
        Things = [new LevelThing { Type = 3004, X = 100 }],
    });
}
