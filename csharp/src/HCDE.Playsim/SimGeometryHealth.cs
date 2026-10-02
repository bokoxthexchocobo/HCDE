using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimSectorHealth(int Floor, int Ceiling, int ThreeD);

public sealed class SimGeometryHealth
{
    public List<int> Lines { get; } = [];
    public List<SimSectorHealth> Sectors { get; } = [];
    public SortedDictionary<int, int> Groups { get; } = [];

    internal byte[] Write()
    {
        var bytes = new byte[checked(16 + Lines.Count * 4 + Sectors.Count * 12 + Groups.Count * 8)];
        var offset = 0;
        void Put(int value) { BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), value); offset += 4; }
        Put(Lines.Count);
        foreach (var health in Lines) Put(health);
        Put(Sectors.Count);
        foreach (var health in Sectors) { Put(health.Floor); Put(health.Ceiling); Put(health.ThreeD); }
        Put(Groups.Count);
        foreach (var group in Groups) { Put(group.Key); Put(group.Value); }
        Put(bytes.Length);
        return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimGeometryHealth health)
    {
        health = new();
        var offset = 0;
        if (!Count(bytes, ref offset, 4, out var lines)) return false;
        for (var i = 0; i < lines; i++, offset += 4)
            health.Lines.Add(BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]));
        if (!Count(bytes, ref offset, 12, out var sectors)) return false;
        for (var i = 0; i < sectors; i++, offset += 12)
            health.Sectors.Add(new(BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[(offset + 4)..]), BinaryPrimitives.ReadInt32LittleEndian(bytes[(offset + 8)..])));
        if (!Count(bytes, ref offset, 8, out var groups)) return false;
        for (var i = 0; i < groups; i++, offset += 8)
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
            if (id <= 0 || !health.Groups.TryAdd(id, BinaryPrimitives.ReadInt32LittleEndian(bytes[(offset + 4)..]))) return false;
        }
        return bytes.Length - offset == 4 && BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]) == bytes.Length;
    }

    private static bool Count(ReadOnlySpan<byte> bytes, ref int offset, int size, out int count)
    {
        count = 0;
        if (bytes.Length - offset < 4) return false;
        count = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
        offset += 4;
        return count >= 0 && count <= (bytes.Length - offset) / size;
    }
}
