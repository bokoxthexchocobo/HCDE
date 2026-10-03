using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsViewHeightTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(3276800)]
    public void SetGetAndCheckUseConfiguredDefault(int value)
    {
        var sim = Room(); var player = sim.Players.Single();
        Set(sim, player, value);
        Assert.Equal(value, player.DefaultViewHeight.Raw);
        Assert.Equal(new Fixed(value).ToDouble(), player.ViewHeight);
        Query(sim, player, value);
        Run(sim, player, [3, 7, 3, 0, 3, 39, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, player, [3, 7, 3, 0, 3, 39, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
        sim.Tick(); Assert.Equal(new Fixed(value).ToDouble(), player.ViewHeight);
    }

    [Fact]
    public void CrouchScalesEyeHeightButQueriesKeepDefault()
    {
        var sim = Room(); var player = sim.Players.Single(); Set(sim, player, 50 * 65536);
        for (var i = 0; i < 6; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        Assert.Equal(0.5, player.CrouchFactor, 10); Assert.Equal(25, player.ViewHeight, 10);
        Query(sim, player, 50 * 65536);
        for (var i = 0; i < 6; i++) { sim.QueueCommand(0, new PlayerCommand()); sim.Tick(); }
        Assert.Equal(50, player.ViewHeight, 10);
    }

    [Fact]
    public void SetWhileCrouchedImmediatelyUpdatesEyeThenNextTickScalesIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        for (var i = 0; i < 6; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        Set(sim, player, 48 * 65536); Assert.Equal(48, player.ViewHeight);
        sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick();
        Assert.Equal(24, player.ViewHeight, 10); Query(sim, player, 48 * 65536);
    }

    [Fact]
    public void NonPlayerSetterIsIgnoredAndQueryReturnsZero()
    {
        var sim = Room(); var monster = sim.AddBot(128, 0); Set(sim, monster, 12345);
        Query(sim, monster, 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetsIgnoreWrites(bool destroyed)
    {
        var sim = Room(); var player = sim.Players.Single(); if (destroyed) player.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, player, [3, tid, 3, 39, 3, 12345, 245,
            3, 7, 3, tid, 3, 39, 246, 5, 112, 1]);
        Assert.Equal(41 * 65536, player.DefaultViewHeight.Raw); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void ChecksumIncludesDefaultEvenWhenCurrentEyeHeightMatches()
    {
        var first = Room(); var second = Room(); second.Players.Single().DefaultViewHeight = Fixed.FromInt(50);
        // Dead players skip crouch updates; their changing eye heights remain equal.
        first.Players.Single().Health = 0; second.Players.Single().Health = 0;
        first.Tick(); second.Tick();
        Assert.Equal(first.Players.Single().ViewHeight, second.Players.Single().ViewHeight);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void DeathEyeHeightDoesNotChangeConfiguredQuery()
    {
        var sim = Room(); var player = sim.Players.Single(); Set(sim, player, 50 * 65536);
        player.Health = 0; sim.Tick(); Assert.Equal(49, player.ViewHeight);
        Query(sim, player, 50 * 65536);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void UnknownPropertyCheckDoesNotMatchZero(int property)
    {
        var sim = Room(); var player = sim.Players.Single();
        Run(sim, player, [3, 7, 3, 0, 3, property, 3, 0, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    private static void Set(AuthoritySimulation sim, Actor actor, int value) =>
        Run(sim, actor, [3, 0, 3, 39, 3, value, 245, 1]);
    private static void Query(AuthoritySimulation sim, Actor actor, int expected)
    {
        Run(sim, actor, [3, 7, 3, 0, 3, 39, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }
    private static void Run(AuthoritySimulation sim, Actor? actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
