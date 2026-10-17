using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloorPicDefaultsTests
{
    [Theory]
    [InlineData("CANTLEAVEFLOORPIC + 6", true, false)]
    [InlineData("CANTLEAVEFLOORPIC + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordCantLeaveFloorPicDefaults(string bits, bool restricted, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(restricted, actor.CantLeaveFloorPic); Assert.Equal(restricted, actor.DefaultFloorPic);
        Assert.Equal(restricted, bot.CantLeaveFloorPic); Assert.Equal(restricted, bot.DefaultFloorPic);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerCantLeaveFloorPicPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = CANTLEAVEFLOORPIC + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.CantLeaveFloorPic); Assert.True(player.DefaultFloorPic); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("CANTLEAVEFLOORPIC + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsCantLeaveFloorPicDefault(string bits, bool restricted)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(restricted, missile.CantLeaveFloorPic); Assert.Equal(restricted, missile.DefaultFloorPic);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool restrictedDefault)
    {
        var sim = Room($"Thing 2\nBits = {(restrictedDefault ? "CANTLEAVEFLOORPIC + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.CantLeaveFloorPic = !restrictedDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(restrictedDefault, actor.CantLeaveFloorPic); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = CANTLEAVEFLOORPIC + 6\n"); var actor = sim.Actors.Single();
        actor.CantLeaveFloorPic = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(339, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultFloorPic = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.CantLeaveFloorPic); Assert.True(actor.DefaultFloorPic); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.CantLeaveFloorPic);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultFloorPic = actor.CantLeaveFloorPic = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultFloorPic); Assert.False(actor.CantLeaveFloorPic);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = CANTLEAVEFLOORPIC + 6\n"); var actor = sim.Actors.Single(); actor.CantLeaveFloorPic = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultFloorPic = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = CANTLEAVEFLOORPIC + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultFloorPic);
        Assert.False(Start(replaced).Actors.Single().DefaultFloorPic);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = CANTLEAVEFLOORPIC + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-floorpicdefaults-value", error);
    }

    [Theory]
    [InlineData("CANTLEAVEFLOORPIC", "OTHER", 0, false)]
    [InlineData("CANTLEAVEFLOORPIC", "FLOOR", 0, true)]
    [InlineData("CANTLEAVEFLOORPIC", "FLOOR", 8, false)]
    [InlineData("PASSMOBJ", "OTHER", 0, true)]
    public void PatchedDefaultControlsCrossSectorMovementAfterSave(string bits, string texture, short floor, bool allowed)
    {
        var geometry = GameplayFoundationTests.TwoRooms(floor, 128).Level;
        geometry.Sectors[0].FloorPic = "FLOOR"; geometry.Sectors[1].FloorPic = texture;
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides,
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: patch);
        var actor = sim.Actors.Single();
        var saved = SimSavegame.Write(sim, true); actor.CantLeaveFloorPic = !actor.CantLeaveFloorPic;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(allowed, ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
