namespace HCDE.Playsim.Tests;

public class VmMatrix1248SliceTests
{
    const string HeadGolden =
"""
mul 2,0,0,0,0,1,0,0,0,0,1,0,4,3,4,1
mul 1,0,0,0,0,1,0,0,0,0,1,0,4,6,8,1

""";

    const string TailGolden =
"""
---- edge
det4 22.5
adj 1,-0.5,1,3,43,1,-2,-6,-21,10.5,1.5,4.5,-7,3.5,-7,1.5
det4 1
adj 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1
det4 120
adj 60,0,0,0,0,40,0,0,0,0,30,0,0,0,0,24
mul null
mul null
det4 short
adj short 16 4
det3 short
det3 60
mul short 8 16

""";

    [Fact]
    public void Multiply_AppendsTheRightMatrixInColumnMajorOrder()
    {
        string dump = Build(out float[] product, out double det, out float[] adj);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("mul 2,0,0,0,0,1,0,0,0,0,1,0,4,3,4,1\n", head, StringComparison.Ordinal);
        Assert.Contains("mul 1,0,0,0,0,1,0,0,0,0,1,0,4,6,8,1\n", head, StringComparison.Ordinal);
        Assert.Equal(new float[] { 2, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 4, 3, 4, 1 }, product);
        Assert.Equal(22.5, det);
        Assert.Equal(new float[] { 1, -0.5f, 1, 3, 43, 1, -2, -6, -21, 10.5f, 1.5f, 4.5f, -7, 3.5f, -7, 1.5f }, adj);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Determinant_ExpandsMinorsAndRejectsAShortMatrix()
    {
        string dump = Build(out _, out _, out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("det4 1\n", tail, StringComparison.Ordinal);
        Assert.Contains("det4 120\n", tail, StringComparison.Ordinal);
        Assert.Contains("det4 22.5\n", tail, StringComparison.Ordinal);
        Assert.Contains("adj 60,0,0,0,0,40,0,0,0,0,30,0,0,0,0,24\n", tail, StringComparison.Ordinal);
        Assert.Contains("mul null\n", tail, StringComparison.Ordinal);
        Assert.Contains("det4 short\n", tail, StringComparison.Ordinal);
        Assert.Contains("adj short 16 4\n", tail, StringComparison.Ordinal);
        Assert.Contains("det3 60\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out float[] product, out double det, out float[] adj)
    {
        var vm = new VmMatrix1248();
        float[] id = Identity();
        float[] scale = Identity();
        scale[0] = 2;
        float[] shift = Identity();
        shift[12] = 2;
        shift[13] = 3;
        shift[14] = 4;
        Assert.True(vm.Multiply(scale, shift));
        product = (float[])scale.Clone();

        float[] twice = Identity();
        twice[12] = 2;
        twice[13] = 3;
        twice[14] = 4;
        Assert.True(vm.Multiply(twice, twice));

        vm.Log.Add("---- edge");
        float[] n = Identity();
        n[1] = 0.5f;
        n[4] = -1;
        n[6] = 2;
        n[11] = -3;
        n[12] = 7;
        det = vm.Mat4Det(n);
        adj = new float[16];
        Assert.True(vm.Adjoint(n, adj));
        Assert.Equal(1, vm.Mat4Det(id));
        var idAdj = new float[16];
        Assert.True(vm.Adjoint(id, idAdj));
        float[] diag = new float[16];
        diag[0] = 2;
        diag[5] = 3;
        diag[10] = 4;
        diag[15] = 5;
        Assert.Equal(120, vm.Mat4Det(diag));
        var diagAdj = new float[16];
        Assert.True(vm.Adjoint(diag, diagAdj));
        Assert.False(vm.Multiply(null, id));
        Assert.False(vm.Multiply(id, null));
        Assert.Equal(0, vm.Mat4Det(new float[4]));
        Assert.False(vm.Adjoint(diag, new float[4]));
        Assert.Equal(0, vm.Mat3Det(new float[3]));
        float[] block =
        {
            3, 0, 0,
            0, 4, 0,
            0, 0, 5,
        };
        Assert.Equal(60, vm.Mat3Det(block));
        Assert.False(vm.Multiply(new float[8], id));

        string dump = string.Join('\n', vm.Log) + "\n";
        return dump;
    }

    static float[] Identity()
    {
        var m = new float[16];
        for (int i = 0; i < 4; i++)
            m[i * 4 + i] = 1;
        return m;
    }
}
