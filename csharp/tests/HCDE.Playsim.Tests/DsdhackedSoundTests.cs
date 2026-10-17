using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DsdhackedSoundTests
{
    private const string Header = "Patch File for DeHackEd v3.0\nDoom version = 2021\nPatch format = 6\n\n";

    [Theory]
    [InlineData(114, "~dsdhacked/#113")]
    [InlineData(499, "~dsdhacked/#498")]
    [InlineData(700, "~dsdhacked/#699")]
    [InlineData(2147483647, "~dsdhacked/#2147483646")]
    public void UnknownPositiveIdsBecomeTentativeAndSurviveFreshSave(int id, string expected)
    {
        var patch = DehackedPatch.Apply(Header + $"Thing 34\nAlert sound = {id}\n");
        Assert.Empty(patch.Errors); Assert.True(patch.DsdhackedSoundMode); Assert.Contains(expected, patch.TentativeSoundNames);
        var sim = Room(patch); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()).Sound);
        Assert.True(sim.Level.SoundSettings[expected].IsTentative);
        Assert.True(SoundResourceResolver.TryResolve(sim.Level, expected, out _, out var error)); Assert.Null(error);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        var loaded = Assert.IsType<ProjectileActor>(fresh.Actors.Single(a => a.Id == missile.Id));
        Assert.Equal(expected, loaded.ActorSounds[ActorSoundType.See]);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Assert.True(fresh.Level.SoundSettings[expected].IsTentative);
    }

    [Theory]
    [InlineData(1, "weapons/pistol")]
    [InlineData(500, "dehextra/sound000")]
    [InlineData(699, "dehextra/sound199")]
    [InlineData(0, "")]
    [InlineData(-1, "")]
    public void KnownIdsAndNoSoundKeepExistingMeaning(int id, string expected)
    {
        var patch = DehackedPatch.Apply(Header + $"Thing 34\nAlert sound = {id}\n");
        Assert.Empty(patch.Errors); Assert.Empty(patch.TentativeSoundNames);
        Assert.Equal(expected, patch.Actors.Single(a => a.Index == 34).SoundNameOverrides[0]);
    }

    [Fact]
    public void MaximumNativeLookupIndexDoesNotOverflowTentativeName()
    {
        var patch = DehackedPatch.Apply(Header + "Thing 34\nAlert sound = 2147483648\n");
        Assert.Empty(patch.Errors);
        Assert.Equal("~dsdhacked/#2147483647", patch.Actors.Single(a => a.Index == 34).SoundNameOverrides[0]);
    }

    [Fact]
    public void LaterBexRenameMaterializesInheritedTentativeSoundWithoutChangingBaseline()
    {
        var original = DehackedPatch.Apply(Header + "Thing 34\nAlert sound = 700\n");
        var patched = DehackedPatch.Apply("[SOUNDS]\n700 = EXTENDED\n", original);
        var sim = Room(patched);
        Assert.False(sim.Level.SoundSettings["~dsdhacked/#699"].IsTentative);
        Assert.True(SoundResourceResolver.TryResolve(sim.Level, "~dsdhacked/#699", out var resource, out var error));
        Assert.Equal("dsEXTENDED", resource); Assert.Null(error);
        Assert.Empty(original.SoundLumpOverrides); Assert.True(Room(original).Level.SoundSettings["~dsdhacked/#699"].IsTentative);
        Assert.False(DehackedPatch.Apply("Thing 34\nAlert sound = 700\n").DsdhackedSoundMode);
    }

    [Fact]
    public void RenameBeforeThingAssignmentProducesSameResolvedResource()
    {
        var patch = DehackedPatch.Apply(Header + "[SOUNDS]\n700 = BEFORE\n\nThing 34\nAlert sound = 700\n");
        var sim = Room(patch);
        Assert.True(SoundResourceResolver.TryResolve(sim.Level, "~dsdhacked/#699", out var resource, out var error));
        Assert.Equal("dsBEFORE", resource); Assert.Null(error); Assert.False(sim.Level.SoundSettings["~dsdhacked/#699"].IsTentative);
    }

    [Theory]
    [InlineData("Doom version = 2021\nThing 34\nAlert sound = 700\n")]
    [InlineData("Patch File for DeHackEd v3.0\nDoom version = 19\nPatch format = 6\nThing 34\nAlert sound = 700\n")]
    public void OrdinaryPatchesDoNotAllocateExtendedSounds(string text)
    {
        var patch = DehackedPatch.Apply(text);
        Assert.False(patch.DsdhackedSoundMode); Assert.Empty(patch.TentativeSoundNames);
        Assert.Equal("", patch.Actors.Single(a => a.Index == 34).SoundNameOverrides[0]);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
}
