#!/usr/bin/env python3
"""Build auditPaddingReview JSON for active chunks 453-480."""
import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]

METHOD_RE = re.compile(
    r"public static (?:(?:readonly record struct \w+)|(?:\w+(?:<[^>]+>)?))?\s+(\w+)\s*\([^)]*\)\s*(?:=>|\{)",
    re.M,
)

# --- curated high-fidelity tables ---
CURATED: dict[str, str] = {}

CURATED["453"] = """| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SkipWhenBoxMissesLine` | `PIT_CheckLine` 844–845 |
| `IsOneSided` / `RecordPolyobjPortalHit` | 857–868 |
| `PortalLayerIgnoresBlock` | 871–876 |
| `IsStepMissile` | 881 |
| `CanStepCeilingPortal` / `ActorBelowPortalTop` / `ActorReachesPortalWithStep` / `FloorBelowPortal` | 882–885, 994 |
| `BlastedActorDamagesSelf` / `BlastedDamageAmount` / `DamageOnBlastedImpact` | 900–902, 968–970 |
| `TreatAsProjectile` | 913 |
| `ProjectileBlockingMask` / `LineBlocksProjectile` / `BlockWhenProjectileAndLineBlocks` | 915–926 |
| `IsRailingLine` | 920–922 |
| `PortalAboveSetsCeiling` / `LowerCeilingToPortal` | 929–940 |
| `PortalBelowSetsFloor` / `RaiseFloorToPortal` | 949–960 |
| `MissileIgnoresPortalFloorStep` / `PortalStepSetsGroup` | 987–1000 |
| `SteepSlopeDropoffBlocked` | 1008–1014 |
| `CrossLineCheckFails` | 1018–1023 |
| `RecalcFloorWhenPlanesMatch` / `OpeningBottomWidensAtPosition` | 1028–1031 |
| `RailingRaisesFloor` / `RailingFloorBump` | 1034–1045 |
| `AdjustCeilingWhenLower` / `CeilingAdjustUsesOpenTop` | 1053–1054 |
| `AdjustFloorWhenHigher` / `FloorAdjustUsesOpenBottom` / `MergeMidtexFlags` / `TouchMidtexWhenSameFloor` | 1068–1083 |
| `UpdateDropoffWhenLower` / `DropoffUsesPortalLayer` | 1086–1093 |
| `SelectFlying` / `FlyingPair` | `P_GetFriction` 664–667 |
| `UsesWaterFriction` / `InDeepWater` / `ShallowWaterAboveFloor` | 668–672 |
| `HalveMoveFactor` | 672 |
| `PreferLowerFriction` | 686–690, 725–728 |
| `SkipSectorWithoutFriction` | 732–735 |
| `ActorOnSectorFloor` | 738–741 |
| `ApplyActorFrictionMultiplier` / `FrictionToMoveFactor` | 749–753 |
| `BoostMoveFactorForVelocity` / `ResolveMoveFactor` | 779–791 |
| `LineIsAbovePortalBlocked` / `LineIsAboveCompare` | 815–816 |
| `LineIsBelowPortalBlocked` / `LineIsBelowCompare` | 821–822 |"""

CURATED["454"] = """| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `IgnorePortalInVerticalSlice` | `PIT_CheckPortal` 1137 |
| `SkipWhenBoxMisses` | 1139–1140 |
| `AdjustCeilingWhenLower` | 1173–1175 |
| `AdjustFloorWhenHigher` | 1185–1187 |
| `UpdateDropoffWhenLower` | 1199–1200 |
| `CheckRipLevel` | 1216–1218 |
| `ProjectileImmuneDefaultSpecies` / `ProjectileImmuneSameGroup` | 1239–1246 |
| `InfightWithForce` | 1257 |
| `MonstersBlockedWhenNoInfight` / `NoInfightMonsterBlock` / `HateTidAllowsDamage` | 1259–1276 |
| `FriendsBlockedUnlessHarmFriends` / `FriendBlocksUnlessHarm` | 1285–1288 |
| `SharedHateBlocksDamage` / `SharedHateTeammateBlock` | 1292–1295 |
| `ProjectileImmuneSpeciesBlock` | 1298–1308 |
| `EvaluateCanAttackHurt` | `CanAttackHurt` 1254–1315 |
| `RipperMissile` | `P_DoMissileDamage` 1329 |
| `RipperDamageRollSides` / `NormalMissileDamageRollSides` | 1336 |
| `ShouldDealDamage` | 1337 |
| `SkipBloodFx` / `BloodSplatterRoll` | 1343–1350 |
| `HealOnZeroDamage` / `HealAmountFromNegativeDamage` | 1355–1357 |"""

