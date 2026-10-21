namespace HCDE.Playsim.Tests;

public class VmSoundEngineLoadSoundTests
{
    [Fact]
    public void LoadSound_DecodesVocHeader()
    {
        var data = "Creative Voice File\x0"u8.ToArray();
        var ctx = new VmSoundEngineLoadSound.LoadSoundContext
        {
            Lumpnum = 1,
            SfxEmpty = 0,
            LumpBytes = data,
        };
        ctx.LoadVoc = b =>
        {
            ctx.DataValid = b.Length > 19;
            return ctx.DataValid;
        };
        Assert.Equal(1, VmSoundEngineLoadSound.LoadSound(ctx));
        Assert.True(ctx.DataValid);
    }

    [Fact]
    public void LoadSound_SharesCatalogBuffer()
    {
        var ctx = new VmSoundEngineLoadSound.LoadSoundContext
        {
            Lumpnum = 42,
            Catalog = new[] { (true, -1, 42, 11025) },
            DataValid = false,
        };
        Assert.Equal(42, VmSoundEngineLoadSound.LoadSound(ctx));
        Assert.Equal(0, ctx.LinkIndex);
    }
}
