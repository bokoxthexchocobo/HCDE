namespace HCDE.Playsim;

/// <summary>View roll, blood type, portal transitions, model timer, and bone queries from <c>p_mobj.cpp</c> (4191–4456).</summary>
public static class VmPMobjPortalBones
{
    public const int SpfInterpolate = 4;
    public const int CfInterpviewangles = 4;
    public const int ModeldataGetBoneInfo = 1;
    public const int ModeldataGetBoneInfoRecalc = 2;
    public const int Mf9Decoupledanimations = 1;
    public const int MdlModelsareattachments = 1;

    public sealed class PortalSector
    {
        public bool CeilingBlocks { get; set; }
        public bool FloorBlocks { get; set; }
        public double CeilingPortalZ { get; set; }
        public double FloorPortalZ { get; set; }
    }

    public sealed class PortalActor
    {
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public PortalSector Sector { get; set; } = new();
        public int PrevPortalGroup { get; set; }
        public (double X, double Y, double Z) Pos { get; set; }
        public (double X, double Y, double Z) Prev { get; set; }
    }

    public sealed class PortalLevelContext
    {
        public Func<(double X, double Y, double Z), (double X, double Y, double Z)>? PosRelativeCeiling { get; init; }
        public Func<(double X, double Y, double Z), (double X, double Y, double Z)>? PosRelativeFloor { get; init; }
        public Func<(double X, double Y, double Z), PortalSector>? PointInSector { get; init; }
        public Action? UnlinkFromWorld { get; init; }
        public Action? LinkToWorld { get; init; }
    }

    public static void SetViewRoll(OrientationActorLike self, double roll, int fflags)
    {
        if (roll == self.ViewRoll)
            return;
        self.ViewRoll = roll;
        if (self.PlayerCheats != null && (fflags & SpfInterpolate) != 0)
            self.PlayerCheats |= CfInterpviewangles;
    }

    public sealed class OrientationActorLike
    {
        public double ViewRoll { get; set; }
        public int? PlayerCheats { get; set; }
    }

    public static object? GetBloodType(bool hasVirtual, Func<object?>? vmCall) =>
        hasVirtual ? vmCall?.Invoke() : null;

    public static (bool Moved, (double X, double Y, double Z) Pos, PortalSector Sector) GetPortalTransition(
        PortalLevelContext ctx,
        PortalActor actor,
        double byOffset)
    {
        var sec = actor.Sector;
        var testZ = actor.Z + byOffset;
        var pos = actor.Pos;
        var moved = false;

        while (!sec.CeilingBlocks)
        {
            if (testZ >= sec.CeilingPortalZ)
            {
                pos = ctx.PosRelativeCeiling?.Invoke(pos) ?? pos;
                sec = ctx.PointInSector?.Invoke(pos) ?? sec;
                testZ = pos.Z + byOffset;
                moved = true;
            }
            else break;
        }

        if (!moved)
        {
            while (!sec.FloorBlocks)
            {
                if (testZ < sec.FloorPortalZ)
                {
                    pos = ctx.PosRelativeFloor?.Invoke(pos) ?? pos;
                    sec = ctx.PointInSector?.Invoke(pos) ?? sec;
                    testZ = pos.Z + byOffset;
                    moved = true;
                }
                else break;
            }
        }

        return (moved, pos, sec);
    }

    public static void CheckPortalTransition(PortalLevelContext ctx, PortalActor actor, bool isLinked)
    {
        var moved = false;
        while (!actor.Sector.CeilingBlocks)
        {
            if (actor.Z >= actor.Sector.CeilingPortalZ)
            {
                var oldPos = actor.Pos;
                if (isLinked && !moved)
                    ctx.UnlinkFromWorld?.Invoke();
                actor.Pos = ctx.PosRelativeCeiling?.Invoke(actor.Pos) ?? actor.Pos;
                actor.Z = actor.Pos.Z;
                actor.Prev = (
                    actor.Prev.X + (actor.Pos.X - oldPos.X),
                    actor.Prev.Y + (actor.Pos.Y - oldPos.Y),
                    actor.Prev.Z + (actor.Pos.Z - oldPos.Z));
                actor.Sector = ctx.PointInSector?.Invoke(actor.Pos) ?? actor.Sector;
                actor.PrevPortalGroup = actor.Sector.GetHashCode();
                moved = true;
            }
            else break;
        }

        if (!moved)
        {
            while (!actor.Sector.FloorBlocks)
            {
                var portalZ = actor.Sector.FloorPortalZ;
                if (actor.Z < portalZ && actor.FloorZ < portalZ)
                {
                    var oldPos = actor.Pos;
                    if (isLinked && !moved)
                        ctx.UnlinkFromWorld?.Invoke();
                    actor.Pos = ctx.PosRelativeFloor?.Invoke(actor.Pos) ?? actor.Pos;
                    actor.Z = actor.Pos.Z;
                    actor.Prev = (
                        actor.Prev.X + (actor.Pos.X - oldPos.X),
                        actor.Prev.Y + (actor.Pos.Y - oldPos.Y),
                        actor.Prev.Z + (actor.Pos.Z - oldPos.Z));
                    actor.Sector = ctx.PointInSector?.Invoke(actor.Pos) ?? actor.Sector;
                    actor.PrevPortalGroup = actor.Sector.GetHashCode();
                    moved = true;
                }
                else break;
            }
        }

        if (isLinked && moved)
            ctx.LinkToWorld?.Invoke();
    }

    public static int GetModelTimer(bool clientSide, int localWorldTimer, int totalTime) =>
        clientSide ? localWorldTimer : totalTime;

