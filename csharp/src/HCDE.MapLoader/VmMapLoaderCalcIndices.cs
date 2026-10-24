namespace HCDE.MapLoader;

/// <summary><c>MapLoader::CalcIndices</c> from <c>maploader.cpp</c> (2909–2936).</summary>
public static class VmMapLoaderCalcIndices
{
    public static int[] AssignVertexIndices(int count)
    {
        var indices = new int[count];
        for (var i = 0; i < count; i++)
            indices[i] = i;
        return indices;
    }

    public static int[] AssignLineIndices(int count) => AssignVertexIndices(count);

    public static int[] AssignSideIndices(int count) => AssignVertexIndices(count);

    public static int[] AssignSegIndices(int count) => AssignVertexIndices(count);

    public static int[] AssignSubsectorIndices(int count) => AssignVertexIndices(count);

    public static int[] AssignNodeIndices(int count) => AssignVertexIndices(count);

    public static void ApplyCalcIndices(LevelIndexArrays arrays)
    {
        arrays.VertexNums = AssignVertexIndices(arrays.VertexNums.Length);
        arrays.LineNums = AssignLineIndices(arrays.LineNums.Length);
        arrays.SideNums = AssignSideIndices(arrays.SideNums.Length);
        arrays.SegNums = AssignSegIndices(arrays.SegNums.Length);
        arrays.SubsectorNums = AssignSubsectorIndices(arrays.SubsectorNums.Length);
        arrays.NodeNums = AssignNodeIndices(arrays.NodeNums.Length);
    }

    public sealed class LevelIndexArrays
    {
        public int[] VertexNums { get; set; } = Array.Empty<int>();
        public int[] LineNums { get; set; } = Array.Empty<int>();
        public int[] SideNums { get; set; } = Array.Empty<int>();
        public int[] SegNums { get; set; } = Array.Empty<int>();
        public int[] SubsectorNums { get; set; } = Array.Empty<int>();
        public int[] NodeNums { get; set; } = Array.Empty<int>();
    }
}
