namespace HCDE.Net.Core.Tests;

public class UserCmdCodecTests
{
    [Fact]
    public void UserCmd_RoundTrip_Is16Bytes()
    {
        var command = new UserCmd(0x1234, 100, -200, 50, 10, -5, 1);
        Span<byte> buffer = stackalloc byte[LiveConstants.ExplicitUserCmdBytes];
        var cursor = 0;
        Assert.Equal(LiveConstants.ExplicitUserCmdBytes, UserCmdCodec.Write(buffer, ref cursor, command));
        cursor = 0;
        Assert.True(UserCmdCodec.TryRead(buffer, ref cursor, out var parsed));
        Assert.Equal(command.Buttons, parsed.Buttons);
        Assert.Equal(command.Pitch, parsed.Pitch);
        Assert.Equal(command.ForwardMove, parsed.ForwardMove);
        Assert.Equal(LiveConstants.ExplicitUserCmdBytes, cursor);
    }
}
