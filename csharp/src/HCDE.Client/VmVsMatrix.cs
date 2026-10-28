namespace HCDE.Client;

/// <summary>Native <c>matrix.cpp</c> / <c>VSMatrix</c> (column-major 4×4).</summary>
public sealed class VmVsMatrix
{
    private readonly float[] _matrix = new float[16];

    public VmVsMatrix() => LoadIdentity();

    public ReadOnlySpan<float> Matrix => _matrix;

    public void LoadIdentity()
    {
        Array.Clear(_matrix);
        for (var index = 0; index < 4; index++)
            _matrix[index + index * 4] = 1f;
    }

    public void LoadMatrix(ReadOnlySpan<float> matrix)
    {
        if (matrix.Length < 16)
            throw new ArgumentException("Matrix must contain 16 values.");
        matrix[..16].CopyTo(_matrix);
    }

    public void MultMatrix(ReadOnlySpan<float> matrix)
    {
        Span<float> result = stackalloc float[16];
        MultiplyInto(result, _matrix, matrix);
        result.CopyTo(_matrix);
    }

    public void MultQuaternion(VmQuaternion quaternion) => MultQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

    public void MultQuaternion(float x, float y, float z, float w)
    {
        Span<float> rotation = stackalloc float[16];
        rotation[0] = 1f - 2f * y * y - 2f * z * z;
        rotation[4] = 2f * x * y - 2f * w * z;
        rotation[8] = 2f * x * z + 2f * w * y;
        rotation[1] = 2f * x * y + 2f * w * z;
        rotation[5] = 1f - 2f * x * x - 2f * z * z;
        rotation[9] = 2f * y * z - 2f * w * x;
        rotation[2] = 2f * x * z - 2f * w * y;
        rotation[6] = 2f * y * z + 2f * w * x;
        rotation[10] = 1f - 2f * x * x - 2f * y * y;
        rotation[15] = 1f;
        MultMatrix(rotation);
    }

    public void Translate(float x, float y, float z)
    {
        _matrix[12] = _matrix[0] * x + _matrix[4] * y + _matrix[8] * z + _matrix[12];
        _matrix[13] = _matrix[1] * x + _matrix[5] * y + _matrix[9] * z + _matrix[13];
        _matrix[14] = _matrix[2] * x + _matrix[6] * y + _matrix[10] * z + _matrix[14];
    }

    public void Scale(float x, float y, float z)
    {
        for (var row = 0; row < 4; row++)
        {
            _matrix[row] *= x;
            _matrix[4 + row] *= y;
            _matrix[8 + row] *= z;
        }
    }

    public void Rotate(float angleDegrees, float x, float y, float z)
    {
        Normalize(ref x, ref y, ref z);
        var radians = angleDegrees * (MathF.PI / 180f);
        var co = MathF.Cos(radians);
        var si = MathF.Sin(radians);
        var x2 = x * x;
        var y2 = y * y;
        var z2 = z * z;
        Span<float> rotation = stackalloc float[16];
        rotation[0] = co + x2 * (1 - co);
        rotation[4] = x * y * (1 - co) - z * si;
        rotation[8] = x * z * (1 - co) + y * si;
        rotation[1] = x * y * (1 - co) + z * si;
        rotation[5] = co + y2 * (1 - co);
        rotation[9] = y * z * (1 - co) - x * si;
        rotation[2] = x * z * (1 - co) - y * si;
        rotation[6] = y * z * (1 - co) + x * si;
        rotation[10] = co + z2 * (1 - co);
        rotation[15] = 1f;
        MultMatrix(rotation);
    }

    public void LookAt(
        float xPos, float yPos, float zPos,
        float xLook, float yLook, float zLook,
        float xUp, float yUp, float zUp)
    {
        Span<float> direction = stackalloc float[3];
        Span<float> right = stackalloc float[3];
        Span<float> up = stackalloc float[3];
        up[0] = xUp;
        up[1] = yUp;
        up[2] = zUp;
        direction[0] = xLook - xPos;
        direction[1] = yLook - yPos;
        direction[2] = zLook - zPos;
        NormalizeVector(direction);
        CrossProduct(direction, up, right);
        NormalizeVector(right);
        CrossProduct(right, direction, up);
        NormalizeVector(up);

        Span<float> view = stackalloc float[16];
        view[0] = right[0];
        view[4] = right[1];
        view[8] = right[2];
        view[1] = up[0];
        view[5] = up[1];
        view[9] = up[2];
        view[2] = -direction[0];
        view[6] = -direction[1];
        view[10] = -direction[2];
        view[15] = 1f;

        Span<float> translation = stackalloc float[16];
        SetIdentity(translation);
        translation[12] = -xPos;
        translation[13] = -yPos;
        translation[14] = -zPos;

        MultMatrix(view);
        MultMatrix(translation);
    }

    public void Perspective(float fovDegrees, float ratio, float nearPlane, float farPlane)
    {
        var f = 1f / MathF.Tan(fovDegrees * (MathF.PI / 360f));
        LoadIdentity();
        _matrix[0] = f / ratio;
        _matrix[5] = f;
        _matrix[10] = (farPlane + nearPlane) / (nearPlane - farPlane);
        _matrix[11] = -1f;
        _matrix[14] = 2f * farPlane * nearPlane / (nearPlane - farPlane);
        _matrix[15] = 0f;
    }

