using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Column-major multiply, 3×3 and 4×4 determinants, and the 4×4 adjoint from
/// <c>matrix.cpp</c> (<b>516–723</b>). Chunk <b>1248</b>. <c>inverseMatrix</c> at
/// <b>724</b> stays on the earlier matrix chunk. Elements are <c>float</c>
/// (<c>USE_DOUBLE</c> is off). The 3×3 minor is row-major; the 4×4 matrix is column-major.
/// </summary>
public sealed class VmMatrix1248
{
    public readonly List<string> Log = new();

    public bool Multiply(float[]? left, float[]? right)
    {
        if (left == null || right == null)
        {
            Log.Add("mul null");
            return false;
        }
        if (left.Length < 16 || right.Length < 16)
        {
            Log.Add($"mul short {left.Length} {right.Length}");
            return false;
        }
        var res = new float[16];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                float acc = 0f;
                for (int k = 0; k < 4; k++)
                    acc += left[k * 4 + i] * right[j * 4 + k];
                res[j * 4 + i] = acc;
            }
        }
        Array.Copy(res, left, 16);
        Log.Add("mul " + Row(left));
        return true;
    }

    public double Mat3Det(ReadOnlySpan<float> m)
    {
        if (m.Length < 9)
        {
            Log.Add("det3 short");
            return 0;
        }
        float value = Mat3(m);
        Log.Add("det3 " + Fmt(value));
        return value;
    }

    public double Mat4Det(ReadOnlySpan<float> m)
    {
        if (m.Length < 16)
        {
            Log.Add("det4 short");
            return 0;
        }
        float a = MinorDet(m, 0, 0);
        float b = MinorDet(m, 1, 0);
        float c = MinorDet(m, 2, 0);
        float d = MinorDet(m, 3, 0);
        float value = m[0] * a;
        value -= m[1] * b;
        value += m[2] * c;
        value -= m[3] * d;
        Log.Add("det4 " + Fmt(value));
        return value;
    }

    public bool Adjoint(ReadOnlySpan<float> m, float[]? result)
    {
        if (result == null)
        {
            Log.Add("adj null");
            return false;
        }
        if (m.Length < 16 || result.Length < 16)
        {
            Log.Add($"adj short {m.Length} {result.Length}");
            return false;
        }
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                float sign = ((r + c) & 1) == 0 ? 1f : -1f;
                result[c * 4 + r] = sign * MinorDet(m, c, r);
            }
        }
        Log.Add("adj " + Row(result));
        return true;
    }

    static float MinorDet(ReadOnlySpan<float> m, int dropRow, int dropCol)
    {
        Span<float> block = stackalloc float[9];
        int n = 0;
        for (int row = 0; row < 4; row++)
        {
            if (row == dropRow)
                continue;
            for (int col = 0; col < 4; col++)
            {
                if (col == dropCol)
                    continue;
                block[n++] = m[col * 4 + row];
            }
        }
        return Mat3(block);
    }

    static float Mat3(ReadOnlySpan<float> m) =>
        m[0] * (m[4] * m[8] - m[5] * m[7])
        + m[1] * (m[5] * m[6] - m[8] * m[3])
        + m[2] * (m[3] * m[7] - m[4] * m[6]);

    static string Row(float[] m)
    {
        var text = new StringBuilder();
        for (int i = 0; i < 16; i++)
        {
            if (i > 0)
                text.Append(',');
            text.Append(Fmt(m[i]));
        }
        return text.ToString();
    }

    static string Fmt(float v)
    {
        if (v == 0f)
            v = 0f;
        return v.ToString("G9", CultureInfo.InvariantCulture);
    }
}
