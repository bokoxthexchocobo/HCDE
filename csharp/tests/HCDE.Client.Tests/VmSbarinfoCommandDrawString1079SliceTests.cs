namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawString1079SliceTests
{
    [Fact]
    public void ResolveDrawStringXAfterRealign_CenterHalvesMonospaceWidth()
    {
        var x = VmSbarinfoCommandDrawString1079Slice.ResolveDrawStringXAfterRealign(
            VmSbarinfoCommandDrawString1079Slice.NativeStringAlignment.Center,
            startX: 100,
            spacingCharacter: ' ',
            stringWidth: 0,
            spacing: 2,
            charCount: 4,
            monospaceCharWidth: 8);
        Assert.Equal(80, x);
    }

    [Fact]
    public void ResolveDrawStringXAfterRealign_RightUsesProportionalWidth()
    {
        var x = VmSbarinfoCommandDrawString1079Slice.ResolveDrawStringXAfterRealign(
            VmSbarinfoCommandDrawString1079Slice.NativeStringAlignment.Right,
            startX: 200,
            spacingCharacter: VmSbarinfoCommandDrawString1079Slice.ProportionalSpacingSentinel,
            stringWidth: 50,
            spacing: 1,
            charCount: 10,
            monospaceCharWidth: 0);
        Assert.Equal(140, x);
    }

    [Fact]
    public void ResolveTagStringWhenActorPresent_RealignsOnClassChange()
    {
        var (cache, _, realign) = VmSbarinfoCommandDrawString1079Slice.ResolveTagStringWhenActorPresent(
            priorCache: 1,
            actorClassPointer: (nint)2,
            actorTag: "Key");
        Assert.Equal(2, cache);
        Assert.True(realign);
    }

    [Fact]
    public void ResolveTagStringWhenActorMissing_ClearsCacheAndText()
    {
        var (cache, text) = VmSbarinfoCommandDrawString1079Slice.ResolveTagStringWhenActorMissing();
        Assert.Equal(-1, cache);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryClassifyDrawNumberValueKeyword_MapsHealthArmorAmmo()
    {
        var (ok, kind) = VmSbarinfoCommandDrawString1079Slice.TryClassifyDrawNumberValueKeyword("ammo1capacity");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Ammo1Capacity, kind);
    }

    [Fact]
    public void ResolveDrawNumberValueSource_DistinguishesConstantInt()
    {
        var (kind, parsedInt) = VmSbarinfoCommandDrawString1079Slice.ResolveDrawNumberValueSource(sawIntConst: true);
        Assert.Equal(VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Constant, kind);
        Assert.True(parsedInt);
    }
}
