namespace HCDE.Playsim;

/// <summary><c>ChangePortalLine</c> and <c>ChangePortal</c> from <c>portal.cpp</c> (330–383).</summary>
public static class VmPortalChangePortal
{
    public static bool ChangePortalLine(
        VmPortalUpdateFinalize.LinePortalState port,
        int destLineIndex,
        Func<int, VmPortalUpdateFinalize.LinePortalState?>? destPortalForLine,
        Action<VmPortalUpdateFinalize.LinePortalState?>? setRotationOnDest = null)
    {
        if (port.Type == VmPortalUpdateFinalize.PorttLinked)
            return false;
        port.DestinationLineIndex = destLineIndex == 0 ? null : destLineIndex;
        if (port.DestinationLineIndex is null)
        {
            port.Flags = 0;
        }
        else if (port.Type == VmPortalUpdateFinalize.PorttInteractive)
        {
            var portd = destPortalForLine?.Invoke(destLineIndex);
            if (portd is not null && portd.Type == VmPortalUpdateFinalize.PorttInteractive &&
                portd.DestinationLineIndex == port.DestinationLineIndex)
            {
                port.Flags = port.DefFlags | VmPortalUpdateFinalize.PortfInteractive;
                portd.Flags = portd.DefFlags | VmPortalUpdateFinalize.PortfInteractive;
            }
            else
                port.Flags = port.DefFlags;
            setRotationOnDest?.Invoke(portd);
        }
        else
            port.Flags = port.DefFlags;

        VmPortalBuildBlockmap.SetPortalRotation(port.Type, port.DestinationLineIndex is not null, 0, 0, false, false, port.Rotation);
        return true;
    }

    public static bool ChangePortal(
        int thisId,
        int sourceLineIndex,
        VmPTagManager.LineIdIterator lineIdIterator,
        Func<int, bool> changeLine)
    {
        if (thisId == 0)
            return changeLine(sourceLineIndex);
        var res = false;
        int lineno;
        while ((lineno = lineIdIterator.Next()) >= 0)
            res |= changeLine(lineno);
        return res;
    }
}
