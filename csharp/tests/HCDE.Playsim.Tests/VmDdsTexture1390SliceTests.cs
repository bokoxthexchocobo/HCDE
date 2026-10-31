using S = HCDE.Playsim.VmDdsTexture1390Slice;

namespace HCDE.Playsim.Tests;

public class VmDdsTexture1390SliceTests
{
    static uint Le(char a, char b, char c, char d) =>
        (uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24);

    static uint At(byte[] data, int offset) =>
        data[offset]
        | ((uint)data[offset + 1] << 8)
        | ((uint)data[offset + 2] << 16)
        | ((uint)data[offset + 3] << 24);

    [Fact]
    public void FourCcFlagsAndHeaderLayout()
    {
        Assert.Equal(Le('D', 'D', 'S', ' '), S.IdDds);
        Assert.Equal(0x20534444u, S.IdDds);
        Assert.Equal(Le('D', 'X', 'T', '1'), S.IdDxt1);
        Assert.Equal(Le('D', 'X', 'T', '2'), S.IdDxt2);
        Assert.Equal(Le('D', 'X', 'T', '3'), S.IdDxt3);
        Assert.Equal(Le('D', 'X', 'T', '4'), S.IdDxt4);
        Assert.Equal(Le('D', 'X', 'T', '5'), S.IdDxt5);
        Assert.Equal(S.MakeId('D', 'X', 'T', '5'), S.IdDxt5);

        Assert.Equal(0x00000001u, S.Flag("DDSD_CAPS"));
        Assert.Equal(0x00000002u, S.Flag("DDSD_HEIGHT"));
        Assert.Equal(0x00000004u, S.Flag("DDSD_WIDTH"));
        Assert.Equal(0x00000008u, S.Flag("DDSD_PITCH"));
        Assert.Equal(0x00001000u, S.Flag("DDSD_PIXELFORMAT"));
        Assert.Equal(0x00020000u, S.Flag("DDSD_MIPMAPCOUNT"));
        Assert.Equal(0x00080000u, S.Flag("DDSD_LINEARSIZE"));
        Assert.Equal(0x00800000u, S.Flag("DDSD_DEPTH"));
        Assert.Equal(0x00000001u, S.Flag("DDPF_ALPHAPIXELS"));
        Assert.Equal(0x00000004u, S.Flag("DDPF_FOURCC"));
        Assert.Equal(0x00000040u, S.Flag("DDPF_RGB"));
        Assert.Equal(0x00000008u, S.Flag("DDSCAPS_COMPLEX"));
        Assert.Equal(0x00001000u, S.Flag("DDSCAPS_TEXTURE"));
        Assert.Equal(0x00400000u, S.Flag("DDSCAPS_MIPMAP"));
        Assert.Equal(0x00000200u, S.Flag("DDSCAPS2_CUBEMAP"));
        Assert.Equal(0x00000400u, S.Flag("DDSCAPS2_CUBEMAP_POSITIVEX"));
        Assert.Equal(0x00000800u, S.Flag("DDSCAPS2_CUBEMAP_NEGATIVEX"));
        Assert.Equal(0x00001000u, S.Flag("DDSCAPS2_CUBEMAP_POSITIVEY"));
        Assert.Equal(0x00002000u, S.Flag("DDSCAPS2_CUBEMAP_NEGATIVEY"));
        Assert.Equal(0x00004000u, S.Flag("DDSCAPS2_CUBEMAP_POSITIVEZ"));
        Assert.Equal(0x00008000u, S.Flag("DDSCAPS2_CUBEMAP_NEGATIZEZ"));
        Assert.Equal(0x00200000u, S.Flag("DDSCAPS2_VOLUME"));
        Assert.Equal(S.Ddscaps2CubemapNegatizeZ, S.Flag("DDSCAPS2_CUBEMAP_NEGATIZEZ"));
        Assert.Throws<KeyNotFoundException>(() => S.Flag("DDSCAPS2_CUBEMAP_NEGATIVEZ"));
        Assert.Equal(S.DdsdPixelFormat, S.DdscapsTexture);
        Assert.Equal(S.DdsdPixelFormat, S.Ddscaps2CubemapPositiveY);

        var header = new S.Header { Magic = S.IdDds };
        header.Desc.Size = S.IdDds;
        header.Desc.Flags = S.DdsdCaps | S.DdsdPixelFormat | S.DdsdWidth | S.DdsdHeight;
        header.Desc.Height = 8;
        header.Desc.Width = 16;
        header.Desc.Pitch = -2;
        header.Desc.Depth = 1;
        header.Desc.MipMapCount = 3;
        header.Desc.Reserved1[0] = 0x11;
        header.Desc.Reserved1[10] = 0xA1B2C3D4;
        header.Desc.Format.Size = 32;
        header.Desc.Format.Flags = S.DdpfFourCc;
        header.Desc.Format.FourCc = S.IdDxt5;
        header.Desc.Format.RgbBitCount = 32;
        header.Desc.Format.R = 0x00FF0000;
        header.Desc.Format.G = 0x0000FF00;
        header.Desc.Format.B = 0x000000FF;
        header.Desc.Format.A = 0xFF000000;
        header.Desc.Caps.Caps1 = S.DdscapsTexture | S.DdscapsComplex;
        header.Desc.Caps.Caps2 = S.Ddscaps2Cubemap | S.Ddscaps2CubemapNegatizeZ;
        header.Desc.Caps.Reserved[0] = 4;
        header.Desc.Caps.Reserved[1] = 9;
        header.Desc.Reserved2 = 7;

        Assert.Equal(0xFFFFFFFEu, header.Desc.LinearSize);
        header.Desc.LinearSize = 0x11223344;
        Assert.Equal(0x11223344, header.Desc.Pitch);
        header.Desc.Pitch = -2;

        var bytes = S.WriteHeader(header);
        Assert.Equal(128, bytes.Length);
        Assert.Equal(32, S.PixelFormatBytes);
        Assert.Equal(16, S.CapsBytes);
        Assert.Equal(124, S.SurfaceBytes);
        Assert.Equal(128, S.HeaderBytes);
        Assert.Equal((byte)'D', bytes[0]);
        Assert.Equal((byte)'D', bytes[1]);
        Assert.Equal((byte)'S', bytes[2]);
        Assert.Equal((byte)' ', bytes[3]);
        Assert.Equal(S.IdDds, At(bytes, 4));
        Assert.Equal(0xFFFFFFFEu, At(bytes, 4 + 16));
        Assert.Equal(0xA1B2C3D4u, At(bytes, 4 + 28 + 40));
        Assert.Equal(32u, At(bytes, 4 + 72));
        Assert.Equal(S.IdDxt5, At(bytes, 4 + 72 + 8));
        Assert.Equal(0x00FF0000u, At(bytes, 4 + 72 + 16));
        Assert.Equal(0x0000FF00u, At(bytes, 4 + 72 + 20));
        Assert.Equal(0x000000FFu, At(bytes, 4 + 72 + 24));
        Assert.Equal(0xFF000000u, At(bytes, 4 + 72 + 28));
        Assert.Equal(S.Ddscaps2Cubemap | S.Ddscaps2CubemapNegatizeZ, At(bytes, 4 + 104 + 4));
        Assert.Equal(9u, At(bytes, 4 + 104 + 12));
        Assert.Equal(7u, At(bytes, 124));
        Assert.Equal(16, S.DescOffset("Pitch"));
        Assert.Equal(S.DescOffset("Pitch"), S.DescOffset("LinearSize"));
        Assert.Equal(72, S.DescOffset("PixelFormat"));
        Assert.Equal(104, S.DescOffset("Caps"));

        var read = S.ReadHeader(bytes);
        Assert.Equal(S.IdDds, read.Magic);
        Assert.Equal(S.IdDds, read.Desc.Size);
        Assert.Equal(8u, read.Desc.Height);
        Assert.Equal(16u, read.Desc.Width);
        Assert.Equal(-2, read.Desc.Pitch);
        Assert.Equal(0x11u, read.Desc.Reserved1[0]);
        Assert.Equal(S.IdDxt5, read.Desc.Format.FourCc);
        Assert.Equal(0xFF000000u, read.Desc.Format.A);
        Assert.Equal(4u, read.Desc.Caps.Reserved[0]);
        Assert.Equal(7u, read.Desc.Reserved2);

        header.Desc.Reserved1 = new uint[10];
        Assert.Throws<ArgumentException>(() => S.WriteHeader(header));
        Assert.Throws<ArgumentException>(() => S.ReadHeader(new byte[127]));
        Assert.Throws<ArgumentNullException>(() => S.ReadHeader(null!));
    }

