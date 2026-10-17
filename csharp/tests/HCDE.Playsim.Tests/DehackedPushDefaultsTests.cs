using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedPushDefaultsTests
{
    [Theory]
    [InlineData("PASSMOBJ", 0)]
    [InlineData("PUSHABLE", 1)]
    [InlineData("CANNOTPUSH", 2)]
    [InlineData("PUSHABLE+CANNOTPUSH", 3)]
    public void SpawnSaveAndRevivalKeepBothIndependentDefaults(string bits, int flags)
    {
        var sim = Room($"Thing 2\nBits = {bits}\nThing 35\nBits = {bits}\n");
        var map = sim.Actors.Single(); var dynamic = sim.AddBot(200, 0);
        var missile = sim.SpawnProjectile(map, ProjectileKind.Plasma);
        foreach (var actor in new Actor[] { map, dynamic, missile })
        {
            Assert.Equal(flags, actor.DefaultPushFlags);
            Assert.Equal((flags & 1) != 0, actor.Pushable); Assert.Equal((flags & 2) != 0, actor.CannotPush);
            actor.Pushable = actor.CannotPush = false;
        }
        var saved = SimSavegame.Write(sim, true);
        map.DefaultPushFlags = dynamic.DefaultPushFlags = missile.DefaultPushFlags = 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
        foreach (var actor in new Actor[] { map, dynamic, missile })
        {
            Assert.False(actor.Pushable); Assert.False(actor.CannotPush); Assert.Equal(flags, actor.DefaultPushFlags);
            actor.Pushable = actor.CannotPush = true; ActorRaise.ReviveSupported(actor);
            Assert.Equal((flags & 1) != 0, actor.Pushable); Assert.Equal((flags & 2) != 0, actor.CannotPush);
        }
    }

    [Theory]
    [InlineData("PUSHABLE", "PASSMOBJ", 6)]
    [InlineData("PUSHABLE", "CANNOTPUSH", 0)]
    [InlineData("PASSMOBJ", "PASSMOBJ", 0)]
    [InlineData("PUSHABLE+CANNOTPUSH", "PASSMOBJ", 6)]
    public void PatchedFlagsControlContactMomentumAfterSave(string targetBits, string moverBits, int expected)
    {
        var sim = Room($"Thing 2\nBits = {targetBits}\nThing 1\nBits = {moverBits}\n", true);
        var player = sim.Players.Single(); var target = sim.AddBot(40, 0);
        target.Brain = null; target.Solid = false; player.VelocityX = Fixed.FromInt(24);
        var saved = SimSavegame.Write(sim, true); target.Pushable = !target.Pushable; player.CannotPush = !player.CannotPush;
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, player);
        Assert.Equal(expected, target.VelocityX.ToDouble());
    }

    [Fact]
    public void SecondaryReplacementClearsBothWhilePrimaryReplacementPreservesThem()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = PUSHABLE+CANNOTPUSH\n");
        var inherited = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", inherited);
        Assert.Equal(3, Start(inherited).Actors.Single().DefaultPushFlags);
        Assert.Equal(0, Start(replaced).Actors.Single().DefaultPushFlags);
    }

    [Fact]
    public void LegacyDefaultsClearAndDifferentDefaultsChangeChecksum()
    {
        var sim = Room("Thing 2\nBits = PUSHABLE+CANNOTPUSH\n"); var actor = sim.Actors.Single();
        actor.Pushable = actor.CannotPush = false; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        actor.DefaultPushFlags = 0; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(hash, sim.Checksum);
        var legacy = SimSavegame.Write(sim, true); actor.DefaultPushFlags = 3;
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, actor.DefaultPushFlags);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void InvalidDefaultMasksAreRejected(int value)
    {
        var sim = Room("Thing 2\nBits = PUSHABLE\n"); var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(341, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-pushdefaults-value", error);
        var state = sim.CaptureState(); state.Actors[0].PushDefaultsFlags = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    private static AuthoritySimulation Room(string text, bool player = false) => Start(DehackedPatch.Apply(text), player);
    private static AuthoritySimulation Start(DehackedPatchResult patch, bool player = false)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = (ushort)(player ? 1 : 3004) }] }, dehacked: patch);
    }
}
