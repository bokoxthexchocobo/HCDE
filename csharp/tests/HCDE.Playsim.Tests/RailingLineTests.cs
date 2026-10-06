using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RailingLineTests
{
    [Theory]
    [InlineData(0, 0, false, false)]
    [InlineData(0, 8, false, true)]
    [InlineData(0, 32, false, true)]
    [InlineData(16, 0, false, false)]
    [InlineData(16, 0, true, true)]
    [InlineData(0, 0, true, false)]
    public void RailRaisesOpeningWithCompatibilityGate(short floor, int z, bool compat, bool passes)
    {
        var geometry = GameplayFoundationTests.TwoRooms(floor, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags = LevelLine.RailingFlag
            }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -40 }]
        }, compat: compat ? CompatSurface.Railing : CompatSurface.None);
        var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(z);
        Assert.Equal(passes ? null : (int?)2, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 40));
        Assert.Equal(z, actor.Z.ToDouble());
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, 1, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JumpOverImportsNativeRailingFlag(bool enabled)
    {
        var text = "namespace=\"ZDoom\"; vertex {x=0;y=-128;} vertex {x=0;y=128;} "
            + "linedef {v1=0;v2=1;sidefront=0;sideback=1;jumpover=" + (enabled ? "true" : "false") + ";}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(enabled, (LevelBuilder.FromUdmf(map, "MAP01").Lines.Single().Flags & LevelLine.RailingFlag) != 0);
    }
}
