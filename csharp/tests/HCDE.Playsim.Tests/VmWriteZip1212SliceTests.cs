namespace HCDE.Playsim.Tests;

public class VmWriteZip1212SliceTests
{
    static VmZipTm1212 Clock() => new()
    {
        Year = 126,
        Month = 0,
        Day = 15,
        Hour = 13,
        Minute = 5,
        Second = 9,
    };

    static VmZipEntry1212 Entry(int method, uint crc, string name, string data) => new()
    {
        Method = method,
        Crc = crc,
        Size = (ulong)data.Length,
        CompressedSize = (ulong)data.Length,
        Name = name,
        Data = System.Text.Encoding.ASCII.GetBytes(data),
    };

    static string Zip(VmZipEntry1212[] entries, VmZipTm1212? clock)
    {
        var buf = new VmZipBuffer1212();
        Assert.True(VmWriteZip1212Slice.Write("out.zip", entries, _ => buf, () => clock, _ => { }));
        Assert.True(buf.Closed);
        return Convert.ToHexString(buf.Bytes);
    }

    [Fact]
    public void Write_StoresLocalCentralAndEndHeaders()
    {
        var stamp = VmWriteZip1212Slice.TimeToDos(Clock());
        Assert.Equal((ushort)0x68A4, stamp.Time);
        Assert.Equal((ushort)0x5C2F, stamp.Date);
        var blank = VmWriteZip1212Slice.TimeToDos(null);
        Assert.Equal((ushort)0, blank.Time);
        Assert.Equal((ushort)0, blank.Date);
        var early = VmWriteZip1212Slice.TimeToDos(new VmZipTm1212 { Year = 79, Second = 9 });
        Assert.Equal((ushort)0, early.Time);
        Assert.Equal((ushort)0, early.Date);

        const string stored =
            "504B03041400000000002F5CA46844332211040000000400000005000000612E7478745758595A" +
            "504B01021400140000000000A4682F5C443322110400000004000000050000000000000000000000000000000000612E747874" +
            "504B0506000000000100010033000000270000000000";
        Assert.Equal(stored, Zip(new[] { Entry(0, 0x11223344, "a.txt", "WXYZ") }, Clock()));

        const string deflate =
            "504B03041400020008002F5CA46844332211040000000400000005000000612E7478745758595A" +
            "504B01021400140002000800A4682F5C443322110400000004000000050000000000000000000000000000000000612E747874" +
            "504B0506000000000100010033000000270000000000";
        Assert.Equal(deflate, Zip(new[] { Entry(8, 0x11223344, "a.txt", "WXYZ") }, Clock()));

        const string implode =
            "504B03041400040006002F5CA46844332211040000000400000005000000612E7478745758595A" +
            "504B01021400140004000600A4682F5C443322110400000004000000050000000000000000000000000000000000612E747874" +
            "504B0506000000000100010033000000270000000000";
        Assert.Equal(implode, Zip(new[] { Entry(1004, 0x11223344, "a.txt", "WXYZ") }, Clock()));

        Assert.Equal(
            "504B0506000000000000000000000000000000000000",
            Zip(Array.Empty<VmZipEntry1212>(), null));

        var two = Zip(
            new[] { Entry(0, 1, "A", "Q"), Entry(8, 2, "B", "R") },
            new VmZipTm1212 { Year = 126, Month = 9, Day = 29, Hour = 1, Minute = 2, Second = 3 });
        Assert.StartsWith("504B03041400000000005D5D4108", two);
        Assert.Contains("504B03041400020008005D5D4108", two);
        Assert.EndsWith("504B050600000000020002005E000000400000000000", two);
    }

    [Fact]
    public void Write_RemovesTheFileWhenAHeaderIsRejected()
    {
        string? removed = null;
        var opened = new VmZipBuffer1212();
        Assert.False(VmWriteZip1212Slice.Write(
            "bad.zip",
            new[] { Entry(1337, 1, "A", "Q") },
            name =>
            {
                Assert.Equal("bad.zip", name);
                return opened;
            },
            Clock,
            name => removed = name));
        Assert.Equal("bad.zip", removed);
        Assert.True(opened.Closed);
        Assert.Empty(opened.Bytes);

        removed = null;
        Assert.False(VmWriteZip1212Slice.Write("missing.zip", new[] { Entry(0, 1, "A", "Q") }, _ => null, Clock, name => removed = name));
        Assert.Null(removed);

        removed = null;
        var partial = new VmZipBuffer1212 { FailOnWrite = 3 };
        Assert.False(VmWriteZip1212Slice.Write("part.zip", new[] { Entry(0, 1, "A", "Q") }, _ => partial, Clock, name => removed = name));
        Assert.Equal("part.zip", removed);
        Assert.True(partial.Closed);
        Assert.NotEmpty(partial.Bytes);

        var untimed = new VmZipBuffer1212();
        Assert.True(VmWriteZip1212Slice.Write("x.zip", new[] { Entry(0, 1, "A", "Q") }, _ => untimed, null, _ => { }));
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, untimed.Bytes.AsSpan(10, 4).ToArray());
        Assert.Throws<NullReferenceException>(() => VmWriteZip1212Slice.Write("x.zip", new[] { Entry(0, 1, "A", "Q") }, null!, Clock, _ => { }));
        var nameless = Entry(0, 1, "A", "Q");
        nameless.Name = null;
        Assert.Throws<NullReferenceException>(() => VmWriteZip1212Slice.AppendLocal(new VmZipBuffer1212(), nameless, 1, 2));
    }
}
