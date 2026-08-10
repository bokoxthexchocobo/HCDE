namespace HCDE.Net.Transport;

public static class HcdeConnectInfo
{
    public const int EncodedSize = 6;

    public static bool TryRead(ReadOnlySpan<byte> data, out byte version, out HcdeConnectFlags flags)
    {
        version = 0;
        flags = HcdeConnectFlags.None;
        if (data.Length < EncodedSize)
            return false;

        if (!data[..4].SequenceEqual(PregameConstants.ConnectMagic))
            return false;

        version = data[4];
        flags = (HcdeConnectFlags)data[5];
        return true;
    }

    public static int Write(Span<byte> buffer, byte version, HcdeConnectFlags flags)
    {
        if (buffer.Length < EncodedSize)
            return 0;

        PregameConstants.ConnectMagic.CopyTo(buffer);
        buffer[4] = version;
        buffer[5] = (byte)flags;
        return EncodedSize;
    }
}
