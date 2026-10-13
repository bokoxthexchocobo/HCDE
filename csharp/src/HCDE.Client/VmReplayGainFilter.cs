namespace HCDE.Client;

public sealed class VmReplayGainFilter
{
    private readonly float[] kernel;
    private readonly float[] inputHistory;
    private readonly float[] outputHistory;
    private readonly bool yule;

    public VmReplayGainFilter(ReadOnlySpan<float> kernel, bool yule)
    {
        var order = yule ? 10 : 2;
        if (kernel.Length != order * 2 + 1) throw new ArgumentException("Invalid filter coefficient count.", nameof(kernel));
        this.kernel = kernel.ToArray();
        this.yule = yule;
        inputHistory = new float[order];
        outputHistory = new float[order];
    }

    public void Process(ReadOnlySpan<float> input, Span<float> output)
    {
        if (output.Length < input.Length) throw new ArgumentException("Output cannot hold all input samples.", nameof(output));
        if (input.Overlaps(output, out var offset) && offset != 0)
            throw new ArgumentException("Input and output must not partially overlap.", nameof(output));
        for (var i = 0; i < input.Length; i++)
        {
            var sample = input[i];
            var value = yule ? 1e-10f + sample * kernel[0] : sample * kernel[0];
            for (var j = 0; j < inputHistory.Length; j++)
            {
                value -= outputHistory[j] * kernel[j * 2 + 1];
                value += inputHistory[j] * kernel[j * 2 + 2];
            }
            for (var j = inputHistory.Length - 1; j > 0; j--)
            {
                inputHistory[j] = inputHistory[j - 1];
                outputHistory[j] = outputHistory[j - 1];
            }
            inputHistory[0] = sample;
            outputHistory[0] = value;
            output[i] = value;
        }
    }

    public void Reset()
    {
        Array.Clear(inputHistory);
        Array.Clear(outputHistory);
    }
}
