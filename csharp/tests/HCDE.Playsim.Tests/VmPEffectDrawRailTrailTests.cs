namespace HCDE.Playsim.Tests;

public class VmPEffectDrawRailTrailTests
{
    private static VmPEffectDrawRailTrail.PortalHit Hit(double x1, double y1, double z1, double x2, double y2, double z2) =>
        new()
        {
            ContX = x1,
            ContY = y1,
            ContZ = z1,
            OutDirX = x2 - x1,
            OutDirY = y2 - y1,
            OutDirZ = z2 - z1,
            HitX = x2,
            HitY = y2,
            HitZ = z2,
        };

    [Fact]
    public void DrawRailTrail_SkipsWhenTooFewPortalHits()
    {
        var s = new VmPEffectDrawRailTrail.DrawRailTrailState { PortalHits = new[] { Hit(0, 0, 0, 10, 0, 0) } };
        VmPEffectDrawRailTrail.DrawRailTrail(s);
        Assert.Empty(s.Trail);
    }

    [Fact]
    public void DrawRailTrail_ZeroLengthReturnsBeforeParticles()
    {
        var s = new VmPEffectDrawRailTrail.DrawRailTrailState
        {
            PortalHits = new[]
            {
                Hit(0, 0, 0, 0, 0, 0),
                Hit(0, 0, 0, 0, 0, 0),
            },
            Color1 = 1,
        };
        VmPEffectDrawRailTrail.DrawRailTrail(s);
        Assert.Empty(s.Particles);
    }

    [Fact]
    public void DrawRailTrail_PlaysSoundAtShooterWhenNearCamera()
    {
        var played = false;
        var s = new VmPEffectDrawRailTrail.DrawRailTrailState
        {
            PortalHits = new[] { Hit(0, 0, 0, 30, 0, 0), Hit(30, 0, 0, 60, 0, 0) },
            HasPlayer = true,
            CameraX = 1,
            CameraY = 1,
            DefaultRailSoundId = 42,
            PlaySoundAtCamera = (atCam, _) => played = atCam,
        };
        VmPEffectDrawRailTrail.DrawRailTrail(s);
        Assert.True(played);
        Assert.Equal(1, s.SoundCalls);
    }

    [Fact]
    public void DrawRailTrail_SpawnsParticlesWhenColorsSet()
    {
        var s = new VmPEffectDrawRailTrail.DrawRailTrailState
        {
            PortalHits = new[] { Hit(0, 0, 0, 30, 0, 0), Hit(30, 0, 0, 60, 0, 0) },
            Color1 = 5,
            Color2 = 6,
            Sparsity = 1,
            RailSpiralSparsity = 1,
            RailTrailSparsity = 1,
            NewParticle = () => new VmPEffectDrawRailTrail.ParticleSpawn(),
            JitterParticle = _ => new VmPEffectDrawRailTrail.ParticleSpawn(),
            ParticleColor = c => c,
        };
        VmPEffectDrawRailTrail.DrawRailTrail(s);
        Assert.NotEmpty(s.Particles);
    }
}
