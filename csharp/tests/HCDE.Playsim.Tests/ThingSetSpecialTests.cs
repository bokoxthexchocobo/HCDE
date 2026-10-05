using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingSetSpecialTests
{
    [Fact]
    public void MapLineDispatchSetsEveryMatchingActorAndConsumesLine()
    {
        var sim = Room();
        var line = new LevelLine { Special = 127, Arg0 = 7, Arg1 = 19, Arg2 = 11, Arg3 = 12, Arg4 = 13, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special);
        Assert.All(sim.Actors.Where(actor => actor.ThingId == 7), actor =>
        {
            Assert.Equal(19, actor.Special);
            Assert.Equal(new[] { 11, 12, 13, 4, 5 }, actor.SpecialArgs);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(99)]
    public void NativeTargetingAndArgumentPreservation(int tid)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(ThingSetSpecial.Execute(sim, 127, player, tid, 19, -1, 2, 3));
        foreach (var actor in sim.Actors)
        {
            var selected = tid == 0 ? actor == player : actor.ThingId == tid;
            Assert.Equal(selected ? 19 : 80, actor.Special);
            Assert.Equal(selected ? -1 : 1, actor.SpecialArgs[0]);
            Assert.Equal(4, actor.SpecialArgs[3]); Assert.Equal(5, actor.SpecialArgs[4]);
        }
        Assert.True(ThingSetSpecial.Execute(sim, 127, null, 0, 0, 0, 0, 0));
        Assert.Null(ThingSetSpecial.Execute(sim, 999, player, 0, 0, 0, 0, 0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsStackAndDirectDispatch(bool stack)
    {
        var sim = Room();
        int[] words = stack ? [3, 7, 3, 19, 3, 11, 3, 12, 3, 13, 8, 127, 1]
            : [13, 127, 7, 19, 11, 12, 13, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.Equal(19, sim.Actors[1].Special);
        Assert.Equal(new[] { 11, 12, 13, 4, 5 }, sim.Actors[1].SpecialArgs);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchiveRestoresAllArgumentsIncludingClearedSpecial(bool clearAll)
    {
        var sim = Room();
        ThingSetSpecial.Execute(sim, 127, sim.Players.Single(), 0, 0, 0, 0, 0);
        if (clearAll) Array.Clear(sim.Players.Single().SpecialArgs);
        var bytes = SimSavegame.Write(sim.CaptureState());
        Assert.Equal(42, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Players.Single().Special = 99; Array.Fill(sim.Players.Single().SpecialArgs, 99);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, sim.Players.Single().Special);
        Assert.Equal(clearAll ? new int[5] : new[] { 0, 0, 0, 4, 5 }, sim.Players.Single().SpecialArgs);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 13);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-actor-special-size", error);
    }

    [Fact]
    public void ActorSpecialAffectsChecksum()
    {
        var first = Room(); var second = Room();
        second.Actors[1].SpecialArgs[4] = 6;
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500, Special = 80, Args = [1, 2, 3, 4, 5] },
            new LevelThing { Type = 3004, X = 20, Id = 7, Special = 80, Args = [1, 2, 3, 4, 5] },
            new LevelThing { Type = 3004, X = 200, Id = 7, Special = 80, Args = [1, 2, 3, 4, 5] }],
    });
}
