namespace HCDE.Client;

/// <summary>
/// <c>SBarInfoCommandFlowControl::NextCommand</c> EOF tail from <c>sbarinfo_commands.cpp</c> (**3601–3614**).
/// </summary>
public static class VmSbarinfoCommandFlowControlNextCommand1097Slice
{
    public static bool IsNextCommandDispatchCaseWithinEofBand(
        VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind kind) =>
        kind >= VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.Alpha
        && kind <= VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfCVarInt;

    public static bool TryResolveNextCommandTailFactoryKind(
        VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind kind,
        out NextCommandTailFactoryKind factoryKind)
    {
        factoryKind = kind switch
        {
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.Alpha
                => NextCommandTailFactoryKind.CommandAlpha,
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfHealth
                => NextCommandTailFactoryKind.CommandIfHealth,
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfInvulnerable
                => NextCommandTailFactoryKind.CommandIfInvulnerable,
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfWaterLevel
                => NextCommandTailFactoryKind.CommandIfWaterLevel,
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfCVarInt
                => NextCommandTailFactoryKind.CommandIfCVarInt,
            _ => NextCommandTailFactoryKind.None
        };
        return factoryKind != NextCommandTailFactoryKind.None;
    }

    public enum NextCommandTailFactoryKind
    {
        None,
        CommandAlpha,
        CommandIfHealth,
        CommandIfInvulnerable,
        CommandIfWaterLevel,
        CommandIfCVarInt
    }

    public static (bool ReportScriptError, bool ReturnNull) ResolveNextCommandUnknownIdentifierOutcome(
        bool switchMatchedKnownCase) =>
        switchMatchedKnownCase
            ? (ReportScriptError: false, ReturnNull: false)
            : (ReportScriptError: true, ReturnNull: true);

    public static (bool MustGetClosingBrace, bool ReturnNull) ResolveNextCommandWhenIdentifierAbsent() =>
        (MustGetClosingBrace: true, ReturnNull: true);
}
