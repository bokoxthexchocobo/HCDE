namespace HCDE.Playsim.Tests;

public class VmPInteractionActorDieStatesTests
{
    [Fact]
    public void Unmorph_Destroy() =>
        Assert.True(VmPInteractionActorDieStates.UnmorphDestroyEarly(VmPInteractionActorDieStates.MfUnmorphed));

    [Fact]
    public void Extreme_FromGib() =>
        Assert.True(VmPInteractionActorDieStates.ExtremeByGib(true, false, false));

    [Fact]
    public void Ice_GenericFreeze() =>
        Assert.True(VmPInteractionActorDieStates.IceGenericFreeze(false, 0, false, true));

    [Fact]
    public void Tics_Randomized() =>
        Assert.Equal(3, VmPInteractionActorDieStates.RandomizeDeathTics(5, 2));
}
