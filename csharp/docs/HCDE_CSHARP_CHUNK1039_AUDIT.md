# Chunk 1039 — Global `DamageTypeDefinition`, `GetMobjDamageFactor`, and `ParseDamageDefinition` from `info.cpp` (**894–1043**). **Pivot:** primary **983–1132** aborted (EOF **1043**; **983–1043** = **61** lines). Follows **1038** (**650–799**).

## Scope

Global `DamageTypeDefinition`, `GetMobjDamageFactor`, and `ParseDamageDefinition` from `info.cpp` (**894–1043**). **Pivot:** primary **983–1132** aborted (EOF **1043**; **983–1043** = **61** lines). Follows **1038** (**650–799**).

## Native ranges

- `src/gamedata/info.cpp` **DamageTypeDefinition / ParseDamageDefinition** 894–1043 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmInfoDamageType1039Slice.cs`
Managed LOC (tally): 123.
Conversion commit: `35e50b20a6623678c6ea0ff3ac7973a166e61999`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DamageTypeDefinition::Apply` global insert (**898–901**) | yes |
| `Get` / `IgnoreArmor` (**903–913**) | yes |
| `GetObituary` (**922–927**) | yes |
| `GetMobjDamageFactor` (**945–990**) | yes |
| `ApplyMobjDamageFactor` (**993–997**) | yes |
| `ParseDamageDefinition` property loop (**1005–1042**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **500–799** | chunks **1037–1038** |
| **800–893** | `DmgFactors::Apply`, `SummonActor`, CCMDs outside hull (**894** starts globals) |
| **915–920** | `DEFINE_ACTION_FUNCTION` host macro |
| **983–1132** (primary target) | past EOF **1043** / sub-150 without padding |
| **800–982** | deferred (sub-bands) until file-scoped preflight only |
| `VmDamageTypeDefinitionRuntime` | uncredited helper — seam only |

| Managed (`VmInfoDamageType1039Slice.cs`) | Native (`info.cpp`) |
|-------------------------------------------|---------------------|
| `RegisterGlobalDamageType` | **898–901** |
| `GlobalIgnoresArmor` | **908–913** |
| `ResolveGlobalObituary` | **922–927** |
| `ResolveMobjDamageFactor` | **945–990** |
| `ApplyMobjDamageFactor` | **993–997** |
| `ClassifyDamageDefinitionProperty` / apply helpers | **1015–1035** |
| Band hull (exclusive slice, EOF **1043**) | **894–1043** |

### Chunk **1039** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1038** |


**Post-review ratio:** 123/150 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmInfoDamageType1039SliceTests.ResolveMobjDamageFactor_UsesTypedActorFactor`
- `VmInfoDamageType1039SliceTests.ResolveMobjDamageFactor_ReplaceFactorOverridesUntyped`
- `VmInfoDamageType1039SliceTests.ApplyMobjDamageFactor_ScalesDamage`
- `VmInfoDamageType1039SliceTests.GlobalIgnoresArmor_ReadsFlag`
- `VmInfoDamageType1039SliceTests.ApplyFactorProperty_ZeroSetsReplace`
- `VmInfoDamageType1039SliceTests.ClassifyDamageDefinitionProperty_RecognizesNoArmor`
- `VmInfoDamageType1039SliceTests.RegisterGlobalDamageType_StoresDraft`
- `VmInfoDamageType1039SliceTests.ResolveGlobalObituary_ReturnsEmptyWhenMissing`
- `VmInfoDamageType1039SliceTests.ResolveMobjDamageFactor_NoneWithoutFactors_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1039 --to 1039` (0 hard / 0 soft).

## Seams

`GlobalDamageDefinitions`, `FMapInfoParser` scanner, `ParseOpenBrace`, and `DEFINE_ACTION_FUNCTION` remain host-injected; uncredited `VmDamageTypeDefinitionRuntime` is a parallel catalog — not re-credited.

## Credit rationale

File-scoped UNCredited **`info.cpp`** **894–1043** (+150 to EOF); excludes **500–799** (**1037–1038**). `uncredited_range_check.py` now matches credited ranges by exact file path (fixes false *info.cpp* collision with `g_mapinfo.cpp` **833–982**).
