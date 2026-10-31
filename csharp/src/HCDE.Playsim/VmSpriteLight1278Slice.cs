using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Light-probe grid, smoothstep falloff, and dynamic sprite light from
/// <c>hw_spritelight.cpp</c> (1-278). Chunk 1278.
/// </summary>
public sealed class VmSpriteProbe1278
{
    public int Id;
    public float X;
    public float Y;
    public float Z;
    public float Red;
    public float Green;
    public float Blue;
}
public sealed class VmSpriteProbeCell1278
{
    public VmSpriteProbe1278[] Probes = Array.Empty<VmSpriteProbe1278>();
}
public sealed class VmSpriteActor1278
{
    public int Id;
    public double X;
    public double Y;
    public double Center;
    public double RenderRadius;
    public double Alpha = 1;
    public int RenderFlags2;
    public VmSpriteSection1278? Section;
    public int PortalGroup;
}
public sealed class VmSpriteParticle1278
{
    public double X;
    public double Y;
    public double Z;
    public int Flags;
    public VmSpriteSubsector1278? Subsector;
}
public sealed class VmSpriteSection1278
{
    public int Index;
    public int ValidCount;
}
public sealed class VmSpriteSubsector1278
{
    public VmSpriteSection1278 Section = new();
    public int PortalGroup;
}
public sealed class VmSpriteLightNode1278
{
    public VmSpriteDynLight1278 Light;

    public VmSpriteLightNode1278(VmSpriteDynLight1278 light)
    {
        Light = light;
    }
}
public sealed class VmSpriteDynLight1278
{
    public int Id;
    public double X;
    public double Y;
    public double Z;
    public float Radius;
    public float SecondRadius;
    public bool UseSecondRadius;
    public int Red;
    public int Green;
    public int Blue;
    public double Intensity = 1;
    public bool Spot;
    public bool Subtractive;
    public bool ShadowMapped;
    public bool ShadowPass = true;
    public bool LightActor = true;
    public double Yaw;
    public double Pitch;
    public double OuterDegrees;
    public double InnerDegrees;
    public int SectorGroup;
    public VmSpriteActor1278? Target;
    public double RelX;
    public double RelY;
    public double RelZ;
    public int RadiusReads;
    public int IntensityReads;
    public int ActorChecks;
    public int ShadowCalls;
    public VmSpriteActor1278? LastActor;

    public float ReadRadius()
    {
        RadiusReads++;
        if (RadiusReads > 1 && UseSecondRadius)
            return SecondRadius;
        return Radius;
    }

    public double ReadIntensity()
    {
        IntensityReads++;
        return Intensity;
    }

    public bool Accepts(VmSpriteActor1278? self)
    {
        ActorChecks++;
        LastActor = self;
        return LightActor;
    }
}
public sealed class VmSpriteAdded1278
{
    public int Id;
    public int Group;
    public bool Force;
}
public sealed class VmSpriteLight1278
{
    public const int CellSize = 32;
    public const int LightMultAlpha = 0x0400;
    public const int NoDynamicLighting = 0x4000;
    public const int ParticleNoDynamicLighting = 1 << 18;

    public List<VmSpriteProbe1278> Probes = new();
    public int MinX;
    public int MinY;
    public int Width;
    public int Height;
    public VmSpriteProbeCell1278[] Cells = Array.Empty<VmSpriteProbeCell1278>();
    public int CellReads;
    public int DisplacementCount;
    public Dictionary<(int From, int To), (double X, double Y)> Offsets = new();
    public List<List<VmSpriteLightNode1278?>> FlatLists = new();
    public int ValidCount = 1;
    public float LastRadiusSquared;
    public List<VmSpriteSubsector1278> Walk = new();
    public List<VmSpriteAdded1278> Added = new();
    public int ListChecks;
    public int ShadowTests;
    public float LastShadowX;
    public float LastShadowY;
    public float LastShadowZ;

    public static double SmoothStep(double edge0, double edge1, double x)
    {
        double t = (x - edge0) / (edge1 - edge0);
        if (t < 0.0)
            t = 0.0;
        else if (t > 1.0)
            t = 1.0;
        return t * t * (3.0 - 2.0 * t);
    }

