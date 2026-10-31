namespace HCDE.Playsim;

/// <summary>
/// AABB node path, ray overlap, and ray-line hit from <c>hw_aabbtree.cpp</c> (<b>1–161</b>).
/// Chunk <b>1216</b>. Node and line coordinates are float and widen to double for the ray.
/// The ray Z is dropped. The walk visits the right child before the left, and a full 32-slot stack skips that node.
/// </summary>
public struct VmAabbNode1216
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;
    public int LeftNode;
    public int RightNode;
    public int LineIndex;
}

public struct VmAabbLine1216
{
    public float X;
    public float Y;
    public float Dx;
    public float Dy;
}

public static class VmAabbTree1216Slice
{
    public const int StackLimit = 32;
    public const double ParallelEpsilon = 0.0000001;

    public static int[] FindNodePath(VmAabbNode1216[] nodes, VmAabbLine1216[] lines, int line, int node)
    {
        VmAabbNode1216 n = nodes[node];
        VmAabbLine1216 mark = lines[line];
        if (n.Left > mark.X || n.Right < mark.X || n.Top > mark.Y || n.Bottom < mark.Y)
            return Array.Empty<int>();
        if (n.LineIndex == -1)
        {
            int[] path = FindNodePath(nodes, lines, line, n.LeftNode);
            if (path.Length == 0)
                path = FindNodePath(nodes, lines, line, n.RightNode);
            if (path.Length != 0)
            {
                int[] grown = new int[path.Length + 1];
                for (int i = 0; i < path.Length; i++)
                    grown[i] = path[i];
                grown[path.Length] = node;
                return grown;
            }
            return path;
        }
        if (n.LineIndex == line)
            return new[] { node };
        return Array.Empty<int>();
    }

    public static double RayTest(
        VmAabbNode1216[] nodes,
        VmAabbLine1216[] lines,
        double startX, double startY, double startZ,
        double endX, double endY, double endZ)
    {
        _ = startZ;
        _ = endZ;
        double rayDx = endX - startX;
        double rayDy = endY - startY;
        double rayDist2 = rayDx * rayDx + rayDy * rayDy;
        double rayNx = rayDy;
        double rayNy = -rayDx;
        double rayD = rayNx * startX + rayNy * startY;
        if (rayDist2 < 1.0)
            return 1.0;
        double hit = 1.0;
        int[] stack = new int[StackLimit];
        int stackPos = 1;
        stack[0] = nodes.Length - 1;
        while (stackPos > 0)
        {
            int nodeIndex = stack[stackPos - 1];
            VmAabbNode1216 node = nodes[nodeIndex];
            if (!Overlap(startX, startY, endX, endY, node))
            {
                stackPos--;
            }
            else if (node.LineIndex != -1)
            {
                double t = Intersect(startX, startY, endX, endY, lines[node.LineIndex], rayDx, rayDy, rayD, rayDist2);
                if (!(hit < t))
                    hit = t;
                stackPos--;
            }
            else if (stackPos == StackLimit)
            {
                stackPos--;
            }
            else
            {
                stack[stackPos - 1] = node.LeftNode;
                stack[stackPos] = node.RightNode;
                stackPos++;
            }
        }
        return hit;
    }

    static bool Overlap(double startX, double startY, double endX, double endY, VmAabbNode1216 node)
    {
        double centerX = (startX + endX) * 0.5;
        double centerY = (startY + endY) * 0.5;
        double centerZ = (0.0 + 0.0) * 0.5;
        double extentX = endX - centerX;
        double extentY = endY - centerY;
        double extentZ = 0.0 - centerZ;
        double boxMinX = node.Left;
        double boxMinY = node.Top;
        double boxMinZ = -1.0;
        double boxMaxX = node.Right;
        double boxMaxY = node.Bottom;
        double boxMaxZ = 1.0;
        double halfX = (boxMaxX - boxMinX) * 0.5;
        double halfY = (boxMaxY - boxMinY) * 0.5;
        double halfZ = (boxMaxZ - boxMinZ) * 0.5;
        centerX -= (boxMaxX + boxMinX) * 0.5;
        centerY -= (boxMaxY + boxMinY) * 0.5;
        centerZ -= (boxMaxZ + boxMinZ) * 0.5;
        double absX = Math.Abs(extentX);
        double absY = Math.Abs(extentY);
        double absZ = Math.Abs(extentZ);
        if (Math.Abs(centerX) > absX + halfX || Math.Abs(centerY) > absY + halfY || Math.Abs(centerZ) > absZ + halfZ)
            return false;
        if (Math.Abs(centerY * extentZ - centerZ * extentY) > halfY * absZ + halfZ * absY
            || Math.Abs(centerX * extentZ - centerZ * extentX) > halfX * absZ + halfZ * absX
            || Math.Abs(centerX * extentY - centerY * extentX) > halfX * absY + halfY * absX)
            return false;
        return true;
    }

    static double Intersect(
        double startX, double startY, double endX, double endY,
        VmAabbLine1216 line, double rayDx, double rayDy, double rayD, double rayDist2)
    {
        _ = endX;
        _ = endY;
        double normalX = rayDy;
        double normalY = -rayDx;
        double lineX = line.X;
        double lineY = line.Y;
        double lineDx = line.Dx;
        double lineDy = line.Dy;
        double den = normalX * lineDx + normalY * lineDy;
        if (Math.Abs(den) > ParallelEpsilon)
        {
            double tLine = (rayD - (normalX * lineX + normalY * lineY)) / den;
            if (tLine >= 0.0 && tLine <= 1.0)
            {
                double hitX = lineX + lineDx * tLine - startX;
                double hitY = lineY + lineDy * tLine - startY;
                double t = (rayDx * hitX + rayDy * hitY) / rayDist2;
                if (t > 0.0)
                    return t;
                return 1.0;
            }
        }
        return 1.0;
    }
}
