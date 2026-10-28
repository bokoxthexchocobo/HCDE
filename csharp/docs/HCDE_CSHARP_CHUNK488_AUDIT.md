# Chunk 488 — SpawnMapThing ShouldSpawn gate and actor placement UDMF tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-compose-chunk29 src/playsim/p_mobj.cpp 6882-6941).

## Scope

SpawnMapThing ShouldSpawn gate and actor placement UDMF tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-compose-chunk29 src/playsim/p_mobj.cpp 6882-6941).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `c50818e49e8a9def30779ef01c9aedd030255bed`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `AddFloorZOnSpawn` / `SubtractCeilingZOnSpawn` / `RecordFloatbobSpecial` / `AdjustFloorClipOnSpawn` / `ZeroGravitySpawn` | `SpawnMapThing` 6871–6881 (6879: `if (!ret) return nullptr;`) |
| `ResolveMapthingSpawnZ` / `ApplyConversation` / `ApplyAlpha` / `ApplyRenderStyle` / `ApplyScaleX` / `ApplyScaleY` / `ApplyPitch` / `ApplyRoll` / `ApplyScore` / `ApplyFillColor` / `ReshuffleSpotRgb` / `ApplyLightFlagOverride` / `PropagateUdmfLightToAttached` / `LevelSpawnedWhenAlive` / `ScaleHealthFromMapthing` / `PositiveScaledHealth` / `NegativeScaledHealth` / `ScheduleDieFromSpawn` / `CaptureStartHealth` / `CopyMapthingSpecial` | `SpawnMapThing` 6942–7094 (6944: `if (mthing->Conversation > 0)`; 6948: `if (root != -1)`; 6956: `if (mthing->Alpha >= 0)`) |

### Out-of-body uncredit (488)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **164** |
| **Out-of-body removed** | **9** |
| **Honest** | **155** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 488 --to 488` (0 hard / 0 soft).

## Seams

`VMCall` ShouldSpawn, `StaticSpawn`, `CallBeginPlay`, and `GetConversation` are not hosted.

## Credit rationale

Credits virtual spawn gate, mapthing Z selection, conversation/UDMF/light/health placement policy from `p_mobj.cpp` (6871–6881, 6942–7094).
