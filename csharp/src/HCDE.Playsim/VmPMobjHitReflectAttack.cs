namespace HCDE.Playsim;

/// <summary>Missile hit virtuals, reflection, inventory walks, sounds, and attack eligibility from <c>p_mobj.cpp</c> (3830–4019).</summary>
public static class VmPMobjHitReflectAttack
{
    public const int Mf2Dontreflect = 1;
    public const int Mf4Shieldreflect = 1;
    public const int Mf4Deflect = 1;
    public const int Mf7Thrureflect = 1;
    public const int Mf7Noshieldreflect = 1;
    public const int Mf7Aimreflect = 1;
    public const int Mf7Nevertarget = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf5Summonedmonster = 1;
    public const int MfMissile = 1;
    public const int MfFriendly = 1;
    public const int Mf3Screenseeker = 1;
    public const int MfShootable = 4;
    public const int Mf2Dormant = 1;
    public const int Mf3Fullvolactive = 1;

    public static int SpecialMissileHit(bool hasVirtual, Func<int>? vmCall) =>
        hasVirtual ? vmCall?.Invoke() ?? 0 : -1;

    public static int SpecialBounceHit(bool hasVirtual, Func<int>? vmCall) =>
        hasVirtual ? vmCall?.Invoke() ?? 0 : -1;

    public sealed class ReflectThing
    {
        public int Flags4 { get; set; }
        public int Flags7 { get; set; }
        public double YawDeg { get; set; }
        public bool HasTarget { get; set; }
    }

    public sealed class ReflectSelf
    {
        public int Flags2 { get; set; }
        public int Flags7 { get; set; }
        public bool HasTarget { get; set; }
        public Action? FaceSelfTarget { get; init; }
        public Action? FaceThingTarget { get; init; }
    }

    public static bool AdjustReflectionAngle(
        ReflectSelf self,
        ReflectThing thing,
        ref double angleDeg,
        int reflectRoll,
        Func<double, double, double>? absAngleDeg)
    {
        if ((self.Flags2 & Mf2Dontreflect) != 0)
            return true;
        if ((thing.Flags7 & Mf7Thrureflect) != 0)
            return false;

        if ((thing.Flags4 & Mf4Shieldreflect) != 0)
        {
            var diff = absAngleDeg?.Invoke(angleDeg, thing.YawDeg) ?? 0;
            if ((self.Flags7 & Mf7Noshieldreflect) != 0 || diff > 45)
                return true;
            angleDeg += reflectRoll < 128 ? 45 : -45;
        }
        else if ((thing.Flags4 & Mf4Deflect) != 0)
        {
            angleDeg += reflectRoll < 128 ? 45 : -45;
        }
        else
        {
            angleDeg += (reflectRoll % 16) - 8;
        }

        if ((thing.Flags7 & Mf7Aimreflect) != 0)
        {
            if (self.HasTarget)
                self.FaceSelfTarget?.Invoke();
            else if (thing.HasTarget)
                self.FaceThingTarget?.Invoke();
        }

        return false;
    }

    public sealed class InventoryItem
    {
        public InventoryItem? Inventory { get; set; }
        public bool HasAbsorbVirtual { get; set; }
        public bool HasAlterWeaponVirtual { get; set; }
    }

    public static int AbsorbDamage(
        InventoryItem? inventory,
        int damage,
        Action<InventoryItem, int>? absorbVirtual)
    {
        for (var item = inventory; item != null; item = item.Inventory)
        {
            if (item.HasAbsorbVirtual)
                absorbVirtual?.Invoke(item, damage);
        }
        return damage;
    }

    public static void AlterWeaponSprite(IReadOnlyList<InventoryItem> reverseItems, Action<InventoryItem>? alterVirtual)
    {
        for (var i = reverseItems.Count - 1; i >= 0; i--)
        {
            var item = reverseItems[i];
            if (item.HasAlterWeaponVirtual)
                alterVirtual?.Invoke(item);
        }
    }

