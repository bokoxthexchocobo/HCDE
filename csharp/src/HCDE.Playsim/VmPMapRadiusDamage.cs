namespace HCDE.Playsim;

/// <summary>Faithful port of radius falloff cluster (<c>p_map.cpp</c> 6130–6480).</summary>
public static class VmPMapRadiusDamage
{
    public const int MfShootable = 1;
    public const int Mf6Vulnerable = 1;
    public const int Mf3Noradiusdmg = 1;
    public const int Mf4Forceradiusdmg = 1;
    public const int Mf4Dontharmclass = 1;
    public const int Mf6Dontharmspecies = 1;
    public const int Mf5Oldradiusdmg = 1;
    public const int Mf7Forcezeroradiusdmg = 1;
    public const int Mf7Dontthrust = 1;
    public const int Mf2Blasted = 1;
    public const int MfIcecorpse = 1 << 1;
    public const int RadfHurtsource = 1;
    public const int RadfNoallies = 1 << 1;
    public const int RadfNodamage = 1 << 2;
    public const int RadfOldradiusdamage = 1 << 3;
    public const int RadfCircular = 1 << 4;
    public const int RadfThrustless = 1 << 5;
    public const int RadfCirculartThrust = 1 << 6;
    public const int RadfThrustz = 1 << 7;
    public const int RadfSourceisspot = 1 << 8;
    public const int SfIgnorevisibility = 1;

    public readonly record struct ActorRef(string Id, double X, double Y, double Z, double Top, double Radius, int Mass, int Flags, int Flags2, int Flags3, int Flags4, int Flags5, int Flags6, int Flags7, bool Player, bool NoRadiusDamage, bool DontThrust, bool CanTakeDamage, string Species, string ClassName, double RadiusDamageFactor);

    public static double GetRadiusDamage(bool fromAction, ActorRef bomb, ActorRef thing, int bombDamage, double bombDistance, double fullDamageDistance, bool thingIsBombSource, bool round, double splashFactor = 1)
    {
        var bombDistanceFloat = 1.0 / (bombDistance - fullDamageDistance);
        double len;
        if (!round)
        {
            var dx = Math.Abs(bomb.X - thing.X);
            var dy = Math.Abs(bomb.Y - thing.Y);
            var boxRadius = thing.Radius;
            len = Math.Max(dx, dy);
            if (bomb.Z < thing.Z || bomb.Z >= thing.Top)
            {
                var dz = bomb.Z > thing.Z ? bomb.Z - thing.Top : thing.Z - bomb.Z;
                len = len <= boxRadius ? dz : Math.Sqrt(Math.Max(0, len - boxRadius) * Math.Max(0, len - boxRadius) + dz * dz);
            }
            else len = Math.Max(0, len - boxRadius);
        }
        else len = Distance3D(bomb, thing);

        len = Math.Clamp(len - fullDamageDistance, 0, len);
        var points = bombDamage * (1.0 - len * bombDistanceFloat);
        if (!fromAction)
        {
            if (thingIsBombSource) points *= splashFactor;
            points *= thing.RadiusDamageFactor;
        }
        return points;
    }

