namespace HCDE.Playsim;

/// <summary>16.16 fixed point from <c>m_fixed.h</c>. <c>FRACBITS</c> is 16.</summary>
public readonly record struct Fixed
{
    public const int FracBits = 16;
    public const int Unit = 1 << FracBits;

    public int Raw { get; }

    public Fixed(int raw) => Raw = raw;

    public static Fixed FromInt(int value) => new(value << FracBits);

    public static Fixed FromDouble(double value) => new(RoundHalfEven(value * Unit));

    public double ToDouble() => Raw * (1.0 / Unit);

    public int ToInt() => (Raw + (1 << (FracBits - 1))) >> FracBits;

    public static Fixed Mul(Fixed left, Fixed right) =>
        new((int)(((long)left.Raw * right.Raw) >> FracBits));

    public static Fixed Div(Fixed left, Fixed right)
    {
        if (right.Raw == 0)
            throw new DivideByZeroException("Fixed division by zero.");
        return new((int)(((long)left.Raw << FracBits) / right.Raw));
    }

    public static int RoundHalfEven(double value) =>
        (int)Math.Round(value, MidpointRounding.ToEven);
}

/// <summary>Binary angle from <c>basics.h</c>. A full circle is 2^32.</summary>
public readonly record struct BamAngle
{
    public const uint Angle90 = 0x40000000;
    public const uint Angle180 = 0x80000000;

    public uint Raw { get; }

    public BamAngle(uint raw) => Raw = raw;

    public static BamAngle FromDegrees(double degrees) =>
        new(unchecked((uint)(long)Math.Round((degrees % 360) * (4294967296.0 / 360), MidpointRounding.ToEven)));

    public double ToDegrees() => Raw * (90.0 / Angle90);
}

/// <summary>Doom tic clock. <c>TICRATE</c> is 35.</summary>
public sealed class GameTicClock
{
    public const int TicRate = 35;

    public int Tic { get; private set; }

    public void Advance() => Tic++;

    public void Restore(int tic) => Tic = tic < 0 ? 0 : tic;

    public double Seconds => Tic / (double)TicRate;
}
