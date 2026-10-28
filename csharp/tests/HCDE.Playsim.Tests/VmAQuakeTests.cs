using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAQuakeTests
{
    [Fact]
    public void GetFalloff_InsideMinRange_ReturnsOne()
    {
        var q = new VmAQuake.QuakeThinker { Falloff = 64, DamageRadius = 256 };
        Assert.Equal(1.0, VmAQuake.GetFalloff(q, 32, 256));
    }

    [Fact]
    public void GetModIntensity_DoublesBaseWhenNoScaleFlags()
    {
        var q = new VmAQuake.QuakeThinker { Flags = 0, Countdown = 10, CountdownStart = 20 };
        Assert.Equal(2.0, VmAQuake.GetModIntensity(q, 1.0, false));
    }

    [Fact]
    public void GetModIntensity_ScaleDown_UsesCountdownRatio()
    {
        var q = new VmAQuake.QuakeThinker
        {
            Flags = VmAQuake.QfScaledown,
            Countdown = 5,
            CountdownStart = 10,
        };
        var v = VmAQuake.GetModIntensity(q, 1.0, false);
        Assert.True(v > 0 && v < 2.0);
    }

    [Fact]
    public void StartQuake_TidZero_SpawnsAtActivator()
    {
        var spawned = false;
        var ctx = new VmAQuake.StartQuakeContext { SpawnQuakeAt = _ => spawned = true };
        Assert.True(VmAQuake.StartQuake(ctx, new object(), 0, 3, 100, 128, 512, new object()));
        Assert.True(spawned);
    }

    [Fact]
    public void Construct_SetsCountdownAndSpot()
    {
        var spot = new object();
        var q = new VmAQuake.QuakeThinker();
        VmAQuake.Construct(q, spot, 1, 2, 3, 100, 64, 256, new object(), 0, 0, 0, 0, 0, 0, 0, 0, 1, 0.5, 0);
        Assert.Equal(spot, q.Spot);
        Assert.Equal(100, q.Countdown);
    }

    [Fact]
    public void Tick_NullSpot_Destroys()
    {
        var destroyed = false;
        var q = new VmAQuake.QuakeThinker { DestroySelf = () => destroyed = true };
        VmAQuake.Tick(q, new VmAQuake.QuakeTickContext());
        Assert.True(destroyed);
    }

    [Fact]
    public void StaticGetQuakeIntensities_CountsInRadius()
    {
        var victim = new VmAQuake.QuakeVictim { Z = 0, FloorZ = 0 };
        var j = new VmAQuake.QuakeJiggers();
        var q = new VmAQuake.QuakeThinker
        {
            Spot = new object(),
            TremorRadius = 200,
            Distance2D = (_, _) => 50,
        };
        var n = VmAQuake.StaticGetQuakeIntensities(0, victim, j, new[] { q }, _ => false);
        Assert.Equal(1, n);
    }

    [Fact]
    public void DoQuakeDamage_InRadius_AppliesDamage()
    {
        var dmg = 0;
        var q = new VmAQuake.QuakeThinker { Spot = new object(), DamageRadius = 100, Damage = 5, Flags = 0 };
        var v = new VmAQuake.QuakeVictim
        {
            Z = 0,
            FloorZ = 0,
            ApplyDamage = d => dmg = d,
        };
        VmAQuake.DoQuakeDamage(q, v, false, () => 10, _ => 4);
        Assert.Equal(5, dmg);
    }
}
