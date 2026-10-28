namespace HCDE.Client;

/// <summary><c>D_MultiExec</c> and <c>GetCmdLineFiles</c> from <c>d_main.cpp</c>.</summary>
public static partial class VmDMainDebugCcmds
{
    public readonly record struct WadFileEntry(string Path, bool Optional);

    public static IEnumerable<string> MultiExecFileList(IReadOnlyList<string> argList)
    {
        for (var i = 0; i < argList.Count; i++)
            yield return argList[i];
    }

    public static int GetCmdLineFilesStartIndex(bool optional, int existingWadCount) =>
        optional ? 0 : existingWadCount;

    public static bool ShouldClearOptionalFlagAfterAdd(bool optional, int addedCount) =>
        optional && addedCount > 0;

    public static int CountNewWadSlots(int sizeBefore, int sizeAfter) => sizeAfter - sizeBefore;

    public static int ResolveCmdLineArgCount(bool optional, int argcFromArgs) => argcFromArgs;

    public static void ApplyGetCmdLineFiles(
        IList<WadFileEntry> wadFiles,
        bool optional,
        IReadOnlyList<string> args,
        Func<string, IEnumerable<string>> expandWildcards)
    {
        var start = GetCmdLineFilesStartIndex(optional, wadFiles.Count);
        for (var i = start; i < args.Count; i++)
        {
            var sizeBefore = wadFiles.Count;
            foreach (var path in expandWildcards(args[i]))
                wadFiles.Add(new WadFileEntry(path, optional));
            if (ShouldClearOptionalFlagAfterAdd(optional, wadFiles.Count - sizeBefore))
            {
                for (var j = sizeBefore; j < wadFiles.Count; j++)
                    wadFiles[j] = wadFiles[j] with { Optional = false };
            }
        }
    }
}
