using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BrainDisabledSaveTests
{
    [Theory]
    [InlineData(65)]
    [InlineData(7)]
    [InlineData(84)]
    public void DisabledActiveAttackStaysPausedAfterLoadAndResumesWhenEnabled(int type)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        monster.ReactionTime = 0;
        for (var i = 0; i < 5; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        monster.Brain!.Enabled = false;
        var attack = monster.Brain.CaptureRepeatAttack(); Assert.NotNull(attack);
        var bytes = SimSavegame.Write(sim); Assert.Equal(139, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        monster.Brain.Enabled = true;
        SimSavegame.Apply(sim, bytes); monster = sim.Actors.Single(a => a.DoomEdNum == type);
        Assert.False(monster.Brain!.Enabled);
        for (var i = 0; i < 30; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(attack, monster.Brain.CaptureRepeatAttack());
        Assert.Null(sim.CaptureState().CPosAttackRandomState); Assert.Null(sim.CaptureState().SPosAttackRandomState);
        monster.Brain.Enabled = true;
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(attack!.Value.AttackTic + 1, monster.Brain.CaptureRepeatAttack()!.Value.AttackTic);
    }

    [Fact]
    public void LegacySnapshotEnablesBrainAndInvalidArchiveFlagIsRejected()
    {
        var sim = Room(3001); var legacy = sim.CaptureState();
        var monster = sim.Actors.Single(a => a.DoomEdNum == 3001); monster.Brain!.Enabled = false;
        var snapshot = sim.CaptureState(); sim.RestoreState(snapshot);
        Assert.False(sim.Actors.Single(a => a.DoomEdNum == 3001).Brain!.Enabled);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-brain-disabled-invalid", error);
        sim.RestoreState(legacy);
        Assert.True(sim.Actors.Single(a => a.DoomEdNum == 3001).Brain!.Enabled);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }] });
}
