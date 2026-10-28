namespace HCDE.Playsim;

public sealed record SimSparseIntEntry(int Index, int Value);

public sealed record SimSparseIntArray(int ArrayId, IReadOnlyList<SimSparseIntEntry> Entries);

public sealed record SimAcsFiberState(
    int ScriptNumber,
    uint CodeHash,
    bool Always,
    bool Suspended,
    bool LegacyHexenDelay,
    uint ActivatorId,
    int TriggerLineIndex,
    bool BackSide,
    int Pc,
    int Wait,
    int ScriptWaitState,
    int ScriptWaitTarget,
    int TagWait,
    IReadOnlyList<int> Stack,
    IReadOnlyList<int> Locals);

public sealed class SimAcsState
{
    private const int GlobalCount = 64;
    private const int MapVarCount = 128;
    private const int WorldVarCount = 256;

    public int[] Globals { get; init; } = new int[GlobalCount];
    public int[] MapVariables { get; init; } = new int[MapVarCount];
    public int[] WorldVariables { get; init; } = new int[WorldVarCount];
    public IReadOnlyList<SimSparseIntArray> WorldArrays { get; init; } = Array.Empty<SimSparseIntArray>();
    public IReadOnlyList<SimSparseIntArray> GlobalArrays { get; init; } = Array.Empty<SimSparseIntArray>();
    public int[][] MapArrays { get; init; } = Array.Empty<int[]>();
    public IReadOnlyList<string> GlobalStrings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SimAcsFiberState> Fibers { get; init; } = Array.Empty<SimAcsFiberState>();
    public ulong? RandomState { get; init; }
    public int ScriptResult { get; init; }

    internal bool IsDefault =>
        ScriptResult == 0
        && RandomState is null
        && Fibers.Count == 0
        && GlobalStrings.Count == 0
        && WorldArrays.Count == 0
        && GlobalArrays.Count == 0
        && MapArrays.Length == 0
        && Globals.All(value => value == 0)
        && MapVariables.All(value => value == 0)
        && WorldVariables.All(value => value == 0);
}
