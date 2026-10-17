using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCombatDefaultsTests
{
    [Theory]
    [InlineData("PASSMOBJ", 0)]
    [InlineData("JUSTHIT", 1)]
    [InlineData("JUSTATTACKED", 2)]
    [InlineData("JUSTHIT+JUSTATTACKED", 3)]
    [InlineData("192", 3)]
    public void SpawnSaveAndRevivalKeepBothIndependentDefaults(string bits, int flags)
    {
        var sim = Room($"Thing 2\nBits = {bits}\nThing 35\nBits = {bits}\n");
        var map = sim.Actors.Single(); var dynamic = sim.AddBot(200, 0);
        var missile = sim.SpawnProjectile(map, ProjectileKind.Plasma);
        foreach (var actor in new Actor[] { map, dynamic, missile })
        {
            Assert.Equal(flags, actor.DefaultCombatFlags);
            Assert.Equal((flags & 1) != 0, actor.JustHit); Assert.Equal((flags & 2) != 0, actor.JustAttacked);
            actor.JustHit = actor.JustAttacked = false;
        }
        var saved = SimSavegame.Write(sim, true);
        map.DefaultCombatFlags = dynamic.DefaultCombatFlags = missile.DefaultCombatFlags = 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
        foreach (var actor in new Actor[] { map, dynamic, missile })
        {
            Assert.False(actor.JustHit); Assert.False(actor.JustAttacked); Assert.Equal(flags, actor.DefaultCombatFlags);
            actor.JustHit = actor.JustAttacked = true; ActorRaise.ReviveSupported(actor);
            Assert.Equal((flags & 1) != 0, actor.JustHit); Assert.Equal((flags & 2) != 0, actor.JustAttacked);
        }
    }

    [Fact]
    public void PrimaryReplacementClearsBothWhileSecondaryReplacementPreservesThem()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = JUSTHIT+JUSTATTACKED\n");
        var inherited = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", inherited);
        Assert.Equal(3, Start(inherited).Actors.Single().DefaultCombatFlags);
        Assert.Equal(0, Start(replaced).Actors.Single().DefaultCombatFlags);
    }

    [Fact]
    public void LegacyDefaultsClearAndDifferentDefaultsChangeChecksum()
    {
        var sim = Room("Thing 2\nBits = JUSTHIT+JUSTATTACKED\n"); var actor = sim.Actors.Single();
        actor.JustHit = actor.JustAttacked = false; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        actor.DefaultCombatFlags = 0; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(hash, sim.Checksum);
        var legacy = SimSavegame.Write(sim, true); actor.DefaultCombatFlags = 3;
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, actor.DefaultCombatFlags);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void InvalidDefaultMasksAreRejected(int value)
    {
        var sim = Room("Thing 2\nBits = JUSTHIT\n"); var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(348, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-combatdefaults-value", error);
        var state = sim.CaptureState(); state.Actors[0].CombatDefaultsFlags = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    [Fact]
    public void PatchedPostAttackFlagIsConsumedByChaseWithoutLosingSavedDefault()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = JUSTATTACKED+COUNTKILL+6\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = true }], Things = [new LevelThing { Type = 1, X = 300 }] }, dehacked: patch);
        var actor = sim.AddBot(0, 0); actor.ReactionTime = 0; actor.MoveCount = 123;
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        Assert.True(actor.JustAttacked);
        actor.Brain.Tick(sim, actor, MonsterChaseFlags.StopIfBlocked, nativeMovement: true);
        Assert.False(actor.JustAttacked); Assert.Equal(2, actor.DefaultCombatFlags); Assert.Equal(0, actor.Brain.WindupTics);
        var saved = SimSavegame.Write(sim, true); actor.DefaultCombatFlags = 0;
        SimSavegame.Apply(sim, saved); ActorRaise.ReviveSupported(actor); Assert.True(actor.JustAttacked);
    }

    private static AuthoritySimulation Room(string text, bool player = false) => Start(DehackedPatch.Apply(text), player);
    private static AuthoritySimulation Start(DehackedPatchResult patch, bool player = false)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = (ushort)(player ? 1 : 3004) }] }, dehacked: patch);
    }
}
