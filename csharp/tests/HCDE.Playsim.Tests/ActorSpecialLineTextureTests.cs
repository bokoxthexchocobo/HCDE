using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialLineTextureTests
{
    [Theory]
    [InlineData(183, false)]
    [InlineData(183, true)]
    [InlineData(184, false)]
    [InlineData(184, true)]
    public void AlignmentRunsFromActorSpecial(int special, bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, special, 7, 0);
        Run(sim, actor, death); var sector = sim.Level.Sectors[0];
        Assert.Equal(0xc0000000u, special == 184 ? sector.FloorTextureBaseAngle : sector.CeilingTextureBaseAngle);
        Assert.Equal(-10, special == 184 ? sector.FloorTextureBaseOffsetY : sector.CeilingTextureBaseOffsetY, 6);
    }

    [Theory]
    [InlineData(53, false)]
    [InlineData(53, true)]
    [InlineData(56, false)]
    [InlineData(56, true)]
    public void WallTransformsForwardFixedPointValuesAndPartFlags(int special, bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, special, 7, 131072, 196608, 0, 7);
        Run(sim, actor, death); var side = sim.Level.Sides[0];
        if (special == 53)
        {
            Assert.Equal(2, side.TopTextureOffsetX); Assert.Equal(3, side.TopTextureOffsetY);
            Assert.Equal(2, side.MidTextureOffsetX); Assert.Equal(3, side.BottomTextureOffsetY);
        }
        else
        {
            Assert.Equal(2, side.TopTextureScaleX); Assert.Equal(3, side.TopTextureScaleY);
            Assert.Equal(2, side.MidTextureScaleX); Assert.Equal(3, side.BottomTextureScaleY);
        }
    }

    [Theory]
    [InlineData(183, -1)]
    [InlineData(184, 99)]
    [InlineData(53, 0)]
    [InlineData(56, 0)]
    public void InvalidTargetRetainsClearSpecialOnFailure(int special, int id)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, special, id, 0);
        actor.ActivationType = 32;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(special, actor.Special);
    }

    private static void Run(AuthoritySimulation sim, Actor actor, bool death)
    {
        var special = actor.Special;
        if (death) ActorDamage.Apply(actor, 1000, null);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(death ? 0 : special, actor.Special);
    }

    private static void Configure(Actor actor, int special, params int[] args)
    {
        actor.Special = special; Array.Copy(args, actor.SpecialArgs, args.Length);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText, Namespace = "ZDoom",
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { Tag = 7, X1 = 10, Y1 = 20, X2 = 10, Y2 = 84, SideFront = 0, SideBack = -1 }],
        Things = [new LevelThing { Type = 3004, X = 100 }],
    });
}
