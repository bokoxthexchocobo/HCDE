namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchSoundTests
{
    [Fact]
    public void ConsumePatchLines_StopsOnNonOne()
    {
        var calls = 0;
        int Reader()
        {
            calls++;
            return calls < 3 ? 1 : 0;
        }

        Assert.Equal(0, VmDehackedPatchSound.ConsumePatchLines(Reader));
        Assert.Equal(3, calls);
    }
}
