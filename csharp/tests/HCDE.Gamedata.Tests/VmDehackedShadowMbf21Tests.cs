namespace HCDE.Gamedata.Tests;

public class VmDehackedShadowMbf21Tests
{
    [Fact]
    public void SetShadow_UsesNativeMfShadowAndClearsZdoomTrans()
    {
        var (flags, style, rf) = VmDehackedShadowMbf21.SetShadow(0, 4, VmDehackedShadowMbf21.RfZdoomtrans);
        Assert.Equal(0x00040000UL, flags & VmDehackedShadowMbf21.MfShadow);
        Assert.Equal(4, style);
        Assert.Equal(0U, rf);
    }

    [Fact]
    public void ClearShadow_OpaqueUsesNormalLegacyStyle()
    {
        var (flags, style, _) = VmDehackedShadowMbf21.ClearShadow(
            VmDehackedShadowMbf21.MfShadow, 1f, 1, 64, 0);
        Assert.Equal(0UL, flags & VmDehackedShadowMbf21.MfShadow);
        Assert.Equal(1, style);
    }

    [Fact]
    public void Mbf21JumpIfFlagsSet_RequiresAllSpecifiedBits()
    {
        var checkers = new Func<bool>?[32];
        checkers[1] = () => true;
        checkers[3] = () => false;
        Assert.False(VmDehackedShadowMbf21.Mbf21JumpIfFlagsSet(1 << 1 | 1 << 3, 0, checkers, Array.Empty<Func<bool>?>()));
        checkers[3] = () => true;
        Assert.True(VmDehackedShadowMbf21.Mbf21JumpIfFlagsSet(1 << 1 | 1 << 3, 0, checkers, Array.Empty<Func<bool>?>()));
    }

    [Fact]
    public void Mbf21AddFlags_InvokesSettersForMaskedBits()
    {
        var hits = 0;
        var setters = new Action?[32];
        setters[2] = () => hits++;
        setters[5] = () => hits += 10;
        VmDehackedShadowMbf21.Mbf21AddFlags(1 << 2 | 1 << 5, 0, setters, Array.Empty<Action?>());
        Assert.Equal(11, hits);
    }

    [Fact]
    public void Mbf21RemoveFlags_InvokesClearersForMaskedBits()
    {
        var hits = 0;
        var clearers = new Action?[32];
        clearers[0] = () => hits++;
        VmDehackedShadowMbf21.Mbf21RemoveFlags(1, 0, clearers, Array.Empty<Action?>());
        Assert.Equal(1, hits);
    }
}
