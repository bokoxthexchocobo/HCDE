using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNoClipDefaultsTests
{
    [Theory]
    [InlineData("NOCLIP + 6", true, false)]
    [InlineData("NOCLIP + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordNoClipDefaults(string bits, bool noclip, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(noclip, actor.NoClip); Assert.Equal(noclip, actor.DefaultNoClip);
        Assert.Equal(noclip, bot.NoClip); Assert.Equal(noclip, bot.DefaultNoClip);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerNoClipPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = NOCLIP + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.NoClip); Assert.True(player.DefaultNoClip); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("NOCLIP + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsNoClipDefault(string bits, bool noclip)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(noclip, missile.NoClip); Assert.Equal(noclip, missile.DefaultNoClip);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool noclipDefault)
    {
        var sim = Room($"Thing 2\nBits = {(noclipDefault ? "NOCLIP + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.NoClip = !noclipDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(noclipDefault, actor.NoClip); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = NOCLIP + 6\n"); var actor = sim.Actors.Single();
        actor.NoClip = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(347, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultNoClip = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.NoClip); Assert.True(actor.DefaultNoClip); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.NoClip);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultNoClip = actor.NoClip = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultNoClip); Assert.False(actor.NoClip);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = NOCLIP + 6\n"); var actor = sim.Actors.Single(); actor.NoClip = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultNoClip = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedPrimaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = NOCLIP + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultNoClip);
        Assert.False(Start(replaced).Actors.Single().DefaultNoClip);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = NOCLIP + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-noclipdefaults-value", error);
    }

    [Theory]
    [InlineData("NOCLIP+NOGRAVITY", true)]
    [InlineData("4608", true)]
    [InlineData("NOGRAVITY", false)]
    public void PatchedMissilePassesActorsWithoutDamageAfterSave(string bits, bool passes)
    {
        var sim = Room($"Thing 35\nBits = {bits}\n"); var owner = sim.Actors.Single();
        owner.SetXYZ(-200, 0, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); missile.NoClip = !passes;
        SimSavegame.Apply(sim, saved); missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.Equal(!passes, missile.Destroyed); Assert.Equal(passes ? 1000 : 993, target.Health);
    }

    [Fact]
    public void SecondaryOnlyReplacementKeepsPrimaryNoClipDefault()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = NOCLIP+6\n");
        var next = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", original); Assert.Empty(next.Errors);
        var actor = Start(next).Actors.Single(); Assert.True(actor.NoClip); Assert.True(actor.DefaultNoClip);
        Assert.True(actor.Solid); Assert.True(actor.Shootable);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
