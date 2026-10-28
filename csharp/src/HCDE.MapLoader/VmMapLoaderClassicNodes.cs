namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadNodes</c> validation from <c>maploader.cpp</c> (1156–1230).</summary>
public static class VmMapLoaderClassicNodes
{
    public static bool ShouldAttemptLoad(int nodeLumpBytes, int nodeRecordSize, int nodeHeaderOffset, int maxSubsectors)
    {
        var numNodes = nodeLumpBytes >= nodeHeaderOffset
            ? (nodeLumpBytes - nodeHeaderOffset) / nodeRecordSize
            : 0;
        return !((numNodes == 0 && maxSubsectors != 1) || maxSubsectors == 0);
    }

    public enum ChildResolution
    {
        InvalidSubsector,
        InvalidNode,
        NodeAlreadyUsed,
        NodeChild,
        SubsectorChild,
    }

    public static ChildResolution ResolveChild(
        int child,
        int subsectorFlag,
        int maxSubsectors,
        int numNodes,
        int nodeIndex,
        int[] usedByNode)
    {
        if ((child & subsectorFlag) != 0)
        {
            var ss = child & ~subsectorFlag;
            if (ss >= maxSubsectors)
                return ChildResolution.InvalidSubsector;
            return ChildResolution.SubsectorChild;
        }

        if ((uint)child >= numNodes)
            return ChildResolution.InvalidNode;

        if (usedByNode[child] != 0)
            return ChildResolution.NodeAlreadyUsed;

        return ChildResolution.NodeChild;
    }

    public static void MarkNodeUsed(int[] usedByNode, int child, int parentIndex, int side) =>
        usedByNode[child] = side + 1;
}
