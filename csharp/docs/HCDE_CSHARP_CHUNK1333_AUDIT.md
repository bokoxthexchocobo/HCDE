# Chunk 1333 — DeHackEd PatchSound, PatchFrame, PatchSprite, PatchAmmo, and the skill and weapon bit tables from d_dehacked.cpp (1907-2252).

## Scope

DeHackEd PatchSound, PatchFrame, PatchSprite, PatchAmmo, and the skill and weapon bit tables from d_dehacked.cpp (1907-2252).

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchSound** 1907–2252 (346 lines)

**Credited native LOC:** 346 (tally `nativeLoc` 346).

## Managed

`VmDehackedPatch1333Slice.cs`
Managed LOC (tally): 519.
Conversion commit: `5dcf30632cd7fc8ad7db6f2c54a433c3de280328`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sound-patch comment and PatchSound (**1907–1963**) | yes |
| Skill bit table (**1965–1967**) | yes |
| PatchFrame (**1970–2118**) | yes |
| PatchSprite (**2120–2160**) | yes |
| PatchAmmo (**2162–2242**) | yes |
| Weapon bit table (**2244–2251**) | yes |
| Band hull (**1907–2252**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetLine | callee |
| FindState | callee |
| FindSprite | callee |
| Commented sound offset | compiled out |
| PatchWeapon | chunk 514 from line 2253 |

PatchSound consumes assignment lines and returns the first line that is not an assignment. The offset body in comments does not run. PatchFrame forces tics 5 and 4 for frames 47 and 48, then reads keys whose lengths must match. Duration clamps to -1..32767 after the value is narrowed. Unknown 2 writes immediately. Sprite number is stored for a later remap. Next frame calls FindState and sets the changed-state flag. Sprite subnumber replaces the local frame word, including the fullbright bit. Args1 through Args8 store the full 64-bit value. Args0 and Args9 print the invalid index. MBF21 Bits splits on comma, plus, pipe, and whitespace. A numeric token is a base-10 bit index word. SKILL5FAST sets index bit 0, which becomes STF_FAST. An unknown mnemonic is printed and does not by itself change flags. The debug line prints the flags before the final DeHackEd bit. A live frame then sets STF_DEHACKED, warns when the low 15 bits exceed 63, stores six frame bits, and sets or clears fullbright from bit 0x8000. A missing frame prints the out-of-range line and does not publish those final fields. PatchSprite accepts a sprite index below the original count. Offset is atoi. When the offset is positive and the patch is not DSDehacked, the index becomes (offset - toff[dversion] - 22044) / 8. An in-range index looks up the original name and copies four characters from the mapped original name. PatchAmmo accepts indexes 0..3 that are also within the name list, including one past the end as a miss. Max ammo and Per ammo are atoi. The backpack max is twice the new max and the backpack amount is the new per. Descendants copy the max and scale the amount by the new per over the old per. Weapons scale AmmoGive1 and AmmoGive2 when the ammo type matches. The ammo type itself is not scaled again. NOTHRUST is -1. The other weapon bits are the native weapon-info flags.

| Managed (`VmDehackedPatch1333Slice.cs`) | Native (`d_dehacked.cpp`) |
|-----------------------------------------|---------------------------|
| Sound, frame, sprite, ammo, and the bit tables | **1907–2252** |
| Band hull (exclusive slice) | **1907–2252** |

### Chunk **1333** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **346** |
| **Net headline** | **+346** vs post-**1332** |


**Post-review ratio:** 519/346 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatch1333SliceTests.Frame_AppliesKeysBitsAndStops`
- `VmDehackedPatch1333SliceTests.Sprite_Ammo_RemapsAndScales`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1333 --to 1333` (0 hard / 0 soft).

## Seams

GetLine, FindState, and FindSprite stay callees. The commented sound-offset body stays compiled out. PatchWeapon, which reads the weapon bit table, stays chunk 514.

## Credit rationale

UNCredited d_dehacked.cpp 1907-2252 (+346). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of a_ceiling.cpp 216-551 and g_mapinfo.cpp 1491-1826 (336) and whole-file voxeldef.cpp 1-331. PatchThing through line 1906 stays chunk 521. PatchWeapon at 2253 stays chunk 514.