    public void Ortho(float left, float right, float bottom, float top, float nearPlane = -1f, float farPlane = 1f)
    {
        LoadIdentity();
        _matrix[0] = 2f / (right - left);
        _matrix[5] = 2f / (top - bottom);
        _matrix[10] = -2f / (farPlane - nearPlane);
        _matrix[12] = -(right + left) / (right - left);
        _matrix[13] = -(top + bottom) / (top - bottom);
        _matrix[14] = -(farPlane + nearPlane) / (farPlane - nearPlane);
    }

    public void Frustum(float left, float right, float bottom, float top, float nearPlane, float farPlane)
    {
        Span<float> projection = stackalloc float[16];
        SetIdentity(projection);
        projection[0] = 2 * nearPlane / (right - left);
        projection[5] = 2 * nearPlane / (top - bottom);
        projection[8] = (right + left) / (right - left);
        projection[9] = (top + bottom) / (top - bottom);
        projection[10] = -(farPlane + nearPlane) / (farPlane - nearPlane);
        projection[11] = -1f;
        projection[14] = -2 * farPlane * nearPlane / (farPlane - nearPlane);
        MultMatrix(projection);
    }

    public void Transpose()
    {
        Span<float> original = stackalloc float[16];
        _matrix.CopyTo(original);
        _matrix[0] = original[0];
        _matrix[1] = original[4];
        _matrix[2] = original[8];
        _matrix[3] = original[12];
        _matrix[4] = original[1];
        _matrix[5] = original[5];
        _matrix[6] = original[9];
        _matrix[7] = original[13];
        _matrix[8] = original[2];
        _matrix[9] = original[6];
        _matrix[10] = original[10];
        _matrix[11] = original[14];
        _matrix[12] = original[3];
        _matrix[13] = original[7];
        _matrix[14] = original[11];
        _matrix[15] = original[15];
    }

    public void MultMatrixPoint(ReadOnlySpan<float> point, Span<float> result)
    {
        for (var row = 0; row < 4; row++)
        {
            result[row] = 0;
            for (var column = 0; column < 4; column++)
                result[row] += point[column] * _matrix[column * 4 + row];
        }
    }

    public void ComputeNormalMatrix(ReadOnlySpan<float> viewMatrix)
    {
        var det =
            viewMatrix[0] * (viewMatrix[5] * viewMatrix[10] - viewMatrix[6] * viewMatrix[9]) +
            viewMatrix[1] * (viewMatrix[6] * viewMatrix[8] - viewMatrix[10] * viewMatrix[2]) +
            viewMatrix[2] * (viewMatrix[3] * viewMatrix[9] - viewMatrix[4] * viewMatrix[8]);
        var invDet = 1.0 / det;
        _matrix[0] = (float)((viewMatrix[5] * viewMatrix[10] - viewMatrix[6] * viewMatrix[9]) * invDet);
        _matrix[1] = (float)((viewMatrix[6] * viewMatrix[8] - viewMatrix[10] * viewMatrix[2]) * invDet);
        _matrix[2] = (float)((viewMatrix[3] * viewMatrix[9] - viewMatrix[4] * viewMatrix[8]) * invDet);
        _matrix[4] = (float)((viewMatrix[2] * viewMatrix[9] - viewMatrix[1] * viewMatrix[10]) * invDet);
        _matrix[5] = (float)((viewMatrix[0] * viewMatrix[10] - viewMatrix[2] * viewMatrix[8]) * invDet);
        _matrix[6] = (float)((viewMatrix[1] * viewMatrix[6] - viewMatrix[7] * viewMatrix[0]) * invDet);
        _matrix[8] = (float)((viewMatrix[1] * viewMatrix[6] - viewMatrix[4] * viewMatrix[2]) * invDet);
        _matrix[9] = (float)((viewMatrix[2] * viewMatrix[3] - viewMatrix[0] * viewMatrix[5]) * invDet);
        _matrix[10] = (float)((viewMatrix[0] * viewMatrix[4] - viewMatrix[3] * viewMatrix[1]) * invDet);
        _matrix[15] = 1f;
    }

    public bool TryInverse(VmVsMatrix result)
    {
        var det = (float)Mat4Determinant(_matrix);
        if (MathF.Abs(det) < 1e-15f)
        {
            Array.Clear(result._matrix);
            return false;
        }

        Mat4Adjoint(_matrix, result._matrix);
        var invDet = 1f / det;
        for (var index = 0; index < 16; index++)
            result._matrix[index] *= invDet;
        return true;
    }

    private static void MultiplyInto(Span<float> result, ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        for (var column = 0; column < 4; column++)
        for (var row = 0; row < 4; row++)
        {
            result[column * 4 + row] = 0;
            for (var k = 0; k < 4; k++)
                result[column * 4 + row] += left[k * 4 + row] * right[column * 4 + k];
        }
    }

