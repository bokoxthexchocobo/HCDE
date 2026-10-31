namespace HCDE.Client.Tests;

public class VmAmMapKeysThings1141SliceTests
{
    [Fact]
    public void Keys_FollowTheReplaceeAndUseAZeroScaleMark()
    {
        Assert.True(VmAmMapKeysThings1141Slice.AcceptKey(true, true, false));
        Assert.False(VmAmMapKeysThings1141Slice.AcceptKey(true, false, true));
        Assert.False(VmAmMapKeysThings1141Slice.AcceptKey(false, true, false));
        Assert.Equal(100d, VmAmMapKeysThings1141Slice.KeyAngle(10, true, 0));
        Assert.Equal(10d, VmAmMapKeysThings1141Slice.KeyAngle(10, false, 40));
        var colored = VmAmMapKeysThings1141Slice.KeyMark(VmAmMapKeysThings1141Slice.Special, 0x040506);
        Assert.True(colored.Draw);
        Assert.Equal((byte)4, colored.R);
        Assert.Equal(0d, colored.Scale);
        var counted = VmAmMapKeysThings1141Slice.KeyMark(VmAmMapKeysThings1141Slice.Special, -1);
        Assert.True(counted.Draw);
        Assert.False(counted.FromMap);
        Assert.False(VmAmMapKeysThings1141Slice.KeyMark(0, 0x040506).Draw);
    }

    [Fact]
    public void Things_AreVisibleForCheatsOrWhenTheScannerFlagIsClear()
    {
        Assert.True(VmAmMapKeysThings1141Slice.ShowThing(1, VmAmMapKeysThings1141Slice.NotOnAutomap, false, 0));
        Assert.True(VmAmMapKeysThings1141Slice.ShowThing(0, 0, false, VmAmMapKeysThings1141Slice.Invisible));
        Assert.False(VmAmMapKeysThings1141Slice.ShowThing(0, VmAmMapKeysThings1141Slice.NotOnAutomap, true, 0));
        Assert.Equal((12d, 9d), VmAmMapKeysThings1141Slice.ThingPoint(1, 2, 3, 4, 8, 3));
        Assert.Equal(new[] { 3, 2, 1 }, VmAmMapKeysThings1141Slice.SpritePasses(3));
        Assert.Equal(10, VmAmMapKeysThings1141Slice.SpriteFrameBase(10, 1, 4));
        Assert.Equal(14, VmAmMapKeysThings1141Slice.SpriteFrameBase(10, 2, 4));
    }

    [Fact]
    public void SpriteAngle_AddsElevenPointTwoFiveWhenTheSideTexturesDiffer()
    {
        Assert.Equal(270d - 20d - 5d, VmAmMapKeysThings1141Slice.SpriteAngle(20, 5, false, false, 0));
        Assert.Equal(270d - 20d - 5d + 11.25, VmAmMapKeysThings1141Slice.SpriteAngle(20, 5, true, false, 0));
        Assert.Equal(270d - 20d + 11.25 + (40d - 90d), VmAmMapKeysThings1141Slice.SpriteAngle(20, 0, true, true, 40));
        Assert.Equal(1, VmAmMapKeysThings1141Slice.RotationIndex(22.5));
        Assert.Equal(0, VmAmMapKeysThings1141Slice.RotationIndex(11.25));
        Assert.Equal(3, VmAmMapKeysThings1141Slice.TextureSlot(3, 3));
        Assert.Equal(0, VmAmMapKeysThings1141Slice.TextureSlot(2, 3));
        Assert.True(VmAmMapKeysThings1141Slice.FlipRotation(1 << 3, 3));
        Assert.Equal((10d, 5d), VmAmMapKeysThings1141Slice.SpriteScale(32, 16, 0.5));
    }

    [Fact]
    public void ClassifyThing_PrefersLivingMonstersAndSuppressesKeysAlreadyDrawn()
    {
        var friend = VmAmMapKeysThings1141Slice.ClassifyThing(
            VmAmMapKeysThings1141Slice.Friendly | VmAmMapKeysThings1141Slice.CountKill,
            VmAmMapKeysThings1141Slice.IsMonster, false, false, false, false, -1);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.Friend, friend.Color);
        Assert.Equal(16d, friend.Scale);
        var quiet = VmAmMapKeysThings1141Slice.ClassifyThing(0, VmAmMapKeysThings1141Slice.IsMonster, false, false, false, false, -1);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.NocountMonster, quiet.Color);
        var corpse = VmAmMapKeysThings1141Slice.ClassifyThing(
            VmAmMapKeysThings1141Slice.Corpse | VmAmMapKeysThings1141Slice.Special | VmAmMapKeysThings1141Slice.CountItem,
            VmAmMapKeysThings1141Slice.IsMonster, false, false, false, false, -1);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.CountItem, corpse.Color);
        var easy = VmAmMapKeysThings1141Slice.ClassifyThing(VmAmMapKeysThings1141Slice.Special, 0, true, true, false, true, 0x010203);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.Suppressed, easy.Color);
        Assert.False(easy.Triangle);
        var shown = VmAmMapKeysThings1141Slice.ClassifyThing(VmAmMapKeysThings1141Slice.Special, 0, true, false, false, true, 0x010203);
        Assert.True(shown.CheatKey);
        Assert.False(shown.Triangle);
        Assert.Equal((byte)1, shown.R);
        var plain = VmAmMapKeysThings1141Slice.ClassifyThing(0, 0, false, false, false, false, -1);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.Thing, plain.Color);
        var item = VmAmMapKeysThings1141Slice.ClassifyThing(VmAmMapKeysThings1141Slice.Special, 0, false, false, false, false, -1);
        Assert.Equal(VmAmMapKeysThings1141Slice.ThingColor.Item, item.Color);
    }
}
