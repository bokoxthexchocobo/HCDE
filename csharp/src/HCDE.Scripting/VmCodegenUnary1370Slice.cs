namespace HCDE.Scripting;

/// <summary>
/// <c>FxTypeCast::Emit</c>, <c>FxPlusSign</c>, <c>FxMinusSign</c>, and the
/// <c>FxUnaryNotBitwise</c> constructor from <c>codegen.cpp</c> 2017-2240.
/// <c>FxUnaryNotBitwise::Resolve</c> at 2241 stays chunk 56.
/// </summary>
public sealed class VmCodegenUnary1370Slice
{
    public const int RegNil = 128;
    public const int RegInt = 0;
    public const int RegFloat = 1;
    public const int FlopNeg = 1;

    public bool Asserts { get; set; } = true;
    public Func<Node, Node?>? ResolveChild { get; set; }
    public Func<Node, Emit>? EmitChild { get; set; }
    public Func<int, int>? FindConstantInt { get; set; }
    public Func<int, int>? GetConstantInt { get; set; }
    public Func<int, int, int>? Alloc { get; set; }
    public List<string> Errors { get; } = [];
    public List<string> Trace { get; } = [];
    public List<Op> Ops { get; } = [];

    public sealed class Node
    {
        public string Kind { get; init; } = "";
        public Node? Operand { get; set; }
        public bool Resolved { get; set; }
        public bool Deleted { get; set; }
        public bool Numeric { get; set; }
        public bool Vector { get; set; }
        public bool Quaternion { get; set; }
        public bool Constant { get; set; }
        public bool Bool { get; set; }
        public bool Nowarn { get; set; }
        public int RegType { get; set; }
        public int RegCount { get; set; } = 1;
        public int IntValue { get; set; }
        public double FloatValue { get; set; }
        public string? ValueType { get; set; }
    }

    public sealed class Emit
    {
        public int RegNum { get; set; }
        public int RegType { get; set; } = RegNil;
        public int RegCount { get; set; } = 1;
        public bool Konst { get; set; }
        public bool Fixed { get; set; }
        public bool Final { get; set; }
        public bool Target { get; set; }
    }

    public readonly record struct Op(string Name, int A, int B, int? C);

    public Node Create(string kind, Node? operand)
    {
        if (operand == null)
            throw new InvalidOperationException("operand");
        return new Node { Kind = kind, Operand = operand };
    }

    public void Destroy(Node node)
    {
        if (node.Operand != null)
        {
            node.Operand.Deleted = true;
            node.Operand = null;
        }

        node.Deleted = true;
    }

    public Emit TypeCastEmit()
    {
        Trace.Add("assert");
        if (Asserts)
            throw new InvalidOperationException("assert");
        return new Emit();
    }

    public Node? ResolvePlus(Node self)
    {
        if (self.Resolved)
            return self;
        self.Resolved = true;
        var operand = ResolveOne(self);
        if (operand == null)
            return null;
        if (operand.Numeric || operand.Vector || operand.Quaternion)
        {
            self.Operand = null;
            Destroy(self);
            return operand;
        }

        Errors.Add("Numeric type expected");
        Destroy(self);
        return null;
    }

    public Emit EmitPlus(Node self)
    {
        if (self.Operand == null)
            throw new InvalidOperationException("Operand");
        if (EmitChild == null)
            throw new InvalidOperationException("Emit");
        Trace.Add("Emit");
        return EmitChild(self.Operand);
    }

    public Node? ResolveMinus(Node self)
    {
        if (self.Resolved)
            return self;
        self.Resolved = true;
        var operand = ResolveOne(self);
        if (operand == null)
            return null;
        if (!operand.Numeric && !operand.Vector && !operand.Quaternion)
        {
            Errors.Add("Numeric type expected");
            Destroy(self);
            return null;
        }

        if (operand.Constant)
        {
            var folded = operand.RegType == RegInt
                ? new Node { Kind = "const", Constant = true, Numeric = true, RegType = RegInt, IntValue = -operand.IntValue }
                : new Node { Kind = "const", Constant = true, Numeric = true, RegType = RegFloat, FloatValue = -operand.FloatValue };
            Destroy(self);
            return folded;
        }

        if (operand.Bool)
        {
            var cast = new Node { Kind = "intcast", Operand = operand, Nowarn = true };
            if (ResolveChild == null)
                throw new InvalidOperationException("Resolve");
            var resolved = ResolveChild(cast);
            Trace.Add("Resolve");
            if (resolved == null)
                throw new InvalidOperationException("assert");
            self.Operand = resolved;
            self.ValueType = resolved.ValueType;
            self.RegType = resolved.RegType;
            self.RegCount = resolved.RegCount;
            return self;
        }

        self.ValueType = operand.ValueType;
        self.RegType = operand.RegType;
        self.RegCount = operand.RegCount;
        return self;
    }

    public Emit EmitMinus(Node self, Emit from)
    {
        if (from.Konst && from.RegType == RegInt)
        {
            if (FindConstantInt == null || GetConstantInt == null)
                throw new InvalidOperationException("GetConstantInt");
            int value = FindConstantInt(from.RegNum);
            Trace.Add("FindConstantInt");
            int reg = GetConstantInt(-value);
            Trace.Add("GetConstantInt");
            return new Emit { Konst = true, RegType = RegInt, RegNum = reg };
        }

        if (from.Konst || self.RegCount != from.RegCount)
            throw new InvalidOperationException("assert");
        var to = from;
        if (from.Fixed)
        {
            if (Alloc == null)
                throw new InvalidOperationException("Get");
            to = new Emit { RegType = from.RegType, RegCount = from.RegCount, RegNum = Alloc(from.RegType, from.RegCount) };
            Trace.Add("Free");
        }

        if (self.RegType == RegInt)
        {
            Ops.Add(new Op("NEG", to.RegNum, from.RegNum, 0));
            return to;
        }

        if (self.RegType != RegFloat)
            throw new InvalidOperationException("assert");
        var name = from.RegCount switch
        {
            1 => "FLOP",
            2 => "NEGV2",
            3 => "NEGV3",
            4 => "NEGV4",
            _ => "",
        };
        if (name == "FLOP")
            Ops.Add(new Op(name, to.RegNum, from.RegNum, FlopNeg));
        else if (name.Length > 0)
            Ops.Add(new Op(name, to.RegNum, from.RegNum, null));
        return to;
    }

    Node? ResolveOne(Node self)
    {
        if (self.Operand == null)
        {
            Destroy(self);
            return null;
        }

        if (ResolveChild == null)
            throw new InvalidOperationException("Resolve");
        var resolved = ResolveChild(self.Operand);
        Trace.Add("Resolve");
        self.Operand = resolved;
        if (resolved != null)
            return resolved;
        Destroy(self);
        return null;
    }
}
