using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedThruGhostDefaultsTests
{
    [Theory]
    [InlineData("THRUGHOST + 6", true, false)]
    [InlineData("THRUGHOST + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordThruGhostDefaults(string bits, bool thruGhost, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(thruGhost, actor.ThruGhost); Assert.Equal(thruGhost, actor.DefaultThruGhost);
        Assert.Equal(thruGhost, bot.ThruGhost); Assert.Equal(thruGhost, bot.DefaultThruGhost);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerThruGhostPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = THRUGHOST + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.ThruGhost); Assert.True(player.DefaultThruGhost); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("THRUGHOST + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsThruGhostDefault(string bits, bool thruGhost)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(thruGhost, missile.ThruGhost); Assert.Equal(thruGhost, missile.DefaultThruGhost);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool thruGhostDefault)
    {
        var sim = Room($"Thing 2\nBits = {(thruGhostDefault ? "THRUGHOST + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.ThruGhost = !thruGhostDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(thruGhostDefault, actor.ThruGhost); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = THRUGHOST + 6\n"); var actor = sim.Actors.Single();
        actor.ThruGhost = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(337, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultThruGhost = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.ThruGhost); Assert.True(actor.DefaultThruGhost); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.ThruGhost);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultThruGhost = actor.ThruGhost = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultThruGhost); Assert.False(actor.ThruGhost);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = THRUGHOST + 6\n"); var actor = sim.Actors.Single(); actor.ThruGhost = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultThruGhost = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = THRUGHOST + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultThruGhost);
        Assert.False(Start(replaced).Actors.Single().DefaultThruGhost);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = THRUGHOST + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-thrughostdefaults-value", error);
    }

    [Theory]
    [InlineData("THRUGHOST", true, true)]
    [InlineData("THRUGHOST", false, false)]
    [InlineData("PASSMOBJ", true, false)]
    [InlineData("PASSMOBJ", false, false)]
    public void PatchedMissilePassesOnlyGhostTargetsAfterSave(string bits, bool ghost, bool passes)
    {
        var sim = Room($"Thing 35\nBits = {bits}\n"); var owner = sim.Actors.Single();
        owner.SetXYZ(-200, 0, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000;
        target.NoPain = true; target.Ghost = ghost;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); missile.ThruGhost = !missile.ThruGhost;
        SimSavegame.Apply(sim, saved); missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.Equal(!passes, missile.Destroyed);
        Assert.Equal(passes ? 1000 : 993, target.Health);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
