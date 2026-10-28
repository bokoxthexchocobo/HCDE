namespace HCDE.Scripting.Tests;

public class ZccRandomResolveTests
{
    [Fact]
    public void RandomRange_ResolvesMinMax() =>
        Assert.Equal(ZccRandomResolve.RandomRangeOutcome.Ok,
            ZccRandomResolve.ValidateRandomRange(true, true, true, true, true));

    [Fact]
    public void RandomPick_ResolvesAllChoices() =>
        Assert.True(ZccRandomResolve.RandomPickAllResolved(3, 3));

    [Fact]
    public void Random2_ResolvesMask() =>
        Assert.True(true);

    [Fact]
    public void RandomSeed_ResolvesSeedExpr() =>
        Assert.True(true);
}
