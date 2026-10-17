using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDontDrawTests
{
    [Theory]
    [InlineData("DONTDRAW", 1, false)]
    [InlineData("dontdraw", 1, false)]
    [InlineData("DONTDRAW+TRANSLUC25", 0.5, false)]
    [InlineData("DONTDRAW+TRANSLUC50", 0.75, false)]
    [InlineData("DONTDRAW+TRANSLUC75", 1, true)]
    public void NamedRenderBitMatchesNativeHistoricalInterpretation(string bits, double alpha, bool invisible)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\nThing 34\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = Start(patch); var map = sim.Actors.Single(); var dynamic = sim.AddBot(200, 0);
        var missile = sim.SpawnProjectile(map, ProjectileKind.Rocket);
        foreach (var actor in new Actor[] { map, dynamic, missile })
        {
            Assert.Equal(alpha, actor.Alpha); Assert.Equal(invisible, actor.Invisible);
        }
        var saved = SimSavegame.Write(sim, true); map.Invisible = !invisible;
        SimSavegame.Apply(sim, saved); Assert.Equal(invisible, map.Invisible);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RenderOnlyDontDrawClearsInheritedInvisibilityAndPreservesOtherFlagSets()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE+PUSHABLE+TRANSLUC75\n");
        var patch = DehackedPatch.Apply("Thing 2\nBits = DONTDRAW\n", baseline); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == 2);
        Assert.Equal(4u, definition.Bits); Assert.Equal(0x200u, definition.Bits2); Assert.False(definition.Invisible);
        var actor = Start(patch).Actors.Single(); Assert.True(actor.Shootable); Assert.True(actor.Pushable); Assert.False(actor.Invisible);
        Assert.True(Start(baseline).Actors.Single().Invisible);
    }

    [Fact]
    public void DontDrawDoesNotChangeInheritedAlphaOrStyle()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nRender Style = STYLE_Add\nTranslucency = 32768\n");
        var patch = DehackedPatch.Apply("Thing 2\nBits = DONTDRAW\n", baseline); Assert.Empty(patch.Errors);
        var before = Start(baseline).Actors.Single(); var after = Start(patch).Actors.Single();
        Assert.Equal(before.Alpha, after.Alpha); Assert.Equal(before.RenderStyle, after.RenderStyle); Assert.False(after.Invisible);
    }

    private static AuthoritySimulation Start(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
}
