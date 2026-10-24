namespace HCDE.Scripting;

/// <summary><c>FxBitOp::Emit</c> from <c>codegen.cpp</c> (4546–4572).</summary>
public static class ZccBitOpEmit
{
    public const int OpAndRr = 0;
    public const int OpOrRr = 1;
    public const int OpXorRr = 2;

    public static int BaseInstruction(char op) =>
        op switch
        {
            '&' => OpAndRr,
            '|' => OpOrRr,
            '^' => OpXorRr,
            _ => -1,
        };

    public static int EmitOpcode(int baseInstr, bool rightKonst) =>
        baseInstr + (rightKonst ? 1 : 0);
}
