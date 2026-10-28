namespace HCDE.Playsim.Tests;

public class VmPLnspecMiscLineSpecialsTests
{
    [Fact]
    public void TranslucentLine_ClampsAlpha()
    {
        Assert.Equal(1.0, VmPLnspecMiscLineSpecials.TranslucentAlpha(300));
    }

    [Fact]
    public void ChangeSkill_RejectsOutOfRange()
    {
        Assert.Equal(-1, VmPLnspecMiscLineSpecials.NextSkillFromArg(99, 5));
    }

    [Fact]
    public void NoiseAlert_EmitterFallsBackToTarget()
    {
        var target = new object();
        Assert.Same(target, VmPLnspecMiscLineSpecials.ResolveNoiseEmitter(3, 3, null, target, null));
    }

    [Fact]
    public void Communicator_RejectsBacksideWhenFrontOnly()
    {
        Assert.True(VmPLnspecMiscLineSpecials.CommunicatorRejectBackside(true, true));
    }

    [Fact]
    public void ClearForceField_ReturnsTrueWhenSectorFound()
    {
        Assert.True(VmPLnspecMiscLineSpecials.ClearForceFieldReturns(true));
    }

    [Fact]
    public void GlassBreak_ClearsBlockingFlags()
    {
        Assert.Equal(0, VmPLnspecMiscLineSpecials.GlassBreakClearFlags(3));
    }
}
