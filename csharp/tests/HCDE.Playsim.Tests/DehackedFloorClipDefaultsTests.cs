using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloorClipDefaultsTests
{
    [Theory]
    [InlineData("FLOORCLIP + 6", true, false)]
    [InlineData("FLOORCLIP + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordFloorClipEnabledDefaults(string bits, bool clip, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(clip, actor.FloorClipEnabled); Assert.Equal(clip, actor.DefaultFloorClip);
        Assert.Equal(clip, bot.FloorClipEnabled); Assert.Equal(clip, bot.DefaultFloorClip);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerFloorClipEnabledPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = FLOORCLIP + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.FloorClipEnabled); Assert.True(player.DefaultFloorClip); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("FLOORCLIP + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsFloorClipEnabledDefault(string bits, bool clip)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(clip, missile.FloorClipEnabled); Assert.Equal(clip, missile.DefaultFloorClip);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool clipDefault)
    {
        var sim = Room($"Thing 2\nBits = {(clipDefault ? "FLOORCLIP + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.FloorClipEnabled = !clipDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(clipDefault, actor.FloorClipEnabled); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = FLOORCLIP + 6\n"); var actor = sim.Actors.Single();
        actor.FloorClipEnabled = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(343, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultFloorClip = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.FloorClipEnabled); Assert.True(actor.DefaultFloorClip); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.FloorClipEnabled);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultFloorClip = actor.FloorClipEnabled = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultFloorClip); Assert.False(actor.FloorClipEnabled);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = FLOORCLIP + 6\n"); var actor = sim.Actors.Single(); actor.FloorClipEnabled = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultFloorClip = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = FLOORCLIP + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultFloorClip);
        Assert.False(Start(replaced).Actors.Single().DefaultFloorClip);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = FLOORCLIP + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-floorclipdefaults-value", error);
    }

    [Theory]
    [InlineData("FLOORCLIP", 0, 8)]
    [InlineData("FLOORCLIP", 16, 0)]
    [InlineData("PASSMOBJ", 0, 0)]
    public void PatchedFlagControlsTerrainClipAfterSave(string bits, int height, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)], DefaultTerrain = "Water",
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
        }, dehacked: patch);
        var actor = sim.Actors.Single(); actor.Brain = null;
        actor.Z = Fixed.FromInt(height); actor.NoGravity = true;
        var saved = SimSavegame.Write(sim, true); actor.FloorClipEnabled = !actor.FloorClipEnabled;
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.FloorClip);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
