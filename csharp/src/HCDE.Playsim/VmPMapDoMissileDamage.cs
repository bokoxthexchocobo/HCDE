namespace HCDE.Playsim;

/// <summary>Rip/infight helpers and <c>P_DoMissileDamage</c> from <c>p_map.cpp</c> (1214–1366).</summary>
public static class VmPMapDoMissileDamage
{
    public const int Mf2Rip = 1;
    public const int Mf4Strifedamage = 1;
    public const int Mf6Forcepain = 1;
    public const int Mf7Causepain = 1;
    public const int Mf3Bloodlessimpact = 1;
    public const int MfNoblood = 1;
    public const int Mf2Invulnerable = 2;
    public const int Mf2Dormant = 4;
    public const int Mf2Reflective = 8;
    public const int Mf5Bloodsplatter = 1;
    public const int Mf7Forceinfighting = 1;
    public const int Mf7Harmfriends = 1;
    public const int Mf3Ismonster = 1;
    public const int MfShootable = 4;
    public const int Mf6Doharmspecies = 1;
    public const int PgGroupless = -1;
    public const int PgDefault = 0;

    public static bool CheckRipLevel(int victimRipMin, int victimRipMax, int projectileRipLevel)
    {
        if (victimRipMin > 0 && projectileRipLevel < victimRipMin) return false;
        if (victimRipMax > 0 && projectileRipLevel > victimRipMax) return false;
        return true;
    }

    public static bool ProjectileImmune(int targetProjectileGroup, int sourceProjectileGroup, bool sameActor, bool sameSpecies, bool doHarmSpecies)
    {
        if (targetProjectileGroup == PgGroupless && !sameActor)
            return false;
        if (targetProjectileGroup == PgDefault && sameSpecies && !doHarmSpecies)
            return true;
        if (targetProjectileGroup != PgDefault && targetProjectileGroup == sourceProjectileGroup)
            return true;
        return false;
    }

    public static bool CanAttackHurt(CanAttackHurtState s)
    {
        if (s.VictimPlayer || s.ShooterPlayer)
            return true;

        var infight = s.Infight;
        if (infight < 0 && (s.VictimFlags7 & Mf7Forceinfighting) != 0)
            infight = 0;

        if (infight < 0)
        {
            if ((s.ShooterFlags & MfShootable) != 0)
            {
                if ((s.VictimFlags3 & Mf3Ismonster) != 0)
                {
                    if (!s.VictimHostileToShooter)
                    {
                        if (s.VictimTid == 0 || s.ShooterTidToHate != s.VictimTid)
                            return false;
                    }
                }
            }
        }
        else if (infight == 0)
        {
            if (s.VictimFriendOfShooter)
            {
                if ((s.ShooterFlags7 & Mf7Harmfriends) == 0)
                    return false;
            }
            else
            {
                if (s.VictimTidToHate != 0 && s.VictimTidToHate == s.ShooterTidToHate)
                    return false;

                if (ProjectileImmune(s.VictimProjectileGroup, s.ShooterProjectileGroup, false, s.SameSpecies, s.VictimDoHarmSpecies))
                {
                    if (!s.VictimHostileToShooter)
                    {
                        if (s.VictimTid == 0 || s.ShooterTidToHate != s.VictimTid)
                            return false;
                    }
                }
            }
        }

        return true;
    }

    public sealed class CanAttackHurtState
    {
        public bool VictimPlayer { get; init; }
        public bool ShooterPlayer { get; init; }
        public int Infight { get; init; }
        public int VictimFlags7 { get; init; }
        public int ShooterFlags { get; init; }
        public int VictimFlags3 { get; init; }
        public bool VictimHostileToShooter { get; init; }
        public int VictimTid { get; init; }
        public int ShooterTidToHate { get; init; }
        public bool VictimFriendOfShooter { get; init; }
        public int ShooterFlags7 { get; init; }
        public int VictimTidToHate { get; init; }
        public int VictimProjectileGroup { get; init; }
        public int ShooterProjectileGroup { get; init; }
        public bool SameSpecies { get; init; }
        public bool VictimDoHarmSpecies { get; init; }
    }

    public sealed class MissileDamageSelf
    {
        public int Flags2 { get; init; }
        public int Flags3 { get; init; }
        public int Flags4 { get; init; }
        public int Flags5 { get; init; }
        public int Flags6 { get; init; }
        public int Flags7 { get; init; }
        public int PoisonDamage { get; init; }
        public int PoisonDuration { get; init; }
        public int PoisonPeriod { get; init; }
        public string PoisonDamageType { get; init; } = "";
        public object? Target { get; init; }
        public string DamageType { get; init; } = "";
        public int RipperLevel { get; init; }
        public Func<int, int, int>? GetMissileDamage { get; init; }
        public Action<object, MissileDamageSelf, object?, int, int, int, string>? PoisonMobj { get; init; }
        public Action<MissileDamageSelf, string>? PlayRipSound { get; init; }
        public Func<object, MissileDamageSelf, object?, int, string, int>? DamageMobj { get; init; }
        public Action<MissileDamageSelf, object>? RipperBlood { get; init; }
        public Action<(double X, double Y, double Z), object, double>? BloodSplatter { get; init; }
        public Func<double>? AngleToVictim { get; init; }
        public Func<int>? PrCheckThing { get; init; }
        public Action<int, object, MissileDamageSelf>? TraceBleed { get; init; }
        public Action<object, int>? GiveBody { get; init; }
    }

    public sealed class MissileDamageVictim
    {
        public int Flags { get; init; }
        public int Flags2 { get; init; }
        public int Flags3 { get; init; }
    }

    public static void DoMissileDamage(MissileDamageSelf self, MissileDamageVictim victim)
    {
        var ripper = (self.Flags2 & Mf2Rip) != 0;

        if (self.PoisonDamage > 0 && self.PoisonDuration != int.MinValue)
            self.PoisonMobj?.Invoke(victim, self, self.Target, self.PoisonDamage, self.PoisonDuration, self.PoisonPeriod, self.PoisonDamageType);

        var damage = ripper
            ? self.GetMissileDamage?.Invoke(3, 2) ?? 0
            : self.GetMissileDamage?.Invoke((self.Flags4 & Mf4Strifedamage) != 0 ? 3 : 7, 1) ?? 0;

        if (damage > 0 || (self.Flags6 & Mf6Forcepain) != 0 || (self.Flags7 & Mf7Causepain) != 0)
        {
            if (ripper)
                self.PlayRipSound?.Invoke(self, "rip");

            var dealt = self.DamageMobj?.Invoke(victim, self, self.Target, damage, self.DamageType) ?? 0;
            if (damage > 0 && (self.Flags3 & Mf3Bloodlessimpact) == 0 &&
                (victim.Flags & MfNoblood) == 0 &&
                (victim.Flags2 & (Mf2Invulnerable | Mf2Dormant | Mf2Reflective)) == 0)
            {
                if (ripper)
                    self.RipperBlood?.Invoke(self, victim);
                else if ((self.Flags5 & Mf5Bloodsplatter) != 0 && (self.PrCheckThing?.Invoke() ?? 255) < 192)
                    self.BloodSplatter?.Invoke((0, 0, 0), victim, self.AngleToVictim?.Invoke() ?? 0);

                self.TraceBleed?.Invoke(dealt > 0 ? dealt : damage, victim, self);
            }
        }
        else
            self.GiveBody?.Invoke(victim, -damage);
    }
}
