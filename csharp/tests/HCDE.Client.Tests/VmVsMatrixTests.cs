namespace HCDE.Client.Tests;

public class VmVsMatrixTests
{
    [Fact]
    public void Perspective_MatchesKnownFrustumCoefficients()
    {
        var matrix = new VmVsMatrix();
        matrix.Perspective(90f, 1f, 1f, 100f);
        Assert.Equal(1f, matrix.Matrix[0], precision: 3);
        Assert.Equal(-1f, matrix.Matrix[11]);
        Assert.True(matrix.Matrix[14] < 0);
    }

    [Fact]
    public void Translate_AppendsToColumnMajorTranslationRow()
    {
        var matrix = new VmVsMatrix();
        matrix.Translate(1, 2, 3);
        Assert.Equal(1f, matrix.Matrix[12]);
        Assert.Equal(2f, matrix.Matrix[13]);
        Assert.Equal(3f, matrix.Matrix[14]);
    }

    [Fact]
    public void Frustum_MultipliesOntoCurrentMatrix()
    {
        var matrix = new VmVsMatrix();
        matrix.Frustum(-1, 1, -1, 1, 1, 100);
        Assert.Equal(-1f, matrix.Matrix[11]);
    }
}
