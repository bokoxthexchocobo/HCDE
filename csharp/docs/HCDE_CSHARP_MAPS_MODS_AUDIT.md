# Maps and mods — implementation and combined audit

> Historical pass. See the [phase 2/3 continuation audit](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md)
> for PK3/Hexen support, corrected bootstrap/activation behavior and current gaps.

Date: 2026-09-26. Phase 3 of the new eight-phase engine-completion plan.
This is separate from the historical `HCDE_CSHARP_PHASE3_AUDIT.md` networking
milestone. Baseline: 832 passing managed tests after the combat/AI pass.

## Verdict

The maps/mods foundation described below is implemented and audited. **Full
phase-3 native compatibility is not complete and is not signed off.** The whole
engine remains a partial C# conversion, with the native sources retained.
Changes from this and the preceding gameplay/invasion work remain uncommitted
and unpushed.

This review covers changed map parsing, WAD load order, spawning, line dispatch,
server bootstrap and input integration; it also revisits combat, invasion,
replication and persistence risks from the previous audits. Solution-wide build,
regression and dependency checks cover all 23 managed projects. This is a
risk-based self-audit, not an independent security assessment or a line-by-line
review of the entire native engine. Synthetic fixtures do not establish real-WAD
playthrough or native-client compatibility.

## Implemented and corrected

- Ordered WAD loading through repeated `--file <PWAD>` arguments. The last map
  marker selects the replacement map as a group; a broken replacement cannot
  silently fall back to an earlier map. Non-map lumps remain in directory order.
  Combined file sizes and the merged representation are bounded to 256 MiB.
- Embedded DEHACKED lumps are applied in load order during server bootstrap.
  The existing twelve-entry actor subset now has its Doom editor numbers, so
  supported ordinary Thing patches can affect spawned actors. Unpatched
  placeholder defaults no longer override every actor just because a patch exists.
  Patch errors and map errors abort startup before binding the game socket.
- Overflow-safe WAD directory and lump bounds. TEXTMAP now uses the checked
  lump reader rather than slicing unchecked offsets.
- Runtime geometry validation checks vertex/side/sector references, zero-length
  lines, inverted sectors, finite positions and supported coordinate ranges.
  Binary maps carrying BEHAVIOR are explicitly rejected for playback: the
  existing binary decoder understands Doom records, not Hexen record layouts.
- UDMF thing positions, height offsets, angles, IDs, arguments and spawn flags
  survive level construction. Fractional XY/Z positions survive spawning.
  Sector heights outside the current integral signed-short runtime are rejected
  before narrowing. Integer fields reject fractions, nonfinite values and overflow.
  Accepted namespace names are Doom, ZDoom and ZDoomTranslated; this is not a
  claim of implementing every feature of those namespaces.
- Skill and single/co-op/deathmatch filters control spawning. Player starts
  bypass skill flags. UDMF omitted spawn flags remain false, consistent with
  native zero-initialized map things. Invasion reads surviving spawned markers,
  so excluded markers no longer create enemies. Its no-marker fallback remains.
- BAM conversion handles negative and greater-than-180-degree map angles without
  signed-int overflow. Spawn height is bounded before fixed-point conversion and
  then fitted to the sector.
- Map line numbers now dispatch by format/namespace. Classic Doom specials
  1/31 are use doors, 2/4/90 crossing doors, 11/51 use exits, 52/124 crossing
  exits, and 39/97 crossing teleports. ZDoom UDMF handles door 11/12, teleport
  70, ACS 80 and exits 243/244 with explicit playeruse/playercross/repeat flags.
  Unsupported map actions remain inactive instead of falling into the previous
  mixed-number dispatch. Hand-built levels with Unknown format retain the old
  internal action convention for compatibility with existing fixtures.
- BT_USE reaches the simulation. A 64-unit use trace stops at blocking geometry.
  Manual doors target the back sector; successful one-shot actions clear once.
  Doom teleports select by sector tag, and the supported ZDoom teleport path
  selects by thing ID. Door timings and teleport semantics remain approximations.

Native references: `src/maploader/udmf.cpp`, `src/doomdata.h`,
`src/d_event.h`, and `src/playsim/actionspecials.h`. No native gameplay source
was removed by this pass.

## Remaining findings and release gates