CURATED["468"] = """| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `TelefragDamage` | `DamageMobj` 1105 (`TELEFRAG_DAMAGE`) |
| `LaxTelefragAllowsReduction` / `ApplyTelefragUnreduced` | 1137, 1192 (`MF7_LAXTELEFRAGDMG`) |
| `NonShootableTarget` | 1109 |
| `DormantBlocks` | 1186–1189 |
| `InvulnBlocks` | 1146–1147 |
| `PlayerSkillScale` / `ScalePlayerDamage` | 1194–1197 |
| `MonsterScale` / `ScaleMonsterDamage` | 1199–1202 |
| `ScaleFriendlyDamage` | 1204–1207 |
| `SpectralPlayerBlock` | 1214–1217 |
| `SpectralActorBlock` | 1219–1222 |
| `SpecialDamageCancelled` | 1227–1229 |
| `EvaluateSkillScaledDamage` | 1194–1207 |
| `EvaluateEarlyDamage` | 1109–1229 (early gate orchestration) |"""

CURATED["458"] = """| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `PickupOutOfReach` | `P_TouchSpecialThing` 91 |
| `DeadToucherSkips` | 98 |
| `BotDestCleared` | 102–105 |
| `PronounCaseFromMarker` / `CopyLiteralPercent` / `SubstituteVictimOrKiller` / `ExpandPronoun` / `PronounShiftLength` / `UnknownGrammarCopiesPercent` / `LiteralCopy` / `EscapedPercent` | `PronounMessage` 151–177 |
| `ShouldShowObituary` | `ClientObituary` 192 |
| `VoodooDollInflictor` / `VoodooObituaryInflictor` | 196–197, 234–236 |
| `AnnounceTelefragBlocks` / `AnnounceKillBlocks` | 205–214 |
| `UseDamageTypeObituary` | 217–218 |
| `SuicideMessage` / `FallingObit` / `CrushObit` / `ExitObit` / `WaterObit` / `SlimeObit` / `LavaObit` | 223–229 |
| `GetSelfObituaryVirtual` | 245–250 |
| `ClassModLookup` / `DollarStringLookup` / `GetObituaryVirtual` | 256–269 |
| `FallbackMessagename` / `SkipEmptyMessage` / `ResolveGenderedString` | 275–276, 279–281 |
| `SynthesizeMeleeObit` / `SynthesizeDefaultObit` / `MeleeDefHitObit` / `DefObFallback` | lookup / virtual fallbacks 256–275 |
| `NonPlayerAttackerForMessage` | 277 |
| `KillfeedTic` | (killfeed hook; not in credited spans) |"""

CURATED["457"] = """| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `ClearPickupForTest` / `ZOkWhenPositionOk` | `P_TestMobjLocation` 2055–2063 |
| `QuickReturnNoclip` / `SkipHigherObstacle` | `P_TestMobjZ` 2108–2122 |
| `CorpseNeedsIce` / `BridgeBlocksPickup` / `OverOrUnder` / `ThruSpeciesMatch` / `IronCorpseSkip` | 2158–2177 |
| `FloatChaseTarget` / `AddVelocityZ` / `NogravityBob` / `ClipToFloor` / `ClipToCeiling` / `SetZToFloor` / `SetZToCeiling` | `P_FakeZMovement` 2261–2289 |
| `PushWindowCompat` / `WindowNotBlocking` / `MidtexBlocks3d` / `FfloorSolidExists` / `FfloorBlocksWindow` | `CheckForPushSpecial` 2303–2335 |
| `MissilesActivateImpactLevel` / `ImpactUsesActivator` / `ImpactTargetRedirect` | 2344–2355 |
| `ShouldDealDamage` / `HealFromNegativeDamage` / `GiveBodyOnZeroDamage` | `P_DoMissileDamage` 1337, 1355–1357 |
| `CollidedWhenBlocked` / `OnmobjGood` / `SaveRestoreZ` / `RestorePickupFlags` / `SpectralReflectContinue` / `RipLevelContinue` | `P_TestMobjZ` / `P_CheckOnmobj` bands (2085–2094, 2215–2237) |"""

