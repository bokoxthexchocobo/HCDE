using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class JustAttackedTests
{
    [Fact]
    public void GuardSkipsOneAttackThenAllowsAttack()
    {
        var sim = Room(); var actor = Hunter(sim); actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        actor.JustAttacked = true; actor.Brain!.Tick(sim, actor);
        Assert.False(actor.JustAttacked); Assert.Equal(0, actor.Brain.WindupTics);
        actor.Brain.Tick(sim, actor); Assert.Equal(MonsterMode.Windup, actor.Brain.Mode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuardConsumesRegardlessOfNightmareTimingOption(bool fast)
    {
        var sim = Room(); var actor = Hunter(sim); actor.SetTargetPointer(sim.Players.Single()); actor.JustAttacked = true;
        actor.Brain!.Tick(sim, actor, nightmareFast: fast);
        Assert.False(actor.JustAttacked); Assert.True(actor.VelocityX.Raw > 0); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void GuardRoundTripsThroughSave()
    {
        var sim = Room(); var actor = Hunter(sim); actor.JustAttacked = true;
        var saved = SimSavegame.Write(sim, true); actor.JustAttacked = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.JustAttacked); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OlderSnapshotClearsGuard()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); Hunter(sim).JustAttacked = true;
        SimSavegame.Apply(sim, saved); Assert.False(Hunter(sim).JustAttacked);
    }

    [Fact]
    public void MalformedFlagIsRejected()
    {
        var sim = Room(); Hunter(sim).JustAttacked = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }]
    });
}
