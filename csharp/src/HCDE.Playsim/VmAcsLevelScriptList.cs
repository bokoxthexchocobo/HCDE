namespace HCDE.Playsim;

/// <summary><c>DLevelScript</c> controller and list ops from <c>p_acs.cpp</c> (3584–3672).</summary>
public static class VmAcsLevelScriptList
{
    public sealed class AcsScriptListController
    {
        public AcsScriptNode? Scripts;
        public AcsScriptNode? LastScript;
    }

    public sealed class AcsScriptNode
    {
        public AcsScriptNode? Prev;
        public AcsScriptNode? Next;
    }

    public static AcsScriptNode? GetControllerNative(bool clientSide, AcsScriptNode? clientSideThinker, AcsScriptNode? acsThinker) =>
        clientSide ? clientSideThinker : acsThinker;

    public static void UnlinkNative(
        AcsScriptListController controller,
        AcsScriptNode self,
        AcsScriptNode? prev,
        AcsScriptNode? next)
    {
        if (ReferenceEquals(controller.LastScript, self))
            controller.LastScript = prev;
        if (ReferenceEquals(controller.Scripts, self))
            controller.Scripts = next;
        if (prev is not null)
            prev.Next = next;
        if (next is not null)
            next.Prev = prev;
    }

    public static void LinkNative(AcsScriptListController controller, AcsScriptNode self)
    {
        self.Next = controller.Scripts;
        if (controller.Scripts is not null)
            controller.Scripts.Prev = self;
        self.Prev = null;
        controller.Scripts = self;
        if (controller.LastScript is null)
            controller.LastScript = self;
    }

    public static void PutLastNative(AcsScriptListController controller, AcsScriptNode self)
    {
        if (ReferenceEquals(controller.LastScript, self))
            return;

        UnlinkNative(controller, self, self.Prev, self.Next);
        if (controller.Scripts is null)
        {
            LinkNative(controller, self);
            return;
        }

        if (controller.LastScript is not null)
            controller.LastScript.Next = self;
        self.Prev = controller.LastScript;
        self.Next = null;
        controller.LastScript = self;
    }

    public static void PutFirstNative(AcsScriptListController controller, AcsScriptNode self)
    {
        if (ReferenceEquals(controller.Scripts, self))
            return;

        UnlinkNative(controller, self, self.Prev, self.Next);
        LinkNative(controller, self);
    }
}
