namespace HCDE.Playsim;

/// <summary>
/// Static and dynamic AABB build, longest-axis split, and the moved-line
/// refresh from <c>doom_aabbtree.cpp</c> (1-265). Chunk 1283. The path walk
/// matches <c>FindNodePath</c> so a moved line can refresh its leaf. That
/// walk is not a second credit.
/// </summary>
public sealed class VmAabbLine1283
{
    public float X1;
    public float Y1;
    public float X2;
    public float Y2;
    public bool BackSector;
    public bool Sidedef0;
    public int Flags;
}

public sealed class VmAabbNode1283
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;
    public int LeftNode = -1;
    public int RightNode = -1;
    public int LineIndex = -1;
}

public sealed class VmAabbTreeLine1283
{
    public float X;
    public float Y;
    public float Dx;
    public float Dy;
}

public sealed class VmDoomAabb1283
{
    public const int WallPoly = 64;

    public List<VmAabbLine1283> LevelLines = new();
    public List<VmAabbNode1283> Nodes = new();
    public List<VmAabbTreeLine1283> TreeLines = new();
    public List<int> MapLines = new();
    public int DynamicStartNode;
    public int DynamicStartLine;

    public void Build(IReadOnlyList<VmAabbLine1283> lines)
    {
        LevelLines = new List<VmAabbLine1283>(lines);
        Nodes.Clear();
        TreeLines.Clear();
        MapLines.Clear();
        DynamicStartNode = 0;
        DynamicStartLine = 0;
        var centroids = new (float X, float Y)[lines.Count];
        for (int i = 0; i < lines.Count; i++)
        {
            float x1 = lines[i].X1;
            float y1 = lines[i].Y1;
            float x2 = lines[i].X2;
            float y2 = lines[i].Y2;
            centroids[i] = ((x1 + x2) * 0.5f, (y1 + y2) * 0.5f);
        }
        if (!GenerateTree(centroids, false))
            return;
        int staticRoot = Nodes.Count - 1;
        DynamicStartNode = Nodes.Count;
        DynamicStartLine = TreeLines.Count;
        if (GenerateTree(centroids, true))
        {
            int dynamicRoot = Nodes.Count - 1;
            VmAabbNode1283 left = Nodes[staticRoot];
            VmAabbNode1283 right = Nodes[dynamicRoot];
            float minX = MathF.Min(left.Left, right.Left);
            float minY = MathF.Min(left.Top, right.Top);
            float maxX = MathF.Max(left.Right, right.Right);
            float maxY = MathF.Max(left.Bottom, right.Bottom);
            Nodes.Add(MakeNode(minX, minY, maxX, maxY, staticRoot, dynamicRoot));
        }
        TreeLines.Clear();
        for (int i = 0; i < MapLines.Count; i++)
        {
            VmAabbLine1283 line = LevelLines[MapLines[i]];
            var treeLine = new VmAabbTreeLine1283
            {
                X = line.X1,
                Y = line.Y1,
                Dx = line.X2 - line.X1,
                Dy = line.Y2 - line.Y1,
            };
            TreeLines.Add(treeLine);
        }
    }

    public bool Update()
    {
        bool modified = false;
        for (int i = DynamicStartLine; i < MapLines.Count; i++)
        {
            VmAabbLine1283 line = LevelLines[MapLines[i]];
            var treeLine = new VmAabbTreeLine1283
            {
                X = line.X1,
                Y = line.Y1,
                Dx = line.X2 - line.X1,
                Dy = line.Y2 - line.Y1,
            };
            if (LinesDiffer(TreeLines[i], treeLine))
            {
                List<int> path = FindNodePath(i, Nodes.Count - 1);
                if (path.Count > 0)
                {
                    int nodeIndex = path[0];
                    float x1 = line.X1;
                    float y1 = line.Y1;
                    float x2 = line.X2;
                    float y2 = line.Y2;
                    Nodes[nodeIndex].Left = MathF.Min(x1, x2);
                    Nodes[nodeIndex].Right = MathF.Max(x1, x2);
                    Nodes[nodeIndex].Top = MathF.Min(y1, y2);
                    Nodes[nodeIndex].Bottom = MathF.Max(y1, y2);
                    for (int j = 1; j < path.Count; j++)
                    {
                        VmAabbNode1283 cur = Nodes[path[j]];
                        VmAabbNode1283 left = Nodes[cur.LeftNode];
                        VmAabbNode1283 right = Nodes[cur.RightNode];
                        cur.Left = MathF.Min(left.Left, right.Left);
                        cur.Top = MathF.Min(left.Top, right.Top);
                        cur.Right = MathF.Max(left.Right, right.Right);
                        cur.Bottom = MathF.Max(left.Bottom, right.Bottom);
                    }
                    TreeLines[i] = treeLine;
                    modified = true;
                }
            }
        }
        return modified;
    }

    bool GenerateTree((float X, float Y)[] centroids, bool dynamicSubtree)
    {
        var lineElements = new List<int>();
        for (int i = 0; i < LevelLines.Count; i++)
        {
            VmAabbLine1283 line = LevelLines[i];
            if (!line.BackSector)
            {
                bool isPolyLine = line.Sidedef0 && (line.Flags & WallPoly) != 0;
                if (isPolyLine && dynamicSubtree)
                {
                    lineElements.Add(MapLines.Count);
                    MapLines.Add(i);
                }
                else if (!isPolyLine && !dynamicSubtree)
                {
                    lineElements.Add(MapLines.Count);
                    MapLines.Add(i);
                }
            }
        }
        if (lineElements.Count == 0)
            return false;
        int[] work = new int[lineElements.Count * 2];
        int[] lines = lineElements.ToArray();
        GenerateTreeNode(lines, 0, lines.Length, centroids, work);
        return true;
    }

