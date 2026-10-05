using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialTextureTests
{
    [Theory]
    [InlineData(186, false)]
    [InlineData(186, true)]
    [InlineData(187, false)]
    [InlineData(187, true)]
    public void PanningForwardsBothFractionalAxes(int special, bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; Configure(actor, special, 7, 2, 25, -3, 50);
        Run(sim, actor, death); var sector = sim.Level.Sectors[0]; var floor = special == 187;
        Assert.Equal(2.25, floor ? sector.FloorTextureOffsetX : sector.CeilingTextureOffsetX);
        Assert.Equal(-2.5, floor ? sector.FloorTextureOffsetY : sector.CeilingTextureOffsetY);
        Assert.Equal(0, floor ? sector.CeilingTextureOffsetX : sector.FloorTextureOffsetX);
        Assert.Equal(0, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(0, sim.Level.Sectors[1].CeilingTextureOffsetX);
    }

    [Theory]
    [InlineData(170, false)]
    [InlineData(170, true)]
    [InlineData(171, false)]
    [InlineData(171, true)]
    [InlineData(188, false)]
    [InlineData(188, true)]
    [InlineData(189, false)]
    [InlineData(189, true)]
    public void ScaleUsesReciprocalAndPreservesZeroAxis(int special, bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; var fixedPoint = special is 170 or 171;
        Configure(actor, special, 7, fixedPoint ? 131072 : 2, 0, 0, 0);
        Run(sim, actor, death); var sector = sim.Level.Sectors[0]; var floor = special is 171 or 189;
        Assert.Equal(0.5, floor ? sector.FloorTextureScaleX : sector.CeilingTextureScaleX);
        Assert.Equal(1, floor ? sector.FloorTextureScaleY : sector.CeilingTextureScaleY);
        Assert.Equal(1, floor ? sector.CeilingTextureScaleX : sector.FloorTextureScaleX);
        Assert.Equal(1, sim.Level.Sectors[1].FloorTextureScaleX);
        Assert.Equal(1, sim.Level.Sectors[1].CeilingTextureScaleX);
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
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { Tag = 8, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 20 }],
    });
}
