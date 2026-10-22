namespace HCDE.Playsim.Tests;

public class VmPEffectVisualThinkerTests
{
    [Fact]
    public void DisconnectEffect_SpawnsUntilPoolEmpty()
    {
        var n = 0;
        var s = new VmPEffectVisualThinker.DisconnectState
        {
            Radius = 32,
            Height = 64,
            JitterParticle = () => n++ < 3 ? new VmPEffectVisualThinker.ParticleSlot() : null,
            Random128 = () => 64,
        };
        VmPEffectVisualThinker.DisconnectEffect(s);
        Assert.Equal(3, s.Spawned);
    }

    [Fact]
    public void Tick_DestroysWhenTextureInvalid()
    {
        var s = new VmPEffectVisualThinker.VisualThinkerState();
        VmPEffectVisualThinker.Tick(s);
        Assert.True(s.Destroyed);
    }

    [Fact]
    public void GetParticleType_MapsSquareFlag()
    {
        var s = new VmPEffectVisualThinker.VisualThinkerState { VisualFlags = VmPEffectVisualThinker.VtfIsparticle | VmPEffectVisualThinker.VtfParticlesquare };
        Assert.Equal(VmPEffectVisualThinker.PtSquare, VmPEffectVisualThinker.GetParticleType(s));
    }

    [Fact]
    public void InterpolatedPosition_BlendsPrevAndCurrent()
    {
        var s = new VmPEffectVisualThinker.VisualThinkerState();
        s.PrevX = 0;
        s.Pt.PosX = 10;
        var (x, _, _) = VmPEffectVisualThinker.InterpolatedPosition(s, 0.5);
        Assert.Equal(5, x);
    }
}