    [Fact]
    public void ClassDeclaresModesFieldsAndMethods()
    {
        Assert.Equal(0, S.PixelMode("PIX_Palette"));
        Assert.Equal(1, S.PixelMode("PIX_Alphatex"));
        Assert.Equal(2, S.PixelMode("PIX_ARGB"));
        Assert.Throws<InvalidOperationException>(() => S.PixelMode("PIX_Other"));

        Assert.Equal("FImageSource", S.BaseClass);
        Assert.Equal("FTexture", S.FriendClass);
        Assert.Equal(
            new[]
            {
                "Format", "RMask", "GMask", "BMask", "AMask",
                "RShiftL", "GShiftL", "BShiftL", "AShiftL",
                "RShiftR", "GShiftR", "BShiftR", "AShiftR",
                "Pitch", "LinearSize",
            },
            S.FieldNames);

        Assert.Equal(8, S.Methods.Length);
        var ctor = S.Method("FDDSTexture");
        Assert.Contains("void *surfdesc", ctor.Signature, StringComparison.Ordinal);
        Assert.StartsWith("public FDDSTexture(", ctor.Signature, StringComparison.Ordinal);
        Assert.False(ctor.DefaultFrame);
        Assert.Throws<InvalidOperationException>(() => S.DefaultFrame("FDDSTexture"));

        var pixels = S.Method("CreatePalettedPixels");
        Assert.Contains("frame = 0", pixels.Signature, StringComparison.Ordinal);
        Assert.StartsWith("public override ", pixels.Signature, StringComparison.Ordinal);
        Assert.Equal(0, S.DefaultFrame("CreatePalettedPixels"));

        var shift = S.Method("CalcBitShift");
        Assert.Contains("protected static void", shift.Signature, StringComparison.Ordinal);
        Assert.Contains("uint8_t *lshift", shift.Signature, StringComparison.Ordinal);

        var rgb = S.Method("ReadRGB");
        Assert.DoesNotContain("premultiplied", rgb.Signature, StringComparison.Ordinal);
        var dxt1 = S.Method("DecompressDXT1");
        Assert.DoesNotContain("premultiplied", dxt1.Signature, StringComparison.Ordinal);
        Assert.Contains("bool premultiplied", S.Method("DecompressDXT3").Signature, StringComparison.Ordinal);
        Assert.Contains("bool premultiplied", S.Method("DecompressDXT5").Signature, StringComparison.Ordinal);

        var copy = S.Method("CopyPixels");
        Assert.StartsWith("protected override int CopyPixels(", copy.Signature, StringComparison.Ordinal);
        Assert.Contains("frame = 0", copy.Signature, StringComparison.Ordinal);
        Assert.Equal(0, S.DefaultFrame("CopyPixels"));
        Assert.Throws<InvalidOperationException>(() => S.Method("CheckDDS"));
        Assert.Throws<InvalidOperationException>(() => S.DefaultFrame("ReadRGB"));

        var members = new S.Members
        {
            Format = S.IdDxt1,
            RMask = 0x00FF0000,
            GMask = 0x0000FF00,
            BMask = 0x000000FF,
            AMask = 0xFF000000,
            RShiftL = 8,
            GShiftL = 16,
            BShiftL = 24,
            AShiftL = 0,
            RShiftR = 16,
            Pitch = -5,
            LinearSize = 64,
        };
        Assert.Equal(S.IdDxt1, members.Format);
        Assert.Equal(0x0000FF00u, members.GMask);
        Assert.Equal((byte)24, members.BShiftL);
        Assert.Equal((byte)16, members.RShiftR);
        Assert.Equal(0, members.GShiftR);
        Assert.Equal(-5, members.Pitch);
        Assert.Equal(64u, members.LinearSize);
    }
}
