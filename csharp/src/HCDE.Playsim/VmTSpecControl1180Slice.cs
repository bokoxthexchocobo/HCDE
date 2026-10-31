namespace HCDE.Playsim;

/// <summary>
/// FraggleScript brace, if, elseif, else, while, and for from <c>t_spec.cpp</c> (<b>1–230</b>). Chunk <b>1180</b>.
/// <c>CreateVariable</c> at 231 stays outside this band.
/// </summary>
public static class VmTSpecControl1180Slice
{
    public const int StEmpty = 0;
    public const int StIf = 1;
    public const int StElseIf = 2;
    public const int StElse = 3;
    public const int StLoop = 4;
    public const int BracketOpen = 0;
    public const int BracketClose = 1;

    public sealed class Section
    {
        public int Type;
    }

    public sealed class Frame
    {
        public int BraceType;
        public Section? Section;
        public Section? PrevSection;
        public int NumTokens;
        public int Rover;
        public List<string> Errors { get; } = new();
        public List<(int Start, int Stop)> Spans { get; } = new();
        public Func<int, int, string, int>? FindOperator;
        public Action<VmTOperLogic1179Slice.Value, int, int>? Evaluate;
        public Func<Section, int>? SectionEnd;
        public Func<Section, int>? SectionLoop;
    }

    public static void SpecBrace(Frame frame)
    {
        if (frame.BraceType != BracketClose)
        {
            return;
        }

        var section = frame.Section;
        if (section == null)
        {
            throw new InvalidOperationException("spec_brace requires a section.");
        }

        if (section.Type == StIf || section.Type == StElse)
        {
            return;
        }

        if (section.Type == StLoop)
        {
            frame.Rover = LoopStart(frame, section);
        }
    }

    public static bool SpecIf(Frame frame, VmTOperLogic1179Slice.Value eval)
    {
        var endToken = Find(frame, 0, frame.NumTokens - 1, ")");
        if (endToken == -1)
        {
            frame.Errors.Add("parse error in if statement\n");
            return false;
        }

        Eval(frame, eval, 2, endToken - 1);
        var ifResult = VmTOperLogic1179Slice.IntValue(eval) != 0;
        if (frame.Section != null && frame.BraceType == BracketOpen && endToken == frame.NumTokens - 1)
        {
            if (ifResult)
            {
                return ifResult;
            }

            frame.Rover = End(frame, frame.Section) + 1;
        }
        else if (ifResult && endToken != frame.NumTokens - 1)
        {
            Eval(frame, eval, endToken + 1, frame.NumTokens - 1);
        }

        return ifResult;
    }

    public static bool SpecElseIf(Frame frame, VmTOperLogic1179Slice.Value eval, bool lastIf)
    {
        var endToken = Find(frame, 0, frame.NumTokens - 1, ")");
        if (endToken == -1)
        {
            frame.Errors.Add("parse error in elseif statement\n");
            return false;
        }

        if (lastIf)
        {
            var section = frame.Section;
            if (section == null)
            {
                throw new InvalidOperationException("spec_elseif requires a section when the previous if matched.");
            }

            frame.Rover = End(frame, section) + 1;
            return true;
        }

        Eval(frame, eval, 2, endToken - 1);
        var ifResult = VmTOperLogic1179Slice.IntValue(eval) != 0;
        if (frame.Section != null && frame.BraceType == BracketOpen && endToken == frame.NumTokens - 1)
        {
            if (!ifResult)
            {
                frame.Rover = End(frame, frame.Section) + 1;
            }
        }
        else if (ifResult && endToken != frame.NumTokens - 1)
        {
            Eval(frame, eval, endToken + 1, frame.NumTokens - 1);
        }

        return ifResult;
    }

    public static void SpecElse(Frame frame, bool lastIf)
    {
        if (!lastIf)
        {
            return;
        }

        var section = frame.Section;
        if (section == null)
        {
            throw new InvalidOperationException("spec_else requires a section when the previous if matched.");
        }

        frame.Rover = End(frame, section) + 1;
    }

    public static void SpecWhile(Frame frame, VmTOperLogic1179Slice.Value eval)
    {
        if (frame.Section == null)
        {
            frame.Errors.Add("no {} section given for loop\n");
            return;
        }

        var endToken = Find(frame, 0, frame.NumTokens - 1, ")");
        if (endToken == -1)
        {
            frame.Errors.Add("parse error in loop statement\n");
            return;
        }

        Eval(frame, eval, 2, endToken - 1);
        if (VmTOperLogic1179Slice.IntValue(eval) == 0)
        {
            frame.Rover = End(frame, frame.Section) + 1;
        }
    }

    public static void SpecFor(Frame frame, VmTOperLogic1179Slice.Value eval)
    {
        if (frame.Section == null)
        {
            frame.Errors.Add("need {} delimiters for for()\n");
            return;
        }

        var start = 2;
        var comma1 = Find(frame, start, frame.NumTokens - 1, ",");
        var comma2 = -1;
        if (comma1 != -1)
        {
            comma2 = Find(frame, comma1 + 1, frame.NumTokens - 1, ",");
        }

        if (comma1 == -1 || comma2 == -1)
        {
            frame.Errors.Add("incorrect arguments to for()\n");
            return;
        }

        if (ReferenceEquals(frame.Section, frame.PrevSection))
        {
            Eval(frame, eval, comma2 + 1, frame.NumTokens - 2);
            Eval(frame, eval, comma1 + 1, comma2 - 1);
            if (VmTOperLogic1179Slice.IntValue(eval) == 0)
            {
                frame.Rover = End(frame, frame.Section) + 1;
            }
        }
        else
        {
            Eval(frame, eval, start, comma1 - 1);
        }
    }

    static int Find(Frame frame, int start, int stop, string symbol)
    {
        if (frame.FindOperator == null)
        {
            return -1;
        }

        return frame.FindOperator(start, stop, symbol);
    }

    static void Eval(Frame frame, VmTOperLogic1179Slice.Value eval, int start, int stop)
    {
        frame.Spans.Add((start, stop));
        frame.Evaluate?.Invoke(eval, start, stop);
    }

    static int End(Frame frame, Section section)
    {
        if (frame.SectionEnd == null)
        {
            throw new InvalidOperationException("SectionEnd is required.");
        }

        return frame.SectionEnd(section);
    }

    static int LoopStart(Frame frame, Section section)
    {
        if (frame.SectionLoop == null)
        {
            throw new InvalidOperationException("SectionLoop is required.");
        }

        return frame.SectionLoop(section);
    }
}
