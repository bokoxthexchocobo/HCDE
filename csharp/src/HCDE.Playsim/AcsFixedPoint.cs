namespace HCDE.Playsim;

/// <summary>Native DoubleToACS / FloatToFixed conversion for ACS fixed-point results.</summary>
internal static class AcsFixedPoint
{
    internal static int FromDouble(double value) => (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
}