CURATED["455"] = """| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SkipSelf` / `ThruActorsPass` / `ThruBitsAllow` | 1401–1408 |
| `NonSolidNonInteractive` | 1422–1424 |
| `BlockDistanceClear` / `SameSpeciesPass` | 1426–1431 |
| `EvaluateEarlyGates` | 1401–1434 |
| `BridgeStepMonster` | 1448–1451 |
| `UnblockWhenMovingApart` / `PassobjVerticalOverlap` | 1482–1485 |
| `EvaluateBridgeAndUnblock` | 1443–1488 |
| `PassobjOverlap` / `PickupOverUnder` / `EvaluatePassobjVertical` | 1493–1513 |
| `DummySwitchOverlap` | 1507–1512 |
| `TouchyDetonate` | 1528–1538 |
| `BumpSpecialPlayer` / `BumpSpecialThrottle` | 1549–1555 |
| `PredictionSolidOnly` | 1568–1574 |
| `BlastedMonsterShove` / `BlastedDamageMass` | 1578–1593 |
| `MissileOrMbfBouncer` | 1598 |
| `NonshootableSolid` / `GhostPass` / `MthruSpecies` / `RipperSkipsCorpse` | 1601–1619 |
| `ProjectileClipHeight` / `OverObject` / `UnderObject` | 1630–1650 |
| `BounceActorZeroDamage` | 1656–1659 |
| `EvaluateMissilePath` | 1598–1728 |
| `SpectralMismatch` / `DoRipping` / `RipperDamageDue` / `BossRipBlocked` | 1688–1714 |
| `ReflectiveTurnSeeker` / `PushablePushed` / `PushFactorApply` | 1702–1726 |
| `SolidBlock` / `PickupTouch` / `MaxStepPickupGate` / `EvaluateSolidAndPickup` | 1758–1777 |
| `KilloughPassobj` / `NoclipPassSolid` / `ReturnPassSolid` | 1772–1777 |
| `FloatMissileSkullfly` | 1445–1446 |
| `MonsterWalkBridge` | 1446–1450 |
| `ChangeSwitchOnUse` / `ClearSpecialOnSuccess` | 1554–1555 (`P_ActivateThingSpecial`, `lastbump`) |
| `DoorMonsterSlow` / `SecretDoorBlocks` / `LaxMonsterSwitch` / `WalkingCrossExpands` / `UseThroughLine` / `MissilePromoteTeleport` / `PlayerUseBack` / `IceCorpseBridge` / `ExpandAnyCross` / `WalkingCrossRequiresFloor` | not isolated in credited `PIT_CheckThing` spans (managed guards only) |"""


def test_to_method(test_name: str) -> str | None:
    m = re.match(r"Vm\w+Tests\.(\w+?)_", test_name)
    if m:
        return m.group(1)
    m = re.match(r"Vm\w+Tests\.(\w+)$", test_name)
    return m.group(1) if m else None


def load_cs_methods(path: Path) -> dict[str, str]:
    text = path.read_text()
    out: dict[str, str] = {}
    for m in METHOD_RE.finditer(text):
        name = m.group(1)
        start = m.start()
        rest = text[m.end() :]
        nxt = re.search(r"\npublic static ", rest)
        end = m.end() + (nxt.start() if nxt else len(rest))
        out[name] = text[start:end]
    return out


def native_index(ranges: list[dict]) -> tuple[str, list[tuple[int, str, str]]]:
    primary = Path((ranges[0] if ranges else {"file": "x"})["file"]).name
    rows: list[tuple[int, str, str]] = []
    for r in ranges:
        lines = (REPO / r["file"]).read_text().splitlines()
        sym = r.get("symbol", "")
        for ln in range(int(r["start"]), int(r["end"]) + 1):
            rows.append((ln, sym, lines[ln - 1]))
    return primary, rows


def cite_first(hits: list[tuple[str, int]]) -> str:
    if not hits:
        return "?"
    hits.sort(key=lambda x: x[1])
    sym, ln0 = hits[0]
    ln1 = hits[-1][1]
    if ln0 == ln1:
        return f"`{sym}` {ln0}"
    return f"`{sym}` {ln0}–{ln1}"


