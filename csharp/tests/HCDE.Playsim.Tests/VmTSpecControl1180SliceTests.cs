namespace HCDE.Playsim.Tests;

public class VmTSpecControl1180SliceTests
{
    [Fact]
    public void BraceAndIf_SkipClosedLoopsAndBareStatements()
    {
        var section = new VmTSpecControl1180Slice.Section { Type = VmTSpecControl1180Slice.StIf };
        var frame = Frame(section, 4);
        frame.BraceType = VmTSpecControl1180Slice.BracketOpen;
        frame.Rover = 3;
        VmTSpecControl1180Slice.SpecBrace(frame);
        Assert.Equal(3, frame.Rover);

        frame.BraceType = VmTSpecControl1180Slice.BracketClose;
        VmTSpecControl1180Slice.SpecBrace(frame);
        Assert.Equal(3, frame.Rover);

        section.Type = VmTSpecControl1180Slice.StElse;
        VmTSpecControl1180Slice.SpecBrace(frame);
        Assert.Equal(3, frame.Rover);

        section.Type = VmTSpecControl1180Slice.StEmpty;
        VmTSpecControl1180Slice.SpecBrace(frame);
        Assert.Equal(3, frame.Rover);

        section.Type = VmTSpecControl1180Slice.StLoop;
        VmTSpecControl1180Slice.SpecBrace(frame);
        Assert.Equal(40, frame.Rover);

        var eval = new VmTOperLogic1179Slice.Value();
        frame = Frame(section, 4);
        frame.FindOperator = (_, _, _) => -1;
        Assert.False(VmTSpecControl1180Slice.SpecIf(frame, eval));
        Assert.Equal(new[] { "parse error in if statement\n" }, frame.Errors);
        Assert.Empty(frame.Spans);

        frame = Frame(section, 4);
        frame.BraceType = VmTSpecControl1180Slice.BracketOpen;
        frame.FindOperator = (_, _, symbol) => symbol == ")" ? 3 : -1;
        frame.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtString;
            into.Text = "0";
        };
        Assert.False(VmTSpecControl1180Slice.SpecIf(frame, eval));
        Assert.Equal(new[] { (2, 2) }, frame.Spans);
        Assert.Equal(11, frame.Rover);

