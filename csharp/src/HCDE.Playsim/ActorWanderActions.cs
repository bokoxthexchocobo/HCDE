namespace HCDE.Playsim;

[Flags]
public enum ActorWanderFlags { NoRandomTurn = 32, NoDirectionTurn = 64, StopIfBlocked = 256 }

public static class ActorWanderActions
{
    private static readonly double[] XSpeed = [1, Math.Sqrt(0.5), 0, -Math.Sqrt(0.5), -1, -Math.Sqrt(0.5), 0, Math.Sqrt(0.5)];
    private static readonly double[] YSpeed = [0, Math.Sqrt(0.5), 1, Math.Sqrt(0.5), 0, -Math.Sqrt(0.5), -1, -Math.Sqrt(0.5)];
    internal static (double X, double Y) DirectionVector(byte direction) => (XSpeed[direction], YSpeed[direction]);

    /// <summary>Native P_DoNewChaseDir direction search for a supplied target or avoidance vector.</summary>
    public static void ChooseChaseDirection(Actor actor, double deltaX, double deltaY, bool avoidingDropOff = false)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (!double.IsFinite(deltaX)) throw new ArgumentOutOfRangeException(nameof(deltaX));
        if (!double.IsFinite(deltaY)) throw new ArgumentOutOfRangeException(nameof(deltaY));
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Chase direction selection requires a simulation.");
        var old = Math.Min((int)actor.MoveDirection, 8);
        var opposite = old == 8 ? 8 : (old + 4) & 7;
        var attempts = new bool[8];
        bool TryDirection(int direction)
        {
            if (direction == 8 || attempts[direction]) return false;
            attempts[direction] = true;
            actor.MoveDirection = (byte)direction;
            return TryWalk(sim, actor, avoidingDropOff);
        }
        var horizontal = deltaX > 10 ? 0 : deltaX < -10 ? 4 : 8;
        var vertical = deltaY > 10 ? 2 : deltaY < -10 ? 6 : 8;
        if (horizontal != 8 && vertical != 8)
        {
            var diagonal = deltaX > 0 ? deltaY > 0 ? 1 : 7 : deltaY > 0 ? 3 : 5;
            if (diagonal != opposite && TryDirection(diagonal)) return;
        }
        if (!avoidingDropOff)
        {
            if (sim.NextNewChaseDirByte() > 200 || Math.Abs(deltaY) > Math.Abs(deltaX))
                (horizontal, vertical) = (vertical, horizontal);
            if (horizontal == opposite) horizontal = 8;
            if (vertical == opposite) vertical = 8;
        }
        if (TryDirection(horizontal) || TryDirection(vertical)) return;
        if (!avoidingDropOff && TryDirection(old)) return;
        var ascending = (sim.NextNewChaseDirByte() & 1) != 0;
        for (var i = 0; i < 8; i++)
        {
            var direction = ascending ? i : 7 - i;
            if (direction != opposite && TryDirection(direction)) return;
        }
        if (TryDirection(opposite)) return;
        actor.MoveDirection = 8;
    }

    public static void Wander(Actor actor, ActorWanderFlags flags = 0)
        => WanderCore(actor, flags, nativeSight: false);

    internal static void WanderNative(Actor actor) => WanderCore(actor, 0, nativeSight: true);

    private static void WanderCore(Actor actor, ActorWanderFlags flags, bool nativeSight)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if ((flags & ~(ActorWanderFlags.NoRandomTurn | ActorWanderFlags.NoDirectionTurn | ActorWanderFlags.StopIfBlocked)) != 0)
            throw new ArgumentOutOfRangeException(nameof(flags));
        if (actor.Destroyed) return;
        actor.InCombat = false;
        if (actor.InConversation || actor.StandStill) return;
        if (actor.ReactionTime != 0) { actor.ReactionTime = unchecked(actor.ReactionTime - 1); return; }
        var sim = actor.Simulation ?? throw new InvalidOperationException("Wander requires a simulation.");
        if ((flags & ActorWanderFlags.NoDirectionTurn) == 0 && actor.MoveDirection < 8)
            TurnTowardMoveDirection(actor);
        actor.MoveCount = unchecked(actor.MoveCount - 1);
        if (actor.MoveCount < 0 && (flags & ActorWanderFlags.NoRandomTurn) == 0
            || !Move(sim, actor) && (flags & ActorWanderFlags.StopIfBlocked) == 0)
        {
            RandomDirection(sim, actor, nativeSight || sim.NativeMonsterMovement);
            actor.MoveCount = unchecked(actor.MoveCount + 5);
        }
    }

    internal static void TurnTowardMoveDirection(Actor actor)
    {
        if (actor.MoveDirection >= 8) return;
        var angle = Math.Floor(actor.Angle.ToDegrees() / 45) * 45;
        var delta = Math.IEEERemainder(actor.MoveDirection * 45 - angle, 360);
        actor.Angle = BamAngle.FromDegrees(angle + Math.Sign(delta) * 45);
    }

    internal static bool Move(AuthoritySimulation sim, Actor actor, bool avoidingDropOff = false)
    {
        var avoid = actor.AvoidHazards || sim.Compat.HasFlag(CompatSurface.AvoidHazards);
        var target = sim.Actors.FirstOrDefault(a => a.Id == actor.TargetId && !a.Destroyed);
        var onLift = (actor.StayOnLift || sim.Compat.HasFlag(CompatSurface.StayOnLift)) && target is { Health: > 0 }
            && ActorMovementHazards.IsOnLift(sim, actor) && ActorMovementHazards.SharesPrimaryTag(sim, target, actor);
        var wasUnderDamage = avoid && ActorMovementHazards.UnderDamage(sim, actor) != 0;
        if (!MoveCore(sim, actor, avoidingDropOff)) return false;
        if (onLift && sim.NextStayOnLiftByte() < 230 && !ActorMovementHazards.IsOnLift(sim, actor))
            actor.MoveDirection = 8;
        else if (avoid && !wasUnderDamage)
        {
            var damage = ActorMovementHazards.UnderDamage(sim, actor);
            if (damage < 0 || damage > 0 && sim.NextAvoidCrushByte() < 200) actor.MoveDirection = 8;
        }
        if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(actor);
        return true;
    }

    private static bool MoveCore(AuthoritySimulation sim, Actor actor, bool avoidingDropOff)
    {
        if (actor.Blasted) return true;
        if (actor.MoveDirection >= 8) { actor.MoveDirection = 8; return false; }
        if (!actor.NoGravity && !actor.CanJump && !actor.OnMobj && actor.Z.ToDouble() > ActorPhysics.SectorSupportFloor(sim, actor)) return false;
        var target = sim.Actors.FirstOrDefault(a => a.Id == actor.TargetId && !a.Destroyed);
        var speed = actor.MovementSpeed.ToDouble();
        var friction = ActorPhysics.GroundFriction;
        var moveFactor = 2048 / 65536.0;
        if (sim.Compat.HasFlag(CompatSurface.MbfMonsterMove) && !actor.NoFriction)
        {
            (friction, moveFactor) = ActorPhysics.TerrainMovementFactors(sim, actor);
            if (friction < ActorPhysics.GroundFriction)
            {
                speed *= (1 + moveFactor / (2048 / 65536.0)) / 2;
                if (speed == 0) speed = actor.MovementSpeed.ToDouble();
            }
        }
        var jumpDown = actor.JumpDown && !sim.Level.NoJumpDown && target is { Health: > 0 }
            && !actor.IsFriend(target) && actor.Distance2D(target) < 144 && sim.NextDropoffByte() < 235;
        var dx = speed * XSpeed[actor.MoveDirection]; var dy = speed * YSpeed[actor.MoveDirection];
        var maximum = actor.Radius.ToDouble() - 1;
        var component = Math.Max(Math.Abs(dx), Math.Abs(dy));
        var steps = maximum > 0 && component > maximum ? checked(1 + (int)(component / maximum)) : 1;
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble();
        var originalSector = actor.SectorIndex; var originalOnGround = actor.OnGround;
        for (var step = 0; step < steps; step++)
        {
            if (ActorPhysics.TryWanderMove(sim, actor, x + dx * (step + 1) / steps, y + dy * (step + 1) / steps,
                out var floatOk, out var floor, out var blockingWall, avoidingDropOff || jumpDown)) continue;
            if (MonsterBlockedMoveActions.TryClearBlockedMove(sim, actor, blockingWall)) return true;
            if (!floatOk) return false;
            if (!double.IsFinite(actor.FloatSpeed)) throw new InvalidOperationException("FloatSpeed must be finite.");
            var oldZ = actor.Z;
            actor.Z = Fixed.FromDouble(oldZ.ToDouble() + (oldZ.ToDouble() < floor ? actor.FloatSpeed : -actor.FloatSpeed));
            if (ActorPhysics.FindZBlocker(sim, actor) is not null) { actor.Z = oldZ; return false; }
            actor.InFloat = true;
            return true;
        }
        if (friction > ActorPhysics.GroundFriction)
        {
            actor.X = Fixed.FromDouble(x); actor.Y = Fixed.FromDouble(y);
            actor.SectorIndex = originalSector; actor.OnGround = originalOnGround;
            var scale = moveFactor / (2048 / 65536.0) / 4;
            actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + dx * scale);
            actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + dy * scale);
        }
        if (!actor.NoGravity && !actor.CanJump && !actor.OnMobj)
        {
            var floor = ActorPhysics.SectorSupportFloor(sim, actor);
            var oldZ = actor.Z;
            if (oldZ.ToDouble() < floor) actor.Z = Fixed.FromDouble(floor);
            else if (oldZ.ToDouble() <= floor + actor.MaxStepHeight.ToDouble())
            {
                actor.Z = Fixed.FromDouble(floor);
                if (ActorPhysics.FindZBlocker(sim, actor) is not null) actor.Z = oldZ;
            }
        }
        actor.InFloat = false;
        return true;
    }

    private static bool TryWalk(AuthoritySimulation sim, Actor actor, bool avoidingDropOff = false)
    {
        if (!Move(sim, actor, avoidingDropOff)) return false;
        actor.MoveCount = sim.NextTryWalkByte() & 15;
        return true;
    }

    private static void RandomDirection(AuthoritySimulation sim, Actor actor, bool nativeSight)
    {
        var old = Math.Min((int)actor.MoveDirection, 8); var opposite = old == 8 ? 8 : (old + 4) & 7;
        bool TryDirection(int direction)
        {
            if (direction == 8 || direction == opposite) return false;
            actor.MoveDirection = (byte)direction;
            return TryWalk(sim, actor);
        }
        if (actor.Friendly && !actor.DontFollowPlayers)
        {
            var player = SelectPlayer(sim, actor);
            if (player is not null && ((sim.NextNewChaseDirByte() & 1) != 0
                || !(nativeSight ? CombatTrace.CheckSight(sim, actor, player) : CombatTrace.HasLineOfSight(sim, actor, player))))
            {
                var dx = player.X.ToDouble() - actor.X.ToDouble(); var dy = player.Y.ToDouble() - actor.Y.ToDouble();
                var horizontal = dx > 128 ? 0 : dx < -128 ? 4 : 8;
                var vertical = dy > 128 ? 2 : dy < -128 ? 6 : 8;
                if (horizontal != 8 && vertical != 8 && TryDirection(dx > 0 ? dy > 0 ? 1 : 7 : dy > 0 ? 3 : 5)) return;
                if (sim.NextNewChaseDirByte() > 200 || Math.Abs(dy) > Math.Abs(dx)) (horizontal, vertical) = (vertical, horizontal);
                if (TryDirection(horizontal) || TryDirection(vertical)) return;
            }
        }
        if (sim.NextNewChaseDirByte() < 150 && TryWalk(sim, actor)) return;
        var turn = (sim.NextNewChaseDirByte() & 1) != 0 ? -1 : 1;
        if (old == 8) old = sim.NextNewChaseDirByte() & 7;
        for (var direction = (old + turn) & 7; direction != old; direction = (direction + turn) & 7)
            if (TryDirection(direction)) return;
        if (opposite != 8)
        {
            actor.MoveDirection = (byte)opposite;
            if (TryWalk(sim, actor)) { actor.MoveCount = sim.NextNewChaseDirByte() & 15; return; }
        }
        actor.MoveDirection = 8;
    }

    private static PlayerPawn? SelectPlayer(AuthoritySimulation sim, Actor actor)
    {
        if (actor.FriendPlayer != 0) return sim.Players.FirstOrDefault(p => !p.Destroyed && p.PlayerNum == actor.FriendPlayer - 1);
        if (sim.GameMode == SpawnGameMode.Single) return sim.Players.FirstOrDefault(p => !p.Destroyed && p.PlayerNum == 0);
        if (!sim.Players.Any(p => !p.Destroyed)) return null;
        var slot = sim.NextNewChaseDirByte() & 63;
        for (var i = 0; i < 64; i++, slot = (slot + 1) & 63)
            if (sim.Players.FirstOrDefault(p => !p.Destroyed && p.PlayerNum == slot) is { } player) return player;
        return null;
    }
}
