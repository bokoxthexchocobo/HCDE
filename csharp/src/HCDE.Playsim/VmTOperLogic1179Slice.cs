using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// FraggleScript operator table and the handlers through <c>OPmultiply</c> from <c>t_oper.cpp</c> (<b>1–356</b>). Chunk <b>1179</b>.
/// Divide and the later handlers stay outside this band.
/// </summary>
public static class VmTOperLogic1179Slice
{
    public const int SvtString = 0;
    public const int SvtInt = 1;
    public const int SvtMobj = 2;
    public const int SvtFixed = 6;
    public const int Forward = 0;
    public const int Backward = 1;
    public const int NumOperators = 21;

    public sealed class Value
    {
        public int Type = SvtInt;
        public string Text = "";
        public int Int;
        public int Fixed;
        public object? Mobj;

        public void SetDouble(double amount)
        {
            Fixed = (int)(amount * 65536d);
            Type = SvtFixed;
        }
    }

    public sealed class Variable
    {
        public string Name = "";
        public Value? Assigned;
    }

    public sealed class OperatorRow
    {
        public string Symbol = "";
        public string Handler = "";
        public int Direction;
        public bool BodyInThisBand;
    }

    public sealed class Frame
    {
        public string[] Tokens = [];
        public object? GlobalScript;
        public List<string> Errors { get; } = new();
        public List<(int Start, int Stop)> Spans { get; } = new();
        public Func<string, object?, Variable?>? FindVariable;
        public Action<Variable, Value>? SetValue;
        public Action<Value, int, int>? Evaluate;
    }

    public static readonly OperatorRow[] Operators =
    [
        Row("=", "OPequals", Backward, true),
        Row("||", "OPor", Forward, true),
        Row("&&", "OPand", Forward, true),
        Row("|", "OPor_bin", Forward, false),
        Row("&", "OPand_bin", Forward, false),
        Row("==", "OPcmp", Forward, true),
        Row("!=", "OPnotcmp", Forward, true),
        Row("<", "OPlessthan", Forward, true),
        Row(">", "OPgreaterthan", Forward, true),
        Row("<=", "OPlessthanorequal", Forward, false),
        Row(">=", "OPgreaterthanorequal", Forward, false),
        Row("+", "OPplus", Forward, true),
        Row("-", "OPminus", Forward, true),
        Row("*", "OPmultiply", Forward, true),
        Row("/", "OPdivide", Forward, false),
        Row("%", "OPremainder", Forward, false),
        Row("~", "OPnot_bin", Forward, false),
        Row("!", "OPnot", Forward, true),
        Row("++", "OPincrement", Forward, false),
        Row("--", "OPdecrement", Forward, false),
        Row(".", "OPstructure", Forward, false),
    ];

    public static int IntValue(Value value)
    {
        if (value.Type == SvtString)
        {
            return Atoi(value.Text);
        }

        if (value.Type == SvtFixed)
        {
            return (int)(value.Fixed / 65536d);
        }

        if (value.Type == SvtMobj)
        {
            return -1;
        }

        return value.Int;
    }

    public static int FixedValue(Value value)
    {
        if (value.Type == SvtFixed)
        {
            return value.Fixed;
        }

        if (value.Type == SvtString)
        {
            return (int)(Atof(value.Text) * 65536d);
        }

        if (value.Type == SvtMobj)
        {
            return -65536;
        }

        return unchecked(value.Int * 65536);
    }

    public static double FloatValue(Value value)
    {
        if (value.Type == SvtString)
        {
            return Atof(value.Text);
        }

        if (value.Type == SvtFixed)
        {
            return value.Fixed / 65536d;
        }

        if (value.Type == SvtMobj)
        {
            return -1d;
        }

        return value.Int;
    }

    public static void EqualsOp(Frame frame, Value result, int start, int n, int stop)
    {
        Variable? variable = null;
        if (frame.FindVariable != null)
        {
            variable = frame.FindVariable(frame.Tokens[start], frame.GlobalScript);
        }

        if (variable != null)
        {
            Evaluate(frame, result, n + 1, stop);
            frame.SetValue?.Invoke(variable, result);
        }
        else
        {
            frame.Errors.Add("unknown variable '" + frame.Tokens[start] + "'\n");
        }
    }

    public static void OrOp(Frame frame, Value result, int start, int n, int stop)
    {
        Evaluate(frame, result, start, n - 1);
        var exprTrue = IntValue(result) != 0;
        if (!exprTrue)
        {
            Evaluate(frame, result, n + 1, stop);
            exprTrue = IntValue(result) != 0;
        }

        result.Type = SvtInt;
        result.Int = exprTrue ? 1 : 0;
    }

    public static void AndOp(Frame frame, Value result, int start, int n, int stop)
    {
        Evaluate(frame, result, start, n - 1);
        var exprTrue = IntValue(result) != 0;
        if (exprTrue)
        {
            Evaluate(frame, result, n + 1, stop);
            exprTrue = IntValue(result) != 0;
        }

        result.Type = SvtInt;
        result.Int = exprTrue ? 1 : 0;
    }

    public static void CmpOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        LeftAndRight(frame, left, right, start, n, stop);
        result.Type = SvtInt;
        if (left.Type == SvtString && right.Type == SvtString)
        {
            result.Int = left.Text == right.Text ? 1 : 0;
            return;
        }

