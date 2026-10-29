# Chunk 1171 — Rail-attack tail, camera offset, and talk facing from `p_map.cpp` (**5568–5821**).

## Scope

Rail-attack tail, camera offset, and talk facing from `p_map.cpp` (**5568–5821**).

## Native ranges

- `src/playsim/p_map.cpp` **P_RailAttack tail** 5568–5821 (254 lines)

**Credited native LOC:** 254 (tally `nativeLoc` 254).

## Managed

`VmPMapRailTalk1171Slice.cs`
Managed LOC (tally): 574.
Conversion commit: `b86ed19b85d72bc77c88014ceb43ac177a543753`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Rail puff, damage, and trail (**5568–5691**) | yes |
| Chase cvars and `R_OffsetView` (**5699–5744**) | yes |
| Conversation actions (**5755–5805**) | yes |
| `P_TalkFacing` (**5807–5821**) | yes |
| Band hull (**5568–5821**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **5495–5567** | rail setup is chunk **482** |
| **5822–5828** | use-lines comment, left uncredited |
| **5829–6100** | use lines are chunk **716** |
| **6101–6522** | radius damage stays off the queue |
| `Trace` / `P_DamageMobj` / `P_SpawnPuff` | the trace and the effect calls stay host-supplied |

A puff class spawns the probe with allow-replace and copies its species. A missing probe uses species `None`. Thru-bits are copied, and the thru-bits enable is cleared unless the probe has `MF8_ALLOWTHRUBITS` (**0x8000**). The trace mask is shootable (**4**) and the wall mask is block-everything.

An actor with no blood, or a dormant actor, or an invulnerable actor whose puff does not foil invulnerability, spawns a puff whenever the class exists and does not bleed. Any other actor spawns only when always-puff is set, adds the bleed puff flag, and bleeds unless the impact is bloodless. A missing hit puff falls back to the probe. Poison runs when the damage is positive and the duration is not `int.MinValue`, before damage. Damage flags are puff-inflictor, use-angle, and railgun (**8192**), plus player attack, foil-invul, and foil-buddha when those apply. Bleed and the blood spawn use the positive applied damage, otherwise the original amount, at the unshifted hit.

Geometry runs after the hits. A wall with always-puff spawns at the trace angle and destroys that puff on a horizon line when the puff does not sky-explode. Force-decal with a generator decals the defaults; every other wall decals the source. A floor or ceiling with always-puff destroys the puff on the matching sky texture when it does not sky-explode. The fired event sees the probe. Crossed water splashes. Otherwise a floor with no height sector hits water. The probe is destroyed after that, and the trail is drawn with the portal list and the rail parameters. Chase height is **-8** and chase distance is **90**.

An out-of-bounds hardware view steps by the direction times the distance and reads the render sector. A trace hit pulls the camera back by the minimum of **5** and the hit distance, then adds the normalized delta from the direction angle to the source angle. A miss copies the hit position and the trace sector. When the view is not both out of bounds and hardware, a negative direction Z walks floor portals while the position is below the plane, and a positive Z walks ceiling portals while it is above the plane.

`CanTalk` requires a conversation, health above **0**, and no in-combat flag. `HasConversation` checks only the conversation. A net game with conversation limits refuses a player body that is not a settings controller, and prints the controller message when that body is the console player. Rumble `menu/activate` runs only when rumble and menu haptics are both set. The actor then plays conversation animation **0** and starts the conversation. Talk facing aims at yaw, yaw plus **5.625**, and yaw minus **5.625**, range **128**, pitch **35**, and flags force-no-smart, check-conversation, and portal-restrict. The first actor ends the search, even when the conversation returns false.

| Managed (`VmPMapRailTalk1171Slice.cs`) | Native (`p_map.cpp`) |
|-----------------------------------------|----------------------|
| Rail tail, offset view, and talk facing | **5568–5821** |
| Band hull (exclusive slice) | **5568–5821** |

### Chunk **1171** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **254** |
| **Net headline** | **+254** vs post-**1170** |


**Post-review ratio:** 574/254 managed/native ≈ 226% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapRailTalk1171SliceTests.Rail_SpawnsTheProbeAndBleedsAtTheReturnedDamage`
- `VmPMapRailTalk1171SliceTests.Rail_DecalsTheWallAndSplashesBeforeTheProbeDies`
- `VmPMapRailTalk1171SliceTests.View_PullsTheCameraBackAndWalksDownThroughAFloor`
- `VmPMapRailTalk1171SliceTests.Talk_StopsAtTheFirstActorAndBlocksANetPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1171 --to 1171` (0 hard / 0 soft).

## Seams

The rail trace, puff spawn, damage, blood, decal, splash, and camera trace stay host-supplied. Rail setup **5495–5567** is chunk **482**. Use lines from **5829** are chunk **716**.

## Credit rationale

UNCredited **`p_map.cpp`** **5568–5821** (+254). Chunk **482** ends at **5567**. Chunk **716** starts at **5829**. **5822–5828** is the use-lines comment and stays uncredited. A full tally line scan shows no overlap. Radius damage **6101–6522** is not this band.
