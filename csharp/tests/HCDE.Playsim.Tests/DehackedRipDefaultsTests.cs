using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedRipDefaultsTests
{
    [Theory]
    [InlineData("RIP + 6", true, false)]
    [InlineData("RIP + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordRipDefaults(string bits, bool rip, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(rip, actor.Rip); Assert.Equal(rip, actor.DefaultRip);
        Assert.Equal(rip, bot.Rip); Assert.Equal(rip, bot.DefaultRip);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerRipPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = RIP + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Rip); Assert.True(player.DefaultRip); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("RIP + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsRipDefault(string bits, bool rip)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(rip, missile.Rip); Assert.Equal(rip, missile.DefaultRip);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool ripDefault)
    {
        var sim = Room($"Thing 2\nBits = {(ripDefault ? "RIP + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Rip = !ripDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(ripDefault, actor.Rip); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = RIP + 6\n"); var actor = sim.Actors.Single();
        actor.Rip = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(340, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultRip = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Rip); Assert.True(actor.DefaultRip); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Rip);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultRip = actor.Rip = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultRip); Assert.False(actor.Rip);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = RIP + 6\n"); var actor = sim.Actors.Single(); actor.Rip = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultRip = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = RIP + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultRip);
        Assert.False(Start(replaced).Actors.Single().DefaultRip);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = RIP + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-ripdefaults-value", error);
    }

    [Theory]
    [InlineData("RIP", false, true)]
    [InlineData("RIP", true, false)]
    [InlineData("PASSMOBJ", false, false)]
    [InlineData("PASSMOBJ", true, false)]
    public void PatchedRipControlsDamageAndPassageAfterSave(string bits, bool dontRip, bool passes)
    {
        var sim = Room($"Thing 35\nBits = {bits}\n"); var owner = sim.Actors.Single();
        owner.SetXYZ(-200, 0, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000;
        target.NoPain = true; target.DontRip = dontRip;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); missile.Rip = !missile.Rip;
        SimSavegame.Apply(sim, saved); missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.Equal(!passes, missile.Destroyed); Assert.Equal(993, target.Health);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
