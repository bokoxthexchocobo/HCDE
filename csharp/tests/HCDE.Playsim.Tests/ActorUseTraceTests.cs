using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorUseTraceTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 1)]
    [InlineData(30, 0)]
    public void ZeroRangeUsesOverlappingActorOnly(int actorX, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var actor = sim.AddBot(actorX, 0); var calls = 0;
        actor.UseAction = user => { calls++; return true; };
        var player = sim.Players.Single(); player.UseRange = 0; player.UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidUseSpecialTrailerIsRejected(bool badHeader)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        sim.AddBot(0, 0).UseSpecial = true;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + (badHeader ? 0 : 8)), badHeader ? 84 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Theory]
    [InlineData(112, true, 0, 255)]
    [InlineData(9999, true, 1, 128)]
    [InlineData(112, false, 1, 128)]
    public void UseSpecialRunsBeforeCallbackAndFlagRoundTrips(int special, bool enabled, int expectedCalls, int expectedLight)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { Tag = 1, LightLevel = 128, CeilingHeight = 128 }],
        });
        var actor = sim.AddBot(32, 0);
        var stack = new List<int> { 0, 0, enabled ? 1 : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = actor },
            ["USESPECIAL"], new AcsGlobalStrings(), AcsCallFunctions.SetActorFlag, 3, out var result));
        Assert.Equal(1, result);
        actor.Special = special; actor.SpecialArgs[0] = 1; actor.SpecialArgs[1] = 255; actor.ActivationType = 32;
        var calls = 0; actor.UseAction = user => { calls++; return true; };
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.UseSpecial = !enabled; sim.RestoreState(state);
        Assert.Equal(enabled, actor.UseSpecial);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        sim.Players.Single().UsePressed = true; LineSpecials.ActivateUses(sim);
        Assert.Equal(expectedCalls, calls); Assert.Equal(expectedLight, sim.LightOf(0));
        Assert.Equal(enabled && special == 112 ? 0 : special, actor.Special);
    }

    [Theory]
    [InlineData(true, 32, false, 1, 128, false, false)]
    [InlineData(false, 32, false, 1, 255, false, false)]
    [InlineData(true, 96, false, 0, 255, false, false)]
    [InlineData(true, 32, true, 0, 128, false, false)]
    [InlineData(false, 32, false, 1, 255, true, false)]
    [InlineData(true, 32, false, 0, 255, false, true)]
    [InlineData(false, 32, false, 0, 255, false, true)]
    public void ActorCallbackIsOrderedWithLinesAndCanConsumeUse(bool consume, int actorX, bool wall, int calls, int light, bool spawn, bool noBlockmap)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.DoomBinary,
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { Tag = 1, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = wall ? 8 : 56, X2 = wall ? 8 : 56, Y1 = 64, Y2 = -64,
                SideFront = 0, SideBack = wall ? -1 : 1, Special = wall ? 0 : 138, Tag = 1 }],
        });
        var actor = sim.AddBot(actorX, 0); var count = 0;
        ActorPropertyActions.ChangeFlag(actor, "NOBLOCKMAP", noBlockmap);
        actor.UseAction = user =>
        {
            Assert.Same(sim.Players.Single(), user); count++;
            if (spawn) sim.AddBot(200, 0);
            return consume;
        };
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(calls, count); Assert.Equal(light, sim.LightOf(0));
    }
}
