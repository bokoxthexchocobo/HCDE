namespace HCDE.Playsim;

/// <summary>
/// Image-layout barriers and the mip chain from <c>vk_imagetransition.cpp</c> (<b>1–152</b>).
/// Chunk <b>1260</b>. A matching layout returns before either stage mask changes. Source and
/// destination masks accumulate across later images. An unknown destination still keeps the
/// source stage bit set just before the abort, and it does not record a barrier or change the
/// layout. Mip generation halves each side with a floor of one, and it writes the access masks
/// in the call rather than looking them up from the current layout.
/// </summary>
public sealed class VmImageTransition1260
{
    public const int LayoutUndefined = 0;
    public const int LayoutGeneral = 1;
    public const int LayoutColor = 2;
    public const int LayoutDepth = 3;
    public const int LayoutShaderRead = 5;
    public const int LayoutTransferSrc = 6;
    public const int LayoutTransferDst = 7;

    public const int AccessShaderRead = 0x20;
    public const int AccessColorRead = 0x80;
    public const int AccessColorWrite = 0x100;
    public const int AccessDepthRead = 0x200;
    public const int AccessDepthWrite = 0x400;
    public const int AccessTransferRead = 0x800;
    public const int AccessTransferWrite = 0x1000;

    public const int StageTop = 0x1;
    public const int StageFragment = 0x80;
    public const int StageEarly = 0x100;
    public const int StageLate = 0x200;
    public const int StageColor = 0x400;
    public const int StageTransfer = 0x1000;

    public const int AspectColor = 0x1;
    public const int FilterLinear = 1;

    public readonly List<string> Log = new();
    public int SrcStage;
    public int DstStage;
    public bool NeedBarrier;
    public string Fatal = "";
    int _barriers;

    public sealed class Texture
    {
        public int Id;
        public int Width;
        public int Height;
        public int Layout;
        public int Aspect = AspectColor;
    }

    public VmImageTransition1260 Add(Texture image, int target, bool undefinedSrc, int baseMip = 0, int levelCount = 1)
    {
        if (image.Layout == target)
        {
            Log.Add($"same {image.Id}");
            return this;
        }

        if (!SourceAccess(image.Layout, out int srcAccess))
        {
            Fatal = "Unimplemented src image layout transition";
            Log.Add("fatal src");
            return this;
        }

        if (!DestAccess(target, out int dstAccess))
        {
            Fatal = "Unimplemented dst image layout transition";
            Log.Add("fatal dst");
            return this;
        }

        int oldLayout = undefinedSrc ? LayoutUndefined : image.Layout;
        NoteBarrier(image.Id, oldLayout, target, srcAccess, dstAccess, image.Aspect, baseMip, levelCount);
        _barriers++;
        NeedBarrier = true;
        image.Layout = target;
        Log.Add($"layout {image.Id} {image.Layout}");
        return this;
    }

    public void Execute()
    {
        if (!NeedBarrier)
            return;

        Log.Add($"execute {SrcStage:X} {DstStage:X} {_barriers}");
    }

    public void GenerateMipmaps(Texture image)
    {
        int mipWidth = image.Width;
        int mipHeight = image.Height;
        int i = 1;
        while (mipWidth > 1 || mipHeight > 1)
        {
            NoteBarrier(image.Id, image.Layout, LayoutTransferSrc, AccessTransferWrite, AccessTransferRead, AspectColor, i - 1, 1);
            NoteBarrier(image.Id, LayoutUndefined, LayoutTransferDst, AccessTransferWrite, AccessTransferRead, AspectColor, i, 1);
            Log.Add($"execute {StageTransfer:X} {StageTransfer:X} 2");
            image.Layout = LayoutTransferDst;

            int nextWidth = Math.Max(mipWidth >> 1, 1);
            int nextHeight = Math.Max(mipHeight >> 1, 1);
            Log.Add(
                $"blit {image.Id} {LayoutTransferSrc} {LayoutTransferDst} 0 0 0 {mipWidth} {mipHeight} 1 {i - 1} 0 0 0 {nextWidth} {nextHeight} 1 {i} {AspectColor:X} 0 1 {FilterLinear}");
            NoteBarrier(image.Id, LayoutTransferSrc, LayoutShaderRead, AccessTransferRead, AccessShaderRead, AspectColor, i - 1, 1);
            Log.Add($"execute {StageTransfer:X} {StageFragment:X} 1");

            mipWidth = nextWidth;
            mipHeight = nextHeight;
            i++;
        }

        NoteBarrier(image.Id, LayoutTransferDst, LayoutShaderRead, AccessTransferRead, AccessShaderRead, AspectColor, i - 1, 1);
        Log.Add($"execute {StageTransfer:X} {StageFragment:X} 1");
        image.Layout = LayoutShaderRead;
    }

    bool SourceAccess(int layout, out int access)
    {
        switch (layout)
        {
            case LayoutUndefined:
                access = 0;
                SrcStage |= StageTop;
                return true;
            case LayoutTransferSrc:
                access = AccessTransferRead;
                SrcStage |= StageTransfer;
                return true;
            case LayoutShaderRead:
                access = AccessShaderRead;
                SrcStage |= StageFragment;
                return true;
            case LayoutTransferDst:
                access = AccessTransferWrite;
                SrcStage |= StageTransfer;
                return true;
            case LayoutColor:
                access = AccessColorRead | AccessColorWrite;
                SrcStage |= StageColor;
                return true;
            case LayoutDepth:
                access = AccessDepthRead | AccessDepthWrite;
                SrcStage |= StageEarly | StageLate;
                return true;
            default:
                access = 0;
                return false;
        }
    }

    bool DestAccess(int layout, out int access)
    {
        switch (layout)
        {
            case LayoutGeneral:
                access = 0;
                DstStage |= StageTop;
                return true;
            case LayoutTransferSrc:
                access = AccessTransferRead;
                DstStage |= StageTransfer;
                return true;
            case LayoutShaderRead:
                access = AccessShaderRead;
                DstStage |= StageFragment;
                return true;
            case LayoutTransferDst:
                access = AccessTransferWrite;
                DstStage |= StageTransfer;
                return true;
            case LayoutColor:
                access = AccessColorRead | AccessColorWrite;
                DstStage |= StageColor;
                return true;
            case LayoutDepth:
                access = AccessDepthRead | AccessDepthWrite;
                DstStage |= StageEarly | StageLate;
                return true;
            default:
                access = 0;
                return false;
        }
    }

    void NoteBarrier(int id, int oldLayout, int newLayout, int srcAccess, int dstAccess, int aspect, int baseMip, int levelCount)
    {
        Log.Add($"barrier {id} {oldLayout} {newLayout} {srcAccess:X} {dstAccess:X} {aspect:X} {baseMip} {levelCount} 0 1");
    }
}
