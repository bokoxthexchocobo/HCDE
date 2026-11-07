using S = HCDE.Playsim.VmQuake1396Slice;

namespace HCDE.Playsim.Tests;

public class VmQuake1396SliceTests
{
    [Fact]
    public void FalloffWaveAndIntensityFollowTheFlagBranches()
    {
        var quake = new S.Quake { Falloff = 10, DamageRadius = 30 };
        Assert.Equal(1, S.GetFalloff(quake, 5, 30));
        Assert.Equal(1, S.GetFalloff(quake, 10, 30));
        Assert.Equal(0.5, S.GetFalloff(quake, 20, 30));
        Assert.Equal(1, S.GetFalloff(quake, 30, 30));
        quake.Falloff = 0;
        Assert.Equal(1, S.GetFalloff(quake, 4, 30));
        quake.Falloff = 40;
        Assert.Equal(1, S.GetFalloff(quake, 4, 30));

        quake.Countdown = 35;
        Assert.Equal(0, S.GetModWave(quake, 0, 1), 6);
        Assert.Equal(1, S.GetModWave(quake, 0, 0.25), 6);
        Assert.Equal(35, S.TicRate);
        Assert.Equal(335, S.StaticIntensityLine);

        quake.Countdown = 5;
        quake.CountdownStart = 10;
        Assert.Equal(8, S.GetModIntensity(quake, 4, false));

        quake.Flags = S.QfScaledown;
        Assert.Equal(4, S.GetModIntensity(quake, 4, false));

        quake.Flags = S.QfScaleup;
        quake.Countdown = 2;
        Assert.Equal(4.8, S.GetModIntensity(quake, 3, false), 6);

        quake.Flags = S.QfScaledown | S.QfScaleup;
        quake.Countdown = 3;
        quake.Highpoint = 0;
        Assert.Equal(3, S.GetModIntensity(quake, 5, false));
        quake.Flags |= S.QfMax;
        Assert.Equal(7, S.GetModIntensity(quake, 5, false));
        quake.Flags = S.QfScaledown | S.QfScaleup | S.QfFullIntensity;
        Assert.Equal(6, S.GetModIntensity(quake, 5, false));
        Assert.Equal(3, S.GetModIntensity(quake, 5, true));

        quake.Flags = S.QfScaledown | S.QfScaleup;
        quake.Highpoint = 4;
        quake.MiniCount = 2;
        Assert.Equal(5, S.GetModIntensity(quake, 5, false));
        quake.MiniCount = 0;
        quake.Flags |= S.QfMax;
        quake.Countdown = 1;
        Assert.Equal(10 * 5 / 6.0, S.GetModIntensity(quake, 5, false), 6);

        quake.Flags = S.QfScaleup;
        quake.Countdown = 2;
        quake.CountdownStart = 10;
        quake.Highpoint = 6;
        quake.MiniCount = 1;
        Assert.Equal(10 * 5 / 6.0, S.GetModIntensity(quake, 5, false), 6);

        quake.CountdownStart = 1;
        quake.Countdown = 2;
        Assert.Throws<InvalidOperationException>(() => S.GetModIntensity(quake, 1, false));
        quake.CountdownStart = 0;
        quake.Countdown = 0;
        quake.Highpoint = 0;
        quake.Flags = S.QfScaledown;
        Assert.Throws<InvalidOperationException>(() => S.GetModIntensity(quake, 1, false));
    }

    [Fact]
    public void DamageAndThrustUseRadiusFloorAndTheRandomCalls()
    {
        var hits = new List<S.Hit>();
        S.DoQuakeDamage(null, new S.Victim(), false, () => 0, _ => 1, hits);
        S.DoQuakeDamage(new S.Quake(), null, false, () => 0, _ => 1, hits);
        Assert.Empty(hits);

        var quake = new S.Quake { DamageRadius = 10, Distance = 10, Damage = 3, DamageMultiplier = 2 };
        var victim = new S.Victim();
        int rolls = 0;
        S.DoQuakeDamage(quake, victim, false, () => { rolls++; return 0; }, _ => 1, hits);
        Assert.Equal(0, rolls);
        Assert.Empty(hits);

        quake.Distance = 5;
        victim.Z = 2;
        victim.FloorZ = 1;
        S.DoQuakeDamage(quake, victim, false, () => { rolls++; return 0; }, _ => 1, hits);
        Assert.Equal(0, rolls);

        victim.Z = 1;
        quake.Flags = S.QfShakeOnly;
        quake.ThrustMultiplier = 3;
        quake.IntensityX = 2;
        quake.IntensityY = 4;
        victim.Yaw = 0;
        var queue = new Queue<int>(new[] { 0 });
        S.DoQuakeDamage(quake, victim, false, () => queue.Dequeue(), _ => throw new InvalidOperationException("dice"), hits);
        Assert.Empty(hits);
        Assert.Empty(queue);
        Assert.Equal(6, victim.VelX);
        Assert.Equal(0, victim.VelY);

        victim.VelX = 0;
        quake.Flags = 0;
        quake.ThrustMultiplier = 0;
        quake.Damage = 3;
        queue = new Queue<int>(new[] { 0 });
        S.DoQuakeDamage(quake, victim, false, () => queue.Dequeue(), _ => throw new InvalidOperationException("dice"), hits);
        Assert.Equal(6, hits[0].Amount);
        Assert.Equal(S.QuakeDamage, hits[0].Type);
        Assert.Equal(0, victim.VelX);

        hits.Clear();
        quake.Damage = 0;
        quake.DamageMultiplier = 2;
        queue = new Queue<int>(new[] { 10 });
        int diceCount = 0;
        S.DoQuakeDamage(quake, victim, false, () => queue.Dequeue(), count => { diceCount = count; return 3; }, hits);
        Assert.Equal(1, diceCount);
        Assert.Equal(6, hits[0].Amount);

        hits.Clear();
        quake.DamageMultiplier = 0.4;
        queue = new Queue<int>(new[] { 0 });
        S.DoQuakeDamage(quake, victim, false, () => queue.Dequeue(), _ => 1, hits);
        Assert.Equal(1, hits[0].Amount);

        hits.Clear();
        quake.Falloff = 10;
        quake.DamageRadius = 30;
        quake.Distance = 20;
        quake.Damage = 4;
        quake.DamageMultiplier = 2;
        quake.ThrustMultiplier = 2;
        quake.IntensityX = 2;
        quake.IntensityY = 0;
        queue = new Queue<int>(new[] { 0, 0 });
        S.DoQuakeDamage(quake, victim, true, () => queue.Dequeue(), _ => throw new InvalidOperationException("dice"), hits);
        Assert.Equal(4, hits[0].Amount);
        Assert.Equal(2, victim.VelX);
        Assert.Empty(queue);

        hits.Clear();
        victim.VelX = 9;
        victim.Flags7 = S.Mf7DontThrust;
        quake.ThrustMultiplier = 5;
        queue = new Queue<int>(new[] { 49 });
        S.DoQuakeDamage(quake, victim, true, () => queue.Dequeue(), _ => 1, hits);
        Assert.Equal(4, hits[0].Amount);
        Assert.Equal(9, victim.VelX);
        Assert.Empty(queue);

        hits.Clear();
        victim.Flags7 = 0;
        queue = new Queue<int>(new[] { 50, 0 });
        S.DoQuakeDamage(quake, victim, false, () => queue.Dequeue(), _ => throw new InvalidOperationException("dice"), hits);
        Assert.Empty(hits);
        Assert.Equal(1, S.QfRelative);
    }
}
