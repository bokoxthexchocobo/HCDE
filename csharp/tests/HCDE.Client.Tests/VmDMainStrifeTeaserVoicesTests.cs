namespace HCDE.Client.Tests;

public class VmDMainStrifeTeaserVoicesTests
{
    [Fact]
    public void ShouldScanVoices_RequiresStrifeShareware()
    {
        Assert.True(VmDMainStrifeTeaserVoices.ShouldScanVoices(VmDMainStrifeTeaserVoices.GameStrife, VmDMainStrifeTeaserVoices.GiShareware));
    }

    [Fact]
    public void IsAllDigitsVoiceSuffix_RejectsLetters()
    {
        Assert.False(VmDMainStrifeTeaserVoices.IsAllDigitsVoiceSuffix("VOC12A00".AsSpan()));
    }
}