| Priority | Finding / consequence | Required work |
| --- | --- | --- |
| High | Snapshot record count and 8,000-byte transport limits remain. Actor tombstones accumulate; sustained projectile traffic can throw and terminate the server. Large maps can hit these limits immediately. | Bounded baselines, paging, retirement acknowledgements and loss/reorder tests in multiplayer work. |
| High | WAD stacking currently supplies map data and embedded DEHACKED. There is no complete resource resolver/client integration, PK3/ZIP stack, MAPINFO-driven map progression, DECORATE/ZScript class loading or complete DEHACKED/BEX runtime. Unknown classes still become generic actors. | Further phase-3 mod compatibility and scripting integration; supported mod corpus. |
| High | Hexen binary playback, most line/sector specials, locked doors/keys, full lifts/floors/crushers, switch textures, map transitions, slopes, portals and 3D floors remain incomplete. Unknown UDMF properties are ignored; accepted namespace is not feature validation. | Complete format translation and geometry/special behavior with native-reference playthroughs. |
| High | Map spawns are not a full participant lifecycle. Unused player starts, deathmatch starts, duplicate starts, respawn and disconnect require integration. Invasion still uses generic bots, a fallback spawn and a simplified wave director. | Multiplayer/invasion lifecycle and real invasion-map testing, including wipe/retry. |
| High | Mod resource lists/hashes are not negotiated with clients. Managed projectile IDs, generic actor definitions and missing presentation prevent a native-interoperability claim. | Resource/class negotiation and client conversion. |
| High | Native weapon/enemy definitions, RNG, psprite states, pitch/autoaim and specialized attacks remain approximations. DEHACKED parsed fields are not all executed; baseline patch objects are mutable and reused when chaining patches. | Combat/scripting parity and complete immutable mod definitions. |
| High | Pose archives omit inventory, actor membership, AI/projectile state, movers, running scripts and invasion state. Single pending input slots still overwrite multiple commands drained in one tic. | Persistence and command-queue/replay phases. |
| High | Native engine/runtime verification remains unavailable: generated VS files reference another checkout and there is no local native executable/IWAD fixture for a representative playthrough. | Build this checkout and compare native/managed host-client behavior, including the originally reported invasion round/timer/enemy desync. |
| Medium | Use traces and actions are a subset: no complete front-side/use-through/monster activation rules or native held-button semantics. ZDoom action arguments beyond the documented selection are incomplete. | Full activation translation and reference tests. |
| Medium | Map validation is structural, not a topology/resource audit. Empty/incomplete regions and missing textures, unknown properties, unsupported specials and BSP inconsistencies are not fully diagnosed. Direct scans have no large-map performance evidence. | Compatibility diagnostics, real map corpus and profiling. |
| Medium | Rendering/audio/scripting remain subset implementations. Existing client tests exercise those subsets, not a complete playable native-equivalent client. | Remaining scripting and client phases. |

The snapshot failure is the most immediate release blocker. The original native
invasion issue has managed regression coverage and a compiled native policy test,
but has not been reproduced and cleared in a real native multiplayer session.

## Verification

- Final Release solution regression run: **858 passed, zero failed/skipped**,
  including 130 playsim tests. This adds 26 cases above the previous baseline.
- Release solution build with `--no-restore --verbosity quiet -warnaserror`:
  **zero warnings, zero errors**.
- Solution-wide `dotnet list ... package --vulnerable --include-transitive`:
  **no known vulnerable packages** reported by the configured NuGet source.
- Native invasion policy assertions compile with MSVC C++17 `/W4 /WX /c`.
  This checks that helper test, not the full native engine or `d_net.cpp`.
- `git diff --check`: no whitespace errors (Git reports existing CRLF normalization
  notices). New files are also included in the build/test runs.
- Commands run from repository root using SDK 10.0.201 targeting net8.0. The
  existing `csharp/global.json` SDK pin is unchanged.

New cases cover overflowing directory/lump bounds; corrupt TEXTMAP; map override
order and broken replacements; invalid references; Hexen playback rejection;
UDMF numeric preservation/rejection; skill/mode filtering; unsigned angles;
DEHACKED spawning and ordered server loading; missing files/bad startup; namespace
dispatch; use versus crossing; repeat/one-shot behavior; back-sector doors and
wall-occluded use. Existing combat/AI, physics, saves, protocol, pregame, invasion,
stale snapshot and multi-recipient regressions remain passing.

Example supported load order, from repository root:

```powershell
dotnet run --project csharp/src/HCDE.Server -- --iwad C:\games\doom2.wad --file C:\mods\maps.wad --file C:\mods\tuning.wad --map MAP01
```

Later WADs override earlier map groups; DEHACKED lumps accumulate in that order.
This command demonstrates the loader interface, not compatibility certification
for arbitrary WADs.
