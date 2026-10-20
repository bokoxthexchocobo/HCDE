namespace HCDE.Playsim.Tests;

public class VmAcsDoSetActorPropertyTests
{
    [Fact]
    public void ApplyHealth_KillsWhenNonPositive()
    {
        Assert.True(VmAcsDoSetActorProperty.ApplyHealth(0, false, out var die));
        Assert.True(die);
    }

    [Fact]
    public void TryMapLegacyRenderStyle_FindsIndex()
    {
        Assert.True(VmAcsDoSetActorProperty.TryMapLegacyRenderStyle(2, [1, 2, 3], out var idx));
        Assert.Equal(1, idx);
    }

    [Fact]
    public void ApplyFlagBit_SetsAmbush()
    {
        var flags = VmAcsDoSetActorProperty.ApplyFlagBit(0, VmAcsDoSetActorProperty.MfAmbush, true);
        Assert.Equal(VmAcsDoSetActorProperty.MfAmbush, flags);
    }

    [Fact]
    public void ScaleComponentFromAcs_MatchesDoubleConversion()
    {
        Assert.Equal(1f, VmAcsDoSetActorProperty.ScaleComponentFromAcs(65536));
    }
}
