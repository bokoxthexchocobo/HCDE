using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed record SimPlaneTransform(double FloorX, double FloorY, double CeilingX, double CeilingY,
    double FloorScaleX, double FloorScaleY, double CeilingScaleX, double CeilingScaleY,
    double FloorBaseY, double CeilingBaseY, uint FloorAngle, uint CeilingAngle, uint FloorBaseAngle, uint CeilingBaseAngle)
{
    internal const int Size = 96;
    private double[] Values => [FloorX, FloorY, CeilingX, CeilingY, FloorScaleX, FloorScaleY,
        CeilingScaleX, CeilingScaleY, FloorBaseY, CeilingBaseY];
    internal bool IsValid => Values.All(double.IsFinite);
    internal static SimPlaneTransform Capture(LevelSector s) => new(s.FloorTextureOffsetX, s.FloorTextureOffsetY,
        s.CeilingTextureOffsetX, s.CeilingTextureOffsetY, s.FloorTextureScaleX, s.FloorTextureScaleY,
        s.CeilingTextureScaleX, s.CeilingTextureScaleY, s.FloorTextureBaseOffsetY, s.CeilingTextureBaseOffsetY,
        s.FloorTextureAngle, s.CeilingTextureAngle, s.FloorTextureBaseAngle, s.CeilingTextureBaseAngle);
    internal void Apply(LevelSector s)
    {
        s.FloorTextureOffsetX = FloorX; s.FloorTextureOffsetY = FloorY;
        s.CeilingTextureOffsetX = CeilingX; s.CeilingTextureOffsetY = CeilingY;
        s.FloorTextureScaleX = FloorScaleX; s.FloorTextureScaleY = FloorScaleY;
        s.CeilingTextureScaleX = CeilingScaleX; s.CeilingTextureScaleY = CeilingScaleY;
        s.FloorTextureBaseOffsetY = FloorBaseY; s.CeilingTextureBaseOffsetY = CeilingBaseY;
        s.FloorTextureAngle = FloorAngle; s.CeilingTextureAngle = CeilingAngle;
        s.FloorTextureBaseAngle = FloorBaseAngle; s.CeilingTextureBaseAngle = CeilingBaseAngle;
    }
    internal void Write(Span<byte> bytes)
    {
        var values = Values;
        for (var i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteInt64LittleEndian(bytes[(i * 8)..], BitConverter.DoubleToInt64Bits(values[i]));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[80..], FloorAngle);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[84..], CeilingAngle);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[88..], FloorBaseAngle);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[92..], CeilingBaseAngle);
    }
    internal static SimPlaneTransform Read(ReadOnlySpan<byte> bytes)
    {
        var v = new double[10];
        for (var i = 0; i < v.Length; i++)
            v[i] = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes[(i * 8)..]));
        return new(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], v[9],
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[80..]), BinaryPrimitives.ReadUInt32LittleEndian(bytes[84..]),
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[88..]), BinaryPrimitives.ReadUInt32LittleEndian(bytes[92..]));
    }
}
