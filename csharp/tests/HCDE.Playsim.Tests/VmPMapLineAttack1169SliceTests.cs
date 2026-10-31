namespace HCDE.Playsim.Tests;

public class VmPMapLineAttack1169SliceTests
{
    [Fact]
    public void Setup_StopsBeforeTheTraceAndPlacesTheShot()
    {
        var shooter = new VmPMapLineAttack1169Slice.Shooter();
        var victim = new VmPMapLineAttack1169Slice.LineTarget { Target = "stale", Unlinked = true, AttackAngle = 9 };
        var damage = new VmPMapLineAttack1169Slice.DamageSlot { Value = 8 };
        var blocked = new VmPMapLineAttack1169Slice.AttackHost { PreFired = true };
        Assert.Null(Attack(shooter, blocked, victim, damage, 0, 0, 0, "None"));
        Assert.Empty(blocked.Lag);
        Assert.Empty(blocked.Traces);
        Assert.Equal("stale", victim.Target);

        var suppressed = new VmPMapLineAttack1169Slice.AttackHost { Suppress = true };
        Assert.Null(Attack(shooter, suppressed, victim, damage, 0, 0, 0, "None"));
        Assert.Null(victim.Target);
        Assert.Equal(0, damage.Value);
        Assert.Empty(suppressed.Traces);

        var placed = new VmPMapLineAttack1169Slice.AttackHost();
        Attack(shooter, placed, null, null, 0, 4, 2, "Melee");
        var query = placed.Traces[0];
        Assert.Equal(1d, query.Direction.X);
        Assert.Equal(0d, query.Direction.Y);
        Assert.Equal(0d, query.Direction.Z);
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(104, 198, 33), query.From);
        Assert.Equal(VmPMapLineAttack1169Slice.MfShootable, query.ActorMask);
        Assert.Equal(VmPMapLineAttack1169Slice.MlBlockEverything | VmPMapLineAttack1169Slice.MlBlockHitscan, query.WallMask);
        Assert.Equal(VmPMapLineAttack1169Slice.TraceNoSky | VmPMapLineAttack1169Slice.TraceImpact, query.TraceFlags);
        Assert.Equal("Fire", placed.ResolvedDamageType);