    public static double CosDeg(double degrees)
    {
        return Math.Cos(degrees * (Math.PI / 180.0));
    }

    public static double SinDeg(double degrees)
    {
        return Math.Sin(degrees * (Math.PI / 180.0));
    }

    public VmSpriteProbe1278? FindLightProbe(float x, float y, float z)
    {
        VmSpriteProbe1278? found = null;
        if (Probes.Count > 0)
        {
            double rcpCellSize = 1.0 / CellSize;
            int centerX = (int)Math.Floor(x * rcpCellSize) - MinX;
            int centerY = (int)Math.Floor(y * rcpCellSize) - MinY;
            float last = 0f;
            for (int gridY = centerY - 1; gridY <= centerY + 1; gridY++)
            {
                for (int gridX = centerX - 1; gridX <= centerX + 1; gridX++)
                {
                    if (gridX >= 0 && gridY >= 0 && gridX < Width && gridY < Height)
                    {
                        CellReads++;
                        VmSpriteProbeCell1278 cell = Cells[gridX + gridY * Width];
                        for (int i = 0; i < cell.Probes.Length; i++)
                        {
                            VmSpriteProbe1278 probe = cell.Probes[i];
                            float dx = probe.X - x;
                            float dy = probe.Y - y;
                            float dz = probe.Z - z;
                            float dist = dx * dx + dy * dy + dz * dz;
                            if (found == null || dist < last)
                            {
                                found = probe;
                                last = dist;
                            }
                        }
                    }
                }
            }
        }
        return found;
    }

    public void GetDynSpriteLight(
        VmSpriteActor1278? self,
        float x,
        float y,
        float z,
        VmSpriteSection1278 section,
        int portalGroup,
        float[] color)
    {
        color[0] = color[1] = color[2] = 0f;
        VmSpriteProbe1278? probe = FindLightProbe(x, y, z);
        if (probe != null)
        {
            color[0] = probe.Red;
            color[1] = probe.Green;
            color[2] = probe.Blue;
        }
        if (FlatLists.Count > section.Index)
        {
            foreach (VmSpriteLightNode1278? node in FlatLists[section.Index])
            {
                if (node == null)
                    continue;
                VmSpriteDynLight1278 light = node.Light;
                if (!light.Accepts(self))
                    continue;
                float lx;
                float ly;
                float lz;
                if (DisplacementCount > 0)
                {
                    int fromGroup = light.SectorGroup;
                    int toGroup = portalGroup;
                    if (fromGroup == toGroup || fromGroup == 0 || toGroup == 0)
                    {
                        lx = x - (float)light.X;
                        ly = y - (float)light.Y;
                        lz = z - (float)light.Z;
                    }
                    else
                    {
                        (double offX, double offY) = Offset(fromGroup, toGroup);
                        lx = x - (float)(light.X + offX);
                        ly = y - (float)(light.Y + offY);
                        lz = z - (float)light.Z;
                    }
                }
                else
                {
                    lx = x - (float)light.X;
                    ly = y - (float)light.Y;
                    lz = z - (float)light.Z;
                }
                float dist = lx * lx + ly * ly + lz * lz;
                float radius = light.ReadRadius();
                if (dist < radius * radius)
                {
                    dist = MathF.Sqrt(dist);
                    float frac = 1.0f - (dist / radius);
                    if (light.Spot)
                    {
                        float scale = -1.0f / dist;
                        lx *= scale;
                        ly *= scale;
                        lz *= scale;
                        double negPitch = -light.Pitch;
                        double xyLen = CosDeg(negPitch);
                        double spotDirX = -CosDeg(light.Yaw) * xyLen;
                        double spotDirY = -SinDeg(light.Yaw) * xyLen;
                        double spotDirZ = -SinDeg(negPitch);
                        double cosDir = lx * spotDirX + ly * spotDirY + lz * spotDirZ;
                        frac *= (float)SmoothStep(CosDeg(light.OuterDegrees), CosDeg(light.InnerDegrees), cosDir);
                    }
                    if (frac > 0f && (!light.ShadowMapped || (light.ReadRadius() > 0f && ShadowTest(light, x, y, z))))
                    {
                        float lr = light.Red / 255.0f;
                        float lg = light.Green / 255.0f;
                        float lb = light.Blue / 255.0f;
                        if (light.Target != null && (light.Target.RenderFlags2 & LightMultAlpha) != 0)
                        {
                            float alpha = (float)light.Target.Alpha;
                            lr *= alpha;
                            lg *= alpha;
                            lb *= alpha;
                        }
                        lr = (float)((double)lr * light.ReadIntensity());
                        lg = (float)((double)lg * light.ReadIntensity());
                        lb = (float)((double)lb * light.ReadIntensity());
                        if (light.Subtractive)
                        {
                            float sum = lr * lr + lg * lg + lb * lb;
                            float bright = (float)Math.Sqrt((double)sum);
                            lr = (bright - lr) * -1f;
                            lg = (bright - lg) * -1f;
                            lb = (bright - lb) * -1f;
                        }
                        color[0] += lr * frac;
                        color[1] += lg * frac;
                        color[2] += lb * frac;
                    }
                }
            }
        }
    }

