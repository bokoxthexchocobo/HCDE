namespace HCDE.Net.Core.Tests;

public class VmINetJoinGameTests
{
    [Fact]
    public void JoinGameNative_FailsWithoutHostArg()
    {
        var fatal = "";
        VmINetJoinGame.JoinGameNative(new JoinGameContext
        {
            Arg = 1,
            NumArgs = () => 2,
            GetArg = i => i == 1 ? "-host" : null,
            FatalError = s => fatal = s,
        });
        Assert.Contains("host machine", fatal);
    }
}
