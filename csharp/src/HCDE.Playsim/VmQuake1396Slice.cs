namespace HCDE.Playsim;

/// <summary>
/// Earthquake damage, wave, intensity, and falloff from <c>a_quake.cpp</c> 165-334.
/// Chunk 1396. <c>StaticGetQuakeIntensities</c> starts at line 335 and stays outside.
/// </summary>
public static class VmQuake1396Slice
{
    public const int QfRelative = 1;
    public const int QfScaledown = 1 << 1;
    public const int QfScaleup = 1 << 2;
    public const int QfMax = 1 << 3;
    public const int QfFullIntensity = 1 << 4;
    public const int QfShakeOnly = 1 << 9;
    public const int Mf7DontThrust = 0x100;
    public const int TicRate = 35;
    public const int StaticIntensityLine = 335;
    public const double Pi = 3.14159265358979323846;
    public const string QuakeDamage = "Quake";

    public sealed class Quake
    {
        public int Flags;
        public double DamageRadius;
        public double Falloff;
        public double DamageMultiplier = 1;
        public double ThrustMultiplier;
        public int Damage;
        public double IntensityX;
        public double IntensityY;
        public int Countdown;
        public int CountdownStart;
        public int Highpoint;
        public int MiniCount;
        public double Distance;
    }

    public sealed class Victim
    {
        public double Z;
        public double FloorZ;
        public int Flags7;
        public double Yaw;
        public double VelX;
        public double VelY;
    }

    public readonly record struct Hit(int Amount, string Type);

    public static void DoQuakeDamage(Quake? quake, Victim? victim, bool falloff, Func<int> nextRandom, Func<int, int> hitDice, List<Hit> hits)
    {
        if (quake == null || victim == null)
            return;
        if (nextRandom == null || hitDice == null || hits == null)
            throw new ArgumentNullException(nameof(nextRandom));
        double dist = quake.Distance;
        double thrustFalloff = falloff ? GetFalloff(quake, dist, quake.DamageRadius) : 1.0;
        if (dist < quake.DamageRadius && victim.Z <= victim.FloorZ)
        {
            if ((quake.Flags & QfShakeOnly) == 0 && nextRandom() < 50)
            {
                int damage;
                if (quake.Damage < 1)
                {
                    double dice = hitDice(1);
                    damage = falloff
                        ? (int)(dice * GetFalloff(quake, dist, quake.DamageRadius) * quake.DamageMultiplier)
                        : (int)(dice * quake.DamageMultiplier);
                }
                else
                {
                    damage = falloff
                        ? (int)(quake.Damage * GetFalloff(quake, dist, quake.DamageRadius) * quake.DamageMultiplier)
                        : (int)(quake.Damage * quake.DamageMultiplier);
                }
                if (damage < 1)
                    damage = 1;
                hits.Add(new Hit(damage, QuakeDamage));
            }
            if ((victim.Flags7 & Mf7DontThrust) == 0 && quake.ThrustMultiplier > 0)
            {
                double degrees = victim.Yaw + nextRandom();
                double radians = degrees * (Pi / 180.0);
                victim.VelX += quake.IntensityX * Math.Cos(radians) * quake.ThrustMultiplier * thrustFalloff;
                victim.VelY += quake.IntensityY * Math.Sin(radians) * quake.ThrustMultiplier * thrustFalloff;
            }
        }
    }

    public static double GetModWave(Quake quake, double ticFrac, double waveMultiplier)
    {
        if (quake == null)
            throw new ArgumentNullException(nameof(quake));
        double time = quake.Countdown - ticFrac;
        return Math.Sin(waveMultiplier * time * (Pi * 2 / TicRate));
    }

    public static double GetModIntensity(Quake quake, double intensity, bool fake)
    {
        if (quake == null)
            throw new ArgumentNullException(nameof(quake));
        if (quake.CountdownStart < quake.Countdown)
            throw new InvalidOperationException("countdown");
        intensity += intensity;
        if ((quake.Flags & (QfScaledown | QfScaleup)) != 0)
        {
            bool check = quake.Highpoint > 0 && quake.Highpoint < quake.CountdownStart;
            int divider = check ? quake.Highpoint : quake.CountdownStart;
            int scalar;
            if ((quake.Flags & (QfScaledown | QfScaleup)) == (QfScaledown | QfScaleup))
            {
                if (check)
                {
                    if (quake.MiniCount > 0)
                        scalar = (quake.Flags & QfMax) != 0 ? quake.MiniCount : quake.Highpoint - quake.MiniCount;
                    else
                    {
                        divider = quake.CountdownStart - quake.Highpoint;
                        scalar = (quake.Flags & QfMax) != 0 ? divider - quake.Countdown : quake.Countdown;
                    }
                }
                else
                {
                    divider = quake.CountdownStart;
                    scalar = (quake.Flags & QfMax) != 0
                        ? Math.Max(quake.Countdown, quake.CountdownStart - quake.Countdown)
                        : Math.Min(quake.Countdown, quake.CountdownStart - quake.Countdown);
                }
                scalar = scalar > divider ? divider : scalar;
                if (!fake && (quake.Flags & QfFullIntensity) != 0)
                    scalar *= 2;
            }
            else
            {
                if ((quake.Flags & QfScaledown) != 0)
                    scalar = quake.Countdown;
                else
                {
                    scalar = quake.CountdownStart - quake.Countdown;
                    if (quake.Highpoint > 0)
                    {
                        if (quake.Highpoint - quake.MiniCount < divider)
                            scalar = quake.Highpoint - quake.MiniCount;
                        else
                            scalar = divider;
                    }
                }
                scalar = scalar > divider ? divider : scalar;
            }
            if (divider <= 0)
                throw new InvalidOperationException("divider");
            intensity = intensity * scalar / divider;
        }
        return intensity;
    }

    public static double GetFalloff(Quake quake, double dist, double radius)
    {
        if (quake == null)
            throw new ArgumentNullException(nameof(quake));
        if (dist < quake.Falloff || quake.Falloff >= radius || quake.Falloff <= 0 || radius - quake.Falloff <= 0)
            return 1;
        if (dist > quake.Falloff && dist < radius)
            return 1 - ((dist - quake.Falloff) / (radius - quake.Falloff));
        return 1;
    }
}