    private static void SetIdentity(Span<float> matrix)
    {
        matrix.Clear();
        for (var index = 0; index < 4; index++)
            matrix[index + index * 4] = 1f;
    }

    private static void Normalize(ref float x, ref float y, ref float z)
    {
        var length = MathF.Sqrt(x * x + y * y + z * z);
        if (length <= 0)
            return;
        x /= length;
        y /= length;
        z /= length;
    }

    private static void NormalizeVector(Span<float> vector)
    {
        var length = MathF.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
        if (length <= 0)
            return;
        vector[0] /= length;
        vector[1] /= length;
        vector[2] /= length;
    }

    private static void CrossProduct(ReadOnlySpan<float> a, ReadOnlySpan<float> b, Span<float> result)
    {
        result[0] = a[1] * b[2] - b[1] * a[2];
        result[1] = a[2] * b[0] - b[2] * a[0];
        result[2] = a[0] * b[1] - b[0] * a[1];
    }

    private static double Mat3Determinant(ReadOnlySpan<float> matrix) =>
        matrix[0] * (matrix[4] * matrix[8] - matrix[5] * matrix[7]) +
        matrix[1] * (matrix[5] * matrix[6] - matrix[8] * matrix[3]) +
        matrix[2] * (matrix[3] * matrix[7] - matrix[4] * matrix[6]);

    private static double Mat4Determinant(ReadOnlySpan<float> matrix)
    {
        Span<float> block = stackalloc float[9];
        static double Det3(ReadOnlySpan<float> m) => Mat3Determinant(m);

        block[0] = matrix[5];
        block[1] = matrix[9];
        block[2] = matrix[13];
        block[3] = matrix[6];
        block[4] = matrix[10];
        block[5] = matrix[14];
        block[6] = matrix[7];
        block[7] = matrix[11];
        block[8] = matrix[15];
        var a = Det3(block);

        block[0] = matrix[4];
        block[1] = matrix[8];
        block[2] = matrix[12];
        block[3] = matrix[6];
        block[4] = matrix[10];
        block[5] = matrix[14];
        block[6] = matrix[7];
        block[7] = matrix[11];
        block[8] = matrix[15];
        var b = Det3(block);

        block[0] = matrix[4];
        block[1] = matrix[8];
        block[2] = matrix[12];
        block[3] = matrix[5];
        block[4] = matrix[9];
        block[5] = matrix[13];
        block[6] = matrix[7];
        block[7] = matrix[11];
        block[8] = matrix[15];
        var c = Det3(block);

        block[0] = matrix[4];
        block[1] = matrix[8];
        block[2] = matrix[12];
        block[3] = matrix[5];
        block[4] = matrix[9];
        block[5] = matrix[13];
        block[6] = matrix[6];
        block[7] = matrix[10];
        block[8] = matrix[14];
        var d = Det3(block);

        return matrix[0] * a - matrix[4] * b + matrix[8] * c - matrix[12] * d;
    }

    private static void Mat4Adjoint(ReadOnlySpan<float> matrix, Span<float> result)
    {
        // Port of native mat4Adjoint (3×3 minors).
        static void FillMinor(ReadOnlySpan<float> source, Span<float> minor, int r0, int r1, int r2, int c0, int c1, int c2)
        {
            minor[0] = source[r0 * 4 + c0];
            minor[1] = source[r1 * 4 + c0];
            minor[2] = source[r2 * 4 + c0];
            minor[3] = source[r0 * 4 + c1];
            minor[4] = source[r1 * 4 + c1];
            minor[5] = source[r2 * 4 + c1];
            minor[6] = source[r0 * 4 + c2];
            minor[7] = source[r1 * 4 + c2];
            minor[8] = source[r2 * 4 + c2];
        }

        Span<float> minor = stackalloc float[9];
        FillMinor(matrix, minor, 1, 2, 3, 1, 2, 3);
        result[0] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 1, 2, 3, 0, 2, 3);
        result[4] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 1, 2, 3, 0, 1, 3);
        result[8] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 1, 2, 3, 0, 1, 2);
        result[12] = (float)-Mat3Determinant(minor);

        FillMinor(matrix, minor, 0, 2, 3, 1, 2, 3);
        result[1] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 2, 3, 0, 2, 3);
        result[5] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 2, 3, 0, 1, 3);
        result[9] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 2, 3, 0, 1, 2);
        result[13] = (float)Mat3Determinant(minor);

        FillMinor(matrix, minor, 0, 1, 3, 1, 2, 3);
        result[2] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 3, 0, 2, 3);
        result[6] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 3, 0, 1, 3);
        result[10] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 3, 0, 1, 2);
        result[14] = (float)-Mat3Determinant(minor);

        FillMinor(matrix, minor, 0, 1, 2, 1, 2, 3);
        result[3] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 2, 0, 2, 3);
        result[7] = (float)Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 2, 0, 1, 3);
        result[11] = (float)-Mat3Determinant(minor);
        FillMinor(matrix, minor, 0, 1, 2, 0, 1, 2);
        result[15] = (float)Mat3Determinant(minor);
    }
}

public readonly record struct VmQuaternion(float X, float Y, float Z, float W);
