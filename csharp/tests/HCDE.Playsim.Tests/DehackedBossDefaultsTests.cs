using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedBossDefaultsTests
{
    [Theory]
    [InlineData(2, 3004, "BOSS", true)]
    [InlineData(2, 3004, "BOSS+PASSMOBJ", true)]
    [InlineData(2, 3004, "PASSMOBJ", false)]
    [InlineData(22, 16, "PASSMOBJ", false)]
    [InlineData(22, 16, "BOSS", true)]
    public void MapAndDynamicActorsApplyBossDefaultAndRevive(int thing, int type, string bits, bool boss)
    {
        var patch = Patch($"Thing {thing}\nBits = {bits}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = (ushort)type }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var map = sim.Actors.Single(); var dynamic = sim.AddBot(300, 0, type);
        Assert.Equal(boss, map.Boss); Assert.Equal(boss, dynamic.Boss);
        var saved = SimSavegame.Write(sim, true); map.Boss = dynamic.Boss = !boss;
        SimSavegame.Apply(sim, saved); Assert.Equal(boss, map.Boss); Assert.Equal(boss, dynamic.Boss);
        foreach (var actor in new[] { map, dynamic })
        {
            actor.Boss = !boss; actor.Health = 0; ActorRaise.ReviveSupported(actor);
            Assert.Equal(boss, actor.Boss);
        }
    }

    [Theory]
    [InlineData("BOSS", true)]
    [InlineData("PASSMOBJ", false)]
    public void ProjectileBossDefaultSurvivesSaveAndRevival(string bits, bool boss)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: Patch($"Thing 35\nBits = {bits}\n"));
        var missile = sim.SpawnProjectile(sim.AddBot(0, 0), ProjectileKind.Plasma);
        Assert.Equal(boss, missile.Boss);
        var saved = SimSavegame.Write(sim, true); missile.Boss = !boss;
        SimSavegame.Apply(sim, saved); Assert.Equal(boss, missile.Boss);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        missile.Boss = !boss; ActorRaise.ReviveSupported(missile); Assert.Equal(boss, missile.Boss);
    }

    [Theory]
    [InlineData("BOSS", true, true)]
    [InlineData("BOSS", false, false)]
    [InlineData("PASSMOBJ", true, false)]
    public void PatchedBossControlsNoBossRipContact(string bits, bool noBossRip, bool stopped)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            dehacked: Patch($"Thing 12\nBits = {bits}\n"));
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.Rip = true; missile.NoBossRip = noBossRip; missile.DamageExpression = _ => 7;
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stopped, missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void PrimaryOnlyPatchRetainsBossUntilSecondaryReplacement()
    {
        var original = Patch("Thing 2\nBits = BOSS\n");
        var inherited = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", inherited);
        Assert.True(Start(inherited).AddBot(0, 0).Boss);
        Assert.False(Start(replaced).AddBot(0, 0).Boss);
    }

    [Fact]
    public void UnpatchedBossClassKeepsStockClassification()
    {
        var sim = Start(Patch("Thing 22\nHit points = 3500\n"));
        Assert.True(sim.AddBot(0, 0, 16).Boss); Assert.False(sim.AddBot(300, 0).Boss);
    }

    private static AuthoritySimulation Start(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
    private static DehackedPatchResult Patch(string text)
    {
        var patch = DehackedPatch.Apply(text); Assert.Empty(patch.Errors); return patch;
    }
}
