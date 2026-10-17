using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedBlastedDefaultsTests
{
    [Theory]
    [InlineData("BLASTED + 6", true, false)]
    [InlineData("BLASTED + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordBlastedDefaults(string bits, bool blasted, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(blasted, actor.Blasted); Assert.Equal(blasted, actor.DefaultBlasted);
        Assert.Equal(blasted, bot.Blasted); Assert.Equal(blasted, bot.DefaultBlasted);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerBlastedPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = BLASTED + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Blasted); Assert.True(player.DefaultBlasted); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("BLASTED + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsBlastedDefault(string bits, bool blasted)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(blasted, missile.Blasted); Assert.Equal(blasted, missile.DefaultBlasted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool blastedDefault)
    {
        var sim = Room($"Thing 2\nBits = {(blastedDefault ? "BLASTED + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Blasted = !blastedDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(blastedDefault, actor.Blasted); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = BLASTED + 6\n"); var actor = sim.Actors.Single();
        actor.Blasted = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(344, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultBlasted = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Blasted); Assert.True(actor.DefaultBlasted); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Blasted);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultBlasted = actor.Blasted = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultBlasted); Assert.False(actor.Blasted);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = BLASTED + 6\n"); var actor = sim.Actors.Single(); actor.Blasted = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultBlasted = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = BLASTED + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultBlasted);
        Assert.False(Start(replaced).Actors.Single().DefaultBlasted);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = BLASTED + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-blasteddefaults-value", error);
    }

    [Theory]
    [InlineData("BLASTED", 1)]
    [InlineData("PASSMOBJ", 0)]
    public void PatchedFlagControlsDropoffAfterSave(string bits, int sector)
    {
        var geometry = GameplayFoundationTests.TwoRooms(-48, 128).Level;
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides,
        }, dehacked: patch);
        var actor = sim.AddBot(-1, 80); actor.Brain = null;
        actor.AllowDropOff = false; actor.NoDropOff = true; actor.VelocityX = Fixed.FromInt(4);
        var saved = SimSavegame.Write(sim, true); actor.Blasted = !actor.Blasted;
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(sector, actor.SectorIndex);
    }

    [Fact]
    public void RestClearsRuntimeFlagButKeepsSavedRevivalDefault()
    {
        var sim = Room("Thing 2\nBits = BLASTED\n"); var actor = sim.Actors.Single(); actor.Brain = null;
        sim.Tick(); Assert.False(actor.Blasted); Assert.True(actor.DefaultBlasted);
        var saved = SimSavegame.Write(sim, true); actor.DefaultBlasted = false;
        SimSavegame.Apply(sim, saved); Assert.False(actor.Blasted); Assert.True(actor.DefaultBlasted);
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Blasted);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
