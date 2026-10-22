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
    public void StartQuake_TidZero_SpawnsAtActivator()
    {
        var spawned = false;
        var ctx = new VmAQuake.StartQuakeContext { SpawnQuakeAt = _ => spawned = true };
        Assert.True(VmAQuake.StartQuake(ctx, new object(), 0, 3, 100, 128, 512, new object()));
        Assert.True(spawned);
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
