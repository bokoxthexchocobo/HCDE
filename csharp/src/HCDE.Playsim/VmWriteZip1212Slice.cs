namespace HCDE.Playsim;

/// <summary>
/// DOS time and zip local, central, and end headers from <c>writezip.cpp</c> (<b>1–230</b>).
/// Chunk <b>1212</b>. This host is little endian, so <c>LittleShort</c> and <c>LittleLong</c> are identities.
/// The local header stores the DOS time in <c>ModDate</c> and the DOS date in <c>ModTime</c>.
/// </summary>
public sealed class VmZipTm1212
{
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;
    public int Second;
}

public sealed class VmZipEntry1212
{
    public int Method;
    public uint Crc;
    public ulong Size;
    public ulong CompressedSize;
    public byte[]? Data;
    public string? Name;
}

public sealed class VmZipBuffer1212
{
    readonly List<byte> bytes = new();

    public int FailOnWrite { get; set; } = -1;
    public int Writes { get; private set; }
    public bool Closed { get; private set; }
    public byte[] Bytes => bytes.ToArray();

    public long Tell()
    {
        return bytes.Count;
    }

    public int Write(ReadOnlySpan<byte> data)
    {
        int n = Writes++;
        if (FailOnWrite >= 0 && n == FailOnWrite)
            return data.Length == 0 ? 0 : data.Length - 1;
        for (int i = 0; i < data.Length; i++)
            bytes.Add(data[i]);
        return data.Length;
    }

    public void Close()
    {
        Closed = true;
    }
}

public static class VmWriteZip1212Slice
{
    public const int MethodStored = 0;
    public const int MethodImplode = 6;
    public const int MethodDeflate = 8;
    public const int MethodImplodeMin = 1000;
    public const int MethodImplodeMax = 1006;
    public const int MethodRejected = 1337;
    const uint LocalMagic = 0x04034b50;
    const uint CentralMagic = 0x02014b50;
    const uint EndMagic = 0x06054b50;

    public static (ushort Time, ushort Date) TimeToDos(VmZipTm1212? time)
    {
        if (time == null || time.Year < 80)
            return (0, 0);
        int clock = time.Hour * 2048 + time.Minute * 32 + time.Second / 2;
        int day = (time.Year - 80) * 512 + (time.Month + 1) * 32 + time.Day;
        return ((ushort)clock, (ushort)day);
    }

    public static bool Write(
        string? filename,
        VmZipEntry1212[]? entries,
        Func<string?, VmZipBuffer1212?> open,
        Func<VmZipTm1212?>? clock,
        Action<string?> remove)
    {
        var stamp = TimeToDos(clock == null ? null : clock());
        if (open == null)
            throw new NullReferenceException();
        var file = open(filename);
        if (file == null)
            return false;
        int count = entries?.Length ?? 0;
        var positions = new int[count];
        for (int i = 0; i < count; i++)
        {
            int pos = AppendLocal(file, entries![i], stamp.Time, stamp.Date);
            if (pos == -1)
            {
                file.Close();
                remove(filename);
                return false;
            }

            positions[i] = pos;
        }

        int directory = (int)file.Tell();
        for (int i = 0; i < count; i++)
        {
            if (AppendCentral(file, entries![i], stamp.Time, stamp.Date, positions[i]) < 0)
            {
                file.Close();
                remove(filename);
                return false;
            }
        }

        var end = new byte[22];
        U32(end, 0, EndMagic);
        U16(end, 8, (ushort)count);
        U16(end, 10, (ushort)count);
        U32(end, 12, (uint)((int)file.Tell() - directory));
        U32(end, 16, (uint)directory);
        if (file.Write(end) != end.Length)
        {
            file.Close();
            remove(filename);
            return false;
        }

        file.Close();
        return true;
    }

    public static int AppendLocal(VmZipBuffer1212 file, VmZipEntry1212 entry, ushort time, ushort date)
    {
        if (!Encode(entry.Method, out int flags, out int method))
            return -1;
        int position = (int)file.Tell();
        byte[] name = NameBytes(entry.Name);
        var header = new byte[30];
        U32(header, 0, LocalMagic);
        header[4] = 20;
        U16(header, 6, (ushort)flags);
        U16(header, 8, (ushort)method);
        U16(header, 10, date);
        U16(header, 12, time);
        U32(header, 14, entry.Crc);
        U32(header, 18, (uint)entry.CompressedSize);
        U32(header, 22, (uint)entry.Size);
        U16(header, 26, (ushort)name.Length);
        if (file.Write(header) != header.Length || file.Write(name) != name.Length || WritePayload(file, entry) < 0)
            return -1;
        return position;
    }

    public static int AppendCentral(VmZipBuffer1212 file, VmZipEntry1212 entry, ushort time, ushort date, int position)
    {
        if (!Encode(entry.Method, out int flags, out int method))
            return -1;
        byte[] name = NameBytes(entry.Name);
        var header = new byte[46];
        U32(header, 0, CentralMagic);
        header[4] = 20;
        header[6] = 20;
        U16(header, 8, (ushort)flags);
        U16(header, 10, (ushort)method);
        U16(header, 12, time);
        U16(header, 14, date);
        U32(header, 16, entry.Crc);
        U32(header, 20, (uint)entry.CompressedSize);
        U32(header, 24, (uint)entry.Size);
        U16(header, 28, (ushort)name.Length);
        U32(header, 42, (uint)position);
        if (file.Write(header) != header.Length || file.Write(name) != name.Length)
            return -1;
        return 0;
    }

    static int WritePayload(VmZipBuffer1212 file, VmZipEntry1212 entry)
    {
        int n = (int)entry.CompressedSize;
        if (n < 0)
            n = 0;
        if (n == 0)
            return file.Write(ReadOnlySpan<byte>.Empty) == 0 ? 0 : -1;
        if (entry.Data == null)
            throw new NullReferenceException();
        if (file.Write(entry.Data.AsSpan(0, n)) != n)
            return -1;
        return 0;
    }

    static bool Encode(int method, out int flags, out int stored)
    {
        flags = 0;
        stored = method;
        if (method >= MethodImplodeMin && method <= MethodImplodeMax)
        {
            flags = method - MethodImplodeMin;
            stored = MethodImplode;
            return true;
        }

        if (method == MethodDeflate)
        {
            flags = 2;
            return true;
        }

        if (method >= MethodRejected)
            return false;
        return true;
    }

    static byte[] NameBytes(string? name)
    {
        if (name == null)
            throw new NullReferenceException();
        var bytes = new byte[name.Length];
        for (int i = 0; i < name.Length; i++)
            bytes[i] = (byte)name[i];
        return bytes;
    }

    static void U16(byte[] data, int at, ushort value)
    {
        data[at] = (byte)value;
        data[at + 1] = (byte)(value >> 8);
    }

    static void U32(byte[] data, int at, uint value)
    {
        data[at] = (byte)value;
        data[at + 1] = (byte)(value >> 8);
        data[at + 2] = (byte)(value >> 16);
        data[at + 3] = (byte)(value >> 24);
    }
}
