namespace HCDE.Playsim.Tests;

public class VmPMobjObjectToWorldMatrixTests
{
    [Fact]
    public void ObjectToWorldMatrix_GateRequiresBoneFlag() =>
        Assert.True(VmPMobjObjectToWorldMatrix.GetObjectToWorldMatrixGate(true, 1, false));

    [Fact]
    public void GetObjectToWorldMatrix_FillsMatrix()
    {
        var outMat = new double[16];
        VmPMobjObjectToWorldMatrix.FillObjectToWorldMatrix(outMat, new double[] { 1, 2, 3 });
        Assert.Equal(1, outMat[0]);
        Assert.Equal(3, outMat[2]);
    }
}
