namespace HCDE.Net.Core;

/// <summary>Whole <c>HCDEAppendLegacyRunArgs</c> / <c>HCDEAppendLegacyEventPayload</c> from <c>d_net.cpp</c> (4111–4299).</summary>
public static class VmDNetLegacyEventPayloadAppend
{
    public sealed class LegacyPayloadSink
    {
        public ReadOnlyMemory<byte> Data { get; set; }
        public int InputCursor { get; set; }
        public List<byte> Output { get; } = new();

        public bool ReadByte(out byte value)
        {
            value = 0;
            if (InputCursor >= Data.Length)
                return false;
            value = Data.Span[InputCursor++];
            return true;
        }

        public bool ReadBE16(out ushort value)
        {
            value = 0;
            if (InputCursor + 2 > Data.Length)
                return false;
            value = (ushort)((Data.Span[InputCursor] << 8) | Data.Span[InputCursor + 1]);
            InputCursor += 2;
            return true;
        }

        public bool AppendByte(byte value)
        {
            Output.Add(value);
            return true;
        }

        public bool AppendBE16(ushort value)
        {
            Output.Add((byte)(value >> 8));
            Output.Add((byte)value);
            return true;
        }

        public bool AppendFieldBytes(uint count)
        {
            if (InputCursor + count > Data.Length)
                return false;
            Output.AddRange(Data.Span.Slice(InputCursor, (int)count).ToArray());
            InputCursor += (int)count;
            return true;
        }

        public Func<bool>? AppendLegacyStringFromCanonical { get; set; }
        public Func<int, bool>? AppendLegacyCVarChange { get; set; }
        public Func<bool, bool>? AppendLegacyRunArgs { get; set; }
        public Func<bool>? AppendLegacyWeaponIndex { get; set; }
    }

    public static bool AppendLegacyRunArgsNative(LegacyPayloadSink sink, bool named)
    {
        if (named)
        {
            if (sink.AppendLegacyStringFromCanonical is null
                || !sink.AppendLegacyStringFromCanonical()
                || !sink.ReadByte(out var argCount)
                || !sink.AppendByte(argCount))
            {
                return false;
            }

            argCount &= 127;
            return sink.AppendFieldBytes((uint)argCount * 4u);
        }

        if (!sink.AppendFieldBytes(2u)
            || !sink.ReadByte(out var argCount2)
            || !sink.AppendByte(argCount2))
        {
            return false;
        }

        return sink.AppendFieldBytes((uint)argCount2 * 4u);
    }

