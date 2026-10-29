namespace HCDE.Playsim.Tests;

public class VmPMobjState1152SliceTests
{
    [Fact]
    public void GetTics_HalvesFastStatesBeforeDoublingSlowOnes()
    {
        Assert.Equal(3, VmPMobjState1152Slice.GetTics(false, 5, 9, true, true, true, true));
        Assert.Equal(10, VmPMobjState1152Slice.GetTics(false, 5, 9, false, true, true, true));
        Assert.Equal(9, VmPMobjState1152Slice.GetTics(true, 5, 9, false, false, false, false));
        Assert.Equal(5, VmPMobjState1152Slice.GetTics(false, 5, 9, true, false, true, false));
    }

    [Fact]
    public void SetState_WalksZeroTicsAndKeepsASkinSprite()
    {
        var dead = VmPMobjState1152Slice.SetState(Array.Empty<VmPMobjState1152Slice.ActorFrame>(), -1, false, false, false, false, false, 0, 3, 1, 0, 0, 0, 4, false, 0, 0, 0, 0, 0);
        Assert.False(dead.Alive);
        Assert.True(dead.Destroyed);

        var unusable = One(useFlags: 0);
        Assert.False(unusable.Alive);

        var held = One(sprite: VmPMobjState1152Slice.SprFixed, frame: 9, sameFrame: false, tics: 4, fullBright: false, renderFlags: 0x11);
        Assert.Equal(3, held.Sprite);
        Assert.Equal(1, held.Frame);
        Assert.Equal(0x01, held.RenderFlags);
        Assert.Equal(4, held.Tics);

        var bright = One(sprite: 8, frame: 2, tics: 4, fullBright: true, renderFlags: 0x01);
        Assert.Equal(8, bright.Sprite);
        Assert.Equal(2, bright.Frame);
        Assert.Equal(0x11, bright.RenderFlags);

        var skin = One(sprite: 4, spawnSprite: 4, hasPlayer: true, skinCount: 2, skinSprite: 20, tics: 3);
        Assert.Equal(20, skin.Sprite);
        var body = One(sprite: 4, spawnSprite: 4, hasCurrent: true, currentSprite: 4, tics: 3, actorSprite: 7);
        Assert.Equal(7, body.Sprite);
        var stripped = One(sprite: 4, spawnSprite: 4, flags4: VmPMobjState1152Slice.NoSkin, tics: 3);
        Assert.Equal(4, stripped.Sprite);

        var chain = VmPMobjState1152Slice.SetState(new[]
        {
            Frame(sprite: VmPMobjState1152Slice.SprNoChange, tics: 0, next: 1),
            Frame(sprite: VmPMobjState1152Slice.SprNoChange, frame: 5, tics: 6, sameFrame: false),
        }, 0, false, false, false, false, true, 6, 2, 1, 0, 0, 0, 9, false, 0, 0, 0, 0, 0);
        Assert.True(chain.Alive);
        Assert.Equal(2, chain.Sprite);
        Assert.Equal(5, chain.Frame);
        Assert.Equal(6, chain.Tics);

        var jumped = VmPMobjState1152Slice.SetState(new[]
        {
            Frame(tics: 8, calls: true, jump: 1),
            Frame(sprite: 9, tics: 2),
        }, 0, false, false, false, false, false, 0, 1, 0, 0, 0, 0, 3, false, 0, 0, 0, 0, 0);
        Assert.Equal(9, jumped.Sprite);
        Assert.Equal(2, jumped.Tics);

        var spared = One(calls: true, euthanize: true, noFunction: true, tics: 2);
        Assert.True(spared.Alive);
        var removed = One(calls: true, euthanize: true, tics: 2);
        Assert.False(removed.Alive);

        var lights = One(tics: 2, oldLight: 1);
        Assert.Equal(VmPMobjState1152Slice.RecreateLights, lights.Flags8);
        Assert.Equal(VmPMobjState1152Slice.LightCreated, lights.LevelFlags3);

        var loop = VmPMobjState1152Slice.SetState(new[]
        {
            Frame(tics: 0, calls: true, jump: 0),
        }, 0, false, false, false, false, false, 0, 1, 0, 0, 0, 0, 3, false, 0, 0, 0, 0, 0);
        Assert.False(loop.Alive);
    }

    [Fact]
    public void Inventory_DestroysSurvivorsAndUsesOnlyThroughTheVm()
    {
        var gone = VmPMobjState1152Slice.DestroyInventory(new[] { (1, false), (2, true), (3, false) });
        Assert.Equal(new[] { 1, 3 }, gone);
        Assert.Empty(VmPMobjState1152Slice.DestroyInventory(Array.Empty<(int, bool)>()));
        Assert.False(VmPMobjState1152Slice.UseInventory(false, true));
        Assert.False(VmPMobjState1152Slice.UseInventory(true, false));
        Assert.True(VmPMobjState1152Slice.UseInventory(true, true));
    }

    private static VmPMobjState1152Slice.ActorFrame Frame(
        int sprite = 4,
        int frame = 1,
        int tics = 1,
        int clientTics = 1,
        bool fast = false,
        bool slow = false,
        bool sameFrame = false,
        bool fullBright = false,
        int useFlags = VmPMobjState1152Slice.ActorUse,
        int light = 0,
        int next = -1,
        bool calls = false,
        int jump = -1,
        bool euthanize = false)
    {
        return new VmPMobjState1152Slice.ActorFrame(
            sprite, frame, tics, clientTics, fast, slow, sameFrame, fullBright, useFlags, light, next, calls, jump, euthanize);
    }

    private static VmPMobjState1152Slice.StateStep One(
        int sprite = 4,
        int frame = 1,
        int tics = 1,
        bool sameFrame = false,
        bool fullBright = false,
        int useFlags = VmPMobjState1152Slice.ActorUse,
        int renderFlags = 0,
        int spawnSprite = 8,
        bool hasPlayer = false,
        int skinCount = 0,
        int skinSprite = 0,
        bool hasCurrent = false,
        int currentSprite = 0,
        int flags4 = 0,
        int actorSprite = 3,
        bool calls = false,
        bool euthanize = false,
        bool noFunction = false,
        int oldLight = 0)
    {
        return VmPMobjState1152Slice.SetState(
            new[] { Frame(sprite, frame, tics, tics, false, false, sameFrame, fullBright, useFlags, 0, -1, calls, -1, euthanize) },
            0, noFunction, false, false, false, hasCurrent, currentSprite, actorSprite, 1, renderFlags, flags4, 0, spawnSprite,
            hasPlayer, skinCount, skinSprite, 0, oldLight, 0);
    }
}
