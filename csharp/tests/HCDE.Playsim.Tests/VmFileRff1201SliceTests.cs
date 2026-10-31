namespace HCDE.Playsim.Tests;

public class VmFileRff1201SliceTests
{
    [Fact]
    public void BloodCrypt_XorsWithLowKeyAndHalfIndex()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        VmFileRff1201Slice.BloodCrypt(data, 0x1234, 8);
        Assert.Equal(new byte[] { 0x35, 0x36, 0x36, 0x31, 0x33, 0x30, 0x30, 0x3F }, data);
        VmFileRff1201Slice.BloodCrypt(data, 0x1234, 8);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, data);
    }

    [Fact]
    public void Check_DecryptsDirectoryAndBuildsNames()
    {
        Assert.Null(VmFileRff1201Slice.Check("short.rff", new byte[15]));
        var wrong = new byte[16];
        wrong[0] = (byte)'R';
        wrong[1] = (byte)'F';
        wrong[2] = (byte)'F';
        wrong[3] = 0x18;
        Assert.Null(VmFileRff1201Slice.Check("old.rff", wrong));

        var plain = new byte[VmFileRff1201Slice.LumpSize * 2];
        WriteU32(plain, 16, 100);
        WriteU32(plain, 20, 40);
        plain[32] = 0x10;
        plain[33] = (byte)'P';
        plain[34] = (byte)'N';
        plain[35] = (byte)'G';
        CopyName(plain, 36, "SPRITE");
        WriteU32(plain, 44, 7);
        WriteU32(plain, 48 + 16, 200);
        WriteU32(plain, 48 + 20, 3);
        plain[48 + 32] = 0x01;
        plain[48 + 33] = (byte)'W';
        plain[48 + 34] = (byte)'A';
        plain[48 + 35] = (byte)'V';
        CopyName(plain, 48 + 36, "AB\0CDEFG");
        WriteU32(plain, 48 + 44, 9);
        VmFileRff1201Slice.BloodCrypt(plain, 16, plain.Length);

        var file = new byte[16 + plain.Length];
        file[0] = (byte)'R';
        file[1] = (byte)'F';
        file[2] = (byte)'F';
        file[3] = 0x1A;
        WriteU32(file, 8, 16);
        WriteU32(file, 12, 2);
        Buffer.BlockCopy(plain, 0, file, 16, plain.Length);

        var opened = VmFileRff1201Slice.Check("BLOOD.RFF", file, static name => name.ToLowerInvariant());
        Assert.NotNull(opened);
        Assert.Equal("BLOOD.RFF", opened.SourceName);
        Assert.Equal(1, opened.HashBuilds);
        Assert.Equal(2, opened.Entries.Count);
        Assert.Equal(100u, opened.Entries[0].Position);
        Assert.Equal(40u, opened.Entries[0].Length);
        Assert.Equal(40u, opened.Entries[0].CompressedSize);
        Assert.Equal(VmFileRff1201Slice.ResCompressed, opened.Entries[0].Flags);
        Assert.Equal(VmFileRff1201Slice.MethodRffCrypt, opened.Entries[0].Method);
        Assert.Equal(VmFileRff1201Slice.NamespaceGlobal, opened.Entries[0].Namespace);
        Assert.Equal(7u, opened.Entries[0].ResourceId);
        Assert.Equal("sprite.png", opened.Entries[0].FileName);
        Assert.Equal(200u, opened.Entries[1].Position);
        Assert.Equal(3u, opened.Entries[1].Length);
        Assert.Equal(0, opened.Entries[1].Flags);
        Assert.Equal(VmFileRff1201Slice.MethodStored, opened.Entries[1].Method);
        Assert.Equal(9u, opened.Entries[1].ResourceId);
        Assert.Equal("ab.wav", opened.Entries[1].FileName);

        var full = new byte[VmFileRff1201Slice.LumpSize];
        CopyName(full, 36, "ABCDEFGH");
        full[33] = (byte)'X';
        full[34] = (byte)'Y';
        full[35] = (byte)'Z';
        VmFileRff1201Slice.BloodCrypt(full, 16, full.Length);
        var wrapped = new byte[16 + full.Length];
        wrapped[0] = (byte)'R';
        wrapped[1] = (byte)'F';
        wrapped[2] = (byte)'F';
        wrapped[3] = 0x1A;
        WriteU32(wrapped, 8, 16);
        WriteU32(wrapped, 12, 1);
        Buffer.BlockCopy(full, 0, wrapped, 16, full.Length);
        var eight = VmFileRff1201Slice.Check("eight.rff", wrapped);
        Assert.Equal("ABCDEFGH.XYZ", eight!.Entries[0].FileName);
    }

    static void WriteU32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    static void CopyName(byte[] data, int offset, string name)
    {
        for (int i = 0; i < name.Length && i < 8; i++)
            data[offset + i] = (byte)name[i];
    }
}
