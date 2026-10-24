namespace HCDE.Scripting;

/// <summary>Native <c>scriptutil.cpp</c> parameter stack and VM dispatch.</summary>
public sealed class VmScriptUtil
{
    private readonly VmScriptUtilParameterStack _parameters = new();
    private readonly Dictionary<string, VmFunction> _functions = new(StringComparer.Ordinal);
    private readonly Func<string, VmFunction?> _resolveUndefined;

    public VmScriptUtil(Func<string, VmFunction?> resolveUndefined)
    {
        _resolveUndefined = resolveUndefined ?? throw new ArgumentNullException(nameof(resolveUndefined));
    }

    public void Clear()
    {
        _parameters.Clear();
        _functions.Clear();
    }

    public int Exec(string functionName, IReadOnlyList<int> typeStream, IReadOnlyList<object?> values)
    {
        var paramStart = _parameters.BeginFrame();
        try
        {
            _parameters.AppendFromTypeStream(typeStream, values);
            return RunFunction(functionName, paramStart);
        }
        catch
        {
            _parameters.Clamp(paramStart);
            throw;
        }
        finally
        {
            _parameters.Clamp(paramStart);
        }
    }

    public int RunFunction(string functionName, int paramStart)
    {
        if (!_functions.TryGetValue(functionName, out var function))
        {
            function = _resolveUndefined(functionName)
                       ?? throw new InvalidOperationException($"Call to undefined function ScriptUtil.{functionName}");
            _functions[functionName] = function;
        }

        var slice = _parameters.Snapshot().Skip(paramStart).Select(ToVmValue).ToArray();
        var result = VmExec.RunNativeFunction(function, slice, 1);
        if (!result.Returned || result.Values.Count == 0)
            throw new InvalidOperationException($"ScriptUtil.{functionName} failed.");
        return result.Values[0].Value;
    }

    private static VmValue ToVmValue(object? value) => value switch
    {
        null => VmValue.Nil(),
        int integer => VmValue.FromInteger(integer),
        double floating => VmValue.FromFloat(floating),
        string text => VmValue.FromString(text),
        _ => VmValue.FromObject(value),
    };
}