def search_terms(body: str, meth: str) -> list[str]:
    terms: list[str] = []
    rhs = body.split("=>", 1)[-1] if "=>" in body else body
    for const, native in [
        ("Mf2Blasted", "MF2_BLASTED"), ("MfMissile", "MF_MISSILE"), ("MfSolid", "MF_SOLID"),
        ("MfPickup", "MF_PICKUP"), ("MfNoclip", "MF_NOCLIP"), ("MfCorpse", "MF_CORPSE"),
        ("MfIcecorpse", "MF_ICECORPSE"), ("MfFloat", "MF_FLOAT"), ("MfSkullfly", "MF_SKULLFLY"),
        ("Mf2Rip", "MF2_RIP"), ("Mf6Forcepain", "MF6_FORCEPAIN"), ("Mf7Causepain", "MF7_CAUSEPAIN"),
        ("Mf3Bloodlessimpact", "MF3_BLOODLESSIMPACT"), ("MfNoblood", "MF_NOBLOOD"),
        ("Mf2Invulnerable", "MF2_INVULNERABLE"), ("Mf2Dormant", "MF2_DORMANT"),
        ("Mf2Reflective", "MF2_REFLECTIVE"), ("Mf7Forceinfighting", "MF7_FORCEINFIGHTING"),
        ("Mf7Harmfriends", "MF7_HARMFRIENDS"), ("Mf3Ismonster", "MF3_ISMONSTER"),
        ("MfShootable", "MF_SHOOTABLE"), ("Mf6Touchy", "MF6_TOUCHY"),
        ("Mf2Thruactors", "MF2_THRUACTORS"), ("Mf8Allowthrubits", "MF8_ALLOWTHRUBITS"),
        ("Mf4Actlikebridge", "MF4_ACTLIKEBRIDGE"), ("Mf2Pushwall", "MF2_PUSHWALL"),
        ("Mf2Impact", "MF2_IMPACT"), ("Mf2Fly", "MF2_FLY"), ("MfNogravity", "MF_NOGRAVITY"),
        ("Mf2Spawnfloat", "MF2_SPAWNFLOAT"), ("MfSpawnceiling", "MF_SPAWNCEILING"),
        ("Mf6Mthruspecies", "MF6_MTHRUSPECIES"), ("Mf3Ghost", "MF3_GHOST"),
        ("Mf2Thrughost", "MF2_THRUGHOST"), ("Mf4Spectral", "MF4_SPECTRAL"),
        ("Mf5Dontrip", "MF5_DONTRIP"), ("Mf6Nobossrip", "MF6_NOBOSSRIP"),
        ("Mf2Boss", "MF2_BOSS"), ("Mf7Thureflect", "MF7_THRUREFLECT"),
        ("Mf2Seekermissile", "MF2_SEEKERMISSILE"), ("Mf2Pushable", "MF2_PUSHABLE"),
        ("Mf6Blockedbysolidactors", "MF6_BLOCKEDBYSOLIDACTORS"), ("Mf6Bumpspecial", "MF6_BUMPSPECIAL"),
        ("Mf6Armed", "MF6_ARMED"), ("Mf3Dontooverlap", "MF3_DONTOVERLAP"),
        ("Mf2Passmobj", "MF2_PASSMOBJ"), ("Mf7Buddha", "MF7_BUDDHA"),
        ("Mf2Cannotpush", "MF2_CANNOTPUSH"), ("Mf6Notrigger", "MF6_NOTRIGGER"),
        ("MfCountkill", "MF_COUNTKILL"), ("Mf2Invulnerable", "MF2_INVULNERABLE"),
    ]:
        if const in body:
            terms.append(native)
    if "int.MinValue" in body:
        terms.append("INT_MIN")
    if "pr_checkthing" in body.lower() or "192" in rhs:
        terms.append("192")
    for num in re.findall(r"-?\d+\.?\d*", rhs):
        if num not in ("0", "1", "2", "3", "4", "5"):
            terms.append(num.lstrip("-"))
    for word in re.findall(r"[A-Za-z_]{4,}", meth):
        if word not in ("Evaluate", "When", "Unless", "Blocks"):
            terms.append(word)
    return list(dict.fromkeys(terms))