    public sealed class ModelBoneSlot
    {
        public List<BoneTrs> Bones { get; } = new();
        public List<BoneTrs> BonesWithOverride { get; } = new();
        public List<double[]> Positions { get; } = new();
        public List<double[]> PositionsWithOverride { get; } = new();
    }

    public sealed class BoneTrs
    {
        public double Tx { get; set; }
        public double Ty { get; set; }
        public double Tz { get; set; }
    }

    public sealed class ModelData
    {
        public int Flags { get; set; }
        public List<ModelBoneSlot> ModelBoneInfo { get; } = new();
    }

    public sealed class CalcBonesContext
    {
        public bool ClientSide { get; init; }
        public bool PicnumValid { get; init; }
        public bool FoundModelFrame { get; init; }
        public int SmfFlags { get; init; }
        public int Flags9 { get; init; }
        public int LocalWorldTimer { get; init; }
        public int TotalTime { get; init; }
        public int ModelsAmount { get; init; }
        public Func<int, bool>? CalcModelOverrides { get; init; }
        public Action<int, bool, double>? ProcessModelFrame { get; init; }
    }

    public static bool ShouldCalcBones(ModelData? modelData, bool recalc) =>
        modelData != null
        && (!recalc || (modelData.Flags & ModeldataGetBoneInfoRecalc) != 0)
        && (modelData.Flags & ModeldataGetBoneInfo) != 0;

    public static void CalcBones(CalcBonesContext ctx, ModelData modelData, bool recalc)
    {
        if (!ShouldCalcBones(modelData, recalc))
            return;
        if (ctx.PicnumValid || !ctx.FoundModelFrame)
            return;

        var isDecoupled = (ctx.Flags9 & Mf9Decoupledanimations) != 0;
        var tic = GetModelTimer(ctx.ClientSide, ctx.LocalWorldTimer, ctx.TotalTime) + 1.0;
        var evaluatedSingle = false;

        while (modelData.ModelBoneInfo.Count < ctx.ModelsAmount)
            modelData.ModelBoneInfo.Add(new ModelBoneSlot());

        for (var i = 0u; i < ctx.ModelsAmount; i++)
        {
            if (ctx.CalcModelOverrides?.Invoke((int)i) != true)
                continue;

            if (!evaluatedSingle)
            {
                var nextFrame = false;
                ctx.ProcessModelFrame?.Invoke((int)i, nextFrame, tic);
                if ((ctx.SmfFlags & MdlModelsareattachments) != 0 || isDecoupled)
                {
                    evaluatedSingle = true;
                    break;
                }
            }
        }
    }

    public static BoneTrs GetBoneTrs(ModelData? modelData, int modelIndex, int boneIndex, bool withOverride)
    {
        if (modelData == null || (modelData.Flags & ModeldataGetBoneInfo) == 0)
            return new BoneTrs();
        if (modelIndex < 0 || boneIndex < 0)
            return new BoneTrs();
        if (modelData.ModelBoneInfo.Count <= modelIndex)
            return new BoneTrs();
        var slot = modelData.ModelBoneInfo[modelIndex];
        var list = withOverride ? slot.BonesWithOverride : slot.Bones;
        if (list.Count <= boneIndex)
            return new BoneTrs();
        return list[boneIndex];
    }

    public static void GetBoneMatrix(
        ModelData? modelData,
        int modelIndex,
        int boneIndex,
        bool withOverride,
        Span<double> outMat)
    {
        if (outMat.Length < 16)
            return;
        if (modelData == null || (modelData.Flags & ModeldataGetBoneInfo) == 0)
            return;
        if (modelIndex < 0 || boneIndex < 0)
            return;
        if (modelData.ModelBoneInfo.Count <= modelIndex)
            return;
        var slot = modelData.ModelBoneInfo[modelIndex];
        var list = withOverride ? slot.PositionsWithOverride : slot.Positions;
        if (list.Count <= boneIndex)
            return;
        var src = list[boneIndex];
        for (var i = 0; i < 16 && i < src.Length; i++)
            outMat[i] = src[i];
    }

    public static (double Yaw, double Pitch, double Roll) GetBoneEulerAngles(
        ModelData? modelData,
        bool picnumValid,
        int modelIndex,
        int boneIndex,
        bool withOverride,
        Func<double[], (double X, double Y, double Z)>? transformForwardUp)
    {
        if (modelData == null || (modelData.Flags & ModeldataGetBoneInfo) == 0)
            return (0, 0, 0);
        if (modelIndex < 0 || boneIndex < 0)
            return (0, 0, 0);
        if (picnumValid)
            return (0, 0, 0);
        if (modelData.ModelBoneInfo.Count <= modelIndex)
            return (0, 0, 0);
        var slot = modelData.ModelBoneInfo[modelIndex];
        var list = withOverride ? slot.PositionsWithOverride : slot.Positions;
        if (list.Count <= boneIndex)
            return (0, 0, 0);

        if (transformForwardUp == null)
            return (0, 0, 0);

        var (fwdX, fwdY, fwdZ) = transformForwardUp(list[boneIndex]);
        var yaw = Math.Atan2(fwdZ, fwdX) * (180.0 / Math.PI);
        var pitch = -fwdY;
        const double yComponent = 1.0;
        if (Math.Abs(fwdY - yComponent) < 1e-6 && Math.Abs(fwdX) < 1e-6 && Math.Abs(fwdZ) < 1e-6)
            return (0, pitch, -fwdX);

        return (yaw, pitch, 0);
    }
}
