using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed record SimWallTransform(double TopX, double TopY, double MidX, double MidY,
    double BottomX, double BottomY, double TopScaleX, double TopScaleY,
    double MidScaleX, double MidScaleY, double BottomScaleX, double BottomScaleY)
{
    internal const int Size = 96;
    private double[] Values => [TopX, TopY, MidX, MidY, BottomX, BottomY,
        TopScaleX, TopScaleY, MidScaleX, MidScaleY, BottomScaleX, BottomScaleY];
    internal bool IsValid => Values.All(double.IsFinite);

    internal static SimWallTransform Capture(LevelSide side) => new(side.TopTextureOffsetX, side.TopTextureOffsetY,
        side.MidTextureOffsetX, side.MidTextureOffsetY, side.BottomTextureOffsetX, side.BottomTextureOffsetY,
        side.TopTextureScaleX, side.TopTextureScaleY, side.MidTextureScaleX, side.MidTextureScaleY,
        side.BottomTextureScaleX, side.BottomTextureScaleY);

    internal void Apply(LevelSide side)
    {
        side.TopTextureOffsetX = TopX; side.TopTextureOffsetY = TopY;
        side.MidTextureOffsetX = MidX; side.MidTextureOffsetY = MidY;
        side.BottomTextureOffsetX = BottomX; side.BottomTextureOffsetY = BottomY;
        side.TopTextureScaleX = TopScaleX; side.TopTextureScaleY = TopScaleY;
        side.MidTextureScaleX = MidScaleX; side.MidTextureScaleY = MidScaleY;
        side.BottomTextureScaleX = BottomScaleX; side.BottomTextureScaleY = BottomScaleY;
    }

    internal void Write(Span<byte> bytes)
    {
        var values = Values;
        for (var i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteInt64LittleEndian(bytes[(i * 8)..], BitConverter.DoubleToInt64Bits(values[i]));
    }

    internal static SimWallTransform Read(ReadOnlySpan<byte> bytes)
    {
        var values = new double[12];
        for (var i = 0; i < values.Length; i++)
            values[i] = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[(i * 8)..]));
        return new(values[0], values[1], values[2], values[3], values[4], values[5],
            values[6], values[7], values[8], values[9], values[10], values[11]);
    }
}
