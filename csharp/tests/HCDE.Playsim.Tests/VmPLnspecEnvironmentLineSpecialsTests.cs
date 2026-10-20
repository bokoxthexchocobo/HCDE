namespace HCDE.Playsim.Tests;

public class VmPLnspecEnvironmentLineSpecialsTests
{
    [Fact]
    public void PortalTarget_PacksIds()
    {
        var (a, b) = VmPLnspecEnvironmentLineSpecials.PortalTarget(3, 9);
        Assert.Equal(3, a);
        Assert.Equal(9, b);
    }

    [Fact]
    public void PlaneReflection_ScalesArgs()
    {
        Assert.Equal(1f, VmPLnspecEnvironmentLineSpecials.ReflectionScale(255));
    }

    [Fact]
    public void GlobalFog_RejectsUnknownType()
    {
        Assert.True(VmPLnspecEnvironmentLineSpecials.GlobalFogRejectUnknown(9));
    }

    [Fact]
    public void Glow_InvalidatesOnNegativeHeight()
    {
        Assert.Equal(-1, VmPLnspecEnvironmentLineSpecials.GlowColorInvalid(-1));
    }

    [Fact]
    public void LineHealth_ClampsNegativeToZero()
    {
        Assert.Equal(0, VmPLnspecEnvironmentLineSpecials.LineHealthClamped(-5));
    }

    [Fact]
    public void ChangeSkill_RejectsOutOfRange()
    {
        Assert.Equal(-1, VmPLnspecEnvironmentLineSpecials.NextSkillFromArg(9, 4));
    }

    [Fact]
    public void NoiseAlert_EmitterFallsBackToTarget()
    {
        var t = new object();
        Assert.Same(t, VmPLnspecEnvironmentLineSpecials.ResolveNoiseEmitter(2, 2, null, t, null));
    }

    [Fact]
    public void Communicator_RejectsBacksideWhenFrontOnly()
    {
        Assert.True(VmPLnspecEnvironmentLineSpecials.CommunicatorRejectBackside(true, true));
    }
}
