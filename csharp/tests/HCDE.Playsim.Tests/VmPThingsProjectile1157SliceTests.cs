namespace HCDE.Playsim.Tests;

public class VmPThingsProjectile1157SliceTests
{
    [Fact]
    public void Projectile_BlocksMonstersAndZerosHuggerVelocityAfterSpeed()
    {
        var none = VmPThingsProjectile1157Slice.Projectile(
            false, VmPThingsProjectile1157Slice.IsMonster, VmPThingsProjectile1157Slice.NoMonsters, 0,
            0, 0, 0, 3, 4, 0, 9, false, true, Origin(1, 10, 0), Array.Empty<VmPThingsProjectile1157Slice.Origin>(),
            false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), Array.Empty<VmPThingsProjectile1157Slice.Shot>());
        Assert.False(none.Any);

        var blocked = VmPThingsProjectile1157Slice.Projectile(
            true, VmPThingsProjectile1157Slice.IsMonster, 0, VmPThingsProjectile1157Slice.LevelNoMonsters,
            0, 0, 0, 3, 4, 0, 9, true, true, Origin(1, 10, 3), Array.Empty<VmPThingsProjectile1157Slice.Origin>(),
            false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), Array.Empty<VmPThingsProjectile1157Slice.Shot>());
        Assert.False(blocked.Any);

        var shot = new VmPThingsProjectile1157Slice.Shot(
            true, VmPThingsProjectile1157Slice.Missile | VmPThingsProjectile1157Slice.Special | VmPThingsProjectile1157Slice.NoGravity,
            0, 0, 1, 8, true, true, 0, 0, 0, 0);
        var lob = VmPThingsProjectile1157Slice.Projectile(
            true, 0, 0, 0, 0, 0, 0, 3, 4, 0, 9, true, true, Origin(4, 10, 3), Array.Empty<VmPThingsProjectile1157Slice.Origin>(),
            false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), new[] { shot });
        var one = lob.Actors[0];
        Assert.True(lob.Any);
        Assert.Equal(7, one.Z);
        Assert.Equal(3, one.VelX);
        Assert.Equal(0, one.VelY);
        Assert.Equal(4, one.VelZ);
        Assert.Equal(5, one.Speed);
        Assert.Equal(9, one.Tid);
        Assert.Equal(4, one.TargetId);
        Assert.Equal(-1, one.TracerId);
        Assert.False(one.Aimed);
        Assert.True(one.PlayedSpawn);
        Assert.Equal(VmPThingsProjectile1157Slice.Missile | VmPThingsProjectile1157Slice.Special | VmPThingsProjectile1157Slice.Dropped | VmPThingsProjectile1157Slice.NoGravity, one.Flags);

        var hugged = shot with { Flags3 = VmPThingsProjectile1157Slice.FloorHugger };
        var flat = VmPThingsProjectile1157Slice.Projectile(
            true, VmPThingsProjectile1157Slice.FloorHugger, 0, 0, 0, 0, 0, 3, 4, 1, 2, false,
            true, Origin(4, 10, 3), Array.Empty<VmPThingsProjectile1157Slice.Origin>(),
            false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), new[] { hugged });
        Assert.Equal(VmPThingsProjectile1157Slice.OnFloorZ, flat.Actors[0].Z);
        Assert.Equal(0, flat.Actors[0].VelZ);
        Assert.Equal(5, flat.Actors[0].Speed);
        Assert.Equal(VmPThingsProjectile1157Slice.EighthGravity, flat.Actors[0].Gravity);
        Assert.Equal(0, flat.Actors[0].Flags & VmPThingsProjectile1157Slice.NoGravity);

        var aimed = shot with { Flags2 = VmPThingsProjectile1157Slice.Seeker, InterceptX = 6, InterceptY = 0, InterceptZ = 8, InterceptYaw = 12 };
        var lead = VmPThingsProjectile1157Slice.Projectile(
            true, 0, 0, 0, 7, 3, 0, 3, 4, 2, 2, true, false, default,
            new[] { Origin(5, 1, 0), Origin(6, 2, 0) }, true, default,
            new[] { new VmPThingsProjectile1157Slice.Aim(20), new VmPThingsProjectile1157Slice.Aim(21) },
            new[] { aimed, aimed, aimed with { MissileFits = false }, aimed });
        Assert.Equal(4, lead.Actors.Count);
        Assert.True(lead.Any);
        Assert.Equal(20, lead.Actors[0].TracerId);
        Assert.Equal(6, lead.Actors[0].VelX);
        Assert.Equal(8, lead.Actors[0].VelZ);
        Assert.Equal(12, lead.Actors[0].Yaw);
        Assert.True(lead.Actors[0].Lead);
        Assert.Equal(10, lead.Actors[0].Speed);
        Assert.Equal(5, lead.Actors[0].TargetId);
        Assert.Equal(21, lead.Actors[1].TracerId);
        Assert.Equal(5, lead.Actors[1].TargetId);
        Assert.Equal(6, lead.Actors[3].TargetId);
        Assert.Equal(21, lead.Actors[3].TracerId);
        Assert.False(lead.Actors[2].Alive);
        Assert.True(lead.Actors[2].Destroyed);
        Assert.False(lead.Actors[2].Cleared);
        Assert.Equal(1, lead.Actors[0].Gravity);

        var emptyDest = VmPThingsProjectile1157Slice.Projectile(
            true, 0, 0, 0, 7, 3, 0, 3, 4, 0, 2, false, false, default,
            new[] { Origin(5, 1, 0) }, false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), new[] { shot });
        Assert.False(emptyDest.Any);
        Assert.Empty(emptyDest.Actors);

        var stuck = shot with { Flags = 0, LocationOk = false };
        var removed = VmPThingsProjectile1157Slice.Projectile(
            true, VmPThingsProjectile1157Slice.CeilingHugger, 0, 0, 0, 0, 90, 3, 4, 0, 2, false,
            true, Origin(8, 1, 0), Array.Empty<VmPThingsProjectile1157Slice.Origin>(),
            false, default, Array.Empty<VmPThingsProjectile1157Slice.Aim>(), new[] { stuck });
        Assert.False(removed.Any);
        Assert.True(removed.Actors[0].Destroyed);
        Assert.True(removed.Actors[0].Cleared);
        Assert.Equal(VmPThingsProjectile1157Slice.OnCeilingZ, removed.Actors[0].Z);
    }

    [Fact]
    public void Damage_HealsUpToSpawnHealthAndCountsAFullActor()
    {
        var victims = new[]
        {
            new VmPThingsProjectile1157Slice.Victim(true, 90, 100, true, 90),
            new VmPThingsProjectile1157Slice.Victim(true, 10, 10, false, 0),
            new VmPThingsProjectile1157Slice.Victim(false, 4, 10, false, 0),
        };
        var healed = VmPThingsProjectile1157Slice.Damage(4, -30, victims);
        Assert.Equal(2, healed.Count);
        Assert.Equal(100, healed.Hits[0].Health);
        Assert.Equal(100, healed.Hits[0].PlayerHealth);
        Assert.True(healed.Hits[0].Healed);
        Assert.False(healed.Hits[1].Healed);
        Assert.Equal(10, healed.Hits[1].Health);
        Assert.Equal(2, healed.Hits.Count);

        var hurt = VmPThingsProjectile1157Slice.Damage(4, 6, victims);
        Assert.Equal(2, hurt.Count);
        Assert.True(hurt.Hits[0].Dealt);
        Assert.Equal(90, hurt.Hits[0].Health);

        var only = VmPThingsProjectile1157Slice.Damage(0, -5, victims);
        Assert.Equal(1, only.Count);
        Assert.Equal(95, only.Hits[0].Health);
        Assert.Equal(95, only.Hits[0].PlayerHealth);
    }

    [Fact]
    public void Raise_RestoresABlockedCorpseAndKeepsSizeWhenResurrectFails()
    {
        var missing = VmPThingsProjectile1157Slice.Raise(false, true, 4, 5, 6, 0, 1, 2, 10, 11, 0, true, true, true);
        Assert.False(missing.Rose);
        Assert.Equal(4, missing.VelX);

        var blocked = VmPThingsProjectile1157Slice.Raise(true, true, 4, 5, 6, 0, 1, 2, 10, 11, 0, false, true, true);
        Assert.False(blocked.Rose);
        Assert.True(blocked.Restored);
        Assert.Equal(0, blocked.VelX);
        Assert.Equal(0, blocked.VelY);
        Assert.Equal(6, blocked.VelZ);
        Assert.Equal(0, blocked.Flags);
        Assert.Equal(1, blocked.Height);
        Assert.False(blocked.CheckedResurrect);

        var refused = VmPThingsProjectile1157Slice.Raise(true, true, 4, 5, 6, 0, 1, 2, 10, 11, 0, true, false, true);
        Assert.False(refused.Rose);
        Assert.False(refused.Restored);
        Assert.True(refused.CheckedResurrect);
        Assert.Equal(VmPThingsProjectile1157Slice.Solid, refused.Flags);
        Assert.Equal(10, refused.Height);
        Assert.Equal(11, refused.Radius);

        var raised = VmPThingsProjectile1157Slice.Raise(
            true, true, 4, 5, 6, 0, 1, 2, 10, 11, VmPThingsProjectile1157Slice.TransferFriend, true, true, true);
        Assert.True(raised.Rose);
        Assert.True(raised.Played);
        Assert.Equal(VmPThingsProjectile1157Slice.BodyChannel, raised.Channel);
        Assert.Equal(VmPThingsProjectile1157Slice.IdleAttenuation, raised.Attenuation);
        Assert.True(raised.Revived);
        Assert.True(raised.CopiedFriend);
        Assert.False(raised.ChangeTarget);
        Assert.True(raised.ResetHealth);
        Assert.True(raised.EnteredRaise);
        Assert.Equal(VmPThingsProjectile1157Slice.Solid, raised.Flags);

        var alone = VmPThingsProjectile1157Slice.Raise(
            true, true, 1, 1, 1, 0, 1, 2, 10, 11, VmPThingsProjectile1157Slice.TransferFriend | VmPThingsProjectile1157Slice.NoCheckPosition,
            false, true, false);
        Assert.True(alone.Rose);
        Assert.False(alone.CopiedFriend);

        var noState = VmPThingsProjectile1157Slice.CanRaise(false, 0, 1, 2, 10, 11, true);
        Assert.False(noState.Can);
        Assert.Equal(1, noState.Height);
        var open = VmPThingsProjectile1157Slice.CanRaise(true, 0, 1, 2, 10, 11, true);
        Assert.True(open.Can);
        Assert.Equal(0, open.Flags);
        Assert.Equal(1, open.Height);
        Assert.Equal(2, open.Radius);
        Assert.False(VmPThingsProjectile1157Slice.CanRaise(true, 0, 1, 2, 10, 11, false).Can);
    }

    [Fact]
    public void RemoveAndVelocity_SpareTheLivePlayerAndReplaceBob()
    {
        Assert.False(VmPThingsProjectile1157Slice.Remove(true, true, true).Removed);
        var corpse = VmPThingsProjectile1157Slice.Remove(true, false, true);
        Assert.True(corpse.Removed);
        Assert.True(corpse.Cleared);
        Assert.False(VmPThingsProjectile1157Slice.Remove(false, false, false).Removed);
        Assert.True(VmPThingsProjectile1157Slice.Remove(false, false, true).Removed);

        var held = VmPThingsProjectile1157Slice.SetVelocity(false, true, 9, 8, 7, 1, 2, 3, 4, 5, 6, false, true);
        Assert.Equal(9, held.X);
        Assert.Equal(3, held.PlayerZ);

        var replaced = VmPThingsProjectile1157Slice.SetVelocity(true, true, 9, 8, 7, 1, 2, 3, 4, 5, 6, false, true);
        Assert.Equal(4, replaced.X);
        Assert.Equal(5, replaced.Y);
        Assert.Equal(6, replaced.Z);
        Assert.Equal(4, replaced.PlayerX);
        Assert.Equal(5, replaced.PlayerY);
        Assert.Equal(0, replaced.PlayerZ);

        var added = VmPThingsProjectile1157Slice.SetVelocity(true, true, 10, 0, 0, 1, 2, 3, 4, 5, 6, true, true);
        Assert.Equal(14, added.X);
        Assert.Equal(5, added.Y);
        Assert.Equal(6, added.Z);
        Assert.Equal(5, added.PlayerX);
        Assert.Equal(7, added.PlayerY);
        Assert.Equal(3, added.PlayerZ);

        var cleared = VmPThingsProjectile1157Slice.SetVelocity(true, true, 9, 8, 7, 1, 2, 3, 4, 5, 6, false, false);
        Assert.Equal(4, cleared.X);
        Assert.Equal(0, cleared.PlayerX);
        Assert.Equal(0, cleared.PlayerZ);
    }

    private static VmPThingsProjectile1157Slice.Origin Origin(int id, double z, double floorClip)
    {
        return new VmPThingsProjectile1157Slice.Origin(id, 2, 3, z, floorClip, 16);
    }
}
