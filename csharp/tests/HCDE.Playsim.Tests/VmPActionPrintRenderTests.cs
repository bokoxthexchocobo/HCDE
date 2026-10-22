namespace HCDE.Playsim.Tests;

public class VmPActionPrintRenderTests
{
    [Fact]
    public void Print_SkipsWhenNotLocal()
    {
        var printed = false;
        VmPActionPrintRender.Print(new()
        {
            MidPrint = (_, _) => printed = true,
        });
        Assert.False(printed);
    }

    [Fact]
    public void SetTranslucent_ClampsAlpha()
    {
        var a = new VmPActionPrintRender.RenderActor();
        VmPActionPrintRender.SetTranslucent(a, 2, 0);
        Assert.Equal(1, a.Alpha);
    }

    [Fact]
    public void FadeOut_RemoveAtZero()
    {
        var removed = false;
        var a = new VmPActionPrintRender.RenderActor { Alpha = 0.05 };
        VmPActionPrintRender.FadeOut(a, 0.1, VmPActionPrintRender.FtfRemove, () => removed = true);
        Assert.True(removed);
    }
}
