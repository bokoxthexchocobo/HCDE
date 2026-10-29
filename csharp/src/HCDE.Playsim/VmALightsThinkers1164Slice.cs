namespace HCDE.Playsim;

/// <summary>
/// Flicker, flash, strobe, glow, and phased light thinkers from <c>a_lights.cpp</c>
/// (<b>159–551</b>). Chunk <b>1164</b>.
/// </summary>
public static class VmALightsThinkers1164Slice
{
    public const int GlowSpeed = 8;
    public const int PhaseSteps = 12;
    public const int PhaseWrap = 63;
    public const int SequenceOne = 3;
    public const int SequenceTwo = 4;

    public sealed class Flicker
    {
        public int Max;
        public int Min;
        public int Count;
        public int Light;
    }

    public sealed class LightFlash
    {
        public int Max;
        public int Min;
        public int MaxTime;
        public int MinTime;
        public int Count;
        public int Light;
    }

    public sealed class Strobe
    {
        public int Max;
        public int Min;
        public int BrightTime;
        public int DarkTime;
        public int Count;
        public int Light;
    }

    public sealed class Glow
    {
        public int Max;
        public int Min;
        public int Direction;
        public int Light;
    }

    public sealed class Glow2
    {
        public int Start;
        public int End;
        public int MaxTics;
        public int Tics;
        public bool OneShot;
        public int Light;
        public bool Destroyed;
    }

    public sealed class PhaseSector
    {
        public int Light;
        public int Special;
        public int ValidCount;
    }

    public sealed class PhaseThinker
    {
        public PhaseSector Sector = null!;
        public int Base;
        public int Phase;
        public bool Spawned;
    }

    public sealed class PhaseWorld
    {
        public int ValidCount;
        public PhaseThinker Origin = null!;
        public List<PhaseThinker> Spawned { get; } = new();
    }

    public static int ClampLight(int level)
    {
        if (level < short.MinValue)
            return short.MinValue;
        if (level > short.MaxValue)
            return short.MaxValue;
        return level;
    }

    public static Flicker ConstructFlicker(int upper, int lower, int random)
    {
        int max = ClampLight(upper);
        return new Flicker
        {
            Max = max,
            Min = ClampLight(lower),
            Light = max,
            Count = (random & 64) + 1,
        };
    }

    public static LightFlash ConstructFlashFromSector(int sectorLight, int minSurrounding, int random)
    {
        return new LightFlash
        {
            Max = sectorLight,
            Min = minSurrounding,
            MaxTime = 64,
            MinTime = 7,
            Light = sectorLight,
            Count = (random & 64) + 1,
        };
    }

    public static LightFlash ConstructFlashFromArgs(int min, int max, int random)
    {
        int storedMax = ClampLight(max);
        return new LightFlash
        {
            Max = storedMax,
            Min = ClampLight(min),
            MaxTime = 64,
            MinTime = 7,
            Light = storedMax,
            Count = (random & 64) + 1,
        };
    }

    public static void TickFlash(LightFlash flash, int random)
    {
        flash.Count--;
        if (flash.Count != 0)
            return;

        if (flash.Light == flash.Max)
        {
            flash.Light = ClampLight(flash.Min);
            flash.Count = (random & flash.MinTime) + 1;
            return;
        }

        flash.Light = ClampLight(flash.Max);
        flash.Count = (random & flash.MaxTime) + 1;
    }

    public static Strobe ConstructStrobeHexen(int upper, int lower, int brightTics, int darkTics)
    {
        return new Strobe
        {
            Max = ClampLight(upper),
            Min = ClampLight(lower),
            BrightTime = brightTics,
            DarkTime = darkTics,
            Count = 1,
        };
    }

    public static Strobe ConstructStrobeDoom(int sectorLight, int minSurrounding, int brightTics, int darkTics, bool inSync, int random)
    {
        int min = minSurrounding;
        if (min == sectorLight)
            min = 0;

        int count = 1;
        if (!inSync)
            count = (random & 7) + 1;

        return new Strobe
        {
            Max = sectorLight,
            Min = min,
            BrightTime = brightTics,
            DarkTime = darkTics,
            Light = sectorLight,
            Count = count,
        };
    }