        if (left.Type == SvtMobj && right.Type == SvtMobj)
        {
            result.Int = ReferenceEquals(left.Mobj, right.Mobj) ? 1 : 0;
            return;
        }

        if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            result.Int = FixedValue(left) == FixedValue(right) ? 1 : 0;
            return;
        }

        result.Int = IntValue(left) == IntValue(right) ? 1 : 0;
    }

    public static void NotCmpOp(Frame frame, Value result, int start, int n, int stop)
    {
        CmpOp(frame, result, start, n, stop);
        result.Type = SvtInt;
        if (result.Int == 0)
        {
            result.Int = 1;
        }
        else
        {
            result.Int = 0;
        }
    }

    public static void LessThanOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        LeftAndRight(frame, left, right, start, n, stop);
        result.Type = SvtInt;
        var less = false;
        if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            less = FixedValue(left) < FixedValue(right);
        }
        else
        {
            less = IntValue(left) < IntValue(right);
        }

        result.Int = less ? 1 : 0;
    }

    public static void GreaterThanOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        LeftAndRight(frame, left, right, start, n, stop);
        result.Type = SvtInt;
        var greater = false;
        if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            greater = FixedValue(left) > FixedValue(right);
        }
        else
        {
            greater = IntValue(left) > IntValue(right);
        }

        result.Int = greater ? 1 : 0;
    }

    public static void NotOp(Frame frame, Value result, int start, int n, int stop)
    {
        Evaluate(frame, result, n + 1, stop);
        var on = IntValue(result) != 0;
        result.Type = SvtInt;
        if (on)
        {
            result.Int = 0;
        }
        else
        {
            result.Int = 1;
        }
    }

    public static void PlusOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        LeftAndRight(frame, left, right, start, n, stop);
        if (left.Type == SvtString)
        {
            if (right.Type == SvtString)
            {
                result.Text = left.Text + right.Text;
            }
            else if (right.Type == SvtFixed)
            {
                result.Text = left.Text + FloatValue(right).ToString("0.0000", CultureInfo.InvariantCulture);
            }
            else
            {
                result.Text = left.Text + IntValue(right).ToString(CultureInfo.InvariantCulture);
            }

            result.Type = SvtString;
        }
        else if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            result.Type = SvtFixed;
            result.Fixed = unchecked(FixedValue(left) + FixedValue(right));
        }
        else
        {
            result.Type = SvtInt;
            result.Int = unchecked(IntValue(left) + IntValue(right));
        }
    }

    public static void MinusOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        if (start == n)
        {
            Evaluate(frame, right, n + 1, stop);
        }
        else
        {
            LeftAndRight(frame, left, right, start, n, stop);
        }

        if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            result.Type = SvtFixed;
            result.Fixed = unchecked(FixedValue(left) - FixedValue(right));
        }
        else
        {
            result.Type = SvtInt;
            result.Int = unchecked(IntValue(left) - IntValue(right));
        }
    }

    public static void MultiplyOp(Frame frame, Value result, int start, int n, int stop)
    {
        var left = new Value();
        var right = new Value();
        LeftAndRight(frame, left, right, start, n, stop);
        if (left.Type == SvtFixed || right.Type == SvtFixed)
        {
            result.SetDouble(FloatValue(left) * FloatValue(right));
        }
        else
        {
            result.Type = SvtInt;
            result.Int = unchecked(IntValue(left) * IntValue(right));
        }
    }

    static OperatorRow Row(string symbol, string handler, int direction, bool bodyInThisBand)
    {
        return new OperatorRow
        {
            Symbol = symbol,
            Handler = handler,
            Direction = direction,
            BodyInThisBand = bodyInThisBand,
        };
    }

    static void LeftAndRight(Frame frame, Value left, Value right, int start, int n, int stop)
    {
        Evaluate(frame, left, start, n - 1);
        Evaluate(frame, right, n + 1, stop);
    }

    static void Evaluate(Frame frame, Value into, int start, int stop)
    {
        frame.Spans.Add((start, stop));
        frame.Evaluate?.Invoke(into, start, stop);
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    public static int Atoi(string text)
    {
        var i = 0;
        while (i < text.Length && IsCSpace(text[i]))
        {
            i++;
        }

        var sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
            {
                sign = -1;
            }

            i++;
        }

        var value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = unchecked((value * 10) + (text[i] - '0'));
            i++;
        }

        return unchecked(sign * value);
    }

    public static double Atof(string text)
    {
        var i = 0;
        while (i < text.Length && IsCSpace(text[i]))
        {
            i++;
        }

        var start = i;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            i++;
        }

        var sawDigit = false;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            sawDigit = true;
            i++;
        }

        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            {
                sawDigit = true;
                i++;
            }
        }

        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            var exponent = i + 1;
            if (exponent < text.Length && (text[exponent] == '+' || text[exponent] == '-'))
            {
                exponent++;
            }

            var exponentDigits = exponent;
            while (exponent < text.Length && text[exponent] >= '0' && text[exponent] <= '9')
            {
                exponent++;
            }

            if (exponent > exponentDigits)
            {
                i = exponent;
            }
        }

        if (!sawDigit)
        {
            return 0d;
        }

        return double.Parse(text.Substring(start, i - start), CultureInfo.InvariantCulture);
    }
}