    int GenerateTreeNode(int[] lines, int offset, int numLines, (float X, float Y)[] centroids, int[] work)
    {
        if (numLines == 0)
            return -1;
        float medianX = 0;
        float medianY = 0;
        int first = MapLines[lines[offset]];
        VmAabbLine1283 firstLine = LevelLines[first];
        float minX = firstLine.X1;
        float minY = firstLine.Y1;
        float maxX = minX;
        float maxY = minY;
        for (int i = 0; i < numLines; i++)
        {
            VmAabbLine1283 line = LevelLines[MapLines[lines[offset + i]]];
            float x1 = line.X1;
            float y1 = line.Y1;
            float x2 = line.X2;
            float y2 = line.Y2;
            minX = MathF.Min(minX, x1);
            minX = MathF.Min(minX, x2);
            minY = MathF.Min(minY, y1);
            minY = MathF.Min(minY, y2);
            maxX = MathF.Max(maxX, x1);
            maxX = MathF.Max(maxX, x2);
            maxY = MathF.Max(maxY, y1);
            maxY = MathF.Max(maxY, y2);
            var centroid = centroids[MapLines[lines[offset + i]]];
            medianX += centroid.X;
            medianY += centroid.Y;
        }
        medianX /= numLines;
        medianY /= numLines;
        if (numLines == 1)
        {
            Nodes.Add(MakeLeaf(minX, minY, maxX, maxY, lines[offset]));
            return Nodes.Count - 1;
        }
        float[] axisLengths = { maxX - minX, maxY - minY };
        int[] axisOrder = { 0, 1 };
        if (axisLengths[1] > axisLengths[0])
        {
            axisOrder[0] = 1;
            axisOrder[1] = 0;
        }
        float[] axisX = { 1f, 0f };
        float[] axisY = { 0f, 1f };
        int leftCount = 0;
        int rightCount = 0;
        for (int attempt = 0; attempt < 2; attempt++)
        {
            int axis = axisOrder[attempt];
            float planeX = axisX[axis];
            float planeY = axisY[axis];
            float planeW = -(medianX * planeX + medianY * planeY);
            leftCount = 0;
            rightCount = 0;
            for (int i = 0; i < numLines; i++)
            {
                int lineIndex = lines[offset + i];
                var centroid = centroids[MapLines[lineIndex]];
                float side = centroid.X * planeX + centroid.Y * planeY + planeW;
                if (side >= 0f)
                {
                    work[leftCount] = lineIndex;
                    leftCount++;
                }
                else
                {
                    work[numLines + rightCount] = lineIndex;
                    rightCount++;
                }
            }
            if (leftCount != 0 && rightCount != 0)
                break;
        }
        if (leftCount == 0 || rightCount == 0)
        {
            leftCount = numLines / 2;
            rightCount = numLines - leftCount;
        }
        else
        {
            for (int i = 0; i < leftCount; i++)
                lines[offset + i] = work[i];
            for (int i = 0; i < rightCount; i++)
                lines[offset + i + leftCount] = work[numLines + i];
        }
        int leftIndex = -1;
        int rightIndex = -1;
        if (leftCount > 0)
            leftIndex = GenerateTreeNode(lines, offset, leftCount, centroids, work);
        if (rightCount > 0)
            rightIndex = GenerateTreeNode(lines, offset + leftCount, rightCount, centroids, work);
        Nodes.Add(MakeNode(minX, minY, maxX, maxY, leftIndex, rightIndex));
        return Nodes.Count - 1;
    }

    List<int> FindNodePath(int line, int node)
    {
        VmAabbNode1283 current = Nodes[node];
        VmAabbTreeLine1283 treeLine = TreeLines[line];
        if (current.Left > treeLine.X || current.Right < treeLine.X || current.Top > treeLine.Y || current.Bottom < treeLine.Y)
            return new List<int>();
        var path = new List<int>();
        if (current.LineIndex == -1)
        {
            path = FindNodePath(line, current.LeftNode);
            if (path.Count == 0)
                path = FindNodePath(line, current.RightNode);
            if (path.Count > 0)
                path.Add(node);
        }
        else if (current.LineIndex == line)
        {
            path.Add(node);
        }
        return path;
    }

    static bool LinesDiffer(VmAabbTreeLine1283 stored, VmAabbTreeLine1283 next)
    {
        return Bits(stored.X) != Bits(next.X)
            || Bits(stored.Y) != Bits(next.Y)
            || Bits(stored.Dx) != Bits(next.Dx)
            || Bits(stored.Dy) != Bits(next.Dy);
    }

    static int Bits(float value)
    {
        return BitConverter.SingleToInt32Bits(value);
    }

    static VmAabbNode1283 MakeLeaf(float minX, float minY, float maxX, float maxY, int lineIndex)
    {
        return new VmAabbNode1283
        {
            Left = minX,
            Top = minY,
            Right = maxX,
            Bottom = maxY,
            LineIndex = lineIndex,
        };
    }

    static VmAabbNode1283 MakeNode(float minX, float minY, float maxX, float maxY, int left, int right)
    {
        return new VmAabbNode1283
        {
            Left = minX,
            Top = minY,
            Right = maxX,
            Bottom = maxY,
            LeftNode = left,
            RightNode = right,
            LineIndex = -1,
        };
    }
}
