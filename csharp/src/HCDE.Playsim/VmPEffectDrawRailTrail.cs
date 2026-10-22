namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_DrawRailTrail</c> (<c>p_effect.cpp</c> 674–985).</summary>
public static class VmPEffectDrawRailTrail
{
    public const int RafFullbright = 1;
    public const int RafSilent = 2;
    public const int SpfFullbright = 1;

    public sealed class PortalHit
    {
        public double ContX { get; set; }
        public double ContY { get; set; }
        public double ContZ { get; set; }
        public double OutDirX { get; set; }
        public double OutDirY { get; set; }
        public double OutDirZ { get; set; }
        public double HitX { get; set; }
        public double HitY { get; set; }
        public double HitZ { get; set; }
    }

    public sealed class TrailSegment
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double DirX { get; set; }
        public double DirY { get; set; }
        public double DirZ { get; set; }
        public double ExtendX { get; set; }
        public double ExtendY { get; set; }
        public double ExtendZ { get; set; }
        public double Length { get; set; }
        public double SoundPosX { get; set; }
        public double SoundPosY { get; set; }
        public double SoundDist { get; set; }
    }

    public sealed class ParticleSpawn
    {
        public int Ttl { get; set; }
        public float Alpha { get; set; }
        public int Size { get; set; }
        public int Flags { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double AccZ { get; set; }
        public int Color { get; set; }
    }

    public sealed class DrawRailTrailState
    {
        public bool SourceNull { get; set; }
        public bool LevelNull { get; set; }
        public IReadOnlyList<PortalHit> PortalHits { get; set; } = Array.Empty<PortalHit>();
        public int Color1 { get; set; }
        public int Color2 { get; set; }
        public double MaxDiff { get; set; }
        public int Flags { get; set; }
        public object? SpawnClass { get; set; }
        public double Angle { get; set; }
        public int Duration { get; set; }
        public double Sparsity { get; set; }
        public double Drift { get; set; }
        public int SpiralOffset { get; set; }
        public double Pitch { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasReadyWeapon { get; set; }
        public int AttackSoundId { get; set; }
        public int WeaponAttackSoundId { get; set; }
        public double CameraX { get; set; }
        public double CameraY { get; set; }
        public double CameraZ { get; set; }
        public double ViewpointZ { get; set; }
        public double RailSpiralSparsity { get; set; } = 1;
        public double RailTrailSparsity { get; set; } = 1;
        public bool RailSmartSpiral { get; set; }
        public int DefaultRailSoundId { get; set; }
        public int Ticrate { get; set; } = 35;
        public List<TrailSegment> Trail { get; } = new();
        public List<ParticleSpawn> Particles { get; } = new();
        public List<object> SpawnedActors { get; } = new();
        public int SoundCalls { get; set; }
        public Func<int, int>? Random { get; init; }
        public Func<int, int>? RailTrailRandom { get; init; }
        public Func<ParticleSpawn?>? NewParticle { get; init; }
        public Func<int, ParticleSpawn?>? JitterParticle { get; init; }
        public Func<double, double, double, object?>? SpawnActor { get; init; }
        public Action<bool, int>? PlaySoundAtCamera { get; init; }
        public Action<double, double, double, int>? PlaySoundAtPoint { get; init; }
        public Func<int, int>? ParticleColor { get; init; }
        public Func<int, int, int, int>? PickRailBlue { get; init; }
        public Func<int, int, int>? PickGrey { get; init; }
    }

    static void BuildPerpendicular(TrailSegment seg)
    {
        var dir = new[] { seg.DirX, seg.DirY, seg.DirZ };
        var minelem = 1.0;
        var epos = 0;
        for (var ii = 0; ii < 3; ii++)
        {
            if (Math.Abs(dir[ii]) < minelem)
            {
                epos = ii;
                minelem = Math.Abs(dir[ii]);
            }
        }

        var temp = new double[3];
        temp[epos] = 1;
        var dot = dir[0] * temp[0] + dir[1] * temp[1] + dir[2] * temp[2];
        seg.ExtendX = (temp[0] - dot * dir[0]) * 3;
        seg.ExtendY = (temp[1] - dot * dir[1]) * 3;
        seg.ExtendZ = (temp[2] - dot * dir[2]) * 3;
    }

    public static void DrawRailTrail(DrawRailTrailState s)
    {
        if (s.SourceNull || s.LevelNull || s.PortalHits.Count < 2)
            return;

        var length = 0.0;
        s.Trail.Clear();
        for (var i = 0; i < s.PortalHits.Count - 1; i++)
        {
            var hit = s.PortalHits[i];
            var next = s.PortalHits[i + 1];
            var seg = new TrailSegment
            {
                StartX = hit.ContX,
                StartY = hit.ContY,
                StartZ = hit.ContZ,
                DirX = hit.OutDirX,
                DirY = hit.OutDirY,
                DirZ = hit.OutDirZ,
            };
            var dx = next.HitX - seg.StartX;
            var dy = next.HitY - seg.StartY;
            var dz = next.HitZ - seg.StartZ;
            seg.Length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            BuildPerpendicular(seg);
            length += seg.Length;

            if (s.HasPlayer && seg.Length > 0)
            {
                var r = ((seg.StartY - s.CameraY) * (-seg.DirY) - (seg.StartX - s.CameraX) * seg.DirX)
                    / (seg.Length * seg.Length);
                r = Math.Clamp(r, 0, 1);
                seg.SoundPosX = seg.StartX + r * seg.DirX;
                seg.SoundPosY = seg.StartY + r * seg.DirY;
                var sdx = seg.SoundPosX - s.CameraX;
                var sdy = seg.SoundPosY - s.CameraY;
                seg.SoundDist = sdx * sdx + sdy * sdy;
            }
            else
            {
                seg.SoundDist = -1;
            }

            s.Trail.Add(seg);
        }

        var steps = (int)Math.Floor(length / 3);
        var fullbright = (s.Flags & RafFullbright) != 0;
        if (steps == 0)
            return;

        if ((s.Flags & RafSilent) == 0 && s.HasPlayer)
        {
            var sound = !s.HasReadyWeapon ? s.AttackSoundId : s.WeaponAttackSoundId;
            if (sound == 0)
                sound = s.DefaultRailSoundId;
            if (sound != 0)
            {
                if (Math.Abs(s.CameraX - s.Trail[0].StartX) < 20 && Math.Abs(s.CameraY - s.Trail[0].StartY) < 20)
                    s.PlaySoundAtCamera?.Invoke(true, sound);
                else
                {
                    var shortest = s.Trail[0];
                    foreach (var seg in s.Trail)
                    {
                        if (shortest.SoundDist < 0 || seg.SoundDist < shortest.SoundDist)
                            shortest = seg;
                    }

                    s.PlaySoundAtPoint?.Invoke(shortest.SoundPosX, shortest.SoundPosY, s.ViewpointZ, sound);
                }

                s.SoundCalls++;
            }
        }

        if (s.Color1 != -1 && (!s.RailSmartSpiral || s.Color2 == -1) && s.RailSpiralSparsity > 0 && s.SpawnClass == null)
        {
            var stepsize = 3 * s.RailSpiralSparsity * s.Sparsity;
            var spiralSteps = (int)(steps * s.RailSpiralSparsity / s.Sparsity);
            var segment = 0;
            var lencount = s.Trail[0].Length;
            var color1 = s.Color1 == 0 ? -1 : s.ParticleColor?.Invoke(s.Color1) ?? s.Color1;
            var posX = s.Trail[0].StartX;
            var posY = s.Trail[0].StartY;
            var posZ = s.Trail[0].StartZ;
            var deg = s.SpiralOffset;
            for (var i = spiralSteps; i > 0; i--)
            {
                var p = s.NewParticle?.Invoke();
                if (p == null)
                    return;
                var spiralduration = s.Duration == 0 ? s.Ticrate : s.Duration;
                p.Alpha = 1;
                p.Ttl = spiralduration;
                p.Size = 3;
                if (fullbright)
                    p.Flags |= SpfFullbright;
                var seg = s.Trail[segment];
                var tempX = seg.ExtendX;
                var tempY = seg.ExtendY;
                var tempZ = seg.ExtendZ;
                p.VelX = tempX * s.Drift / 16;
                p.VelY = tempY * s.Drift / 16;
                p.VelZ = tempZ * s.Drift / 16;
                p.PosX = posX + tempX;
                p.PosY = posY + tempY;
                p.PosZ = posZ + tempZ;
                posX += seg.DirX * stepsize;
                posY += seg.DirY * stepsize;
                posZ += seg.DirZ * stepsize;
                deg += (int)(s.RailSpiralSparsity * 14);
                lencount -= stepsize;
                p.Color = color1 == -1 ? s.PickRailBlue?.Invoke(s.Random?.Invoke(0) ?? 0, 0, 0) ?? 0 : color1;
                s.Particles.Add(p);
                if (lencount <= 0)
                {
                    segment++;
                    if (segment < s.Trail.Count)
                    {
                        var ns = s.Trail[segment];
                        posX = ns.StartX - ns.DirX * lencount;
                        posY = ns.StartY - ns.DirY * lencount;
                        posZ = ns.StartZ - ns.DirZ * lencount;
                        lencount += ns.Length;
                    }
                    else
                        break;
                }
            }
        }

        if (s.Color2 != -1 && s.RailTrailSparsity > 0 && s.SpawnClass == null)
        {
            var stepsize = 3 * s.RailTrailSparsity * s.Sparsity;
            var trailSteps = (int)Math.Floor(steps * s.RailTrailSparsity / s.Sparsity);
            var color2 = s.Color2 == 0 ? -1 : s.ParticleColor?.Invoke(s.Color2) ?? s.Color2;
            var diffX = 0.0;
            var diffY = 0.0;
            var diffZ = 0.0;
            var posX = s.Trail[0].StartX;
            var posY = s.Trail[0].StartY;
            var posZ = s.Trail[0].StartZ;
            var lencount = s.Trail[0].Length;
            var segment = 0;
            for (var i = trailSteps; i > 0; i--)
            {
                var innerduration = s.Duration == 0 ? 33 : s.Duration;
                var p = s.JitterParticle?.Invoke(innerduration);
                if (p == null)
                    return;
                if (s.MaxDiff > 0)
                {
                    var rnd = s.Random?.Invoke(0) ?? 0;
                    if ((rnd & 1) != 0)
                        diffX = Math.Clamp(diffX + ((rnd & 8) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                    if ((rnd & 2) != 0)
                        diffY = Math.Clamp(diffY + ((rnd & 16) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                    if ((rnd & 4) != 0)
                        diffZ = Math.Clamp(diffZ + ((rnd & 32) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                }

                p.Size = 2;
                p.PosX = posX + diffX;
                p.PosY = posY + diffY;
                p.PosZ = posZ + diffZ;
                if (s.Color1 != -1)
                    p.AccZ -= 1.0 / 4096;
                var seg = s.Trail[segment];
                posX += seg.DirX * stepsize;
                posY += seg.DirY * stepsize;
                posZ += seg.DirZ * stepsize;
                lencount -= stepsize;
                if (fullbright)
                    p.Flags |= SpfFullbright;
                p.Color = color2 == -1 ? s.PickGrey?.Invoke(s.Random?.Invoke(0) ?? 0, 0) ?? 0 : color2;
                s.Particles.Add(p);
                if (lencount <= 0)
                {
                    segment++;
                    if (segment < s.Trail.Count)
                    {
                        var ns = s.Trail[segment];
                        posX = ns.StartX - ns.DirX * lencount;
                        posY = ns.StartY - ns.DirY * lencount;
                        posZ = ns.StartZ - ns.DirZ * lencount;
                        lencount += ns.Length;
                    }
                    else
                        break;
                }
            }
        }

        if (s.SpawnClass != null)
        {
            var sparsity = s.Sparsity < 1 ? 32 : s.Sparsity;
            var stepsize = sparsity;
            var trailSteps = (int)((steps * 3) / sparsity);
            var diffX = 0.0;
            var diffY = 0.0;
            var diffZ = 0.0;
            var posX = s.Trail[0].StartX;
            var posY = s.Trail[0].StartY;
            var posZ = s.Trail[0].StartZ;
            var lencount = s.Trail[0].Length;
            var segment = 0;
            for (var i = trailSteps; i > 0; i--)
            {
                if (s.MaxDiff > 0)
                {
                    var rnd = s.RailTrailRandom?.Invoke(0) ?? 0;
                    if ((rnd & 1) != 0)
                        diffX = Math.Clamp(diffX + ((rnd & 8) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                    if ((rnd & 2) != 0)
                        diffY = Math.Clamp(diffY + ((rnd & 16) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                    if ((rnd & 4) != 0)
                        diffZ = Math.Clamp(diffZ + ((rnd & 32) != 0 ? 1 : -1), -s.MaxDiff, s.MaxDiff);
                }

                var thing = s.SpawnActor?.Invoke(posX + diffX, posY + diffY, posZ + diffZ);
                if (thing != null)
                    s.SpawnedActors.Add(thing);
                var seg = s.Trail[segment];
                posX += seg.DirX * stepsize;
                posY += seg.DirY * stepsize;
                posZ += seg.DirZ * stepsize;
                lencount -= stepsize;
                if (lencount <= 0)
                {
                    segment++;
                    if (segment < s.Trail.Count)
                    {
                        var ns = s.Trail[segment];
                        posX = ns.StartX - ns.DirX * lencount;
                        posY = ns.StartY - ns.DirY * lencount;
                        posZ = ns.StartZ - ns.DirZ * lencount;
                        lencount += ns.Length;
                    }
                    else
                        break;
                }
            }
        }
    }
}
