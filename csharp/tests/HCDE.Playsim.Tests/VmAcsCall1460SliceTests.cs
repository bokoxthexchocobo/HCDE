using Xunit;

namespace HCDE.Playsim.Tests;

public class VmAcsCall1460SliceTests
{
    [Fact]
    public void MathHudAndCvarsFollowTheCaseRules()
    {
        var world = new VmAcsCall1460Slice.World { ClientSide = true, Activator = 3 };
        Assert.Equal(0, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.Sqrt, 0, Array.Empty<int>(), out int need));
        Assert.Equal(1, need);
        Assert.Equal(3, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.Sqrt, 1, new[] { 9 }, out _));
        Assert.Equal(1, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.Sqrt, 1, new[] { 2 }, out _));
        Assert.Equal(65536, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.FixedSqrt, 1, new[] { 65536 }, out _));
        Assert.Equal(5 * 65536, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.VectorLength, 2, new[] { 3 * 65536, 4 * 65536 }, out _));

        int seenA = -1, seenB = -1;
        world.Unique = (a, b, client) => { seenA = a; seenB = b; return client ? 9 : 0; };
        Assert.Equal(9, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.UniqueTid, 2, new[] { 4, -3 }, out _));
        Assert.Equal(4, seenA);
        Assert.Equal(0, seenB);
        world.TidUsed = (tid, client) => tid == 8 && client ? 1 : 0;
        Assert.Equal(1, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.IsTidUsed, 1, new[] { 8 }, out _));

        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.SetHudClipRect, 0, Array.Empty<int>(), out _);
        Assert.True(world.HandleAspect);
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.SetHudClipRect, 6, new[] { 1, 2, 3, 4, 5, 0 }, out _);
        Assert.Equal(5, world.WrapWidth);
        Assert.False(world.HandleAspect);
        world.Strings[7] = "sv_test";
        world.ReadCVar = (name, text) => name == "sv_test" && text ? 11 : 0;
        Assert.Equal(11, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.GetCVarString, 1, new[] { 7 }, out _));
        world.ResolvePlayer = player => player == 1 ? -1 : 2;
        Assert.Equal(0, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.GetUserCVar, 2, new[] { 1, 7 }, out _));
        world.WriteUserCVar = (player, name, value, text) => player == 4 && name == "sv_test" && value == 6 && text ? 1 : 0;
        Assert.Equal(1, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.SetUserCVarString, 3, new[] { 4, 7, 6 }, out _));
    }

    [Fact]
    public void LineAttackAndSoundWalkEveryTarget()
    {
        var world = new VmAcsCall1460Slice.World { Activator = 5 };
        world.ActorsOf = tid => tid == 0 ? new[] { 8 } : tid == 4 ? new[] { 1, 2 } : Array.Empty<int>();
        world.Fire = (source, angle, pitch, damage, puff, kind, range, flags) => source + 10;
        Assert.Equal(0, VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.LineAttack, 4, new[] { 0, 16384, 0, 7 }, out _));
        var shot = Assert.Single(world.Shots);
        Assert.Equal(5, shot.Source);
        Assert.Equal(90, shot.Angle);
        Assert.Equal(VmAcsCall1460Slice.MissileRange, shot.Range);
        Assert.Equal("BulletPuff", shot.Puff);
        Assert.Equal("None", shot.DamageType);
        Assert.Empty(world.TidSets);

        world.Strings[3] = "Plasma";
        world.Strings[4] = "Fire";
        world.Shots.Clear();
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.LineAttack, 9, new[] { 4, 0, 0, 1, 3, 4, 65536, 3, 9 }, out _);
        Assert.Equal(new[] { 1, 2 }, world.Shots.Select(item => item.Source));
        Assert.Equal(6, world.Shots[0].Flags);
        Assert.Equal(1, world.Shots[0].Range);
        Assert.Equal(new[] { (11, 9), (12, 9) }, world.TidSets);

        world.Strings[2] = "weapons/pistol";
        world.FindSound = name => name == "weapons/pistol" ? 15 : 0;
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.PlaySound, 7, new[] { 0, 2, 4, 65536, 1, 32768, 1 }, out _);
        Assert.Equal(new[] { 5, 8 }, world.HeardSounds.Select(item => item.Actor));
        Assert.Equal(4, world.HeardSounds[0].Channel);
        Assert.Equal(16384 | 256 | 4096, world.HeardSounds[0].Flags);
        Assert.Equal(1f, world.HeardSounds[0].Volume);
        Assert.Equal(0.5f, world.HeardSounds[0].Attenuation);

        world.HeardSounds.Clear();
        world.FindSound = _ => 0;
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.PlaySound, 2, new[] { 0, 2 }, out _);
        Assert.Empty(world.HeardSounds);
        world.ActorSound = (actor, kind) => actor == 1 && kind == 6 ? 21 : 0;
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.PlayActorSound, 2, new[] { 4, 6 }, out _);
        Assert.Equal(21, Assert.Single(world.HeardSounds).Sound);

        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.StopSound, 1, new[] { 0 }, out _);
        Assert.Equal((5, 4), Assert.Single(world.Stops));
        world.Stops.Clear();
        VmAcsCall1460Slice.Call(world, VmAcsCall1460Slice.StopSound, 2, new[] { 4, 9 }, out _);
        Assert.Equal(new[] { (1, 9), (2, 9) }, world.Stops);
    }
}