    public void GetDynSpriteLight(VmSpriteActor1278? thing, VmSpriteParticle1278? particle, float[] color)
    {
        if (thing != null && (thing.RenderFlags2 & NoDynamicLighting) == 0)
        {
            GetDynSpriteLight(
                thing,
                (float)thing.X,
                (float)thing.Y,
                (float)thing.Center,
                thing.Section!,
                thing.PortalGroup,
                color);
        }
        else if (particle != null && (particle.Flags & ParticleNoDynamicLighting) == 0)
        {
            VmSpriteSubsector1278 sub = particle.Subsector!;
            GetDynSpriteLight(
                null,
                (float)particle.X,
                (float)particle.Y,
                (float)particle.Z,
                sub.Section,
                sub.PortalGroup,
                color);
        }
    }

    public void GetDynModelLight(VmSpriteActor1278? self)
    {
        Added.Clear();
        if (self != null)
        {
            var addedLights = new List<VmSpriteDynLight1278>();
            float x = (float)self.X;
            float y = (float)self.Y;
            float z = (float)self.Center;
            float actorRadius = (float)self.RenderRadius;
            float radiusSquared = actorRadius * actorRadius;
            LastRadiusSquared = radiusSquared;
            ValidCount++;
            foreach (VmSpriteSubsector1278 sub in Walk)
            {
                VmSpriteSection1278 section = sub.Section;
                if (section.ValidCount == ValidCount)
                    continue;
                ListChecks++;
                if (FlatLists.Count > section.Index)
                {
                    foreach (VmSpriteLightNode1278? node in FlatLists[section.Index])
                    {
                        if (node == null)
                            continue;
                        VmSpriteDynLight1278 light = node.Light;
                        if (!light.Accepts(self))
                            continue;
                        int group = sub.PortalGroup;
                        float radius = (float)(light.ReadRadius() + actorRadius);
                        double dx = light.RelX - x;
                        double dy = light.RelY - y;
                        double dz = light.RelZ - z;
                        double distSquared = dx * dx + dy * dy + dz * dz;
                        if (distSquared < radius * radius)
                        {
                            if (!addedLights.Contains(light))
                            {
                                Added.Add(new VmSpriteAdded1278
                                {
                                    Id = light.Id,
                                    Group = group,
                                    Force = true,
                                });
                                addedLights.Add(light);
                            }
                        }
                    }
                }
            }
        }
    }

    bool ShadowTest(VmSpriteDynLight1278 light, float x, float y, float z)
    {
        ShadowTests++;
        light.ShadowCalls++;
        LastShadowX = x;
        LastShadowY = y;
        LastShadowZ = z;
        return light.ShadowPass;
    }

    (double X, double Y) Offset(int fromGroup, int toGroup)
    {
        if (Offsets.TryGetValue((fromGroup, toGroup), out (double X, double Y) found))
            return found;
        return (0d, 0d);
    }

    public static string Num(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
