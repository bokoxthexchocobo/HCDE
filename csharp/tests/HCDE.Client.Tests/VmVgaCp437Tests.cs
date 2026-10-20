namespace HCDE.Client.Tests;

public class VmVgaCp437Tests
{
    [Fact]
    public void Cp437ToAscii_TranslatesControlGlyphs()
    {
        Assert.Equal('^', VmVgaCp437.Cp437ToAscii(24));
        Assert.Equal('A', VmVgaCp437.Cp437ToAscii(65));
    }

    [Fact]
    public void ProbeTerminal_ClassifiesEnv()
    {
        Assert.Equal(VmVgaCp437.TerminalColorSupport.Dumb, VmVgaCp437.ProbeTerminal("dumb", "truecolor"));
        Assert.Equal(VmVgaCp437.TerminalColorSupport.Full, VmVgaCp437.ProbeTerminal("xterm", "truecolor"));
    }
}
