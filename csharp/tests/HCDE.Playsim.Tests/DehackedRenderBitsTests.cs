using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedRenderBitsTests
{
    [Theory]
    [InlineData("TRANSLUC25", 0.5, false)]
    [InlineData("TRANSLUC50", 0.75, false)]
    [InlineData("TRANSLUC75", 1, true)]
    [InlineData("TRANSLUC25 + TRANSLUC50", 0.5, false)]
    [InlineData("TRANSLUC25 + TRANSLUC75", 0.5, true)]
    [InlineData("TRANSLUCENT", 0.5, false)]
    [InlineData("2147483648", 0.5, false)]
    [InlineData("-2147483648", 0.5, false)]
    public void NativeBitTableAndBoomRemapReachSpawnsAndSaveReplay(string bits, double alpha, bool invisible)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var actor = sim.Actors.Single();
        var style = alpha == 1 ? ActorRenderStyle.Normal : ActorRenderStyle.Translucent;
        Assert.Equal(style, actor.RenderStyle); Assert.Equal(alpha, actor.Alpha);
        Assert.Equal(invisible, actor.Invisible);
        Assert.Equal(!invisible, new Actor { DontSeekInvisible = true }.CanSeek(actor));
        Assert.Equal(0u, patch.Actors[1].Bits & 0x80000000u);
        var bytes = SimSavegame.Write(sim);
        actor.Invisible = !invisible; actor.Alpha = 0; actor.RenderStyle = ActorRenderStyle.OptFuzzy;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(invisible, actor.Invisible); Assert.Equal(alpha, actor.Alpha); Assert.Equal(style, actor.RenderStyle);
        var dynamic = sim.AddBot(500, 0, 3004);
        Assert.Equal(invisible, dynamic.Invisible); Assert.Equal(alpha, dynamic.Alpha); Assert.Equal(style, dynamic.RenderStyle);
    }

    [Theory]
    [InlineData("Bits = SHADOW + TRANSLUC50\n", true, 0.75)]
    [InlineData("Bits = SHADOW + TRANSLUC50\nRender Style = STYLE_Add\n", false, 0.75)]
    [InlineData("Render Style = STYLE_Add\nBits = SHADOW + TRANSLUC50\n", false, 0.75)]
    public void ExtendedBitsPreserveNativeSectionStylePrecedence(string body, bool fuzzy, double alpha)
    {
        var patch = DehackedPatch.Apply("Thing 2\n" + body); Assert.Empty(patch.Errors);
        var actor = Room(patch).Actors.Single();
        // Extended bits mark translucency, but do not mark an explicit style.
        var style = fuzzy ? ActorRenderStyle.OptFuzzy : body.StartsWith("Render Style")
            ? ActorRenderStyle.Translucent : ActorRenderStyle.FromDehacked(DehackedRenderStyle.Add);
        Assert.Equal(style, actor.RenderStyle);
        // A later Bits field overwrites an earlier explicit style before section fallback.
        Assert.Equal(alpha, actor.Alpha);
    }

    [Fact]
    public void OnlyRenderGroupUpdatesClearInvisibleAndLeaveOtherFlagsAlone()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + TRANSLUC75\n");
        var flagsOnly = DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE\n", baseline);
        Assert.True(Room(flagsOnly).Actors.Single().Invisible);
        var renderOnly = DehackedPatch.Apply("Thing 2\nBits = TRANSLUC25\n", baseline);
        var actor = Room(renderOnly).Actors.Single();
        Assert.False(actor.Invisible); Assert.True(actor.Solid); Assert.True(actor.Shootable);
        Assert.True(Room(baseline).Actors.Single().Invisible);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
