using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class PainElementalPatchedSoulTests
{
    [Theory]
    [InlineData(3006)]
    [InlineData(4000)]
    public void ClassSpawnReceivesParsedDefaultsEvenWhenEditorIdChanges(int editorId)
    {
        var patch = DehackedPatch.Apply($"Thing 19\nID # = {editorId}\nHit points = 235\nWidth = 786432\nHeight = 2621440\nSpeed = 12\nPain chance = 45\nMass = 123\nMissile damage = 9\nReaction time = 17\n");
        Assert.Empty(patch.Errors);
        var sim = Room(patch); var parent = sim.Actors.Single(a => a.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, sim.Players.Single(), 0)!;
        Assert.NotNull(soul); Assert.Equal(editorId, soul.DoomEdNum);
        Assert.Equal(235, soul.Health); Assert.Equal(235, soul.ResurrectionHealth); Assert.Equal(-235, soul.GibHealth);
        Assert.Equal(12, soul.Radius.ToDouble()); Assert.Equal(40, soul.Height.ToDouble());
        Assert.Equal(3, soul.ChaseSpeed); Assert.Equal(45, soul.PainChance);
        Assert.Equal(123, soul.Mass); Assert.Equal(9, soul.Damage); Assert.Equal(17, soul.ReactionTime);
        Assert.True(soul.Brain!.Charging);
        Assert.True(soul.IsSameSpecies(sim.AddBot(500, 0, editorId)));
        Assert.Equal(4 + (parent.Radius.ToDouble() + 12) * 1.5, soul.X.ToDouble(), 3);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void SmallPatchedRadiusUsesNativeMovementSubdivisionFallback(int width)
    {
        var sim = Room(DehackedPatch.Apply($"Thing 19\nWidth = {width}\n"));
        var parent = sim.Actors.Single(a => a.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, null, 0);
        Assert.NotNull(soul); Assert.True(double.IsFinite(soul.X.ToDouble()));
        Assert.Equal(4 + (parent.Radius.ToDouble() + width / 65536.0) * 1.5, soul.X.ToDouble(), 3);
    }
    [Theory]
    [InlineData(3, 17)]
    [InlineData(4, 0)]
    public void NightmareOverridesPatchedReactionOnClassSpawn(int skill, int expected)
    {
        var sim = Room(DehackedPatch.Apply("Thing 19\nReaction time = 17\n"), skill);
        var soul = sim.SpawnLostSoul(sim.Actors.Single(a => a.DoomEdNum == 71), sim.Players.Single(), 0)!;
        Assert.Equal(expected, soul.ReactionTime);
    }
    [Fact]
    public void ExplicitPrimaryAndExtendedFlagsReachClassSpawn()
    {
        var patch = DehackedPatch.Apply("Thing 19\nBits = SOLID + SHOOTABLE + COUNTKILL + NOTELEPORT\n");
        Assert.Empty(patch.Errors);
        var sim = Room(patch);
        var soul = sim.SpawnLostSoul(sim.Actors.Single(a => a.DoomEdNum == 71), null, 0)!;
        Assert.True(soul.IsMonster); Assert.False(soul.NoGravity); Assert.False(soul.Floating);
        Assert.True(soul.NoTeleport); Assert.True(soul.Solid); Assert.True(soul.Shootable);
    }
    private static AuthoritySimulation Room(DehackedPatchResult patch, int skill = 2) =>
        AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 71 }, new LevelThing { Type = 1, X = 800 }] },
            dehacked: patch, spawnOptions: new SpawnOptions(Skill: skill));
}