    public static void TickStrobe(Strobe strobe)
    {
        strobe.Count--;
        if (strobe.Count != 0)
            return;

        if (strobe.Light == strobe.Min)
        {
            strobe.Light = ClampLight(strobe.Max);
            strobe.Count = strobe.BrightTime;
            return;
        }

        strobe.Light = ClampLight(strobe.Min);
        strobe.Count = strobe.DarkTime;
    }

    public static Glow ConstructGlow(int sectorLight, int minSurrounding)
    {
        return new Glow
        {
            Min = minSurrounding,
            Max = sectorLight,
            Direction = -1,
            Light = sectorLight,
        };
    }

    public static void TickGlow(Glow glow)
    {
        int next = glow.Light;
        if (glow.Direction == -1)
        {
            next -= GlowSpeed;
            if (next <= glow.Min)
            {
                next += GlowSpeed;
                glow.Direction = 1;
            }
        }
        else if (glow.Direction == 1)
        {
            next += GlowSpeed;
            if (next >= glow.Max)
            {
                next -= GlowSpeed;
                glow.Direction = -1;
            }
        }

        glow.Light = ClampLight(next);
    }

    public static Glow2 ConstructGlow2(int start, int end, int tics, bool oneShot)
    {
        return new Glow2
        {
            Start = ClampLight(start),
            End = ClampLight(end),
            MaxTics = tics,
            Tics = -1,
            OneShot = oneShot,
        };
    }

    public static void TickGlow2(Glow2 glow)
    {
        if (glow.Tics >= glow.MaxTics)
        {
            glow.Tics++;
            if (glow.OneShot)
            {
                glow.Light = ClampLight(glow.End);
                glow.Destroyed = true;
                return;
            }

            int temp = glow.Start;
            glow.Start = glow.End;
            glow.End = temp;
            glow.Tics -= glow.MaxTics;
        }
        else
        {
            glow.Tics++;
        }

        int span = unchecked(glow.End - glow.Start);
        int light = unchecked(span * glow.Tics) / glow.MaxTics + glow.Start;
        glow.Light = ClampLight(light);
    }

    public static void ConstructPhase(PhaseThinker thinker, int baseLevel, int phase)
    {
        thinker.Base = LowByte(baseLevel);
        thinker.Phase = LowByte(phase);
    }

    public static void TickPhase(PhaseThinker thinker)
    {
        int level;
        if (thinker.Phase < PhaseSteps)
            level = ((255 - thinker.Base) * thinker.Phase) / PhaseSteps + thinker.Base;
        else if (thinker.Phase < 2 * PhaseSteps)
            level = ((255 - thinker.Base) * (2 * PhaseSteps - thinker.Phase - 1) / PhaseSteps) + thinker.Base;
        else
            level = thinker.Base;

        thinker.Sector.Light = ClampLight(level);
        if (thinker.Phase == 0)
            thinker.Phase = PhaseWrap;
        else
            thinker.Phase--;
    }

    public static int Propagate(PhaseWorld world, Func<PhaseSector, int, PhaseSector?> next)
    {
        world.ValidCount++;
        return PhaseHelper(world.Origin.Sector, 0, 0, world.ValidCount, next, world);
    }

    public static int PhaseHelper(
        PhaseSector? sector,
        int index,
        int light,
        int validCount,
        Func<PhaseSector, int, PhaseSector?> next,
        PhaseWorld world)
    {
        if (sector == null || sector.ValidCount == validCount)
            return index;

        int baseLevel = sector.Light != 0 ? sector.Light : light;
        int stored = LowByte(baseLevel);
        sector.ValidCount = validCount;

        PhaseThinker thinker;
        if (index == 0)
        {
            thinker = world.Origin;
            thinker.Base = stored;
        }
        else
        {
            thinker = new PhaseThinker
            {
                Sector = sector,
                Base = stored,
                Phase = 0,
                Spawned = true,
            };
            world.Spawned.Add(thinker);
        }

        int wanted = sector.Special == SequenceOne ? SequenceTwo : SequenceOne;
        PhaseSector? following = next(sector, wanted);
        int steps = PhaseHelper(following, index + 1, thinker.Base, validCount, next, world);
        thinker.Phase = LowByte(((steps - index - 1) * 64) / steps);
        sector.Special = 0;
        return steps;
    }

    static int LowByte(int value)
    {
        return unchecked((byte)value);
    }
}