    static double Distance3D(ActorRef a, ActorRef b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static int GetOldRadiusDamage(bool fromAction, ActorRef bomb, ActorRef thing, int bombDamage, double bombDistance, double fullDamageDistance, double splashFactor = 1)
    {
        var dx = Math.Abs(bomb.X - thing.X);
        var dy = Math.Abs(bomb.Y - thing.Y);
        var dist = Math.Max(dx, dy) - thing.Radius;
        if (dist < 0) dist = 0;
        if (dist >= bombDistance) return fromAction ? 0 : -1;
        dist = Math.Clamp(dist - fullDamageDistance, 0, dist);
        var damage = (int)(bombDamage * (bombDistance - dist) / bombDistance);
        if (!fromAction) damage = (int)(damage * splashFactor * thing.RadiusDamageFactor);
        return damage;
    }

    public static int GetRadiusDamageAction(ActorRef self, ActorRef thing, int damage, double distance, double fullDmgDistance, bool oldRadius, bool circular)
    {
        if (self.Id == thing.Id) return damage;
        fullDmgDistance = Math.Clamp(fullDmgDistance, 0, Math.Max(0, distance - 1));
        if (distance <= 0) distance = damage;
        return oldRadius
            ? GetOldRadiusDamage(true, self, thing, damage, distance, fullDmgDistance)
            : (int)GetRadiusDamage(true, self, thing, damage, distance, fullDmgDistance, false, circular);
    }

    public sealed class RadiusAttackState
    {
        public ActorRef BombSpot { get; set; }
        public ActorRef? BombSource { get; set; }
        public int BombDamage { get; set; }
        public double BombDistance { get; set; }
        public double FullDamageDistance { get; set; }
        public int Flags { get; set; }
        public string Species { get; set; } = "";
        public double SplashFactor { get; set; } = 1;
        public double SelfThrustScale { get; set; } = 1;
        public int CompatExplode2 { get; set; }
        public List<ActorRef> Candidates { get; } = new();
        public List<ActorRef> Targets { get; } = new();
        public int DamageCount { get; set; }
        public Func<ActorRef, bool>? CheckSight { get; set; }
        public Func<ActorRef, ActorRef, int, int>? DamageMobj { get; set; }
        public Action<ActorRef, ActorRef, int>? TraceBleed { get; set; }
        public Action<ActorRef, double, double, double>? Thrust { get; set; }
        public Action<ActorRef, double>? AddVelZ { get; set; }
        public Func<ActorRef, ActorRef, bool>? IsFriend { get; set; }
        public Func<ActorRef, ActorRef, bool>? ShouldPassThroughPlayer { get; set; }
        public Func<ActorRef, bool>? MirrorActor { get; set; }
    }

    public static int RadiusAttack(RadiusAttackState s)
    {
        if (s.BombDistance <= 0) return 0;
        s.FullDamageDistance = Math.Clamp(s.FullDamageDistance, 0, s.BombDistance - 1);
        if ((s.Flags & RadfSourceisspot) != 0)
            s.BombSource = s.BombSpot;
        var source = s.BombSource ?? s.BombSpot;
        if (s.MirrorActor?.Invoke(s.BombSpot) == true || s.MirrorActor?.Invoke(source) == true)
            return 0;

        foreach (var thing in s.Candidates)
        {
            if (s.MirrorActor?.Invoke(thing) == true) continue;
            if ((thing.Flags & MfShootable) == 0 && (thing.Flags6 & Mf6Vulnerable) == 0) continue;
            if ((thing.Flags3 & Mf3Noradiusdmg) != 0 && (s.BombSpot.Flags4 & Mf4Forceradiusdmg) == 0) continue;
            if ((s.Flags & RadfHurtsource) == 0 && (thing.Id == source.Id || thing.Id == s.BombSpot.Id)) continue;
            if (!string.IsNullOrEmpty(s.Species) && thing.Species != s.Species) continue;
            if ((s.Flags & RadfNoallies) != 0 && s.IsFriend?.Invoke(source, thing) == true
                && thing.Id != source.Id && thing.Id != s.BombSpot.Id) continue;
            if (source.Player && s.ShouldPassThroughPlayer?.Invoke(source, thing) == true) continue;
            s.Targets.Add(thing);
        }

        foreach (var thing in s.Targets)
        {
            var modern = (s.Flags & RadfNodamage) == 0
                && (((s.BombSpot.Flags5 | thing.Flags5) & Mf5Oldradiusdmg) == 0)
                && (s.Flags & RadfOldradiusdamage) == 0
                && s.CompatExplode2 == 0;
            if (modern)
            {
                var points = GetRadiusDamage(false, s.BombSpot, thing, s.BombDamage, s.BombDistance, s.FullDamageDistance, thing.Id == source.Id, (s.Flags & RadfCircular) != 0, s.SplashFactor);
                var check = (int)points * s.BombDamage;
                if ((check > 0 || (check == 0 && (s.BombSpot.Flags7 & Mf7Forcezeroradiusdmg) != 0))
                    && s.CheckSight?.Invoke(thing) == true)
                {
                    var damage = Math.Abs((int)points);
                    var newDam = s.DamageMobj?.Invoke(thing, s.BombSpot, damage) ?? 0;
                    if (newDam > 0 || damage > 0) s.DamageCount++;
                    ApplyThrust(s, thing, source, points, damage, newDam);
                }
            }
            else
            {
                var damage = GetOldRadiusDamage(false, s.BombSpot, thing, s.BombDamage, s.BombDistance, s.FullDamageDistance, s.SplashFactor);
                if (damage < 0) continue;
                if (damage > 0 || (s.BombSpot.Flags7 & Mf7Forcezeroradiusdmg) != 0)
                {
                    var newDam = s.DamageMobj?.Invoke(thing, s.BombSpot, damage) ?? 0;
                    s.TraceBleed?.Invoke(thing, s.BombSpot, newDam > 0 ? newDam : damage);
                    s.DamageCount++;
                }
            }
        }

        return s.DamageCount;
    }

    static void ApplyThrust(RadiusAttackState s, ActorRef thing, ActorRef source, double points, int damage, int newDam)
    {
        if ((thing.Flags2 & MfIcecorpse) != 0) return;
        if ((s.Flags & RadfThrustless) != 0 || (thing.Flags7 & Mf7Dontthrust) != 0) return;
        var thrust = points * 0.5 / thing.Mass;
        if (thing.Id == source.Id) thrust *= s.SelfThrustScale;
        if ((s.Flags & RadfCirculartThrust) != 0)
        {
            var dx = thing.X - s.BombSpot.X;
            var dy = thing.Y - s.BombSpot.Y;
            var len = Math.Sqrt(dx * dx + dy * dy);
            if (len > 1e-6) s.Thrust?.Invoke(thing, dx / len * thrust, dy / len * thrust, 0);
        }
        else
        {
            var angle = Math.Atan2(thing.Y - s.BombSpot.Y, thing.X - s.BombSpot.X);
            s.Thrust?.Invoke(thing, Math.Cos(angle) * thrust, Math.Sin(angle) * thrust, 0);
            if ((s.Flags & RadfNodamage) != 0 || (s.Flags & RadfThrustz) != 0)
            {
                var vz = (thing.Z - s.BombSpot.Z) * thrust * (thing.Id == source.Id ? 0.8 : 0.5);
                s.AddVelZ?.Invoke(thing, vz);
            }
        }
    }
}