        frame.Spans.Clear();
        frame.Rover = 0;
        frame.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = 5;
        };
        Assert.True(VmTSpecControl1180Slice.SpecIf(frame, eval));
        Assert.Equal(0, frame.Rover);
        Assert.Equal(new[] { (2, 2) }, frame.Spans);

        frame = Frame(null, 6);
        frame.FindOperator = (_, _, symbol) => symbol == ")" ? 3 : -1;
        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 2 ? 1 : 9;
        };
        Assert.True(VmTSpecControl1180Slice.SpecIf(frame, eval));
        Assert.Equal(new[] { (2, 2), (4, 5) }, frame.Spans);

        frame.Spans.Clear();
        frame.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = 0;
        };
        Assert.False(VmTSpecControl1180Slice.SpecIf(frame, eval));
        Assert.Equal(new[] { (2, 2) }, frame.Spans);
    }

    [Fact]
    public void ElseIfAndElse_SkipWhenThePreviousBranchMatched()
    {
        var section = new VmTSpecControl1180Slice.Section { Type = VmTSpecControl1180Slice.StElseIf };
        var eval = new VmTOperLogic1179Slice.Value();
        var frame = Frame(section, 4);
        frame.FindOperator = (_, _, _) => -1;
        Assert.False(VmTSpecControl1180Slice.SpecElseIf(frame, eval, false));
        Assert.Equal(new[] { "parse error in elseif statement\n" }, frame.Errors);

        frame = Frame(section, 4);
        frame.BraceType = VmTSpecControl1180Slice.BracketOpen;
        frame.FindOperator = (_, _, symbol) => symbol == ")" ? 3 : -1;
        frame.Rover = 1;
        Assert.True(VmTSpecControl1180Slice.SpecElseIf(frame, eval, true));
        Assert.Empty(frame.Spans);
        Assert.Equal(11, frame.Rover);

        frame.Spans.Clear();
        frame.Rover = 0;
        frame.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtFixed;
            into.Fixed = 0;
        };
        Assert.False(VmTSpecControl1180Slice.SpecElseIf(frame, eval, false));
        Assert.Equal(new[] { (2, 2) }, frame.Spans);
        Assert.Equal(11, frame.Rover);

        frame = Frame(null, 6);
        frame.FindOperator = (_, _, symbol) => symbol == ")" ? 2 : -1;
        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 2 ? 4 : 8;
        };
        Assert.True(VmTSpecControl1180Slice.SpecElseIf(frame, eval, false));
        Assert.Equal(new[] { (2, 1), (3, 5) }, frame.Spans);

        var elseFrame = Frame(section, 4);
        elseFrame.Rover = 2;
        VmTSpecControl1180Slice.SpecElse(elseFrame, false);
        Assert.Equal(2, elseFrame.Rover);
        VmTSpecControl1180Slice.SpecElse(elseFrame, true);
        Assert.Equal(11, elseFrame.Rover);
    }

    [Fact]
    public void Loops_RunTheInitOnceAndTheStepOnTheWayBack()
    {
        var section = new VmTSpecControl1180Slice.Section { Type = VmTSpecControl1180Slice.StLoop };
        var eval = new VmTOperLogic1179Slice.Value();
        var missing = Frame(null, 4);
        VmTSpecControl1180Slice.SpecWhile(missing, eval);
        Assert.Equal(new[] { "no {} section given for loop\n" }, missing.Errors);

        var broken = Frame(section, 4);
        broken.FindOperator = (_, _, _) => -1;
        VmTSpecControl1180Slice.SpecWhile(broken, eval);
        Assert.Equal(new[] { "parse error in loop statement\n" }, broken.Errors);
        Assert.Empty(broken.Spans);

        var running = Frame(section, 4);
        running.FindOperator = (_, _, symbol) => symbol == ")" ? 3 : -1;
        running.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = 2;
        };
        VmTSpecControl1180Slice.SpecWhile(running, eval);
        Assert.Equal(0, running.Rover);
        Assert.Equal(new[] { (2, 2) }, running.Spans);

        running.Spans.Clear();
        running.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = 0;
        };
        VmTSpecControl1180Slice.SpecWhile(running, eval);
        Assert.Equal(11, running.Rover);

        var noBraces = Frame(null, 13);
        VmTSpecControl1180Slice.SpecFor(noBraces, eval);
        Assert.Equal(new[] { "need {} delimiters for for()\n" }, noBraces.Errors);

        var bad = Frame(section, 13);
        var finds = 0;
        bad.FindOperator = (_, _, _) =>
        {
            finds++;
            return -1;
        };
        VmTSpecControl1180Slice.SpecFor(bad, eval);
        Assert.Equal(1, finds);
        Assert.Equal(new[] { "incorrect arguments to for()\n" }, bad.Errors);
        Assert.Empty(bad.Spans);

        var first = Frame(section, 13);
        first.PrevSection = new VmTSpecControl1180Slice.Section();
        first.FindOperator = (start, _, symbol) =>
        {
            if (symbol != ",")
            {
                return -1;
            }

            return start == 2 ? 5 : 9;
        };
        VmTSpecControl1180Slice.SpecFor(first, eval);
        Assert.Equal(new[] { (2, 4) }, first.Spans);
        Assert.Equal(0, first.Rover);

        var again = Frame(section, 13);
        again.PrevSection = section;
        again.FindOperator = first.FindOperator;
        again.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 6 ? 0 : 1;
        };
        VmTSpecControl1180Slice.SpecFor(again, eval);
        Assert.Equal(new[] { (10, 11), (6, 8) }, again.Spans);
        Assert.Equal(11, again.Rover);
    }

    static VmTSpecControl1180Slice.Frame Frame(VmTSpecControl1180Slice.Section? section, int numTokens)
    {
        return new VmTSpecControl1180Slice.Frame
        {
            Section = section,
            NumTokens = numTokens,
            SectionEnd = _ => 10,
            SectionLoop = _ => 40,
        };
    }
}
