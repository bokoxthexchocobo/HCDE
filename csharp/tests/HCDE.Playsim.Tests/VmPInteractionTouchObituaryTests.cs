namespace HCDE.Playsim.Tests;

public class VmPInteractionTouchObituaryTests
{
    [Fact]
    public void PickupOutOfReach_WhenAboveFeet() =>
        Assert.True(VmPInteractionTouchObituary.PickupOutOfReach(64, 41, 16));

    [Fact]
    public void PronounCaseFromMarker_Grammar() =>
        Assert.Equal(0, VmPInteractionTouchObituary.PronounCaseFromMarker('g'));

    [Fact]
    public void ExpandPronoun_They() =>
        Assert.Equal("they", VmPInteractionTouchObituary.ExpandPronoun(2, 0));

    [Fact]
    public void ShouldShowObituary_RequiresPlayerMo() =>
        Assert.True(VmPInteractionTouchObituary.ShouldShowObituary(true, true, true));
}
