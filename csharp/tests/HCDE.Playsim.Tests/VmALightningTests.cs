using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmALightningTests
{
    [Fact]
    public void Construct_SetsNextFlashFromRandom()
    {
        var thinker = new VmALightning.LightningThinker { RandomLightning = () => 0 };
        VmALightning.Construct(thinker, 0, 2);
        Assert.Equal(5 * VmALightning.Ticrate, thinker.NextLightningFlash);
        Assert.Equal(2, thinker.LightningLightLevels.Count);
    }

    [Fact]
    public void Tick_DecrementsCountdownWhenIdle()
    {
        var thinker = new VmALightning.LightningThinker { NextLightningFlash = 3, LightningFlashCount = 0 };
        VmALightning.Tick(thinker);
        Assert.Equal(2, thinker.NextLightningFlash);
    }

    [Fact]
    public void LightningFlash_RestoreClearsSwapSkies()
    {
        var flags = VmALightning.LevelSwapSkies;
        var restored = -1;
        var sec = new VmALightning.LightningSector { SetLightLevel = v => restored = v };
        var thinker = new VmALightning.LightningThinker
        {
            LightningFlashCount = 1,
            Sectors = new[] { sec },
            LevelFlags = flags,
            SetLevelFlags = f => flags = f,
        };
        thinker.LightningLightLevels.Add(80);
        VmALightning.LightningFlash(thinker);
        Assert.Equal(80, restored);
        Assert.Equal(0, flags & VmALightning.LevelSwapSkies);
    }

    [Fact]
    public void StartLightning_HexenNoSky_ClearsStartFlag()
    {
        var flags = VmALightning.LevelStartlightning;
        VmALightning.StartLightning(
            VmALightning.GameHexen,
            VmALightning.Level2Hexenhack,
            new[] { new VmALightning.LightningSector { GetCeilingTexture = () => 2, SkyFlatNum = 1 } },
            1,
            ref flags,
            () => new VmALightning.LightningThinker(),
            null);
        Assert.Equal(0, flags & VmALightning.LevelStartlightning);
    }

    [Fact]
    public void LightningFlash_StartPhase_SetsFlashCountAndSwapSkies()
    {
        var flags = 0;
        var level = 100;
        var sec = new VmALightning.LightningSector
        {
            SkyFlatNum = 1,
            GetCeilingTexture = () => 1,
            LightLevel = 100,
            SetLightLevel = v => level = v,
        };
        var thinker = new VmALightning.LightningThinker
        {
            RandomLightning = () => 0,
            Sectors = new[] { sec },
            SetLevelFlags = f => flags = f,
            LightningSound = 42,
        };
        thinker.LightningLightLevels.Add(VmALightning.ShrtMax);
        VmALightning.LightningFlash(thinker);
        Assert.True(thinker.LightningFlashCount > 0);
        Assert.NotEqual(0, flags & VmALightning.LevelSwapSkies);
    }

    [Fact]
    public void ForceLightningLevel_CreatesWhenMissing()
    {
        var created = false;
        VmALightning.ForceLightningLevel(null, _ => { created = true; return new VmALightning.LightningThinker(); }, 0, 1);
        Assert.True(created);
    }
}
