namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawNumber1080SliceTests
{
    [Fact]
    public void TryClassifyDrawNumberValueKeyword_MapsStatCounters()
    {
        var (ok, kind) = VmSbarinfoCommandDrawNumber1080Slice.TryClassifyDrawNumberValueKeyword("totalsecrets");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.TotalSecrets, kind);
    }

    [Fact]
    public void ShouldRequireClassNameToken_WhenNotParenthesizedOrMissingString()
    {
        Assert.True(VmSbarinfoCommandDrawNumber1080Slice.ShouldRequireClassNameToken(
            parenthesized: false,
            sawStringConst: false));
        Assert.True(VmSbarinfoCommandDrawNumber1080Slice.ShouldRequireClassNameToken(true, false));
        Assert.False(VmSbarinfoCommandDrawNumber1080Slice.ShouldRequireClassNameToken(true, true));
    }

    [Fact]
    public void ShouldUseDefaultAmmoClassFallback_DetectsInvalidAmmoType()
    {
        Assert.True(VmSbarinfoCommandDrawNumber1080Slice.ShouldUseDefaultAmmoClassFallback(
            actorFound: true,
            isAmmoKind: false));
    }

    [Fact]
    public void IsDrawNumberGlobalIndexInRange_DelegatesTo1078()
    {
        Assert.False(VmSbarinfoCommandDrawNumber1080Slice.IsDrawNumberGlobalIndexInRange(64));
    }

    [Fact]
    public void ResolveDrawNumberIntCVarAcceptance_AllowsBoolAndInt()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1080Slice.IntCVarAcceptedType.Int,
            VmSbarinfoCommandDrawNumber1080Slice.ResolveDrawNumberIntCVarAcceptance(true, false, true));
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1080Slice.IntCVarAcceptedType.Rejected,
            VmSbarinfoCommandDrawNumber1080Slice.ResolveDrawNumberIntCVarAcceptance(true, false, false));
    }

    [Fact]
    public void ShouldValidateInventoryItemClass_OnlyForInventoryKind()
    {
        Assert.True(VmSbarinfoCommandDrawNumber1080Slice.ShouldValidateInventoryItemClass(
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Inventory));
        Assert.False(VmSbarinfoCommandDrawNumber1080Slice.ShouldValidateInventoryItemClass(
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Frags));
    }
}
