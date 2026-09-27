namespace HCDE.Playsim;

internal enum LightEffectKind { Fade, Glow, Strobe, Flicker, SectorGlow, LightFlash, FireFlicker, Phased }

internal sealed class LightEffect
{
    public int Sector { get; init; }
    public LightEffectKind Kind { get; init; }
    public int Start { get; set; }
    public int End { get; set; }
    public int Duration { get; init; }
    public int DarkTime { get; init; }
    public long Tics { get; set; } = -1;

    public bool Tick(AuthoritySimulation sim)
    {
        if (Kind == LightEffectKind.Phased)
        {
            var step = Tics < 12 ? Tics : Tics < 24 ? 23 - Tics : 0;
            sim.Lights[Sector] = (short)Math.Clamp(((255 - Start) * step) / 12 + Start, short.MinValue, short.MaxValue);
            Tics = Tics == 0 ? 63 : Tics - 1;
            return false;
        }
        if (Kind == LightEffectKind.LightFlash)
        {
            if (--Tics == 0)
            {
                var darken = sim.LightOf(Sector) == Start;
                sim.Lights[Sector] = (short)(darken ? End : Start);
                Tics = (sim.NextLightFlashRandom() & (darken ? 7 : 64)) + 1;
            }
            return false;
        }
        if (Kind == LightEffectKind.FireFlicker)
        {
            if (--Tics == 0)
            {
                var amount = (sim.NextFireFlickerRandom() & 3) << 4;
                // Native compares the current light, not the captured maximum.
                sim.Lights[Sector] = (short)Math.Clamp(sim.LightOf(Sector) - amount < End ? End : Start - amount,
                    short.MinValue, short.MaxValue);
                Tics = 4;
            }
            return false;
        }
        if (Kind == LightEffectKind.SectorGlow)
        {
            var light = sim.LightOf(Sector) + (int)Tics * 8;
            if (Tics < 0 && light <= End || Tics > 0 && light >= Start)
            {
                light -= (int)Tics * 8;
                Tics = -Tics;
            }
            sim.Lights[Sector] = (short)Math.Clamp(light, short.MinValue, short.MaxValue);
            return false;
        }
        if (Kind == LightEffectKind.Flicker)
        {
            if (Tics != 0) Tics--;
            else
            {
                var darken = sim.LightOf(Sector) == Start;
                sim.Lights[Sector] = (short)(darken ? End : Start);
                Tics = (sim.NextFlickerRandom() & (darken ? 7 : 31)) + 1;
            }
            return false;
        }
        if (Kind == LightEffectKind.Strobe)
        {
            if (--Tics == 0)
            {
                var brighten = sim.LightOf(Sector) == End;
                sim.Lights[Sector] = (short)(brighten ? Start : End);
                Tics = brighten ? Duration : DarkTime;
            }
            return false;
        }
        // DGlow2 increments after its duration comparison; its first tick writes Start.
        if (Tics++ >= Duration)
        {
            if (Kind == LightEffectKind.Fade)
            {
                sim.Lights[Sector] = (short)End;
                return true;
            }
            (Start, End) = (End, Start);
            Tics -= Duration;
        }
        sim.Lights[Sector] = (short)(((long)(End - Start) * Tics) / Duration + Start);
        return false;
    }

    public uint Checksum
    {
        get
        {
            var hash = 2166136261u;
            foreach (var value in new long[] { Sector, (int)Kind, Start, End, Duration, DarkTime, Tics })
            {
                hash = unchecked((hash ^ (uint)value) * 16777619u);
                hash = unchecked((hash ^ (uint)(value >> 32)) * 16777619u);
            }
            return hash;
        }
    }
}
