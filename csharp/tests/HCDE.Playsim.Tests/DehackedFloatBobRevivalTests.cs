using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloatBobRevivalTests
{
    [Theory]
    [InlineData("FLOATBOB + 6", true, false)]
    [InlineData("FLOATBOB + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordBobDefaults(string bits, bool bob, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(bob, actor.FloatBob); Assert.Equal(bob, actor.DefaultFloatBob);
        Assert.Equal(bob, bot.FloatBob); Assert.Equal(bob, bot.DefaultFloatBob);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerFloatBobPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = FLOATBOB + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.FloatBob); Assert.True(player.DefaultFloatBob); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("FLOATBOB + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsBobDefault(string bits, bool bob)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(bob, missile.FloatBob); Assert.Equal(bob, missile.DefaultFloatBob);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool bobDefault)
    {
        var sim = Room($"Thing 2\nBits = {(bobDefault ? "FLOATBOB + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.FloatBob = !bobDefault; actor.Health = 0;
        actor.FloatBobStrength = 2; actor.FloatBobFactor = -1; actor.FloatBobPhase = 48;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(bobDefault, actor.FloatBob); Assert.True(actor.Health > 0);
        Assert.Equal(2, actor.FloatBobStrength); Assert.Equal(-1, actor.FloatBobFactor); Assert.Equal(48, actor.FloatBobPhase);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = FLOATBOB + 6\n"); var actor = sim.Actors.Single();
        actor.FloatBob = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(334, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultFloatBob = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.FloatBob); Assert.True(actor.DefaultFloatBob); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.FloatBob);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultFloatBob = actor.FloatBob = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultFloatBob); Assert.False(actor.FloatBob);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = FLOATBOB + 6\n"); var actor = sim.Actors.Single(); actor.FloatBob = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultFloatBob = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = FLOATBOB + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultFloatBob);
        Assert.False(Start(replaced).Actors.Single().DefaultFloatBob);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = FLOATBOB + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-floatbobdefaults-value", error);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
