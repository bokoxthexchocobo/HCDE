namespace HCDE.Playsim;

/// <summary>`AActor::GetObjectToWorldMatrix` from <c>p_mobj.cpp</c> (4501–4518).</summary>
public static class VmPMobjObjectToWorldMatrix
{
    public const int ModeldataGetBoneInfo = 1;

    public static bool GetObjectToWorldMatrixGate(bool hasModelData, int flags, bool picnumValid) =>
        hasModelData && (flags & ModeldataGetBoneInfo) != 0 && !picnumValid;

    public static void FillObjectToWorldMatrix(double[] outMat, IReadOnlyList<double> sourceMatrix)
    {
        for (var i = 0; i < 16 && i < sourceMatrix.Count; i++)
            outMat[i] = sourceMatrix[i];
    }
}
