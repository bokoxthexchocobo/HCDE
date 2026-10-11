namespace HCDE.Playsim.Tests;

public class ActorRenderStyleVisibilityTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(0.25, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void TranslucentVisibilityClampsAlpha(double alpha, bool expected)
        => Assert.Equal(expected, ActorRenderStyle.Translucent.IsVisible(alpha));

    [Theory]
    [InlineData(ActorBlendOperation.None, false)]
    [InlineData(ActorBlendOperation.Add, false)]
    [InlineData(ActorBlendOperation.ReverseSubtract, false)]
    [InlineData(ActorBlendOperation.Subtract, true)]
    [InlineData(ActorBlendOperation.Fuzz, true)]
    [InlineData(ActorBlendOperation.FuzzOrAdd, true)]
    [InlineData(ActorBlendOperation.FuzzOrSubtract, true)]
    [InlineData(ActorBlendOperation.FuzzOrReverseSubtract, true)]
    [InlineData(ActorBlendOperation.Shadow, true)]
    public void BlendOperationsFollowNativeVisibilityRule(ActorBlendOperation operation, bool expected)
        => Assert.Equal(expected, new ActorRenderStyle(operation, ActorAlphaFactor.Zero, ActorAlphaFactor.One).IsVisible(0));

    [Theory]
    [InlineData(ActorAlphaFactor.Zero, false)]
    [InlineData(ActorAlphaFactor.One, true)]
    [InlineData(ActorAlphaFactor.Source, false)]
    [InlineData(ActorAlphaFactor.InverseSource, true)]
    [InlineData(ActorAlphaFactor.SourceColor, true)]
    [InlineData(ActorAlphaFactor.InverseSourceColor, true)]
    [InlineData(ActorAlphaFactor.DestinationColor, true)]
    [InlineData(ActorAlphaFactor.InverseDestinationColor, true)]
    [InlineData(ActorAlphaFactor.Destination, true)]
    [InlineData(ActorAlphaFactor.InverseDestination, true)]
    public void SourceFactorAndPixelDependentFallbackMatchNative(ActorAlphaFactor factor, bool expected)
        => Assert.Equal(expected, new ActorRenderStyle(ActorBlendOperation.Add, factor, ActorAlphaFactor.One).IsVisible(0));

    [Fact]
    public void AlphaOneForcesVisibilityAndDestinationFactorCanMakeStyleVisible()
    {
        Assert.True(ActorRenderStyle.Normal.IsVisible(0));
        Assert.True((ActorRenderStyle.Translucent with { Flags = 2 }).IsVisible(0));
        Assert.True(new ActorRenderStyle(ActorBlendOperation.Add, ActorAlphaFactor.Zero, ActorAlphaFactor.Zero).IsVisible(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SeekabilityUsesExplicitStyleAlongsideExistingTargetFlags(bool dontSeek)
    {
        var missile = new Actor { DontSeekInvisible = dontSeek }; var target = new Actor();
        Assert.Equal(!dontSeek, missile.CanSeek(target, ActorRenderStyle.Translucent, 0));
        Assert.True(missile.CanSeek(target, ActorRenderStyle.Normal, 0));
        target.CantSeek = true;
        Assert.False(missile.CanSeek(target, ActorRenderStyle.Normal, 1));
    }
}
