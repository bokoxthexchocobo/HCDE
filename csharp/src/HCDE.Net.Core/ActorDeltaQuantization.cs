using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class ActorDeltaQuantization
{
    public static int QuantizePos(double value) =>
        (int)Math.Round(value * LiveConstants.ActorDeltaPosScale, MidpointRounding.AwayFromZero);

    public static double DequantizePos(int value) => value / LiveConstants.ActorDeltaPosScale;

    public static short QuantizeVel(double value) =>
        (short)Math.Round(value * LiveConstants.ActorDeltaVelScale, MidpointRounding.AwayFromZero);

    public static double DequantizeVel(short value) => value / LiveConstants.ActorDeltaVelScale;

    public static ushort CompactAngle(uint bam) => (ushort)(bam >> 16);

    public static uint ExpandCompactAngle(ushort compact) => (uint)compact << 16;

    public static int WriteQuantizedPos(Span<byte> buffer, ref int cursor, double value)
    {
        if (buffer.Length - cursor < 4)
            return 0;

        BinaryPrimitives.WriteInt32BigEndian(buffer[cursor..], QuantizePos(value));
        cursor += 4;
        return 4;
    }

    public static bool TryReadQuantizedPos(ReadOnlySpan<byte> buffer, ref int cursor, out double value)
    {
        value = 0;
        if (buffer.Length - cursor < 4)
            return false;

        value = DequantizePos(BinaryPrimitives.ReadInt32BigEndian(buffer[cursor..]));
        cursor += 4;
        return true;
    }

    public static int WriteQuantizedVel(Span<byte> buffer, ref int cursor, double value)
    {
        if (buffer.Length - cursor < 2)
            return 0;

        BinaryPrimitives.WriteInt16BigEndian(buffer[cursor..], QuantizeVel(value));
        cursor += 2;
        return 2;
    }

    public static bool TryReadQuantizedVel(ReadOnlySpan<byte> buffer, ref int cursor, out double value)
    {
        value = 0;
        if (buffer.Length - cursor < 2)
            return false;

        value = DequantizeVel(BinaryPrimitives.ReadInt16BigEndian(buffer[cursor..]));
        cursor += 2;
        return true;
    }
}
