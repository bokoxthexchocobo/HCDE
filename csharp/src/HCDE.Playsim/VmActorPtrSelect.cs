namespace HCDE.Playsim;

/// <summary><c>COPY_AAPTR</c> family and chain verification from <c>actorptrselect.cpp</c>.</summary>
public static class VmActorPtrSelect
{
    public const int AaptrDefault = 0;
    public const int AaptrNull = 0x1;
    public const int AaptrTarget = 0x2;
    public const int AaptrMaster = 0x4;
    public const int AaptrTracer = 0x8;
    public const int AaptrPlayerGettarget = 0x10;
    public const int AaptrPlayerGetconversation = 0x20;
    public const int AaptrPlayerSelectors = AaptrPlayerGettarget | AaptrPlayerGetconversation;
    public const int AaptrFriendplayer = 0x4000;
    public const int AaptrGetLinetarget = 0x8000;
    public const int AaptrGeneralSelectors = AaptrTarget | AaptrMaster | AaptrTracer | AaptrFriendplayer | AaptrGetLinetarget;
    public const int AaptrStaticSelectors = 0x40 | 0x80 | 0x100 | 0x200 | 0x400 | 0x800 | 0x1000 | 0x2000 | AaptrNull;
    public const int PtropUnsafetarget = 1;
    public const int PtropUnsafemaster = 2;
    public const int CsptrIgnore = 0;
    public const int CsptrClientside = 1;

    public static object? CopyAaptrex(
        object? origin,
        int selector,
        int clientSide,
        Func<int, object?> resolvePlayerInGame,
        Func<object, bool> originIsPlayer,
        Func<object, object?> bulletSlopeTarget,
        Func<object, object?> conversationNpc,
        Func<object, object?> getTarget,
        Func<object, object?> getMaster,
        Func<object, object?> getTracer,
        Func<object, int> friendPlayer,
        Func<object, bool> actorIsClientSide,
        int consolePlayer)
    {
        if (selector == AaptrDefault)
            return origin;

        var clientsideOnly = clientSide == CsptrClientside;

        object? ResolvePlayerNum(int playernum)
        {
            if (clientsideOnly && playernum != consolePlayer)
                return null;
            return resolvePlayerInGame(playernum);
        }

        if (origin != null)
        {
            if (originIsPlayer(origin))
            {
                switch (selector & AaptrPlayerSelectors)
                {
                    case AaptrPlayerGettarget:
                        if (clientsideOnly)
                            return null;
                        return bulletSlopeTarget(origin);
                    case AaptrPlayerGetconversation:
                        return clientsideOnly ? null : conversationNpc(origin);
                }
            }

            var checkRes = false;
            object? ret = null;
            switch (selector & AaptrGeneralSelectors)
            {
                case AaptrTarget:
                    checkRes = true;
                    ret = getTarget(origin);
                    break;
                case AaptrMaster:
                    checkRes = true;
                    ret = getMaster(origin);
                    break;
                case AaptrTracer:
                    checkRes = true;
                    ret = getTracer(origin);
                    break;
                case AaptrFriendplayer:
                    var fp = friendPlayer(origin);
                    return fp != 0 ? ResolvePlayerNum(fp - 1) : null;
                case AaptrGetLinetarget:
                    if (clientsideOnly)
                        return null;
                    return bulletSlopeTarget(origin);
            }

            if (checkRes)
            {
                if (ret != null && clientSide != CsptrIgnore)
                {
                    var isConsoleMo = false; // injected at seam: ret is console player's mo
                    if (!isConsoleMo && (clientsideOnly ^ actorIsClientSide(ret)))
                        ret = null;
                }

                return ret;
            }
        }

        var staticSel = selector & AaptrStaticSelectors;
        if (staticSel == AaptrNull)
            return null;
        if (staticSel == 0x40) return ResolvePlayerNum(0);
        if (staticSel == 0x80) return ResolvePlayerNum(1);
        if (staticSel == 0x100) return ResolvePlayerNum(2);
        if (staticSel == 0x200) return ResolvePlayerNum(3);
        if (staticSel == 0x400) return ResolvePlayerNum(4);
        if (staticSel == 0x800) return ResolvePlayerNum(5);
        if (staticSel == 0x1000) return ResolvePlayerNum(6);
        if (staticSel == 0x2000) return ResolvePlayerNum(7);

        return origin;
    }

    public static object? CopyAaptr(object? origin, int selector, Func<object?, int, int, object?> copyAaptrex) =>
        origin == null ? null : copyAaptrex(origin, selector, CsptrIgnore);

    public static void VerifyTargetChain(
        object? self,
        bool preciseMissileCheck,
        Func<object?, bool> isMissile,
        Func<object?, object?> getTarget,
        Action<object?> clearTarget)
    {
        if (self == null || !isMissile(self))
            return;
        var origin = self;
        var next = getTarget(origin);
        while (next != null && isMissile(next))
        {
            var compare = self;
            while (true)
            {
                if (ReferenceEquals(compare, next))
                {
                    clearTarget(self);
                    return;
                }

                if (ReferenceEquals(compare, origin))
                    break;
                compare = getTarget(compare);
            }

            origin = next;
            next = getTarget(next);
        }
    }

    public static void VerifyMasterChain(object? self, Func<object?, object?> getMaster, Action<object?> clearMaster)
    {
        if (self == null)
            return;
        var origin = self;
        var next = getMaster(origin);
        while (next != null)
        {
            var compare = self;
            while (true)
            {
                if (ReferenceEquals(compare, next))
                {
                    clearMaster(self);
                    return;
                }

                if (ReferenceEquals(compare, origin))
                    break;
                compare = getMaster(compare);
            }

            origin = next;
            next = getMaster(next);
        }
    }

    public static void AssignAaptr(
        object toActor,
        int toSlot,
        object? ptr,
        int flags,
        Action<object, object?> setTarget,
        Action<object, object?> setMaster,
        Action<object, object?> setTracer,
        Action<object, bool> verifyTargetChain,
        Action<object> verifyMasterChain)
    {
        switch (toSlot)
        {
            case AaptrTarget:
                setTarget(toActor, ptr);
                if ((flags & PtropUnsafetarget) == 0)
                    verifyTargetChain(toActor, false);
                break;
            case AaptrMaster:
                setMaster(toActor, ptr);
                if ((flags & PtropUnsafemaster) == 0)
                    verifyMasterChain(toActor);
                break;
            case AaptrTracer:
                setTracer(toActor, ptr);
                break;
        }
    }
}
