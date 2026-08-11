namespace HCDE.Net.Core;

public readonly struct UserCmd
{
    public UserCmd(
        uint buttons,
        short pitch,
        short yaw,
        short roll,
        short forwardMove,
        short sideMove,
        short upMove)
    {
        Buttons = buttons;
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
        ForwardMove = forwardMove;
        SideMove = sideMove;
        UpMove = upMove;
    }

    public uint Buttons { get; }
    public short Pitch { get; }
    public short Yaw { get; }
    public short Roll { get; }
    public short ForwardMove { get; }
    public short SideMove { get; }
    public short UpMove { get; }

    public static UserCmd Zero => default;
}
