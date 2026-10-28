namespace HCDE.Playsim;

/// <summary>DECORATE cast placeholder actions from <c>p_actionfunctions.cpp</c> (494–518).</summary>
public static class VmActionDecorateInternalPlaysim
{
    public const string IntIntrinsic = "__decorate_internal_int__";
    public const string BoolIntrinsic = "__decorate_internal_bool__";
    public const string FloatIntrinsic = "__decorate_internal_float__";

    public static bool FloatIntrinsicWritesRet(bool numretPositive, double returnme, out double writtenValue)
    {
        writtenValue = returnme;
        return numretPositive;
    }

    public static int FloatIntrinsicReturnCount(bool numretPositive) =>
        numretPositive ? 1 : 0;
}