    public static bool AppendLegacyEventPayloadNative(int eventType, LegacyPayloadSink sink)
    {
        sink.InputCursor = 0;
        var ok = false;

        switch (eventType)
        {
            case VmDNetLegacyEventPayload.DemSuicide:
            case VmDNetLegacyEventPayload.DemKillbots:
            case VmDNetLegacyEventPayload.DemInvuseall:
            case VmDNetLegacyEventPayload.DemPause:
            case VmDNetLegacyEventPayload.DemCenterView:
            case VmDNetLegacyEventPayload.DemCrouch:
            case VmDNetLegacyEventPayload.DemCheckautosave:
            case VmDNetLegacyEventPayload.DemDoautosave:
            case VmDNetLegacyEventPayload.DemConvclose:
            case VmDNetLegacyEventPayload.DemConvnull:
            case VmDNetLegacyEventPayload.DemRevertcamera:
            case VmDNetLegacyEventPayload.DemFinishgame:
            case VmDNetLegacyEventPayload.DemEndscreenjob:
            case VmDNetLegacyEventPayload.DemReadied:
            case VmDNetLegacyEventPayload.DemUseflechette:
                ok = true;
                break;

            case (int)DemoCommand.Say:
                ok = sink.AppendFieldBytes(1u)
                     && sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical();
                break;

            case (int)DemoCommand.AddBot:
                ok = sink.AppendFieldBytes(1u)
                     && sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendFieldBytes(4u);
                break;

            case (int)DemoCommand.GiveCheat:
            case (int)DemoCommand.TakeCheat:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendFieldBytes(4u);
                break;

            case (int)DemoCommand.SetInv:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendFieldBytes(5u);
                break;

            case (int)DemoCommand.NetEvent:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendFieldBytes(14u);
                break;

            case (int)DemoCommand.ZscCmd:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.ReadBE16(out var commandBytes)
                     && sink.AppendBE16(commandBytes)
                     && sink.AppendFieldBytes(commandBytes);
                break;

            case (int)DemoCommand.Summon2:
            case (int)DemoCommand.SummonFriend2:
            case (int)DemoCommand.SummonFoe2:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendFieldBytes(25u);
                break;

            case (int)DemoCommand.ChangeMap2:
                ok = sink.AppendFieldBytes(1u)
                     && sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical();
                break;

            case (int)DemoCommand.MusicChange:
            case (int)DemoCommand.Print:
            case (int)DemoCommand.CenterPrint:
            case (int)DemoCommand.UinfChanged:
            case (int)DemoCommand.ChangeMap:
            case (int)DemoCommand.Summon:
            case (int)DemoCommand.SummonFriend:
            case (int)DemoCommand.SummonFoe:
            case (int)DemoCommand.SummonMbf:
            case (int)DemoCommand.Remove:
            case (int)DemoCommand.Spray:
            case (int)DemoCommand.MorphEx:
            case (int)DemoCommand.KillClassCheat:
            case (int)DemoCommand.Mdk:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical();
                break;

            case (int)DemoCommand.WarpCheat:
                ok = sink.AppendFieldBytes(6u);
                break;

            case (int)DemoCommand.InvUse:
            case (int)DemoCommand.Fov:
            case (int)DemoCommand.MyFov:
            case (int)DemoCommand.ChangeSkill:
                ok = sink.AppendFieldBytes(4u);
                break;

            case (int)DemoCommand.InvDrop:
                ok = sink.AppendFieldBytes(8u);
                break;

            case (int)DemoCommand.GenericCheat:
            case (int)DemoCommand.AddController:
            case (int)DemoCommand.DelController:
            case (int)DemoCommand.Kick:
            case (int)DemoCommand.WeapSelect:
                ok = sink.AppendFieldBytes(1u);
                break;

            case (int)DemoCommand.SaveGame:
                ok = sink.AppendLegacyStringFromCanonical is not null
                     && sink.AppendLegacyStringFromCanonical()
                     && sink.AppendLegacyStringFromCanonical();
                break;

            case VmDNetLegacyEventPayload.DemSinfchanged:
            case VmDNetLegacyEventPayload.DemSinfchangedxor:
                ok = sink.AppendLegacyCVarChange is not null
                     && sink.AppendLegacyCVarChange(eventType);
                break;

            case VmDNetLegacyEventPayload.DemRunscript:
            case VmDNetLegacyEventPayload.DemRunscript2:
            case VmDNetLegacyEventPayload.DemRunspecial:
                ok = sink.AppendLegacyRunArgs is not null
                     && sink.AppendLegacyRunArgs(false);
                break;

            case VmDNetLegacyEventPayload.DemRunnamedscript:
                ok = sink.AppendLegacyRunArgs is not null
                     && sink.AppendLegacyRunArgs(true);
                break;

            case (int)DemoCommand.ConvReply:
                ok = sink.AppendFieldBytes(3u);
                break;

            case VmDNetLegacyEventPayload.DemSetslot:
            case VmDNetLegacyEventPayload.DemSetslotpnum:
                if (eventType == VmDNetLegacyEventPayload.DemSetslotpnum
                    && !sink.AppendFieldBytes(1u))
                {
                    return false;
                }

                if (!sink.ReadByte(out var slot)
                    || !sink.ReadByte(out var count)
                    || !sink.AppendByte(slot)
                    || !sink.AppendByte(count))
                {
                    return false;
                }

                for (var i = 0; i < count; i++)
                {
                    if (sink.AppendLegacyWeaponIndex is null || !sink.AppendLegacyWeaponIndex())
                        return false;
                }

                ok = true;
                break;

            default:
                return false;
        }

        return ok;
    }
}