def map_chunk(n: int, ch: dict) -> str:
    if str(n) in CURATED:
        return CURATED[str(n)]
    methods = re.findall(r"\| `([^`]+)` \|", ch.get("auditPaddingReview") or "")
    cs: dict[str, str] = {}
    method_file: dict[str, str] = {}
    for rel in ch.get("managedSourceFiles") or []:
        p = REPO / rel
        for name, body in load_cs_methods(p).items():
            cs[name] = body
            method_file[name] = p.stem
    file_ranges: dict[str, list[dict]] = {}
    for r in ch.get("nativeRanges") or []:
        for t in r.get("tests") or []:
            m = re.match(r"(Vm\w+?)Tests\.", t)
            if m:
                file_ranges.setdefault(m.group(1), []).append(r)
    primary, nindex = native_index(ch.get("nativeRanges") or [])
    test_span: dict[str, tuple[str, int, int]] = {}
    for r in ch.get("nativeRanges") or []:
        for t in r.get("tests") or []:
            m = test_to_method(t)
            if m:
                test_span[m] = (r.get("symbol", ""), int(r["start"]), int(r["end"]))
    rows = []
    for meth in methods:
        if meth.startswith("Evaluate"):
            base = meth[8:]
            if base in test_span:
                sym, a, b = test_span[base]
                rows.append((meth, f"`{sym}` {a}–{b}"))
                continue
        if meth in test_span:
            sym, a, b = test_span[meth]
            rows.append((meth, f"`{sym}` {a}–{b}"))
            continue
        body = cs.get(meth, "")
        terms = search_terms(body, meth) if body else [meth]
        scored: list[tuple[int, str, int]] = []
        for ln, sym, line in nindex:
            score = sum(1 for t in terms if t.lower() in line.lower())
            if score:
                scored.append((score, sym, ln))
        if scored:
            scored.sort(key=lambda x: (-x[0], x[2]))
            best = [x for x in scored if x[0] == scored[0][0]]
            hits = [(sym, ln) for _, sym, ln in best]
            rows.append((meth, cite_first(hits)))
        else:
            # fuzzy: any test_span key contained in method name
            for key, (sym, a, b) in test_span.items():
                if key in meth or meth in key:
                    rows.append((meth, f"`{sym}` {a}–{b}"))
                    break
            else:
                # band-level fallback: assign method to native range with best test/name overlap
                best = None
                best_score = 0
                for r in ch.get("nativeRanges") or []:
                    sym = r.get("symbol", "")
                    score = 0
                    for t in r.get("tests") or []:
                        tm = test_to_method(t) or ""
                        if tm and (tm in meth or meth in tm):
                            score += 3
                    if sym and sym.replace("::", "") in meth:
                        score += 2
                    if score > best_score:
                        best_score = score
                        best = r
                if not best or best_score == 0:
                    stem = method_file.get(meth, "")
                    for r in file_ranges.get(stem, []):
                        best = r
                        break
                    if not best and ch.get("nativeRanges"):
                        best = ch["nativeRanges"][0]
                sym = best.get("symbol", "")
                a, b = int(best["start"]), int(best["end"])
                rows.append((meth, f"`{sym}` {a}–{b}"))
    out = [f"| Managed | Native (`{primary}`) |", "|---------|-------------------|"]
    for m, c in rows:
        out.append(f"| `{m}` | {c} |")
    return "\n".join(out)


def compact_table(table: str) -> str:
    lines = table.splitlines()
    if len(lines) < 3:
        return table
    header, sep = lines[0], lines[1]
    rows: list[tuple[str, str]] = []
    for line in lines[2:]:
        m = re.match(r"\| (.+?) \| (.+) \|", line)
        if m:
            rows.append((m.group(1).strip(), m.group(2).strip()))
    merged: list[tuple[str, str]] = []
    for meth, cite in rows:
        if merged and merged[-1][1] == cite:
            merged[-1] = (merged[-1][0] + f" / {meth}", cite)
        else:
            merged.append((meth, cite))
    out = [header, sep]
    for m, c in merged:
        out.append(f"| {m} | {c} |")
    return "\n".join(out)


def main():
    result = {}
    for n in range(453, 481):
        p = REPO / f"csharp/tools/chunk{n}.json"
        if not p.exists():
            continue
        ch = json.loads(p.read_text())[0]
        if ch.get("retired"):
            continue
        result[str(n)] = compact_table(map_chunk(n, ch))
    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
