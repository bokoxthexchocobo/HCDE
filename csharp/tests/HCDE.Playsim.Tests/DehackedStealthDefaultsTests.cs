using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedStealthDefaultsTests
{
    [Theory]
    [InlineData("STEALTH + 6", true, false)]
    [InlineData("FRIEND + STEALTH + 6", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsUseNativeSharedBitSelector(string bits, bool stealth, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(stealth, actor.Stealth); Assert.Equal(stealth, actor.DefaultStealth);
        Assert.Equal(stealth, bot.Stealth); Assert.Equal(stealth, bot.DefaultStealth);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerStealthPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = STEALTH + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Stealth); Assert.True(player.DefaultStealth); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("STEALTH + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileDefaultsUseSameSelector(string bits, bool stealth)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(stealth, missile.Stealth); Assert.Equal(stealth, missile.DefaultStealth);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool stealthDefault)
    {
        var sim = Room($"Thing 2\nBits = {(stealthDefault ? "STEALTH + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Stealth = !stealthDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(stealthDefault, actor.Stealth); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = STEALTH + 6\n"); var actor = sim.Actors.Single();
        actor.Stealth = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(332, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultStealth = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Stealth); Assert.True(actor.DefaultStealth); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Stealth);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultStealth = actor.Stealth = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultStealth); Assert.False(actor.Stealth);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = STEALTH + 6\n"); var actor = sim.Actors.Single(); actor.Stealth = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultStealth = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSelectorIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = STEALTH + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = FRIEND + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultStealth);
        Assert.False(Start(replaced).Actors.Single().DefaultStealth);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = STEALTH + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-stealthdefaults-value", error);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
