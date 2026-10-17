using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportFreezeTimeTests
{
    [Theory]
    [InlineData(18, 18)]
    [InlineData(4, 4)]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    public void OrdinaryTeleportUsesConfiguredNonnegativeFreeze(int configured, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.TeleportFreezeTime = configured;
        Assert.True(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.Equal(expected, player.ReactionTime);
    }

    [Fact]
    public void CustomGetterRunsBeforeSuccessfulPostHook()
    {
        var sim = Room(); var player = sim.Players.Single(); var calls = 0;
        player.TeleportFreezeTimeFunction = _ => { calls++; return 7; };
        player.PostTeleportFunction = (actor, _, _, _) => Assert.Equal(7, actor.ReactionTime);
        Assert.True(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0)); Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoStopAndCancelledTeleportsDoNotCallFreezeGetter(bool cancel)
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 3;
        var calls = 0; player.TeleportFreezeTimeFunction = _ => { calls++; return 7; };
        if (cancel) player.PreTeleportFunction = (_, _, _, _) => false;
        Assert.Equal(!cancel, LineSpecials.ExecuteTeleportSpecial(sim, cancel ? 70 : 154, 0, 7, player, false));
        Assert.Equal(0, calls); Assert.Equal(3, player.ReactionTime);
    }

    [Fact]
    public void DurationSaveChecksumAndOlderDefaultArePreserved()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        var baseline = sim.Checksum; player.TeleportFreezeTime = -5; sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(310, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(-5, restored.Players.Single().TeleportFreezeTime); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older); Assert.Equal(18, restored.Players.Single().TeleportFreezeTime);
    }

    [Fact]
    public void InvalidTrailerHeaderIsRejected()
    {
        var sim = Room(); sim.Players.Single().TeleportFreezeTime = 4;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size), 310);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
