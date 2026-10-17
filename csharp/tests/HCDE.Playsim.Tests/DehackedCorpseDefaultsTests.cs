using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCorpseDefaultsTests
{
    [Theory]
    [InlineData("CORPSE + 6", true, false)]
    [InlineData("CORPSE + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordCorpseDefaults(string bits, bool corpse, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(corpse, actor.Corpse); Assert.Equal(corpse, actor.DefaultCorpse);
        Assert.Equal(corpse, bot.Corpse); Assert.Equal(corpse, bot.DefaultCorpse);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerCorpsePatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = CORPSE + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Corpse); Assert.True(player.DefaultCorpse); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("CORPSE + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsCorpseDefault(string bits, bool corpse)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(corpse, missile.Corpse); Assert.Equal(corpse, missile.DefaultCorpse);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool corpseDefault)
    {
        var sim = Room($"Thing 2\nBits = {(corpseDefault ? "CORPSE + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Corpse = !corpseDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(corpseDefault, actor.Corpse); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = CORPSE + 6\n"); var actor = sim.Actors.Single();
        actor.Health = 0; actor.Corpse = false; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(350, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultCorpse = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Corpse); Assert.True(actor.DefaultCorpse); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Corpse);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultCorpse = actor.Corpse = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultCorpse); Assert.False(actor.Corpse);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = CORPSE + 6\n"); var actor = sim.Actors.Single(); actor.Corpse = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultCorpse = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedPrimaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = CORPSE + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultCorpse);
        Assert.False(Start(replaced).Actors.Single().DefaultCorpse);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = CORPSE + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-corpsedefaults-value", error);
    }

    [Theory]
    [InlineData("CORPSE+DROPOFF+6", true)]
    [InlineData("1049606", true)]
    [InlineData("DROPOFF+6", false)]
    public void PatchedCorpseFlagControlsLedgeFrictionWithoutKillingActor(string bits, bool slides)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-24, 128).Level;
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides }, dehacked: patch);
        var actor = sim.AddBot(1, 80); actor.Brain = null;
        actor.Z = default; actor.SectorIndex = 1; actor.OnGround = true; actor.VelocityX = Fixed.FromDouble(0.5);
        Assert.True(actor.Health > 0); Assert.False(actor.IsDead); Assert.Equal(slides, actor.Corpse);
        var saved = SimSavegame.Write(sim, true); actor.Corpse = !slides;
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, actor);
        Assert.Equal(Fixed.FromDouble(0.5 * (slides ? 1 : ActorPhysics.GroundFriction)), actor.VelocityX);
        Assert.True(actor.Health > 0);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