        var absolute = new VmPMapLineAttack1169Slice.AttackHost();
        Attack(shooter, absolute, null, null, VmPMapLineAttack1169Slice.LafAbsPosition | VmPMapLineAttack1169Slice.LafOverrideZ, 4, 2, "MDK");
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(4, 2, 4), absolute.Traces[0].From);
        Assert.Equal("MDK", absolute.ResolvedDamageType);

        var offset = new VmPMapLineAttack1169Slice.AttackHost();
        Attack(shooter, offset, null, null, VmPMapLineAttack1169Slice.LafAbsOffset, 4, 2, "Hitscan");
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(104, 202, 33), offset.Traces[0].From);
        Assert.Equal("Fire", offset.ResolvedDamageType);

        var origin = new VmPMapLineAttack1169Slice.AttackHost();
        Attack(shooter, origin, null, null, 0, 0, 0, "None");
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(100, 200, 33), origin.Traces[0].From);
    }

    [Fact]
    public void Miss_PlaysTheSoundOrReturnsTheForcedPuff()
    {
        var shooter = new VmPMapLineAttack1169Slice.Shooter { Player = true, WeaponThruGhost = true };
        var host = new VmPMapLineAttack1169Slice.AttackHost
        {
            Defaults =
            {
                Flags7 = VmPMapLineAttack1169Slice.Mf7AllowThruFlags,
                Flags2 = VmPMapLineAttack1169Slice.Mf2ThruActors,
                Flags6 = VmPMapLineAttack1169Slice.Mf6ThruSpecies | VmPMapLineAttack1169Slice.Mf6MThruSpecies | VmPMapLineAttack1169Slice.Mf6NoTrigger,
                Flags4 = VmPMapLineAttack1169Slice.Mf4Spectral,
                Flags3 = VmPMapLineAttack1169Slice.Mf3SkyExplode,
                ActiveSoundValid = true,
                ActiveSound = "miss",
            },
            ProbeFlags8 = VmPMapLineAttack1169Slice.Mf8AllowThruBits,
            Trace = { HitPos = new VmPMapLineAttack1169Slice.Vec3(1, 2, 3), SrcAngle = 15 },
        };
        var flags = VmPMapLineAttack1169Slice.LafIsMeleeAttack | VmPMapLineAttack1169Slice.LafNoRandomPuffZ;
        Assert.Null(Attack(shooter, host, null, null, flags, 0, 0, "None"));
        Assert.Equal("miss", host.Sound);
        Assert.False(host.Fired);
        Assert.Equal(VmPMapLineAttack1169Slice.AllowReplace, host.SpeciesReplace);
        Assert.True(host.DestroyedProbes[0].Destroyed);
        var origin = host.Traces[0].Origin;
        Assert.True(origin.HitGhosts);
        Assert.True(origin.MThruSpecies);
        Assert.True(origin.Spectral);
        Assert.True(origin.ThruSpecies);
        Assert.True(origin.ThruActors);
        Assert.True(origin.UseThruBits);
        Assert.Equal(5, origin.ThruBits);
        Assert.Equal("Imp", origin.PuffSpecies);
        Assert.Equal(VmPMapLineAttack1169Slice.TraceHitSky, host.Traces[0].TraceFlags);

        host.DidHit = false;
        host.Defaults.Flags3 = VmPMapLineAttack1169Slice.Mf3AlwaysPuff;
        host.Defaults.Flags6 = 0;
        host.Sound = null;
        var puff = Attack(shooter, host, null, null, flags, 0, 0, "None");
        Assert.Same(puff, host.FiredPuff);
        Assert.True(host.Fired);
        Assert.True(host.Cosmetic);
        Assert.Equal(15d, host.CosmeticAngle);
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(1, 2, 3), host.Spawns[^1].Pos);
        Assert.Equal(15d, host.Spawns[^1].HitDir);
        Assert.Equal(2, host.Spawns[^1].UpDown);
        Assert.Equal(VmPMapLineAttack1169Slice.PfMeleeRange | VmPMapLineAttack1169Slice.PfNoRandomZ, host.Spawns[^1].Flags);

        var quiet = new VmPMapLineAttack1169Slice.AttackHost
        {
            Trace = { HitPos = new VmPMapLineAttack1169Slice.Vec3(8, 0, 1), SrcAngle = 4 },
        };
        var returned = Attack(shooter, quiet, null, null, VmPMapLineAttack1169Slice.LafNoInteract, 0, 0, "None");
        Assert.Same(returned, quiet.Spawns[0].Actor);
        Assert.Null(quiet.Sound);
        Assert.False(quiet.Fired);
        Assert.Equal(VmPMapLineAttack1169Slice.TraceNoSky, quiet.Traces[0].TraceFlags);
    }

    [Fact]
    public void Wall_PullsThePuffBackAndChoosesTheDecal()
    {
        var shooter = new VmPMapLineAttack1169Slice.Shooter { Player = true, WeaponDecal = true };
        var host = new VmPMapLineAttack1169Slice.AttackHost
        {
            DidHit = true,
            Defaults = { Flags7 = VmPMapLineAttack1169Slice.Mf7ForceDecal, Flags3 = VmPMapLineAttack1169Slice.Mf3SkyExplode },
            Trace =
            {
                HitType = VmPMapLineAttack1169Slice.TraceHitWall,
                HitPos = new VmPMapLineAttack1169Slice.Vec3(10, 4, 5),
                HitVector = new VmPMapLineAttack1169Slice.Vec3(1, 0, 0.5),
                SrcAngle = 40,
                Unlinked = true,
                LineSpecial = VmPMapLineAttack1169Slice.LineHorizon,
            },
        };
        var victim = new VmPMapLineAttack1169Slice.LineTarget();
        var puff = Attack(shooter, host, victim, null, 0, 0, 0, "None");
        Assert.True(host.Geometry);
        Assert.True(victim.Unlinked);
        Assert.Null(victim.Target);
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(6, 4, 3), host.Spawns[0].Pos);
        Assert.Equal(-50d, host.Spawns[0].ParticleDir);
        Assert.Equal(0, host.Spawns[0].UpDown);
        Assert.Equal(VmPMapLineAttack1169Slice.EqualEpsilon, puff!.Radius);
        Assert.Equal(VmPMapLineAttack1169Slice.PfHitSky, host.Spawns[0].Flags);
        Assert.True(host.Fired);

        host.Defaults.Flags3 = 0;
        host.Defaults.Flags7 = 0;
        host.Trace.LineSpecial = VmPMapLineAttack1169Slice.LineHorizon;
        host.Spawns.Clear();
        host.Fired = false;
        Assert.Null(Attack(shooter, host, victim, null, 0, 0, 0, "None"));
        Assert.Empty(host.Spawns);
        Assert.Empty(host.Decals);
        Assert.True(host.Geometry);

        host.Trace.LineSpecial = 0;
        host.Trace.HitVector = new VmPMapLineAttack1169Slice.Vec3(0, 1, 0);
        host.SpawnHasDecal = true;
        host.Defaults.Flags7 = VmPMapLineAttack1169Slice.Mf7ForceDecal;
        Attack(shooter, host, victim, null, 0, 0, 0, "None");
        Assert.Equal(("puff", "puff"), host.Decals[^1]);

        host.Defaults.Flags7 = 0;
        host.Decals.Clear();
        Attack(shooter, host, victim, null, 0, 0, 0, "None");
        Assert.Equal(("shooter", "shooter"), host.Decals[^1]);

        shooter.WeaponDecal = false;
        shooter.Decal = false;
        host.Decals.Clear();
        Attack(shooter, host, victim, null, VmPMapLineAttack1169Slice.LafNoImpactDecal, 0, 0, "None");
        Assert.Empty(host.Decals);

        host.Trace.HitType = VmPMapLineAttack1169Slice.TraceHitFloor;
        host.Trace.CrossedWater = false;
        host.Water = false;
        Attack(shooter, host, victim, null, 0, 0, 0, "None");
        Assert.True(host.Water);
    }

    [Fact]
    public void Actor_DamagesFromTheTargetAndBleedsAtTheUnshiftedPoint()
    {
        var target = new VmPMapLineAttack1169Slice.Shooter();
        var shooter = new VmPMapLineAttack1169Slice.Shooter
        {
            Player = true,
            WeaponPierceArmor = true,
            Target = target,
            AngleToDegrees = 12,
        };
        var host = new VmPMapLineAttack1169Slice.AttackHost
        {
            DidHit = true,
            DamageResult = 0,
            Defaults =
            {
                PoisonDamage = 3,
                PoisonDuration = 2,
                Flags6 = VmPMapLineAttack1169Slice.Mf6ForcePain,
            },
            Trace =
            {
                HitType = VmPMapLineAttack1169Slice.TraceHitActor,
                HitPos = new VmPMapLineAttack1169Slice.Vec3(10, 4, 8),
                HitVector = new VmPMapLineAttack1169Slice.Vec3(1, 0, 0.5),
                SrcAngle = 20,
                ActorCompat = VmPMapLineAttack1169Slice.CompatHitscan,
                Actor = "imp",
                CrossedWater = true,
            },
        };
        var victim = new VmPMapLineAttack1169Slice.LineTarget();
        var slot = new VmPMapLineAttack1169Slice.DamageSlot();
        var returned = Attack(shooter, host, victim, slot, VmPMapLineAttack1169Slice.LafTargetIsSource, 0, 0, "None");
        Assert.Null(returned);
        Assert.True(host.Spawns[0].Actor.Destroyed);
        Assert.Equal(new VmPMapLineAttack1169Slice.Vec3(0, 4, 13), host.Spawns[0].Pos);
        Assert.Equal("imp", victim.Target);
        Assert.Equal(20d, victim.AttackAngle);
        Assert.Equal(12d, victim.AngleFromSource);
        Assert.Equal(3, host.PoisonDamage);
        Assert.Same(shooter, host.PoisonInflictor);
        Assert.Equal(0, slot.Value);
        Assert.Equal(
            VmPMapLineAttack1169Slice.DmgInflictorIsPuff | VmPMapLineAttack1169Slice.DmgPlayerAttack | VmPMapLineAttack1169Slice.DmgNoArmor | VmPMapLineAttack1169Slice.DmgUseAngle,
            host.AppliedFlags);
        Assert.Same(target, host.AppliedSource);
        Assert.Same(host.Spawns[0].Actor, host.AppliedInflictor);
        Assert.True(host.Blood);
        Assert.Equal(5d, host.BloodDamage);
        Assert.Equal(0d, host.BloodNew);
        Assert.Equal(5d, host.BleedAmount);
        Assert.Equal(host.Trace.HitPos, host.BleedAt);
        Assert.True(host.Splash);
        Assert.Same(host.Spawns[0].Actor, host.SplashPuff);
        Assert.True(host.Fired);
        Assert.False(host.Cosmetic);
        Assert.Equal(VmPMapLineAttack1169Slice.PfHitThing | VmPMapLineAttack1169Slice.PfTemporary, host.Spawns[0].Flags);

        host.Trace.Unlinked = true;
        host.Trace.ActorFlags = VmPMapLineAttack1169Slice.MfNoBlood;
        host.Trace.CrossedWater = false;
        host.Defaults.Flags3 = VmPMapLineAttack1169Slice.Mf3BloodlessImpact;
        host.Defaults.PoisonDuration = int.MinValue;
        host.PoisonDamage = -1;
        host.Blood = false;
        host.DamageResult = 9;
        host.Spawns.Clear();
        victim.Clear();
        var kept = Attack(shooter, host, victim, slot, 0, 0, 0, "None");
        Assert.NotNull(kept);
        Assert.False(kept!.Destroyed);
        Assert.Equal(20d, victim.AngleFromSource);
        Assert.Equal(-1, host.PoisonDamage);
        Assert.Equal(VmPMapLineAttack1169Slice.PfHitThing, host.Spawns[0].Flags & VmPMapLineAttack1169Slice.PfHitThing);
        Assert.Equal(0, host.Spawns[0].Flags & VmPMapLineAttack1169Slice.PfHitThingBleed);
        Assert.False(host.Blood);
        Assert.Equal(9, slot.Value);
        Assert.True(host.Cosmetic);
    }

    private static VmPMapLineAttack1169Slice.PuffActor? Attack(
        VmPMapLineAttack1169Slice.Shooter shooter,
        VmPMapLineAttack1169Slice.AttackHost host,
        VmPMapLineAttack1169Slice.LineTarget? victim,
        VmPMapLineAttack1169Slice.DamageSlot? damage,
        int flags,
        double forward,
        double side,
        string damageType)
    {
        host.Defaults.DamageType = host.Defaults.DamageType == "None" ? "Fire" : host.Defaults.DamageType;
        return VmPMapLineAttack1169Slice.LineAttack(
            shooter,
            0,
            1024,
            0,
            5,
            damageType,
            "BulletPuff",
            flags,
            victim,
            damage,
            4,
            forward,
            side,
            host);
    }
}
