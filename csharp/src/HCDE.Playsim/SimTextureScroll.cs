using System.Buffers.Binary;

namespace HCDE.Playsim;

public sealed record SimTextureScroll(int Target, int Kind, double Dx, double Dy,
    int Control = -1, ScrollCarryAffect Affect = ScrollCarryAffect.All,
    double LastHeight = 0, double Vdx = 0, double Vdy = 0)
{
    internal const int Size = 24;
    internal const int CarrySize = 56;
    // 0/1: planes; 2..8: wall masks; 9/10: constant/accelerating carry;
    // 11/12: controlled carry; 13/14: controlled ceiling; 15/16: controlled floor.
    // 17/18: displacement/accelerating controlled wall (all parts).
    // 19..30: controlled wall masks 1..6, paired displacement/acceleration.
    internal bool IsValid => Target >= 0 && Kind is >= 0 and <= 30
        && (Kind < 11 || Control >= 0) && (Affect & ~ScrollCarryAffect.All) == 0
        && double.IsFinite(Dx) && double.IsFinite(Dy) && double.IsFinite(LastHeight)
        && double.IsFinite(Vdx) && double.IsFinite(Vdy);
    internal void Write(Span<byte> bytes, bool carry = false)
    {
        BinaryPrimitives.WriteInt32LittleEndian(bytes, Target);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], Kind);
        BinaryPrimitives.WriteInt64LittleEndian(bytes[8..], BitConverter.DoubleToInt64Bits(Dx));
        BinaryPrimitives.WriteInt64LittleEndian(bytes[16..], BitConverter.DoubleToInt64Bits(Dy));
        if (!carry) return;
        BinaryPrimitives.WriteInt32LittleEndian(bytes[24..], Control);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[28..], (int)Affect);
        BinaryPrimitives.WriteInt64LittleEndian(bytes[32..], BitConverter.DoubleToInt64Bits(LastHeight));
        BinaryPrimitives.WriteInt64LittleEndian(bytes[40..], BitConverter.DoubleToInt64Bits(Vdx));
        BinaryPrimitives.WriteInt64LittleEndian(bytes[48..], BitConverter.DoubleToInt64Bits(Vdy));
    }
    internal static SimTextureScroll Read(ReadOnlySpan<byte> bytes, bool carry = false) => new(
        BinaryPrimitives.ReadInt32LittleEndian(bytes), BinaryPrimitives.ReadInt32LittleEndian(bytes[4..]),
        BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[8..])),
        BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[16..])),
        carry ? BinaryPrimitives.ReadInt32LittleEndian(bytes[24..]) : -1,
        carry ? (ScrollCarryAffect)BinaryPrimitives.ReadInt32LittleEndian(bytes[28..]) : ScrollCarryAffect.All,
        carry ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[32..])) : 0,
        carry ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[40..])) : 0,
        carry ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[48..])) : 0);
}
