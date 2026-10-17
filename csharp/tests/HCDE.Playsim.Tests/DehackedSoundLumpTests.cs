using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSoundLumpTests
{
    [Theory]
    [InlineData("PISTOL", "dsPISTOL")]
    [InlineData("DSPISTOL", "dsDSPISTOL")]
    [InlineData("custom", "dscustom")]
    public void RenamedResourceResolvesActualProjectileRequestAndFreshSave(string suffix, string lump)
    {
        var patch = DehackedPatch.Apply($"Thing 34\nAlert sound = 1\n\n[SOUNDS]\n1 = {suffix}\n");
        Assert.Empty(patch.Errors);
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["weapons/pistol"] = "OLD" } };
        var sim = AuthoritySimulation.Start(level, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal("weapons/pistol", Assert.Single(sim.DrainSoundRequests()).Sound);
        Assert.True(SoundResourceResolver.TryResolve(sim.Level, missile.ActorSounds[ActorSoundType.See], out var resource, out var error));
        Assert.Null(error); Assert.Equal(lump, resource);
        Assert.Equal("OLD", level.SoundDefinitions["weapons/pistol"]);
        var saved = SimSavegame.Write(sim, true);
        var fresh = AuthoritySimulation.Start(level, dehacked: patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.True(SoundResourceResolver.TryResolve(fresh.Level, "weapons/pistol", out resource, out error));
        Assert.Equal(lump, resource); Assert.Null(error);
    }

    [Theory]
    [InlineData("37 = FIRST\n36 = LAST", "dsLAST")]
    [InlineData("36 = FIRST\n37 = LAST", "dsLAST")]
    [InlineData("36 trailing = PREFIX", "dsPREFIX")]
    public void DuplicateNativeSoundIdsFollowInputOrder(string body, string expected)
    {
        var patch = DehackedPatch.Apply($"[SOUNDS]\n{body}\n");
        Assert.Equal(expected, patch.SoundLumpOverrides["grunt/sight"]);
        var layered = DehackedPatch.Apply("[SOUNDS]\n38 = LAYER\n", patch);
        Assert.Equal("dsLAYER", layered.SoundLumpOverrides["grunt/sight"]);
        Assert.Equal(expected, patch.SoundLumpOverrides["grunt/sight"]);
        Assert.Equal("dsLAYER", DehackedPatch.Apply("Thing 2\nMass = 200\n", layered).SoundLumpOverrides["grunt/sight"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(114)]
    [InlineData(499)]
    [InlineData(700)]
    public void InvalidOrPaddingIdsDoNotCreateResources(int id)
    {
        var patch = DehackedPatch.Apply($"[SOUNDS]\n{id} = INVALID\n");
        Assert.Empty(patch.SoundLumpOverrides);
    }

    [Fact]
    public void ExtraSoundRenameClearsRandomAndTentativeStatusButPreservesSettings()
    {
        var patch = DehackedPatch.Apply("[SOUNDS]\n500 = EXTRA\n");
        var level = new PlayLevel {
            RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["dehextra/sound000"] = ["other"] },
            SoundSettings = new Dictionary<string, SndInfoSoundSettings> {
                ["dehextra/sound000"] = new(Volume: 0.5f, IsTentative: true) } };
        var sim = AuthoritySimulation.Start(level, dehacked: patch);
        Assert.Empty(sim.Level.RandomSoundGroups);
        Assert.True(sim.Level.SoundSettings.TryGetValue("dehextra/sound000", out var setting));
        Assert.False(setting.IsTentative); Assert.Equal(0.5f, setting.Volume);
        Assert.True(SoundResourceResolver.TryResolve(sim.Level, "dehextra/sound000", out var resource, out var error));
        Assert.Equal("dsEXTRA", resource); Assert.Null(error);
        Assert.Single(level.RandomSoundGroups); Assert.True(level.SoundSettings["dehextra/sound000"].IsTentative);
    }
}
