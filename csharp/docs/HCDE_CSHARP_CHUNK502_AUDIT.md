# Chunk 502 — GetTag, counters, damage factor, translation, RestoreSpecialPosition, PrintMiscActorInfo

## Scope

GetTag, counters, damage factor, translation, RestoreSpecialPosition, PrintMiscActorInfo

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::GetTag** 8811–8833 (23 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTag** 8835–8839 (5 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetCharacterName** 8841–8853 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::ClearCounters** 8855–8874 (20 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetModifiedDamage** 8876–8890 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::ApplyDamageFactor** 8892–8900 (9 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTranslation** 8902–8918 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::RestoreSpecialPosition** 8927–8974 (48 lines)
- `src/playsim/p_mobj.cpp` **PrintMiscActorInfo** 8987–9061 (75 lines)

**Credited native LOC:** 225 (tally `nativeLoc` 225).

## Managed

`VmPMobjTagDamageRestore.cs`, `VmPMobjPrintMiscActorInfo.cs`
Managed LOC (tally): 201.
Conversion commit: `71d387b2f9ce15442b932e18242428fa7ffd5239`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `TagUsesStringTable` / `TagUsesLiteral` / `TagFallsBackToDefault` / `TagFallsBackToClassName` / `ClearTagOnEmpty` / `ResolveGetTag` | `GetTag` 8811–8833 (8813: `if (Tag != NULL)`; 8816: `if (tag[0] == '$')`; 8818: `return GStrings.GetString(tag + 1);`) |
| `FlagBitSet` | `SetTag` 8835–8839 (8837: `if (def == NULL || *def == 0) Tag = nullptr;`) |
| `CharacterNameFromConversation` / `LineSpecialUsesName` | `GetCharacterName` 8841–8853 (8843: `if (Conversation && Conversation->SpeakerName.Len() != 0)`; 8846: `if (cname[0] == '$')`; 8848: `return GStrings.GetString(cname + 1);`) |
| `ClearKillCounter` / `ClearItemCounter` / `ClearSecretCounter` / `ApplyClearCounters` / `CountInventoryBeforeEuthanize` / `CountFlagBits` | `ClearCounters` 8855–8874 (8857: `if (CountsAsKill() && health > 0)`; 8860: `flags &= ~MF_COUNTKILL;`; 8863: `if (flags & MF_COUNTITEM)`) |
| `InventoryWalkNext` | `GetModifiedDamage` 8876–8890 (8889: `return damage;`) |
| `ApplyPositiveDamageFactor` / `ApplyMobjFactorWhenPositive` / `ApplyDamageFactorChain` | `ApplyDamageFactor` 8892–8900 (8895: `if (damage > 0)`; 8899: `return damage;`) |
| `CustomTranslationFound` | `SetTranslation` 8902–8918 (8905: `if (trname.GetChars()[0] == 0)`; 8909: `return;`; 8913: `if (tnum != INVALID_TRANSLATION)`) |
| `RestoreUsesSpawnCeiling` / `RestoreUsesSpawnFloat` / `SpawnFloatSpace` / `SpawnFloatZ` / `ClampBelowFloor` / `ClampAboveCeiling` / `CeilingSpawnZ` / `FloorSpawnZ` / `ChooseRestoreZ` / `FinalRestoreZ` | `RestoreSpecialPosition` 8927–8974 (8939: `if (flags & MF_SPAWNCEILING)`; 8943: `else if (flags2 & MF2_SPAWNFLOAT)`; 8946: `if (space > 48)`) |
| `FDropItem` | `FDropItem` 8982–8985 |
| `MatchLegacyRenderStyle` / `RenderStyleInTable` / `ShouldPrint` / `RenderStyleLabel` / `ScanLegacyRenderStyle` | `PrintMiscActorInfo` 8987–9061 (8989: `if (query)`; 8995: `if (LegacyRenderStyles[style] == query->RenderStyle)`; 9009: `if (query->flags & ActorFlags::FromInt(1<<flagi)) Printf(" %s", FLAG_…`) |

### Out-of-body uncredit (502)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **229** |
| **Out-of-body removed** | **4** |
| **Honest** | **225** |


**Post-review ratio:** 201/225 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjTagDamageRestoreTests.Tag_StringTableWhenDollar`
- `VmPMobjTagDamageRestoreTests.ClearCounters_KillWhenAlive`
- `VmPMobjTagDamageRestoreTests.DamageFactor_ScalesPositive`
- `VmPMobjTagDamageRestoreTests.Translation_EmptyResets`
- `VmPMobjTagDamageRestoreTests.Restore_SpawnFloatNeedsSpace`
- `VmPMobjTagDamageRestoreTests.Restore_ClampFloor`
- `VmPMobjPrintMiscActorInfoTests.Print_SkipsNullQuery`
- `VmPMobjPrintMiscActorInfoTests.Print_LegacyStyleMatch`
- `VmPMobjPrintMiscActorInfoTests.Print_CustomStyleLabel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 502 --to 502` (0 hard / 0 soft).

## Seams

`GStrings`, `mStringPropertyData`, inventory `VMCall`, `R_FindCustomTranslation`, and `Printf` are not hosted.

## Credit rationale

Credits tag/character name resolution, kill/item/secret counter clearing, inventory damage walk, damage factor scaling, translation lookup, item respawn Z policy, and misc actor debug summary from `p_mobj.cpp` (8811–9061).
