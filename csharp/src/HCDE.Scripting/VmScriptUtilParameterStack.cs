namespace HCDE.Scripting;

/// <summary>Native <c>scriptutil.cpp</c> <c>BuildParameters</c> / stack clamp.</summary>
public sealed class VmScriptUtilParameterStack
{
    public const int TypeEnd = 0;
    public const int TypeInt = 1;
    public const int TypePointer = 2;
    public const int TypeFloat = 3;
    public const int TypeString = 4;
    public const int TypeClass = 5;

    private readonly List<object?> _parameters = new();

    public int Count => _parameters.Count;

    public void Clear() => _parameters.Clear();

    public int BeginFrame() => _parameters.Count;

    public void AppendFromTypeStream(IReadOnlyList<int> typeStream, IReadOnlyList<object?> values)
    {
        var valueIndex = 0;
        for (var index = 0; index < typeStream.Count && typeStream[index] != TypeEnd; index++)
        {
            switch (typeStream[index])
            {
                case TypeInt:
                    _parameters.Add(values[valueIndex++]);
                    break;
                case TypePointer:
                case TypeClass:
                case TypeString:
                    _parameters.Add(values[valueIndex++]);
                    break;
                case TypeFloat:
                    _parameters.Add(values[valueIndex++]);
                    break;
            }
        }
    }

    public void Clamp(int paramStart)
    {
        if (paramStart < 0 || paramStart > _parameters.Count)
            return;
        _parameters.RemoveRange(paramStart, _parameters.Count - paramStart);
    }

    public IReadOnlyList<object?> Snapshot() => _parameters.AsReadOnly();
}
