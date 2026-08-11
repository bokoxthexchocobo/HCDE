using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class UserCmdCodec
{
    public static bool TryRead(ReadOnlySpan<byte> data, ref int cursor, out UserCmd command)
    {
        command = default;
        if (cursor < 0 || data.Length - cursor < LiveConstants.ExplicitUserCmdBytes)
            return false;

        var buttons = BinaryPrimitives.ReadUInt32BigEndian(data[cursor..]);
        cursor += 4;
        var pitch = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;
        var yaw = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;
        var roll = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;
        var forward = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;
        var side = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;
        var up = (short)BinaryPrimitives.ReadUInt16BigEndian(data[cursor..]);
        cursor += 2;

        command = new UserCmd(buttons, pitch, yaw, roll, forward, side, up);
        return true;
    }

    public static int Write(Span<byte> data, ref int cursor, UserCmd command)
    {
        if (cursor < 0 || data.Length - cursor < LiveConstants.ExplicitUserCmdBytes)
            return 0;

        BinaryPrimitives.WriteUInt32BigEndian(data[cursor..], command.Buttons);
        cursor += 4;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.Pitch);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.Yaw);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.Roll);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.ForwardMove);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.SideMove);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(data[cursor..], (ushort)command.UpMove);
        cursor += 2;
        return LiveConstants.ExplicitUserCmdBytes;
    }
}