    public static List<InventoryItem> CollectInventoryForward(InventoryItem? head)
    {
        var list = new List<InventoryItem>();
        for (var item = head; item != null; item = item.Inventory)
            list.Add(item);
        return list;
    }

    public sealed class SoundActor
    {
        public bool ActiveSoundValid { get; set; }
        public bool VoiceChannelBusy { get; set; }
        public int Flags3 { get; set; }
        public bool BodyChannelBusy { get; set; }
    }

    public static void PlayActiveSound(SoundActor self, Action<float>? playSound)
    {
        if (!self.ActiveSoundValid || self.VoiceChannelBusy)
            return;
        var atten = (self.Flags3 & Mf3Fullvolactive) != 0 ? 0f : 1f;
        playSound?.Invoke(atten);
    }

    public static void PlayPushSound(SoundActor self, Action? playSound)
    {
        if (self.BodyChannelBusy)
            return;
        playSound?.Invoke();
    }

    public sealed class AttackLink
    {
        public bool IsSelf { get; init; }
        public bool Player { get; init; }
        public bool Monster { get; init; }
        public int Flags { get; init; }
        public int Flags2 { get; init; }
        public int Flags3 { get; init; }
        public int Flags5 { get; init; }
        public int Flags7 { get; init; }
        public bool TracerMatchesFriend { get; init; }
        public bool LinkPlayer { get; init; }
        public bool FriendPlayer { get; init; }
        public bool FriendIsFriend { get; init; }
        public bool BothFriendly { get; init; }
        public bool IsFriend { get; init; }
    }

    public sealed class AttackSelf
    {
        public int Flags { get; init; }
        public int Flags5 { get; init; }
        public int Flags3 { get; init; }
        public bool FriendPlayer { get; init; }
        public AttackLink? SummonedFriend { get; init; }
        public AttackLink? MissileTargetFriend { get; init; }
        public AttackLink? FriendlyPlayerFriend { get; init; }
        public AttackLink? DefaultFriend { get; init; }
    }

    public static AttackLink? ResolveAttackFriend(AttackSelf self)
    {
        if ((self.Flags5 & Mf5Summonedmonster) != 0)
            return self.SummonedFriend;
        if ((self.Flags & MfMissile) != 0)
            return self.MissileTargetFriend;
        if ((self.Flags & MfFriendly) != 0 && self.FriendPlayer)
            return self.FriendlyPlayerFriend;
        return self.DefaultFriend;
    }

    public static bool IsOkayToAttack(
        AttackSelf self,
        AttackLink link,
        bool multiplayer,
        bool deathmatch,
        Func<bool>? checkSight,
        Func<double, double, double>? absAngleDeg,
        double friendYawDeg,
        double angleToLinkDeg)
    {
        if (link.IsSelf)
            return false;
        if (!link.Player && (link.Flags3 & Mf3Ismonster) == 0)
            return false;
        if ((link.Flags & MfShootable) == 0)
            return false;
        if ((link.Flags2 & Mf2Dormant) != 0)
            return false;
        if ((link.Flags7 & Mf7Nevertarget) != 0)
            return false;

        var friend = ResolveAttackFriend(self);
        if (friend == null || ReferenceEquals(friend, link))
            return false;
        if (friend.FriendIsFriend)
            return false;
        if ((link.Flags5 & Mf5Summonedmonster) != 0 && link.TracerMatchesFriend)
            return false;
        if (multiplayer && !deathmatch && link.LinkPlayer && friend.LinkPlayer)
            return false;
        if ((self.Flags & link.Flags & MfFriendly) != 0 && link.IsFriend)
            return false;

        if (checkSight?.Invoke() != true)
            return false;

        if ((self.Flags3 & Mf3Screenseeker) != 0)
        {
            var angle = absAngleDeg?.Invoke(angleToLinkDeg, friendYawDeg) ?? 999;
            var fov = 30 * (256.0 / 360.0);
            return angle < fov;
        }

        return true;
    }
}
