namespace HCDE.Playsim;

/// <summary>Hexen lightning from <c>mapthinkers/a_lightning.cpp</c> (46–316).</summary>
public static class VmALightning
{
    public const int Ticrate = 35;
    public const int ShrtMax = short.MaxValue;
    public const int LevelSwapSkies = 1 << 1;
    public const int LevelStartlightning = 1 << 2;
    public const int LightOutdoorLightning = 39;
    public const int LightIndoorLightning1 = 40;
    public const int LightIndoorLightning2 = 41;
    public const int GameHexen = 2;
    public const int Level2Hexenhack = 1 << 0;
    public const int NoSound = 0;

    public class LightningSector
    {
        public int Special { get; set; }
        public int LightLevel { get; set; }
        public Action<int> SetLightLevel { get; init; } = _ => { };
        public Action<int> ChangeLightLevel { get; init; } = _ => { };
        public Func<int> GetCeilingTexture { get; init; } = () => 0;
        public int SkyFlatNum { get; set; }
    }

    public sealed class LightningThinker
    {
        public bool Stopped { get; set; }
        public int NextLightningFlash { get; set; }
        public int LightningFlashCount { get; set; }
        public int TempLightningSound { get; set; }
        public List<int> LightningLightLevels { get; } = new();
        public Action DestroySelf { get; init; } = () => { };
        public Func<int> RandomLightning { get; init; } = () => 0;
        public IReadOnlyList<LightningSector> Sectors { get; init; } = Array.Empty<LightningSector>();
        public int LevelFlags { get; set; }
        public Action<int> SetLevelFlags { get; init; } = _ => { };
        public int LevelTime { get; set; }
        public int LightningSound { get; set; }
        public Action<int, int, int, double, int> PlaySound { get; init; } = (_, _, _, _, _) => { };
        public Action WorldLightning { get; init; } = () => { };
        public Action StartTypedScripts { get; init; } = () => { };
    }

    public static void Construct(LightningThinker thinker, int tempSound, int sectorCount)
    {
        thinker.Stopped = false;
        thinker.LightningFlashCount = 0;
        thinker.NextLightningFlash = ((thinker.RandomLightning() & 15) + 5) * Ticrate;
        thinker.TempLightningSound = tempSound;
        thinker.LightningLightLevels.Clear();
        for (var i = 0; i < sectorCount; i++)
            thinker.LightningLightLevels.Add(ShrtMax);
    }

    public static void Tick(LightningThinker thinker)
    {
        if (thinker.NextLightningFlash == 0 || thinker.LightningFlashCount != 0)
            LightningFlash(thinker);
        else
        {
            thinker.NextLightningFlash--;
            if (thinker.Stopped)
                thinker.DestroySelf();
        }
    }

    public static void LightningFlash(LightningThinker thinker)
    {
        if (thinker.LightningFlashCount > 0)
        {
            thinker.LightningFlashCount--;
            if (thinker.LightningFlashCount > 0)
            {
                for (var j = 0; j < thinker.Sectors.Count; j++)
                {
                    var sec = thinker.Sectors[j];
                    if (thinker.LightningLightLevels[j] < sec.LightLevel - 4)
                        sec.ChangeLightLevel(-4);
                }
            }
            else
            {
                for (var j = 0; j < thinker.Sectors.Count; j++)
                {
                    if (thinker.LightningLightLevels[j] != ShrtMax)
                        thinker.Sectors[j].SetLightLevel(thinker.LightningLightLevels[j]);
                }

                for (var j = 0; j < thinker.LightningLightLevels.Count; j++)
                    thinker.LightningLightLevels[j] = ShrtMax;
                thinker.SetLevelFlags(thinker.LevelFlags & ~LevelSwapSkies);
            }

            return;
        }

        thinker.LightningFlashCount = (thinker.RandomLightning() & 7) + 8;
        var flashLight = 200 + (thinker.RandomLightning() & 31);
        for (var j = 0; j < thinker.Sectors.Count; j++)
        {
            var sec = thinker.Sectors[j];
            var special = sec.Special;
            if (sec.GetCeilingTexture() == sec.SkyFlatNum
                || special == LightIndoorLightning1
                || special == LightIndoorLightning2
                || special == LightOutdoorLightning)
            {
                thinker.LightningLightLevels[j] = sec.LightLevel;
                if (special == LightIndoorLightning1)
                    sec.SetLightLevel(Math.Min(sec.LightLevel + 64, flashLight));
                else if (special == LightIndoorLightning2)
                    sec.SetLightLevel(Math.Min(sec.LightLevel + 32, flashLight));
                else
                    sec.SetLightLevel(flashLight);
                if (sec.LightLevel < thinker.LightningLightLevels[j])
                {
                    sec.SetLightLevel(thinker.LightningLightLevels[j]);
                    thinker.LightningLightLevels[j] = ShrtMax;
                }
            }
        }

        thinker.SetLevelFlags(thinker.LevelFlags | LevelSwapSkies);
        if (thinker.TempLightningSound == NoSound)
            thinker.PlaySound(0, 0, thinker.LightningSound, 1.0, 0);
        else
        {
            thinker.PlaySound(0, 0, thinker.TempLightningSound, 1.0, 0);
            thinker.TempLightningSound = NoSound;
        }

        thinker.WorldLightning();
        thinker.StartTypedScripts();
        if (thinker.NextLightningFlash == 0)
        {
            var r = thinker.RandomLightning();
            if (r < 50)
                thinker.NextLightningFlash = (thinker.RandomLightning() & 15) + 16;
            else if (r < 128 && (thinker.LevelTime & 32) == 0)
                thinker.NextLightningFlash = ((thinker.RandomLightning() & 7) + 2) * Ticrate;
            else
                thinker.NextLightningFlash = ((thinker.RandomLightning() & 15) + 5) * Ticrate;
        }
    }

    public static void ForceLightningThinker(LightningThinker thinker, int mode, int tempSound)
    {
        switch (mode)
        {
            default:
                thinker.NextLightningFlash = 0;
                thinker.TempLightningSound = tempSound;
                break;
            case 1:
                thinker.NextLightningFlash = 0;
                thinker.TempLightningSound = tempSound;
                thinker.Stopped = true;
                break;
            case 2:
                thinker.Stopped = true;
                break;
        }
    }

    public static void StartLightning(
        int gameType,
        int levelFlags2,
        IReadOnlyList<LightningSector> sectors,
        int skyFlatNum,
        ref int levelFlags,
        Func<LightningThinker> createThinker,
        LightningThinker? existing)
    {
        var isOriginalHexen = gameType == GameHexen && (levelFlags2 & Level2Hexenhack) != 0;
        if (isOriginalHexen)
        {
            var hasLightning = false;
            foreach (var sector in sectors)
            {
                hasLightning = sector.GetCeilingTexture() == skyFlatNum
                    || sector.Special == LightIndoorLightning1
                    || sector.Special == LightIndoorLightning2;
                if (hasLightning)
                    break;
            }

            if (!hasLightning)
            {
                levelFlags &= ~LevelStartlightning;
                return;
            }
        }

        if (existing == null)
            createThinker();
    }

    public static void ForceLightningLevel(
        LightningThinker? existing,
        Func<int, LightningThinker> createThinker,
        int mode,
        int tempSound)
    {
        var lightning = existing ?? createThinker(tempSound);
        ForceLightningThinker(lightning, mode, tempSound);
    }

}
