using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FastModeOverrideTests
{
    [Theory]
    [InlineData(2, false, false, false)]
    [InlineData(2, true, false, true)]
    [InlineData(2, false, true, false)]
    [InlineData(2, true, true, true)]
    [InlineData(4, false, false, true)]
    [InlineData(4, true, false, true)]
    [InlineData(4, false, true, false)]
    [InlineData(4, true, true, true)]
    public void FlagsOverrideSkillWithAlwaysFastPrecedence(int skill, bool always, bool never, bool expected)
    {
        var sim = Room(skill); var actor = sim.Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.AlwaysFast", always));
        Assert.True(AcsActorFlags.TrySet(actor, "neverfast", never));
        Assert.True(AcsActorFlags.TryGet(actor, "ALWAYSFAST", out var actual)); Assert.Equal(always, actual);
        Assert.Equal(expected, actor.IsFast());
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Fast: true)], 0);
        actor.States.Enter(actor, 1); Assert.Equal(expected ? 3 : 5, actor.States.RemainingTics);
        var saved = SimSavegame.Write(sim); actor.AlwaysFast = actor.NeverFast = false;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(always, actor.AlwaysFast); Assert.Equal(never, actor.NeverFast);
        Assert.Equal(expected, actor.IsFast()); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Fact]
    public void OlderSaveAndRevivalResetOverrides()
    {
        var sim = Room(2); var actor = sim.Actors[0]; var older = SimSavegame.Write(sim);
        actor.AlwaysFast = actor.NeverFast = true;
        SimSavegame.Apply(sim, older); Assert.False(actor.AlwaysFast); Assert.False(actor.NeverFast);
        actor.AlwaysFast = actor.NeverFast = true;
        ActorRaise.ReviveSupported(actor); Assert.False(actor.AlwaysFast); Assert.False(actor.NeverFast);
    }

    [Fact]
    public void InvalidFlagBitsRejectBeforeMutation()
    {
        var sim = Room(2); var actor = sim.Actors[0]; actor.AlwaysFast = true;
        var saved = SimSavegame.Write(sim);
        Assert.Equal(108, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 12), 4);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved));
        Assert.True(actor.AlwaysFast);
    }

    private static AuthoritySimulation Room(int skill) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] },
        spawnOptions: new SpawnOptions(Skill: skill));
}
