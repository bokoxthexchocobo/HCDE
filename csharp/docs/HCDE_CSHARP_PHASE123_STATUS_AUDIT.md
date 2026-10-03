# Gameplay phases 1–3: implementation and completion audit

## Conversion and audit: ACS armor bonus grants (2026-10-02)

Audited ArmorBonus defaults and BasicArmorBonus.SetGiveAmount/Use. Converted
previously ignored ACS ArmorBonus grants to the shared catalog bonus path.
Bonus grants now honor explicit amounts, add up to the vanilla 200 cap and
raise stored armor capacity to at least 200 on success. Active suit protection
and absorption caps remain unchanged. Full/over-cap armor rejects the bonus
without changing capacity. Addition uses remaining capacity before adding,
avoiding overflow for large positive requested counts.

Added six regressions for counts, cap/large input, existing suit protection,
full armor and explicit catalog amounts. Release validation: **4,501 tests
passed (3,428 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with
zero warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include armor skill factors, depleted armor absorption-field
reset parity, custom bonus counters/alternate semantics, Armor/BasicArmor
direct grants, full inventory serialization and native executable comparison.
Changes uncommitted.

## Conversion and audit: ACS armor suit grants (2026-10-02)

Audited GreenArmor/BlueArmor defaults and BasicArmorPickup.SetGiveAmount/Use.
Converted previously ignored ACS suit grants: GreenArmor grants 100 times
the requested count, BlueArmor grants 200 times the count, and the existing
managed MegaArmor spelling is accepted as an alias. Uses the catalog suit
path for protection percentage, maximum capacity and absorption-cap reset.
Catalog suit grants now honor explicit pickup amounts. A weaker/equal suit
leaves the existing suit intact. Count multiplication uses checked arithmetic
so unrepresentable grants fail explicitly rather than wrap to a default gift.

Added six regressions for class names, count scaling beyond 200, maximum and
protection values, and weaker-suit rejection. Release validation: **4,495
tests passed (3,422 Playsim; 500 MapLoader)**. Warnings-as-errors build passed
with zero warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include Armor alias versus BasicArmor direct-grant behavior,
armor skill factors, custom armor classes/bonus counters, full inventory
serialization and native executable comparison. Changes uncommitted.

## Conversion and audit: stored armor maximum (2026-10-02)

Audited BasicArmor default inventory maximum, BasicArmorPickup.Use and
BasicArmorBonus.Use. Added ArmorMaximum independently of current Armor:
default one, suit SaveAmount on acquisition or stored-suit promotion, and
the larger existing/bonus maximum after a successful bonus. Depletion leaves
the maximum intact. ACS maximum queries now use the stored value instead
of the remaining armor amount. Pistol-start reset restores the default.
Converted existing armor-use grants to update this value and included
nondefault capacity in simulation checksums.

Added four regressions covering green/mega depletion, bonus/suit transitions
and stored-suit promotion/reset. Release validation: **4,489 tests passed
(3,416 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include native ACS Armor/BasicArmor grant semantics, full
inventory save serialization, cooperative armor filtering lifecycle, custom
armor bonus counters/alternate semantics and native executable comparison.
Changes uncommitted.

## Conversion and audit: ACS health and key maximum queries (2026-10-02)

Audited p_acs.cpp CheckInventory, PlayerPawn.GetMaxHealth and native default
deh.MaxHealth (100), plus Inventory's default MaxAmount (one) inherited by
Doom keys. Corrected managed maximum-health queries from the bonus cap of
200 to the supported vanilla normal limit of 100. Current-health queries
still return actual health, including values above the normal limit.
Key maximum queries now return one independent of ownership; current key
queries retain the existing shared-color ownership model.

Added nine regressions covering three current health values and all six
card/skull classes. Updated the cooperative GetMaxInventory opcode test
from its incorrect 200 expectation to 100. Release validation: **4,485 tests
passed (3,412 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with
zero warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include custom Player.MaxHealth, Dehacked Max Health settings,
health upgrades, separate card/skull inventory identity, stored BasicArmor
MaxAmount, removal lifecycle/ClearInventory and native executable comparison.
Changes uncommitted.

## Conversion and audit: ACS weapon and backpack ownership queries (2026-10-02)

Audited native p_acs.cpp CheckInventory and Inventory's default MaxAmount of
one. Found managed queries omitted Fist, Pistol and Backpack and reported
zero maximum for unowned weapons. Converted weapon queries to the existing
shared weapon-name lookup and added backpack queries. Current counts follow
ownership; maximum queries return the vanilla default capacity of one even
when the item is absent.

Added twelve regressions covering all nine vanilla weapons, starting-weapon
removal/regrant, case-insensitive names and backpack acquisition/removal.
Release validation: **4,476 tests passed (3,403 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Remaining gaps include custom inventory maximums, key/armor/health maximum
query parity, native ready/pending weapon object removal, ClearInventory
semantics (currently pistol-start reset) and native executable comparison.
These removal lifecycle gaps require representing the native no-ready-weapon
state. Changes uncommitted.

## Conversion and audit: ACS SetWeapon selection semantics (2026-10-02)

Audited ScriptUtil.SetWeapon while examining inventory removal and selection.
Found managed SetWeapon delegated directly to UseInventory, allowing health,
armor and backpack use and failing to cancel pending selection for the ready
weapon. Converted a weapon-only validation path. Owned ready weapons now
return success and clear Pending even with empty ammo; different owned
weapons still require sufficient ammo and queue through the existing path.
Failed requests preserve the current pending selection. ACS opcode wiring
already calls this helper.

Added ten regressions covering empty ready-weapon cancellation, four invalid
or nonweapon classes, ownership/ammo gates and invalid string indices.
Release validation: **4,464 tests passed (3,391 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Remaining gaps include custom weapon classes, alternate-fire ammo checks,
no-ready-weapon representation, removal of ready/pending weapon inventory
objects and native executable comparison. Changes uncommitted.

## Conversion and audit: backpack capacity preservation (2026-10-02)

Audited BackpackItem.CreateCopy, HandlePickup and DetachFromOwner. Found
managed first-backpack acquisition overwrote larger ammo capacities and
removal unconditionally reset every capacity. Converted first acquisition
to raise each capacity only when below its vanilla backpack maximum.
Removal now resets/clamps only ammo whose current maximum equals that
backpack maximum; other capacities and amounts remain unchanged. Repeated
backpacks continue to add ammo without resetting capacities.

Added five regressions for normal/depleted first acquisition with larger
limits, mixed limits on removal, a changed limit between repeated pickups
and ACS grant/removal integration. Release validation: **4,454 tests passed
(3,381 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include loading custom ammo Default.MaxAmount and
BackpackMaxAmount, native inventory object lifetimes, unlimited pickup and
native executable comparison. This conversion preserves runtime limits in
the supported vanilla ammo model. Changes uncommitted.

## Conversion and audit: ACS ammo and backpack skill scaling (2026-10-02)

Audited ScriptUtil.GiveInventory, Actor.GiveInventory and Ammo.HandlePickup/
CreateCopy plus BackpackItem.CreateCopy/HandlePickup. Native ACS grants use
pickup callbacks with skill scaling; GiveInventory restores the saved pending
weapon when a ready weapon exists. Found managed direct ammo and backpack
grants bypassed scaling. Converted direct ammo to ScaleAmmo and backpack
grants to the existing catalog backpack path. Pending and Selected remain
unchanged in the managed ready-weapon subset. Requested backpack count does
not multiply its ammo: each grant runs one pickup callback, as in native code.

Added thirteen regressions covering all five Doom skills, fractional ammo
factor truncation, DoubleAmmo, zero factor, repeated backpack grants, pending
preservation and normal/backpack ammo caps. Release validation: **4,449 tests
passed (3,376 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with
zero warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include no-ready-weapon and absent-versus-depleted ammo object
semantics, custom ammo classes, unlimited pickup, native inventory callbacks
and native executable comparison. The prior section's direct ACS ammo and
backpack scaling audit is now resolved for the supported vanilla subset.
Changes uncommitted.

## Conversion and audit: ACS weapon grant ammo (2026-10-02)

Audited ScriptUtil.GiveInventory, Actor.GiveInventory, Inventory.SetGiveAmount,
Weapon.AddAmmo/PickupForAmmo and vanilla Doom AmmoGive declarations. Found
that managed ACS weapon grants incorrectly used the requested inventory count
as ammo. Converted pistol/chaingun to 20 bullets, shotgun/super shotgun to 8
shells, rocket launcher to 2 rockets and plasma/BFG to 40 cells, with the
existing skill ammo scaling and caps. Repeated grants use the same default
ammo amount. The current ready-weapon subset preserves Pending and Selected,
consistent with native GiveInventory restoring its saved pending weapon.

Audited Backpack.CreateCopy: its direct ammo additions do not call
CheckWeaponSwitch. Kept the managed backpack path and added coverage for
empty ammo with better owned weapons. Added nine regressions covering all
seven ammo-using weapon classes, repeated grants/pending preservation and
backpack non-switching. Release validation: **4,436 tests passed
(3,363 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Remaining gaps include native no-ready-weapon acquisition (the managed
Selected field cannot represent this state), custom weapon AmmoGive and
replacement classes, deathmatch extra weapon ammo, full ACS inventory object
semantics and native executable comparison. Direct ACS ammo grants and
backpack skill scaling still need their own parity audit. Changes uncommitted.

## Conversion and audit: ammo pickup weapon switching (2026-10-02)

Audited Ammo.HandlePickup, Weapon.PickupForAmmo and PlayerPawn.CheckWeaponSwitch/
BestWeapon, plus vanilla weapon SelectionOrder declarations. Converted ammo
grants from empty to positive to consider switching when fist/pistol is ready,
there is no pending weapon and NeverAutoSwitch is false. Selects the best
owned, usable matching-ammo vanilla weapon with better selection order than
the ready weapon. Catalog ammo and owned-weapon ammo grants use this path;
new weapon acquisition retains its attach-style pending selection.

Added ten regressions for matching-ammo selection, super-shotgun/plasma
priority, never-switch/pending/ready/nonempty gates, insufficient ammo and
owned weapon grants from empty. Updated the previous owned-weapon regression
to use nonempty ammo; its old expectation correctly failed after this native
behavior was converted. Final Release validation: **4,427 tests passed
(3,354 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Custom selection orders/minimum ammo/weapon flags, backpack/ACS inventory
grant switching paths, preference import/persistence, weapon-stay behavior,
native contact ordering/RNG parity and executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: new weapon pickup pending switch (2026-10-02)

Audited Weapon.AttachToOwner in inventory/weapons.zs: a newly attached weapon
becomes PendingWeapon unless GetNeverSwitch or the weapon's no-auto-switch
flag blocks it. Converted vanilla catalog new-ownership grants to queue the
weapon through existing Pending/lower/raise handling, without immediately
changing Selected. Added NeverAutoSwitch preference to the managed inventory;
it suppresses automatic selection while preserving ownership/ammo grants.
Already-owned weapons retain their ammo-only behavior. Nondefault preference
contributes to checksum without changing default baselines.

Added nine regressions for four weapon types, preference suppression,
already-owned pickup, pending replacement, end-to-end contact/lower/raise and
checksum behavior. Release validation: **4,417 tests passed (3,344 Playsim;
500 MapLoader)**. Warnings-as-errors build passed with zero warnings/errors.
Touched-file whitespace validation passed.

Custom weapon no-auto-switch flags, ammo-triggered switching and selection
order, weapon-stay behavior, external player preference import/persistence,
native contact ordering/RNG parity and executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: patched player contact defaults on rebirth (2026-10-02)

Continued native SpawnPlayer class-default rebirth conversion. Map spawning
now captures initial PICKUP/SPECIAL defaults after explicit Dehacked bits
are applied. Successful player rebirth restores those captured defaults,
replacing the prior hardcoded vanilla reset. Current flag save restoration
does not overwrite spawn defaults. Nonvanilla player contact defaults also
contribute to checksum because they affect future rebirth behavior.

Added five regressions for all patched bit combinations through runtime
mutation, archive restoration, death/rebirth and subsequent collection,
plus checksum separation when current flags match but rebirth defaults differ.
Final Release validation: **4,408 tests passed (3,335 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Spawn defaults derive from the loaded map/patch and are not serialized as
standalone class metadata. Custom player classes, other Dehacked flags,
full player replacement, native contact ordering/RNG parity and executable
acceptance remain open. Changes remain uncommitted.

## Conversion and audit: explicit Dehacked pickup/contact bits (2026-10-02)

Audited native actor.h MF_SPECIAL (0x1) and MF_PICKUP (0x800), managed numeric
Dehacked Bits parsing and map actor spawning. Added explicit BitsPatched
tracking so numeric Bits = 0 is distinct from an omitted Bits field. Map actor
spawning now imports the two converted contact bits when explicitly patched.
Patches affecting only health/size retain existing pickup defaults. Existing
contact checks, ACS controls, checksum and current archive handle these flags.

Added six regressions for all combinations of the two bits, explicit zero,
collection eligibility, omitted Bits and a remapped catalog pickup with
SPECIAL cleared. Release validation: **4,403 tests passed (3,330 Playsim;
500 MapLoader)**. Warnings-as-errors build passed with zero warnings/errors.
Touched-file whitespace validation passed.

Other Dehacked bits and symbolic expressions, patched player contact defaults
through rebirth, dropped/replacement class patch inheritance, non-player
inventory collection, native RNG parity and executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: player respawn contact defaults (2026-10-02)

Audited native G_DoReborn/SpawnPlayer paths in g_game.cpp and PlayerPawn's
PICKUP class default. Native rebirth uses a newly spawned player actor; the
managed respawn path reuses the existing actor. Converted successful managed
rebirth to restore converted contact defaults (PICKUP enabled, SPECIAL
disabled) before player respawn scripts. Script changes made before death no
longer suppress item collection after successful rebirth. Blocked respawn
retains current flags; other flag reset/reconstruction remains outside scope.

Added four regressions covering supported single-player in-place respawn,
cooperative/deathmatch rebirth and blocked respawn, including collection after
rebirth. Release validation: **4,397 tests passed (3,324 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Custom player class defaults, full actor replacement/other flag defaults,
native inventory/respawn parity, movement contact ordering, RNG parity and
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: default contact flag restore completion (2026-10-02)

Closed the prior contact-flag archive limitation: CaptureState now records
PICKUP/SPECIAL for every actor even when values match spawn defaults. Current
nonempty actor archives therefore use version 18. A save made before scripts
change flags now restores the saved defaults and permits collection again.
Legacy archives without contact flags continue preserving current values.

Updated the default-restore regression to verify subsequent collection and
added an explicit legacy-preservation regression. Updated pitch/roll/pickup
archive tests for current version 18, and retained explicit older fixtures for
legacy trailer corruption/compatibility tests. The first targeted run caught
34 archive assertions relying on the prior version/fixture shape; updated
those fixtures without weakening their legacy checks. Final full Release
validation: **4,393 tests passed (3,320 Playsim; 500 MapLoader)**. After the final
collection assertion, all 3,320 Playsim tests passed again. Warnings-as-errors
build passed with zero warnings/errors. Touched-file whitespace validation
passed.

Other actor flags, missing dynamic actor recreation, native archive format,
Dehacked flag import, native contact ordering/RNG parity and executable
acceptance remain open. Changes remain uncommitted.

## Conversion and audit: modified pickup/contact flag archive (2026-10-02)

Continued native actor flag persistence conversion (AActor.Serialize flags
in p_mobj.cpp). Added optional managed archive version 18 for PICKUP/SPECIAL
contact flags. When any actor differs from its spawn default, capture includes
both flags for every actor and the trailer preserves the underlying version
16 or 17 archive. Restore applies flags onto matching actors. Writer/restore
validation rejects invalid or incomplete flags before mutation; reader checks
trailer shape, prior version, actor count and unknown bits.

Added seven regressions for changed flags and collection after restore,
checksum continuation, invalid bits, all truncations and legacy/default
archive behavior. Release validation: **4,392 tests passed (3,319 Playsim;
500 MapLoader)**. Warnings-as-errors build passed with zero warnings/errors.
Touched-file whitespace validation passed.

All-default captures intentionally retain prior archive versions and legacy
behavior (flags absent from those archives leave current flags unchanged).
Full actor-flag/native archive parity, missing dynamic actor recreation,
Dehacked flag import, native movement contact ordering, native RNG parity
and executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: native SPECIAL item contact flag (2026-10-02)

Audited MF_SPECIAL pickup gate in PIT_CheckThing and inventory.zs world-item
special flag handling. Added Actor.SpecialPickup with catalog map/drop and
depleted backpack defaults enabled. Collection now requires this flag before
granting or applying always-pickup fallback. Added case-insensitive ACS
CheckActorFlag/ModActorFlag SPECIAL support. Nondefault flag values contribute
to checksum while preserving existing default baselines. Direct inventory
grants retain their separate behavior.

Added five regressions for disabling/re-enabling ammo and bonus contact,
player/item spawn defaults, dropped weapons/depleted backpacks and checksum
changes. Release validation: **4,385 tests passed (3,312 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

General actor flag save persistence (including PICKUP/SPECIAL), Dehacked flag
import, custom inventory lifecycle, native movement contact ordering, native
RNG parity and executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: native PICKUP eligibility flag (2026-10-02)

Audited MF_PICKUP gate in PIT_CheckThing and PlayerPawn's +PICKUP default
in player.zs. Added Actor.CanPickupItems, enabled by default for players and
disabled for ordinary actors. Managed player contact collection now requires
the flag before attempting grants or always-pickup fallback. Added ACS
CheckActorFlag/ModActorFlag support for case-insensitive PICKUP. Nondefault
flag values affect checksum while preserving existing default baselines.

Added five regressions for disabling/re-enabling ammo and bonus collection,
default ordinary actor behavior, destroyed actor flag rejection and checksum
changes. Release validation: **4,380 tests passed (3,307 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Non-player inventory collection, native special-item flags/class restrictions,
general actor-flag archive persistence (including this flag), native movement
contact ordering, native RNG parity and executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: always-pickup Doom bonuses (2026-10-02)

Audited INVENTORY.ALWAYSPICKUP on Doom HealthBonus and ArmorBonus and the
failed-grant fallback in inventory.zs. Converted collection to consume those
bonus actors even when health/armor is already at or above the cap. Existing
grant logic preserves current values; ordinary health/armor pickups still
remain when they offer no benefit. TryGive retains its grant-result contract;
the collection layer handles the native always-pickup fallback.

Added eight regressions for both bonuses at/above cap and ordinary stimpack,
medikit, green armor and mega armor rejection at full values. Release
validation: **4,375 tests passed (3,302 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Custom inventory flags/ShouldStay, pickup messages/sounds and item counters,
weapon pickup switching/stay behavior, native contact ordering, native RNG
parity and executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: native catalog pickup heights (2026-10-02)

Audited Actor defaults (Radius 20, Height 16) in actor.zs, DoomKey/armor
dimensions and Backpack's Height 26 override in doomammo.zs. Converted
unpatched map catalog pickups, spawned dropped pickups and depleted backpack
drops from managed generic Height 56 to native pickup heights. Existing map
Dehacked actor dimensions retain precedence. Pickup radii already use 20.
This corrects vertical reach and movement geometry for these item actors.

Added seven regressions covering map/drop dimension agreement for ammo,
weapons, skull keys, health, cell packs and backpacks, plus depleted backpack
height. Release validation: **4,367 tests passed (3,294 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

General/custom inventory dimensions, native movement-time pickup contact
ordering and temporary-height compensation, full dynamic actor restoration,
native RNG parity and executable acceptance remain open. Changes remain
uncommitted.

## Conversion and audit: native horizontal pickup contact (2026-10-02)

Audited PIT_CheckThing in p_map.cpp: contact rejects absolute X or Y distance
greater than or equal to the sum of radii. Converted managed pickup collection
from inclusive circular overlap to strict square axis contact. This accepts
native diagonal contacts and rejects equality at either edge. Other collision
helpers retain their existing behavior; pickup vertical reach remains separate.

Added eleven regressions covering diagonal contact, positive/negative edges,
one fixed-point unit inside/outside an edge and simulation-level collection.
Release validation: **4,360 tests passed (3,287 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Native movement-time contact ordering/blockmap traversal, the temporary
height/MaxStepHeight compensation gate before P_TouchSpecialThing, custom
Touch behavior, native RNG parity and executable acceptance remain open.
This converts the horizontal contact geometry rather than the complete
native movement contact pipeline. Changes remain uncommitted.

## Conversion and audit: native pickup vertical reach (2026-10-02)

Audited P_TouchSpecialThing in p_interaction.cpp. Native reach accepts pickup
Z minus toucher Z from min(-32, -pickup Height) through toucher Height,
including both endpoints. Managed collection used strict cylinder vertical
overlap, rejecting the upper endpoint and reachable items below the feet.
Separated the existing horizontal overlap calculation and applied the native
vertical reach specifically to pickups; other cylinder-overlap callers retain
their prior behavior. Dead-player rejection remains in the collection loop.

Added ten regressions for exact inclusive limits, one fixed-point unit beyond
each limit, tall/zero-height pickups and simulation-level collection. The
upper boundary regression caught the old strict overlap gate during validation;
corrected the collection path. Final Release validation: **4,349 tests passed
(3,276 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Native horizontal/blockmap contact ordering, custom pickup Touch behavior,
full pickup class properties, native RNG parity and executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: ACS DropItem attempt-count result (2026-10-02)

Audited ACSF_DropItem in p_acs.cpp: cnt increments for each actor passed to
P_DropItem, independent of chance failure or the returned spawned actor.
Corrected managed DropItem to report actors processed rather than successful
pickup spawns. Invalid catalog names and missing activators/TIDs still return
zero. Existing chance checks and safe multi-actor snapshot iteration remain.

Added six regressions covering multi-TID results across failed, intermediate
and guaranteed chances, failed activator chance, and the ACS dispatcher result.
Updated existing chance regression to assert spawn counts separately from the
attempt result. Corrected test API/stack setup errors during validation.
Final Release validation: **4,339 tests passed (3,266 Playsim; 500 MapLoader)**.
Warnings-as-errors build passed with zero warnings/errors. Touched-file
whitespace validation passed.

Custom class lookup/replacement, native dedicated RNG parity, exact spawn
clipping/vertical integration and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: case-insensitive ACS drop class lookup (2026-10-02)

Audited PClass.FindActor string overloads in common/objects/dobjtype.h and
FName case normalization in utility/name.cpp. Converted supported catalog
drop names to invariant case normalization before lookup. Existing aliases
and whitespace trimming remain supported, and separate skull/card identity
is preserved. Unknown and empty names still fail lookup.

Added ten regressions, including exhaustive upper/lower-case lookup checks
for all 39 supported names/aliases, mixed-case ACS spawning, Turkish-culture
independence and invalid inputs. Release validation: **4,333 tests passed
(3,260 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

General/custom class lookup and replacement, exact native non-ASCII name
handling, separate card/skull inventory semantics, native RNG parity and
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: ACS dropped key class identity (2026-10-02)

Audited ACSF_DropItem class lookup in p_acs.cpp and separate Blue/Red/Yellow
Card and Skull classes in Doom doomkeys.zs. Corrected managed drop-name
mapping: skull requests now spawn their own skull editor type instead of
the same-color card. Card requests retain their original identity. Managed
inventory continues its documented shared color-key representation.

Added six regressions covering every card/skull class through ACS drop
spawning and subsequent collection, checking actor identity and only the
matching inventory key color. Release validation: **4,323 tests passed
(3,250 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Native case-insensitive/general class lookup, custom key classes, separate
card/skull inventory semantics, native RNG parity and executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: native drop style 2 (2026-10-02)

Audited Actor.A_DropItem/TossItem in inventory_util.zs, sv_dropstyle in
p_enemy.cpp and FRandom.Random2(mask) in common/engine/m_random.h. Added
simulation DropStyle selection: 0 uses the current Doom default; 2 uses
source Z + 24, horizontal random2(7) velocities and no vertical toss.
The native parameter 7 is a mask, not a shift. Style 2 uses four toss draws;
other styles follow the default branch. NoTossDrops overrides both height
offset and toss randomness. Explicit style values contribute to checksum;
the default value preserves the existing idle checksum baseline.

Added nine regressions for exact masked velocities and height, draw counts,
initial falling, no-toss override, fallback styles and setting/checksum
behavior through pose restore. Initial validation caught an idle checksum
baseline change; corrected default checksum handling. Final Release validation:
**4,317 tests passed (3,244 Playsim; 500 MapLoader)**. Warnings-as-errors build
passed with zero warnings/errors. Touched-file whitespace validation passed.

DropStyle is a simulation setting like existing server settings; the pose
archive preserves the current setting rather than serializing it. MAPINFO
game-default style import, native dedicated RNG streams, class velocity
defaults, native spawn clipping/vertical integration, dynamic actor restore
and native executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: no-toss drop compatibility (2026-10-02)

Audited COMPATF_NOTOSSDROPS branches in Actor.A_DropItem in inventory_util.zs.
Added managed CompatSurface.NoTossDrops: drops use the source Z without the
half-height offset or randomized toss. Gravity remains enabled and pickup
amount adjustment is unchanged. Toss RNG draws are skipped; ACS and vanilla
death drop chance draws still occur. The existing compatibility checksum
includes the new flag.

Added six regressions for grounded/elevated/fractional source Z, zero momentum,
interpolation, draw accounting, ammo amount, vanilla death drops and flag
composition/checksum. Release validation: **4,308 tests passed (3,235 Playsim;
500 MapLoader)**. Warnings-as-errors build passed with zero warnings/errors.
Touched-file whitespace validation passed.

Alternate drop style 2, native compatibility setting import, dedicated RNG
streams, native spawn clipping/vertical integration, full dynamic actor
restoration and native executable acceptance remain open. This flag is
selected through the existing simulation compatibility argument. Changes
remain uncommitted.

## Conversion and audit: default Doom dropped-pickup toss velocity (2026-10-02)

Audited Actor.TossItem in inventory_util.zs. Converted default style-1 toss:
horizontal velocity uses two random-byte differences divided by 256 per axis,
and vertical velocity is 5 plus one random byte divided by 64. Spawning uses
five draws after any chance draw. Source momentum is not inherited. The
existing managed gravity path carries the drop through ascent and landing.

Added five regressions for exact formulas, velocity bounds, draw consumption,
source-momentum independence and invalid pickup rejection. Updated chance
tests to account for successful toss draws, and the drop-height movement test
to expect initial ascent before landing. Release validation: **4,302 tests
passed (3,229 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with
zero warnings/errors. Touched-file whitespace validation passed.

Dedicated native DropItem RNG sequence parity remains open: this uses the
managed shared combat generator. Alternate style 2, COMPATF_NOTOSSDROPS,
native vertical integration/spawn clipping, custom class velocity defaults,
full dynamic actor restoration and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: default Doom dropped-pickup spawn height (2026-10-02)

Audited Actor.A_DropItem in inventory_util.zs and defaultdropstyle = 1 in
mapinfo/doomcommon.txt. Converted the default Doom height calculation:
source Z plus half source Height. Managed spawning formerly reset every
drop to the sector floor. Sector initialization now precedes restoring the
requested spawn Z, updating grounded status and remembering interpolation
position. Existing ACS, vanilla shatter and player weapon drop callers use
this common spawning path. Existing worktree changes were preserved as the
user-authorized baseline.

Added six regressions covering elevated sources, raised/negative floors,
zero and fractional heights, interpolation position, gravity eligibility,
and subsequent falling/landing. Release validation: **4,297 tests passed
(3,224 Playsim; 500 MapLoader)**. Warnings-as-errors build passed with zero
warnings/errors. Touched-file whitespace validation passed.

Randomized toss velocities, alternate drop style 2, COMPATF_NOTOSSDROPS,
native spawn clipping, dedicated DropItem RNG, full dynamic actor restore
and native executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: drop chance boundaries and safe ACS iteration (2026-10-01)

Audited Actor.A_DropItem and TossItem in inventory_util.zs. Native chance
accepts random[DropItem]() <= chance, including equality and zero when the
random byte is zero. It draws even for guaranteed chance 256. Converted the
managed chance comparison and draw consumption; vanilla shatter drops now
use the same chance path. The managed generator remains the shared combat
generator, not the native dedicated DropItem stream.

The new multi-TID regression exposed collection invalidation: successful
spawning appended to the list being enumerated. Snapshotting matching actors
before spawning fixes the exception and processes each original match once.
Added ten regressions for negative/zero/intermediate/guaranteed chances,
equality, zero-byte success, per-match draws, unresolved types and missing
droppers. The first test run caught the iteration bug; after fixing it,
**4,291 tests passed (3,218 Playsim; 500 MapLoader)**. Warnings-as-errors build
passed with zero warnings/errors. Touched-file whitespace validation passed.

Native source also confirms drop-style-dependent height and randomized toss
velocity, gated by COMPATF_NOTOSSDROPS. Those movement behaviors remain open,
along with dedicated RNG streams, missing dynamic actor restoration, custom
drop classes and native executable acceptance. Changes remain uncommitted.

## Conversion and audit: pickup property archive persistence (2026-10-01)

Audited native inventory Amount and BackpackItem.bDepleted fields, actor flag
serialization in p_mobj.cpp, and DObject.SerializeUserVars/WriteAllFields.
The managed pose archive omitted PickupAmount, IgnoreAmmoSkill and Depleted.
Added an optional version-17 pickup trailer over the existing version-16
archive, capturing those properties for catalog pickups and restoring them
onto matching actor IDs. Default values are saved explicitly, so restoring
can clear changed properties. Versions 1-16 retain their existing behavior.
Archives without catalog pickups retain their prior version.

Added sixteen regressions for exact property round trips, checksum and ammo
continuation, depleted backpack behavior, a matching spawned weapon drop,
legacy preservation, malformed flags/amounts/sizes/counts, truncation and
negative-amount rejection before restore mutation. Release validation:
**4,281 tests passed (3,208 Playsim; 500 MapLoader)**. Warnings-as-errors build
passed with zero warnings/errors. Touched-file whitespace validation passed.

This remains the managed pose archive, not the native save format. Missing
dynamic actors are not recreated; full actor/inventory/thinker reconstruction,
custom drop properties, toss behavior and native executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: native default dropped-ammo amounts (2026-10-01)

Audited Actor.A_DropItem in inventory_util.zs and Ammo/Weapon.ModifyDropAmount
in inventory/ammo.zs and inventory/weapons.zs. Converted the vanilla default
drop factor: ammo drops halve their regular amount with a minimum of one;
weapon drops halve their ammo grant. Positive explicit ammo amounts retain
their requested amount, while weapon inventory amounts do not override ammo.
Pickup applies skill/server ammo scaling after this adjustment, including
explicit ammo amounts. Existing player weapon-drop ignore-skill behavior is
preserved. Uses the existing PickupAmount field and checksum contribution.

Added eight regressions: four frozen-monster drops collected through simulation,
and doubled-ammo cases covering default clips, minimum-one rockets, explicit
ammo and weapon amounts. Release validation: **4,265 tests passed (3,192 Playsim;
500 MapLoader)**. Warnings-as-errors build passed with zero warnings/errors.
Touched-file whitespace validation passed.

Custom skill DropAmmoFactor, DropAmmoFactorMultiplier, custom DropAmount,
drop toss/height, class replacement, dropped pickup property save persistence,
ordinary death-state drop wiring and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: vanilla frozen-monster item drops (2026-10-01)

Audited A_Unblock/A_NoBlocking in native a_action.cpp and drop declarations
in Doom possessed.zs. Converted standard item drops at managed shatter
completion: ZombieMan/WolfensteinSS drop Clip, ShotgunGuy drops Shotgun,
ChaingunGuy drops Chaingun. Reused existing dropped-pickup spawning before
corpse removal. Delayed shattering drops nothing and removed corpses cannot
repeat drops. The helper excludes players and is limited to vanilla editor types.

Added six regressions for four native drop mappings, unchanged pickup spawn
coordinates/flags, exactly-once drops, delay and a monster without a drop.
Adjusted the debris-only trace test to keep newly dropped pickups outside its
ray. Release validation: **4,257 tests passed (3,184 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
An initial network attack test timed out; subsequent full-suite runs passed.

General/custom drop metadata, conversation drops, class replacement, ordinary
death-state drop wiring, dropped-item native toss/ammo properties, corpse queue,
player head transfer, shatter timing, RNG parity and executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: completed shatter corpse removal (2026-10-01)

Audited the end of native A_FreezeDeathChunks in shared/ice.zs: after debris,
player-head and boss handling, it performs A_NoBlocking and enters null state.
Converted the ordinary managed completion path to clear collision/shootability
and enter the removal state after spawning debris. Destroyed corpses cannot
repeat spawning. The moving-corpse delay returns before removal, retaining its
blocking flags and active actor identity.

Added three regressions for stationary/forced completion, null state and flags,
repeat spawn/damage without extra shards or RNG consumption, thinker cleanup
after ticking, and retention of a delayed moving corpse.
Release validation: **4,251 tests passed (3,178 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Player head/camera/inventory transfer, boss actions and full A_NoBlocking item
drop behavior remain open. Damage-triggered shattering still spawns immediately
instead of waiting for the native next state action. GenericFreezeDeath wiring,
RNG parity, terrain timing, dynamic actor archival and executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: moving-corpse shatter delay (2026-10-01)

Audited A_FreezeDeathChunks in native shared/ice.zs. Converted the entry gate:
when velocity is nonzero and Shattering is unset, the operation resets current
state tics to 3*TICRATE (105) and returns before clearing velocity or drawing
random values. Stationary corpses or explicitly forced shattering proceed.
Updated momentum-reset regressions to identify their forced-shatter scenario.

Added four regressions for motion on each axis, unchanged frame/velocity/random
state and actor count during delay, forced bypass after delay, and stationary
immediate spawning. Existing damage-triggered shatter coverage still passes.
Release validation: **4,248 tests passed (3,175 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

This converts the shared operation's gate, not the full GenericFreezeDeath
state/action wiring or natural timed shatter lifecycle. Dedicated RNG streams,
terrain timing, head chunks, complete vertical physics, dynamic actor recreation
and native executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: shatter operation clears corpse momentum (2026-10-01)

Audited native A_FreezeDeathChunks in shared/ice.zs: after the moving-corpse
delay gate, the shatter operation clears all corpse velocity before spawning
shards. The managed damage caller already cleared velocity, but SpawnIceChunks
itself depended on that caller. Moved the native stop rule into the spawning
operation as well, preserving corpse coordinates and independent shard velocity.

Added three regressions for positive/negative and vertical-only corpse motion.
Moving and stationary corpses with identical managed random state produce equal
post-shatter checksums and remain equal after ticking. Tests verify stopped
corpse velocity, unchanged origin, moving shards and equal random consumption.
Release validation: **4,244 tests passed (3,171 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

The pre-shatter moving-corpse delay gate, dedicated random streams, terrain
timing, head chunks, full vertical physics, dynamic actor recreation and native
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: ice debris randomized initial frame (2026-10-01)

Audited A_FreezeDeathChunks in native shared/ice.zs: spawned IceChunk enters
SpawnState plus a random offset in 0-2, running that frame's A_IceSetTics.
Converted selection among the first three frames in managed shatter spawning,
including duration resampling on selected frame zero. Selection happens before
horizontal velocity draws. Later state progression now naturally runs only the
frames remaining after the selected starting state rather than always all four.

Added seven regressions for all three starting frames at both duration bounds,
exact expiry, production shatter coverage of frames 0-2 and deterministic
two-simulation checks before/after ticking.
Release validation: **4,241 tests passed (3,168 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Selection and timing still use the existing managed combat stream rather than
native independent FreezeDeathChunks/IceTics streams. Terrain-dependent timing,
full vertical physics, head chunks, dynamic actor recreation and native
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: ice debris four-frame lifecycle (2026-10-01)

Audited IceChunk's ICEC ABCD sequence and A_IceSetTics in native shared/ice.zs.
Replaced the managed one-duration hidden lifetime with four timed actor states,
then removal. Subsequent simulated frame entries resample 70-133 tics using the
current managed random stream. RemainingTics now reports current-frame progress,
and existing actor pose state/tics determine expiry for an already-present chunk.
Constructor duration remains the initial frame duration; random initial frame
selection is not yet converted.

Added six tests for duration endpoints, all four frames, simulated resampling,
restored final-frame expiry and invalid duration rejection. Updated the prior
expiry test to restore the final frame rather than treat the first as terminal.
Release validation: **4,234 tests passed (3,161 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Dedicated IceTics RNG and its archive state, terrain Fire/Ice scaling, initial
spawn-frame selection, full vertical physics, head chunks, dynamic actor
recreation and native executable acceptance remain open. Resampling still
shares the existing managed combat stream; native RNG parity is not claimed.
Changes remain uncommitted.

## Conversion and audit: signed ice spawn offsets and velocity scaling (2026-10-01)

Audited A_FreezeDeathChunks in native shared/ice.zs. Corrected managed XY spawn
offsets to convert the random byte to signed integer before subtracting 128;
unsigned subtraction previously wrapped negative offsets into huge coordinates.
Converted horizontal ice velocity scaling from weapon spread's 255 divisor to
the native Random2 / 128 scale. Kept the same managed random draws and left
weapon spread unchanged. Dedicated native FreezeDeathChunks RNG remains open.

Added two regressions with different corpse dimensions at a nonzero XYZ origin.
Tests verify every shard lies within corpse bounds, spreads on both XY sides,
retains previous position, and has horizontal/vertical velocity in native bounds.
The deterministic sample also exercises horizontal speeds above one, detecting
the prior weapon-spread scaling. Existing two-simulation shatter determinism
coverage continues to pass.
Release validation: **4,228 tests passed (3,155 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Native random stream/order, randomized spawn frame, terrain-dependent lifetime,
head chunks, full vertical physics, dynamic actor archival and executable
acceptance remain open. Changes remain uncommitted.

## Conversion and audit: ice debris gravity factor (2026-10-01)

Audited IceChunk's Gravity 0.125 default in shared/ice.zs, AActor::GetGravity
in actorinlines.h and P_ZMovement in p_mobj.cpp. Converted the inherent ice
actor gravity multiplier within the existing managed physics model. Ordinary
actors retain factor one; ice debris uses one eighth. NoGravity still bypasses
acceleration, and resting debris does not acquire downward velocity. The factor
is derived from actor type, so it introduces no mutable pose field or RNG state.

Added eight regressions for rising/resting/falling velocity, NoGravity override,
two successive ticks, unchanged ordinary gravity and grounded debris.
Release validation: **4,226 tests passed (3,153 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

This converts the actor multiplier, not complete native vertical physics.
Managed gravity/movement order, native FallAndSink, level/sector gravity,
runtime actor gravity properties, terrain lifetime, IceTics RNG, head chunks,
dynamic actor archival and executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: ice debris movement, spawn height and query exclusion (2026-10-01)

Audited IceChunk defaults in native shared/ice.zs and blockmap actor linkage/
trace iteration in p_maputl.cpp. Converted inherent ice debris NOBLOCKMAP query
exclusion. Managed IceChunk.Tick also called ActorPhysics.Step explicitly before
base actor ticking called it again; removed the extra movement pass so debris
advances once and previous-position tracking retains the actual tick origin.
SpawnIceChunks now restores its requested randomized Z after sector placement
and sets airborne contact correctly rather than snapping every chunk to floor.

Added six regressions for positive/negative/stationary horizontal velocity,
single upward movement, previous-position tracking, expiry without movement,
elevated randomized spawn heights and all-actor/solid-mask query exclusion.
Release validation: **4,218 tests passed (3,145 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

IceChunk's native 0.125 gravity factor, terrain-dependent lifetime, IceTics RNG,
head chunks, dynamic actor archival, general NOBLOCKMAP flag handling,
advanced geometry and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: puff blockmap query exclusion and dimensions (2026-10-01)

Audited BulletPuff's NOBLOCKMAP default in doommisc.zs, inherited Actor radius
20/height 16 in actor.zs, and blockmap linkage/thing traversal in p_maputl.cpp.
Managed combat tracing previously scanned cosmetic puffs, allowing all-actor or
NoGravity masks to select them before a real target. Added inherent blockmap
participation to managed actor types and excluded nonparticipating puff actors
before trace mask/box evaluation. Puffs retain actor/TID registration; production
ACS PickActor can no longer reassign a cosmetic puff's TID by tracing it.
Converted inherited puff dimensions, preserving the geometry-specific epsilon
radius applied after impact spawning.

Added six regressions for all-actor/NoGravity masks, later targets versus misses,
trace selection, ACS TID assignment/stack behavior and zero-distance containing
puffs. Release validation: **4,212 tests passed (3,139 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

General runtime NOBLOCKMAP flag handling, native blockmap traversal/order,
randomization, temporary/blood puff classification, custom classes, dynamic
actor archival, advanced geometry and executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: ACS hitscan puff inflictor and damage source (2026-10-01)

Audited actor-hit P_LineAttack in native p_map.cpp and P_SpawnPuff in p_mobj.cpp.
Native damage uses the puff as inflictor while retaining the shooter as source;
spawned puffs retain a separate damagesource reference. Corrected ACS helper
damage to pass the puff as inflictor and added the source reference to actor and
geometry puffs. Shooter attribution remains unchanged. Shooter-only ForcePain,
Painless and FoilBuddha flags no longer leak into ordinary BulletPuff damage.

Added five regressions through production ACS invocation for source ForcePain,
Painless and FoilBuddha isolation, Buddha survival, shooter attribution, and
actor/geometry puff source references. Release validation: **4,206 tests passed
(3,133 Playsim; 500 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Native temporary versus visible actor puffs, blood classification, puff source
reference archival, weapon/monster inflictor wiring, randomization, melee state
selection, custom classes, advanced geometry and executable acceptance remain
open. ACS puff/decal flags were audited but their remaining unsupported effects
are not claimed converted. Changes remain uncommitted.

## Conversion and audit: bullet puff state-frame lifecycle (2026-10-01)

Audited BulletPuff Spawn/Melee frames in native doommisc.zs and first-frame
randomization in P_SpawnPuff in p_mobj.cpp. Replaced the managed two-tic hidden
timer with the existing actor state machine: four frames, four tics each by
default, first-frame full brightness, remaining frames unlit, then removal.
The first frame accepts the native randomized duration bounds of one to four
tics; production currently uses four pending the dedicated SpawnPuff RNG port.
Total frame lifetime is therefore 13-16 tics, with the unrandomized default 16.
Also converted BulletPuff's mass of five.

Lifetime derives from current state/tics, removing an independent counter that
would disagree with restored state progress. Existing pose fields can represent
this progress for an already-present puff; recreation and full cosmetic state
restoration are not complete. Updated the prior two-tic expiry regression.
Added seven tests for all first-frame durations, frame progression/brightness,
expiry, restored timer progress and invalid duration rejection.
Release validation: **4,201 tests passed (3,128 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Random first-frame selection, random puff Z, SpawnPuff RNG/archive state,
melee state selection, sprite/render properties, actor blood/puff classification,
custom classes, dynamic actor recreation, advanced geometry and native executable
acceptance remain open. Changes remain uncommitted.

## Conversion and audit: bullet puff drift and facing (2026-10-01)

Audited BulletPuff defaults/states in native doommisc.zs and P_SpawnPuff in
p_mobj.cpp. Converted the default upward VSpeed of one unit per tick and
opposite-shot yaw (hitdir + 180 degrees). Managed puffs now use normal actor
movement rather than the empty movement override; NoGravity and noncolliding
flags remain intact. ACS actor and geometry puffs receive the native facing,
including negative-range shots whose hit direction remains the original angle.

Added eight regressions for four cardinal shot directions with positive and
negative ranges, checking exact BAM facing, upward drift, stationary XY,
NoGravity, collision flags, TID and single-hit damage.
Release validation: **4,194 tests passed (3,121 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

The current two-tic managed lifetime remains a placeholder. Native BulletPuff
has four four-tic frames and randomizes the first frame; random Z/lifetime,
dedicated SpawnPuff RNG/archive state, actor blood/puff classification, custom
puff classes, SKYEXPLODE/ALWAYSPUFF, weapon/monster puff creation, advanced
geometry and native executable acceptance remain open. Changes are uncommitted.

## Conversion and audit: geometry puff placement and horizon suppression (2026-10-01)

Audited the geometry-hit branch of P_LineAttack in native p_map.cpp. Converted
normal horizon-wall puff suppression while preserving the stopping wall trace
and existing geometry damage immunity. Ordinary geometry puffs now spawn four
units behind the impact along the original shot vector, including pitch and
negative-range shots, and use native EQUAL_EPSILON radius. Geometry damage is
applied before puff creation, matching native ordering.

New coordinate tests exposed that SpawnHitscanPuff's floor placement overwrote
the supplied Z coordinate. It now retains sector placement information but
restores requested impact Z before remembering position and registering the puff.
This also fixes actor-hit puff height without changing its other cosmetics.

Added eight regressions covering horizontal directions, positive/negative
ranges, pitched floor/ceiling offsets, puff radius/TID, and horizon suppression
with/without a requested TID. Release validation: **4,186 tests passed
(3,113 Playsim; 500 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Actor blood/puff classification, random puff Z/lifetime, custom puff classes,
SKYEXPLODE/ALWAYSPUFF, weapon/monster puff creation, callbacks, blockmap ties,
compatibility, autoaim, slopes/portals/3D floors, full AI archives and native
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: standard sky hitscan suppression (2026-10-01)

Audited EditTraceResult in native p_trace.cpp and the default TRACE_NoSky flags
in P_LineAttack/P_LinePickActor in p_map.cpp. Converted suppression of standard
F_SKY1 floor/ceiling impacts and upper-wall impacts with sky ceilings on both
sides. Lower-wall and one-sided impacts remain ordinary geometry hits. A
discarded sky result preserves impact coordinates, stops selection through that
boundary, and reports no hit, preventing normal attack damage and puff creation.
Reused the standard sky-texture helper for existing plane damage protection.

Added six regressions for sky floor/ceiling, case-insensitive texture names,
both-sky upper walls and mixed sky/ordinary ceilings. Tests verify classification,
coordinates, blocked actor selection, puff count, wall and sector health.
Release suite: **4,178 tests passed (3,105 Playsim; 500 MapLoader)**. After adding
the explicit one-sided exclusion, all six targeted sky tests passed again.
Final build with warnings treated as errors passed with zero warnings/errors.

Custom sky aliases, SKYEXPLODE/ALWAYSPUFF classes, callbacks, blockmap ties,
compatibility, autoaim, slopes/portals/3D floors, full AI archives and native
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: combat trace miss endpoints (2026-10-01)

Audited the TRACE_HitNone completion path in native TraceTraverse in p_trace.cpp.
When traversal selects no actor, line or plane, native trace results still report
Start + Vec * MaxDist as the endpoint. Managed tracing previously returned a
default result with zero coordinates. Valid misses now retain their signed-range
endpoint while Hit remains false and actor/wall/plane metadata remains empty.
Invalid inputs retain the existing validation behavior.

Added eight regressions for cardinal yaw, ascending/descending pitch, negative
ranges and zero distance from a nonzero XYZ origin. Cases verify endpoint
coordinates and classification, PickActor misses, absence of attack damage/puff
creation, and unchanged random state. Release validation: **4,172 tests passed
(3,099 Playsim; 500 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Callbacks, blockmap ordering/ties, compatibility, autoaim, slopes/portals/3D
floors, full AI archives and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: ordered flat-sector transitions (2026-10-01)

Audited LineCheck, TraceTraverse and CheckPlane in native p_trace.cpp. Converted
the current flat-geometry subset to visit line intercepts in distance order,
carry CurSector across open two-sided lines, and record signed EnterDist.
Near-side selection now prefers the current sector before native geometric
fallback; one-sided fallback uses the front side. Plane classification at a line
uses the current sector and requires distance strictly after EnterDist and
before MaxDist. Failed classifications retain the destination-opening fallback.
After unsuccessful traversal, plane checks use the final current sector rather
than searching all sectors by sampled XY position. Actors entered before a
stopping line remain eligible under the existing native actor-entry ordering.

Added four regressions using deliberately reversed line storage order, both
horizontal directions and floor/ceiling entry boundaries. Ceiling entry equality
does not block a later open destination; descending floor arithmetic rounds
below the opening and stops at the first line. Tests verify query selection,
classification and attack damage. Corrected older one-sided wall-side and
rounded ceiling/wall expectations to follow native classification operations.
Release validation: **4,164 tests passed (3,091 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

This converts CurSector/EnterDist for ordinary flat-sector lines; callbacks,
blockmap ordering/ties, compatibility, autoaim, slopes/portals/3D floors,
full AI archives and native executable acceptance remain open. Exact floating
boundary parity still requires executable comparison. Changes remain uncommitted.

## Conversion and audit: pitched failed-plane opening fallback (2026-10-01)

Audited LineCheck's cont path and CheckPlane in native p_trace.cpp. When a
near-sector floor/ceiling classification cannot produce a plane intersection
strictly after EnterDist and before MaxDist, a two-sided line falls back to
the destination floor/ceiling opening. In that path, wall-mask flags are ignored.
Managed code previously applied this fallback only to horizontal or negative
shots. It now also applies to pitched shots whose plane intersection is at the
origin, behind the origin, or outside the signed range. The check uses the
original vertical direction and signed range for negative traversal.

Added sixteen regressions for pitched shots beginning exactly on the near
floor/ceiling, both traversal directions, open/closed destination sectors,
and lines with/without hitscan-blocking flags. Cases verify actor selection,
PickActor, trace classification, actor damage and wall health.
Release validation: **4,160 tests passed (3,087 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

The lower plane-distance bound remains the initial EnterDist of zero; tracking
later sector-entry distances requires full CurSector/EnterDist traversal.
Blockmap ordering/ties, compatibility, autoaim, slopes/portals/3D floors,
full AI archives and native executable acceptance remain open.
Changes remain uncommitted.

## Conversion and audit: zero-distance actor tracing (2026-10-01)

Audited FPathTraverse::init/AddThingIntercepts in native p_maputl.cpp and
ThingCheck/CheckPlane in p_trace.cpp. A zero-delta traversal still visits its
starting block and adds fraction-zero intercepts for boxes strictly containing
the XY origin. Zero-delta endpoint side tests do not select box edges or lines.
Actor height checks include exact top/bottom boundaries; planes require a
strictly positive distance below MaxDist and cannot hit at zero range.

Removed zero-range rejection from the shared trace and internal attack helper.
The existing actor-box and signed height checks now resolve origin-only hits.
Production ACS PickActor preserves explicit zero distance; production ACS
LineAttack retains its native omitted/zero range default of 2048.

Added eleven regressions covering strict XY containment and boundary misses,
exact top/bottom height hits and misses, origin coordinates, read-only query
state, production ACS PickActor/TID assignment/stack behavior, helper damage,
and coincident line/plane misses. Release validation: **4,144 tests passed
(3,071 Playsim; 500 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Full CurSector/EnterDist traversal, blockmap ordering/ties, compatibility,
autoaim, slopes/portals/3D floors, full AI archives and native executable
acceptance remain open. Zero-distance behavior is converted within the current
noncompatibility actor-box subset. Changes remain uncommitted.

## Conversion and audit: signed backward combat tracing (2026-10-01)

Audited TraceTraverse/ThingCheck/CheckPlane in native p_trace.cpp and actor-box
intercepts/Next in p_maputl.cpp. Converted negative ranges for the current
actor-box and flat-line subset: traversal reverses and orders intercepts along
the reversed delta, while actor height adjustment retains the original vertical
direction and signed maximum distance. PickActor and the ACS attack helper now
allow negative ranges through the shared trace. Positive height adjustment also
uses the native explicit top/bottom checks rather than a clipped Z interval.

Native CheckPlane requires a distance greater than EnterDist (initially zero)
and less than MaxDist, so negative ranges cannot select ordinary plane hits.
The managed negative path omits plane candidates and uses the failed-plane
opening fallback at lines. Omitted/zero ACS range defaults remain unchanged.

Added twelve regressions covering backward actor entry at/beyond range, both
yaw directions, pitched backward wall damage without plane damage, and original
pitch/signed-limit actor-height rejection. Tests exercise trace coordinates,
PickActor and production ACS invocation/stack behavior. Release validation:
**4,133 tests passed (3,060 Playsim; 500 MapLoader)**. Build with warnings treated
as errors passed with zero warnings/errors.

Zero-distance tracing, full CurSector/EnterDist traversal, blockmap ties,
compatibility, autoaim, slopes/portals/3D floors, full AI archives and native
executable acceptance remain open. Signed tracing is converted only within
the existing supported geometry subset. Changes remain uncommitted.

## Conversion and audit: ACS range fallback removal (2026-10-01)

Audited ACSF_LineAttack in native p_acs.cpp, P_LineAttack in p_map.cpp,
Trace/TraceTraverse in p_trace.cpp and FPathTraverse::Next in p_maputl.cpp.
Native ACS defaults omitted or zero ranges to MISSILERANGE (2048), but preserves
nonzero signed ranges. The managed ACS boundary already performs that default.
Removed the internal helper's separate 64-unit fallback for nonpositive or
nonfinite ranges, which incorrectly converted negative ACS ranges into forward
shots with actor/geometry damage and puff creation.

Added ten regressions for negative, zero and nonfinite helper ranges, negative
ACS ranges with forward actors/walls, and omitted/zero ACS defaults reaching a
target beyond 64 units. Tests verify damage, actor count, random state, return
value and stack consumption. Release validation: **4,121 tests passed
(3,048 Playsim; 500 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

This removes an unintended forward attack; it does not implement native signed
traversal. Native negative distances reverse the traversal delta while retaining
the original direction and signed distance for intercept/height checks. Managed
nonpositive tracing remains unsupported and returns no hit. Full signed/zero
trace parity, CurSector/EnterDist traversal, blockmap ties, compatibility, autoaim,
slopes/portals/3D floors, full AI archives and native executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: ordinary plane fallback after actor traversal (2026-10-01)

Audited ThingCheck and TraceTraverse in native p_trace.cpp. Ordinary sector
planes are checked after traversal only when no hit was selected. ThingCheck
performs its actor-versus-plane precheck only for sectors containing 3D floors.
Removed the managed ordinary-plane distance rejection of otherwise successful
actor hits. Earlier blocking line intercepts still stop actor selection, including
lines that classify a near-sector floor/ceiling hit. Range and actor bounds remain
enforced. This preserves native selection even for actors outside ordinary planes.

Added twelve regressions covering floor/ceiling, both horizontal directions,
actor misses that fall back to planes, and lines before/after actor entry. Tests
verify PickActor, trace classification, actor damage and plane damage. Corrected
two older tests whose shielding expectations contradicted the native source.
Release validation: **4,111 tests passed (3,038 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Full CurSector/EnterDist traversal, blockmap ties, compatibility, signed ranges,
autoaim, slopes/portals/3D floors, full AI archives and native executable
acceptance remain open. Changes remain uncommitted.

## Conversion and audit: actor entry versus later blocking walls (2026-10-01)

Audited FPathTraverse::Next, TraceTraverse and ThingCheck in native p_maputl.cpp
and p_trace.cpp. A successful actor intercept stops traversal before a later
line is visited, even if height adjustment moves the final actor impact beyond
that line's intersection. Managed tracing now compares actor box entry against
the blocking-line distance, rather than suppressing it with the adjusted hit
distance. The final impact still respects range and actor height/box bounds.

Added eight regressions for both directions, walls before/coincident with actor
entry, between entry and height-adjusted impact, and beyond the final impact.
Cases verify coordinates, PickActor, actor damage and absence of wall damage.
Exact entry ties retain the managed geometry-first policy pending blockmap work.
Release validation: **4,099 tests passed (3,026 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Flat-plane comparisons still use adjusted impact distance; native combined
sector/actor/plane traversal is not complete. Other open work includes blockmap
ties, compatibility, signed ranges, autoaim, slopes/portals/3D floors, full AI
archives and native acceptance. Changes remain uncommitted.

## Conversion and audit: actor entry order before height adjustment (2026-10-01)

Audited AddThingIntercepts/Next in p_maputl.cpp and ThingCheck in p_trace.cpp.
Native traversal visits horizontal box entries before ThingCheck adjusts an
intersection onto the actor's top or bottom. Corrected managed actor selection
to compare box-entry distance rather than adjusted 3D impact distance. Returned
coordinates still describe the actual height-adjusted impact. The managed
equal-entry tie remains actor-ID order pending native blockmap ordering work.

Added eight regressions covering ascending/descending shots, both horizontal
directions and reversed actor insertion order. A large actor entered first is
selected even when its height-adjusted impact is beyond a later actor's entry.
Cases verify trace metadata, PickActor and actual ACS-helper damage selection.
Release validation: **4,091 tests passed (3,018 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Actor-versus-wall/plane ordering after height adjustment still uses the managed
geometry-distance check; fully reproducing native combined intercept traversal
remains open. Other gaps include blockmap ties, compatibility, signed ranges,
autoaim, slopes/portals/3D floors, full AI archives and native acceptance.
Changes remain uncommitted.

## Conversion and audit: close actor/geometry impact ordering (2026-10-01)

Audited FPathTraverse::Next in native p_maputl.cpp: pending intercepts are
ordered with exact fraction comparisons, without a distance epsilon. Removed
the managed actor-versus-geometry exclusion gap. An actor strictly before a
wall or plane now remains eligible even when the separation is below 1e-9.
The managed exact-tie policy still prefers geometry; native equal-fraction
ordering depends on intercept insertion and blockmap traversal.

Added six regressions for both directions with a wall just before, coincident
with or just after actor-box entry, including PickActor and the internal ACS
attack helper. Cases verify actual actor damage versus line health reduction.
The close cases differ by 1e-10 map units.
Release validation: **4,083 tests passed (3,010 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Native equal-fraction ordering, full CurSector/EnterDist traversal,
compatibility handling, signed ranges, autoaim, slopes/portals/3D floors,
full AI archives and executable acceptance remain open. Changes remain
uncommitted.

## Conversion and audit: close plane/wall impact ordering (2026-10-01)

Audited native LineCheck's near-plane classification and CheckPlane's exact
distance comparisons in p_trace.cpp. Removed the artificial epsilon gap that
discarded a flat-plane candidate immediately before a wall. Equal plane/wall
distances now retain the near-sector plane using the selected wall side, rather
than resolving the boundary point into a potentially different sector. Plane
tracing now tests nonzero vertical direction exactly rather than a magnitude
cutoff. Strict ray-origin/range exclusions remain intact.

Added six regressions for floor/ceiling impacts with the wall just before,
exactly on or just after the plane intersection. The close cases differ by
1e-10 map units and verify both impact metadata and which geometry loses health.
Release validation: **4,077 tests passed (3,004 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Full CurSector/EnterDist traversal, nonhorizontal fallback after unsuccessful
near-plane checks, compatibility handling, signed ranges, autoaim,
slopes/portals/3D floors, full AI archives and executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: near-plane precedence over wall masks (2026-10-01)

Follow-up audit of LineCheck in native p_trace.cpp confirmed that near-sector
floor/ceiling classification precedes explicit WallMask handling. When a
horizontal trace cannot intersect the near plane, the destination-opening
fallback can pass through a two-sided line despite BlockHitscan or
BlockEverything. Moved the managed mask check after that fallback. One-sided
lines and closed destination boundaries continue to stop the trace.

Expanded the existing plane-opening regression matrix by sixteen cases for
both wall flags, both directions, floor/ceiling and open/closed destinations.
Each case verifies TraceLineAttack and PickActor. Existing ordinary flagged-wall
and geometry-damage boundary tests also pass.
Release validation: **4,071 tests passed (2,998 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Nonhorizontal near-plane metadata, full CurSector/EnterDist traversal,
compatibility handling, signed ranges, autoaim, slopes/portals/3D floors,
full AI archives and executable acceptance remain open. Changes remain
uncommitted.

## Conversion and audit: horizontal near-plane opening fallback (2026-10-01)

Audited LineCheck's cont block and CheckSectorPlane failure handling in native
p_trace.cpp. When a horizontal trace is on or outside the near sector plane,
the native plane calculation fails; an open two-sided destination can then
allow traversal. Added that fallback to the managed unflagged opening test.
The destination floor/ceiling bounds are strict in this fallback, preserving
blocking when the same boundary also occurs in the destination sector.

Existing door regressions also exposed orientation-only side selection. The
opening check now samples SectorAt immediately before each crossing, selecting
the matching sector side when available and falling back to geometric side
otherwise. Returned wall metadata retains that selected side for damage.
This remains an approximation of native CurSector traversal, not a complete
sector walker.

Added eight regressions for floor/ceiling boundaries, both impact directions,
open/closed destinations and shared PickActor behavior.
Release validation: **4,055 tests passed (2,982 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

This slice covers horizontal unflagged openings. Near-plane precedence over
explicit wall masks, nonhorizontal plane metadata, native CurSector/EnterDist,
compatibility traversal, signed ranges, autoaim, slopes/portals/3D floors,
full AI archives and executable acceptance remain open. Changes remain
uncommitted.

## Conversion and audit: direction-sensitive opening ceilings (2026-10-01)

Audited FTraceInfo::LineCheck in native p_trace.cpp. The current sector's
floor/ceiling bounds exclude exact boundary heights, while the destination
sector accepts its exact floor and ceiling. Replaced BlocksPick's symmetric
opening limits with near/far checks selected by the impact side. An unflagged
opening now permits a ray at exactly the far ceiling; explicit wall masks
continue to block it. Shared TraceLineAttack/PickActor callers receive the fix.

Added eight regressions for front/back traversal one Q16 unit below, exactly
on and above the destination ceiling, including PickActor. Explicit blocking
tests verify upper-tier ceiling and line damage at the exact height.
Release validation: **4,047 tests passed (2,974 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Near-sector out-of-bounds classification still requires full plane/sector
transition metadata rather than the managed wall-block approximation. Native
CurSector selection, compatibility traversal, signed ranges, autoaim,
slopes/portals/3D floors, full AI archives and executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: lower-wall floor damage boundary (2026-10-01)

Audited FTraceInfo::LineCheck in p_trace.cpp and P_GeometryLineAttack in
p_destructible.cpp. Native classifies a blocking wall impact at or below the
opposite sector's floor as TIER_Lower; geometry damage then applies to the back
floor before the line. Corrected the managed lower-tier check from strict less
than to inclusive less-or-equal. Upper-tier behavior remains unchanged.

Added seven regressions for front/back impacts one Q16 unit below, exactly on
and above the floor boundary, plus the ACS path with floor and wall sharing a
health group. That case verifies sequential damage: floor damage synchronizes
the group, then line damage reduces the synchronized value again.
Release validation: **4,039 tests passed (2,966 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Audit still identifies direction-sensitive opening boundaries and native
sector-transition metadata as incomplete in the managed trace. Compatibility
traversal, signed ranges, autoaim, slopes/portals/3D floors, full AI archives
and executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: actor entry-edge classification (2026-10-01)

Audited the non-compatible AddThingIntercepts branch in native p_maputl.cpp.
Actor tracing now tests origin-facing box edges in native top/right/bottom/left
order, using the shared full-delta endpoint-side classification. Only a strictly
inside-box origin receives an immediate intercept when no faces are eligible.
This corrects grazing asymmetry and vertical rays starting exactly on a box
edge. Horizontal interval clipping still bounds later height intersections.

Extracted the scalar segment calculation so wall and actor edges share the
native side test without allocating synthetic walls. Removed its redundant
segment-fraction restriction: native uses endpoint-side classification and ray
fraction, and the extra division rejected floating-point diagonal corner hits.
Existing four-quadrant diagonal regressions caught and verify that correction.

Added ten regressions for opposite grazing edges, nearby inside/outside rays,
strict vertical entry boundaries and PickActor integration.
Release validation: **4,032 tests passed (2,959 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Compatibility-mode diagonal intercepts, blockmap ordering/start nudging,
sector EnterDist, signed ranges, native autoaim, slopes/portals/3D floors,
full AI archives and executable acceptance remain open. Changes remain
uncommitted.

## Conversion and audit: inclusive actor height boundaries (2026-10-01)

Audited ThingCheck in src/playsim/p_trace.cpp. Native rejects entry heights
strictly above Top or below Z, and accepts a top/bottom intersection at exactly
MaxDist. Corrected the managed horizontal trace to include the actor's exact
top surface. Height clipping now uses exact zero direction and interval bounds
rather than classifying small nonzero vertical directions as horizontal or
extending a disjoint interval with an epsilon.

Added ten regressions covering one-Q16-unit positions around top and bottom,
descending top intersections just before/at/after range, and read-only PickActor
at the exact top. This deliberately preserves the distinct native rule for
sector planes, whose intersections must occur strictly before MaxDist.
Release validation: **4,022 tests passed (2,949 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Remaining differences include compatibility-mode actor intercepts, box-edge
side classification, blockmap ordering/start nudging, sector EnterDist and
signed ranges. Native autoaim, slopes/portals/3D floors, full AI archives and
executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: native actor box intersections (2026-10-01)

Audited the non-compatible FPathTraverse::AddThingIntercepts branch in native
p_maputl.cpp and ThingCheck in p_trace.cpp. Native hitscan traversal intersects
the actor's axis-aligned horizontal bounding box rather than a circular radius.
Replaced the managed quadratic circle intersection with interval clipping
against both horizontal box axes. Existing vertical height clipping, 3D range,
nearest-hit selection and geometry occlusion remain wired through the shared
trace, including PickActor and managed target selection.

Added nine regressions for diagonal box entry in all four quadrants, corner
entry before the old circular surface, range cutoffs, vertical corner hits,
outside-box misses and PickActor integration. Existing combat tests pass.
Release validation: **4,012 tests passed (2,939 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

The managed box subset does not yet reproduce native compatibility-mode
diagonal intercepts, edge side tolerances or every ThingCheck top-edge detail.
Blockmap ordering/start nudging, sector EnterDist, signed ranges, autoaim,
slopes/portals/3D floors, full AI archives and native executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: native wall endpoint-side classification (2026-10-01)

Audited P_PointOnDivlineSide in p_maputl.h, AddLineIntercepts and
P_InterceptVector in p_maputl.cpp, and EQUAL_EPSILON in vectors.h. RayLine
now rejects equal endpoint-side classifications using the native 1/65536
tolerance and full traversal delta. TraceLineAttack supplies its range as
the delta scale; line-of-sight already supplies its complete segment delta.
Parallel detection now uses exact zero rather than an unrelated denominator
cutoff. Negative intersection parameters are rejected rather than clamped to
the origin, and segment limits no longer have an artificial epsilon extension.

Added eight regressions covering both wall orientations, the native asymmetric
endpoint rule, full-delta tolerance scaling, small nonzero denominators and
intersections just behind the source. Existing combat/visibility tests pass.
Release validation: **4,003 tests passed (2,930 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Remaining traversal differences include blockmap-start nudging, compatibility
flags, sector EnterDist, signed range, slopes, portals and 3D floors. Native
autoaim, full AI archives and executable acceptance remain open. Changes
remain uncommitted.

## Conversion and audit: vertical and parallel wall traversal (2026-10-01)

Audited FPathTraverse::AddLineIntercepts in src/playsim/p_maputl.cpp, which
skips lines when both endpoints occupy the same side of the trace. Corrected
RayLine's parallel branch to return no intersection. Previously a purely
vertical ray (zero horizontal direction) falsely hit every wall at distance
zero, and collinear travel incorrectly blocked shots. The shared helper also
serves line-of-sight queries. Actual crossing parameter calculations remain.

Added nine regressions for zero/parallel/collinear rays, offset walls, scaled
direction parameters and vertical floor/ceiling damage inside a closed room.
Updated the older collinear-block expectation while retaining its crossing
endpoint tests. Existing combat and line-of-sight coverage continues to pass.
Release validation: **3,995 tests passed (2,922 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Remaining traversal differences include precise native endpoint-side handling,
near-parallel tolerances, sector EnterDist, signed range, slopes, portals and
3D floors. Native autoaim, full AI archives and executable acceptance remain
open. Changes remain uncommitted.

## Conversion and audit: monster hitscan geometry impacts (2026-10-01)

Audited A_PosAttack, A_SPosAttackInternal and A_CPosAttackInternal in native
wadsrc/static/zscript/actors/doom/possessed.zs: attacks invoke LineAttack rather
than an actor-only query. Updated managed native-profile and fallback monster
hitscan dispatch to retain the shared trace's full impact metadata. Actor hits
use existing ActorDamage; wall/flat-plane hits use GeometryLineAttack, including
its shared-health, sky and horizon handling. Existing damage/spread rolls and
attack timing remain intact; impact handling adds no combat random rolls.

Added seven regressions: real zombieman, shotgun-guy and chaingunner profiles
damage blocking walls; health-less control maps consume identical random rolls;
direct monster hitscan covers floor/ceiling impacts, closer actor obstruction
and horizon immunity. Existing same-species infighting immunity stays active.
Release validation: **3,986 tests passed (2,913 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Native vertical autoaim, actor blood/puff class behavior, signed-range traversal,
slopes, portals, 3D floors, full AI archives and native executable acceptance
remain open. Changes remain uncommitted.

## Conversion and audit: shared 3D target tracing (2026-10-01)

Removed the duplicate horizontal-range FindTarget ray-cylinder implementation.
The existing managed targeting API now delegates to TraceLineAttack, retaining
its current caller-supplied yaw/pitch offsets and nonplayer baseline pitch.
Monster hitscan, BFG spray and ACS player-pointer consumers now use the shared
3D range, vertical ray and flat-plane occlusion behavior. Queries remain
read-only and return actor hits only. Existing horizontal surface-range and
actor selection regressions continue to pass.

Native audit: P_LineAttack uses a normalized 3D direction in p_map.cpp; the
P_AimLineAttack traversal also explicitly checks 3D attack range because callers
combine it with P_LineAttack. Added eight regressions for elevated and vertical
target range, ceiling occlusion, wrapped yaw and absent damage/puff/RNG effects.
Release validation: **3,979 tests passed (2,906 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

This converts the managed fixed-ray subset, not native vertical autoaim.
BFG/player-pointer autoaim windows and monster geometry damage remain open;
callers still select an actor rather than consume full impact metadata.
Signed-range traversal, slopes, portals, 3D floors, full AI archives and native
executable acceptance also remain open. Changes remain uncommitted.

## Conversion and audit: strict plane trace range boundaries (2026-10-01)

Follow-up range audit of FTraceInfo::CheckPlane in src/playsim/p_trace.cpp
confirmed strict native comparisons: hitdist must be greater than EnterDist
and less than MaxDist. Corrected the managed flat-plane trace to exclude hits
at the ray origin and at the range endpoint. This closes a real positive-range
mismatch independently of the still-open signed-range traversal conversion.

Corrected two older vertical endpoint expectations and added two one-Q16-unit
beyond-endpoint cases. Added eight regressions for the production ACS path
(floor/ceiling immediately below, at and above the range boundary), geometry
health, puff TIDs, result/stack behavior and planes at the trace origin.
Release validation: **3,971 tests passed (2,898 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Native EnterDist also tracks sector transitions; the managed flat-plane subset
still uses SectorAt at each candidate rather than a complete sector traversal.
Negative-range traversal, slopes, portals and 3D floors remain open, along with
full AI archives and native executable acceptance. Changes remain uncommitted.

## Conversion and audit: relative attack pitch and signed range findings (2026-10-01)

Closed the internal relative line-attack helper's ordinary-actor pitch gap.
It now converts Actor.PitchDegrees with the shared BAM conversion for all
actors, removing a player-only conversion that ignored imported ordinary
actor pitch and truncated BAM precision. Relative pitch offsets wrap normally.
Production ACS LineAttack retains its native absolute-angle path.

Added eight regressions for ordinary actors, vertical floor/ceiling attacks,
wrapped map pitch, offset cancellation, absolute-direction isolation,
geometry damage, puff TIDs, unchanged source pitch and unchanged combat RNG.
Release validation: **3,961 tests passed (2,888 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Signed-range audit: ACSF_LineAttack forwards any nonzero signed range unchanged.
Native FTraceInfo traverses Vec * MaxDist and uses signed intercept distances;
its plane and actor checks do not simply implement a positive reversed ray.
The managed helper still replaces negative range with a short forward attack.
That confirmed mismatch remains open pending a faithful signed traversal path,
including plane behavior; no speculative range change was made in this slice.
Other open work includes legacy FindTarget tracing, render interpolation,
full AI archives and native executable acceptance. Changes remain uncommitted.

## Conversion and audit: ACS pitch query normalization (2026-10-01)

Audited PCD_GETACTORPITCH and PitchToACS in src/playsim/p_acs.cpp against
TAngle::Normalized180, BAMs and Q16 in src/common/utility/vectors.h. Corrected
GetActorPitch to normalize through BAM, interpret the result as signed, then
truncate fixed turns toward zero. Raw map pitch remains unchanged on the
actor. A map pitch of 450 degrees now reads as 16384 (90 degrees), and both
+/-180-degree inputs read as -32768. This also preserves native fractional
truncation for negative angles rather than arithmetic-shift rounding.

Added fifteen regressions exercising the real VM opcode, stack preservation,
wrapped angles, signed-short map boundaries, fractional inputs, activator and
missing-TID lookup, and the map-import-to-query path.
Release validation: **3,953 tests passed (2,880 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Audit scope still includes the internal relative line-attack helper's ordinary
actor pitch handling and explicit negative attack ranges. Production ACS
LineAttack uses absolute directions. Render interpolation, full AI archives
and native executable acceptance remain open. Changes remain uncommitted.

## Conversion and audit: UDMF actor pitch and player-start angles (2026-10-01)

Audited ParseThing, P_SpawnMapThing and SpawnPlayer in the native map loader
and p_mobj.cpp, plus FPlayerStart in doomdata.h. Added integer pitch parsing,
native signed-short wrapping in the level builder, and pitch initialization
for ordinary map actors. Native stores map pitch directly as degrees, so
wrapped values and angles beyond 180 degrees remain intact.

The audit corrected the preceding roll slice: player-start records retain
position/yaw only, and SpawnPlayer resets pitch/roll to zero. Managed spawning
now applies both map angles only to ordinary actors. Corrected the previous
player-roll regression to require zero. Also removed the artificial +/-180
archive restore restriction for ordinary actors, which rejected valid map
pitch values; player pitch validation and invalid-state checks remain.

Added sixteen regressions covering namespaces, omitted pitch, invalid integer
inputs, signed-short boundaries, players one/two, ordinary actors, unnormalized
pitch and archive restoration. Updated two old invalid-pitch cases to test the
actual player limit rather than imposing a non-native ordinary-actor limit.
Release validation: **3,938 tests passed (2,865 Playsim; 500 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Remaining audit scope includes angle consumers with unnormalized map pitch,
render interpolation, full AI archives and native executable acceptance.
Gameplay phases 1-3 remain incomplete. Changes remain uncommitted.

## Conversion and audit: UDMF actor spawn roll (2026-10-01)

Audited ParseThing in src/maploader/udmf.cpp and P_SpawnMapThing in
src/playsim/p_mobj.cpp. The native roll field is an integer converted to a
signed short, then applied as degrees when spawning. The managed parser now
retains roll, the level builder performs the same unchecked signed-short
conversion, and ActorSpawner initializes roll for players and other actors.
The native roll case has no namespace restriction; the managed import matches
that behavior. Existing maps without roll retain zero.

Added eleven regressions for positive/negative and wrapped integer inputs,
namespace behavior, omitted values, fractional-value rejection and the full
UDMF-to-actor-to-save/load path for players, monsters and barrels.
Release validation: **3,922 tests passed (2,858 Playsim; 491 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

No remaining findings in this roll-import slice. Thing pitch import, render
interpolation, full AI archives and native executable acceptance remain open.
Gameplay phases 1-3 remain incomplete. Changes remain uncommitted.

## Conversion and audit: actor roll persistence and checksum (2026-10-01)

Audited AActor::Serialize in src/playsim/p_mobj.cpp: native saves include
the actor angles together, including roll. Closed the C# pose archive's roll
omission with HCSV version 16, preserving exact BAM bits for every actor.
The bounded roll trailer wraps the existing version 15 geometry-health archive.
Older archives remain readable and preserve current roll when absent. Partial
roll snapshots are rejected rather than silently discarding values. Nonzero
roll contributes to the checksum; existing zero-roll traces stay unchanged.

Added 19 regressions covering exact values, multiple actors, geometry-health
coexistence, checksum continuation with stationary actors, legacy reads,
every truncated prefix, malformed sizes/counts and incomplete snapshots.
Updated the current pitch archive version assertion and explicitly omitted
roll from the older version-five fixture.
Release validation: **3,911 tests passed (2,854 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

Audit limitation: full monster-AI continuation remains outside the existing
pose archive; a moving-monster continuation fixture exposed unsaved AI state.
Roll interpolation/rendering and full native executable acceptance remain
open. HCSV remains a C# simulation archive, not native savegame compatibility.
Changes remain uncommitted.

## Conversion and audit: native ACS LineAttack result and damage type (2026-10-01)

Follow-up audit of ACSF_LineAttack and CallFunction's final return in
src/playsim/p_acs.cpp closed the previously documented synthetic hit-result
gap. The ACS call now returns zero after performing attacks, regardless of
actor hits; the internal trace helper's hit indicator remains internal.
Corrected the optional damage-type sentinel: argument zero uses native None
instead of resolving string-table entry zero. Positive string IDs still resolve
through the existing local/global ACS string lookup.

Added four regressions for hit/miss return values, preserved stack prefixes,
actual damage, zero damage-type default and nonzero typed-death selection.
Corrected the older actor-hit test expecting a synthetic return of one.
Release validation: **3,892 tests passed (2,835 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.

The angle audit also checked existing Sin/Cos/VectorAngle opcode units; those
already use fixed turns. Explicit negative attack ranges, puff classes/flags,
roll archives, interpolation and native executable acceptance remain open.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: actor roll ACS units (2026-10-01)

Audited SetActorRoll, ChangeActorRoll and GetActorRoll against their native
ACSF cases, SetActorRoll, ACSToAngle and AngleToACS in src/playsim/p_acs.cpp.
Both setters now convert fixed turns into managed BAM. The getter returns
the normalized unsigned fixed-turn value rather than raw BAM. Negative and
wrapped inputs retain native circular-angle behavior; fractional BAM precision
below one ACS unit truncates on readback.

Updated two older tests asserting raw BAM at the ACS boundary. Added nine
regressions for both setters, signed/wrapped/full-turn values, multiple TIDs,
activator targeting, untouched unrelated actors, getter truncation, missing
targets and stack preservation. Interpolation remains outside the headless
simulation; the optional argument is consumed.

Release validation: **3,888 tests passed (2,831 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
Other angle consumers, roll archive coverage, interpolation, autoaim and native
executable acceptance remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: ChangeActorAngle/Pitch units (2026-10-01)

Audited ACSF_ChangeActorAngle and ACSF_ChangeActorPitch against SetActorAngle,
SetActorPitch and ACSToAngle in src/playsim/p_acs.cpp. Corrected these CallFunc
paths to use ACS fixed turns rather than 32-bit BAM inputs. Yaw shifts the
fixed-turn value into BAM; pitch uses the signed low 16 bits to match native
Normalized180, including the -180-degree boundary. Existing actor-angle
opcodes already used these units. All matching TIDs continue to update.

Corrected two older tests that supplied BAM values and asserted the managed
unit mismatch. Added eight regressions for positive/negative/wrapped yaw and
pitch, multiple matching TIDs, unchanged unrelated actors, stack consumption
and native zero result. The optional interpolation argument is consumed, but
render interpolation is not implemented in this headless simulation.

Release validation: **3,879 tests passed (2,822 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
Other ACS angle consumers, interpolation, attack offsets, autoaim and native
executable acceptance remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: ACS LineAttack directions and sources (2026-10-01)

Audited ACSF_LineAttack in src/playsim/p_acs.cpp and native Q16 angle units.
The ACS call now converts fixed-turn yaw/pitch to BAM and treats them as
absolute directions, rather than adding source yaw/pitch. Omitted or zero
range uses native MISSILERANGE (2048). Nonzero source TIDs fire from every
matching live actor using a snapshot before attacks can spawn puffs.

The existing internal relative-angle test helper remains available; production
ACS dispatch explicitly uses absolute angles. Existing synthetic managed hit
return values and puff behavior are unchanged and are not claimed as native
return/cosmetic parity. Explicit negative range handling remains separate.

Added eight regressions for cardinal/signed/wrapped yaw, absolute vertical
pitch, default and explicit-zero range, multi-source geometry damage and
stack preservation. Release validation: **3,871 tests passed (2,814 Playsim;
484 MapLoader)**. Build with warnings treated as errors passed with zero
warnings/errors. Other ACS angle consumers, puff classes/flags, attack offsets,
native return semantics and executable acceptance remain open.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: ACS PickActor arguments (2026-10-01)

Audited ACSF_PickActor and ACSToAngle in src/playsim/p_acs.cpp, with fromQ16
in src/common/utility/vectors.h. Corrected yaw/pitch conversion from ACS fixed
turns (65536 per turn) to the managed 32-bit BAM representation. Signed and
wrapped values now trace in the native direction.

Corrected the optional actor-mask default: an omitted sixth argument uses
SHOOTABLE, while an explicitly supplied zero selects no actors. This supersedes
the earlier audit entry describing the managed zero-mask substitution.
Confirmed native force-TID and return-TID rules, preserving an existing TID
unless forced, allowing forced clearing and rejecting unforced zero assignment
to an untagged actor. Arguments consume only their own stack entries.

Added 12 regressions for cardinal/signed/wrapped yaw, vertical pitch, omitted
versus explicit zero masks, TID flag combinations and stack preservation.
Release validation: **3,863 tests passed (2,806 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
Other ACS angle consumers still need the same units audit. Full actor flags,
ghost/spectral filtering, attack offsets and native executable acceptance
remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: represented PickActor flags (2026-10-01)

Audited native actor filtering in src/playsim/p_trace.cpp: requested flags
match by any overlapping bit, with 0xffffffff accepting every actor. Corrected
managed filtering, which previously required damage eligibility and treated
SHOOTABLE as mandatory even when another requested flag matched.

Mapped the existing managed SOLID, SHOOTABLE, NOGRAVITY, FLOAT and MISSILE
properties to their native primary-flag bits. PickActor can now select solid
nonshootable objects, floating/no-gravity actors and missiles without applying
damage. The shared trace retains damage eligibility for ordinary attacks.
Direct zero masks select no actors; the existing ACS zero-mask default still
selects SHOOTABLE, as before. Unrepresented flags are not synthesized.

Added ten regressions for mask union/zero/all behavior, nonshootable selection,
movement flags, missiles, ordinary attack filtering and ACS TID assignment.
Release validation: **3,851 tests passed (2,794 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
Remaining primary flags, ghost/spectral callback filtering, attack origins,
autoaim, native executable acceptance and broader gameplay parity remain open.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: PickActor shared 3D tracing (2026-10-01)

Audited P_LinePickActor in src/playsim/p_map.cpp: native selection uses the
same normalized pitch/yaw direction and 3D range as attacks. Routed managed
PickActor through the read-only shared attack trace and removed its duplicate
horizontal-range actor tracer. Existing actor/wall mask parameters, source
exclusion and stable actor ordering remain supported. Flat sector planes now
block actor selection beyond them; selection applies no damage or randomness.

Added six regressions for pitched/vertical range boundaries, unchanged actor
health, floor obstruction and read-only destructible-plane selection.
Release validation: **3,841 tests passed (2,784 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
FindTarget/autoaim, complete native actor-mask semantics, attack-origin offsets,
portals, slopes, 3D floors and native executable acceptance remain open.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: normalized 3D hitscan range (2026-10-01)

Closed the shared attack trace's horizontal-distance limitation against native
P_LineAttack in src/playsim/p_map.cpp. Direction now uses
(cos(pitch)*cos(yaw), cos(pitch)*sin(yaw), -sin(pitch)), making wall, actor and
flat-plane intersection parameters distances along the normalized 3D ray.
Removed the attack trace's 89-degree pitch clamp and added true vertical rays.
Actor intersection now solves the cylinder quadratic with the scaled
horizontal direction, including the zero-horizontal-direction case.

ACS and player weapon attacks use this shared trace. Existing FindTarget and
PickActor helpers are unchanged and still require their own range/autoaim
audits; this entry does not claim all combat tracing has native parity.

Added ten regressions for pitched floor/ceiling range boundaries, exact vertical
plane distances without horizontal drift, and vertical actor-cylinder range.
Release validation: **3,835 tests passed (2,778 Playsim; 484 MapLoader)**.
Build with warnings treated as errors passed with zero warnings/errors.
Native attack-origin offsets, autoaim, sky identity, slopes, 3D floors,
geometry radius damage, callbacks and executable acceptance remain open.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: direct hitscan plane damage (2026-10-01)

Extended the shared attack trace to flat sector floor/ceiling intersections.
Candidates use current simulation heights and are accepted only when the
impact point belongs to that sector through existing SectorAt geometry.
Nearest wall, plane and actor selection preserves geometry-over-actor ties.
The returned plane sector/part remains read-only until the attack applies damage.

Converted the flat-plane branch of P_GeometryLineAttack and its vulnerability
check from src/playsim/p_destructible.cpp. ACS and player weapon attacks now
damage the selected positive-health floor/ceiling and its shared health group.
Standard F_SKY1 planes remain immune; custom resolved sky aliases and native
sky puff behavior are still outside this subset.

Added nine regressions for ACS floor/ceiling impacts, exact height and metadata,
read-only tracing, player pistol pitch, sky immunity, closer wall/actor
precedence and range. Release validation: **3,825 tests passed (2,768 Playsim;
484 MapLoader)**. Build with warnings treated as errors passed with zero
warnings/errors.

Trace range still follows the existing managed horizontal-distance convention;
native 3D range and vertical autoaim parity require further work. SectorAt's
existing polygon limits, slopes, 3D floors, geometry radius damage, callbacks
and native executable acceptance remain open. Phases 1-3 remain incomplete.

## Conversion and audit: projectile plane geometry damage (2026-10-01)

Converted the flat floor/ceiling portion of P_ProjectileHitPlane from
src/playsim/p_destructible.cpp. Swept projectile collision now retains the
winning plane's sector and part, clearing both when a closer actor wins.
Wall collision precedence remains unchanged. Plane impacts apply separately
rolled missile damage to positive health and use existing group propagation.

Added conventional F_SKY1 immunity corresponding to P_CheckSectorVulnerable.
Sky and zero-health planes consume no damage roll. The managed map model
stores texture names rather than native resolved texture IDs: custom sky
aliases are not covered, and sky explosion removal remains separate work.

Added seven regressions covering floor/ceiling health, linked groups, exact
cylinder impact height, damage bounds, sky and zero-health random preservation,
and closer actor precedence. Release validation: **3,816 tests passed
(2,759 Playsim; 484 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Hitscan direct plane traces, geometry radius damage, native sky identity and
removal, slopes, 3D-floor impacts, callbacks and native executable acceptance
remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: projectile wall geometry damage (2026-10-01)

Converted the managed wall-impact portion of native P_ProjectileHitLinedef
and its call from P_ExplodeMissile, using src/playsim/p_destructible.cpp and
src/playsim/p_mobj.cpp. Swept projectile collision now retains the blocking
line, clearing it when a closer plane or actor collision wins. Wall impacts
apply geometry damage before existing explosion effects.

Positive opposite-sector floor and ceiling health receive independent
missile-damage rolls when the cylinder touches the corresponding lower/upper
wall parts, followed by a separate roll for positive linedef health. The
native EQUAL_EPSILON value (1/65536) is used. Existing shared health propagation
handles linked geometry. Impact-side presence and horizon vulnerability checks
follow the native helper. Geometry with no positive health consumes no rolls.

Added eight regressions covering rocket/plasma/imp/baron wall impacts, random
damage bounds, linked groups, lower/upper sectors, actor obstruction and open
boundaries. Release validation: **3,809 tests passed (2,752 Playsim;
484 MapLoader)**. Build with warnings treated as errors passed with zero
warnings/errors.

Direct floor/ceiling projectile damage, radius damage to geometry, sky removal,
slopes, 3D-floor impacts, damage/destruction callbacks and native executable
acceptance remain open. Managed blast damage still affects actors only.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: player weapon wall damage (2026-10-01)

Connected player hitscan/melee weapon dispatch to the shared geometry wall
damage path. Native source comparison used P_LineAttack in src/playsim/p_map.cpp
and P_GeometryLineAttack in src/playsim/p_destructible.cpp. HitscanCombat now
uses the read-only attack trace for both actor and wall impacts, retaining
damage-before-spread rolls, pellet counts, ammo spending and firing cadence.
Yaw and pitch spread feed the same trace; closer actors shield geometry.

Pistol, chaingun, shotgun, super shotgun, fist and chainsaw can now damage
destructible walls and shared groups. Lower/upper wall impacts also reach
the opposite sector plane through the existing geometry implementation.
Projectile dispatch still takes its existing separate path.

Added 11 regressions covering all six weapons, group propagation, identical
random consumption with wall versus open shots, lower/upper sectors, melee
range, actor obstruction and cooldown/ammo behavior. Release validation:
**3,801 tests passed (2,744 Playsim; 484 MapLoader)**. Build with warnings
treated as errors passed with zero warnings/errors.

Direct plane traces, projectile and monster attack geometry damage, puff/sky
parity, slopes, 3D-floor traces and damage/destruction callbacks remain open.
Native executable acceptance has not been run. Phases 1-3 remain incomplete.

## Conversion and audit: ACS wall geometry hits (2026-10-01)

Converted the wall portion of P_GeometryLineAttack from
src/playsim/p_destructible.cpp into the existing ACS LineAttack path.
CombatTrace now returns the nearest blocking linedef and impact side while
remaining read-only. ACS applies geometry damage after creating its existing
puff. Actor hits still win when closer and do not damage the wall behind them;
wall hits retain the existing managed return value of zero.

Wall damage checks the native vulnerability conditions: Line_Horizon (special
9) is immune and the impacted side must exist. Positive linedef health takes
damage through the shared group implementation. Lower/upper wall hits first
damage the opposite sector's floor/ceiling health, then the linedef, following
native ordering. Open two-sided boundaries and out-of-range walls are unaffected.

Added 11 regressions covering damage/overkill/nonpositive values, group updates,
lower and upper walls, horizon and missing-side immunity, open boundaries,
range, impact-side selection, read-only tracing and actor occlusion. Release
validation: **3,790 tests passed (2,733 Playsim; 484 MapLoader)**. Build with
warnings treated as errors passed with zero warnings/errors.

This slice covers ACS wall hits. Direct floor/ceiling intersections still need
trace support; player weapon and projectile geometry damage, sky handling,
slopes, 3D-floor traces and destruction callbacks remain open. Native executable
acceptance has not been run. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: native 3D health group membership (2026-10-01)

Follow-up audit against P_InitHealthGroups, P_SetHealthGroupHealth and
P_DamageHealthGroup in src/playsim/p_destructible.cpp corrected a membership
assumption in the earlier geometry-health slice. Native startup registers
lines and floor/ceiling sector groups, but does not register 3D health groups
independently. Therefore 3D health alone neither creates a pooled group nor
contributes to its initial maximum. Local starting values remain unchanged.

Group setters and damage synchronization now visit sector 3D health only when
that sector's floor or ceiling registered it in the same group. A 3D setter
can still update an existing group created elsewhere, while its own targeted
local value changes directly. GetSectorHealth continues to return existing
pooled health even for a sector outside the group's registered membership,
matching src/playsim/p_acs.cpp. This distinguishes query lookup from propagation.

Added 11 regressions covering 3D-only maps, existing groups, initial maximum,
local startup values, setter/damage propagation, queries and save/checksum
continuation. Release validation: **3,779 tests passed (2,722 Playsim;
484 MapLoader)**. Build with warnings treated as errors passed with zero
warnings/errors. Native damage/destruction callbacks, rendered/executable
acceptance and full 3D-floor geometry remain open. Phases 1-3 remain incomplete.

## Conversion and audit: geometry health persistence (2026-10-01)

Closed the previous slice's managed geometry-health save and checksum gap.
Native source comparison used src/p_saveg.cpp sector health fields and
src/playsim/p_destructible.cpp pooled group serialization. HCSV version 15
adds a bounded health trailer containing each line's health, each sector's
floor/ceiling/3D health and pooled group IDs/health. Versions 1-14 remain
readable and preserve current health when no health payload is present.
The managed format remains distinct from the native serializer.

Restore validates line/sector counts and exact group ID membership before
clock or geometry mutations. It preserves local and pooled values separately,
including zero health, instead of rebuilding groups from map defaults.
The reader rejects truncated sections, invalid lengths, nonpositive and
duplicate group IDs. Checksums include all local and pooled health values in
deterministic group order; maps with no geometry health keep their existing
checksum behavior.

Added 14 regressions for round-trip queries, zero and ungrouped health,
post-restore damage/checksum continuation, version-14 preservation, independent
map-count/group mismatches, every health checksum surface, truncation, invalid
trailer lengths and duplicate IDs. Updated current-version assertions and
plane-corruption fixture offsets for the new trailer. Release validation:
**3,768 tests passed (2,711 Playsim; 484 MapLoader)**. Build with warnings
treated as errors passed with zero warnings/errors.

Native executable acceptance, destruction callbacks/events, dynamic tag and
group membership changes, full 3D-floor geometry and broader gameplay archive
coverage remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: geometry health actions (2026-10-01)

Converted native Line_SetHealth (150) and Sector_SetHealth (151) from
src/playsim/p_lnspec.cpp for Hexen/UDMF map activation and both ACS direct
and stack special dispatch. Line IDs and sector tags include additional IDs;
negative health clamps to zero. Sector parts use floor=0, ceiling=1, 3D=2.
Invalid parts and missing targets retain native success behavior. Sector tag
zero selects untagged sectors, while line ID -1 remains unset.

Audit against P_SetHealthGroupHealth in src/playsim/p_destructible.cpp found
that existing managed damage synchronization stopped at lines or the same
sector part. Added shared group synchronization across all linked lines and
all linked floor, ceiling and 3D parts; both setters and damage now use it.
Unknown groups do not get synthesized, matching the native group lookup.

Added 18 regressions for map use/cross activation, ACS continuation, additional
IDs/tags, clamping, missing targets, invalid parts, tag-zero selection and
mixed geometry health groups. Release validation: **3,754 tests passed
(2,697 Playsim; 484 MapLoader)**. Build with warnings treated as errors passed
with zero warnings/errors.

Geometry health and pooled health are still absent from the managed save
payload and simulation checksum; save/load parity remains open. Destruction
callbacks/events and full 3D-floor geometry are also outside this slice.
Native executable acceptance has not been run. Gameplay phases 1-3 remain
incomplete.

## Conversion and audit: nested scanner buffer preparation (2026-10-01)

Follow-up audit against FScanner::PrepareScript and the classic quoted-string
loop found that the managed nested ID scanner omitted native buffer preparation.
Native parsing strips a leading UTF-8 BOM and appends a final newline (or
replaces a terminal NUL). The newline becomes part of an unterminated quoted
token, causing numeric parsing to reject it instead of accepting a final ID.
Converted that preparation for both line moreids and sector moreids.

Added 12 regressions covering unfinished quoted numbers/MAXINT, terminal NUL,
BOM, final comments, quoted CRLF and closed quotes. Two cases pass escaped
quotes through the actual TEXTMAP parser before LevelBuilder, verifying both
complete and unfinished nested strings end to end.

Release validation: **3,736 tests passed (2,679 Playsim; 484 MapLoader)**.
The solution build with warnings treated as errors passed with zero warnings
and errors. Source comparison closes the previously documented quoted EOF
logic gap; native executable acceptance has not been run. General scanner
conversion, dynamic tag serialization, mixed thinker ordering, slopes and
replication remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: nested UDMF ID scanner (2026-10-01)

Audited line and sector moreids against src/maploader/udmf.cpp and the
classic FScanner implementation in src/common/engine/sc_man.cpp and
sc_man_scanner.re. Converted comment handling (block, line, semicolon and
region directives), quoted numeric tokens, punctuation boundaries and native
signed 64-bit saturation followed by the 32-bit ID cast. Both map ID surfaces
share the same parser; existing namespace gates and line/sector filtering
remain in place. Parsing retains the valid prefix at the first invalid token.

Added 21 regression cases through LevelBuilder for both lines and sectors,
including comments, quotes, escaped quotes, octal/hex numbers, overflow,
trailing garbage, whitespace and slash boundaries. Release validation:
**3,724 tests passed (2,679 Playsim; 472 MapLoader)**. The solution build with
warnings treated as errors passed with zero warnings/errors.

This converts the nested ID-string numeric subset, not the general scripting
scanner. Malformed quoted-string EOF behavior still needs native acceptance
comparison. Dynamic tag serialization, mixed thinker ordering, slopes,
replication and native rendered acceptance remain open. Gameplay phases 1-3
remain incomplete.

## Conversion and audit: multiple UDMF sector tags (2026-10-01)

Converted sector `moreids` against `src/maploader/udmf.cpp` and native tag
membership in `src/playsim/p_tags.cpp`. Additional tags use the existing numeric
prefix parser and ZDoom/ZDoomTranslated/Vavoom namespace gate. Duplicates and
zero are omitted; unlike line IDs, sector tag -1 is valid. Corrected primary
UDMF sector IDs being narrowed to short: LevelSector now preserves full integer
tags, including values beyond 65535. Imported extra tags are read-only map data.

Added distinct stored membership and selector predicates. Zero is never a
stored tag, while a zero selector finds sectors with no primary or additional
tags. Shared sector targeting, plane transforms, lighting, terrain/damage,
destructible health, relevant ACS counts/queries/waits and scroller targeting
now recognize additional tags. Trigger-side fallback for tag-zero actions in
TargetSectors remains unchanged.

Audit corrected two scroller edge cases: CopyScroller suppresses copies only
for actual stored source-tag membership, so tag zero may duplicate on an
untagged destination; runtime SetScroller scans actual membership before
creation, so repeated nonzero-rate tag-zero calls append thinkers on untagged
sectors instead of updating them. This follows native source behavior.
Static map tags survive simulation copies and same-map archive continuation;
no save format change was needed.

Added 15 regressions covering namespace gates, negative/zero/deduplicated tags,
full-width IDs, shared targeting, startup/runtime scrolling, transforms,
CopyScroller suppression, native tag-zero cases and save/checksum continuation.
Release validation: **3,703 tests passed (2,679 Playsim; 451 MapLoader)**;
build has zero warnings/errors. Full FScanner string grammar, dynamic tag
serialization, mixed thinker ordering, slopes, replication and native rendered
acceptance remain open. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: multiple UDMF line IDs (2026-10-01)

Converted UDMF linedef `moreids` import and shared line targeting against
`src/maploader/udmf.cpp`, `FScanner::CheckNumber` in
`src/common/engine/sc_man.cpp`, and `FTagManager::AddLineID` in
`src/playsim/p_tags.cpp`. ZDoom, ZDoomTranslated and Vavoom namespaces admit
additional IDs; Doom/Hexen/Heretic namespaces ignore them. The primary ID is
retained, zero/unset (-1) entries and duplicates are omitted, and additional
IDs preserve input order. Numeric-prefix parsing supports decimal, signed,
hexadecimal, octal and MAXINT tokens and stops at the first invalid token.

LevelLine exposes a shared HasId predicate and read-only imported ID list.
ACS first/all line lookup, model/offset startup wall scrollers and runtime
wall/offset/scale actions use additional IDs without duplicating a matched
line. Simulation copies retain immutable map IDs; archives continue to restore
thinkers against the same source map without a format change. Unset -1 cannot
match a line through shared lookup.

Added 16 regressions covering all namespace gates, duplicate/unset/zero
filtering, numeric forms and prefix termination, shared lookup, both startup
wall models, runtime wall/texture actions and save/checksum continuation.
Release validation: **3,688 tests passed (2,673 Playsim; 442 MapLoader)**;
build has zero warnings/errors. Remaining scope includes multiple sector tags,
full native FScanner grammar inside moreids strings (comments/quoted tokens
and extreme 64-bit overflow), dynamic tag-manager serialization, mixed thinker
ordering, slopes, replication and native rendered acceptance. Gameplay phases
1-3 remain open.

## Conversion and audit: runtime wall actions and ACS dispatch (2026-10-01)

Converted runtime Scroll_Texture_Both (221) against `LS_Scroll_Texture_Both`
in `src/playsim/p_lnspec.cpp`: ID zero fails, positive IDs select front walls,
negative IDs select back walls, X is (left-right)/64 and Y is (down-up)/64.
Native SetWallScroller supplies all-part rate updates and removes matching
thinkers when both rates are zero. Existing controllers keep their height
history and velocity when rates change. Missing IDs succeed without mutation.
The minimum signed integer ID safely produces no match rather than overflowing.

Wired both Both (221) and Scroll_Wall (52) into direct and stack ACS dispatch,
reusing map activation logic. Audit corrected Scroll_Wall's ID-zero return and
preserved native fixed-point rate decoding, nonzero side selection, part masks
and unset-ID behavior. Full-suite validation exposed legacy synthetic maps
with Unknown format that use ACS 52 as a Doom exit; their established fallback
remains intact while declared map formats use the native wall action.

Added 20 regressions covering both actions through all three dispatch routes,
both sides, native rate signs, invalid/missing IDs, opposing-rate removal and
controller history/velocity preservation. Release validation: **3,672 tests
passed (2,668 Playsim; 431 MapLoader)**; build has zero warnings/errors.
Remaining gaps include multi-ID lines/sectors, mixed thinker ordering, full
map translation, slopes, replication and native rendered acceptance.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: Doom directional wall mappings (2026-10-01)

Converted Doom binary wall specials 48/85 and 422-428 against
`wadsrc/static/xlat/base.txt`, `wadsrc/static/xlat/defines.i`, and the directional
branches in `src/maploader/specials.cpp`. SCROLL_UNIT is 64 and native actions
divide by 64, giving one texture unit per tick: 48 moves +X; 85/422 move -X;
423/424 move +Y/-Y; 425-428 select all four diagonal sign combinations.
Translation ignores Hexen arguments and sector tags, scrolls the source front
wall's texture parts, and retains the native 3D midtexture exclusion.

Cardinal specials retain their raw Doom numbers in the managed simulation for
compatibility, corresponding to native restoration of translated directional
specials. Diagonal Both specials are consumed at startup. Hexen/UDMF action
numbers remain separate. Existing constant thinker handling supports runtime
rate updates/removal, archive version 14 and checksum continuation without a
new save format. Source maps remain unchanged across simulation starts.

Added 18 regressions covering every mapping, exact two-tick rates, all parts,
front-side isolation, special retention/consumption, 3D midtextures, saved
continuation without duplicates, format separation and runtime update/removal.
Release validation: **3,652 tests passed (2,648 Playsim; 431 MapLoader)**;
build has zero warnings/errors. Remaining scope includes full native translation,
multi-ID lines/sectors, mixed thinker insertion order, slopes, rendering flags,
replication and native rendered acceptance. Gameplay phases 1-3 remain open.

## Conversion and audit: offset-driven wall scrollers (2026-10-01)

Converted Scroll_Texture_Offsets (225) against `src/maploader/specials.cpp`.
Rates are negative source middle X offset and positive middle Y offset, divided
by max(1, argument 3). Argument 0 selects texture parts with native all-part
defaults; argument 1 selects line IDs or the source wall at zero. Unlike wall
model scrollers, a nonzero ID includes the source when it matches. Argument 2
selects displacement/acceleration using the source front-sector height sum.
Startup samples offsets once and consumes the special on the simulation copy.

Converted Doom 255 and MBF21 1024/1025/1026 mappings from
`wadsrc/static/xlat/base.txt`: 255 scrolls its own wall with divisor one;
1024-1026 use the Doom tag as line ID and divisor eight, with constant,
displacement and acceleration respectively. Audit uncovered dropped binary
sidedef horizontal offsets and missing top/bottom vertical offsets; Doom
binary loading now imports both base offsets into all three texture parts.

Partial controlled walls retain their native part masks through ticks, runtime
rate updates/removal, checksums and archive version 14. New archive kinds
19-30 represent masks 1-6 with displacement/acceleration pairs; existing all-part
kinds 17/18 remain unchanged. Version 13 retains existing partial controllers
while restoring its supported all-part controllers. Target validation rejects
malformed records before restore mutates the clock.

Added 35 regressions covering rates/divisors, all part masks, controller modes,
Doom/MBF21 translations, all twelve partial-controller archive combinations,
runtime updates/removal, legacy saves, source targeting, fixed startup rates,
invalid targets and binary offset import. Release validation: **3,634 tests
passed (2,630 Playsim; 431 MapLoader)**; build has zero warnings/errors.
Remaining scope includes Doom directional wall translations, native multi-ID
lines, mixed thinker ordering, slopes, rendering/interpolation flags,
replication and native rendered acceptance. Gameplay phases 1-3 remain open.

## Conversion and audit: UDMF wall accumulation and directional actions (2026-10-01)

Corrected UDMF wall initialization against `src/maploader/udmf.cpp` and the
CreateScroller default argument in `src/maploader/maploader.h`. Native parsing
computes per-part selection masks, but its wall creation call omits that mask
and defaults to all texture parts. Managed initialization now reproduces that
observed native behavior: each imported entry creates a separate all-part
thinker, rather than replacing an existing line scroller. UDMF thinkers are
created before linedef map-start thinkers, matching native load order.
Imported mask metadata remains intact; this is fidelity to the checked-in
native creation call, not a claim that native per-part intent is implemented.

Converted constant directional wall actions 100/101/102/103 and map-start
Scroll_Texture_Both (221) against `src/maploader/specials.cpp`. Directional
rates use argument 0 divided by 64, and part masks default to all for nonpositive
or unknown-bit values. Native directional specials are retained for compatibility;
Both is consumed, creates only for ID zero, and computes X/Y from opposing
argument differences divided by 64. Doom binary action numbers remain separate.
Existing version-13 archive/checksum coverage persists these constant thinkers.

Added 20 regressions covering all directions, every valid part mask and defaults,
combined rates, nonzero-ID startup behavior, Doom format separation, independent
UDMF/model rate accumulation, native UDMF mask omission, controller retention
and archive continuation. Release validation: **3,599 tests passed (2,596 Playsim)**;
build has zero warnings/errors. Remaining scope includes offset-driven wall
scrollers (225), Doom directional translations, native multi-ID lines, mixed
controlled/constant tick ordering, slopes, rendering flags, replication and
native rendered acceptance. Gameplay phases 1-3 remain incomplete.

## Conversion and audit: wall-model scrollers (2026-10-01)

Converted map-start Scroll_Texture_Model (222) for Hexen/UDMF and Doom binary
specials 218/249/254 from `wadsrc/static/xlat/base.txt`. Audited target selection
against `MapLoader::SpawnScrollers` in `src/maploader/specials.cpp` and projection
against the wall-model DScroller constructor in
`src/playsim/mapthinkers/a_scroll.cpp`. Source linedef direction supplies the
world-space rate; target linedef direction/length projects it into wall X/Y
offset motion. Nonzero IDs select other matching lines and exclude the source;
ID zero scrolls the source wall. All front texture parts scroll, preserving the
native two-sided 3D midtexture exclusion.

Source front-sector floor-plus-ceiling height drives displacement (Doom 249)
or acceleration (Doom 218); Doom 254 remains constant. Controlled walls retain
history/velocity and participate in checksums. Managed archive version 13
stores those thinkers with sidedef-target validation; version 12 retains
existing controlled walls. Runtime SetWallScroller updates now touch every
matching duplicate/controller while preserving history, and zero rates remove
matching wall thinkers as native does. Invalid controllers, sidedefs and
zero-length target walls fail explicitly instead of generating nonfinite rates.

Added 21 regression cases and replaced three prior unconverted-Doom assertions.
Coverage includes cardinal/diagonal projection, self targeting, source exclusion,
all controller modes, Doom translation, 3D midtextures, duplicates, runtime
updates/removal, legacy saves, controller archive/checksum continuation and
invalid saved targets. Release validation: **3,579 tests passed (2,576 Playsim)**;
build has zero warnings/errors. Remaining gaps include native multi-ID lines,
mixed thinker insertion order, UDMF/map-start scroller interactions, slopes,
decal/interpolation rendering flags, replication and native rendered acceptance.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: Doom plane scroller translation (2026-10-01)

Converted the plane-scroller subset of native `wadsrc/static/xlat/base.txt`
for Doom binary maps: 214-217 accelerate, 245-248 use displacement controllers,
and 250-253 use constant linedef-direction scrolling. Within each group the
four specials select ceiling texture, floor texture, carry only, and floor
texture plus carry. Doom line tags select targets, linedef deltas supply rates,
and controlled modes retain front-sector height history. CopyScroller specials
352/353/354 translate to ceiling/floor/floor-plus-carry masks 1/2/6. Existing
plane/carry initialization, copies, saves and checksums handle decoded settings.

Audit found that raw Doom 223/224 were incorrectly consumed as Hexen plane
scrollers; native defines them as friction and wind. Corrected the format
boundary so those actions remain intact. Doom wall-model specials 218/249/254
remain unconverted and are also preserved. Translation is limited to Doom
binary input and does not reinterpret Hexen/UDMF action numbers. Original
source lines retain their specials; only simulation copies consume startup
scrollers. Signed copy masks retain native bit selection, including -1.

Added 27 regressions covering all twelve plane mappings, all three copy
mappings, format separation, signed copy masks, preserved unrelated specials,
controller-copy runtime updates and save/checksum continuation. Release
validation: **3,561 tests passed (2,558 Playsim)**; build has zero warnings/errors.
Remaining work includes wall-model scrollers, full Doom/UDMF translation,
friction/wind gameplay, multi-tag sectors, mixed thinker ordering, slopes,
replication and native rendered acceptance. Gameplay phases 1-3 remain open.

## Conversion and audit: runtime SetScroller updates (2026-10-01)

Converted native tag-wide SetScroller behavior from
`src/playsim/mapthinkers/a_scroll.cpp` and Scroll_Floor/Scroll_Ceiling runtime
actions in `src/playsim/p_lnspec.cpp`. Runtime actions update all existing
thinkers of the selected type whose destination sector has the requested tag.
Updates preserve duplicate thinkers, controller references, controller height
history, acceleration velocity and carry affect masks. A zero rate keeps the
thinker, including accumulated acceleration. If any matching thinker exists,
native code suppresses creation on other sectors sharing the tag. Only an
absent type/tag and a nonzero rate creates new constant scrollers.

Managed runtime dispatch now uses that rule rather than replacing scrollers
per sector. Existing direct single-sector replacement helpers retain their
separate semantics and have corrected documentation. Controlled plane/carry
and accelerated carry rates are mutable; archive version 12 and checksums
already capture their updated values. Audit also corrected initially zero-rate
carry creation so stationary map-start thinkers remain eligible for updates.

Added 15 regressions covering controller preservation, zero-rate velocity and
resumption, duplicates, sparse tagged coverage, carry affect masks, initial
creation, stationary carry thinkers and exact save/checksum continuation.
Release validation: **3,534 tests passed (2,531 Playsim)**; build has zero
warnings/errors. Remaining gaps include native multi-tag sectors, mixed
thinker ordering, sloped controllers, Doom CopyScroller translation,
replication and native rendered acceptance. Gameplay phases 1-3 remain open.

## Conversion and audit: Sector_CopyScroller (2026-10-01)

Converted map-start Sector_CopyScroller (58) against the discovery pass and
Scroll_Ceiling/Scroll_Floor branches of `MapLoader::SpawnScrollers` in
`src/maploader/specials.cpp`. Copy lines are collected before source scrollers,
independent of map line order. Argument 0 matches the source scroller tag;
argument 1 bits 1/2/4 select ceiling texture, floor texture and floor carry.
Unknown bits do not select additional components. Destinations come from the
copy line's front sidedef. The copy inherits the source rate, controller and
acceleration, while texture rotation follows the destination sector.

Audit verified native same-tag suppression, original floor mode predicates,
duplicate copy/source accumulation, copies with no originally tagged sector,
and the absence of recursive copying. Copy specials are consumed only on the
simulation copy. Format gating preserves Doom binary special 58; this action
is recognized only in Hexen/UDMF maps. Invalid front sides/sectors fail
explicitly. Existing version-12 scroller persistence/checksums cover copied
thinkers without a new archive format.

Added 24 regression cases, including all selection masks, source/copy ordering,
destination rotation, inherited displacement/acceleration and archive
continuation. Release validation: **3,519 tests passed (2,516 Playsim)**;
build has zero warnings/errors. Remaining scope includes runtime SetScroller
updates of controlled thinkers, multi-tag sectors, mixed thinker ordering,
sloped controllers, replication and native rendered acceptance. Native Doom
352-354 translation into CopyScroller remains outside this managed action.
Gameplay phases 1-3 remain incomplete.

## Conversion and audit: map-start floor scrolling (2026-10-01)

Corrected Scroll_Floor (223) initialization against
`src/maploader/specials.cpp`: argument 1 contains controller/direction flags,
argument 2 selects texture/carry behavior, and arguments 3/4 contain biased
rates. Bit 4 uses linedef direction instead. Texture scrolling applies when
mode is not 1; carry applies for every positive mode. Independent map lines
append thinkers, tag zero selects untagged sectors, and startup consumes the
special on the simulation copy. Removed unused initialization helpers that
encoded the runtime argument layout. Runtime action dispatch stays separate.

Generalized controlled ceiling texture thinkers to controlled plane thinkers.
Floor displacement and acceleration use the same floor-plus-ceiling controller
height history as native DScroller. Rotation affects texture motion only;
carry retains world-space direction. Controller flags 2/3 preserve accumulated
motion when the controller stops and cancel it with opposite displacement.

Managed archive version 12 includes floor controller history/velocity, with
checksum coverage. Version 11 preserves existing floor controller thinkers;
ceiling and floor restore inclusion flags independently select replacement.
Corrected seven existing map-start fixtures that used runtime arguments and
added 21 regressions for mode predicates, all controller/mode combinations,
duplicates, direction flags, tag zero, rotation, archive continuation,
legacy saves, invalid controller references and checksum discrimination.

Release validation: **3,495 tests passed (2,492 Playsim)**; build has zero
warnings/errors. Remaining scroller conversion includes CopyScroller,
runtime SetScroller updates of existing controlled thinkers, mixed thinker
ordering, sloped controllers, replication and native rendered acceptance.
This does not certify completion of gameplay phases 1-3.

## Conversion and audit: controlled ceiling scrolling (2026-10-01)

Converted map-start Scroll_Ceiling controller flags 1/2/3 against native
`src/maploader/specials.cpp` and `DScroller::Tick` in
`src/playsim/mapthinkers/a_scroll.cpp`. The front sidedef selects the controller.
Floor plus ceiling height changes drive displacement; acceleration accumulates
before the zero-motion check, preserving scrolling when the controller stops.
Ceiling texture rotation compensation applies to the resulting motion.

Managed archive version 11 stores controller references, last height and both
velocity components. Older archives retain existing controlled ceiling thinkers.
Controller state participates in checksums and invalid references fail before
restore mutates the simulation clock. Regression coverage verifies all three
flag combinations, reversing motion, ceiling-height control, save continuation,
version-10 compatibility and invalid controller input.

Release validation: **3,474 tests passed (2,471 Playsim)**; build passes with
zero warnings/errors. Remaining gaps include map-start floor scrolling,
runtime SetScroller updates of existing controlled thinkers, mixed thinker
insertion ordering, sloped controller planes, replication and native rendered
acceptance. Gameplay phases 1-3 remain incomplete.

Updated: 2026-09-27. Cumulative baseline: `55f8fa46` (923 tests), including the
preceding `0a37c839` gameplay, maps/mods and invasion work. The first 1,065 tests
and conversion work were committed as `d792c54a`; subsequent changes are included
below. Validation reruns the full managed regression suite.

**Result: phases 1–3 remain incomplete.** Here phase 1 means gameplay foundation,
phase 2 means combat/AI, and phase 3 means maps/mods. The older numbered audits
describe smaller protocol/server milestones; their "complete" labels do not
certify these gameplay phases or the full C++ conversion.

## Audit: word-format BEHAVIOR binding (2026-09-27)

A Hexen map with an ACS\0 BEHAVIOR lump now registers scripts when the
simulation starts and queues OPEN scripts for the next tic. `AcsMapBehaviorTests`
covers a sector-light change, number-ordered OPEN scripts, a module-address
branch past an early terminate, closed/enter scripts staying idle, empty
directories, and explicit rejection of enhanced, redesigned, unterminated,
oversized, duplicate, and truncated type/argument values. Release build with
warnings as errors is clean. `git diff --check` is clean.

The first binder draft failed its own audit and was corrected before this note:

- Duplicate detection called a helper that always returned false, so a second
  script with the same number would have replaced the first. Registration now
  uses a set and happens only after every script validates.
- Script bytes were cut at the first terminate. ACC can jump over that
  instruction, so the branch fixture died and left the light unchanged. Each
  script now owns the bytes from its address to the next script address, or to
  the directory when the directory follows the code. `TryWalkWordSpan` decodes
  that whole range.
- Old-format script type and argument count were stored in a byte. Type 257
  would have become OPEN, and argument count 256 would have become zero. The
  directory reader now keeps the full integer, and the binder rejects both.

Still true after those corrections:

- This is not native parity and does not finish master-plan step 1. Compact
  modules, libraries, functions, strings, map arrays, and jump-table chunks are
  not loaded. A jump from one script into another script's span stops the fiber.
- Duplicate script numbers and two scripts at the same address are rejected.
  Native old-format lumps warn and can keep both numbers. One program slot per
  number cannot do that safely.
- Only OPEN scripts start at map load (`maploader/specials.cpp`, `runNow` false).
  ENTER scripts start later, once per spawned player. Respawn, death, and the
  other known types are registered and wait.
- `LegacyHexenDelay` stays off. Native adds that extra tic only for
  `GAME_Hexen`, and this actor model is Doom.
- OPEN locals stay zero. Native passes one zero `arg1`, which matches that.
- A present but empty or unsupported BEHAVIOR lump fails the boot. A Doom map
  with no BEHAVIOR lump does not.
- If enqueue failed after registration, the VM would already contain programs.
  Enqueue fails only when the number was not registered, which this path has
  just done. The constructor still discards the simulation on any bind error.

`dotnet test csharp/HCDE.sln -c Release` reported three failures, all in the
untracked `Phase7AcsTests.cs`, which never loads a BEHAVIOR lump. Divide by zero
stops the fiber in the committed VM; that test still expects a pushed zero.
Thing-count and the first-tic timer expectation also disagree with the committed
VM. Those failures are outside this slice. The new behavior tests passed
(13), and MapLoader's existing walker tests passed (396).

## Audit: flat dropoff and ENTER startup (2026-09-27)

`ActorPhysics.TryMove` now refuses a move when the destination floor is strictly
more than `MaxDropOffHeight` (default 24) below the floor the actor is already
in. `AllowDropOff` is set on `PlayerPawn` and `ProjectileActor`. Floating actors
are also exempt. `NoGravity` alone is not. A drop of exactly 24 is allowed.
`GameplayFoundationTests.MonsterDropOffMatchesNativeLimit` and
`PlayerMayWalkOffATallLedge` cover the monster, floater, flag, and player cases.
The existing 24-unit player fall test still passes.

This is the center-of-actor sector change, not `PIT_CheckLine`. A monster whose
radius overlaps a ledge while its center stays on the high floor is not stopped.
`COMPATF_CROSSDROPOFF`, `MF2_ONMOBJ`, `MF5_AVOIDINGDROPOFF`, and `MF5_NODROPOFF`
are not implemented. Player corners now take the two-clip `FSlide::SlideMove`
path described in the corner audit below. Monster and missile blocks still
use one clip against the first blocking line.

`AllowDropOff` and `MaxDropOffHeight` are mixed into the actor checksum.
`SimSavegame` still stores a pose, not those fields. `RestoreState` writes the
pose back onto the same actor, so a non-default flag survives that restore. A
fresh actor built only from the pose would lose it. That is the same gap as
`NoGravity` and `Floating`.

ENTER follows `p_mobj.cpp`: after OPEN is queued, each spawned player gets
`ExecuteAlways` for every ENTER script, with activator set to that player and
one zero argument. Number order matches OPEN, which is not the native directory
order. No player means the script stays idle, which is why
`ClosedAndEnterScriptsAreRegisteredWithoutRunning` still holds. Save restore
does not start ENTER again. Death and respawn scripts start later, on those
events, and are covered in the respawn audit below. A second player uses
always-instances; a normal execute would reject the second start while the
first fiber is still queued.

Single-player spawn still requires the Hexen single-player thing flag, including
player starts. An ENTER script cannot run for a player the spawner drops.

`dotnet build csharp/HCDE.sln -c Release -warnaserror` is clean, and
`git diff --check` is clean. The full Release suite still fails the same three
untracked `Phase7AcsTests` cases (divide by zero, thing count, first-tic timer).
Playsim otherwise passed 2,013 tests. Those three failures do not load this
BEHAVIOR path and are not a dropoff or ENTER regression.

## Audit: negative overkill health (2026-09-28)

`P_DamageMobj` subtracts the post-armor remainder and leaves health negative.
`GetGibHealth` is `-SpawnHealth`. Extreme death requires `health < gibhealth`
and a `Death.Extreme` state. This slice stores that remainder. Spawn paths set
`GibHealth` from the spawn health. The default state table has no extreme
frame, so the death state stays the normal one unless a caller configures
`ExtremeDeathState`. Players still absorb with the existing armor percent.
Monsters have no inventory item to absorb with, and a test locks that a
30-health monster hit for 80 ends at -50.

`RestoreHealth` no longer clamps at 0, so the pose round-trips a negative
value. `GibHealth` and `ExtremeDeathState` are checksummed and are not pose
fields. Sector healing already ignores dead actors, so a negative corpse is
not revived by a heal sector. A killing blow still skips the pain state
because the health write happens first.

The Release suite after this change still fails the same three untracked
`Phase7AcsTests` cases. Playsim otherwise passed 2,015. The warn-as-error
build is clean.

## Audit: use trace side and range (2026-09-28)

`P_UseLines` looks along the player's yaw for `UseRange` (64). `P_UseTraverse`
activates the back side only for `SPAC_UseBack`, and that activation eats the
press. An open line that cannot be used from the side the player is on does
not stop the trace. `ActivateUses` now does that for flat maps. UDMF
`playeruseback` is stored on the line. The trace origin is the player's map
position; portals are not applied. The block test is the existing hitscan
opening at chest height.

`UseRange` is mixed into the player checksum and is not a pose field. The
use flags are mixed into the line checksum. They live on the loaded map, so
a pose restore does not need to write them back.

A one-sided line used from its back side now stops the trace without
activating. `DoomSwitchExitRequiresUseAndClearsOnce` had the player on that
back side. The line winding was turned so the origin is the front, which is
the side `P_VanillaPointOnLineSide` calls side 0.

## Audit: player death and respawn (2026-09-28)

`p_interaction.cpp` starts `SCRIPT_Death` when a player dies and sets
`respawn_time` to `level.time + TICRATE`. `DeathThink` buffers a rising edge
of use, attack, or alt-attack. After the wait, single-player without a
respawn cvar reloads the level in `G_DoReborn`. Coop and deathmatch respawn
at a player start. `PST_REBORN` then starts `SCRIPT_Respawn`.

The managed path does the wait, the rising edge, and the mode split.
`ReloadRequested` is the single-player result. Nothing in the simulation
loads another map. Coop, deathmatch, and `AllowSinglePlayerRespawn` move the
same pawn to its start (`thing.Type == PlayerNum + 1`, otherwise the first
player start), restore spawn health, clear velocity, and enter the spawn
state. Deathmatch inventory goes back to 50 bullets, fist, and pistol.
Coop keeps what the player was carrying. `ForceRespawn` auto-revives
deathmatch after the wait. `sv_norespawn` is the later no-respawn section.

A command from a dead player is not stored. Its use or attack edge can arm
respawn, and a later revive does not replay a weapon slot from that command.
A button that was already down on the killing tic does not arm.

`SCRIPT_Death` runs on the death tic, before the end-of-tic ACS pass.
`SCRIPT_Respawn` runs on the revive tic the same way. Both use
`ExecuteAlways` and one zero argument, in script-number order.

The new sim flags and the player's respawn tic, armed bit, and attack edge
are in the checksum. They are not pose fields. `RestoreState` on the same
actor keeps them. The attack edge is not written by the pose, unlike
`UseHeld`.

Still absent: random deathmatch starts, the coop inventory filter, weapon
drop, and the roster. Phase 1 stays open. Death facing is the section after
ice corpses.
Psprites and a native tick trace are the other open gates. Monster armor is
the section after crouch.
Player stacking is the following section. Crouch is the section after it.

## Audit: player corner slide (2026-09-28)

`P_XYMovement` calls `P_SlideMove` only for a non-missile with `MF2_SLIDE`
or `MF2_BLASTED`. `FSlide::SlideMove` traces the leading corners, moves up
to the nearest line, and `HitSlideLine` clears the into-wall axis. If that
clipped move is blocked, it retries, then stairsteps Y and then X.

The player path now does two of those clips. The line is the one the actor
center reaches first along the step, so line order does not choose the wall.
A vertical line clears X and a horizontal line clears Y. The second clip
covers the corner where the first slide runs into the other wall. No new
checksum field is involved: the result is position and velocity, which the
pose already stores.

Not in this subset: the three bounding-box corner traces, the blockmap,
`COMPATF_WALLRUN`, icy bounce, and slope walking. Monsters and missiles stay
on the old single clip against whichever blocking line `TryMove` returns
first. Actor-versus-actor blocks still split X, then Y.

## Audit: player stacking (2026-09-28)

`P_TryMove` treats a solid non-player as a step when `Top() - Z()` is within
`MaxStepHeight` and the player still fits under the ceiling. `MF2_PASSMOBJ`
is what lets the player finish that move. The Z pass then sets the player
on that top and clears downward velocity.

A grounded player now does that. Height 24 is stood on. Height 25 blocks.
A ceiling that the head would enter blocks. Once up, that top is the floor
for gravity, landing, and `OnGround`, so a fall stops on the actor instead
of the sector. The result is position and `OnGround`, which the pose already
stores. There is no rider id. After a restore, the next tic sees the top
under the saved feet and keeps the player there.

Monsters do not step up. Corpses are already non-solid, so they are not
platforms. Crush, `MF4_ACTLIKEBRIDGE`, and moving the actor the player
stands on are absent.

## Audit: crouch (2026-09-28)

`CheckJump` runs before `CheckCrouch`. `BT_CROUCH` is button bit 3. The
command sink now copies that bit. Factor changes by `CROUCHSPEED` (`1/12`)
and clamps to `[0.5, 1]`. Height becomes `FullHeight * factor` and view
height becomes `41 * factor`. Height is rewritten only when the factor
changes, so a floor test that sets a taller body is left in place while the
player is standing. Growing calls `TryMove` at the new height, so
a ceiling of 30 keeps a half-height player crouched. A jump while the factor
is below 1 sets the stand-up lock and does not set vertical velocity. Death
restores the spawned height.

`CrouchFactor`, `FullHeight`, `ViewHeight`, and the lock are checksummed.
They are not pose fields. Height is already in the checksum. A same-actor
restore keeps the live body. Crouch sprites, fake-floor triggers, and button
bits other than attack, use, jump, and crouch are absent.

## Audit: monster armor (2026-09-28)

`P_DamageMobj` calls `AbsorbDamage` for a non-player when the actor has an
inventory and the hit is not `DMG_NO_ARMOR` or `DMG_FORCED`.
`ABasicArmor::AbsorbDamage` saves `full absorb + (damage - full) * SavePercent`
and caps that by `Amount` and `MaxAbsorb`. This subset has no full-absorb
bonus and no max-absorb cap, so the save is the same integer formula the
player already uses: percent 33 is `damage / 3`, and every other percent is
`damage * percent / 100`. The armor amount caps the save.

A non-player stores that amount and percent on the actor. Players still
absorb only from `PlayerInventory` and do not clear their percent. When a
non-player's amount reaches 0 because some armor was spent, the percent is
cleared, which is the BasicArmor branch. A hit that spends nothing leaves a
zero amount's percent alone. `BypassArmor` skips both paths.

A former human with health 30 and no armor, hit for 80, still ends at -50
with no armor loss. The same actor with 100 green armor ends at -24, armor
74, and percent 33. Ten points at 50 percent against 100 damage save 10,
leave health at 10, and clear the percent.

`Armor` and `ArmorSavePercent` are in the checksum. They are not pose fields.
`RestoreState` on the same actor keeps them. There is no armor pickup chain,
no reserve `BasicArmorPickup` when the amount hits 0, no Hexen armor, and no
damage-type `IgnoreArmor` beyond the bypass flag. Spawned monsters still
start with armor 0.

Phase 1 stays open. Crush, corpse platforms, the rest of the actor states
and roster, weapon raise and psprites, and a native tick trace are still
absent. Buddha is the following section.

## Audit: Buddha (2026-09-29)

`P_DamageMobj` leaves a player or monster at 1 health when `MF7_BUDDHA`
would otherwise kill them. This subset is that flag on the actor. The raw
hit is compared with `TELEFRAG_DAMAGE` (1,000,000) before armor is
considered, so green armor does not turn a telefrag into a survivable hit.
`DMG_FORCED` skips armor, god mode, invulnerability, and Buddha.
`DMG_FOILBUDDHA` kills a non-player and leaves a player at 1, matching the
player branch, which does not consult that flag.

The clamp happens before `Health` is assigned. Assigning a negative value
first would enter the death state and, for a player, note the death.
A hit from 30 health for 80 ends at 1, death count 0, and not the death
state. A hit for 10 ends at 20. Armor still saves first: 100 green armor
on that 80-damage hit saves 26, and Buddha then stops the remainder at 1.

`Buddha` is in the checksum. It is not a pose field. `RestoreState` on the
same actor keeps it. Buddha2, the PowerBuddha inventory item, an inflictor
`MF7_FOILBUDDHA` flag, voodoo dolls, drain, and damage-type deaths are
absent. A hit that Buddha reduces from 1 health back to 1 does not enter
pain, because pain follows a health change.

Phase 1 stays open. Crush, corpse platforms, the rest of the actor states
and roster, and a native tick trace are still absent. Weapon raise is the
following section.

## Audit: weapon raise and lower (2026-09-29)

`A_Lower` adds 6 to the weapon psprite Y until `WEAPONBOTTOM` (128).
`A_Raise` subtracts 6 until `WEAPONTOP` (32), then the weapon can fire.
The tic that reaches the bottom does not also raise. This subset uses that
offset as a fire gate.

The slot command still changes `Selected` on the tic it runs. Native keeps
the old ready weapon until the lower finishes and only then brings up
`PendingWeapon`. Cycling and the command-queue tests depend on the managed
order, so the offset does not delay the selection itself. An attack on the
switch tic does not fire, and it does not start the refire cooldown. After
32 tics the offset is back at 32 and a shot spends ammo. The next tic is
blocked by that cooldown.

A spawned weapon starts at 32. Assigning `Selected` from a test does not
start the animation. Re-selecting the weapon already in hand does not
restart it. Death leaves the offset where it was. Respawn sets it back to
32 and clears the lowering bit. Sprites, flash, bob, `CF_INSTANTWEAPSWITCH`,
and the pending-weapon handoff are absent.

`WeaponOffsetY` and the lowering bit are in the checksum. They are not
pose fields. `RestoreState` on the same actor keeps them.

Phase 1 stays open. Crush, corpse platforms, the rest of the actor states
and roster, psprite sprites, and a native tick trace are still absent.
Monster bridges are the following section.

## Audit: monster bridges (2026-09-29)

`PIT_CheckThing` lets a non-floating monster walk onto a solid actor with
`MF4_ACTLIKEBRIDGE` when that top is within `MaxStepHeight` and the head
fits. Players already step onto any solid non-player through the
`MF2_PASSMOBJ` path. This subset adds the monster case only.

A bridge of height 16 or 24 is a step. Height 25 blocks, and the monster
stays where it was. The monster then uses that top as its floor, so the
next sector fit does not pull it back down. A dead actor does not block
and is not a floor, even with the flag set, because `BlocksActors` requires
the actor to be alive. Floating actors and projectiles do not take the step.
There is no rider id, so the bridge does not carry the monster when the
bridge itself moves. Crush while standing on it is absent.

`ActsLikeBridge` is in the checksum. It is not a pose field. `RestoreState`
on the same actor keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Ice corpses are
the following section.

## Audit: ice corpses on corpses (2026-09-29)

`P_TestMobjZ` skips a corpse obstacle unless the mover has `MF_ICECORPSE`.
Ordinary actors walk through dead bodies. This subset is that exception.

A corpse stays non-solid for players, living monsters, and door checks.
An ice corpse treats a dead solid actor as a step. Height 16 or 24 is a
floor, and the ice corpse stands there. Height 25 or 56 blocks, and the
ice corpse does not move. Headroom still has to fit under the ceiling.
The corpse is not carried, and it does not crush the actor standing on it.
Shatter, frozen ticks, and blood are absent.

`IceCorpse` is in the checksum. It is not a pose field. `RestoreState` on
the same actor keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Death facing is
the following section.

## Audit: death facing (2026-09-29)

`DeathThink` turns a dead player toward `player.attacker` by at most 5
degrees each tic. The managed killer is `LastDamageSourceId` from the blow
that set it. A north killer from an east-facing pawn is 5 degrees after one
tic and due north after 18. The pawn does not turn when that id is missing
or is the pawn itself.

The same tic lowers view height by 1 until it reaches 6, and moves pitch 3
degrees toward 0, snapping once the remainder is under 3. Native
`Uncrouch` only rewrites the view when a crouch is being cleared, so the
drop is not reset on later dead tics. An ice corpse skips the view and
pitch settle and still turns. Damage and poison counters are not faded.
Respawn puts the view back at 41 and pitch at 0.

View height is already in the checksum and not in the pose. A same-actor
restore keeps the lowered view. A save written before the drop does not
put that view back, so another dead tic leaves it lower. Angle is already
in the pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Weapon drop is
the following section.

## Audit: weapon drop (2026-09-29)

`PlayerPawn.Die` drops the ready weapon when `sv_weapondrop` is set.
The managed flag is `WeaponDrop`, and it stays off. A death with the flag
clear adds no actor.

When the flag is set, chainsaw, shotgun, super shotgun, chaingun, rocket
launcher, plasma, and BFG spawn that map thing at the corpse. The corpse
keeps the weapon. Fist and pistol have no vanilla thing, so they spawn
nothing. The pickup grants the catalog amount (a shotgun gives 8 shells),
not the ammo the player was holding. Drop lists, probability, and skill
ammo are absent.

`WeaponDrop` is in the checksum. It is not a pose field. `RestoreState` on
the same simulation keeps it. The spawned thing is an actor, so a later
pose that includes actors keeps the thing.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Drain is the
following section.

## Audit: drain (2026-09-29)

`P_DamageMobj` heals a player who has PowerDrain by `int(strength * damage)`
after armor, then `P_GiveBody` stops at max health. This subset stores the
strength on the player. 0 is off. 0.5 is the usual power.

A player at 40 health who hits for 80 gains 40 and ends at 80. Green armor
on that same 80-damage hit saves 26, and the drain uses the remaining 54,
which is 27. A player at 95 who would gain 10 stops at 100. Health already
at 150 stays 150. A dead player does not heal. `DontDrain` on the target
and a hit against oneself grant nothing. Another player can be drained.
There is no PowerDrain inventory item, no OnDrain script, and no heal sound.

`DrainStrength` and `DontDrain` are in the checksum. They are not pose
fields. `RestoreState` on the same actor keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Backpack is
the following section.

## Audit: backpack (2026-09-29)

`BackpackItem` gives every parent ammo its backpack amount and, the first
time, its backpack max. Doom thing 8 is that item. This subset knows the
four Doom pools only.

A player already holding 200 bullets picks one up and ends at 210, with
caps 400, 100, 100, and 600, plus 4 shells, 1 rocket, and 20 cells. A
second one adds 10, 4, 1, and 20 again. When every pool is already at the
new cap the pickup still succeeds and the amounts stay put. Skill ammo
factor is not applied, and there is no tossed empty backpack.

`ResetToPistolStart` clears the flag and restores 200/50/50/300, so a
deathmatch revive loses the backpack. Cooperative respawn keeps the live
inventory, including the raised caps.

`HasBackpack` and the four max amounts are in the checksum. They are not
pose fields. `RestoreState` on the same actor keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. The cooperative
inventory filter is the following section.

## Audit: cooperative respawn inventory (2026-09-29)

`FilterCoopRespawnInventory` walks the dead player's inventory before the
new body keeps it. `dmflags` starts at 0, so each lose flag is off and a
cooperative revive still keeps the pack. Deathmatch never calls it.

Lose inventory is a pistol start, including the backpack. Lose keys drops
red, blue, and yellow. Lose weapons keeps fist and pistol and, when the
selected weapon was removed, readies the pistol, or the fist when that
pistol has no bullet. Lose armor clears the
amount and the save percent. Lose ammo puts bullets back at 50 and zeroes
shells, rockets, and cells. Halve ammo divides a pool above 1. Bullets of
120 become 60. Bullets of 40 become 50. A shell of 1 stays 1. When both
ammo flags are set, lose wins. The backpack and its raised caps stay
unless everything is lost. A shotgun with no shells left is put away.

An enabled single-player respawn uses the same filter. Shared keys,
powerups, and Hexen armor are absent.

The six flags are in the checksum. They are not pose fields.
`RestoreState` on the same simulation keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Deathmatch
starts are the following section.

## Audit: deathmatch starts (2026-09-29)

`G_DeathMatchSpawnPlayer` picks a thing 11 for a deathmatch reborn. This
subset does that on respawn. Level load still puts each player on a player
start. A map with no thing 11 still revives at that player's own start.
Cooperative respawn ignores thing 11. Thing 11 is not spawned as an actor.

The roll is a separate counter, seeded from the sim and mixed into the
checksum. It is not the native `DMSpawn` table and it is not in the pose.
`RestoreState` keeps the live counter. An open spot has no living solid
actor overlapping it. Twenty blocked tries still return the last spot, and
nobody there is telefragged. `SpawnFarthest` picks the start farthest from
the nearest other living player. A tie keeps the earlier start. It does
nothing while that player is the only one alive. A farthest miss falls
through to the random thing-11 pick.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. No-respawn is
the following section.

## Audit: no respawn (2026-09-29)

`DeathThink` revives only when `sv_norespawn` is clear. The flag defaults
off. While it is set, a fresh press still buffers, and the corpse stays
after the 35-tic wait. Single-player does not ask for a reload. Deathmatch
`sv_forcerespawn` does not revive. Clearing the flag on a later tic uses
that buffered press, or the force flag, without a new button. The press is
cleared only when the revive runs.

`NoRespawn` is in the checksum. It is not a pose field. `RestoreState` on
the same simulation keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Skill ammo is
the following section.

## Audit: skill ammo (2026-09-29)

`SKILLP_AmmoFactor` is 2 for baby and nightmare and 1 for the other Doom
skills. `sv_ammofactor` defaults to 1 and multiplies that. The product is
truncated toward zero, matching `int(amount * factor)`.

A clip of 10 becomes 20 on baby, so a pistol start ends at 70. The same
clip on easy stays 10 and ends at 60. An extra factor of 1.5 turns that
clip into 15 and a single rocket into 1. A factor of 0 adds nothing and
leaves the pickup on the map. A baby backpack gives 20, 8, 2, and 40. The
caps stay 400/100/100/600. Health and armor are not scaled.

A dropped weapon sets `IgnoreAmmoSkill`. On baby, that shotgun still gives
8 shells. A map shotgun without the flag gives 16.

`AmmoFactor` and `IgnoreAmmoSkill` are in the checksum. They are not pose
fields. `RestoreState` keeps the live values.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Double ammo is
the following section.

## Audit: double ammo (2026-09-29)

`sv_doubleammo` makes `SKILLP_AmmoFactor` return `DoubleAmmoFactor`. Doom
leaves that at 2 for every skill. The flag defaults off. It replaces the
skill factor. Baby stays at 2, which is the same as baby without the flag,
so a clip of 10 is still 20 bullets added. Easy and normal, which were 1,
become 2, and a pistol start ends at 70.

`sv_ammofactor` still multiplies afterward. Baby with the flag and a factor
of 1.5 turns a clip of 10 into 30, so the start ends at 80. A dropped weapon
still skips the factor and gives 8 shells. Heretic and Hexen set the factor
to 1.5, and that table is absent.

`DoubleAmmo` is in the checksum. It is not a pose field. `RestoreState`
keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Armor absorb
caps are the following section.

## Audit: armor absorb caps (2026-09-29)

`BasicArmor.AbsorbDamage` saves `MaxFullAbsorb` in full, then the save
percent on the rest, and `MaxAbsorb` stops the running total. Doom green
and mega leave both caps at 0, so their save is unchanged: percent 33 is
still `damage / 3`.

A suit with full absorb 10 and 50 percent, hit for 30, saves 20. The next
30 saves 15, because the full slice is already used. A 100 percent suit
with a total cap of 25, hit for 40, saves 25 and the player loses 15. The
next 40 saves nothing. The armor amount still caps a save, and a monster
whose armor reaches 0 still clears its percent.

Picking up mega armor clears both caps and keeps `AbsorbCount`. A pistol
start clears the count as well. There is no spare armor item waiting to
replace an empty suit, and damage types still do not ignore armor.

The caps and the count are in the checksum for the player and for other
actors. They are not pose fields. `RestoreState` keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still absent. Drowning is
the following section.

## Audit: drowning ignores armor (2026-09-29)

`DamageTypeDefinition.IgnoreArmor` is true only for the mapinfo type
`Drowning`. That is the only `NoArmor` entry. A hit tagged `Drowning`
skips armor and still subtracts the full amount from health. Green armor
of 100, hit for 30, stays 100 and health falls by 30. The same hit tagged
`Slime` saves 10 and health falls by 20. The lowercase spelling `drowning`
is a different name and still uses armor.

A sector whose damage type is `Drowning` passes that name into the same
call. Ten points on the first tic leave armor at 100 and health at 90.
A `Slime` sector of 10 saves 3. God mode still blocks the hit. The sector
type was already in the checksum, so no new field was added.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing. Buddha2 is
the following section.

## Audit: Buddha2 (2026-09-29)

`CF_BUDDHA2` is checked before ordinary Buddha. It is a player cheat, so
only `PlayerPawn` has it. A killing blow leaves health at 1, including a
hit of 1,000,000. Ordinary Buddha still dies to that hit.

`DMG_FORCED` is cleared before armor and god mode. A forced hit of 80
against 100 armor at 100 percent saves 80 and leaves health at 30. The
same hit with no armor leaves health at 1. God mode still blocks it.
A hit of 10 from 30 health leaves 20. Monsters do not gain this flag.
The PowerBuddha item is the following section.

`Buddha2` is in the checksum. It is not a pose field. `RestoreState`
keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: PowerBuddha (2026-09-29)

`PowerBuddha` is a powerup with `Duration -60`. A negative duration is
seconds, so the item lasts 2,100 tics. While any tics remain, a player
follows ordinary Buddha. A hit of 80 from 30 health leaves 1. A telefrag
and `DMG_FORCED` still kill. Forced damage still skips armor.
`DMG_FOILBUDDHA` does not remove the item from a player.

A second grant is taken and does not extend the timer while more than
128 tics remain. At 128 or below, the timer resets to 2,100. Every player
tic counts down, including while dead. A deathmatch revive clears it. A
cooperative lose-inventory revive clears it. A cooperative revive that
keeps inventory leaves the tics that remain after the wait.

`PowerBuddhaTics` is in the checksum. It is not a pose field.
`RestoreState` keeps it. The inflictor foil flag is the following section.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: inflictor foil Buddha (2026-09-29)

`MF7_FOILBUDDHA` is `Actor.FoilBuddha`. The monster Buddha check looks at
the inflictor, not the source. A missile with the flag, hitting a monster
at 30 health for 80, leaves health at -50. The same hit with the flag only
on the source, and no inflictor, leaves health at 1. A null inflictor does
not foil. A player with Buddha or PowerBuddha stays at 1. Buddha2 is
unchanged.

Direct impact and blast pass the missile. Melee, hitscan, a charge, and
the archvile's direct 20-point hit pass the attacker. BFG spray still has
no puff actor, so it does not copy a puff's foil flag into the damage.

`FoilBuddha` is in the checksum. It is not a pose field. `RestoreState`
keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing. Stored armor
is the following section.

## Audit: stored armor pickups (2026-09-29)

`BasicArmor` with amount 0 clears its save percent, then uses the stored
`BasicArmorPickup` with the highest save percent. An equal percent keeps
the earlier item. The saved-so-far count stays. The new suit's absorb caps
replace the old ones.

A pickup whose max amount is above 0 is used immediately when the worn
amount is below its save amount. Otherwise it is kept. Doom green and mega
have max amount 0, so a full suit still rejects them and they are not
stored. Skill armor factor stays 1. Hexen armor is absent.

A hit of 400 against 100 green armor saves 100. A stored 40-point suit at
100 percent is used before a stored 80-point suit at 50 percent. The same
hit's remainder is not absorbed by the new suit. Drowning does not enter
this path. A pistol start and a lose-armor revive clear the stored list.
A cooperative revive that keeps inventory leaves it.

The stored list is in the checksum. It is not a pose field. `RestoreState`
keeps it. Monsters still have no pickup list.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing. The pending
weapon is the following section.

## Audit: pending weapon (2026-09-29)

`Weapon.Use` sets `PendingWeapon` when the pick is not the ready weapon.
The ready weapon stays `Selected` while the offset lowers by 6 from 32
toward 128. The tic that reaches 128 copies the pending weapon into
`Selected`, clears the pending weapon, and does not raise. The next tics
raise by 6 until 32. Fire stays refused until then. A shotgun press from
the pistol leaves the pistol selected and the shells unspent. Sixteen
lower tics commit the shotgun at offset 128. Sixteen raise tics return
to 32, and the next attack spends a shell.

A second press replaces the pending weapon. Slot 3 with both shotguns
owned picks the super shotgun first, then the shotgun. Pressing the pistol
slot while the pistol is still ready does not cancel that switch. Cycling
uses the pending weapon when one is set. A revive clears the pending
weapon and snaps the offset to 32. Sprites, flash, and bob are absent.
Instant switch is the following section.

`Pending` is in the checksum. It is not a pose field. `RestoreState`
keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: instant weapon switch (2026-09-29)

`CF_INSTANTWEAPSWITCH` is `PlayerPawn.InstantWeaponSwitch`. It is off
until set. `A_Lower` then sets the offset to 128 and `BringUpWeapon`
puts it back at 32 in the same call. The pending weapon becomes the
ready weapon on that tic. An attack queued with the slot press fires:
five shells become four, and the cooldown is 35. Without the flag the
same press still spends the sixteen lower tics and does not fire.

The flag is in the checksum. It is not a pose field. `RestoreState`
keeps it. Sprites, flash, and bob are absent. Turn 180 is the following
section.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: turn 180 (2026-09-29)

`BT_TURN180` is bit 4 on the user command. A fresh press sets
`TurnTicks` to `(35 / 4) + 1`, which is 9. That tic turns 20 degrees and
leaves 8. Yaw on those tics is ignored. Nine tics from angle 0 land on
180. Holding the button past that does not start another turn. Releasing
it and pressing again does. A press during the turn restarts the 9 tics.

`TurnTicks` and the held bit are in the checksum. They are not pose
fields. `RestoreState` keeps both, so a held button keeps counting down.
A revive clears them. Alt attack, reload, zoom, and run are absent.
View bob is the following section.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: view bob (2026-09-29)

`CalcHeight` sets `player.bob` from horizontal speed squared times
`movebob` (0.25), capped at 16. The view term is that amount times
`sin(BobTimer / 20 * 360) * 0.5`. Speed 8 at timer 5 is a 90-degree sine,
so the offset is 8. Timer 10 is a 180-degree sine, so the offset is 0.
Speed 100 still caps at 16 and the same timer still offsets by 8.
Standing still stays at 0. `ViewHeight` stays 41.

`BobTimer` advances once per player tic, in step with the sim clock.
`ViewBobOffset` is the camera term. It is not added to eye height. Both
are in the checksum and not in the pose. A pose restore sets the timer
from the saved clock tic and recomputes the camera term from the restored
horizontal speed. The weapon swing is separate from this camera term.
Vertical speed is not part of this bob.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: weapon bob (2026-09-29)

`BobWeapon` runs after movement. This port takes the normal style at the
end of the tic, so the between-tic blend is the second sample only. The
angle is `BobTimer * 128 * 360 / 8192` degrees. X is the movement bob
times the cosine. Y is that amount times the absolute sine. Speed 8 is
the cap of 16. Timer 16 puts the sprite at X 0, Y 16. Timer 0 puts it at
X 16, Y 0. Vertical speed is not part of the amount.

The sprite stays at 0 while the weapon is lowering or firing. `wbobfire`
is 0, so a fire tic and the cooldown after it do not swing. Eye height
stays 41. The movement bob and both sprite terms are in the checksum and
not in the pose. A pose restore recomputes them from the restored speed
and the clock tic. Other styles, a non-default range, and the 3D bob are
absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: spawn stomp (2026-09-29)

`P_PlayerStartStomp` runs after a coop or deathmatch revive. A monster
on the spot takes telefrag damage. Another player takes it only in
deathmatch, and that hit goes through god mode and invulnerability.
`Buddha2` still stops at 1. Cooperative leaves the other player alone.
The overlap is a square of the two radii: a gap of exactly that sum is
spared, and one map unit closer is not. A body whose feet are above the
revived player's head is spared. `NoTelefrag` skips the hit unless
`AlwaysTelefrag` is set. A shootable actor with no monster brain is not
a monster, so a decoration is spared. Both flags are in the checksum and
not in the pose. A same-actor restore keeps them. Level load still does
not stomp.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: shared keys (2026-09-29)

`sv_coopsharekeys` stays off. In cooperative play, a key pickup is copied
to every other player, including a dead one. A clip picked up on the same
tic is not copied. Deathmatch does not share, even with the flag set.
`sv_cooplosekeys` drops keys only when sharing is off. Losing the whole
pack still clears them. A card and a skull of one color are the same key.
Puzzle items, the pickup message, and the bonus flash are absent. The
flag is in the checksum and not in the pose. A same-actor restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: depleted backpack (2026-09-29)

`BackpackItem.CreateTossable` marks the tossed pack depleted.
`DetachFromOwner` puts the caps back at 200/50/50/300 and cuts ammo
above those caps. Ammo under the caps stays. Picking up that pack is
`CreateCopy` with `bDepleted`: the caps return to 400/100/100/600 and
no ammo is added. A player who already has a pack still receives the
normal pack amounts, because `HandlePickup` does not look at the
depleted flag. There is no drop-inventory key; `DropBackpack` is the
toss. `Depleted` is in the checksum and not in the pose. A same-actor
restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: typed deaths (2026-09-29)

`P_DamageMobj` picks the death frame from the damage type. An extreme
typed frame, such as `Death.Extreme.Fire`, wins when health is strictly
below the gib line and that frame exists. Otherwise `Death.Fire` is used
even when the hit is past the gib line. A missing type falls through to
the gib frame, then the normal death. The name match is ordinal. Damage
type `Extreme` forces the gib frame and does not look up a typed state.
Generic ice freeze, `MF4_NOICEDEATH`, and the inflictor extreme-death
flags are absent. The registrations are in the checksum and not in the
pose. A same-actor restore keeps them. The default four-frame table has
no typed death, so an unconfigured actor still uses the normal death.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: typed pain (2026-09-29)

`TriggerPainChance` looks up `Pain.Fire` before the normal pain frame.
A registered frame is entered when the pain roll succeeds. A missing
frame, or a name that does not match, uses the normal pain state. The
match is ordinal. A per-type chance replaces `PainChance` for that name
only. Chance 0 never flinches, and another type still uses the actor's
own chance. Electric flicker, poison howling, and wound states landed in
later audits below. The registrations are in the checksum and
not in the pose. A same-actor restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: pain threshold (2026-09-29)

`ReactToDamage` flinches only when the post-armor amount is at least
`PainThreshold`. The default is 0, so any health loss can still flinch.
A hit of 9 against a threshold of 10 drops health and stays in the spawn
state. A hit of 10 enters pain. Green armor saves 10 of a 30-point hit,
so 20 remains and a threshold of 21 stays quiet.
The threshold is in the checksum and not in the pose. A same-actor
restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: forced pain (2026-09-29)

`MF6_FORCEPAIN` on the inflictor skips `PainThreshold` and the pain
roll. A hit of 9 against a threshold of 10 still enters pain. Pain
chance 0 still flinches. A hit that armor absorbs completely still
flinches, because the flag does not require a health loss. The source's
flag does not count unless that actor is the inflictor. `DMG_NO_PAIN`
still blocks the flinch. A dead target does not enter pain. The flag is
in the checksum and not in the pose. A same-actor restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: painless hits (2026-09-29)

`MF5_NOPAIN` on the target and `MF5_PAINLESS` on the inflictor block the
pain frame. A 50-point hit still drops health and stays in the spawn
state. Forced pain does not override either flag. An inflictor that has
both flags stays painless. A player with no-pain stays in spawn the same
way. The source's painless flag does not count unless that actor is the
inflictor. A no-pain monster still wakes. These flags only gate the
flinch. Both flags are in the checksum and not in the pose. A
same-actor restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: generic ice freeze (2026-09-29)

An Ice kill uses `GenericFreezeDeath` when that frame is registered and
the actor has no Ice death of its own. The frame is not extreme, so a
killing blow past the gib line stays on it instead of the gib frame. A
typed Ice death, including an extreme one, still wins. `MF4_NOICEDEATH`
blocks the fallback. A decoration does not freeze. A monster is an actor
with a brain, and a player freezes without one. The name `ice` does not
match. A non-killing Ice hit still enters pain. Dehacked autofreeze-off
and ice-corpse shatter are absent. The state and the flag are in the
checksum and not in the pose. A same-actor restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: inflictor extreme death (2026-09-29)

`MF4_EXTREMEDEATH` on the inflictor can enter the gib frame when health
stops at -50 on a -100 gib line. `MF4_NOEXTREMEDEATH` keeps a -101 kill
on the normal death frame instead of the gib frame. A typed extreme
`Fire` death falls back to the plain `Fire` frame when the inflictor
blocks extreme deaths. The source's flag does not count unless that
actor is the inflictor. Both flags are in the checksum and not in the
pose. A same-actor restore keeps them. Player extremely-dead cheats and
the non-player health clamp past gib landed in the extreme-death-clamp
audit below.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: extreme death clamp (2026-09-29)

A non-player extreme death frame clamps health to `GibHealth - 1` when
the post-hit amount is still on or above the gib line. An inflictor
extreme-death flag on a 150-point kill stops at -101 on a -100 gib line
instead of -50. A player keeps -50 overkill. A kill already past the
gib line stays at -101. Player `CF_EXTREMELYDEAD` landed in the
player-extremely-dead audit below.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: monster damage wake (2026-09-29)

A post-armor hit or forced pain clears `ReactionTics`, sets chase
target to the damage source, and enters `SeeState` when the actor is
still in spawn and that frame exists. Pain runs before wake, so a
flinch blocks see on that tic. `MF5_NOPAIN` still wakes. Target
`MF_JUSTHIT` landed in a later audit. Chase threshold and a partial
`OkayToSwitchTarget` landed in the chase-threshold audit below. Last-enemy
memory and friend checks are still absent. `SeeState` is in the checksum
and not in the pose. A same-actor restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: electric pain flicker (2026-09-29)

After the pain chance passes, `DamageType Electric` rolls the flicker
stream. A roll below 96 enters the typed or normal pain frame and clears
fullbright. A miss sets `RF_FULLBRIGHT` and stays in the prior state.
Forced pain still flinches and skips the flicker roll. The flag is in
the checksum and not in the pose. A same-actor restore keeps it. Poison
howling is absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: typed wound (2026-09-29)

`Wound.Fire` replaces pain when health after the hit is at or below
`WoundHealth`. A hit above the line still enters pain. A different
damage type still uses pain. Health equal to the threshold wounds. The
early-out skips wake on that tic. `WoundHealth` and the typed table are
in the checksum and not in the pose. A same-actor restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: ACS TID inventory and ClearInventory (2026-09-29)

`ClearInventory` (142) and `ClearActorInventory` (284) reset a player to the
pistol-start pack (`ResetToPistolStart`, clears `PowerBuddhaTics`). Monsters are
ignored. `GiveActorInventory`, `TakeActorInventory`, and `CheckActorInventory` use
`AcsActorTid` (`LastOrDefault` for check, all matches for give/take) with native
stack order (top = amount or string id, tid on bottom for three-operand forms).
`SingleActorFromTID` client-side and TID 0 world targets are absent.

`AcsPlayerInventoryTests` covers cooperative two-player give-by-TID, `CheckActorInventory`,
and activator clear.

Release playsim: **2,225** passed on 2026-09-29.

Phase 1 stays open. DECORATE inventory classes and Gate 5 native trace are still
missing.

## Audit: ACS GiveInventory / TakeInventory (2026-09-29)

`GiveInventory`, `GiveInventoryDirect`, `TakeInventory`, and `TakeInventoryDirect`
apply to the activator with native stack order (top = amount, next = string-table
index). `AcsPlayerInventory` maps the same Doom names as `CheckInventory` for
health, armor, keys, ammo pools, and owned weapons.

`AcsPlayerInventoryTests` covers give/take `Clip` ammo and the existing check
fixtures.

Release playsim: **2,222** passed on 2026-09-29.

Phase 1 stays open. TID-targeted give/take landed in the audit above. Gate 5 native
trace is still missing.

## Audit: ACS player inventory queries (2026-09-29)

`PlayerFrags` (122) and `PlayerTeam` (119, always 0) read the activator.
Skull-tag opcodes **104–117** push key flags for blue/red/yellow and master
(all three). Extended skull/card colors push 0. `CheckInventory` / `CheckInventoryDirect`
resolve string-table names to Doom keys, armor, health, ammo pools, and owned
weapons (`AcsPlayerInventory`). `GiveInventory` / `TakeInventory` landed in the give/take audit above. `ThingCount`
skips actors that fail `IsMapActor` (always true until owned inventory items exist).

`AcsPlayerInventoryTests` covers frags, blue card, and `CheckInventoryDirect`
for `Clip`. `FragCount` is checksummed and not in the pose.

Phase 1 stays open. DECORATE class inventory, `CheckActorInventory`, and Gate 5
native trace are still missing.

Release playsim: **2,220** passed on 2026-09-29.

## Audit: BEHAVIOR STRE decrypt (2026-09-29)

`TryDecryptStre` now matches `FBehavior::UnencryptStrings` in `p_acs.cpp`: string
count at chunk dword **3** (payload offset 4), offsets at dword **5+strnum**, ciphertext
at payload base **+ ofs**, post-xor zero terminates each string. A false
`behavior-stre-unterminated` when the null byte sat on the last payload byte is
fixed. The in-place `STRL` header rewrite on dword 0 was removed because enhanced
reads use the decrypted padded table body only.

`MapBehaviorStringTableCodecTests.EnhancedStreChunk_DecryptsLikeNativeUnencryptStrings`
builds a synthetic `STRE` chunk with the native xor scramble. Release MapLoader:
**400** passed on 2026-09-29 (codec subset **4**). Playsim **2,217** unchanged.

Phase 1 stays open. Real map lumps with `STRE`, DECORATE replacements, and Gate 5
native trace are still missing.

## Audit: sector carry scroll buffer (2026-09-29)

Carry rates live in a `SectorCarryScroll` list. Each tic, after `Thinkers.Run`,
`RebuildSectorCarryScrolls` clears `SectorScrollX/Y` and sums every entry, matching
native `FLevelLocals::Tick` memset plus `DScroller::sc_carry` accumulation.
`SetSectorScroll` replaces scrollers on one sector (line `SetScroller` rate update).
`AppendSectorCarryScroll` adds another rate so multiple thinkers on the same sector
sum. Displacement, acceleration, texture scroll, and `MF8_INSCROLLSEC` are absent.

`SectorScrollCarryTests.MultipleCarryScrollsOnSameSectorAccumulateEachTic` covers
additive carry. `ManagedGameplayTraceTests` and `HCDE_CSHARP_PHASE1_NATIVE_TRACE.md`
record a managed-only Gate 5 baseline; native IWAD pairing is still missing.
Release playsim: **2,217** passed on 2026-09-29.

Phase 1 stays open. Gate 5 native recorder and full `DScroller` feature set are
still missing.

## Audit: ACS activator context (2026-09-29)

`ActivatorTid`, `PlayerHealth`, and `LineSide` read the fiber activator and trigger
line context supplied by `AcsVm.Enqueue` / line specials. `ThingCount` TID filtering
now has a positive fixture (`LevelThing.Id` → `Actor.ThingId`). Scroll carry
regressions cover rate updates on repeat `Scroll_Floor` use and multi-tic persistence.

`AcsActivatorQueryTests` and extended `Phase7AcsTests` / `SectorScrollCarryTests`
cover the above.

Phase 1 stays open. `DScroller` displacement/acceleration and gate 5 native trace are
still missing.

## Audit: scroll carry regression pass (2026-09-29)

`Scroll_Floor` carry mode **2** sets carry like native `arg3 > 0` even though floor
texture scroll is still absent. Hexen 201–224 player carry **adds** to line-driven
`SetSectorScroll` on the same sector. `SectorScrollCarryTests` covers both.

ACS regressions now assert `ThingCount` skips dead actors, `ThingCountSector`
honors sector tags, deathmatch `GameType`, and `PlayerCount`. Map-loader `STRE`
chunk id was corrected to native `MAKE_ID('S','T','R','E')`; decrypt parity and a
synthetic encrypted fixture landed in the STRE decrypt audit above.

Phase 1 stays open. `DScroller` thinkers and gate 5 native trace are still missing.

## Audit: BEHAVIOR string tables (2026-09-29)

`MapBehaviorStringTableCodec` reads old-format tables after the script directory
and enhanced `STRL` / `STRE` chunks, including `strbin` unescaping and the native
`STRE` xor decrypt. `AcsBehaviorBinder` copies the parsed table onto every
registered `AcsProgram`. Map-loader and playsim tests cover old lumps, padded
`STRL`, and `ThingCountName` fed from a bound behavior lump.

DECORATE replacements, library string ids, and enhanced-only modules without old
format remain partial. Gate 5 native trace is still missing.

## Audit: ACS ThingCountName subset (2026-09-29)

`ThingCountName` (288) and `ThingCountNameSector` (345) resolve ACS string-table
entries through `DoomActorCatalog.TryEditorNumberForClassName`, then reuse
`ThingCount` on editor numbers. Stack order matches native
`PCD_THINGCOUNTNAME` / `PCD_THINGCOUNTNAMESECTOR`. Programs may supply
`AcsProgram.StringTable` or the bound BEHAVIOR string table. DECORATE
replacements and inventory exclusion are absent.

`AcsThingCountNameTests` covers `DoomImp`, unknown classes, and a tagged sector.

Phase 1 stays open. Automatic string binding from map lumps and gate 5 native
trace are still missing.

## Audit: Scroll_Ceiling line activation (2026-09-29)

`Scroll_Ceiling` (224) now routes through `ExecuteFloorSpecial` like native
`LS_Scroll_Ceiling`. Tagged sectors must exist for activation to succeed.
Ceiling texture scroll and `DScroller` thinkers are still absent, so carry and
planes stay unchanged; the line can clear on one-shot maps.

`SectorScrollCarryTests.ScrollCeilingLineActivatesWithoutCarry` covers use
activation with no movement.

Phase 1 stays open. Real ceiling scrolling and gate 5 native trace are still
missing.

## Audit: ACS GameType and GameSkill (2026-09-29)

`GameType` pushes 0 for single-player, 1 for cooperative, and 2 for
deathmatch. `GameSkill` pushes the boot `SpawnOptions.Skill` clamped to 0–4.
`AcsPlayerQueryTests` covers both alongside the existing player-query opcodes.

Phase 1 stays open. MAPINFO custom skill ACS returns and gate 5 trace are still
missing.

## Audit: ACS ThingCountSector (2026-09-29)

`ThingCountSector` (344) pops tag, tid, then type (top = tag), matching
`PCD_THINGCOUNTSECTOR` in `p_acs.cpp`. The count reuses `ThingCount` with a
sector-tag filter when tag is not `-1`; map actors with health are included.
DECORATE class names, `ThingCountNameSector`, and inventory exclusion are
absent.

`Phase7AcsTests.ThingCountSector_RespectsSectorTag` covers a tagged sector.
Release playsim: **2,210** passed on 2026-09-29 after monster scroll carry test.

Phase 1 stays open. Name-based counts and gate 5 native trace are still missing.

## Audit: Hexen sector scroll player carry (2026-09-29)

Hexen sector specials **201–224** add player-only carry deltas (`* 0.5` table)
on top of line-driven scroll vectors in `ApplySectorScroll`. Monsters ignore
the Hexen table but still follow `SetSectorScroll` / `Scroll_Floor` carry.
Raven compat speeds, `DScroller`, texture/ceiling scroll, and `MF8_INSCROLLSEC`
are absent.

`SectorScrollCarryTests.HexenNorthSlowScrollMovesPlayersOnly` and
`GroundedMonsterMovesWithSectorScroll` cover the split.

Phase 1 stays open. `Scroll_Ceiling` (224) texture scroll and gate 5 trace are
still missing.

## Audit: sector scroll carry subset (2026-09-29)

`SetSectorScroll` stores per-sector XY deltas applied after `Thinkers.Run`.
`ActorPhysics.ApplySectorScroll` moves grounded, non-floating actors with
`TryMove`. Floating actors ignore scroll. `Scroll_Floor` (223) sets or clears
carry from line args with native `/32` scaling; only the carry path is wired
(texture scroll, `Scroll_Ceiling`, `DScroller` thinkers, and `MF8_INSCROLLSEC`
are absent; Hexen 201–224 player carry is documented separately above). Scroll arrays are checksummed and not in
the pose.

`SectorScrollCarryTests` covers direct scroll, line special 223, carry clear,
and a floating skip.

Phase 1 stays open. Polyobjects, ceiling/wall scroll, and gate 5 native trace
are still missing.

## Audit: ACS IsNetworkGame (2026-09-29)

`IsNetworkGame` pushes 0 because this authority sim is always local. Client
sessions are absent. `AcsPlayerQueryTests` covers the opcode.

## Audit: ACS SinglePlayer and PlayerInGame (2026-09-29)

`SinglePlayer` pushes 1 when `SpawnGameMode` is single-player. `PlayerInGame`
checks the internal player slot with `IsPlayerInGame`. `PlayerIsBot` always
pushes 0 because player-slot bot controllers are not implemented. Visible
client slot remapping is absent. `AcsPlayerQueryTests` covers all three
opcodes. `AcsFixedPointTests` no longer treats `SinglePlayer` as unsupported.

`dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release`
reported 2,207 passed on 2026-09-29.

Phase 1 stays open. Scroll sectors, TID iterators, and gate 5 native trace are
still missing.

## Audit: MF4_NOHATEPLAYERS (2026-09-29)

`OkayToSwitchTarget` returns false when the source is a player and the victim
has `NoHatePlayers`. Player damage still applies; wake-up does not chase the
player. Monster retaliation is unchanged. The flag is checksummed and not in the
pose.

Phase 1 stays open. Gate 5 native trace, scroll sectors, and the rest of the
actor roster are still missing.

## Audit: ACS stack divide-by-zero tests (2026-09-29)

`AcsExpressionTests` now expects stack `Divide` and `Modulus` by zero to push 0
and continue, matching `Phase7AcsTests` and native ACS. Script-var and array
divide-by-zero paths still stop the fiber.

`dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release`
reported 2,197 passed on 2026-09-29.

## Audit: Phase 7 ACS opcodes (2026-09-29)

Stack `Divide` and `Modulus` by zero push 0 instead of stopping the fiber.
`ThingCount`, `ThingCountDirect`, `PlayerCount`, and `Timer` match the Phase 7
regression scripts. `Timer` reports start-of-tic `levelTic`, matching `AcsTimerTests`. Native divide-by-zero abort and
name-based thing counts are absent. All six `Phase7AcsTests` pass.

Phase 1 stays open. Gate 5 native trace, the rest of the actor states and
roster, psprite sprites, and scroll sectors are still missing.

## Audit: sector floor mobj carry order (2026-09-29)

Rising sector floors already move grounded actors through post-mover
`FitToSector`. End-of-tic fitting now runs carriers before `OnMobj` riders so
support tops are current when riders snap. Scroll sectors are absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: pr_switcher hate stickiness (2026-09-29)

`OkayToSwitchTarget` uses a dedicated `pr_switcher` stream. When the actor
already chases a living target whose `ThingId` matches `TIDtoHate`, a roll
below 128 with line of sight blocks switching to a new source. `SwitchTargetRandomState`
is checksummed and not in the pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: barrels and HarmFriends (2026-09-29)

`CanAttackHurt` uses `MF3_ISMONSTER` on the victim. With infighting off,
monsters still cannot hurt each other unless hostility or `TIDtoHate` applies,
but they can hurt non-monsters such as barrels. `MF7_HARMFRIENDS` allows a
shooter to damage a friendly at standard infighting. Wake-up is unchanged.
`IsMonster` defaults on for `AddBot`. The flags are checksummed and not in the
pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: horizontal rider carry (2026-09-29)

A solid carrier's physics step moves actors standing on its top by the same
XY and Z delta. Blocked rider moves try axis splits. `MF2_ONMOBJ` tracks mobj
support and keeps dropoff from using only the sector floor while set. Scroll
sectors and polyobjects are absent. `OnMobj` is checksummed and not in the
pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: TIDtoHate (2026-09-29)

`TIDtoHate` and `MF3_NOTARGET` gate monster wake-up and monster-monster
damage. Teammates with the same hate id do not target or hurt each other. With infighting off, a shooter may still hurt an actor whose `ThingId` matches
`TIDtoHate`; wake-up still needs hostility unless standard infighting applies
the hated-tid species exception. At standard infighting, same-species damage still yields
when the hated tid matches. TID look iterators are absent. `pr_switcher` stickiness is documented in a
later audit. `TidToHate` is checksummed and not in the pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: rider stack crush (2026-09-29)

Mover crush and sector pinch crush also hit actors standing on the crushed
actor's top within both radii. Horizontal rider carry is documented separately.
Polyobjects are absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: infighting damage (2026-09-29)

`ActorDamage.Apply` uses the native `CanAttackHurt` subset for monster-monster
hits. Standard infighting blocks same `DoomEdNum` species unless
`MF6_DOHARMSPECIES` or hostility applies. Infighting off blocks unless the
victim is hostile to the shooter. Players are never gated. Non-monsters are not
gated when infighting is off. `TIDtoHate` and `MF7_HARMFRIENDS` are documented
in later audits.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: standard infighting species (2026-09-29)

At infighting `0`, wake-up blocks the same `DoomEdNum` species unless
`MF6_DOHARMSPECIES` is set or the source is hostile. Infighting `1` allows
same-species wake. `MF7_FORCEINFIGHTING` applies standard rules when the
level is set to none. Projectile groups and `TIDtoHate` are absent. The flags
are in the checksum and not in the pose.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: ice chunks (2026-09-29)

A shattering frozen corpse on a simulation spawns deterministic `IceChunk`
debris from the native count and offset formula. Combat RNG drives placement,
velocity, and lifetime. Chunks move with gravity and expire. A corpse with no
simulation still shatters without spawning debris. Chunk heads, terrain melt,
and sounds are absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: last enemy resume (2026-09-29)

After the player scan, an enabled brain tick resumes a living non-friend
`lastenemy` when nothing else is acquired, then clears the slot. A dead or
friendly last enemy only clears the id. Goals and look states are absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: infighting wake-up (2026-09-29)

`MF5_NOINFIGHTING` and the level `infighting` cvar gate wake-up against
non-players when infighting is off. `IsHostile` is a subset: two plain
monsters are not hostile, and opposing friendlies with different
`FriendPlayer` values are. At infighting `0`, same `DoomEdNum` species is
blocked unless `MF6_DOHARMSPECIES` or hostility applies.
`MF7_FORCEINFIGHTING` upgrades a none level to standard rules. Projectile
groups, `TIDtoHate`, and deathmatch teamplay are absent. `NoInfighting`
and `Infighting` are in the checksum and not in the pose. A same-actor
restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: just hit (2026-09-29)

`MF_JUSTHIT` is set when a pain flinch lands and the damage source is the
chase target or there is no chase target. A wound or electric fullbright
does not set it. A monster already chasing someone else stays clear when
a different source hurts it. The brain clears the flag on the next
enabled tick. `IsFriend` gates retargeting and just-hit when the chase
target is a friend. The flag is in the checksum and not in the pose. A
same-actor restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: ice corpse shatter (2026-09-29)

A shootable frozen corpse shatters on damage unless the hit is ice from
an inflictor that lacks `MF7_ICESHATTER`. Fire still shatters. Quiet
ice leaves the corpse alone. An ice-shatter inflictor still breaks it.
A warm corpse ignores the hit. Shatter zeroes velocity, sets
`MF6_SHATTERING`, and forces one state tic. Chunks spawn on a simulation.
The
flags are in the checksum and not in the pose. A same-actor restore
keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: player extremely dead (2026-09-29)

`CF_EXTREMELYDEAD` is set when an extreme death frame kills the player.
A gib-line kill sets it. A typed `Fire` death past the gib line does
not. An inflictor extreme-death flag on a sub-gib kill still sets it.
Raising health back to spawn clears it. The flag is in the checksum and
not in the pose. A same-actor restore keeps it.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: chase threshold (2026-09-29)

Wake-up reloads `Threshold` from `DefThreshold` when the source is already
the chase target or when `OkayToSwitchTarget` allows a switch. A hit from
a new source is ignored while threshold is positive. One hundred enabled
brain tics clear a default threshold of 100 and then allow the switch. A
`DefThreshold` of 0 switches immediately. `MF7_NEVERTARGET` leaves the
target empty. Friends and species checks are absent. `DefThreshold` and
`Threshold` are in the brain checksum. A same-actor restore keeps them.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Audit: last-enemy memory (2026-09-29)

Wake-up stores the prior chase target in `LastEnemyId` when
`OkayToSwitchTarget` allows a switch. The slot is filled when it was empty,
the stored actor is not a player, or the stored actor is dead. A living
player last enemy is kept across later monster switches. `TIDtoHate` and
sleep early-out are absent. `LastEnemyId` is in the brain checksum. A
same-actor restore keeps it.

Phase 1 stays open. The rest of the actor states and roster, psprite
sprites, and a native tick trace are still missing.

## Audit: friendly IsFriend (2026-09-29)

`MF_FRIENDLY` and `FriendPlayer` feed a subset of native `IsFriend`. Two
friendly actors with the same nonzero `FriendPlayer`, or either side at 0,
are friends. Wake-up does not acquire or switch to a friendly source.
`MF_JUSTHIT` is applied after wake-up, matching native `ReactToDamage`.
The gate is the source is already the chase target, there is no chase
target, or the chase target is not a friend.
Deathmatch teamplay and designated teams are absent. Both fields are in the
actor checksum. A same-actor restore keeps them.

Phase 1 stays open. The rest of the actor states and roster, psprite
sprites, and a native tick trace are still missing.

## Audit: sector pinch crush (2026-09-29)

After movers tick, `FitToSector` applies 10 crush damage every four tics
when sector headroom is below actor height. Only solid blockers are
crushed. Map load and per-step fits skip it. An active mover on the
sector or mover crush on the same tic suppresses the pinch so crushers do
not double-hit. Damage type is `Crush`. Carried riders, polyobjects, and
touchy detonation are absent.

Phase 1 stays open. The rest of the actor states and roster, psprite
sprites, and a native tick trace are still missing.

## Audit: target-switch flags (2026-09-29)

`MF4_NOTARGETSWITCH` on the victim blocks `OkayToSwitchTarget` while a
chase target is already set. A later hit from someone else leaves that
target in place. `MF4_QUICKTORETALIATE` lets a new source take over even
when threshold is still positive and reloads threshold from
`DefThreshold` on the switch. Both flags are in the actor checksum and
survive a same-actor restore. Friend, species, and TID hate checks remain
absent.

Phase 1 stays open. Crush, the rest of the actor states and roster,
psprite sprites, and a native tick trace are still missing.

## Implemented and audited in this pass

### Snapshot failure handling

Previously collecting a tail drained queued authority events and dead-spawn
indices, and advanced actor rolling hashes, before discovering that encoding
failed. A retry therefore represented different state from the original attempt.

The tail builder now peeks at pending records and commits them only after a
successful encoding preflight. Checksum-bearing builders reserve the checksum
block during preflight and compute/store hashes only after success. Collection
itself no longer mutates actor hashes. Direct co-op writers follow the same
commit-on-success rule. Tests compare retry bytes with an untouched fresh store
for co-op, checksum-bearing co-op and merged invasion tails.

World-delta player/sector counts and shipping-tail counts are rejected before
narrowing to a byte. Previously 256 sectors could wrap their count to zero.
Invalid invasion header versions are rejected by the writer. Canonical input
event payloads over 65,535 bytes are rejected before their length can wrap.

This is serialization failure safety, **not delivery acknowledgement**. Pending
records are still consumed when a packet is built; this does not recover UDP
loss. The single-tail count/packet limits and accumulating actor tombstones still
block large maps and sustained projectile sessions. LiveSession still throws
when its world cannot fit. Nothing is silently truncated to make that disappear.
The checksum-bearing path encodes twice; its performance has not been profiled.

### Player state and buffered weapon selection

Armor and signed aiming pitch now travel from the authoritative player through
the existing pose wire fields into the guest store. Both co-op and invasion
round trips are tested, including positive/negative pitch near the clamp limits.
Health and armor beyond the signed-short protocol range saturate rather than
becoming negative. Simulation values retain their original precision/range;
exact large mod health would require an extended negotiated wire format.

DEM_WEAPSELECT canonical events now enter the same FIFO as movement and attack.
Selection happens on the command's simulation tic, before firing. Default Doom
slots, repeated slot cycling, next/previous, ownership and primary-ammo checks
are implemented. Multiple selections in one tic retain their order. Queue
rejection makes no inventory change; retry admits selection once space opens.
Input bytes are copied into the queued selection list. Death/restore discard
these actions with the existing command queue. Switching cannot reset cooldown.

Malformed framing or selection payload lengths reject the entire command before
movement or attack is admitted. Empty direct-call blocks remain supported.
Other framed event types remain ignored by this sink. Custom slots, alternate
fire, no-ammo-check options, pending-weapon/psprite lowering and raising, and
other gameplay event commands are still unimplemented.

Native references: `src/d_net.cpp` SelectWeapon, `src/g_game.h` WST enum,
`wadsrc/static/zscript/actors/doom/doomplayer.zs`, and player.zs PickWeapon /
PickNextWeapon / PickPrevWeapon. The default slot selection order matches those
tables; the whole native weapon-switch lifecycle does not.

### Weapon noise and map sound flags

Successful weapon firing alerts eligible existing monsters through connected
flat sectors. Closed openings stop propagation using current moving floor and
ceiling heights. A path may cross one ML_SOUNDBLOCK line, but not two. A lower-cost
alternate path revisits a sector so the result is independent of discovery order.
Idle monsters retain the heard player's position for chase without seeing through
walls. Ambush monsters require line of sight to accept the alert; dead/disabled
monsters and dry firing do not create a new target.

Doom and Hexen binary ambush flags and UDMF `ambush` are retained at spawn.
Binary line sound flags and UDMF `blocksound` feed propagation. Ambush participates
in the simulation checksum. Regression coverage includes all three loading
formats, two sound barriers, cheaper alternate paths, opening/closing doors,
hidden monsters and ambush behavior.

Reference: `src/playsim/p_enemy.cpp` P_RecursiveSound / P_NoiseAlert and
`src/doomdata.h` ML_SOUNDBLOCK. This is an immediate flat-sector alert subset:
native persistent sector sound targets, later-spawned listeners, splash/no-target
options, portal/sloped sound traversal and full native A_Look behavior remain.
The adjacency graph is rebuilt per shot and has not been performance-profiled.

## Combined completion gates, including previous work

### Additional conversion: fractional planes, ceilings, crushers and stairs

Live sector planes now retain fractional heights. UDMF fractional flat-sector
heights are accepted after finite/range validation; collision, player floor
placement, traces, projectiles, sound openings and snapshot publishing consume
the live values. Door/floor/lift argument speeds use division by 8 without integer
truncation. Zero/negative speed requests are rejected rather than coerced to an
unrequested minimum speed. Plane checksums retain fractional bits.

Pose archive version 5 writes sector planes as 16.16 values; readers retain
versions 1–4. A hand-built v4 sector fixture and v5 fractional round trip verify
compatibility. Invalid plane restores are rejected before changing clock or actor
state. This extends plane storage, not the archive's incomplete mover/AI/inventory
coverage. Physics and saves quantize to the existing 16.16 representation.

Converted classic ceiling actions: lower-to-floor, fast/slow repeating crushers,
non-damaging lower-and-crush, stop/resume, and the corresponding supported
repeat/use/walk variants. Extended actions include lower/raise by value,
crush-and-raise, lower-and-crush, crush-stop/remove, crush-raise-and-stay,
distance crushers, lower-to-floor with gap, and raise-to-highest. Slow crushers
retain one-eighth-unit speed after obstruction; damage uses the shared four-tic
cadence. Return legs restore their configured speed. Paused movers retain
direction/speed state, and remove permits a new ceiling action.

Floor and ceiling thinkers can run independently in one sector; duplicate
thinkers on the same plane are rejected. Current opposite planes limit movement
to prevent inverted sectors. Native destination-clamped obstruction behavior is
covered: a ceiling can roll back its final step while completing that leg.
Mover checksums include pause/loop/direction, return speed and slowdown state.

Classic Doom stairs 7/8/100/127 and repeat variants 256–259 now build directed
same-texture chains with native slow/turbo speeds and 8/16-unit steps. Extended
Doom-style stairs 217/270/273 support zero delay/reset. Each chain remains locked
until all steps complete; cycle detection prevents repeated sectors. The first
turbo stair uses stopping crush behavior, while following speed-4 steps use the
native damaging-movement rule. Tests cover fractional progress, final heights,
texture/sidedness boundaries, cycles and whole-chain retrigger locking.

References: `wadsrc/static/xlat/base.txt`, `defines.i`, `src/playsim/p_lnspec.cpp`,
`mapthinkers/a_ceiling.cpp`, `a_floor.cpp` and `dsectoreffect.cpp`.
These cover the implemented flat-sector action variants. Linked sectors,
attached actors, corpse/gib handling, all compatibility flags, texture-changing
ceiling variants, movement audio, synchronized/delayed/reset stairs and remaining
specials are still work to convert. Unsupported ceiling texture-change and stair
delay/reset requests are left unactivated rather than silently dropping those
arguments. Game-specific Hexen defaults beyond the Doom actor/game model are
not certified by loading a Hexen-format map.

### Native Doom attack sequences and projectile review

The continuation adds timed attack profiles for 15 map actor types: zombie,
shotgun guy, chaingunner, Wolfenstein SS, demon/spectre, imp, baron/hell knight,
cacodemon, revenant, mancubus, arachnotron, cyberdemon and spider mastermind.
Profiles implement their attack-action delays, melee damage dice, bullet counts,
projectile classes, volleys and repeat intervals. Pain cancels a pending volley.
Previously several of these monsters had no brain and barons/cacodemons fired
imp balls. Demon melee cannot fall through to a ranged attack if its target flees.

Projectile definitions now carry native speed, radius, height and direct-damage
bases; impact damage rolls 1–8 times the base. Monster spawn heights and initial
normalized source-to-target direction follow P_SpawnMissileXYZ. Revenant tracers
retain their target and steer on four-tic boundaries with bounded yaw and vertical
adjustment. Cyberdemons and spider masterminds ignore radius damage while remaining
vulnerable to direct hits. New properties participate in the simulation checksum.
Managed projectile identities stay unique within the existing ushort range.

The review also fixes BFG spray yaw: it follows the projectile angle even if its
owner turns. Its forty-ray fan uses 90/40 spacing and fifteen 1–8 damage dice per
hit, replacing the uniform damage approximation. The original geometry regression
for plasma now checks native damage bounds instead of the previous fixed 20.

Thirty-one new regressions cover native attack delays, repeat/volley intervals,
mancubus spread, projectile defaults and identities, tracer timing/dead targets,
boss impact damage without splash, pain interruption, demon range escape, hitscan
pellet counts, elevated aim, deterministic replay and BFG owner-turn behavior.
Sources: Doom actor definitions under `wadsrc/static/zscript/actors/doom`,
`src/playsim/p_mobj.cpp` and `src/playsim/p_actionfunctions.cpp`.

This is not complete native AI/state execution. Acquisition/chase, attack-choice
probabilities, per-action RNG streams, refire friendly-fire/visibility rules,
floating movement, native animation states/pain durations and difficulty modifiers
remain open. The following continuations add lost-soul, pain-elemental and
arch-vile behavior. Tracer state-entry cadence, spawn collision/initial displacement,
projectile lifetime/species rules, BFG explosion delay and vertical autoaim still
require conversion. These limitations prevent claiming native behavioral parity.

### Lost souls, pain elementals and invasion descendants

Lost souls now start a charge after the native ten-tic attack windup, preserve
20-unit horizontal speed without ground friction, aim vertically, and inflict
3 times a 1–8 roll on impact. Movement checks in two-unit or smaller increments
prevent crossing actors/walls during a charge, including vertically overlapping
targets. Impact, pain and death end the charge. Source review corrected plane
contact: floor contact clears downward velocity while preserving the charge;
ceiling contact reverses vertical velocity. Charge state is checksummed.

Pain elementals spawn a charging lost soul after their fifteen-tic windup and
three souls at offsets 90/180/270 after 32 death tics. Spawn checks use the native
prestep distance and subdivided path, temporarily ignoring the parent's solidity
and restoring it in a finally block. Low ceilings and blocked paths reject the
child. IDs are monotonic; successful children enter the actor and thinker lists.
Death timer/target state is checksummed and the death burst fires once per death.

The managed invasion director registers descendants only for members of the
current wave. It waits for a pending pain-elemental death burst before ending an
empty wave, then counts successful child spawns. A completely blocked burst still
allows completion after the death action runs. This closes a new managed wave
transition race; it does not certify native invasion runtime parity. Default
wave generation still spawns bots, so the regression installs a pain-elemental
brain on a tracked wave actor to exercise this integration.

Fourteen added regressions cover charging, impact bounds/single hits, walls,
vertical hits, pain/death, floor/ceiling response, spawn delay and path rejection,
death burst timing/uniqueness, deterministic replay, child counts and blocked-burst
victory. The audit used `lostsoul.zs`, `painelemental.zs`, native `AActor::Slam`,
`P_XYMovement`/`P_ZMovement` and actor collision handling.

Remaining limitations include native float/chase/retarget logic, animation states,
compatibility soul limits, friendliness/species and damage-type rules, runtime
actor replacements/DEHACKED defaults for spawned souls, and advanced geometry.
Rejected soul spawns are omitted rather than producing the native killed spawn
actor/effects. The v5 pose archive still does not restore these AI timers or
dynamic actor membership. These are implementation gaps, not passed audit gates.

### Arch-vile attack and resurrection

The arch-vile now acquires a target through the managed brain, starts its fire
stage after ten attack tics and attacks at tic 66, recovering through tic 94.
The attack checks sight again, applies 20 direct damage and a 70-radius blast
behind the target, then sets vertical velocity to 1000 divided by target mass.
Native Doom mass defaults are initialized for map monsters and spawned souls.
Mass and resurrection metadata participate in the simulation checksum.

The resurrection scan checks nearby terminal corpses with native raise states,
clearance against floors/ceilings/walls and occupied actor space. It restores the
spawn health, clears prior retaliation/charge/death state, shares the arch-vile's
target, and holds the revived brain for the type's raise duration. The arch-vile
heals for 30 tics. Review corrected the zombie raise duration to 20 tics and made
pain interrupt healing/raising instead of waiting behind those timers.
Map actor resurrection health preserves the applied spawn health; managed wave
bots retain their own 30-health default. A resurrected wave member keeps its ID
and returns to the live enemy count without incrementing total spawns.

Eighteen regressions cover attack timing/damage/thrust, loss of sight, four raise
durations, four non-raiseable types, actor obstruction, ceiling clearance, healing
interruption, target mass, actual resurrection of a tracked wave bot, and replay.
References: `archvile.zs`, Doom actor Raise blocks, and
`P_CheckForResurrection` in `src/playsim/p_enemy.cpp`.

This completes attack dispatch coverage for the 18 cataloged Doom monster map
types, not full native monster/state parity. The fire stage is currently simulation
state, not a replicated visible fire actor. Native death/raise animation tables,
exact blockmap/chase resurrection selection, friendliness/no-target/damage flags,
modified mass/raise actions, corpse restoration after crushing, ghost compatibility,
blast/autoaim edge semantics and full save restoration remain open. The managed
terminal-corpse state is still reached on its generic death timeline. A source
review and synthetic regressions cannot certify the native gameplay experience.

### Floating movement and persistent hearing

Cacodemons, pain elementals and lost souls now have a separate floating flag,
initialized from the native actor defaults rather than inferred from no-gravity.
After ordinary movement, a live enabled floater adjusts its base height toward
the target's center when horizontal distance is strictly less than three times
the vertical difference. Default float speed is four units per tic. The adjustment
is a position change, preserves vertical velocity, respects current sector planes,
and is bypassed by the dedicated lost-soul charge path. Float state/speed are
included in the checksum. Eight regressions cover all three species, rising,
descending, strict range boundaries, ceiling clipping, no-gravity separation and
dead/disabled actors. Sources: `P_ZMovement` and Actor's `FloatSpeed 4` default.

Weapon noise now records a persistent `LastHeardTargetId` on reached actors,
including those unable to wake at that moment. An idle brain can later acquire a
living remembered source; ambush monsters still require sight. A newer sound is
remembered while the current live target is retained. Five regressions cover
disabled listeners, dead sources, ambush behavior, newly spawned monsters, and
switching to the latest heard source after the previous target dies.

Source review distinguished native per-actor LastHeard from the optional sector
SoundTarget compatibility mode (`NoiseMarkSector`/`A_Look` in `p_enemy.cpp`). The
implementation follows the per-actor default: newly spawned actors do not inherit
old sector noise. Last-heard identity is checksummed. The sector compatibility
mode, splash/range-limited alerts, friendliness/no-target rules, full native look
state cadence, obstacle-directed float adjustment, 3D geometry/actor stacking and
save persistence remain open. This supersedes the earlier immediate-alert-only
limitation without claiming those additional behaviors are converted.

### Player weapon damage, spread and range

Pistol/chaingun damage now rolls 5 times an integer from 1–3; fist/chainsaw damage
rolls 2 times an integer from 1–10. These replace the fixed 10-damage placeholders.
Damage is rolled before spread, following the native action order. Fist and saw
yaw spread use their distinct defaults. Gun/punch spread uses the native /256
scale, while the saw uses /255. The super shotgun now rolls independent vertical
spread for each of its twenty pellets using `7.097 / 256`, in addition to its
horizontal spread. Player hitscan range is now 8192 (PLAYERMISSILERANGE), correcting
the previous use of the 2048-unit monster range.

Eleven new regressions cover damage bounds/variation across seeds for four weapons,
vertical pellet hits, thin-target pellet misses, dry-fire RNG preservation,
deterministic replay and the player-range boundary. Geometry/melee tests that had
assumed fixed damage now check native damage bounds. Both local and UDP invasion
tests fire until the actual kill, bounded by six minimum-damage hits against the
30-health fixture, and still verify the exact wave transition, ammo use, counters
and replicated health. Five initial regression failures were fixed test assumptions
about fixed damage, not waived checks or changes to enemy health.

Sources reviewed: `weaponpistol.zs`, `weaponshotgun.zs`, `weaponssg.zs`,
`weaponfist.zs`, `weaponchainsaw.zs`, Actor's melee constants and
`constants.zs`/`p_local.h`. This does not replace the managed RNG with native
per-action random streams. Native psprite startup/refire timing and first-shot
accuracy, autoaim, berserk/saw-specific effects and optional vertical-spread cvars
remain unconverted. No claim of complete player-weapon parity is made.

### Immediate sector lighting actions

Runtime sector light levels are now separate from immutable map definitions,
included in simulation checksums and read by the snapshot publisher. Converted
extended actions are 110/111/112 (raise/lower/change) and 233/234 (minimum/maximum
neighbor). Doom translations cover 12/13/35/79/80/81/104/138/139/157/169/170/171/
173/192/194 with their walk/use and repeat rules. Tag zero uses untagged sectors;
an unmatched tag succeeds and consumes a one-shot line, as native immediate light
actions do. Minimum includes the current sector; maximum searches neighbors and
can lower brightness. An isolated maximum resolves to -1.

The source audit corrected an initial 0–255 clamp: this checkout's SetLightLevel
clamps to signed short limits. Negative and overbright levels therefore survive
the action and snapshot encoding. The snapshot tests explicitly enable sector
metadata, matching DedicatedServerHost's default; raw tail builders omit that
metadata unless requested. Twenty playsim cases and two snapshot cases cover
activation/repeat rules, tags, neighbor selection, signed bounds, checksums and
wire values. References: `xlat/base.txt`, `p_lnspec.cpp`, `a_lights.cpp` and
`sector_t::ClampLight` in `r_defs.h`.

Remaining sector lighting thinkers, exact native lighting RNG, automatic
map compatibility selection, remaining ACS dispatch, switch textures and
full save persistence remain open. In particular, v5 pose archives do not capture
dynamic light levels or lighting thinkers.

### Animated sector lighting actions

Extended map-line actions 113/114/116/117 now implement fade, glow, configurable
strobe and stop. Reviewed against DGlow2, DStrobe and their EV entry points in
`src/playsim/mapthinkers/a_lights.cpp`: the first fade tick writes its start value;
the endpoint is reached before the next tick writes it again and destroys the thinker. Glows sort
their bounds and reverse without an extra endpoint pause. Strobes inspect the
current light, start after one tick and retain separate bright/dark durations.
Effects can overlap in creation order; immediate fades still apply. Stop removes
only matching tagged effects and preserves their current brightness.

Eighteen new playsim cases exercise those timelines, descending integer rounding,
nonpositive durations, signed bounds, equal targets, missing tags, overlapping effects,
stop/restart and pending-effect checksums. One server test verifies successive
authoritative fade levels through the snapshot encoder/decoder. Wider arithmetic
prevents interpolation overflow for extreme extended arguments; native signed
integer overflow behavior is not emulated. Lighting advances before actor thinkers;
cross-thinker ordering still needs a native runtime comparison. These tests use
synthetic maps and source-derived expectations, not captured native execution.

### Doom-style triggered strobes

Extended action 232 and Doom translations 17/156/172/193 now derive the upper
level from the current sector and the lower level from its current neighbors.
When the minimum equals the upper level, the lower level becomes zero. The
native walk/use and one-shot/repeat mappings are retained. Startup delays are
1–8 ticks; extended arguments control bright/dark durations, while Doom
translations use 5/35. Unmatched tags neither create an effect nor
consume a random draw. Stop also removes these strobes.

Eleven regressions cover mappings, cadence, runtime neighbor values, signed
levels, equal/higher-neighbor fallback, missing tags, stop/restart,
deterministic replay and combat RNG isolation. Source references are DStrobe's
Doom constructor and EV_StartLightStrobing in `a_lights.cpp`, action 232 in
`p_lnspec.cpp`, and `wadsrc/static/xlat/base.txt`.

The dedicated managed strobe stream participates in the checksum and does not
consume combat RNG. Its LCG sequence is not native FCRandom/StrobeFlash sequence
parity. As with lighting thinkers and current light levels, this stream is not
stored in the v5 pose archive. Full lighting save/restore remains incomplete.

### Flicker and correction to overlapping-effect behavior

Extended action 115 now implements DFlicker's immediate upper-level assignment,
independent signed clamping and countdown behavior. Its initial countdown is 1
or 65; transition occurs one tick later. Subsequent dark and bright intervals
are 2–9 and 2–33 ticks respectively. Bounds are not sorted. Ten new regressions
cover these ranges, retriggers, overlapping glow, stop-all behavior, deterministic
replay and isolation from both combat and strobe RNG. Flicker has a dedicated
checksummed managed RNG stream; exact native FCRandom sequence parity and saving
that stream remain open.

The deeper source audit found an error in the preceding lighting passes: checking
`sector.lightingdata` was incorrectly interpreted as these thinkers acquiring a
lock. DLighting has no such constructor, DSectorEffect::Construct only assigns
its sector pointer, and CreateThinker only constructs/links the thinker. The
assignment is in FraggleScript's DLightLevel, not these map-line effects. Removed
the incorrect managed single-effect guard and corrected three earlier tests.
An immediate fade can be overwritten by an older fade's final write; overlapping
effects run in creation order; Stop removes every matching effect. Earlier audit
claims about busy-sector rejection and fade completion locks are superseded.
The separate FraggleScript lighting lock is still unimplemented.

### Automatic sector glow and strobes

Map startup now creates glow/strobe thinkers for Doom sector specials 2/3/4/8/
12/13 and extended specials 66/67/68/72/76/77. Doom's low five special bits select
the effect without confusing higher generalized sector flags with its number.
Extended maps use extended numbers. Neighbor minima are captured on creation.
Ordinary strobes start after 1–8 ticks; synchronized strobes start on tick one.
Bright intervals are five ticks, with fast/slow dark intervals of 15/35 ticks.
DGlow moves eight light units per tick, undoing the step and reversing direction
at either bound, so endpoints are not reached in the usual non-degenerate case.

Eighteen regressions cover automatic startup, format mappings, generalized Doom
bits, fast/slow and synchronized timing, glow reversal, narrow light ranges and
stopping initialized effects. Source review used `src/maploader/specials.cpp`,
`src/playsim/mapthinkers/a_lights.cpp`, `p_lnspec.h` and
`wadsrc/static/xlat/doom.txt`. These are synthetic source-derived tests.
Only the lighting component of strobe/hurt special 4/68 is implemented here;
this pass does not implement its sector damage. Additional game-specific lighting variants,
other game-specific translations and
namespace-specific UDMF handling remain open, along with RNG/save limitations
already listed. Immutable map special values are retained.

### Automatic random flashes and fire flicker

Doom specials 1/17 and extended specials 65/81 now create DLightFlash-style and
DFireFlicker-style effects at startup. Light flashes capture current and minimum
neighbor brightness, with bright countdowns of 1 or 65 ticks and dark countdowns
of 1–8 ticks. Unlike action 115's DFlicker, they transition on the decrement that
reaches zero. Fire flicker updates every fourth tick, chooses a reduction of
0/16/32/48 and compares that reduction against the current brightness before
choosing the captured maximum minus reduction or minimum-neighbor-plus-16.
The latter minimum is signed-short-clamped and may exceed the initial maximum;
the native behavior is preserved rather than normalizing those bounds.

Thirteen regression cases cover both format mappings and generalized Doom bits,
countdown ranges, four-tick cadence, external brightness changes, high minimum
clamping, stop behavior, combat RNG isolation and deterministic replay. Reviewed
against DLightFlash/DFireFlicker in `a_lights.cpp`, startup dispatch in
`src/maploader/specials.cpp` and Doom sector translations. Both effects have
separate checksummed managed RNG streams; they still do not reproduce native
FCRandom sequences or persist through v5 pose saves. The full managed suite,
including existing invasion tests, passes; this is not native runtime validation.

### Standalone phased sector lighting

Doom special 21 and extended special 1 now initialize DPhased-style lighting.
The initial light's low six bits choose the starting phase; the runtime cycle
uses a fixed base brightness of 48, twelve steps per ramp and a 64-tick period.
Integer interpolation produces a peak of 237 for two ticks, not 255. Startup
does not immediately overwrite the map brightness; the first simulation tick
does. Immediate light changes do not reset the phase, and tagged Stop removes
the effect while preserving its current brightness.

Twelve tests cover three full cycles, Doom generalized bits, extended mapping,
signed/overbright initial values, stop/override behavior and checksums for phases
with equal current brightness. Expected cycles were derived from DPhased::Tick
and MapLoader::SpawnLights in native source. Connected LightSequenceStart and
alternating sequence sectors are covered by the subsequent pass below; this section covers standalone
phased sectors. Native runtime comparison, UDMF namespace handling and complete
lighting saves remain open.

### Connected phased light sequences

Doom sector 22 and extended sector 2 now start chains through alternating
23/24 or 3/4 neighbors. Traversal follows the first matching linedef neighbor,
excludes the previous sector and terminates on revisiting a sector. Iterative
traversal avoids native recursion depth limits. Zero brightness inherits the
preceding base; nonzero brightness replaces it. Initial phases distribute the
64-tick cycle across the chain. A local initialization-special array consumes
members so later starts cannot reuse them; immutable map definitions are retained.

Nine tests cover both map formats, inherited brightness, phase spacing, branch
order, cycles, competing starts, isolated zero bases, stop-all behavior and
extended flags. Native InitSectorSpecial strips flags in sector order; the
implementation preserves that timing, so an uninitialized flagged neighbor does
not match P_NextSpecialSector's exact special comparison. Source references are
DPhased::PhaseHelper, P_NextSpecialSector and MapLoader::InitSectorSpecial.
Two initial test expectations used the wrong peak tick and were corrected after
checking the native cycle. No tests were disabled. Runtime special consumption
outside lighting initialization is not yet modeled, and full save persistence,
native runtime comparison and game/UDMF namespace variants remain open.

### Light maximum compatibility and negative search values

The EV_LightTurnOn source audit found that negative ChangeToValue arguments must
remain the initial neighbor-search brightness. The earlier implementation
incorrectly replaced every negative value with -1. Corrected this while retaining
-1 for MaxNeighbor. Added the opt-in CompatSurface.SharedLightMaximum flag,
matching COMPATF_LIGHT's carry-forward behavior across tagged sectors: each
resolved value becomes the next sector's search baseline, and once nonnegative
it is applied directly to subsequent sectors. This is not a global maximum scan.
Raise, lower and minimum operations are unaffected.

Eleven new regressions cover default versus compatibility behavior, negative
baselines, negative carried values and unrelated operations. The source reference
is EV_LightTurnOn in `src/playsim/mapthinkers/a_lights.cpp`. The flag is available
through AuthoritySimulation.Start's existing compatibility argument, defaults off,
and is not automatically selected from native MAPINFO/CVars. Full compatibility
configuration loading remains open. Earlier general lighting test success did
not cover these signed-search cases; this audit fixes that coverage gap.

### Direct ACS lighting calls

The word-format ACS interpreter now reads LSPEC1DIRECT through LSPEC5DIRECT for
converted lighting specials 110–117 and 232–234. Map lines and ACS use a shared
lighting dispatcher. Arguments retain their signed values, omitted arguments are
zero, and all required operands must be present before an action can execute.
No player activator is needed for these tag-based actions. Existing one-argument
internal-action fallback remains unchanged; unsupported multi-argument actions
stop the fiber rather than silently continuing into later mutations.

Thirteen tests cover immediate changes, fade/stop, glow, five-argument strobes,
all five truncated operand counts, zero defaults, instruction alignment and
unsupported-action termination. Native reference: LSPEC1DIRECT–LSPEC5DIRECT in
`src/playsim/p_acs.cpp`. Compact/enhanced ACS, remaining stack-based special calls, native
special-argument compatibility masks, complete actor/line activation context
and non-lighting multi-argument dispatch remain unconverted. This does not make
the existing ACS subset a complete native interpreter.

### Stack-based ACS lighting calls

Word-format LSPEC1–LSPEC5 now pass one to five stack arguments to the same
lighting dispatcher as direct calls and map lines. Argument order follows native
STACK(n) through STACK(1); only the required top operands are consumed, leaving
earlier stack values intact. Missing instruction operands or insufficient stack
values stop the fiber before any action is applied. Unsupported stack specials
also stop rather than being misinterpreted as the legacy internal action API.

Twenty regressions compare all eleven converted lighting specials against direct
calls over 100 future ticks and test every underflow count, argument consumption,
arithmetic-produced values, truncated special operands and unsupported actions.
Reviewed against PCD_LSPEC1–PCD_LSPEC5 in `src/playsim/p_acs.cpp`. Native malformed
stack handling logs and attempts recovery in some cases; the managed subset
deliberately terminates instead. Native compatibility argument masks, compact
bytecode, result-returning/extended special opcodes, activation context and
non-lighting stack special dispatch remain open. Existing invasion regressions
pass, but this does not certify ACS-driven invasion maps against native runtime.

### Legacy invasion ACS queries and opcode correction

Corrected the managed Lspec6/Lspec6Direct enum values from 128/129 to native
129/130. These legacy opcodes are zero-operand invasion queries, not six-argument
special calls. The VM now pushes the managed director's wave for 129 and its
classic state for 130: waiting/disabled 0, countdown 5, wave 6, intermission 7,
victory 8. Disabling invasion does not erase the retained wave number. Existing
bytecode walkers use the corrected enum IDs as well. Opcode 128 is no longer
misidentified as the wave query; its actual non-invasion behavior is unimplemented.

Six new regressions use literal native opcode numbers and cover countdown,
wave completion, intermission, next wave, victory, disabling, zero-operand stack
alignment and rejection of the prior wrong wave ID. References: PCD_LSPEC6 and
PCD_LSPEC6DIRECT in `p_acs.cpp`, Net_GetInvasionWave and
Net_GetClassicInvasionState in `d_net.cpp`. Queries do not advance timers or
recount enemies. ACS still runs before the director in a simulation tick and
therefore reads the state committed by the previous director update.
Remaining native invasion metadata/control functions,
failure-state support and native runtime/map comparison remain open. Passing
these tests does not establish that the original invasion symptom is fully fixed.

### Core invasion function queries

Native word-format CALLFUNC opcode 351 now supports zero-argument functions
19700–19707: native state, state tics, wave, maximum waves, wave budget, spawned,
cleared and active monsters. Queries read committed director fields without
advancing its timer or recounting actors. The snapshot publisher and ACS now use
the same NativeState property; the managed atomic-spawn wave maps to native
cleanup (4), whereas the separate classic query still returns active state 6.
Budget remains Spawned, matching the existing managed snapshot approximation.

Seven tests cover all eight values across countdown, kills, intermission, next
wave and victory, native versus classic IDs, disabled state, invalid argument
counts, unknown functions and truncated instructions. Unsupported CALLFUNC
variants terminate the fiber. An initial byte conversion compile error was fixed;
the subsequent full suite passes. Native references: PCD_CALLFUNC and ACSF
19700–19707 in `p_acs.cpp`, and EInvasionState in `d_net.h`.

The subsequent opcode audit below corrects the older CallFunc alias; the VM
recognizes native 351, now shared with the enum and bytecode walker.
Compact ACS and nonzero-argument function calls remain unsupported. Boss waves,
spawn-plan metadata/control functions, staged native spawning, failure behavior,
native timer authority/cutscene policy and native-map runtime comparisons remain
open. Tests establish consistency with current managed state, not full native
invasion parity or resolution of every reported synchronization symptom.

### Native CALLFUNC and adjacent opcode decoding

Corrected enum IDs for CALLFUNC, SAVESTRING and the six map/world/global string
range operations to native 351–358. Removed redundant walker aliases and changed
the VM to use the corrected CallFunc enum. This prevents native opcode 353
(PRINTMAPCHRANGE) from being decoded as a two-operand function call. Word-format
CALLFUNC has two word operands; compact CALLFUNC has three operand bytes: one
argument-count byte and a two-byte function index. The compact walker previously
skipped only two, losing alignment. A trailing extended-opcode prefix now returns
a truncation rejection instead of reading past the buffer.

Fifteen literal-wire tests cover the eight enum values, adjacent word/compact
instructions, all incomplete compact CALLFUNC operands and truncated extended
opcode prefixes. Native references are the PCD enum, NEXTSHORT and PCD_CALLFUNC
in `src/playsim/p_acs.cpp`. Existing tests and the full solution pass. This audits
only this opcode block; other legacy aliases remain inconsistent, and recognizing
compact bytecode in the loader does not implement compact execution in AcsVm.

### Function-pointer, script-array and translation opcode IDs

Corrected enum IDs for PUSHFUNCTION through ORSCRIPTARRAY to native 359–375,
added named script-array shift opcodes 376/377, and corrected TRANSLATIONRANGE4/5
to 383/384. The walker now accepts the latter native IDs in both word and compact
formats. Twenty-one literal-ID cases and two complete block-alignment tests
cover the corrected values and immediate operand widths. The native reference
is the PCD enum and NEXTBYTE/NEXTWORD instruction handlers in `p_acs.cpp`.

An existing compact translation fixture encoded 364 (ASSIGNSCRIPTARRAY) instead
of intended 362 (TRANSLATIONRANGE3). Corrected the fixture to native 362 after it
failed with the enum correction; its expected instruction count remains unchanged.
All tests pass. These changes correct decoding/naming, not execution of function
pointers, script arrays or translations. Other historical aliases and the
remaining special-call handling still require audit; legacy non-native walker
aliases have not all been removed.

### Extended special-call decoding and lighting execution

Fixed LSPEC5EX/LSPEC5EXRESULT (381/382) decoding: both read a four-byte special
number in word and compact formats. Previously the walker treated them as having
no operands. Two old fixtures were corrected to include the required operand and
the expected operand count was updated. Four new decoding cases check alignment
using a multi-byte special number and reject every truncated operand length.

The word-format VM now executes these two opcodes for converted lighting actions.
Both consume five stack arguments; the result form pushes the action's boolean
result while retaining any earlier stack values. Four runtime regressions check
void/result stack behavior and underflow rejection. Native reference:
PCD_LSPEC5EX/PCD_LSPEC5EXRESULT and NEXTWORD in `p_acs.cpp`. Unsupported actions
still terminate the fiber. Compact execution, native compatibility masks and
other result-returning special opcodes remain unconverted. Full solution tests
and warning-as-error build pass; native runtime parity is not certified.

### Standard result-returning special call

Corrected LSPEC5RESULT from the erroneous managed ID 343 to native 263. Its
special number is a word operand in word-format ACS and a single byte in compact
ACS, unlike LSPEC5EXRESULT's four bytes in both formats. Corrected the walker,
removed a now-unreachable legacy word-walker alias arm, and updated the existing
fixture to supply the required operand. Native 343 remains decoded separately
as SETMUGSHOTSTATE; it is not executed as a lighting call.

The word-format VM now executes 263 for converted lighting actions, consuming
five arguments and pushing success while preserving the lower stack. Six added
cases cover the literal opcode ID, both operand widths, truncated decoding,
result-stack behavior, underflow and missing special operands. Source reference:
PCD_LSPEC5RESULT and NEXTBYTE in `p_acs.cpp`. Other legacy aliases, including
AddGlobalVar's former collision with 263 (corrected below), require care; callers must not
interpret that alias as implemented global-variable execution. Compact execution
and non-lighting result calls remain unconverted. The full managed suite passes.

### Global-variable opcode collisions and compact index widths

Corrected Add/Sub/Mul/Div/Mod/Inc/DecGlobalVar from erroneous 263–269 to native
183–189, removing the collision with LSPEC5RESULT and unrelated sector/player
queries. Corrected SetLineMonsterBlocking from 184 to native 104 so subtraction
is no longer mistaken for an operand-free line operation. Word decoding now
consumes each global-variable index; compact AssignGlobalVar/PushGlobalVar now
consume one byte instead of four, consistent with native NEXTBYTE.

Twenty-one new cases verify ten native enum values, word/compact block alignment
and all nine missing compact indices. Source references are the native PCD enum
and global-variable handlers in `p_acs.cpp`. The full suite and warning-as-error
build pass. These were decoding fixes; the subsequent pass adds scalar execution, while
cross-map lifetime remains unimplemented. Other opcode-table collisions, including
the old player-key aliases around 104, still require audit; this does not certify
the entire ACS decoder or VM.

### Scalar ACS global-variable execution

The word-format VM now implements native opcodes 181–189: assign, push, add,
subtract, multiply, divide, remainder, increment and decrement for 64 signed
scalar globals. Values are shared across fibers in the same VM and included in
the simulation checksum. Invalid indices, missing operands/stack values and zero
divisors terminate the fiber before altering a global. Increment/decrement take
no stack operand. Arithmetic wraps to 32 bits; signed division truncates toward
zero. Widening division/remainder avoids CLR exceptions for INT_MIN/-1, defining
that overflow result as wrapped INT_MIN and zero remainder respectively.

Seventeen new cases exercise shared values, signed arithmetic, overflow, the
highest valid index, invalid indices, divide/modulo by zero, underflow, isolation
of new simulations and checksums. Sources: NUM_GLOBALVARS and global-variable
handlers in `p_acs.h`/`p_acs.cpp`. Native undefined signed-overflow behavior is not
claimed as a portable parity guarantee. Values currently live within one AcsVm;
cross-map game-session lifetime and save persistence remain unimplemented, as do
global arrays, bitwise operations and compact execution. The checksum addition
does not imply complete ACS fiber/stack/program-state hashing or save support.

### ACS expressions and zero-condition branches

Added multiply/divide/remainder, all six signed comparisons, logical AND/OR/NOT,
bitwise AND/OR/XOR, shifts, unary minus and IFNOTGOTO to the word-format VM.
Boolean operations return 0/1. Binary operations now require both stack operands
before consuming them; zero divisors stop the fiber without executing later
world mutations. Unary and conditional branches likewise stop on underflow.
IFNOTGOTO consumes its condition and branches only for zero.

Thirty-nine new cases cover operand order, negative arithmetic, true/false
comparisons, nonzero logical values, shifts, unary operations, branching and
malformed expressions. Reviewed against the expression handlers in `p_acs.cpp`.
Arithmetic wrapping and INT_MIN/-1 use the managed policy described above;
out-of-range shift counts follow C#'s low-five-bit masking, not a claim about
undefined native C++ shifts. Jump offsets still address the supplied AcsProgram
code buffer; native BEHAVIOR/module-relative address integration is incomplete.
These tests use explicit word programs, not compiled native map fixtures.

### ACS delay timing correction

The scheduler now resumes a delayed script on the tick its counter reaches zero;
previously it waited one additional tick. DELAYDIRECT with nonpositive duration
now continues within the current instruction budget instead of yielding. Added
stack-based DELAY with the same timing and stack-underflow handling. Programs
may explicitly enable LegacyHexenDelay to add the native old-Hexen extra tick;
the default remains normal timing. This option requires caller selection because
native game/ACS-format metadata integration remains incomplete.

Fifteen new cases cover both delay forms, positive/zero/negative durations,
legacy timing, retained stack values, underflow and instruction-budget bounds
for zero-delay loops. Corrected the older test that expected Delay(1) to resume
on tick three rather than tick two. References: SCRIPT_Delayed, PCD_DELAY and
PCD_DELAYDIRECT in `p_acs.cpp`. Extreme legacy durations saturate at INT_MAX to
avoid overflow; undefined native signed overflow is not emulated. Full managed
tests, including invasion, pass. This removes a concrete script timing mismatch
but does not certify native invasion map behavior or complete ACS scheduling.

### Script-local scalar variables

Word-format scripts now execute native local assign/push/add/subtract/multiply/
divide/remainder/increment/decrement opcodes. Each fiber owns 20 zero-initialized
locals (native default LOCAL_SIZE), preserved through delays and discarded when
that invocation ends. Scalar arithmetic and validation reuse the global-variable
path; locals never alias global storage. Added names for the previously unnamed
native local arithmetic opcodes.

Twelve tests cover all arithmetic operations, slot 19, concurrent delayed fibers,
fresh invocation state, stack-free increment/decrement, invalid indices and zero
divisors. Reviewed against LOCAL_SIZE in `p_acs.h` and script-variable handlers
in `p_acs.cpp`; existing global and invasion regressions remain passing. Enhanced
script VarCount metadata, script arguments, local arrays, full suspension/resume,
save persistence remains open. At that point the
simulation checksum included globals but not locals; the pass below closes that gap.

### ACS execution-state checksums and owned bytecode

The simulation now incorporates AcsVm.Checksum, covering scalar globals,
registered program IDs/code/options, and ordered fibers with their program/code
identity, program counter, wait, stack and local values. Registration order is
normalized by program ID; execution order is retained because it affects shared
state. Registered bytecode is cloned and hashed once, preventing caller mutations
from changing active code or invalidating cached code hashes. Replacing a program
does not replace the bytecode already held by an active fiber.

Seven tests verify deterministic replay, registration versus execution order,
different stack/local values with otherwise equal programs/globals, wait-counter
changes and input-buffer ownership. Direct-versus-stack lighting equivalence
tests now compare observable lighting and running status, since different program
bytes intentionally produce different full simulation checksums. All tests pass.
This is a managed divergence diagnostic, not a native checksum format or a
collision-free proof. Checksums do not persist or replicate the script state;
save/restore, session lifetime and native ACS parity remain incomplete.

### Map-scoped ACS scalar variables

Added assign/read and arithmetic for the native map-variable opcodes, backed by
128 signed slots shared by scripts in the current VM. Map slots remain separate
from 20 per-fiber locals and 64 scalar globals, and participate in ACS/simulation
checksums. Bounds and arithmetic failure handling reuse the scalar implementation;
new simulations initialize map values to zero.

Thirteen new cases cover the final valid slot, arithmetic, sharing between
scripts, scope separation, increment/decrement, new-map isolation, invalid
indices, zero divisors and checksum differences with identical program registries.
Native references: NUM_MAPVARS in `p_acs.h` and map-variable handlers in
`p_acs.cpp`. This models one behavior module's scalar storage, not native module
imports/variable aliases or map-variable initializer chunks. Map arrays, hub
restoration, save persistence and compact execution remain unconverted. All
managed tests, including invasion regressions, and the warning-as-error build pass.

### ACS case branches

Converted native CASEGOTO (84) and CASEGOTOSORTED (256) for the managed
word-code executor. Matching branches consume the selector; unmatched cases
retain it for later cases or the default path. Sorted tables use signed comparisons
and binary search, matching the handlers in `src/playsim/p_acs.cpp`. Empty tables
continue with the selector intact. Table bounds are checked before searching.

Taken branches now share destination validation with GOTO, IFGOTO and IFNOTGOTO:
negative, unaligned and out-of-buffer destinations terminate the script. Untaken
branches do not validate unused destinations. Twenty-five regression cases cover
case chains, sorted hits/misses, selector consumption, missing selectors, malformed
table lengths and invalid ordinary/conditional branch destinations. Full managed
tests, including invasion regressions, pass.

This is a source comparison and regression audit of these handlers. Destinations
remain offsets within the supplied AcsProgram.Code buffer; native behavior-module
offset relocation and compact bytecode execution are still missing. Alignment and
bounds checks do not prove that a target is an instruction boundary. Sorted-table
ordering is assumed, as in the native handler. These changes do not complete ACS
or establish native runtime parity.

### ACS stack operations and restart

Converted DUP, SWAP, NEGATEBINARY and RESTART in the word-code executor, using
the native handlers in `src/playsim/p_acs.cpp` as the source reference. The audit
found incorrect enum values: DUP is 216 (previously 196), SWAP is 217 (previously
197), and NEGATEBINARY is 330 (previously 332). Corrected these and an existing
decoder fixture that incorrectly identified 332/333/334 as negate/get/set pitch;
the actual sequence is 330/331/332. Other legacy enum aliases remain unaudited.

DUP preserves the original top value, SWAP exchanges only the top two values,
and bitwise negation complements all 32 bits. Insufficient operands terminate
the script before following mutations. RESTART returns to offset zero in the
current script buffer while retaining locals and the evaluation stack. This
does not enqueue a replacement fiber or reset shared state.

Seventeen execution cases cover stack ordering, bit patterns, underflow, rejection
of the former enum numbers, finite restarts retaining locals/stack, delayed
restarts and scheduler fairness for an endless restart. Six new decoder/enum
cases use native wire numbers and verify zero operand widths in both bytecode
formats. All 1,471 managed tests pass, including invasion regressions.

The executor still assumes a single script beginning at offset zero, rather than
native behavior-module entry-point lookup. Endless scripts yield at the existing
64-instruction budget; native runaway-script termination and stack-size limits
are not implemented. Compact bytecode is decoded but not executed by this VM.
This pass does not establish native scheduler or real-map runtime parity.

### ACS fixed-point arithmetic

Converted FIXEDMUL (136) and FIXEDDIV (137) in the word-code executor after
reviewing the handlers in `src/playsim/p_acs.cpp` and MulScale/DivScale in
`src/common/utility/m_fixed.h`. Multiplication uses a signed 64-bit product,
arithmetic right shift by 16, and a wrapping 32-bit result. Division uses the
native magnitude guard before scaled integer division, saturating to signed
minimum/maximum on overflow or zero divisors. Ordinary integer DIVIDE retains
its existing zero-divisor termination behavior. Magnitudes are calculated after
widening to avoid a CLR exception on INT_MIN; no native compiler differential
test for that edge case was run.

The enum audit found an omitted SINGLEPLAYER entry at 135, leaving fixed-point
and adjacent gravity/air-control opcode names shifted by one. Added the entry
and corrected 136 through 141. Decoder widths now follow those native numbers:
the direct gravity/air-control variants consume one four-byte argument in both
formats. This corrects decoding only; SINGLEPLAYER and gravity/air-control
execution are still unsupported by this VM. Existing enum-based decoder fixtures
now emit the corrected numbers.

Thirty-one execution cases verify full 32-bit results, fractional/sign behavior,
rounding, wrapping multiplication, saturating division boundaries, zero divisors,
stack underflow and adjacent unsupported opcodes. Nine enum/decoder cases check
literal native numbers and mixed instruction widths. All 1,511 managed tests,
including invasion regressions, pass. This source/regression audit does not
complete ACS, native behavior-module integration, or real-map validation.

### ACS scalar bitwise updates

Converted AND, exclusive-OR and OR assignment for script-local, map and global
scalar variables, using the corresponding handlers in `src/playsim/p_acs.cpp`.
Each operation consumes one stack operand and an immediate variable index,
updates only the selected slot, and preserves the lower stack. Added the missing
ORGLOBALVAR enum name at native opcode 308. Invalid indices, truncated index
operands and stack underflow terminate the script before an update.

Thirty-four execution cases check all nine operations, full signed bit patterns,
last valid slots, out-of-range/negative indices, underflow, truncated instructions,
scope isolation and persistence of map/global updates between scripts in one VM.
Eleven enum/decoder cases check literal native numbers, compact versus word
index widths, and truncated indices. All 1,556 managed tests pass, including
invasion regressions. Existing ACS checksums already include these scalar slots.

This source audit also identified an enum defect (corrected in the following pass): the legacy world-array
names at 306 through 314 overlapped native scalar/array bitwise opcodes; native
world-array operations actually occupy 226 through 234. Runtime dispatch in this pass follows
native scalar wire numbers, not those array aliases. World-variable and array
execution, scalar shifts, cross-map/hub lifetime, compact execution and full
behavior-module integration remain incomplete. No native runtime parity claim
is made for this pass.

### ACS array opcode and decoder conflict audit

Corrected all nine world-array opcode names to native 226–234 and all nine
global-array names to 235–243. Resolving those collisions also required correcting
direct inventory, music and print names to their native 144/146/148 and 153–158
values, actor property operations to 245/246, ACTIVATORTID to 248, and PUSHBYTE
to 167. The reference is the opcode enumeration and handlers in
`src/playsim/p_acs.cpp`. These changes close the specific world-array enum defect
reported above; unrelated legacy opcode aliases remain outside this pass.

The decoder now consumes exactly one variable index for every world/global array
operation and retains index widths for the former 306–314 aliases, which are
native bitwise/shift instructions. Restored the previously omitted word-format
DIVGLOBALARRAY case. CHECKINVENTORYDIRECT consumes one four-byte string argument,
not two. Compact PUSHBYTE consumes one byte while PLAYERNUMBER consumes none.
Corrected two older compact fixtures that encoded PUSHBYTE and global arrays
using the erroneous numbers; enum-based fixtures now emit the corrected IDs.

Thirty-six new cases verify 31 enum values, all 18 array operand widths and
truncations in both formats, adjacent instructions and former-alias widths, and
compact PUSHBYTE/PLAYERNUMBER boundaries. The full 1,592-test managed suite,
including invasion regressions, passes. The warning-as-error build passes.
This is decoding and source-audit work, not array runtime conversion: the managed
VM still lacks array execution/storage, complete native behavior-module binding,
compact execution and real-map parity validation.

### ACS scalar shift assignments

Converted left/right shift assignment for script-local, map and global scalar
slots. Each handler consumes one stack operand and an immediate index, preserves
the lower stack and updates only its selected scope. The native reference is
PCD_LSSCRIPTVAR through PCD_RSGLOBALVAR in `src/playsim/p_acs.cpp`.

Corrected all 14 variable/array shift enum names from erroneous 393–406 to native
312–325. Added decoding for native 325, which had lost its one-index width when
the conflicting actor-property enum was corrected. Legacy decoder aliases at
393–406 remain accepted for now, with comments corrected to identify them as
legacy rather than native wire numbers. Older tests for those aliases remain;
the new tests independently check the actual native range.

Thirty-four execution cases cover signed right shifts, left-shift bit patterns,
last valid slots, invalid/truncated indices, underflow, scope isolation and shared
values surviving script termination. Sixteen enum/decoder cases verify all 14
native IDs and index/truncation widths in both formats. All 1,642 managed tests,
including invasion regressions, pass. Scalar values already participate in ACS
checksums; no new persistence or replication format was introduced.

The managed implementation uses C# shift-count masking to five bits, including
negative counts and counts at least 32. Those boundary tests establish the managed
contract, not portable native C++ semantics for undefined shifts or overflowing
signed left shifts. World-variable/array execution, compact execution, complete
native module integration and real-map parity remain unfinished. This pass
supersedes the earlier statement that all scalar shifts were unconverted.

### ACS world scalar variables

Converted world-variable assign/read, add/subtract/multiply/divide/remainder,
increment/decrement, bitwise AND/XOR/OR and left/right shift operations. Added
256 signed slots, matching NUM_WORLDVARS in `src/playsim/p_acs.h`, shared among
scripts in the current VM and separate from local, map and global storage.
World values now participate in ACS checksums. Added missing arithmetic enum
names at native 33/36/39/42/45/48; existing decoder ranges already accept their
immediate indices. Native handlers and P_ClearACSVars were reviewed in p_acs.cpp.

Twenty-nine execution cases use literal native opcodes to check arithmetic and
bitwise/shift results, slot 255, signed/wrapping boundaries, stack preservation,
invalid/truncated indices, underflow, zero divisors, four-scope isolation,
inter-script sharing and checksum differences with identical program registries.
All 1,671 managed tests, including invasion regressions, pass. Arithmetic edge
behavior follows the existing managed scalar contract: wrapping integer results,
widened division/remainder intermediates and masked shift counts. This does not
certify portable native C++ behavior for undefined arithmetic cases.

This closes world scalar instruction execution within one VM, superseding earlier
statements that those instructions were unsupported. It does not complete world
scope lifetime: separate simulations initialize separate zeroed storage, with no
hub transition ownership, cross-map handoff, save/restore or native string-root
integration. World/global arrays, compact execution and real-map parity remain
unfinished. Full phases 1–3 completion is still not achieved.

### ACS world/global sparse array execution

Converted the 18 basic world/global array instructions at native 226–243:
read, assign, add, subtract, multiply, divide, remainder, increment and decrement
for both scopes. Storage follows FWorldGlobalArray in `src/playsim/p_acs.h`:
signed 32-bit element keys with missing values reading as zero. World array IDs
are bounded to 256 and global IDs to 64. Read replaces the element index on the
stack; arithmetic/assignment consumes index and value; increment/decrement consumes
only the index. Native stack order and handlers were reviewed in p_acs.cpp.

Array values are included in ACS checksums, sorted by array ID then signed element
key, independently of insertion order. Zero writes remove entries and missing
reads do not insert them. This canonicalizes observable values for managed hashes;
it does not reproduce native dictionary allocation or native save serialization.
Arithmetic uses the existing managed wrapping/widened-division contract. Invalid
array IDs, missing operands and zero divisors terminate without changing an element.

Thirty-nine new cases cover both scopes, signed/sparse keys, the final valid array
IDs, arithmetic and boundary values, stack consumption, invalid IDs, underflow,
zero-divisor preservation, scalar/array separation, inter-script sharing and
checksum ordering/zero canonicalization. All 1,710 managed tests, including
invasion regressions, pass. This supersedes earlier statements that world/global
arrays had no runtime storage or execution support.

Array bitwise and shift operations, local/map arrays, string-array operations,
cross-map/hub ownership, complete save/restore and compact execution remain
unconverted. Sparse storage currently has no entry quota. This source/regression
audit does not establish native runtime parity or complete phases 1–3.

### ACS world/global array bitwise and shift updates

Converted AND, XOR, OR, left-shift and right-shift updates for both world/global
arrays, using the native handlers in `src/playsim/p_acs.cpp` as the reference.
Each instruction reads one array ID and consumes an element index plus operand,
preserving the lower stack. Added the missing ORWORLDARRAY and ORGLOBALARRAY enum
names at native 310/311. Existing decoder widths already support these IDs.
The shared sparse-array executor now covers all 28 basic arithmetic/bitwise/shift
instructions for these two scopes, retaining signed keys and deterministic hashes.

Thirty-six new cases cover all ten operations, high/sign bits, slot 255/63,
negative element keys, inter-script persistence, lower-stack preservation,
missing operands, invalid IDs and truncated array IDs. Shift-boundary tests
explicitly verify C#'s masked-count behavior, not portable native C++ behavior
for undefined shifts. All 1,746 managed tests, including invasion regressions,
pass. Existing zero canonicalization and checksum ordering remain unchanged.

This supersedes the previous world/global array bitwise/shift execution gap.
Local/map arrays, string-array operations, native module binding, compact
execution, cross-map/hub ownership and complete save/restore remain unfinished.
This source/regression audit is not a real-map native parity certification and
does not complete gameplay phases 1–3.

### ACS map-array declaration metadata

Added MapBehaviorMapArrayCodec to decode an already-delimited enhanced ACS chunk
region. It preserves ARAY map-variable references and unsigned lengths plus AINI
signed initializer values in source order. The first ARAY chunk wins, as in native
FindChunk; duplicate variable bindings remain available for later native-order
binding. Unknown chunks are skipped after checking their bounds. Decoding does
not allocate storage based on declared array length, and returned lists own their
data. Native ARAY/AINI loading in `src/playsim/p_acs.cpp` was reviewed.

Fifteen regression cases cover declaration/initializer ordering, repeated bindings,
first-chunk selection, zero/large lengths, signed values, source-buffer ownership,
unknown chunks, malformed payload sizes, invalid map-variable slots, truncation
and chunk-length overflow. All 1,761 managed tests, including invasion regressions,
pass. Bad input returns no partial metadata.

This codec is groundwork, not end-to-end map-array execution. It requires the caller
to delimit the enhanced chunk region; it does not locate old-format enhanced
trailers or bind arrays to the VM. MINI/import bindings, initializer application
and clipping, native string-array tags, runtime allocation limits and local-array
metadata remain unfinished. Raw initializers are intentionally not resolved to
declarations here because native binding depends on module map-variable state.

### ACS map-array binding and execution

Connected decoded ARAY/AINI metadata to an explicit TryLoadMapArrays VM API for
one behavior module, and converted all 14 map-array read/write, arithmetic,
increment/decrement, bitwise and shift instructions. Added ORMAPARRAY at native
309. Shared arithmetic evaluation with world/global arrays so signed/wrapping
and masked-shift behavior remains consistent with the existing managed contract.

Declarations bind map-variable slots to arrays in declaration order; duplicate
slots point at the last declaration. Initializers resolve through the resulting
map-variable state, apply in source order and clip to declared lengths. Existing
map-variable values are retained unless overwritten by declarations, allowing
explicit aliases. Storage owns copied values. Runtime instructions resolve the
binding on each access; invalid array bindings/elements read as zero and ignore
writes, matching FBehavior::GetArrayVal/SetArrayVal. Invalid variable slots and
malformed stack operands terminate scripts. Zero divisors leave values unchanged.

Loading rejects active scripts and validates before committing arrays/bindings.
The managed allocation limits are 4,096 arrays and 1,048,576 total elements per
load; these are supported-runtime limits, not native format restrictions. Array
shape and contents participate in ACS checksums. Twenty-seven regression cases
cover native chunk decoding through execution, all operations, aliasing, duplicate
bindings, clipped/repeated initialization, source ownership, bounds, underflow,
zero divisors, rejected-load atomicity and checksums. All 1,788 managed tests,
including invasion regressions, pass.

This supersedes the prior gap in metadata-to-VM binding and map-array instruction
execution. Callers must still locate/decode chunks and explicitly load metadata;
automatic map startup, MINI parsing, imports/multiple behavior modules, native
string tags, local arrays, compact execution and complete persistence remain
unfinished. No native real-map parity certification or phase completion is claimed.

### ACS MINI initialization and module reset correction

Added checked MINI map-variable initializer decoding and application. Initializers
retain source order and signed values, reject ranges beyond the 128 map slots,
and own their decoded data. An empty range at slot 128 is accepted. Module loading
now clears map variables, applies MINI chunks in order, binds ARAY declarations,
then applies AINI chunks, regardless of the relative placement of chunk types.

This source audit found and corrected a mismatch in the preceding implementation:
it retained prior map-variable values during loading. The native loader clears
MapVarStore first. That earlier description is superseded; aliases must now come
from MINI or subsequent script assignments. The existing alias regression was
updated accordingly. World/global state is retained, and failed or active-script
loads still leave current map state untouched.

Eight decoder and seven VM regression cases verify signed/overlapping initializers,
source ownership, range boundaries, malformed lengths, native load order, array
aliases, reset scope and failed-load atomicity. All 1,803 managed tests pass,
including invasion regressions. This closes MINI decoding/application through the
explicit module-load API. Automatic map startup, imports, local arrays, native
string tags, compact execution, persistence and native runtime validation remain
unfinished; phases 1–3 are not complete.

### ACS script-local arrays and layout metadata

Converted all 14 script-local array instructions at native 364–377. AcsProgram
now accepts LocalVariableCount (default 20) and LocalArraySizes. Each fiber owns
one zeroed local-storage block, with arrays placed after scalar locals and exposed
through checked offsets, matching ACSLocalArrays and ParseLocalArrayChunk in the
native sources. Out-of-range array/element reads return zero and writes are ignored;
missing operands and zero divisors terminate. Layouts are copied at registration,
and replacing a program does not alter active fibers. Program/fiber hashes include
layout metadata as well as code, and local values remain hashed.

Registration validates layout before replacement, with managed limits of 4,096
arrays and 1,048,576 total local slots per fiber. These are supported-runtime limits,
not native format guarantees. Arithmetic uses the existing managed wrapping and
masked-shift contract, with the same native portability caveats as scalar shifts.

Added MapBehaviorLocalLayoutCodec for already-delimited enhanced chunks. SVCT
uses signed script IDs and unsigned local counts; only its first chunk is used.
SARY sizes apply after SVCT regardless of chunk placement. Repeated SARY chunks
append storage after the prior total but replace visible descriptors, following
the native loader. Invalid chunk sizes, unsupported negative sizes and total
storage overflow reject without partial output. The codec retains raw script IDs;
named-script resolution and filtering against the script directory remain external.

Thirty-one execution/integration cases and twelve decoder cases cover operations,
offset sharing, bounds, underflow, delay/concurrent-fiber isolation, definition
ownership, active replacement, layout limits/checksums, signed IDs, chunk order,
repeated arrays and truncation. One integration case executes a layout decoded
from native SVCT/SARY bytes. All 1,846 managed tests, including invasion regressions,
pass. This supersedes the script-local array execution/metadata gaps above.

Callers still explicitly attach decoded layouts to programs. Automatic behavior
module loading, function-local FARY/call frames, script arguments, named-script
binding, compact execution, string-array operations and complete persistence
remain incomplete. Real-map/native runtime parity and full phases 1–3 completion
have not been established.

### ACS script startup arguments

Added AcsProgram.ArgumentCount and an Enqueue overload accepting a read-only span
of signed arguments. Startup copies the smaller of supplied/declared counts into
the first scalar local slots, leaving missing arguments and other local/array
storage zeroed. Extra values are ignored, matching the DLevelScript constructor
in `src/playsim/p_acs.cpp`. The original no-argument enqueue API remains available.
Argument declarations must fit scalar storage so supplied values cannot spill
into the array region. Invalid declarations reject before replacing registration.

Argument count participates in the program/fiber definition hash; copied values
participate through local-storage hashing. Fourteen regressions cover partial and
extra arguments, zero declarations, the original API, input ownership, concurrent
delayed instances, array isolation, invalid definitions, unknown scripts,
checksums and replacement of queued programs. All 1,860 managed tests pass,
including invasion regressions.

This converts startup arguments at the explicit VM API. Script-directory ArgCount
binding, map-line argument forwarding, native activator/line context, duplicate
execution/resume policy and deferred cross-map execution are not converted here.
Automatic behavior loading, function calls, compact execution, persistence and
native runtime validation remain outstanding; phases 1–3 remain incomplete.

### ACS map-line arguments and active-script guard

Extended map-line special 80 now forwards Arg2/Arg3/Arg4 to the declared script
arguments for map target zero. Added TryExecute, which rejects another active
fiber with the same script number before enqueueing, matching the running-script
check for normal ACS_Execute. Enqueue remains an explicit API permitting concurrent
instances. Native references are LS_ACS_Execute in p_lnspec.cpp and P_GetScriptGoing
in p_acs.cpp. Nonzero map targets now fail instead of silently starting locally;
numeric map lookup and deferred cross-map execution are not yet implemented.

Eleven regression cases cover ordered/signed arguments, use/cross activation,
one-shot/repeat rules, missing scripts, nonzero map requests, active/delayed
duplicates, preservation of initial arguments, restart after completion and zero
filling beyond the three line arguments. Failed activation leaves the line intact.
All 1,871 managed tests, including invasion regressions, pass. Initial test fixture
compile errors were corrected by constructing immutable line arguments rather
than trying to modify them.

This closes argument forwarding for the extended map-line path only. The legacy
internal Execute API is unchanged. Native suspended-script resume, ACS_ExecuteAlways,
activator/line context, numeric map resolution (including a nonzero number naming
the current map), automatic module loading, function calls and persistence remain
unfinished. These tests do not certify native runtime parity or phases 1–3 completion.

### ACS opcode suspension and normal execute resume

Corrected PCD_SUSPEND (2), which previously terminated the fiber. It now leaves
the fiber suspended after that instruction; scheduler ticks skip it. TryExecute
resumes a suspended matching script without replacing its arguments or local
state, following P_GetScriptGoing in p_acs.cpp. Already-running scripts still
reject normal duplicate execution. Definition replacement does not alter a
suspended fiber's retained code/layout. Suspension state participates in checksums.

RunningCount continues to count all nonterminated fibers, including suspended
ones, so map-storage reload stays blocked. SuspendedCount reports the suspended
subset. Seven regressions cover local/array/argument preservation, repeated
suspensions, definition replacement, checksum/state stability, storage-reload
rejection, one-shot map-line resume, post-resume delay timing and other scripts
continuing to run. All 1,878 managed tests, including invasion regressions, pass.

This closes opcode-triggered suspend/resume for normal extended map-line execution.
External ACS_Suspend/ACS_Terminate specials and native ACS_ExecuteAlways tracking
remain unconverted. Direct Enqueue can still create concurrent instances; normal
TryExecute selects the first live matching fiber. The managed evaluation stack
is retained across yields; native valid scripts are expected to have an empty
evaluation stack when returning from RunScript, so malformed nonempty-stack yield
parity is not established. Persistence and native runtime validation remain open.

### ACS suspend/terminate map-line controls

Converted extended map-line specials 81/82 for target map zero, backed by VM
SuspendScript/TerminateScript operations. Current-map requests succeed even if
no script matches, following LS_ACS_Suspend/Terminate in p_lnspec.cpp. Control
targets the first live matching fiber, consistent with the current managed normal
execute model. Suspended fibers retain their state; terminated fibers stop being
eligible to run and are removed by the next scheduler cleanup.

Source review of SetScriptState and P_GetScriptGoing showed that external suspend
replaces the delayed state, and resume enters Running. Resume now clears the prior
delay instead of waiting out the old counter. Ten regressions cover missing-script
success, activation/repeat rules, unsupported map targets, delayed suspend/resume,
termination of delayed/suspended scripts, fresh execution after cleanup and
suspension before the first tick. All 1,888 managed tests, including invasion
regressions, pass.

Nonzero map targets reject without mutation; numeric map lookup/deferred requests
remain unsupported. This intentionally differs from the native control specials'
unconditional success for unresolved map numbers. Native ExecuteAlways tracking,
client-side script controllers and exact same-tick termination/restart ordering
remain unverified. Script-invoked control specials, automatic module loading,
function calls, persistence and real-map/native runtime validation remain open.

### Script-invoked ACS execute/suspend/terminate specials

Connected specials 80–82 to both direct and stack LSPEC execution, including
result-returning forms. Map lines and scripts now share ExecuteScriptControl,
so argument order, current-map checks, duplicate rejection and control results
stay consistent. Missing direct operands reject before dispatch. Unsupported
specials retain the existing handling outside this converted set.

Twenty-two cases cover all five direct/stack arities, child-script argument
forwarding, standard/extended result values, unsupported map targets, duplicate
self-execution, self-suspend/resume, self-termination, controlling a later queued
script and truncated control operands. All 1,910 managed tests, including invasion
regressions, pass. Native LSPEC stack ordering and LS_ACS control handlers were
reviewed in p_acs.cpp/p_lnspec.cpp.

The scheduler takes a snapshot per tick: a newly started child runs on the next
VM tick, while a suspended/terminated target already in that snapshot is skipped.
Tests establish this managed scheduling contract; exact native same-tick ordering
is not certified. Activator/back-side context, legacy Hexen argument masks,
ExecuteAlways tracking, remote map handling, automatic module loading, function
calls and persistence remain unfinished. This supersedes the earlier gap in
script-invoked control-special dispatch, not full native runtime parity.

### ACS_ExecuteAlways concurrent instances

Converted current-map special 226 for extended map lines and direct/stack ACS
special calls, including result-returning forms. Each call creates an independent
instance with its own arguments, locals, arrays and execution state. Native
P_GetScriptGoing, DLevelScript construction and SetScriptState in p_acs.cpp
exclude ACS_ALWAYS instances from RunningScripts. Managed normal execute/resume,
suspend and terminate now likewise ignore always instances. Self-suspend and
self-termination opcodes still act on the executing instance. The control mode
participates in the VM checksum because it changes future control behavior.

Fourteen regressions cover concurrent arguments across delay, coexistence with
normal execution, normal duplicate rejection, selective suspend/terminate,
resume preserving old arguments, self-suspended always instances, map-array
reload exclusion, missing programs, checksum distinction, map-line activation
and repeat rules, argument forwarding, rejected remote targets and direct/result
script dispatch. All 1,924 managed tests pass, including prior invasion coverage.
Source review used LS_ACS_ExecuteAlways in p_lnspec.cpp and the tracking code above.

This closes the earlier ExecuteAlways tracking gap for these managed entry points.
The legacy public Enqueue API still permits multiple normal instances and numbered
control selects their first live match; it is not a native RunningScripts registry.
Exact native same-tick scheduling, named scripts, activator/back-side context,
client-side controllers, remote/deferred map execution, automatic module loading,
function calls, persistence and real-map/runtime parity remain incomplete. This
pass does not certify the original invasion round/timer/enemy symptom as fixed.

### Numbered ACS ScriptWait

Converted wordcode ScriptWait (81) and ScriptWaitDirect (82), following the
default native PCD_SCRIPTWAIT handlers and SCRIPT_ScriptWaitPre/ScriptWait states
in p_acs.cpp. An absent target must first be observed running; an existing target
must finish before execution resumes. Suspended normal instances still count,
while ExecuteAlways instances do not. Wait state and target enter the checksum.
External suspend followed by normal execute resumes after the wait instruction,
discarding the previous wait state as native state replacement does. Waiting
instances remain live and prevent map-array reloads.

Eleven regression cases cover stack/direct forms, delayed and absent targets,
always-instance exclusion, suspended targets, termination, both wait states on
resume, self-wait, truncated operands/stack underflow, and checksum target state
with identical remaining locals and stacks. The full 1,935-test suite passes.

The managed scheduler retains its per-tick snapshot/order. Native PutLast/PutFirst
reordering and exact same-tick completion/restart behavior are not reproduced;
a target that starts and finishes between waiter observations can be missed.
Named waits, the COMPATF2_SCRIPTWAIT legacy direct-wait behavior, client-side
controllers and full module loading remain unconverted. This is a source and
regression self-audit, without real-map/native runtime equivalence validation.

### Legacy direct ScriptWait compatibility and scheduler review

Added the opt-in CompatSurface.LegacyScriptWaitDirect switch, corresponding to
the native COMPATF2_SCRIPTWAIT branch of PCD_SCRIPTWAITDIRECT in p_acs.cpp.
Only the direct opcode skips the pre-start wait. It still yields and checks for
a running numbered target on its next scheduled evaluation. Stack ScriptWait
retains the default start-then-finish behavior. Always instances remain excluded.
The flag is supplied through AuthoritySimulation.Start; native compatibility
database/MAPINFO selection is not automatically loaded.

Ten regressions cover the direct/stack and enabled/disabled matrix, delayed and
suspended targets, a target started between ticks, always-instance exclusion,
wait-state checksum differences and truncated operands. All 1,945 managed tests
pass, including the existing invasion regression suite. This closes the prior
legacy direct-wait opcode behavior gap when the switch is explicitly configured.

The scheduler audit also reviewed DACSThinker::Tick and DLevelScript::Link,
PutLast and PutFirst. Native code traverses mutable links, starts scripts at the
front by default and reorders waiters. The managed VM still snapshots its list
in insertion order; changing this requires coordinated execution/control timing
work rather than just moving a waiter in the list. Native ordering, Hexen startup
ordering, named waits and real-map validation remain open. No claim is made that
the original invasion timer/round/enemy symptom is resolved by this change.

### ACS TagWait for managed sector movers

Converted wordcode TagWait (61) and TagWaitDirect (62). Source review of the
native opcode handlers and SCRIPT_TagWait in p_acs.cpp shows that either floor
or ceiling ownership blocks a tagged wait, including paused movers. The managed
VM now checks sector tags against its live motion list, yields even when no
matching mover exists, and resumes when that list no longer contains a matching
sector. Unrelated tags do not block it. External suspend/resume discards the old
wait state, and the saved tag/presence are included in the checksum.

Twelve cases cover stack/direct waits, floor and ceiling completion, absent tags,
both planes moving together, unrelated motion, paused ceiling removal, map-array
reload exclusion, resume, malformed operands and saved-tag checksum differences.
The paused-ceiling test initially supplied removal mode in the wrong argument;
correcting the fixture to use Arg1 matched the existing special contract. All
1,957 managed tests then passed, including previous invasion regressions.

This converts waits for the current managed motion representation, not all native
floor/ceiling thinkers. Completed stair records retained until group cleanup,
multi-tag sectors, unconverted movers and native thinker ordering still need
parity work. Managed simulation advances movers before ACS; exact native tick
ordering and representative real-map validation remain unverified. PolyWait and
named script waits remain unconverted. Phases 1–3 are still incomplete.

### Completed stair steps release floor ownership

The follow-up TagWait audit found that completed stair records were serving two
different roles: active floor ownership and the chain's stair retrigger lock.
Native DFloor completion in mapthinkers/a_floor.cpp clears floordata before
retaining/updating stairlock. Ordinary floor starts check PlaneMoving, whereas
stair starts also check stairlock. The managed retained Completed records now
block only stair reconstruction. TagWait and both managed ordinary floor start
paths ignore them, while still respecting any new active mover on that sector.

Six regressions cover stack/direct TagWait release before the chain finishes,
ordinary map and internal floor activation after step completion, a replacement
floor mover continuing to block TagWait, rejection while a step is still moving,
and retention/eventual release of the stair chain lock. All 1,963 managed tests
pass, including existing mover, ACS and invasion regressions.

This closes the completed-stair ownership gap recorded in the preceding audit.
Stair traversal/reset variants, compatibility-specific lock behavior, advanced
geometry and exact native thinker scheduling still need conversion or validation.
The checks are source review and managed regressions, not native runtime parity
or confirmation of the original invasion round/timer/enemy symptom.

### Stair traversal honors declared two-sided lines

Source review of EV_BuildStairs in mapthinkers/a_floor.cpp found that native
traversal requires ML_TWOSIDED before checking front/back sectors and texture.
Managed traversal previously checked only valid side references. It now also
requires LevelLine.TwoSidedFlag (4). UDMF LevelBuilder now preserves the parsed
twosided property in Flags; binary formats already copy linedef flags.

Three new regression cases cover a back-side reference without the flag and
both values through UDMF text parsing, level construction and stair activation.
Existing synthetic stair fixtures now explicitly declare two-sided connectivity.
The new UDMF fixture initially omitted player spawn eligibility flags; adding
those corrected the fixture. All 1,966 managed tests pass, including previous
stair ownership, TagWait and invasion coverage.

This closes the declared-connectivity gap in the converted stair traversal.
Other geometry consumers still use their existing side-reference rules; this
does not certify malformed-map handling across the engine. Stair sync/reset,
special-sector traversal, compatibility options, slopes and real-map/native
runtime parity remain unfinished. No native files or dependencies changed.

### Synchronized Doom stair map specials

Added extended map specials 271/272 for upward/downward synchronized Doom stairs
with reset zero. Source review of LS_Stairs_BuildUpDoomSync/DownDoomSync and
EV_BuildStairs confirms that later steps use base speed multiplied by their
signed travel distance divided by the signed step size. The seed retains base
speed. This lets unobstructed steps with uneven initial heights finish together.
The unused fifth argument is ignored, matching the native handlers.

Stair construction now stages its motion records before committing. A sync chain
requiring a negative computed speed rejects without creating any movers: the
managed mover lacks the native explicit direction semantics needed for that
case. Pending motions participate in busy checks so branches and overlapping
tagged seeds retain the existing exclusion behavior. Ordinary stair regression
coverage remains passing after this refactor.

Ten new cases cover upward/downward chains with level and uneven initial floors,
repeat/busy activation, unsupported resets, atomic reversed-distance rejection
and a step already at its target. All 1,976 managed tests pass. This is partial
sync conversion: reset, reversed-distance semantics, ceiling-limited/obstructed
timing, special-sector traversal, compatibility options and native runtime
comparison remain unfinished. It does not certify invasion synchronization or
complete phases 1–3.

### Stair reset countdown and return

Converted nonnegative reset arguments for map stair specials 217/270/273 with
delay zero and synchronized specials 271/272. Native DFloor::Tick decrements
the reset countdown from activation, before motion, and reverses toward the
original floor when it reaches zero. Managed stairs now follow that state
sequence, including reversal before construction finishes. Built steps awaiting
reset retain floor ownership, blocking ordinary floor starts and TagWait. They
release ownership after the return finishes; chain locks retain their existing
group cleanup behavior. Countdown and waiting state are checksum inputs.

Nine new cases cover all five specials, upward/downward early reversal,
countdown checksum differences before positions diverge, and TagWait remaining
blocked through the built/waiting/return states. The existing negative-reset
rejection tests remain. All 1,985 managed tests pass, including invasion coverage.

This supersedes the preceding audit's unsupported nonnegative reset restriction.
Negative reset values still reject atomically. Per-step delay, native exact
destination-crossing timing, obstructed/ceiling-limited movement, reversed sync
distances, advanced geometry and real-map/native runtime comparisons remain
unfinished. This source/regression audit does not establish complete stair or
invasion parity, and phases 1–3 remain incomplete.

### Per-step stair delays and reset interaction

Converted nonnegative delay arguments for Doom stair specials 217/270/273.
EV_BuildStairs initializes the step interval by truncating step height divided by
base speed; DFloor::Tick schedules a pause after that many active movement ticks.
The managed mover now tracks that interval, its countdown and pause ticks, and
includes them in the checksum. A sub-tick interval truncates to zero and does
not schedule pauses. Negative delays and intervals beyond the managed integer
range reject before creating motion records.

Reset countdowns run before pause handling. If a reset expires during a pause,
the transition tick still returns without moving; subsequent return ticks skip
the old construction pause machinery, matching the native state branch. Seven
new regressions cover all three specials, upward/downward reset during pause,
sub-tick intervals and negative-delay rejection. All 1,992 managed tests pass,
including earlier stair/ACS and invasion cases.

This closes the prior per-step-delay restriction for these map specials. Exact
native destination-crossing timing, blocked and ceiling-limited motion, native
compatibility variants, special-sector traversal and runtime comparisons remain
open. The source/regression audit is not full native parity validation; phases
1–3 and real-map confirmation of invasion synchronization remain incomplete.

### Blocked stair arrival and mover completion

Review of sector_t::MoveFloor in mapthinkers/dsectoreffect.cpp confirmed inclusive
destination comparisons for unobstructed flat planes. It also exposed a mismatch:
native destination-clamped upward moves report pastdest even when obstruction
rolls the floor back. Managed stairs previously kept retrying that endpoint.
Stairs now retain the arrival result separately from the applied floor position,
allowing completion or reset-wait state after rollback. Intermediate obstruction
still retries. Later crushing steps retain exact arrivals but roll back blocked
overshoots, matching the native stopMoving condition.

Nine cases cover blocked seed endpoints with/without crushing, exact versus
overshooting arrival, intermediate blockage and release, reset after a rolled-back
build endpoint, and later crushing steps. The latter fixture was corrected to
establish actor sector membership through geometry and a simulation tick rather
than assigning the read-only sector property. All 2,001 managed tests pass.

The behavior change is restricted to stairs; other floor/lift endpoint handling
still needs its own parity review. Native actor clipping, attached sectors,
downward obstructions, slopes, compatibility variants and real-map comparisons
remain outside this validation. This closes the audited upward stair endpoint
case, not full mover or invasion parity. Phases 1–3 remain incomplete.

### Ordinary floor and returning lift endpoint obstruction

Extended the upward endpoint correction to ordinary floor and lift motions.
Native MoveFloor reports pastdest on a destination-clamped attempt even after
obstruction rollback; DPlat::Tick distinguishes this from an intermediate
crushed result. Managed floors now release ownership at such endpoints, and
returning down-wait-up-stay lifts finish instead of reversing. Intermediate
noncrushing floor obstruction still retries, and intermediate returning lift
obstruction still reverses. Crushing floors retain exact blocked arrivals but
roll back blocked overshoots before finishing.

Six new regressions cover exact/overshooting ordinary floor endpoints, intermediate
obstruction and recovery, returning lift completion, and crushing exact versus
fractional overshooting targets. Existing intermediate lift reversal and stair
regressions remain passing. All 2,007 managed tests pass, including invasion cases.
Source review used MoveFloor, DFloor::Tick and DPlat::Tick in native mapthinkers.

This closes the preceding audit's upward ordinary floor/lift endpoint review gap.
Downward obstruction, actor clipping/stacking, attached planes, unconverted mover
types, compatibility behavior and real-map/runtime comparisons remain unfinished.
It is a source and regression self-audit, not full native or invasion parity.

### Door closing endpoint obstruction

Reviewed MoveCeiling in dsectoreffect.cpp and the closing branch of DDoor::Tick
in a_doors.cpp. A blocked destination-clamped close reports pastdest after
rollback, and native doorRaise releases its ceiling thinker. The managed door
previously reopened for every closing obstruction. It now finishes at blocked
endpoints while retaining the rolled-back ceiling height. Obstruction before
the destination still reopens and retains ownership.

Three regressions cover exact and overshooting blocked close endpoints,
non-damaging rollback, subsequent reactivation, and intermediate obstruction
retaining the door mover. All 2,010 managed tests pass, including previous
door, floor, stair, ACS and invasion coverage.

This addresses the represented raise/wait/close door's endpoint behavior.
Unconverted door types, opening obstruction through attached geometry, native
actor clipping and sound/light effects still need work. Real-map/native runtime
parity and original invasion synchronization remain unverified; phases 1–3 are
not complete. This remains a source/regression self-audit.

### Extended Door_Close map special

Converted extended map special 10 with light tag zero. LS_Door_Close and
DDoor::Tick in the native sources distinguish close-only doors from ordinary
raise/wait/close doors: intermediate obstruction retains the closing direction,
while destination-clamped obstruction completes after rollback. The managed
motion now records CloseOnly and hashes it. Closing can start without neighboring
sectors, targets the sector floor, respects ceiling ownership and uses speed/8.

Six regressions cover intermediate obstruction and recovery, exact/overshooting
blocked endpoints, no-neighbor closure, repeat/use rules, zero speed and unsupported
lighting rejection. All 2,016 managed tests pass, including previous door/mover,
ACS and invasion coverage. Nonzero light tags reject without consuming the line.

This adds the current flat-sector map activation path. Door-linked lighting,
close-wait-open, native manual reactivation rules, script dispatch for this special,
sound effects, attached geometry and runtime parity remain incomplete. Native
behavior for zero/invalid speed is not claimed. Phases 1–3 and real-map invasion
validation remain open.

### Extended Door_CloseWaitOpen and ACS wait integration

Converted extended map special 249 for positive supported delays and light tag
zero. Native LS_Door_CloseWaitOpen converts eighth-second units using delay*35/8;
DDoor stores the original ceiling as the reopening destination. Managed doors
now close, retain ceiling ownership for the converted wait, then reopen to that
original height. Intermediate closing obstruction reverses immediately; blocked
arrival rolls back and enters the wait. Reopening completion releases ownership.
The new motion mode participates in the simulation checksum.

Nine regressions cover delay conversion/truncation, original-height restoration,
both obstruction paths, invalid delay/lighting rejection, reactivation and ACS
TagWait blocking throughout closing, waiting and reopening. All 2,025 managed
tests pass, including existing invasion regressions. Source review used the
native special handler and DDoor constructor/Tick state transitions.

Zero/negative delays and values exceeding the checked multiplication range reject
without changing the map; native behavior for those inputs is not reproduced.
Door-linked lighting, script dispatch, manual reactivation, sound/attached-plane
behavior and real-map/native runtime comparisons remain incomplete. This closes
the earlier close-wait-open map-special gap for the stated subset, not phases
1–3 or the original invasion synchronization validation.

### ACS dispatch for converted closing door specials

Connected door specials 10/249 to direct and stack ACS LSPEC dispatch, including
result-returning forms. Map lines and scripts share ExecuteDoorSpecial so speed,
delay, unsupported lighting and busy-ceiling results use the same checks. Script
calls with tag zero reject because the current VM does not retain activation-line
context; map-line manual targeting still uses its supplied line. Truncated direct
operands terminate before dispatch. Unsupported options return false rather than
falling through to a differently numbered legacy internal action.

Eleven regressions cover direct/stack calls for both specials followed by TagWait,
standard/extended result forms, missing tags, tag zero, invalid speed/lighting
and truncated bytecode. All 2,036 managed tests pass, including prior map-line,
mover, ACS and invasion regressions. The implementation reuses the native-derived
door handlers and previously reviewed LSPEC argument/result ordering.

This closes script dispatch for these two converted door specials. Open/raise
script dispatch, activation-line/actor context, lighting, manual reactivation,
native scheduler parity and real-map comparisons remain unfinished. Neither full
phases 1–3 completion nor original invasion synchronization is certified.

### ACS Door_Open and Door_Raise dispatch

Connected specials 11/12 to the shared door helper used by map lines and ACS
direct/stack/result calls. Source review of LS_Door_Open/Raise confirms speed/8,
raise delay in tics and the differing light-tag argument positions. Both entry
paths now reject nonzero lighting tags instead of map activation silently ignoring
them; negative raise delays also reject without starting a mover or consuming a
line. Current tag-zero script context restrictions remain.

Seven new cases cover direct/stack opening and raising, argument forwarding,
positive-delay timing, TagWait completion, and consistent map/script rejection
of unsupported lighting or negative delay. All 2,043 managed tests pass, including
previous door/mover, ACS and invasion coverage.

This closes the previous open/raise script-dispatch gap for the represented
subset. The existing zero-delay raise behavior remains a managed compatibility
choice and is not certified against native countdown underflow behavior. Native
manual reactivation, door lighting, actor/line context, special geometry, sound
and runtime comparisons remain unfinished. Phases 1–3 are incomplete.

### ACS dispatch for converted Doom stair specials

Connected specials 217 and 270–273 to ACS direct/stack/result dispatch and shared
ExecuteStairSpecial with map activation. Native handler review confirmed the
different delay/reset argument positions for ordinary and synchronized stairs.
The shared path preserves those positions, direction, crushing and sync options.
Tag-zero script calls reject because activation-line context is not retained;
map-line manual targeting remains available. Failed/truncated calls do not
partially create movers.

Sixteen cases cover direct/stack calls for all five specials with reset and
TagWait, result forms, absent/manual tags, zero speed and truncated operands.
The initial delayed-stair test expectation missed the reset-during-pause tick;
correcting that expectation matched the previously audited native transition.
All 2,059 managed tests pass, including prior map stair, door, ACS and invasion
coverage. Existing map-line activation and repeat handling are retained.

This connects the converted stair subset to scripts, not complete ACS or native
stair parity. Activation context, reversed-distance sync cases, compatibility
variants, special-sector traversal, advanced geometry and runtime comparisons
remain open. Phases 1–3 and original invasion symptom validation remain incomplete.

### ACS dispatch for selected floors and lifts

Connected floor specials 20/21/23/25 and lift 62 to direct/stack/result ACS calls,
sharing ExecuteFloorSpecial with map-line activation. Native handler review
confirmed speed/8, value/neighbor targets, lift delay in tics and its eight-unit
lip. The shared helper rejects unconverted texture/type-change and crushing
options instead of silently ignoring them, and rejects negative lift delays.
Tag-zero script requests still need unconverted activation-line context.

Fourteen cases cover direct/stack value moves and lifts, neighbor targets,
TagWait through completion/return, failure results for unsupported arguments and
truncated direct operands. All 2,073 managed tests pass, including prior mover,
ACS and invasion regressions. Existing map activation/repeat handling remains.

This connects the represented floor/lift subset to ACS. Texture/type transfer,
the rejected crushing variants, full activation context, native zero-delay lift
behavior, advanced geometry and real-map/native comparisons remain unfinished.
Other native floor/platform specials are still unconverted. Phases 1–3 and the
original invasion synchronization validation remain incomplete.

### Crushing arguments for raising floors

Converted the crushing argument for specials 23/25 through their shared map/ACS
helper. Native LS_Floor_RaiseByValue/RaiseToNearest use CRUSH(a), enabling positive
damage and disabling nonpositive values, and request Hexen crushing semantics.
The managed mover now receives that damage plus StopOnCrush: intermediate
obstruction damages on the existing four-tic cadence but rolls the floor back
until space clears. Existing destination-arrival handling remains in force.

Eight new cases cover both specials through map and direct ACS activation,
requested damage amounts, rollback/recovery, and zero/negative damage disabling.
The earlier unsupported-argument cases now test texture/type changes rather than
the newly supported crush arguments. All 2,081 managed tests pass, including
previous mover, ACS and invasion regressions.

This closes the previous rejection of those two crushing arguments. Native actor
clipping, corpses/items, stacked actors, exact damage effects, texture/type
transfer, advanced geometry and real-map/native runtime comparisons remain open.
It does not certify complete mover or invasion parity; phases 1–3 remain incomplete.

### ACS ceiling value moves and stop controls

Connected specials 40/41/44 to shared map/ACS dispatch. Native handler review
confirmed speed/8, height/change/crush argument positions and explicit stop mode
1 (pause) versus 2 (remove). Tag-zero script requests still reject without
activation-line context. Other map ceiling specials keep their existing paths.

Ten new cases cover direct/stack raise/lower moves with TagWait, explicit pause
retaining the wait, removal releasing it, result values, missing/manual tags and
unsupported texture/type-change rejection. All 2,091 managed tests pass,
including prior ceiling, mover, ACS and invasion regressions.

Native default stop mode depends on gameinfo.gametype (Hexen removes; others
pause). The managed default still pauses; map format alone cannot establish game
type. That compatibility gap remains open, along with native stop target/result
edge cases, unconverted ceiling/crusher script dispatch, texture/type transfer,
activation context, advanced geometry and native runtime comparison. This is a
source/regression self-audit; phases 1–3 and invasion runtime validation remain
incomplete.

### Ceiling stop targeting correction and unconditional stop

Native EV_CeilingCrushStop in a_ceiling.cpp filters DCeiling thinkers by their
creation tag and nonzero direction. Corrected managed special 44 accordingly:
paused movers no longer report success or get removed, and matching uses the
motion tag rather than current sector tags. Tag zero is valid for this registry
lookup and no longer requires script activation-line context.

Added Ceiling_Stop (276) to map/ACS dispatch, following EV_StopCeiling: target
sectors, remove their ceiling ownership including paused ceiling movers and doors,
preserve floor movers, and succeed even when no target exists. Existing tests
that incorrectly used CrushStop to remove paused movers now use Ceiling_Stop;
their TagWait release assertions remain. Tag-zero script Ceiling_Stop still
rejects without activation-line context.

Four new cases cover paused-mover exclusion, creation-tag matching including
manual tag zero, unconditional removal of doors without stopping floors, and
missing-target success/one-shot consumption. All 2,095 managed tests pass,
including the corrected ACS/TagWait cases and prior invasion regressions.

This corrects the earlier pause/remove audit's incomplete native targeting model.
Game-type-dependent default mode, native resume edge cases, unconverted ceiling
thinkers, activation context and real-map/native runtime validation remain open.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Ceiling creation tags and native resume ordering

Follow-up review of EV_DoCeiling/CreateCeiling/ActivateInStasisCeiling found that
manual ceiling activation replaces tag zero with sectorIndex | 0x1000000.
Managed ceiling movers now store that derived creation tag. Tagged looping
crusher activation resumes paused ceiling movers by creation tag before scanning
sector targets, preserving the original direction, speed, target and mode.
It can therefore resume a mover even when no sector has that tag.

Manual activation also resumes by its derived tag before attempting creation.
As in the native manual branch, resuming an existing mover can return false
because CreateCeiling then refuses a second mover. Other tagged non-looping
requests do not resume paused movers. The earlier tag-zero stop test was corrected
to use the native derived tag rather than assuming a stored zero.

Four new cases cover manual resume/result behavior, mismatched creation tags,
tagged resume preserving movement, and resume without a matching sector tag.
All 2,099 managed tests pass, including prior mover/ACS/invasion coverage.
This corrects the preceding audit's incomplete manual-tag model. Invalid argument
edge cases, game-specific stop defaults, full activation context and native
runtime comparisons remain open; phases 1–3 are still incomplete.

### ACS crusher activation and lifecycle

ACS line-special dispatch now shares map activation for Ceiling_CrushAndRaise
(42), Ceiling_LowerAndCrush (43), and Ceiling_CrushRaiseAndStay (45). Source
review against native p_lnspec.cpp preserves their existing managed speed,
return-speed, crush-mode and looping settings. This closes the dispatch gap for
these three converted movers, including result-returning script calls.

Eight new regression cases cover direct and stack activation, lower-only and
return-and-stay completion, looping crusher pause/resume with original settings,
TagWait blocking until removal, and success results from both result opcodes.
All 2,107 managed tests pass. This does not establish full native crusher parity:
activation-line context, clipping/damage details, invalid-argument edge cases,
game-specific stop defaults and remaining ceiling variants still need work.
Phases 1–3 and real-runtime invasion synchronization validation remain incomplete.

### ACS distance crushers and ceiling geometry targets

Shared map/ACS dispatch now includes Ceiling_CrushAndRaiseSilentDist (104),
Ceiling_CrushAndRaiseDist (168), Ceiling_LowerToFloor (254), and
Ceiling_RaiseToHighest (262). Review against src/playsim/p_lnspec.cpp confirms
argument placement: distance crushers use argument 1 for clearance and argument
2 for equal down/up speeds; LowerToFloor uses argument 4 for clearance and
argument 3 for crush damage. Unsupported ceiling texture/type changes still
return false without starting a mover. The existing map behavior is retained.

Twelve added regression cases exercise direct/stack calls, requested crusher
clearance and equal return speed, continuing loops, floor clearance and TagWait
release, highest neighboring ceiling targets, and rejected change flags through
both result opcodes. All 2,119 managed tests pass. The initial test compile caught
incorrect fixture property names; these were corrected before the full run.

This closes these four ACS dispatch gaps only. Silent-versus-audible native
sound behavior is not implemented by this mover layer. Native clipping/damage,
activation context, invalid/no-op target edge cases, remaining ceiling variants
and real-map comparison remain open. Phases 1–3 remain incomplete.

### Separate-speed crushers and crush-and-stay slowdown audit

Map and ACS activation now implement Ceiling_LowerAndCrushDist (97),
Ceiling_CrushRaiseAndStayA (195), Ceiling_CrushAndRaiseA (196),
Ceiling_CrushAndRaiseSilentA (197), and Ceiling_CrushRaiseAndStaySilA (255).
Review of src/playsim/p_lnspec.cpp and mapthinkers/a_ceiling.cpp establishes
independent down/up speeds for the A variants and their zero floor clearance,
unlike the eight-unit clearance of specials 42/43/45. Special 97 uses its
explicit distance and finishes after lowering; 195/255 return and finish;
196/197 continue looping.

The native thinker only applies slowdown after obstruction to looping and
lower-only crusher types. Managed special 45 incorrectly enabled slowdown for
mode 3; it now maintains its speed, as do new 195/255 movers. This supersedes
the earlier crusher activation audit's implicit claim that all of special 45's
existing crush-mode behavior was preserved correctly.

Twenty-one new regression cases cover direct ACS, stack ACS and map activation,
distinct down/up speeds, clearance, completion versus looping, TagWait release,
and obstructed mode-3 slowdown differences with an actor in the target sector.
All 2,140 tests pass. Game-specific default crush modes, silent/audible sound
sequences, complete native clipping/damage, activation context and real-map
validation remain open. These additions do not complete phases 1–3 or certify
the original invasion synchronization symptom in a running game.

### Eight-times-distance ceiling movers

Map and ACS dispatch now support Ceiling_RaiseByValueTimes8 (198) and
Ceiling_LowerByValueTimes8 (199). Native p_lnspec.cpp scales argument 2 by eight
but leaves the speed in eighth-units per tic. Both handlers explicitly pass
non-crushing damage (-1), even though the lower variant's comment mentions a
crush argument. The managed handlers therefore ignore argument 4, preserving
the executable native behavior. Texture/type change requests remain unsupported.

Fourteen regressions cover direct/stack ACS and map activation, scaled travel,
one-shot map consumption, TagWait timing, negative and overflowing distances,
unsupported changes, and blocked lowering that causes no damage then completes
after the obstruction clears. Large script distances are rejected before integer
multiplication can wrap; this is a managed validation choice, not a claim of
native behavior for out-of-range inputs. All 2,154 managed tests pass.

Zero-distance activation, signed negative-distance native semantics, general
height limits, complete clipping, texture changes and real-map parity remain
open alongside the previously recorded completion gates. Phases 1–3 are not
complete, and original invasion synchronization remains runtime-unverified.

### Nearest neighboring ceiling targets

Map and ACS dispatch now support Ceiling_RaiseToNearest (252) and
Ceiling_LowerToNearest (264). Source review of p_lnspec.cpp, a_ceiling.cpp and
p_sectors.cpp establishes strict directional selection: the smallest higher
neighbor for raising and largest lower neighbor for lowering, excluding equal
heights. Managed selection preserves fractional heights. Lowering takes optional
crush damage from argument 3; raising remains non-crushing. Change flags are
rejected because ceiling texture/type changes remain unconverted.

Twelve regressions cover map/direct/stack activation, closest-neighbor selection
among higher/lower/equal candidates, fractional travel, TagWait completion,
unsupported change results, absence of a suitable directional neighbor, and
damaging versus non-damaging obstruction. The obstruction fixture initially
failed its sector-membership assertion because its lines had no polygon; adding
actual room boundaries corrected the fixture. All 2,166 tests now pass.

The no-target managed path currently declines to create a mover, unlike native
creation of a no-travel thinker. Native lower-to-nearest also has a distinct
floor-height fallback for sectors with no lines. Those result/ownership/timing
edge cases are not converted in this pass. Slopes, linked-sector exclusions,
full clipping, height limits and runtime parity remain open; phases 1–3 and
original invasion synchronization validation remain incomplete.

### Nearest-ceiling no-travel ownership and results

The no-directional-neighbor gap recorded above is now corrected for specials
252/264 on sectors with boundaries. Native CreateCeiling creates and owns a
ceiling thinker even when the chosen height equals the current height. Managed
nearest-target activation now likewise succeeds, occupies the ceiling, and
finishes on the first mover tick. One-shot map lines are consumed and ACS result
forms return success. Pausing before that tick retains ownership and blocks
TagWait until completion or explicit removal.

Eight added cases cover both result opcodes, competing mover rejection and
subsequent ownership release, one-shot consumption, TagWait timing, and paused
no-travel mover removal. All 2,174 managed tests pass. Existing nearest-target
movement and other mover/ACS/invasion regressions remain passing.

Further source review qualifies the preceding no-lines note: although
FindNextLowestCeiling has a floor-height fallback, CreateCeiling dereferences
the sector's first line before calling it. Therefore that helper fallback alone
does not establish valid native activation behavior for a boundaryless sector.
Such synthetic/invalid geometry remains outside runtime parity claims. No-travel
behavior of other ceiling types, full native scheduling/clipping, linked sectors,
slopes and real-game invasion validation remain open. Phases 1–3 are incomplete.

### Zero-travel ceiling creation and crusher cycles

Native CreateCeiling creates a thinker before choosing its target and does not
reject an equal destination. Managed ceiling creation now retains equal-target
movers for all converted ceiling types, extending the preceding nearest-target
fix. Ordinary zero-travel requests succeed, own the plane, block competing
ceiling movers, and release ownership on their first tick. One-shot map
activations are consumed and TagWait remains blocked until that tick.

Zero-travel crushers retain their normal leg lifecycle: lower-only completes
on the first tick, crush-and-stay changes direction then completes on the second,
and looping crushers continue alternating legs until stopped. Thirteen added
regressions cover zero-distance value/times-eight requests, equal floor-gap and
highest-neighbor targets, ACS results, ownership, map consumption, TagWait and
the three crusher lifecycles. All 2,187 managed tests pass.

This closes the previously recorded equal-target creation gap for converted
ceiling types with positive speed. Zero/negative speeds, reversed crusher
destinations, native collision side effects for already-overlapping actors,
full scheduler ordering and real-runtime parity remain outside this pass.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Instant ceiling raise and lower

Map and ACS dispatch now support Ceiling_LowerInstant (193) and
Ceiling_RaiseInstant (194). Native p_lnspec.cpp scales the height argument by
eight and ignores argument 1. Native a_ceiling.cpp sets movement speed to that
distance, so movement happens in one thinker tick rather than inside activation.
The managed instant target types follow that rule, including a zero-speed,
zero-distance thinker that completes on its first tick. This exception does not
relax speed validation for ordinary movers. Lowering uses argument 4 as crush
damage; raising remains non-crushing. Unsupported changes, negative distances
and multiplication overflow are rejected.

Review of sector_t::MoveCeiling in dsectoreffect.cpp confirms destination-clamped
blocked moves complete even when rolled back. Eleven new cases cover map,
direct and stack activation, ignored speed arguments, delayed first-tick motion,
TagWait release, zero-distance ownership, non-crushing rollback, exact-distance
crushing, and rollback with damage after an overshoot past the floor clamp.
All 2,198 managed tests pass.

Native interpolation suppression, sound sequences, texture/type changes, full
actor/attached-sector clipping, negative-distance semantics and real-runtime
comparison remain unconverted or unverified. Phases 1–3 and original invasion
synchronization validation remain incomplete.

### Absolute-height ceiling movement

Map and ACS activation now support Ceiling_MoveToValue (47), its Times8 variant
(69), and Ceiling_MoveToValueAndCrush (280). Native p_lnspec.cpp and a_ceiling.cpp
specify world-height destinations, with direction chosen from the current height.
The first two handlers multiply the signed height by the optional negative flag
(any nonzero value); 69 also scales by eight. Managed arithmetic uses a wider
intermediate and rejects values outside the integer argument range. Negative
absolute heights are accepted without relaxing distance checks on other movers.

Special 280 uses argument 3 for damage and argument 4 for crush mode. The native
thinker excludes absolute movers from slowdown handling, so mode 3 does not
reduce their speed; explicit mode 2 stops against an intermediate obstruction.
Eighteen regressions cover map/direct/stack activation, upward/downward signed
targets, sign inversion, scaling, TagWait completion, overflow and unsupported
changes, and obstructed movement in all three explicit crush modes. All 2,216
managed tests pass.

Game-specific default modes, ceiling texture changes, full native clipping,
out-of-range height policy, zero-speed movement and runtime comparison remain
open. The existing floor/ceiling height clamps still apply. These additions do
not complete phases 1–3 or validate original invasion synchronization in-game.

### Zero-speed ceiling ownership

Review of SPEED in p_lnspec.cpp, DCeiling construction and sector_t::MoveCeiling
shows zero speed is accepted by native ceiling creation. Converted managed
ceilings now accept zero down/up speeds too. A mover away from its destination
retains ownership without advancing, blocks competing ceiling movement and
keeps TagWait blocked. A zero return speed permits a crusher's down leg but
holds it at the floor afterward. At an already-equal target, zero speed still
completes the leg on its first tick. Negative speeds remain rejected.

Ten regressions cover success results across value, absolute and crusher
variants, ownership and explicit removal, zero-return-speed map activation,
and zero-speed/equal-target completion. All 2,226 managed tests pass. This
supersedes the earlier audit's zero-speed rejection limitation for converted
ceiling movers, including the earlier instant-only exception.

Already-overlapping actors can trigger native collision processing even with
zero displacement; that behavior is not implemented by the managed ceiling
tick and remains a parity gap. Negative-speed semantics, game-specific defaults,
remaining target variants and native runtime validation also remain open.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Crusher mode fallback audit

Native CRUSHTYPE in p_lnspec.cpp treats every argument outside 1..3 as a default
request. Managed dispatch previously recognized only zero for default slowdown,
so negative or greater-than-three script arguments incorrectly selected normal
speed. A shared mode helper now applies the existing non-Hexen fallback to all
out-of-range values while preserving explicit Doom, Hexen and slowdown modes.
The native speed predicates and exclusions for crush-and-stay/absolute movers
remain in place.

Seventeen new regressions cover negative and greater-than-three values across
six affected specials, all explicit modes, zero fallback, and a faster mover
whose default must not slow. All 2,243 managed tests pass. The simulation still
does not model native game-family selection: default mode in Hexen must use
Hexen crushing, independent of map format, and remains a conversion gap.
This pass fixes fallback range handling only; full clipping, remaining variants,
phases 1–3 and real-runtime invasion validation remain incomplete.

### Explicit Hexen ceiling defaults

CompatSurface.HexenCrushDefaults now selects native Hexen default ceiling
crushing and CrushStop removal behavior through AuthoritySimulation.Start's
existing compatibility argument. Map format does not select this flag. Explicit
crush modes 1..3 retain precedence; out-of-range values use the selected default.
CrushStop mode 1 always pauses, mode 2 always removes, and other values remove
when the flag is enabled. Shared dispatch applies these rules to the converted
crusher and absolute-crushing variants.

Compatibility flags now participate in the simulation checksum because they
affect future behavior. This changes checksum values, including for default
configuration; callers comparing stored checksums must use the same version and
configuration. Thirteen regressions cover opt-in/default behavior on the same
Hexen-format fixture, explicit overrides, out-of-range defaults, stop removal
versus pause, and checksum determinism/configuration distinction. All 2,256
managed tests pass.

This implements selectable ceiling defaults, not game-family discovery or full
Hexen gameplay. Server/resource integration must still select the flag from the
actual game configuration. Native collision details, remaining specials and
runtime validation remain open. Phases 1–3 and original invasion synchronization
validation are still incomplete.

### Dedicated-server compatibility configuration

DedicatedServerOptions now exposes Compatibility and the host passes it into
AuthoritySimulation.Start alongside spawn options. The command-line flag
--hexen-crush-defaults selects the new ceiling default rules without inferring
game family from map format. Repeated flags are idempotent. Usage text and the
README describe the limited scope; programmatic hosts may combine compatibility
flags without the host dropping them.

Four integration cases boot generated map data through the real dedicated-server
constructor: absent/enabled/repeated command-line flags and combined programmatic
flags. Booted configuration and deterministic reference checksums agree. All
2,260 managed tests pass, including 55 server tests and prior invasion coverage.
This closes the manual server configuration gap from the preceding audit.
Automatic resource-based selection, client negotiation/persistence of these
rules, full Hexen gameplay and native runtime comparisons remain open. Phases
1–3 and original invasion synchronization validation are still incomplete.

### Lowest ceiling target and fixed movement direction

Ceiling_LowerToLowest (253) now supports map and ACS activation, selecting the
minimum neighboring ceiling rather than the nearest lower height. It uses
argument 3 for optional crush damage and rejects unsupported change flags.
Review of a_ceiling.cpp and sector_t::MoveCeiling also found that native movers
retain their assigned direction. SectorMotion now stores CeilingDirection,
included in its simulation checksum; crusher return legs move upward and
absolute movers choose direction at creation.

This corrects the existing highest-neighbor mover when all neighbors are lower:
native upward movement clamps immediately to a destination already below it.
Likewise the new lower-to-lowest mover reaches a higher destination immediately
when every neighbor is higher. Seven regressions cover map/direct/stack calls,
fractional minimum selection, TagWait, both opposite-side target cases and
crushing versus non-crushing lowering. All 2,267 managed tests pass.

For no neighboring sector, managed lowest selection keeps the current height;
native surrounding-height helpers can return extreme sentinel values. That
edge case, opposite-direction collision side effects, linked/attached sectors,
slopes and native runtime validation remain open. Phases 1–3 and original
invasion synchronization validation are incomplete.

### Additional neighboring-height ceiling variants

Map and ACS activation now support Ceiling_LowerToHighestFloor (192),
Ceiling_RaiseToLowest (265), and Ceiling_RaiseToHighestFloor (266). Review of
p_lnspec.cpp, a_ceiling.cpp and FindHighestFloorSurrounding confirms neighboring
floor versus ceiling selection, assigned direction and argument placement.
Special 192 adds argument 4 as a gap and uses argument 3 for crush damage;
265/266 remain non-crushing. Unsupported ceiling change flags are rejected.

Fourteen regressions exercise all three through map/direct/stack activation,
fractional neighbor targets, floor-versus-ceiling selection, gap placement,
TagWait completion, opposite-side targets for raising, and change rejection.
All 2,281 managed tests pass. Existing explicit-direction motion and checksum
state are reused without adding a second mover implementation.

No-neighbor sentinel semantics, negative gap arguments, linked/attached sectors,
slopes, complete clipping and runtime parity remain open. Managed no-neighbor
floor selection falls back to the current sector floor. Phases 1–3 and original
invasion synchronization validation remain incomplete.

### Highest/floor instant-target ceiling specials

Map and ACS dispatch now implement Ceiling_ToHighestInstant (263) and
Ceiling_ToFloorInstant (267). Despite their names, native p_lnspec.cpp supplies
a fixed speed of two units per tic. The former assigns downward movement toward
the highest neighbor, and the latter upward movement toward the floor plus
argument 3. Targets opposite that direction clamp on the first tick; targets
along it travel at two units per tick. Unsupported change flags in argument 1
are rejected. Argument 2 supplies the native stored crush value.

The native upward thinker calls MoveCeiling without crush damage. Managed
ceiling ticks now omit damage on that direction too, including when the target
is below the current ceiling. Nine regressions cover map/direct/stack calls,
one-shot consumption, first-tick clamping, ordinary-direction travel, TagWait,
and a blocked floor-target move that rolls back and finishes without damage.
All 2,290 managed tests pass.

Interpolation, sound, texture/type changes, negative gaps, no-neighbor sentinel
behavior and full native collision/attached-sector handling remain incomplete.
This pass does not finish phases 1–3 or validate invasion synchronization in a
running native/managed comparison.

### Signed lowering gaps and crusher distances

Native special handlers pass signed gaps/distances directly into downward
ceiling targets. Managed validation now permits negative offsets for
Ceiling_LowerToHighestFloor (192), Ceiling_LowerToFloor (254), and distance
crusher specials 97/104/168. Neighbor-floor gaps may place the target below
that neighbor's floor; the moved ceiling remains clamped at its own sector floor.
Relative raise/lower distances and instant distances keep their existing checks.

Twelve regressions cover map/direct/stack negative floor gaps, native argument
placement, completion/TagWait, floor clamping, and lower-only versus returning
looping crushers with negative clearance. All 2,302 managed tests pass.

Negative gaps for upward Ceiling_ToFloorInstant (267) remain rejected: native
upward movement does not apply the downward floor clamp, while managed ceilings
currently clamp both directions. Floor-crossing behavior, moving-floor target
interactions, slopes, linked sectors and complete native clipping need further
conversion. Phases 1–3 and real-game invasion validation remain incomplete.

### Directional floor clamping and retained destinations

Native sector_t::MoveCeiling applies its flat-sector floor clamp on downward
movement only, using the floor at the current tick. Managed ceiling creation
now retains the requested destination rather than clamping it to the initial
floor, and only downward ticks apply the floor limit. This preserves a below-floor
destination when a concurrent floor mover later makes it reachable. Upward
thinkers may clamp directly to a target below their own floor, as native code
does; Ceiling_ToFloorInstant (267) therefore now accepts negative gaps.

Eight regressions cover direct/stack/map negative upward gaps, upward neighbor
targets below the current floor, a concurrently lowering floor, TagWait timing,
and blocked upward rollback without crush damage. All 2,310 managed tests pass.
This closes the preceding audit's unconditional floor-clamp and rejected-negative
267-gap limitations for the tested flat-sector cases.

Native compatibility flags that disable floor clamping, portal/linked-sector
rules, slopes, full clipping and renderer behavior for crossed planes remain
unconverted or unverified. The existing upper height limit remains. Phases 1–3
and original invasion synchronization validation are incomplete.

### Crusher clearance above the starting ceiling

Native CreateCeiling does not reject a crusher destination above its starting
height. With fixed direction now represented, the managed rejection is removed:
the downward leg clamps to that destination on its first tick. Lower-only
crushers finish there; return-and-stay crushers clamp back on the next tick;
looping crushers continue their leg lifecycle. This closes the previously
recorded reversed-crusher-destination creation gap for unobstructed flat sectors.

Ten regressions cover specials 42/43/45, direct/stack distance variants
97/104/168, TagWait behavior, one-shot map consumption and ownership release.
All 2,320 managed tests pass. Already-overlapping actor effects, attached-sector
collision, extreme heights, renderer behavior and real-runtime comparisons
remain outside this pass. Phases 1–3 and original invasion synchronization
validation remain incomplete.

### Floor lower-to-nearest activation

Floor_LowerToNearest (22) now supports map and ACS activation. Review of
p_lnspec.cpp and native floor creation confirms selection of the closest lower
neighbor, eighth-unit speed arguments and a non-crushing mover. Equal or higher
neighbors are excluded. Unsupported texture/type changes are rejected.
If no lower neighbor exists, this new target type still creates a no-travel
thinker, preserving success results and ownership until the first tick.

Six regressions cover map/direct/stack activation with competing fractional
neighbor heights, TagWait completion, both result opcodes for a no-travel mover,
competing floor rejection/release, and unsupported change handling. All 2,326
managed tests pass. The shared managed floor tick is reused.

Other floor types' no-travel behavior, zero/negative speeds, native floor
clipping/direction edge cases, slopes, linked sectors, texture changes and
real-runtime comparison remain open. Phases 1–3 and original invasion
synchronization validation remain incomplete.

### Eight-times-distance floor variants

Floor_RaiseByValueTimes8 (35) and Floor_LowerByValueTimes8 (36) now support map
and ACS activation. Native p_lnspec.cpp scales only argument 2's distance by
eight; speed remains eighth-units per tic. Raising uses argument 4 for crush
damage and stops at intermediate obstruction, while lowering is non-crushing.
Unsupported change flags, negative distances and overflowing multiplication
are rejected before starting a mover.

Sixteen added regressions cover map/direct/stack activation, scaled travel and
TagWait timing, rejected values/changes, positive crush damage and recovery once
an obstruction clears, and nonpositive damage arguments. All 2,342 managed
tests pass. The existing floor mover implementation handles the new dispatch.

Zero-distance ownership, zero/negative speeds, signed native distance edge
cases, texture/type changes, full clipping and real-runtime comparisons remain
open. Phases 1–3 and original invasion synchronization validation are incomplete.

### No-travel floor creation

Native floor creation allocates its thinker before target selection and does
not reject equal destinations. Converted non-lift map floor movers now likewise
succeed at an equal target, hold floor ownership until their first tick and
preserve TagWait ordering. Lift creation remains separate and unchanged.
An older regression that expected a Doom raise with no higher neighbor to fail
was corrected to assert successful one-shot consumption and first-tick release.

Ten new cases cover ACS success/ownership for value, lowest, next-higher and
times-eight floors, map consumption, and independent ceiling movement during
floor ownership. All 2,352 tests pass after correcting the old expectation.
The first full run also hit the previously observed five-second server input
timeout; its targeted rerun and subsequent full run passed without changing
or disabling the test. This remains an intermittent validation limitation.

Zero/negative floor speeds, lift no-travel lifecycle, native collision side
effects with already-overlapping actors, floor direction edge cases and real-map
parity remain open. Phases 1–3 and original invasion synchronization validation
remain incomplete.

## Conversion and audit: UDMF wall scale import (2026-10-01)

`UdmfTextMapParser` now reads the six `scalex_*` / `scaley_*` sidedef fields
for top, mid and bottom textures. `LevelBuilder.FromUdmf` applies them only in
ZDoom, ZDoomTranslated and Vavoom, following the native Zd/Zdt/Va switch in
`src/maploader/udmf.cpp`. Missing values default to one. Explicit zero is
normalized to one, matching native sidedef setters in `src/gamedata/r_defs.h`;
negative and fractional values are retained directly, without reciprocals.

Audit checked namespace gating, defaults, zero handling and independent part
axes. Six loader regression cases and one loaded-map/runtime multiplication
case were added. The latter confirms special 56 multiplies the imported mid
scale while preserving the top scale. Native uses per-part fields here;
no shared scale composition is introduced.

Release solution tests: **3,426** passed (**2,427** Playsim, **426** MapLoader).
Remaining scope includes full transform persistence/replication/checksums,
native rendered acceptance and multi-ID line tags. Wall scale import and
runtime actions are covered; this does not finish the renderer or client.

## Integration audit: texture transform checksums and namespace offsets (2026-10-01)

Review found two gaps in the converted texture paths. Only mid-wall Y offsets
participated in the managed simulation checksum; all other wall and plane
offsets, scales, mutable/base angles and base Y offsets are now hashed using
both halves of double values. A field-enumeration regression mutates each
numeric texture field independently and checks that the diagnostic hash
changes. This deliberately changes checksum baselines: the 35-tic MAP01 idle
room is now **251903926**, previously **3995474422**. Its position, health and
timing assertions still pass. Old and new managed hashes are not comparable.

Native `src/maploader/udmf.cpp` gates all per-part wall offsets on Zd/Zdt/Va.
The loader previously gated only mid Y; top/mid/bottom per-part offsets now
share that rule while common offsetx/offsety remain active. Four namespace
regressions exercise all six offsets.

After updating the intentional baseline, Release solution tests: **3,431**
passed (**2,428** Playsim, **430** MapLoader). This adds diagnostic coverage,
not texture replication or save/restore support. Native visual acceptance,
full transform persistence/replication and multi-ID line tags remain pending.

## Integration audit: independent runtime wall state (2026-10-01)

Review of map copying before persistence work found that `CopyForSimulation`
copied lines and sectors but shared mutable `LevelSide` instances. Converted
wall offset/scale actions and scrolling could mutate the source map and other
simulations started from it. Runtime copies now clone each side and its
map-load scroll list, retaining all numeric offsets/scales and texture names.

Three regression cases prove actions isolate both a second simulation and
the source map, scroll ticks do not leak, restart uses source offsets and
scroll-list edits on a copy do not modify the source. Follow-up review checked
all mutable side fields: scalar fields copy by value and the only mutable
collection receives its own list. Release solution tests: **3,434** passed
(**2,431** Playsim); Release build has zero warnings/errors and the touched
tracked file passes whitespace checks. This fixes runtime ownership, not
savegame persistence. Transform saves, replication and native rendering
acceptance remain open.

## Conversion and audit: wall transform archive (2026-10-01)

The managed HCSV archive now captures/restores all twelve wall offset/scale
fields as exact double bit patterns. Version **7** adds a wall count and
96-byte records after the existing random-state footer. Saves without wall
payload retain version **6**. Readers retain versions 1–6; those older saves
leave current wall transforms untouched. Version 7 is a managed extension,
not native savegame compatibility or complete native save conversion.

Audit checked bounds/count arithmetic, truncated records, finite values and
restore validation. Nonfinite records and payload/count mismatches are
rejected; current-map wall count is checked before mutation. Six regression
cases cover exact round-trip, version-6 behavior, every truncation boundary,
nonfinite wire/state rejection and wrong-count restore isolation. Initial
tests caught version assertions on saves with no wall payload; preserving
version 6 for those saves resolved the compatibility regression.

Release solution tests: **3,440** passed (**2,437** Playsim). Release build has
zero warnings/errors. Remaining archive scope includes plane transforms,
wall/plane scroller thinker state, actor membership and full gameplay state;
texture replication and native rendering acceptance also remain pending.

## Conversion and audit: plane transform archive (2026-10-01)

Managed HCSV version **8** adds a count and 96-byte plane transform records
after the wall section. Records preserve ten double fields (floor/ceiling
offsets, scales and base Y offsets) and four BAM angles (mutable/base).
Capture and restore include both plane and wall transforms. Readers retain
versions 1–7; versions without plane records leave current plane state
untouched. Explicit states without planes still use versions 6 or 7.

Audit checked count/size bounds before record reads, finite doubles, older
format behavior and validation before applying wall or plane mutations.
Current-map plane count must match before restore changes walls or clock.
Four new cases cover exact mixed wall/plane round-trip, version-7 behavior,
all truncation boundaries/nonfinite data and count-mismatch isolation.
Pitch compatibility tests now distinguish current version 8 from a genuine
legacy pose archive instead of relabeling an extended payload as version 5.

Release solution tests: **3,444** passed (**2,441** Playsim). This is a managed
archive extension, not native savegame compatibility. Scroller thinkers and
full gameplay saves remain incomplete; transforms alone do not reproduce
the future ticking behavior of a restored level. Replication and native
rendering acceptance remain open.

## Conversion and audit: constant texture scroller archive (2026-10-01)

Managed HCSV version **9** appends ordered constant texture scroller records:
target index, floor/ceiling or wall part-mask kind, and exact double X/Y rates.
Capture includes floor/ceiling and sidedef scroller lists. Restore replaces
those lists when supplied; an explicit empty list clears existing scrollers.
Older versions leave current lists untouched. Versions 1–8 remain readable;
explicit states without scroller payload retain older layouts.

Audit checked record lengths/count arithmetic before reads, finite rates,
valid kinds and current-map target bounds before mutation. Four regression
cases cover mixed plane/wall continuation over 20 ticks, explicit clearing,
invalid-target restore isolation, every truncation boundary and nonfinite
writer rejection. The continuation test compares all wall/plane transform
records after each tic. Release solution tests: **3,448** passed (**2,445**
Playsim); Release build has zero warnings/errors.

This covers the currently converted constant texture lists, not all native
`DScroller` modes. Carry, control-sector and accelerating scrollers, scroller
rate checksum coverage, full gameplay archives, replication and native
runtime/rendering acceptance remain open.

## Native carry audit: retained acceleration (2026-10-01)

Review before carry archive work compared control carry ticking with
`DScroller::Tick` in `src/playsim/mapthinkers/a_scroll.cpp`. Native computes
controller displacement, adds accumulated velocity, then checks for zero
motion. Managed code previously skipped zero controller displacement first,
losing movement from retained acceleration. The early skip is removed;
exact zero testing now happens after velocity accumulation. This also avoids
silently dropping small nonzero controller displacement via an epsilon.

Two regressions cover a stopped accelerating controller, reverse displacement
cancelling accumulated velocity, and a nonaccelerating controller stopping.
Release solution tests: **3,450** passed (**2,447** Playsim); Release build has
zero warnings/errors. Carry archive conversion is still pending.

The same review identified another parity gap to resolve before carry saves:
native controller height is floor-center plus ceiling-center, while managed
initialization/ticking currently uses their average. Controller scale and
map-loaded carry expectations must be reviewed together before changing this
behavior. Full carry parity is not claimed by the acceleration fix.

## Conversion and audit: controller height sum (2026-10-01)

Resolved the controller-height mismatch noted above. Native `DScroller`
initialization and ticking in `src/playsim/mapthinkers/a_scroll.cpp` use
`CenterFloor() + CenterCeiling()`. Native map setup in
`src/maploader/specials.cpp` uses line delta divided by 32 without an
additional half factor. Managed map initialization and ticks now use the
floor-plus-ceiling sum, retaining the existing native line-rate calculation.

Direct helper fixtures now supply initial sum 128 rather than average 64;
8 units of controller floor motion with rate 2 produces 16 units of carry,
instead of the old incorrect 8. Map-start and activated displacement tests
were updated together. A new regression checks ceiling-only movement,
opposite plane motion cancelling and simultaneous motion adding. Retained
acceleration/reversal coverage also passes with the corrected height unit.

Release solution tests: **3,451** passed (**2,448** Playsim); Release build
has zero warnings/errors. Audit verified initialization/tick units and both
map activation paths. Sloped sector-center sampling, carry archive state,
native runtime comparison and full gameplay saves remain incomplete.

## Conversion and audit: carry scroller archive (2026-10-01)

Managed HCSV version **10** extends scroller records to 56 bytes, retaining
constant carry rates/affect masks, accelerating base rates and accumulated
velocity, and control-sector references, last height, rates and velocity.
These state requirements follow `DScroller::Serialize` and `Tick` in
`src/playsim/mapthinkers/a_scroll.cpp`; this remains a managed format rather
than native archive compatibility. Texture and carry lists keep their order.

Capture includes all currently modeled carry lists. Restore replaces carry
lists only when the payload explicitly includes them, clears transient
displacement/buffer state, and rebuilds it on the next tic. Older version-9
texture-only saves preserve existing carry. Versions 1–9 remain readable.

Audit checked record sizes, finite rates/history/velocity, kind and affect
masks, and target/controller bounds before mutation. Six new regressions
cover ten-tic continuation for constant, accelerating, displacement and
accelerating-control carry, version-9 behavior and invalid-controller
isolation. Existing archive truncation tests also pass on the new record
size. Release solution tests: **3,457** passed (**2,454** Playsim); Release
build has zero warnings/errors.

Remaining scope includes scroller rate/history checksum coverage, complete
actor/mover/ACS gameplay saves, native runtime acceptance, replication and
unconverted native scroller modes. This establishes the modeled carry
thinkers' continuation, not full savegame parity.

## Integration audit: scroller future-state checksums (2026-10-01)

The diagnostic checksum now includes ordered constant plane/wall scrollers,
constant and accelerating carry, and control carry. Each record hashes kind,
target, X/Y rates, controller index, affect mask, last height and accumulated
X/Y velocity, with both halves of each double. This closes the gap where
identical current transforms/buffers could conceal different future motion.
State fields were reviewed against the version-10 capture schema and native
`DScroller::Serialize` / `Tick` in `src/playsim/mapthinkers/a_scroll.cpp`.

Four regressions detect rate differences while controllers are stationary,
affect differences without actors, acceleration-mode differences at rest and
five ticks of identical diagnostic checksums after a carry archive restore.
Hashing uses direct fields without allocating save records during each tic.
The no-scroller idle baseline remains unchanged; hashes for levels with
scrollers intentionally change and are not comparable to earlier builds.

Release solution tests: **3,461** passed (**2,458** Playsim); Release build
has zero warnings/errors. Full gameplay archives, replication, native runtime
acceptance and remaining native scroller modes remain open. A diagnostic
hash match is supporting evidence, not proof of native engine parity.

## Native action audit: runtime plane scrolling (2026-10-01)

Review against `LS_Scroll_Floor` / `LS_Scroll_Ceiling` in
`src/playsim/p_lnspec.cpp` found runtime floor scrolling incorrectly treating
speed low bits as controller flags and mode 3 as accelerating carry. Runtime
actions now decode X/Y speed divided by 32, enable texture scrolling only in
modes 0/2, use constant carry for every positive mode, and clear the relevant
rate otherwise. Runtime tag zero selects untagged sectors without trigger-side
fallback; missing tags return native success. The floor dispatch's manual
target check no longer blocks tag-zero ACS calls.

Updated two regressions that encoded the old runtime behavior. Three new
cases cover tag zero, low speed bits with carry-only texture clearing, mode-3
constant rate and missing-tag returns. Release solution tests: **3,464**
passed (**2,461** Playsim); Release build has zero warnings/errors.

Map-start native `src/maploader/specials.cpp` uses a different layout:
arg1 flags, arg2 floor texture/carry selection, and either line direction or
biased arg3/arg4 rates. Managed map-start initialization still conflates this
with runtime action arguments and requires its own conversion and fixture
updates. Native updates to already-existing controller/accelerating thinkers
also require review; this change does not establish every SetScroller case.

## Conversion and audit: constant ceiling map-start scrolling (2026-10-01)

Separated constant map-start ceiling setup from runtime `Scroll_Ceiling`,
following `src/maploader/specials.cpp`. Arg1 bit 4 selects line delta divided
by 32; otherwise rates use (arg3 - 128)/32 and (arg4 - 128)/32. The resulting
ceiling rate is (-dx, dy). Tag zero selects untagged sectors. Independent
map-start lines append thinkers rather than replacing rates, and their setup
special is consumed on the simulation copy, preserving the source map.

Controller bits 1/2/3 are still unconverted for ceiling textures and now
raise a specific NotSupportedException instead of starting incorrect
constant movement. This is an explicit supported-scope boundary; callers
may need to surface that startup failure in a future usable client.

Six new regression cases cover biased arguments, direction flags, duplicate
rate summation, source/runtime special isolation, tag zero and rejected
controller modes. The loaded UDMF rotation fixture now uses the correct
ceiling map-start arguments. Release solution tests: **3,470** passed
(**2,467** Playsim); Release build has zero warnings/errors.

Remaining map-start scope includes floor flag/mode conversion, ceiling
controller/accelerating texture modes and CopyScroller destinations. Existing
constant thinker archive/checksum paths retain appended ceiling thinkers;
native runtime/rendering acceptance remains pending.

### Zero-speed floor activation

Native floor creation accepts zero speed and retains a thinker. Managed
non-lift map floor creation now does the same: activation reports success,
competing floor movement is blocked, and TagWait remains pending while the
destination is unreached. A zero-speed mover already at its target completes
on its first tick. Negative speeds remain rejected, and lift speed validation
is unchanged pending its separate native lifecycle audit.

Nine regressions cover seven converted floor specials, result values, waiting
scripts, competing floor actions, independent ceiling movement and zero-speed
equal-target map consumption/release. All 2,361 managed tests pass.
Native collision side effects with no displacement, negative-speed semantics,
lift behavior, remaining floor direction/clipping rules and real-runtime
comparison remain open. Phases 1–3 and original invasion synchronization
validation remain incomplete.

### Instant floor raise/lower activation

Floor_LowerInstant (66) and Floor_RaiseInstant (67) now support map and ACS
activation. Native p_lnspec.cpp ignores argument 1 and scales argument 2 by eight;
native floor creation assigns that distance as speed. The shared managed floor
path therefore moves on its first tick, including normal ownership/TagWait
handling before that tick. Raising honors argument 4 crush damage and native
stop-on-crush behavior; lowering remains non-crushing.

Ten regressions cover map/direct/stack activation, ignored speed arguments,
first-tick travel, zero-distance ownership/release, and blocked instant raises
that roll back but complete, with and without damage. All 2,371 managed tests
pass. Unsupported change flags, negative distances and overflow use the existing
validated times-eight dispatch checks.

Native interpolation/sound, texture changes, signed distance edge cases, full
clipping, remaining floor variants and real-runtime comparison remain open.
Phases 1–3 and original invasion synchronization validation are incomplete.

### Absolute-height floor movement

Floor_MoveToValue (37) and Floor_MoveToValueTimes8 (68) now support map and ACS
activation. Native p_lnspec.cpp specifies signed world-height targets, with
argument 3 negating the height for any nonzero value; 68 additionally scales
the height by eight. A wide arithmetic intermediate prevents integer wrap, and
the new absolute target permits negative heights without relaxing relative
distance validation. Both variants remain non-crushing and reject unsupported
texture/type changes in argument 4.

Fourteen regressions cover direct/stack/map activation, upward/downward signed
targets, sign inversion, scaling, TagWait completion, ownership release,
overflow and change rejection. All 2,385 managed tests pass.
The existing floor height clamps still apply. Native fixed-direction behavior
under moving-ceiling interactions, full clipping, extreme heights, crushing
absolute variant 279, texture changes and runtime comparisons remain open.
Phases 1–3 and original invasion synchronization validation are incomplete.

### Crushing absolute floor movement

Floor_MoveToValueAndCrush (279) now supports map and ACS activation. This native
checkout passes CRUSH(arg3) - 1, so positive damage arguments are reduced by one:
8 means seven damage, and 1 enables crushing with zero damage. Nonpositive
arguments disable crushing. SectorMotion now records CrushWithoutDamage,
included in the checksum, so zero-damage crushing does not collapse into the
existing non-crushing floor representation.

Native floor CRUSHTYPE uses explicit modes 1/2 and the game default for every
other value (including 3). The HexenCrushDefaults option now also supplies that
default for this converted floor special. Ten regressions cover map/direct and
stack-result calls, damage translation, zero-damage movement, stopping versus
continuing at obstruction, Hexen defaults, ownership and TagWait completion.
All 2,395 managed tests pass. CLI help/README scope now includes converted floor
crush defaults, not just ceilings.

This preserves the checked-in native expression rather than assuming a different
damage convention. Native zero-damage collision side effects beyond movement,
full clipping, fixed floor direction and moving-ceiling target behavior remain
incomplete. Phases 1–3 and real-game invasion validation remain unfinished.

### Floor direction and retained destinations

Native floor creation assigns direction once, and MoveFloor applies the ceiling
limit to upward movement only. Converted non-lift map floors now store their
assigned FloorDirection, included in simulation checksums, and retain the
requested destination instead of clamping it to the initial ceiling. Absolute
targets choose direction at creation. Lifts, stairs and internal legacy actions
retain their existing movement paths.

Lower-to-lowest now selects the actual lowest neighbor even when it is higher
than the current floor. Its downward direction clamps to such a target on the
first tick, including an unobstructed target above the current ceiling. Four
regressions cover these two cases and relative/absolute raises whose original
target becomes reachable after a concurrent ceiling raise. All 2,399 managed
tests pass, including existing stair/lift and obstruction coverage.

Native collision handling for opposite-direction destinations, attached sectors,
floor-move compatibility flags, slopes, lower height limits and runtime rendering
of crossed planes remain open. Phases 1–3 and real-game invasion validation
remain incomplete.

### Highest neighboring floor specials

Converted Floor_RaiseToHighest (24), Floor_LowerToHighestEE (256), and
Floor_LowerToHighest (242) through shared map/ACS dispatch. Native references
are p_lnspec.cpp's wrappers and a_floor.cpp's floorLowerToHighest creation case.
Targets use the highest adjacent floor, preserve fractional heights, and retain
the explicitly assigned raise/lower direction. Special 24 uses its crush damage
argument and stops on obstruction; unsupported texture/type changes still fail.

Special 242 applies arg2 minus 128 to the selected height only when that height
differs from the current floor, or when arg3 is exactly 1. The subtraction uses
double arithmetic to avoid integer overflow on ACS arguments. Its equal-height
case still owns the floor until the first tick, and opposite-side destinations
retain native direction-based clamping for unobstructed flat sectors.

Seventeen additional test cases cover map/direct/stack dispatch, fractional
maximum selection, motion completion and TagWait, opposite-side targets, rejected
changes, and positive/nonpositive crush arguments. Three of these test cases each
exercise six adjustment/Heretic-option scenarios. The complete managed suite now
passes 2,416 tests. A restore-enabled run emitted a nested missing-SDK 8.0.423
nuget diagnostic while still passing; the final no-restore test run passed without
that diagnostic. The nested SDK pin remains unchanged.

This closes these flat-sector target/argument gaps only. No-neighbor native
sentinels, texture/type changes, attached sectors, slopes, full native actor
clipping and real-map behavior remain unverified or incomplete. The original
invasion round/timer/enemy report still requires native/game runtime validation.

### Floors targeting ceiling heights

Converted Floor_RaiseToLowestCeiling (238), Floor_LowerToLowestCeiling (258),
and Floor_RaiseToCeiling (259) through shared map/direct/stack ACS dispatch.
Reviewed p_lnspec.cpp wrappers and a_floor.cpp creation cases. Raising to the
lowest neighboring ceiling caps the initial destination at the sector's own
ceiling. Lowering selects the neighboring ceiling without that cap and retains
its assigned downward direction. The native lower variant ignores the supplied
gap; raising to the sector ceiling subtracts its signed gap. Unsupported
texture/type changes return failure before creating a mover.

Sixteen added test cases cover all three call paths and neighboring ceilings
below, above and equal to the sector ceiling; fractional destinations; ignored
versus applied gap arguments; TagWait completion and released floor ownership;
change rejection; and positive/nonpositive crushing for the own-ceiling raise.
The full Release suite passes 2,432 tests. During test development, an inaccessible
internal-array fixture setup was replaced with level initialization, and a
post-completion ownership probe was corrected to lower a floor already at its
ceiling. No production workaround was added for those test errors.

No-neighbor native sentinel behavior remains different: the managed fallback
uses the sector ceiling, whereas a native sector with boundaries but no valid
neighbors can retain FLT_MAX. Slopes, attached sectors, texture/type changes,
full native obstruction behavior, signed-gap edge regression coverage, and
runtime map comparisons remain open. This pass does not complete phases 1–3.

### Fixed-speed lowest-floor and instant ceiling-floor specials

Converted Floor_RaiseToLowest (257) and Floor_ToCeilingInstant (260) through
shared map/direct/stack ACS dispatch. Source review covered p_lnspec.cpp,
a_floor.cpp creation and dsectoreffect.cpp's MoveFloor direction branches.
Special 257 deliberately ignores arg1 and uses speed 2, with change in arg2
and crushing in arg3, as the native wrapper actually implements. Its target is
the lowest neighboring floor, even if equal to or below the current floor.

Special 260 uses arg1 for change, arg2 for crushing and arg3 for a signed gap.
Its stored direction is downward and its speed is zero. Targets at or above the
floor therefore complete on the first tick; targets below it remain active and
hold TagWait. Negative gaps can place the floor above its ceiling because native
downward movement does not apply the upward ceiling clamp. Unsupported changes
are rejected before movement starts. These peculiar semantics are preserved.

Seventeen new cases cover all three activation paths, fractional destinations,
fixed speed, ignored arguments, equal/opposite-side targets, signed gaps,
zero-speed waiting, change rejection, crushing/noncrushing obstruction and
endpoint rollback with released floor ownership. All 2,449 managed tests pass.
No-neighbor sentinels, no-displacement overlap processing, attached/sloped
geometry, complete actor clipping and texture/type changes remain open. Signed
gaps for special 260 now have coverage; the prior special 259 edge-coverage gap
is not closed by these tests. No native runtime comparison was performed, and
phases 1–3 plus real-game invasion validation remain incomplete.

### Floor raise-and-crush variants

Converted Floor_RaiseAndCrush (28) and Floor_RaiseAndCrushDoom (99) for map and
ACS direct/stack calls. Reviewed their p_lnspec.cpp wrappers, boolean CRUSHTYPE
conversion, and a_floor.cpp destination selection. Special 28 targets its own
ceiling minus eight. Special 99 first subtracts eight from the lowest neighboring
ceiling, compares that result with its own ceiling, and only then substitutes
its own ceiling minus eight when exceeded. This differs from subtracting eight
from the minimum of both ceilings.

Both wrappers use raw damage: zero enables crushing without damage, negative
values disable crushing, and positive values preserve their full damage amount.
Mode 1 keeps moving when crushing; mode 2 stops; other modes use the explicit
HexenCrushDefaults compatibility flag. Creation uses the existing plane ownership,
completion, obstruction and TagWait paths. Existing legacy Doom-format floor
special dispatch remains separate from these argument-based specials.

Eight new test cases include six route/special combinations, each with four
neighboring-ceiling scenarios, plus two damage/mode tests each exercising 48
map/script, damage, mode and compatibility combinations. All 2,457 managed tests
pass. No-neighbor native sentinels, attached/sloped geometry, complete actor
clipping, sounds, and native runtime parity remain open. Existing generic floor
endpoint tests remain passing; the new crusher matrix covers intermediate
obstruction and the destination tests cover unobstructed completion. Phases 1–3
and the original invasion runtime report remain incomplete.

### Selective floor crush stop

Converted Floor_CrushStop (46) after reviewing EV_FloorCrushStop in a_floor.cpp.
Native selection is based on the floorRaiseAndCrush mover type, not damage or
whether the mover currently crushes an actor. Added FloorCrushStopEligible to
sector motion state, set only by special 28 and included in simulation checksums.
Stopping removes eligible motions in the selected tagged sectors or manual back
sector and succeeds even when nothing matches. ACS tag zero still requires line
context, which the current ACS implementation cannot supply.

Thirteen new cases cover map/direct/stack stop calls against specials 28, 99 and
279, preserved unrelated movers, immediate floor ownership release, TagWait
resumption, no-mover success and manual back-sector selection. The complete suite
passes 2,470 tests. This new state field changes motion checksum values; runtime
negotiation/savegame compatibility remains part of the broader unfinished work.
The broader Floor_Stop (275), native sound-sequence cleanup, complete stair-lock
and attached-mover behavior remain unconverted or unverified. Phases 1–3 and
real-game invasion synchronization validation remain incomplete.

### General floor stop and interrupted stairs

Converted Floor_Stop (275) through map/direct/stack ACS dispatch after reviewing
EV_StopFloor, stair completion in a_floor.cpp and DSectorEffect::OnDestroy.
It removes active ordinary floor/lift/crusher motions in selected sectors while
preserving ceiling movers. Empty selections succeed; manual tag zero selects the
back sector, while ACS without line context rejects tag zero.

Stopping a later stair step releases its plane and TagWait but bypasses native
stairlock completion. The managed representation retains a completed record with
StairStopped, preventing normal group cleanup and stair retriggering while allowing
ordinary floor movement. Stopping the seed removes its active record; the remaining
steps can finish and unlock. StairStopped is included in checksums. This preserves
the tested interrupted-chain behavior within the existing group model; full native
per-sector stairlock/prevsec/nextsec parity and unusual overlapping chains remain
open. Stopped records are intentionally retained, not active thinkers.

Nine added cases cover three call routes across five mover variants, preservation
of a simultaneous ceiling mover, missing-tag results, manual selection, stopped
later-step TagWait/ownership and stopping a seed while the remaining step finishes.
An initial lift fixture had no lower destination; raising its initial floor fixed
the test setup. All 2,479 tests pass. Sound cleanup, attached movers/elevators,
complete persistence and real-map/native runtime comparisons remain unfinished.
This closes the prior basic Floor_Stop conversion gap, not phases 1–3 or the
original invasion runtime validation.

### Audit correction: restarted stair chains sharing a seed

The floor-stop follow-up audit found that StairGroup was derived from the seed
sector. Stopping and restarting that seed while an old follower remained reused
the same group, so old active/stopped records could prevent the new chain's
completed record from being cleaned up and block later retriggers.

Stair creation now assigns a group distinct from every retained/pending stair
chain and stores StairSeedIndex separately for floor-stop seed handling. Group
allocation rejects integer exhaustion before pending motions are committed.
StairSeedIndex is included in simulation checksums. Two regressions stop/restart
a seed while its former follower is either active or stopped, verify independent
completion and repeat activation, and verify the old active follower eventually
finishes and unlocks. All 2,481 managed tests pass.

This fixes a concrete managed lifecycle bug in the previous pass. It does not
establish full native overlapping-chain equivalence: native prevsec/nextsec links
can be rewritten, and the managed group model still differs from that mechanism.
The broader geometry, stair sector-special traversal, persistence and runtime
parity gates remain open, as does real-game invasion synchronization validation.

### Stair direction and retained destination audit

Reviewed native stair creation and DFloor::Tick reset handling. Stairs now retain
their assigned build direction in FloorDirection and reverse that direction for
the return leg. Previously the managed mover approached its target dynamically,
which moved an already-past-target step slowly instead of applying the native
first-tick clamp. Ceiling limits now follow the effective direction, including
reset, and the original requested target is retained instead of clamping it to
the ceiling at creation. Existing lower representable limits remain unchanged.

Five new cases cover ordinary up/down stairs whose middle step starts beyond
its target, with and without reset, plus a concurrent ceiling raise that makes
the original destination reachable. Existing synchronized, obstruction, pause,
stop and chain regressions still pass. Negative calculated synchronized speeds
remain rejected pending their own conversion; this pass does not close that gap.
Sector-special stair traversal, attached/sloped geometry and full native clipping
remain incomplete.

Final full suite: 2,486 passed. One preceding run reproduced the documented
five-second Pump_LiveSessionReceivesGuestClientInput timeout; the isolated rerun
and subsequent complete run passed. No test timeout or disabling workaround was
added. This intermittent failure remains an audit finding, not a resolved issue.
Phases 1–3 and real-game invasion synchronization remain incomplete.

### Reliable setup retries after Ready

Investigated the recurring Pump_LiveSessionReceivesGuestClientInput timeout.
Twenty isolated repetitions and twenty server-suite repetitions passed before
changes, so its exact trigger was not reproduced. Code review found a separate
concrete reliability gap: PregameHost only periodically drove Connecting/Waiting
clients, leaving a Ready client's pending StartGame without a timer-driven retry.
It depended on further inbound traffic or another explicit StartGame call.

PregameHost.Pump now flushes pending reliable traffic for Ready clients using the
existing resend interval. FlushClient also avoids attempting a send when the queue
reports pending-but-not-due with a zero packet length. A regression deliberately
consumes the first StartGame datagram without acknowledgement, verifies no early
retry, checks retransmission at the deadline without inbound traffic, and sends
a real acknowledgement before asserting live readiness and queue cleanup. It
failed against the old implementation (send count stayed at one), then passed
with the fix. No timeout increase or test disabling was used.

All 2,487 managed tests pass. This closes the demonstrated Ready-state retry gap;
it does not prove the intermittent server-test timeout resolved, nor establish
complete loss/reorder/handoff or native protocol parity. Phases 1–3, snapshot
release gates, and real-game invasion round/timer/enemy validation remain open.

### Delayed connection acknowledgement audit

Found a guest handshake regression: HandleConnectAck unconditionally replaced the
session token and reset Phase to WaitingForAssignment, even after reliable setup
had advanced to Starting. Host connection acknowledgements are independently
retransmitted, so a late copy can regress an established guest. A differing token
also invalidated subsequent traffic without resetting existing reliable state.

ConnectAck now adopts a nonzero token only while no session token has been adopted.
An established guest ignores subsequent ConnectAck packets. Two controlled UDP
regressions cover matching and differing stale tokens after Starting, a dropped
first StartGameAck, a duplicate StartGame, unchanged session/phase, and retransmitted
StartGameAck with the original token. The initial replay tests failed against the
old implementation with WaitingForAssignment instead of Starting. Final tests
wait for observed duplicate processing rather than using an arbitrary settling
sleep. The complete suite passes 2,489 tests.

This fixes the demonstrated replay/state-regression path, not a general reconnect
protocol; a fresh connection needs fresh guest state. Full endpoint validation,
reconnect/session replacement, loss/reorder/live-handoff and native parity audits
remain open. The intermittent server-test timeout has not been conclusively tied
to this defect. Phases 1–3 and real-game invasion validation remain incomplete.

### Connection replay and setup endpoint validation

Found two additional handshake defects. Repeated valid Connect packets could
allocate multiple slots for one endpoint (or receive Full when replaying an
existing session), and PregameGuest accepted decoded setup traffic regardless of
source endpoint. The host now recognizes existing endpoints after credential/
engine validation and before capacity checks, resending their ConnectAck without
resetting token, status or slot. The guest filters setup input against the configured
server endpoint before processing acknowledgements, rejections or services.

Eight added cases cover duplicate Connect in Connecting/Waiting/Ready states
with capacity available or exhausted, plus non-server ConnectAck and token-matching
StartGame packets. The pre-fix run exposed duplicate allocations, missing replay
acknowledgements and foreign packet state changes (seven cases failed); the final
full suite passes 2,497 tests. Tests use controlled UDP endpoints and observe
protocol responses rather than extending existing handshake timeouts.

This closes basic setup endpoint filtering and duplicate admission gaps, not
cryptographic authentication or a reconnect protocol. Credential revalidation,
session expiration/replacement, initial Connect retry, full live handoff and
native interoperability need further audit. The intermittent server-test timeout
is still not conclusively diagnosed. Phases 1–3 and real-game invasion validation
remain incomplete.

### Initial Connect packet-loss recovery

PregameGuest previously sent Connect once and stayed in SentConnect forever if
that datagram was lost. It now records the successful send time and retries only
while SentConnect, using RuntimeConnectAckResendMilliseconds. Incoming traffic is
processed before considering a retry, and a backwards timestamp cannot underflow
the elapsed-time check. Timestamp zero is supported without a sentinel collision.
Adopted sessions and rejected connections do not keep retrying.

Three new cases drop the first request with clocks starting at zero and 1000,
verify no early resend, observe a request at the deadline, verify silence after
session adoption, and check rejection suppresses retries. The two loss cases failed
before the fix. Final full suite: 2,500 passed. Existing duplicate-admission and
stale-ack regressions also pass. This closes the initial Connect retry gap noted
in the preceding audit; reconnect/session expiration, retry deadlines/cancellation,
live handoff, native interoperability and the intermittent server-test timeout's
exact cause remain open. Phases 1–3 and real-game invasion validation are incomplete.

### Signed synchronized stair speeds

Reviewed the native a_floor.cpp stairSync calculation and MoveFloor's assigned
movement directions. Removed the managed rejection of a negative calculated
follower speed. Native stores speed times signed rise divided by signed stair
step; the sign is retained separately from build/reset direction. The previously
converted direction-aware mover now evaluates these targets without rejecting
the seed or other steps.

Replaced two rejection tests with four up/down build/reset cases, and added two
high-speed cases where the signed step moves beyond its destination before reset.
These verify raw signed movement, reset to original planes and released chain
locks. Net test increase is four; all 2,504 tests pass. The historical negative
calculated-sync-speed rejection gap is closed for the tested flat, unobstructed
steps. Explicit negative base speed/step inputs, native sector-special traversal,
negative-speed actor clipping, advanced geometry and full runtime comparisons
remain open. Phases 1–3 and real-game invasion validation remain incomplete.

### Sector-marked Hexen stair traversal

Converted Stairs_BuildDown/Up (26/27) and their synchronized variants (31/32).
Reviewed p_lnspec.cpp wrappers, EV_BuildStairs and P_NextSpecialSectorVC plus
Stairs_Special1/2 definitions (26/27). These variants alternate the required
sector marker, traverse either line orientation in level order, and ignore floor
texture changes. Busy marked steps are visited and consume a height increment
without creating a new mover; traversal continues beyond them. Marked followers
do not inherit the Doom speed-four crush behavior. Existing Doom stair variants
retain their texture/directional traversal.

Fourteen tests cover four specials through map/direct/stack ACS routes, differing
textures, reversed lines, rejected marker branches, TagWait completion, sync timing,
and traversal through busy steps. All 2,518 tests pass. This closes the basic
sector-special traversal gap for flat sectors. Native mutable marker changes,
visited/link behavior in complex cyclic/overlapping chains, complete marker-stair
obstruction/reset combinations, advanced geometry and real-map runtime comparison
remain open. Existing generic stair reset/obstruction tests remain passing but do
not constitute exhaustive coverage of these newly enabled variants. Phases 1–3
and real-game invasion validation remain incomplete.

### Marker-stair reset and obstruction audit

Added six regression cases for the newly enabled marker variants, with no
production-code changes needed. Four cases exercise up/down ordinary/synchronized
chains: reset begins at the native argument position, seed plane ownership is
retained while waiting, TagWait on the final step stays blocked through return,
and the completed chain can be triggered again. Synchronized tests supply a
different unused arg4 to detect accidental reset-argument substitution.

Two follower-obstruction cases contrast marker stairs with existing Doom stairs
at speed four. The marker follower stays blocked without damage, preserves its
chain lock, then completes after the actor stops blocking. Existing Doom exact-
arrival/overshoot cases still pass. Reviewed the p_lnspec.cpp reset argument
wrappers and a_floor.cpp stairUseSpecials-dependent crush assignment.

All 2,524 tests pass. These cases narrow the prior reset/obstruction coverage gap;
negative-speed clipping, delayed/blocked-reset combinations, complex cycles and
links, advanced geometry and native runtime comparisons remain open. This is a
source/regression self-audit, not proof that the gameplay phases are complete.
Real-game invasion round/timer/enemy validation also remains outstanding.

### Generic stairs and repeat direction

Converted Generic_Stairs (204), reviewed against LS_Generic_Stairs. Arg3 bit zero
selects up/down, bit one ignores texture differences, and arg4 supplies reset.
Successful activation of a repeatable map line flips only the direction bit;
busy/rejected activation leaves it unchanged. ACS calls without a line cannot
mutate a map line. The existing Doom traversal and mover behavior are reused.
LevelLine.Arg3 is now mutable for this native behavior, and line Special/Arg3
values participate in the simulation checksum so next-activation state is visible.
This changes checksum values; complete line-state save/replication remains open.

Fourteen cases cover all direction/texture combinations via map/direct/stack
calls, repeat success/failure toggling, preserved texture bits, and checksum
sensitivity. One test initially allowed only eight ticks for a twelve-unit return
path; correcting its expected duration resolved that test failure. All 2,538
managed tests pass. Generic reset-specific interactions, malformed/negative
arguments, native compatibility flags, complex traversal, shared mutable level
ownership and full map/runtime parity remain open. Phases 1–3 and real-game
invasion validation remain incomplete.

### Simulation ownership of mutable lines

The generic-stair follow-up audit confirmed that AuthoritySimulation.Start reused
the supplied PlayLevel and its mutable LevelLine objects. Consuming a special or
toggling Arg3 could therefore leak into another simulation or a later restart
from the same map template. Start now creates a PlayLevel copy with independent
line objects and a private line list before spawning actors. Actors reference
that owned level. Other map data is shared as before; this is deliberately not a
full deep clone of mutable nested resources or thing argument arrays.

Two added cases exercise one-shot and repeatable generic stairs across two
simulations plus a fresh restart, verifying independent Special/Arg3 values,
unchanged template, actor level references and matching final checksums. Updated
the existing Doom exit test to inspect the owned line and assert the template
remains unchanged. Preserving the copied collection as a List retains existing
runtime collision-test insertion behavior. All 2,540 tests pass.

Callers inspecting runtime line changes must now use simulation.Level.Lines.
Direct helper calls supplied with external lines still operate on those explicitly
supplied objects; automatic use/cross activation uses owned lines. Full runtime
level ownership, line-state persistence/replication and native runtime validation
remain unfinished. Phases 1–3 and the original invasion runtime report remain open.

### Raise-and-change floor simulation foundation

Added partial simulation support for Floor_RaiseByValueTxTy (239), following
p_lnspec.cpp and a_floor.cpp creation timing. Successful map calls immediately
copy front-sector FloorPic and Special, then raise by the requested value. ACS
without a trigger clears Special and retains FloorPic. Busy floors do not mutate
metadata; invalid model references reject. This is not complete native TxTy parity:
TransferSpecial also copies damage/type/interval/leakiness and special flags,
which the managed sector model does not yet represent.

PlayLevel.CopyForSimulation now independently copies sectors as well as lines;
FloorPic and Special are mutable runtime metadata. Both participate in simulation
checksums. Existing stair texture/marker selection reads that owned metadata.
The server sector snapshot already reads Special, but FloorPic has no client
replication/rendering path in the current protocol. Texture and sector-effect
persistence, native damage-property transfer and derived sector effects remain
release blockers for this feature; do not treat it as a completed visual conversion.

Four tests cover immediate map changes, busy rejection, template isolation and
ACS direct/stack clearing with TagWait completion. All 2,544 tests pass. Other map
collections remain shared; this is not a complete deep-copy or savegame solution.
Phases 1–3 and real-game invasion validation remain incomplete.

### Sector damage metadata loading and transfer

Compared UDMF loading in `src/maploader/udmf.cpp`, `TransferSpecial` in
`src/playsim/p_sectors.cpp`, and `ClearSpecial` in `src/gamedata/r_defs.h`.
Managed sectors now retain damage amount, type, interval and leakiness. UDMF
level construction preserves signed damage amounts, defaults intervals to 32,
narrows interval/leakiness to the native signed 16-bit fields, clamps intervals
to at least one after narrowing, and clears related properties when damage is
zero. Raw parsed values remain available in the UDMF document model.

Floor special 239 transfers these properties immediately with its model sector,
or clears them when invoked without a trigger. Busy sectors remain unchanged,
simulation copies preserve the source template, and all four fields participate
in simulation checksums. Native field-width review caught an initial omission;
boundary regressions now cover wraparound before normalization.

Ten new map-loading cases and four checksum cases pass; existing map/ACS floor
tests now also verify damage transfer, clearing and isolation. Full Release
suite: 2,558 passed; warnings-as-errors build: zero warnings/errors.

This supersedes only the four missing metadata fields in the preceding audit.
Sector damage is not yet applied to actors. Native special/damage flags, binary
sector-special damage initialization, namespace-specific UDMF gating, client
texture/damage replication and persistence remain incomplete. Real-game invasion
validation and phases 1–3 remain open. This is a source/regression self-audit.

### Ordinary player sector damage and healing

Reviewed `P_ActorInSpecialSector` in `src/playsim/p_spec.cpp` and its actor-tick
call in `src/playsim/p_mobj.cpp`. The managed actor tick now applies ordinary
flat-sector damage to living players after movement, using the shared clock
before its increment. Contact is checked against the current floor, and interval
checks include tic zero. Invalid runtime intervals repair to 32. Positive damage
uses existing armor, invulnerability, pain and death handling without an attacker;
negative damage heals up to 100 without reducing overhealth or resurrecting.
Healing arithmetic widens before negation to avoid integer-minimum overflow.

Sixteen regressions cover cadence, landing without resetting the shared clock,
armor, invulnerability, airborne exclusion, healing limits, death, invalid
interval repair and ACS special 239 clearing active damage. The first version of
the clearing fixture called a nonexistent overload; it was corrected to execute
actual ACS bytecode. Final full Release suite: 2,574 passed. Build with warnings
as errors: zero warnings/errors.

This closes only ordinary flat-floor player damage/healing application. Damage
types currently use generic damage; type-specific resistance/death, radiation
suits/leakiness, hazard/exit/terrain flags, monster opt-in, liquid/3D-floor contact,
native binary damage-special initialization, full tick-order parity, replication
and persistence remain open. Existing managed invasion regressions pass, but the
original real-game round/timer/enemy report still needs native runtime validation.
Phases 1–3 remain incomplete; this is a source/regression self-audit.

### Sector opt-in for non-player and airborne damage

Reviewed `checkForSpecialSector` in `src/playsim/p_spec.h`, contact checks in
`P_ActorInSpecialSector`, UDMF `hurtmonsters`/`harminair` loading, and native
`TransferSpecial`/`ClearSpecial`. Managed UDMF levels now retain these two flags,
including sectors with zero initial damage. They are copied into simulation
sectors and included in checksums. These native MoreFlags are deliberately not
transferred or cleared by floor special 239; destination contact rules remain.

The damage tick permits non-player actors when HurtMonsters is enabled and
bypasses flat-floor contact when HarmInAir is enabled. Existing damage eligibility
and lifecycle handling still apply. Healing now caps non-player actors at their
recorded spawn health, matching the ordinary native monster path; healing amount
is limited to 65,536 using widened arithmetic. Player healing retains its 100 cap.

Four loading tests and eighteen simulation tests cover flag combinations,
airborne players/monsters, default non-player exclusion, monster healing/death,
destination flag preservation during damage transfer and checksum differences.
All 2,596 managed tests pass; warnings-as-errors build has zero warnings/errors.

Actor-level NOSECTORDAMAGE/FORCESECTORDAMAGE flags, damage-type effects, protection,
hazard/exit/terrain flags, liquids/3D floors, skill-adjusted/custom player health,
binary damage-special initialization, namespace gating, replication/persistence
and full native tick-order parity remain open. Dynamically supplied non-player
actors also require their spawn-health metadata to be populated for healing.
This is a source/regression self-audit, not native runtime certification.
Phases 1–3 and the real-game invasion report remain incomplete.

### Actor sector-damage override precedence

Compared the managed eligibility path with `checkForSpecialSector` in
`src/playsim/p_spec.h`. Actors now expose NoSectorDamage and ForceSectorDamage
runtime flags. Force wins over actor immunity and sector monster opt-in, while
floor contact, shared interval timing and normal damage protection still apply.
The same eligibility controls healing. Both flags participate in checksums.

Twenty-two regression cases cover the full player/monster, sector opt-in,
immunity and force matrix, healing, airborne contact, invulnerability, enabling
force between interval boundaries and checksum differences without health changes.
Full Release suite: 2,618 passed; warnings-as-errors build: zero warnings/errors.

This converts runtime eligibility only. Loading these flags from actor definitions
or scripts, replication and complete saves remain unfinished; ordinary existing
spawns retain false defaults. The previous damage-type, protection, hazard/exit,
geometry, skill and native-runtime gaps remain open. Phases 1–3 and the original
real-game invasion report are not complete. This is a source/regression self-audit.

### Sector_SetDamage map and ACS dispatch

Converted special 214 through shared map and ACS direct/stack dispatch, comparing
`LS_Sector_SetDamage` and `MODtoDamageType` in `src/playsim/p_lnspec.cpp` and tag
iteration in `src/playsim/p_tags.cpp`. Matching sectors receive amount, damage
type, interval and leakiness; tag zero selects untagged sectors. Missing tags
still return success. Nonpositive intervals select native legacy defaults using
the original amount before signed 16-bit field conversion. Explicit intervals
and leakiness narrow without extra clamping; runtime contact repairs invalid
intervals as before. Special numbers and contact flags are preserved.

Twelve new tests cover map consumption, both ACS argument routes, multiple tags,
live damage and disabling it, default thresholds, negative amounts, field
overflow, interval repair, unknown damage types, zero tags and missing targets.
Initial gameplay assertions exposed absent sector boundaries in the test fixture;
adding actual room geometry fixed actor placement. Final full Release suite:
2,630 passed; warnings-as-errors build: zero warnings/errors.

Damage types remain metadata with generic gameplay damage, and leakiness still
awaits radiation-suit support. Full tag-manager/multiple-tag representation,
advanced actor definitions, protection/hazard effects, replication/persistence
and native runtime parity remain open. Phases 1–3 and real-game invasion
validation remain incomplete. This is a source/regression self-audit.

### Ordinary damage-special initialization

Compared `SetupSectorDamage` and `InitSectorSpecial` in
`src/maploader/specials.cpp`. Simulation startup now initializes ordinary Doom
binary specials 4/5/7/16 and extended specials 68/69/71/80/85/104/196 with their
damage/healing amounts, 32-tic interval, leakiness and damage type. Existing
nonzero explicit damage metadata takes precedence. Recognized specials are
consumed after lighting initialization, on simulation-owned sectors only.

Sixteen new cases cover numeric namespaces, amounts, first-tic gameplay,
explicit positive/negative damage precedence, source-template isolation and
unconverted special preservation. Full Release suite: 2,646 passed;
warnings-as-errors build: zero warnings/errors. Existing lighting and invasion
regressions also pass.

This converts ordinary low-byte damage initialization only. Specials carrying
high-bit flags are currently left untouched by this initializer. Boom/MBF21
damage/death flags, translated UDMF namespaces, exit/hazard/lava behavior and
Strife special 104's lighting side remain unfinished. Damage types and radiation
suit leakiness still lack their specialized gameplay effects. Full native map
parity, phases 1–3 and real-game invasion validation remain incomplete. This is a
source/regression self-audit, not native runtime certification.

### Combined strobe and damage regression audit

Compared `SpawnLights` in `src/maploader/specials.cpp`. Extended special 104
now starts the same five-bright/fifteen-dark strobe as native Strife, before
damage initialization consumes the special. This closes the missing lighting
side explicitly identified in the preceding pass.

Seven new cases verify the bounded initial strobe delay, native dark duration,
independent light/damage cadence over 65 ticks for Doom 4 and extended 68/104,
map-template isolation, ACS disabling damage while the light keeps running,
and stopping the light while damage continues. All 2,653 managed tests pass;
warnings-as-errors build: zero warnings/errors. No native runtime certification
is implied; exact native lighting RNG, remaining sector flags, specialized damage
effects, phases 1–3 completion and real-game invasion validation remain open.

### Boom damage-bit initialization and precedence

Reviewed the Doom sector bit translation in `wadsrc/static/xlat/doom.txt` and
damage setup in `src/maploader/specials.cpp`. Startup now interprets Doom binary
damage bits 0x20/0x40/0x60 and extended bits 0x100/0x200/0x300 before ordinary
low-number specials. Explicit nonzero damage wins; otherwise the flag damage wins
over the low special, with native amounts, type, interval and leakiness. Consuming
a recognized low special preserves the remaining raw flags, since full secret,
friction and related flag conversion is still incomplete.

Eleven new cases cover each format/amount, first-tic damage, precedence, unrelated
flag preservation and compatibility gating. Corrected an older test that treated
Doom special 69 as unconverted: it actually combines damage bits with special 5.
The MBF21 death-bit branch remains deliberately unimplemented and is not treated
as ordinary damage when that compatibility mode is enabled. All 2,664 managed
tests pass; warnings-as-errors build: zero warnings/errors.

This supersedes the preceding ordinary-initialization high-bit exclusion only
for supported damage combinations. Translated UDMF namespaces, full native flag
consumption/transfer, MBF21 death/exit behavior, protection, specialized damage
effects, phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not exhaustive native parity validation.

### Translated UDMF sector numbering

Reviewed namespace selection and sector translation in `src/maploader/udmf.cpp`,
`TranslateSectorSpecial` in `src/gamedata/p_xlat.cpp`, and the Doom/base translator
files. Lighting and damage initialization now share a map-level predicate for
Doom binary and UDMF Doom/ZDoomTranslated sector numbers. ZDoom retains extended
numbering. Doom UDMF suppresses fire-flicker and phased/sequence extensions that
`doom_base.txt` disables; ZDoomTranslated keeps those extensions.

Fifteen parser-to-simulation tests cover namespace case, ordinary/Boom damage,
combined lighting/damage and disabled versus enabled lighting extensions. Initial
fixtures omitted player spawn flags; the tests were corrected with explicit
skill/mode flags. Final full Release suite: 2,679 passed; warnings-as-errors build:
zero warnings/errors.

This closes namespace selection for the currently converted sector effects only.
Custom translators, other game namespaces, complete sector translation/flag
consumption, advanced damage effects, phases 1–3 completion and real-game invasion
validation remain open. This is a source/regression self-audit.

### Damaging exit-floor foundation

Compared native damage-end setup and `P_ActorInSpecialSector`. Doom sector 11
and extended 75 now initialize 20 damage every 32 tics, leakiness 256 and the
end-god-mode/end-level flags when no explicit damage takes precedence. A separate
player GodMode property protects against ordinary damage; floor contact removes
it before the interval check without removing actor invulnerability. On damage
ticks, player health at or below 10 triggers the normal exit. These flags transfer
and clear with floor special 239 and participate in checksums, as does GodMode.

Seven new cases cover exit timing/threshold, explicit damage precedence, airborne
exclusion, god mode versus invulnerability and explicit damage bypass. Existing
floor-transfer/ACS-clearing tests now assert both flags. Full Release suite:
2,686 passed; warnings-as-errors build: zero warnings/errors.

This is the ordinary managed exit-floor foundation. Deathmatch no-exit rules,
additional cheat modes, radiation-suit interactions, cheat command integration,
complete persistence/replication and exact native damage bypass semantics remain
unfinished. Phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not native runtime certification.

### Deathmatch damaging-floor exit restriction

Converted the `!deathmatch || !(dmflags & DF_NO_EXIT)` condition from native
`P_ActorInSpecialSector`. Simulation startup accepts a no-exit setting, combines
it with the spawn game mode, and records the resulting DamageExitAllowed policy
in the checksum. DedicatedServerOptions.NoExit is passed through server boot.
Blocked exits still apply damage, remove ordinary god mode and can kill players;
single-player/cooperative modes ignore this restriction.

Seven simulation cases cover the complete mode/setting matrix, continuing lethal
damage and checksum differences before gameplay. Four server cases verify policy
propagation. Full Release suite: 2,697 passed; warnings-as-errors build: zero
warnings/errors. This supersedes the damaging-floor restriction gap only.

NoExit is currently an API/server-options setting; command-line controls,
network rule negotiation, live changes, other exit mechanisms and full save
restoration remain unfinished. The existing native-runtime, specialized damage
and phase 1–3 gaps remain open, including real-game invasion validation. This is
a source/regression self-audit.

### Ordinary exit-line deathmatch restriction

Reviewed `CheckIfExitIsGood`, native normal/secret exit specials and the ordinary
telefrag-damage protection rules. Converted Doom use/cross exits and extended
243/244 now consult the existing deathmatch exit policy. Forbidden actor-triggered
exits apply 1,000,000 self-attributed damage through the shared damage boundary,
bypass ordinary armor/invulnerability, return failure and preserve the line.
World-triggered internal exits still succeed without an activator. The internal
normal/ID24 exit entry points use the same check.

Seven new tests cover six normal/secret map routes with restriction enabled and
disabled, player death/source attribution, armor preservation, failed-line
retention, repeat behavior and world-exit exemption. Full Release suite: 2,704
passed; warnings-as-errors build: zero warnings/errors.

Remaining gaps include always-apply-deathmatch-flags, kill-all-monsters/hub gates,
ACS activator propagation and complete exit dispatch, special telefrag protection
flags/cheat variants, exit position/MAPINFO transitions, command-line rule control
and networking/persistence. Phases 1–3 and real-game invasion validation remain
incomplete. This is a source/regression self-audit.

### ACS exit dispatch and activator foundation

ACS fibers now retain an optional starting actor. Map ACS starts, nested Execute
and ExecuteAlways propagate it; resuming preserves the original actor, matching
native script start/resume code in `src/playsim/p_acs.cpp`. Direct and stack normal
and secret exit specials (243/244) use explicit shared exit dispatch, avoiding the
legacy internal special-number fallback. Actor-triggered exits obey no-exit;
world or destroyed-activator exits use the world exemption. Active actor identity
participates in the ACS checksum. Legacy one-argument fallback receives the actor.

Thirteen new tests cover both argument routes, normal/secret exits, world
exemption, actor attribution through map/nested execution, resume/destroyed actor
behavior and checksum identity. Full Release suite: 2,717 passed; warnings-as-errors
build: zero warnings/errors.

This is an activator/exit foundation, not complete ACS context conversion. Trigger
line/side context, SetActivator and actor queries, arbitrary actor lifecycle/save
restoration, client scripts, exit position/hub progression and remaining native
exit rules still need work. Phases 1–3 and real-game invasion validation remain
incomplete. This is a source/regression self-audit.

### ACS trigger-line propagation

Compared native DLevelScript activationline capture and resume behavior. Managed
fibers now retain the optional triggering line through map starts, nested normal
and always execution, and suspension. Converted door/floor/stair/ceiling dispatch
receives that line for manual targeting and model selection. The legacy fallback
also receives it. Line identity and action-relevant fields participate in the ACS
checksum; a resumed script retains its original trigger.

Six new regressions verify direct/stack tag-zero raise-and-change, nested execution,
front-sector texture/damage transfer, back-sector motion, preservation after
one-shot trigger consumption, resume context and checksum distinctions. Existing
door/stair/ceiling regressions pass. Full Release suite: 2,723 passed;
warnings-as-errors build: zero warnings/errors.

Back-side activation context, complete native manual activation rules, arbitrary
line lifecycle/save restoration, other ACS actor/context operations and full
native parity remain unfinished. Phases 1–3 and real-game invasion validation
remain open. This is a source/regression self-audit.

### ACS LineSide and captured activation side

Converted native PCD_LINESIDE (80), which pushes the script's stored backSide.
Fibers now retain a side value through starts, nested execution and suspension;
the value participates in checksums. Map ACS use captures the actor's side, while
crossing captures its previous position so movement cannot invert the reported
origin side. Explicit callers may supply the side; world scripts default to front.

Seven regressions cover both use/crossing directions, nested/resumed context and
checksum distinctions. Full Release suite: 2,730 passed; warnings-as-errors build:
zero warnings/errors. This closes the stored-side query foundation only: exact
on-line/degenerate geometry semantics, full side-dependent special restrictions,
legacy unknown-format activation, persistence and remaining ACS context operations
still need work. Phases 1–3 and real-game invasion validation remain incomplete.
This is a source/regression self-audit.

### ACS activator identity queries

Compared native PCD_PLAYERNUMBER (247) and PCD_ACTIVATORTID (248). The managed VM
now pushes the activator's visible PlayerNum or -1 for a non-player/world context,
and its map ThingId or zero for world context. Destroyed actors use world defaults;
dead but existing actors retain identity. Simulation checksums now include ThingId
and player slot identity because these values are script-observable.

Thirteen regressions cover player slots, monster/world contexts, zero/nonzero
TIDs, destroyed versus dead actors and TID checksum differences. Full Release
suite: 2,743 passed; warnings-as-errors build: zero warnings/errors. Native reserved
slot/roster integration, dynamic TID changes, SetActivator, complete actor query
support and persistence remain unfinished. Phases 1–3 and real-game invasion
validation remain open. This is a source/regression self-audit.

### ACS actor position queries

Converted GetActorX/Y/Z (196/197/198) after reviewing native ACS dispatch,
SingleActorFromTID and head insertion into the native TID hash. TID zero resolves
the retained activator; nonzero TIDs resolve the newest remaining matching actor
in the managed spawn list. Missing/destroyed actors return zero; coordinates keep
their signed 16.16 representation. Empty stacks terminate safely. Both bytecode
walker formats now recognize X/Y as zero-immediate-operand instructions.

Ten new cases cover fractional axes, duplicate TID order, destroyed actor removal,
world/missing lookups and underflow. Two prior opcode-alias regressions were updated
to expect real coordinate queries instead of unsupported-instruction termination.
Full Release suite: 2,753 passed; warnings-as-errors build: zero warnings/errors.

Z bob offsets, native out-of-range double conversion, dynamic TID rehashing,
client-side actor namespaces and complete actor query/state parity remain open.
Phases 1–3 and real-game invasion validation remain incomplete. This is a
source/regression self-audit.

### ACS flat-sector plane and angle queries

Converted GetActorFloorZ (259), GetActorAngle (260) and GetActorCeilingZ (282)
using the existing activator/TID lookup. Reviewed the native query cases and
AngleToACS conversion. The managed flat-sector foundation reads current sector
planes in signed 16.16 format and returns the upper 16 bits of the actor's BAM
angle as ACS turns. Missing actors return zero; underflow stops the script.

Fifteen new cases cover fractional floor/ceiling values, completed floor movement,
cardinal/fractional/wrapped angles, missing actors and underflow. Full Release
suite: 2,768 passed; warnings-as-errors build: zero warnings/errors.

Native actor floorz/ceilingz can differ from the containing sector's planes with
multi-sector contact, slopes, portals and 3D floors; those cases remain unfinished.
Native angle double-precision edge behavior, complete actor queries, phases 1–3
and real-game invasion validation remain open. This is a source/regression
self-audit, not a claim of full native parity.

### ACS actor and sector light queries

Native source audit found GetActorLightLevel mislabeled as 342 in the managed
enum; corrected it to 340 and added recognition in both bytecode walker formats.
Implemented actor light lookup through the existing activator/TID path and
GetSectorLightLevel (281) through the first matching sector tag. Both read current
simulation lighting. Missing actors return zero; missing sector tags return -1.

Ten runtime regressions cover actor/tag lookup, animated lights, first-sector and
zero-tag behavior, defaults and underflow. Two decoder tests lock down the native
opcode and wordcode/compact instruction boundaries. Full Release suite: 2,780
passed; warnings-as-errors build: zero warnings/errors.

Actor queries currently use flat-sector lighting; native 3D-floor light lists,
dynamic actor namespaces and complete light/resource parity remain unfinished.
Phases 1–3 and real-game invasion validation remain open. This is a source and
regression self-audit.

### ACS SetActorAngle

Converted native PCD_SETACTORANGLE (276) after reviewing its helper in
`src/playsim/p_acs.cpp`. TID zero changes only the retained activator; nonzero
TIDs change every matching non-destroyed actor, including dead actors. ACS turns
wrap into BAM angle storage. Missing targets consume arguments and continue;
underflow stops without mutation. Existing angle queries observe the new value.

Twelve regressions cover signed/overflow angle wrapping, activator targeting,
multiple matches, dead/destroyed actors, missing targets, stack preservation and
query integration. Full Release suite: 2,792 passed; warnings-as-errors build:
zero warnings/errors. Interpolated function variants, client view interpolation,
actor pitch control and broader native state/replication parity remain unfinished.
Phases 1–3 and real-game invasion validation remain open. This is a source and
regression self-audit.

### ACS actor pitch foundation

Native opcode review corrected GetActorPitch/SetActorPitch from 333/334 to
331/332. Added both operations, sharing activator/TID targeting with angle queries
and setters. Pitch state now belongs to all actors and participates in checksums;
player input still uses the existing +/-89 degree managed clamp. ACS setters
normalize signed turns for non-player actors and apply the player clamp. Setters
consume two arguments; queries replace the TID argument and return zero if absent.

Eleven regressions cover signed/wrapped monster pitch, player clamping, multi-actor
TID setting without changing yaw, missing targets and stack underflow. The older
unsupported-opcode test now targets native PrintBind (333), since 332 is implemented.
Full Release suite: 2,803 passed; warnings-as-errors build: zero warnings/errors.

Custom native player pitch limits, interpolation, non-player pitch persistence and
replication, rendering/attack integration and native precision edge cases remain
unfinished. Phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not complete native parity validation.

### Actor pitch pose-archive integration

The pitch audit found that CaptureState discarded non-player pitch and restore
applied the player limit to every actor. Pose archives now write version 6 and
retain all actors' pitch in the existing fixed-point field. Versions 1–5 remain
readable. Restore validates player pitch against +/-89 degrees and non-player
pitch against +/-180 before mutating state, then restores pitch for every actor.
The version bump prevents older readers from silently discarding the new values.

Nine new regressions cover player/monster fractional and boundary round trips,
version-5 compatibility and invalid pitch rejection without partial mutation.
Full Release suite: 2,812 passed; warnings-as-errors build: zero warnings/errors.
The wire record size is unchanged. Direct callers must supply normalized monster
pitch for restore. This remains a pose archive: actor membership, inventory,
ACS/movers, invasion state and full rule restoration are not complete savegames.
Pitch networking/rendering and phase 1–3/native-runtime gaps remain open, including
real-game invasion validation. This is a source/regression self-audit.

### ACS sector-plane height queries

Converted native opcodes 261/262 (`GetSectorFloorZ`/`GetSectorCeilingZ`) into
the managed VM. Audited against `src/playsim/p_acs.cpp`: arguments are `(tag,
x, y)` with integer map coordinates, nonzero tags select the first matching
sector, and results are ACS fixed-point heights. Tag zero uses point lookup.
Queries read current simulation heights, including ongoing movement, rather
than the original map heights. Missing sectors return zero; incomplete argument
stacks terminate the fiber before a following action can execute.

Added 16 regression cases covering both planes, fractional heights, duplicate
tags, coordinate order/units, missing tags/unresolved points, each argument
underflow size, moving floors/ceilings, and preservation of the lower stack.
Full Release suite: 2,828 passed; warnings-as-errors build: zero warnings/errors.

Scope remains flat sectors. Managed polygon point lookup is not native BSP
lookup (particularly for points outside enclosing geometry); slopes, portals,
and 3D-floor geometry remain unconverted. This source/regression audit does not
establish full native gameplay or invasion synchronization parity.

### ACS triggering-line row offset and sidedef metadata

Converted native opcode 258 (`GetLineRowOffset`) against `src/playsim/p_acs.cpp`.
The query pushes the triggering line's front-sidedef middle-texture vertical
offset as an integer truncated toward zero, regardless of activation side.
World scripts return zero; malformed managed front-side references also safely
return zero. Nested scripts inherit the trigger and resumed scripts retain the
original trigger. The query does not consume existing stack values.

Source review found that level building discarded sidedef row offsets. Binary
row offsets now reach `LevelSide.MidTextureOffsetY`; UDMF combines `offsety`
with fractional `offsety_mid` in ZDoom, ZDoomTranslated and Vavoom namespaces,
following `src/maploader/udmf.cpp`. Other namespaces retain only the base offset.
The simulation checksum includes the newly observable offset.

Added 20 regressions: signed binary offset limits, UDMF namespace gating and
fractional addition, query truncation/front-side selection, missing/invalid
triggers, nested/resumed context, lower-stack preservation and checksum changes.
Full Release suite: 2,848 passed; warnings-as-errors build: zero warnings/errors.
This converts loading and querying the middle vertical offset; texture rendering,
runtime texture-offset mutation/scrollers, top/bottom offsets, and full native
sidedef special handling remain outside this pass. Full phases 1–3 and native
invasion runtime verification remain incomplete.

### ACS activator health and armor queries

Converted native opcodes 120/121 (`PlayerHealth`/`PlayerArmorPoints`) against
`src/playsim/p_acs.cpp`. Both push a value without consuming stack arguments.
Health reads any existing activator, including monsters and dead actors; armor
reads the managed player's current inventory. World or destroyed activators
return zero. Added opcode 121 to both old and compact bytecode walkers as a
zero-immediate instruction. Health and armor already participate in simulation
checksums, so no extra checksum fields were necessary.

Added 22 regression cases covering player/monster/dead/over-100 health,
player armor after death, missing/destroyed activators, unarmored monsters,
damage absorption, nested map-script context, resumed-script context with
updated values, lower-stack preservation, and old/compact decoder boundaries.
Full Release suite: 2,870 passed; warnings-as-errors build: zero warnings/errors.

Parity limits: native BasicArmor inventory can belong to non-player actors;
the managed inventory model currently belongs to players only. Managed health
and damage clamp at zero, so negative native overkill health is not represented.
These commands expose the current managed state; full inventory/lifecycle parity
and the phases 1–3 completion gates remain open. Compact decoding coverage does
not establish compact-bytecode VM execution parity.

### ACS Timer and authority tick boundary

Converted opcode 93 (`Timer`) against `src/playsim/p_acs.cpp`, which pushes
`Level->time` without consuming stack operands. Source audit of `src/p_tick.cpp`
found that native ACS thinkers execute before `Level->time` increments. Managed
thinker execution advances the clock before the separate ACS pass, so reading
that clock directly would return one tic too many. Authority ticks now capture
their starting tic and pass it to ACS; direct VM ticks read the current clock
without advancing it. No simulation stage was reordered.

Added 12 regressions for initial and later authority ticks, raw tic units,
direct VM calls (including the largest positive clock value), same-tick
consistency, delay elapsed time, suspend/resume, restored pose clocks, and
nested script execution time. Query tests also retain stack values below Timer.
Full Release suite: 2,882 passed; warnings-as-errors build: zero warnings/errors.

This audits clock observation within the current managed scheduler. Nested
scripts still follow the existing managed fiber snapshot schedule; full native
thinker scheduling, hub clocks, pause/freeze behavior, complete ACS saves, and
invasion round/timer/enemy synchronization in a running native game remain
outside this pass. The earlier phase completion gates remain open.

### ACS game mode and standard skill queries

Converted opcodes 91/92 (`GameType`/`GameSkill`) against `src/playsim/p_acs.cpp`
and the default skill return behavior in `src/gamedata/g_skill.cpp`. Authority
simulation now retains its spawn mode and normalized standard skill. GameType
returns 0/1/2 for single/cooperative/deathmatch independently of actor count;
GameSkill returns the configured standard skill index, clamped consistently
with the existing spawn filter. Both push a value without consuming arguments.
The settings are immutable after boot and included in simulation checksums.

Added 18 regressions covering all supported modes, standard skill indexes and
out-of-range normalization, omitted options, lower-stack preservation, checksum
distinction/equivalence, and dedicated-server propagation. Full solution run
passed 2,898 tests; the subsequent server-only run passed all 61 tests including
two added boot cases, bringing verified total coverage to 2,900. Final Release
warnings-as-errors build passed with zero warnings/errors.

Native title-map mode and custom MAPINFO skill ACSReturn values are not yet
represented. This does not implement complete skill gameplay rules or native
roster/network-game queries. Pose archives still rely on the destination
simulation's boot settings; they are not full session saves. Phases 1–3 and
native invasion runtime validation remain incomplete.

### ACS trigonometric operations

Converted native opcodes 220/221/222 (`Sin`, `Cos`, `VectorAngle`) in the enum,
both bytecode walkers and the word-format VM. Source audit covered
`src/playsim/p_acs.cpp` and angle conversions in `src/common/utility/vectors.h`.
Sin/Cos consume Q16 turns and produce rounded 16.16 values; angle inputs are
reduced modulo a full turn. VectorAngle consumes `(x,y)` and returns normalized
unsigned Q16 turns truncated toward zero. Missing operands stop the fiber.

Added 32 regressions for cardinal/diagonal angles, negative/wrapped turns,
integer limits, zero vector, quadrant and argument ordering, scaled vectors,
truncation, underflow, lower-stack preservation, and old/compact decoding.
Full Release solution: 2,932 passed; warnings-as-errors build: zero warnings/errors.

Managed operations use System.Math. Native `cmath.h` selects CRT, custom or
fast math depending on build flags; bit-for-bit results near rounding boundaries
across those backends have not been established by native differential tests.
This source/regression audit does not close full numerical/native parity,
compact VM execution, phases 1–3, or native invasion runtime validation.

### ACS packed-byte stack pushes

Converted PushByte (167), PushBytes (175), and Push2Bytes through Push5Bytes
(176–179) against `src/playsim/p_acs.cpp`. The word-format VM now consumes raw
unsigned bytes in source order and continues at the exact next byte offset,
including unaligned word opcodes. Counted pushes accept zero through 255 values.
Payload bounds are checked before pushing any values; truncated inputs stop
the fiber safely. The existing instruction budget still bounds execution.

The word bytecode walker previously could not handle these packed operands.
It now advances by their exact byte length. Added OperandByteCount metadata
for exact lengths in both walkers while retaining legacy OperandWordCount
units (rounded-up words for new packed word instructions; bytes for compact).

Added 23 regressions spanning execution order/unsigned values, all fixed push
widths, zero and maximum count, instruction-budget continuation, truncation,
and word/compact decoding boundaries. An initial test-helper compile error
from array Reverse overload resolution was corrected before the final run.
Full Release suite: 2,955 passed; warnings-as-errors build: zero warnings/errors.

This supports packed operands in the word-opcode VM; it does not implement
compact opcode execution, remaining byte-immediate specials, or full native
ACS scheduling/parity. Phases 1–3 and native invasion validation remain open.

### ACS byte-immediate specials and delay

Converted Lspec1DirectB through Lspec5DirectB (168–172) and DelayDirectB (173)
against `src/playsim/p_acs.cpp`. Byte specials read an unsigned special ID and
one through five unsigned arguments, zero-fill omitted arguments and reuse the
existing context-aware dispatch. Byte delay reuses countdown and legacy Hexen
extra-tic behavior. Truncated operands stop execution before special mutation.
The word walker now advances by exact byte lengths for these forms.

Added 26 regressions covering all argument widths against word-form sector
damage results, unsigned values, every payload truncation, delay endpoints,
stack preservation, actor context for restricted exits, both walker formats,
and oversized old-format directory counts. Corrected an existing fixture that
incorrectly padded byte operands to whole words. During fixture repair, use of
an enhanced-only test builder exposed an actual old-directory integer overflow:
count validation now uses division and offset checks use widened arithmetic,
rejecting malformed data instead of throwing. Test helper compilation and
minimum-lump-size issues were corrected before final verification.

Full Release suite: 2,981 passed; warnings-as-errors build: zero warnings/errors.
Only already-supported line specials are dispatched; RandomDirectB, compact
VM execution, remaining ACS/native gameplay parity, and native invasion runtime
validation remain open. Phases 1–3 are not complete.

### Packed ACS control-flow follow-up audit

Auditing the recent packed-byte conversion found two integration defects.
The VM rejected jump destinations not divisible by four, although packed
operands can place valid word opcodes at any byte offset. Jump validation now
retains range checks without imposing alignment. Native `PCD_CASEGOTOSORTED`
explicitly aligns its table count to four bytes; the VM now does so too.
The word walker previously discarded fractional-word padding when computing
table length. It now checks/counts the table in bytes, reports exact operand
length, and rejects truncated or oversized tables before traversal.

Added 21 regressions covering Goto, IfGoto, IfNotGoto and CaseGoto targets at
all three unaligned offsets; sorted-table matches/misses after packed operands;
and word-walker padding/truncation at each alignment. Existing invalid-jump
regressions remain passing. Source comparison used `src/playsim/p_acs.cpp`.
Full Release suite: 3,002 passed; warnings-as-errors build: zero warnings/errors.

This fixes control flow in the current word-format program model. Full behavior
module addressing/relocation, compact VM execution, GotoStack jump-table
dispatch and other unconverted ACS operations remain open. Phases 1–3 and
native invasion runtime validation remain incomplete.

### ACS indexed jump-table dispatch

Converted GotoStack (363) against `src/playsim/p_acs.cpp` and `FBehavior::Jump2PC`
in `src/playsim/p_acs.h`: the popped value indexes a jump table rather than
being a raw address. AcsProgram now accepts JumpPoints as byte offsets within
its Code. Registration bounds the table and validates destinations before
changing registered state, clones the table, and hashes it with program data.
Existing fibers retain their original table when a program is replaced.
Missing operands or invalid indexes terminate safely; unaligned targets work.

Added 14 regressions for all target alignments, index bounds, underflow,
lower-stack preservation, ownership, checksum differences, replacement
atomicity and active-fiber stability. Full Release suite: 3,016 passed;
warnings-as-errors build: zero warnings/errors.

Scope is explicitly registered managed programs. Native JUMP chunk loading,
module-relative address relocation and cross-module execution are not wired
into this API yet. This supersedes only the prior GotoStack dispatch gap;
full ACS parity, phases 1–3 and native invasion validation remain incomplete.

### Enhanced ACS JUMP chunk decoding and bounded relocation

Added MapBehaviorJumpTableCodec following native JUMP loading in
`src/playsim/p_acs.cpp`: the first JUMP chunk supplies ordered little-endian
module offsets, including an explicitly empty first table. Unknown chunks
are skipped after bounds checks. Results own their storage and are exposed
read-only; malformed input returns no partial table. Counts are bounded.

The relocation helper converts unsigned module offsets into byte offsets
within a supplied code range, using widened arithmetic and requiring room
for a complete word opcode. It retains unaligned destinations and rejects
targets before/after that range rather than silently binding other code.
Added 17 regressions covering duplicate/absent/empty chunks, unsigned offsets,
ownership, truncated headers/payloads, malformed tails/sizes, relocation
boundaries/alignment, and decode-to-GotoStack execution.
Full Release suite: 3,033 passed; warnings-as-errors build: zero warnings/errors.

The codec accepts an already-delimited enhanced chunk region. Automatic map
module registration, chunk-region extraction, cross-region jump targets,
ordinary branch relocation and compact opcode execution remain unintegrated.
This is the decoding/binding foundation, not complete BEHAVIOR loading or
full phases 1–3 completion. Native invasion validation remains outstanding.

### ACS module-relative branch addressing

Added AcsProgram.CodeBaseOffset for the original module address of a copied
code region. Source audit of `FBehavior::Ofs2PC` in `src/playsim/p_acs.h` and
branch cases in `p_acs.cpp` confirms that ordinary branch operands are module
offsets. Goto, conditional branches and case branches now subtract the code
base with widened bounds checks. Sorted case tables align their count using
the module position, rather than the copied region's local position.
Restart remains local to the script entry; pre-relocated GotoStack targets
remain local and are not relocated twice. Code-base metadata is validated,
copied and included in the program/fiber checksum.

Added 17 regressions covering the four ordinary branch forms, sorted-table
alignment for each code-base residue, indexed jumps, bounded restart loops,
out-of-region targets, invalid code regions and checksum changes. Full Release
suite: 3,050 passed; warnings-as-errors build: zero warnings/errors.

Callers still supply code regions and their original offsets explicitly.
Automatic module registration, cross-region execution, function/module calls,
compact opcode execution and native invasion runtime checks remain unfinished.
This closes addressing within a supplied region, not the full map integration
or phases 1–3 completion gates.

| Phase | Existing verified foundation | Work still preventing completion |
| --- | --- | --- |
| 1 — gameplay | XYZ movement, gravity/jump, flat-sector collision, timed states, damage/death, armor, buffered commands; current regressions pass | Native collision/dropoff/stacking/corner semantics; advanced geometry; complete actor state/action execution; player lifecycle/roster integration and remaining input commands |
| 2 — combat/AI | Managed weapon cadence, player damage dice/range and SSG vertical spread, pitch-aware traces, projectile definitions, 18 monster attack profiles, lost-soul charges, pain-elemental spawns, arch-vile attacks/resurrection, target-height floating, default slots and persistent per-actor hearing | Complete native state/action tables and boss death actions, arch-vile visual fire/remaining resurrection semantics, remaining soul/spawn semantics, obstacle float/path logic, sound compatibility/advanced alert rules, exact RNG/autoaim/refire/psprites/BFG and weapon-specific effects |
| 3 — maps/mods | Checked WAD/PK3 loading, Doom/Hexen/UDMF geometry and spawn flags, fractional planes, ordered overlays, basic DEHACKED, immediate lights plus fade/glow/strobe/flicker/stop and selected doors/floors/lifts/ceilings/crushers/stairs/teleport/exit specials | Remaining line/sector specials and advanced mover variants, remaining sector lighting/exact native lighting RNG and switch textures, full activation-side/monster/projectile rules, slopes/portals/3D floors, MAPINFO progression, resource namespaces/client consumption and complete DEHACKED/BEX/actor-definition execution |
| Shared release blocker | Failed snapshot builds preserve source state | Paging or a negotiated replacement snapshot protocol, atomic receive/apply, bounded tombstones, loss/reorder/late-join coverage; current 255-record / packet-size limits remain |
| Cross-phase validation | Full managed suite and native invasion policy compile | Running this native checkout with representative IWAD/maps/mods and comparing behavior; native class/resource negotiation and original invasion round/timer/enemy symptom still need real runtime confirmation |
| Later persistence phase | Backward-compatible v6 pose archive with fractional sector planes and actor pitch | Full actor membership/inventory/AI/projectiles/movers/ACS/invasion saves; pose archives are not complete savegames |

Earlier detailed reviews remain relevant: [foundation](HCDE_CSHARP_GAMEPLAY_FOUNDATION_AUDIT.md),
[combat/AI](HCDE_CSHARP_COMBAT_AI_AUDIT.md), [maps/mods](HCDE_CSHARP_MAPS_MODS_AUDIT.md),
[phase 2/3](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md),
[movers/damage](HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md), and
[command buffering](HCDE_CSHARP_INPUT_BUFFER_AUDIT.md). New default weapon-selection
and immediate sound-alert coverage supersedes only those specific open items.

## October 3 sync and ACS actor dimensions

Synced four upstream conversion commits through `c367f081`, preserving the local
line-ending-only changes during fast-forward. Added APROP_Height (35) and
APROP_Radius (36) to the existing ACS actor-property reader. Audited against
`src/playsim/p_acs.cpp`: GetActorProperty returns current dimensions as 16.16;
CheckActorProperty compares that exact value; SetActorProperty has no setter
for these read-only properties. Existing actor dimension checksums are retained.

Sixteen new regression cases cover fractional/zero/signed stored dimensions,
ignored setters, missing/destroyed targets, newest matching TID selection,
lower-stack preservation, and exact/mismatching CheckActorProperty results.
The initial post-sync no-restore run lacked assets for the new scripting test
project; root-level dependency restoration resolved that setup issue.

This adds current managed cylinder dimensions to ACS; native geometry, full
actor-property coverage and in-game invasion validation remain unfinished.
The master plan checkpoint is refreshed with the current solution counts.

## ACS configurable player JumpZ (2026-10-03)

Converted APROP_JumpZ (12) using native get/set cases in `src/playsim/p_acs.cpp`.
Only PlayerPawn accepts the setter; other actors return zero. Signed 16.16
values are retained. Player jumping now uses the configured value rather than
the prior hard-coded eight; default behavior stays eight. JumpZ participates
in the simulation checksum. Eight regressions cover signed/fractional/zero
values, player-only behavior, actual jump movement and checksum differences.

Initial integration expectations were corrected to account for current managed
gravity-before-movement ordering. Adding a checksum field required refreshing
the managed idle trace from 251903926 to 4261759856; its position and health
assertions remain unchanged. This is a managed trace, not native acceptance.
JumpZ is not yet included in pose archives or network property replication;
custom class defaults, full jump rules and native movement parity remain open.
Full suite passes 4,525 tests; Release build has zero warnings/errors.

## ACS configurable actor gravity (2026-10-03)

Converted `APROP_Gravity` (15) set/get/check into signed 16.16 actor state.
Native `p_acs.cpp` sets the actor multiplier directly and compares its ACS value;
`actorinlines.h::GetGravity` applies that multiplier unless `MF_NOGRAVITY` is set.
The managed vertical step now consumes the property. Ice chunks initialize it
to 0.125, matching `wadsrc/static/zscript/actors/shared/ice.zs`; scripts replace
this class default rather than multiplying it by another hidden factor.

Fifteen regressions cover exact signed/fractional/extreme ACS values, check
mismatches, zero/reversed/stronger acceleration, NoGravity, ice default replacement,
all-match TID writes, newest-match reads, missing/destroyed targets and checksum
inclusion while resting. Existing ice acceleration tests remain unchanged.
The expanded checksum changes the managed idle baseline to 1525314970; position,
health and tic assertions remain unchanged.

Scope: actor multiplier support is complete for this managed property subset.
Native movement applies Z motion before gravity and has ledge, water, sector and
level gravity rules beyond the current managed step. This pass preserves the
existing managed movement order and does not establish full native physics parity.
General class defaults, save/network replication of gravity and representative
native invasion sessions remain open. Full suite: 4,540 passing tests; Release
build: zero warnings/errors. This is a source review and regression self-audit.

## ACS configurable player view height (2026-10-03)

Converted `APROP_ViewHeight` (39) set/get/check. Native `p_acs.cpp` writes
PlayerPawn ViewHeight and the attached player's current view height; getters
call `player_t::DefaultViewHeight`, which reads the configured pawn property.
The C# player now separates this default from its changing eye height. Script
sets immediately update both; managed crouch, uncrouch and respawn eye resets
use the configured default. Non-player setters are ignored and queries return 0.

Thirteen regressions cover signed/fractional/zero defaults, exact property
checks, persistence through live ticks, crouch scaling and standing restoration,
setting while crouched, death eye-height changes, missing/destroyed actors,
non-player handling and checksum inclusion independent of current eye height.
The audit also found and fixed an unknown-property check incorrectly succeeding
for zero: native CheckActorProperty defaults to false. Two of these cases cover
that correction. The expanded idle checksum is 67100778; its position, health
and tic assertions are unchanged.

Scope: managed player eye-height integration and ACS property behavior.
Full native camera interpolation/bob/death behavior, generalized pawn class
defaults, recreation of pawn properties at respawn and save/network replication
remain incomplete. Full suite: 4,553 passing tests, zero failed/skipped; Release
build: zero warnings/errors. This is a source review and regression self-audit.

## ACS attack height and combat origin audit (2026-10-03)

Converted `APROP_AttackZOffset` (40) set/get/check for player pawns, default 8
from `wadsrc/static/zscript/actors/player/player.zs`. Native `actorinlines.h`
scales player attack offsets by crouch factor and returns 8 for other actors.
Native `p_map.cpp` uses center plus this offset for line attacks and PickActor;
`p_mobj.cpp` uses center plus AttackOffset(-4) for player missiles and clamps
the resulting world Z to the sector floor. Managed combat now follows those
origin rules, and monster hitscan aim accounts for its eight-unit offset.
Non-player ACS queries still return zero, as explicitly documented in native
`p_acs.cpp`; that query differs from the non-player combat default.

Sixteen new cases cover signed/fractional property round trips and exact checks,
default/scripted/crouched origins, missile floor clamping including airborne
owners, non-player query versus combat behavior, target-height hit changes,
missing/destroyed targets and checksum inclusion. The first full run exposed
79 old assertions, including the expanded checksum: geometry fixtures assumed
center origins. Fifteen fixture classes now explicitly configure zero offsets
to retain their exact boundary/range setups and assertions; the miss endpoint
test instead asserts the native default origin. Separate new tests validate the
eight-unit default and configured origins. No tests were disabled. The managed
idle checksum is now 42939098, with position/health/tic assertions unchanged.

Scope: flat-sector combat origins and the supported player missile path.
Floor clipping, slopes/portals/3D floors, complete native autoaim and projectile
spawn handling, general class defaults and save/network propagation of the
property remain open. Full suite: 4,569 passed, zero failed/skipped; Release
build: zero warnings/errors. Source review and regression self-audit only.

## ACS damage scaling and armor ordering (2026-10-03)

Converted `APROP_DamageFactor` (24) and `APROP_DamageMultiplier` (43), with signed
16.16 set/get/check state defaulting to 1. Native `p_acs.cpp` writes DamageFactor
and DamageMultiply directly. `p_interaction.cpp` multiplies positive damage by
the source's DamageMultiply, then the target's factor before armor; native
`AActor::ApplyDamageFactor` in `p_mobj.cpp` truncates the latter result to int.
Forced hits and ordinary telefrags bypass this scaling. A non-positive scaled
result cancels damage effects. An inflictor alone does not supply the source
multiplier. These rules are integrated into the managed damage boundary.

Twenty-seven new regression cases cover signed/fractional/extreme property
round trips and exact checks, separate truncation order, zero/negative damage
cancellation, armor ordering, forced/telefrag bypass, source versus inflictor,
resting checksums, all-match TID writes/newest reads and missing/destroyed actors.
The idle checksum is now 352595618 because both factors enter actor state hashing;
position/health/tic assertions remain unchanged. Full suite: 4,596 passing,
zero failed/skipped; Release warnings-as-errors build: zero warnings/errors.

Scope: the global actor multipliers in the existing managed damage path.
Typed class DamageFactors, passive/active inventory modifiers, NO_FACTOR and
NO_ENHANCE flags, self damage factors, LAXTELEFRAGDMG, and save/network propagation
remain open. Managed arithmetic saturates products outside int range; native
floating-to-int overflow parity has not been established. This is a source
review and regression self-audit, not a complete native damage audit.

## ACS melee range and monster range gates (2026-10-03)

Converted `APROP_MeleeRange` (38) signed 16.16 set/get/check state and integrated
it into managed monster melee decisions, including the attack-time recheck.
Native `actor.zs` defaults MeleeRange to 64 minus MELEEDELTA (20), hence 44.
`p_acs.cpp` sets/gets this property directly. Native `P_CheckMeleeRange` in
`p_enemy.cpp` rejects distance greater than or equal to range plus target radius;
attacker radius does not contribute. Its vertical separation comparisons are
strict, so touching vertical boundaries still qualify. Managed range checks now
follow these rules and require sight and non-friendship.

Eighteen new cases cover signed/fractional/zero property round trips, exact checks,
one-fixed-unit distance and vertical boundary differences, independence from
attacker radius, sight/friendship gates, script-enabled melee windup, reducing
range during windup to cancel damage, resting checksums and missing/destroyed
actors. Existing monster profile/combat regressions pass. The managed idle hash
is now 3247430384 because the range enters actor state hashing; position, health
and tic assertions remain unchanged. Full suite: 4,614 passing tests, zero
failed/skipped; Release warnings-as-errors build: zero warnings/errors.

Scope: actor property and existing managed melee AI paths. Native goal exceptions,
SECF_NOATTACK, NOVERTICALMELEERANGE, generalized class overrides, complete chase
and sight behavior and save/network propagation remain open. Player weapon
range continues to use its existing weapon dispatch and is not converted by this
actor-property pass. This is a source review and regression self-audit.

## ACS actor friction and ground velocity decay (2026-10-03)

Converted `APROP_Friction` (42) signed 16.16 set/get/check state, default 1
matching native `actor.zs`. `p_acs.cpp` sets and reads the actor multiplier
without clamping the stored property. Native `P_GetFriction` in `p_map.cpp`
multiplies surface friction by the actor property and clamps the effective
result to [0, 1]; `p_mobj.cpp` multiplies horizontal velocity by that result.
The managed flat-ground velocity decay now applies the same multiplier/clamp
to its existing surface constant. Stored negative and above-one values remain
queryable; they do not reverse or amplify ground velocity.

Sixteen regressions cover exact signed/fractional/extreme property values and
checks, ground multiplier/clamp limits, both velocity axes, airborne movement,
missing/destroyed targets, all-match TID writes/newest reads and resting checksum
inclusion. All existing physics and combat cases pass. The idle checksum is now
3569868298 due to friction state hashing; position, health and tic assertions
remain unchanged. Full suite: 4,630 passed, zero failed/skipped; Release build
with warnings as errors: zero warnings/errors.

Scope: actor property and the current managed ground decay path. Native sector
and terrain friction selection, friction-dependent movement acceleration,
water/flying/air-control rules, minimum velocity parity, class overrides and
save/network propagation remain open. Airborne cases establish preservation of
the existing managed behavior, not complete native air-friction parity. This is
a source review and regression self-audit.

## ACS NoTrigger and crossing suppression (2026-10-03)

Converted `APROP_Notrigger` (23) set/get/check to actor NoTrigger state.
Native `p_acs.cpp` normalizes setter values to MF6_NOTRIGGER and boolean checks
to nonzero/zero. Native `p_map.cpp` tests this flag before movement crossing
activation; managed ActivateCrossings now skips flagged actors. A skipped line
retains its special, so clearing the flag permits later activation. The separate
use path remains available, matching the native distinction.

Ten regressions cover nonzero signed boolean normalization, exact inverse checks,
crossing in both directions, preserving/re-enabling a one-shot special, live use
activation, missing/destroyed actors and checksum inclusion. The use fixture
initially approached the line from its non-usable back side; correcting its
position/yaw made it exercise front-side use without altering activation rules.
The idle hash is now 1508425544 due to NoTrigger state inclusion; position,
health and tic assertions remain unchanged. Full suite: 4,640 passed, zero
failed/skipped; Release warnings-as-errors build: zero warnings/errors.

Scope: ACS flag and existing movement crossing path. Native push triggers,
projectile crossings, puff-specific impact flags, complete non-player map-line
activation and save/network propagation remain incomplete. The shooter flag
is not substituted for native puff flags. Source review and regression
self-audit only; representative native invasion validation remains open.

## ACS actor score state (2026-10-03)

Converted `APROP_Score` (22) signed integer set/get/check into actor Score.
Native `p_acs.cpp` assigns Score directly, returns it without fixed conversion,
and compares it as an integer. It is separate from player frag statistics.
Managed ACS writes the activator or all matching TIDs and reads the newest match,
with the established missing/destroyed actor handling. Dead actors remain eligible
for score updates; the Health setter's dead-actor restriction does not apply.

Twelve regressions cover zero/positive/negative/full signed integer limits,
exact checks, non-player dead actors, missing/destroyed reads and writes,
missing-target checks against zero, all-match TID writes/newest reads,
independence from player frags and resting checksum inclusion. The expanded
idle hash is 2118297554; position, health and tic assertions remain unchanged.
Full suite: 4,652 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. This is a source review and regression self-audit.

Scope: script-owned actor score state. Automatic mode scoring, HUD presentation,
team score functions, save/network replication and native session comparison
remain incomplete; actor Score is not used as a substitute for player frags or
the invasion director's counters.

## ACS target/tracer pointer properties (2026-10-03)

Added `APROP_TracerTID` (27) reads/checks from managed projectile tracer state.
Native `p_acs.cpp` reads target/tracer pointers and compares their current TIDs;
its setter switch has no TargetTID or TracerTID cases. The audit found the prior
managed TargetTID setter incorrectly redirecting monster AI and removed it.
The earlier test now seeds the AI target directly and verifies that an ACS write
cannot change it. Native `p_mobj.cpp` assigns missile target to its source;
managed projectile TargetTID now reads Owner, while TracerTID reads the homing
target. Queries resolve current TIDs and return zero for destroyed references.

Ten new regression cases cover projectile target/tracer distinction, ignored
setters, exact checks, referenced TID changes/destruction, missing/destroyed
query actors, ordinary missiles without tracers and actors without pointer
state. Existing monster target/property and projectile regressions pass. No new
state fields were added, so the managed idle checksum remains 2118297554.
Full suite: 4,662 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. This is a source review and regression self-audit.

Scope: the existing managed monster AI target and projectile owner/tracer paths.
General actor target/tracer/master storage, ACS pointer-setting functions,
puff-owner queries, complete native pointer lifetime handling and save/network
replication remain open. The removed property setter is intentionally unavailable;
native pointer-changing APIs must be converted separately.

## ACS speed and movement integration (2026-10-03)

Converted `APROP_Speed` (1) signed 16.16 set/get/check to actor MovementSpeed.
Native `p_acs.cpp` assigns Speed directly. PlayerPawn `player.zs` defaults Speed
to 1 and multiplies forward/side movement by Speed/256. The managed thrust path
now applies the property. Existing monster ChaseSpeed is backed by MovementSpeed
with the prior quarter-scale conversion retained; catalog assignments still use
that existing conversion. Projectiles initialize MovementSpeed from their kind
defaults and consume it when aiming or tracking. ACS changes the property without
immediately replacing current velocity, matching the native setter's scope.
Projectile pitch clamping uses absolute speed bounds to avoid reversed bounds
when a script stores a negative value.

Sixteen new cases cover signed/fractional/extreme property round trips and exact
checks, zero/fractional/positive/negative player thrust, monster scale binding,
projectile defaults and future aim versus current velocity, missing/destroyed
targets and resting checksum inclusion. Existing player movement, AI and
projectile regressions pass. The idle hash is now 3597579456 because speed enters
actor state hashing; position/health/tic assertions remain unchanged. Full suite:
4,678 passed, zero failed/skipped; Release build: zero warnings/errors.

Scope: existing managed thrust/chase/projectile paths. Native monster chase timing
and speed are not fully reproduced by the retained quarter-scale foundation.
Player TweakSpeeds, inventory speed modifiers, air/water/flying thrust, complete
signed homing behavior, generalized class defaults and save/network propagation
remain open. This is a source review and regression self-audit.

## ACS base damage and projectile impact integration (2026-10-03)

Converted `APROP_Damage` (2) integer set/get/check state. Native `SetDamage` in
`actor.h` replaces DamageVal and clears DamageFunc; `p_acs.cpp` reads it through
GetMissileDamage(0,1). Native `p_mobj.cpp` returns the base without a random roll
for this query, while ordinary masked missile impacts multiply by a die roll.
Managed projectile constructors now initialize actor Damage from existing kind
defaults, and ImpactDamage reads that state. Existing actor/line/plane impact
paths therefore consume script-configured bases. Blast-radius and BFG spray
behavior retain their separate existing rules.

Eleven new cases cover integer property values and checks, zero/configured plasma
impacts on actors, doubled geometry damage with matching random-stream state,
missing/destroyed actors and resting checksums. Existing projectile defaults and
combat/geometry cases pass. Negative DamageVal without DamageFunc is an invalid
native function encoding: native asserts then returns 0. Managed storage keeps
the signed value but queries and impacts return 0 rather than executing a missing
function. This fallback is explicitly tested and is not scripted-function support.
The expanded idle hash is 3565784442; position/health/tic assertions are unchanged.
Full suite: 4,689 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. Source review and regression self-audit only.

Scope: integer base damage and existing managed projectile impact paths.
DamageFunc expressions, generalized class damage defaults, native random-stream
equivalence and overflow behavior, other missile-damage actions and save/network
propagation remain open. This property is separate from the actor damage factors
and does not replace weapon-specific damage rolls.

## ACS dropped marker and pickup initialization (2026-10-03)

Converted `APROP_Dropped` (18) boolean set/get/check to actor Dropped state.
Native `p_acs.cpp` changes MF_DROPPED directly and normalizes checks to boolean.
Native inventory code initializes dropped inventory and distinguishes it from
map pickups. The managed SpawnDroppedPickup path now initializes the marker;
ordinary map pickups retain false. Changing this flag does not rewrite the
independent ammo-skill and pickup amount fields.

Ten new cases cover nonzero signed boolean normalization and mismatch checks,
spawned drop queries with both ammo-skill modes, clearing the flag without
altering pickup fields, map pickup defaults, missing/destroyed targets and
resting checksums. Existing pickup/drop regressions pass. The idle hash is now
315421400 due to marker inclusion; position/health/tic assertions are unchanged.
Full suite: 4,699 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. Source review and regression self-audit only.

Scope: exposed actor marker and managed drop creation. Native dropped inventory
weapon-stay, item respawn, crusher destruction, local-drop policy, inventory
ownership transitions, class defaults and save/network marker propagation remain
open. These behaviors are not represented by IgnoreAmmoSkill and must be converted
separately; this checkpoint does not claim full dropped-inventory parity.

## ACS reaction time and managed AI counter (2026-10-03)

Converted `APROP_ReactionTime` (37) integer set/get/check. Native `p_acs.cpp`
assigns and reads actor reactiontime directly; `p_enemy.cpp` blocks missile
attacks while reactiontime is nonzero. The managed actor property reads/writes
the existing MonsterBrain reaction countdown when present and stores a counter
for actors without brains. Thus script changes affect the existing managed
initial attack delay, and queries observe countdown and damage wake-up clearing.

Eleven regressions cover integer values on player/non-brain and monster actors,
exact checks, countdown queries with delayed attack start, damage wake-up clearing,
missing/destroyed targets and non-brain checksum inclusion. Existing wake-up,
arch-vile raise and AI tests remain passing. The idle hash is now 3861582658 due
to reaction state inclusion; position/health/tic assertions are unchanged.
Full suite: 4,710 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. This is a source review and regression self-audit.

Scope: ACS counter access and existing managed monster delay. Native player
reaction-time movement blocking, negative counter handling, class-specific
defaults and chase timing remain open. The managed brain still shares this
counter with raise delay; full native separation and replacing/removing brains
without losing actor counter state require further conversion. Save/network
propagation is incomplete. Player counter storage does not imply player movement
blocking has been implemented.

## Player reaction-time movement gate (2026-10-03)

Continued the prior reaction-property pass by converting PlayerPawn HandleMovement
gating from native `player.zs`. Every nonzero reactiontime decrements instead of
calling MovePlayer/CheckJump/CheckMoveUpDown. Turn180 can arm before this check,
but movement yaw and turn progress wait. Native PlayerThink calls CheckPitch and
CheckCrouch separately and still handles weapon/use paths. The C# player now gates
yaw, thrust and jump while preserving those existing independent paths and physics.

Six new cases cover two-tic countdown and following-tic resumption, negative
counter decrement and integer wrap, momentum preservation with pitch/crouch,
turn180 arming/resumption, and attack/use processing during delay. Existing ACS
reaction-property and movement cases pass. Full suite: 4,716 passed, zero
failed/skipped; Release warnings-as-errors build: zero warnings/errors.
No state fields were added; idle hash remains 3861582658. Source review and
regression self-audit only.

This closes the preceding audit's player yaw/thrust/jump blocking gap. Native
CheckMoveUpDown, swim/fly paths, full crouch ordering, teleport delay initialization,
class defaults, general freeze flags and save/network state remain incomplete.
The earlier monster shared raise/reaction-counter limitation also remains open.

## Actor-owned reaction state and brain lifecycle (2026-10-03)

Continued reaction conversion to match native actor-owned reactiontime storage.
The prior property wrapper stored a stale fallback while MonsterBrain held the
active counter, so removing/replacing a brain could lose or restore old values.
Actor now owns the counter; an attached brain reads/writes that same field.
Detachment snapshots the value only for the detached brain, and replacement
preserves the actor's current value. Explicit values set before first attachment,
including zero, take precedence over the existing managed brain default (10).
One brain cannot be attached simultaneously to two actors; validation precedes
mutation so a rejected attachment preserves both actors' state.

Nine new regressions cover live countdown through removal/reattachment, replacement,
explicit zero/negative/positive initial values, existing default initialization,
shared-brain rejection, isolation after detachment and checksum initialization
state. Existing ACS, player movement, wake-up and raise tests pass. The explicit
initialization bit enters hashing because it affects future first attachment;
idle hash becomes 1449899472, with position/health/tic assertions unchanged.
Full suite: 4,725 passed, zero failed/skipped; Release warnings-as-errors build:
zero warnings/errors. Source review and regression self-audit only.

This closes the earlier brain replacement/removal state-loss gap. Managed raise
delay still shares the reaction counter; native class defaults, chase timing,
general brain serialization and save/network replication remain open. The retained
managed default is not claimed to match every native actor class.

## Separate raise wait from actor reaction time (2026-10-03)

Continued resurrection timing conversion. Native `AActor::Revive` in `p_mobj.cpp`
does not replace reactiontime with a raise duration; `p_enemy.cpp` enters the
corpse's raise state separately. The managed Revive path previously used the
ACS reaction counter for this wait. MonsterBrain now owns separate RaiseTics,
included in its checksum. Revive starts that wait without changing reactiontime;
raising ticks do not decrement reactiontime. After the raise wait completes,
the existing managed reaction delay resumes. Death and pain paths clear raise
waiting separately. Existing arch-vile duration assertions now inspect RaiseTics.

Six new cases cover zero/positive/negative reaction preservation throughout raising,
reaction clearing without bypassing raise wait, death cancellation and independent
raise checksum state. Existing arch-vile and invasion resurrection regressions
remain passing. Full suite: 4,731 passed, zero failed/skipped; Release warnings-
as-errors build: zero warnings/errors. The player-only idle hash remains
1449899472 because it has no monster brain. Source review and regression self-audit.

This closes the shared raise/reaction-counter gap. Full native raise-frame execution,
friendliness copying, actor-default restoration, class-specific reaction/chase
timing and save/network propagation remain open. RaiseTics remains the managed
duration gate, not a claim of complete native resurrection-state parity.

## Conversion and audit: resurrection friendship and targets (2026-10-03)

Compared `AActor::Revive`, `CopyFriendliness` in `src/playsim/p_mobj.cpp`
and arch-vile resurrection in `src/playsim/p_enemy.cpp`. Native resurrection
calls `CopyFriendliness(self, false)` after clearing the corpse's target.
The managed path now copies Friendly, FriendPlayer, TidToHate and NoHatePlayers
from the arch-vile. Revive clears both current and last-enemy targets instead
of immediately assigning the arch-vile's target. The independent reaction
counter remains unchanged, and normal target acquisition waits until raising ends.

Five new cases cover hostile-to-friendly, friendly-to-hostile and friendly-to-friendly
revivals, target acquisition after the raise wait, and reaction-counter retention.
Existing resurrection and invasion regressions remain passing. Full Release suite:
4,736 passed, zero failed/skipped; warnings-as-errors build: zero warnings/errors.
Managed player idle checksum remains 1449899472; no new state fields were added.

This is source comparison and managed regression coverage. Native team/last-look
fields, additional friendship flags, complete default-flag restoration and native
kill accounting remain outside this change. Representative native engine and
invasion round/timer/enemy synchronization sessions remain required; phases 1–3
are not complete.

## Conversion and audit: damage-retaliation enemy memory (2026-10-03)

Converted the last-enemy replacement predicate from `ReactToDamage` in
`src/playsim/p_interaction.cpp`. A living remembered player is preserved;
a living remembered monster is also preserved when the owner has a nonzero
TidToHate. Otherwise a successful retaliation switch remembers the old current
target, including null. The previous implementation always replaced remembered
monsters and skipped memory updates entirely when acquiring from a null target.
The audit also corrected the liveness condition to health rather than damage
eligibility: a living non-shootable remembered enemy can still be preserved.
Destroyed references are treated as absent in the managed actor collection.

Ten new regression cases exercise zero, positive and negative hate TIDs, player
and monster memory, dead remembered players/monsters, acquisition after clearing
the current target, and a living non-shootable remembered enemy. The initial
fixtures used same-class monsters, which correctly blocked infighting before
retaliation; fixtures now use distinct attacker/victim classes. Existing
last-enemy, resurrection, checksum and invasion cases remain passing.

Full Release suite: 4,746 passed, zero failed/skipped; Playsim 3,673.
Warnings-as-errors build: zero warnings/errors. No checksum fields were added;
the managed player idle baseline remains unchanged. This is native source
comparison and managed regression coverage, not a native session acceptance
result. Full native hate-target searching, chase/state scheduling, save/network
AI state and representative invasion sessions remain open; phases 1–3 remain
incomplete.

## Conversion and audit: chase threshold target lifetime (2026-10-03)

Converted the threshold-maintenance predicate in `A_DoChase` from
`src/playsim/p_enemy.cpp`: any nonzero threshold clears when the current target
is absent or dead, and otherwise decrements. C# previously decremented only
positive thresholds without checking target lifetime, leaving a retaliation
lock after a target died or was cleared. Removed/destroyed actors count as absent.
The liveness check uses health, so a living non-shootable target still counts down.
Negative thresholds now decrement too; the managed boundary explicitly uses
unchecked arithmetic for deterministic integer wraparound. Native compiler
behavior at signed integer overflow is not established by this source audit.

Nine new cases cover positive/zero/negative counters, the managed integer
boundary, dead and cleared targets allowing an immediate new retaliation,
a removed target and a living non-shootable target. Existing target-switch,
retaliation-memory, resurrection, checksum and invasion regressions pass.
Full Release solution: 4,755 passed, zero failed/skipped; Playsim 3,682.
Warnings-as-errors Release build: zero warnings/errors; whitespace check passes.
No checksum fields were added and the player idle baseline is unchanged.

This converts the bounded threshold rule within the managed tick loop. Native
chase-action cadence, state scheduling, conversation/in-chase guards and complete
goal/invisibility target semantics remain unfinished. Representative native
invasion round/timer/enemy sessions and phase 1–3 acceptance remain required.

## Conversion and audit: retaliation decisions survive chase ticks (2026-10-03)

Compared `ReactToDamage` in `src/playsim/p_interaction.cpp` and `A_DoChase`
in `src/playsim/p_enemy.cpp`. Native damage handling accepts or rejects the
source as a new target through OkayToSwitchTarget; normal chase uses that target.
The managed Tick previously replaced a valid current target with LastDamageSourceId
unconditionally, undoing policy rejection on the following tick. Removed this
redundant override; damage wake-up remains responsible for retaliation targets.
LastDamageSourceId remains available for death attribution and other consumers.

Seven new cases apply real damage and then tick the brain. They verify that
threshold, NoTargetSwitch, NoHatePlayers, NeverTarget, NoTarget and friendly-source
rejections persist, while an accepted switch persists and remembers the prior
enemy. Each rejection case checks that damage attribution did record the source,
so it exercises the formerly broken interaction rather than blocked damage.
Existing chase threshold, retaliation memory, resurrection and invasion cases pass.

Full Release suite: 4,762 passed, zero failed/skipped; Playsim 3,689.
Warnings-as-errors Release build: zero warnings/errors; whitespace check passes.
No checksum fields were added, and the player idle baseline remains unchanged.
This is a native source comparison and managed regression self-audit. Complete
native target acquisition, friendship/team/player rules, chase-state scheduling,
AI save/network state and representative invasion sessions remain unfinished.
Phases 1–3 are still open.

## Conversion and audit: friendship changes during chase and hearing (2026-10-03)

Compared `A_DoChase` and `A_Look` in `src/playsim/p_enemy.cpp`. Native chase
drops a target when it becomes a friend (with a separate goal exception);
native look treats a heard friend as a reason to search for enemies rather than
attack that friend. The managed chase now filters current targets through the
existing IsFriend predicate. Immediate hearing and remembered-noise acquisition
apply the same predicate, preventing a discarded friend from being reacquired
on the same or later tick. Target loss uses the existing idle path to cancel
pending attacks. Noise memory itself is retained.

Six new cases cover friendship changes during windup with and without noise
memory, friendly noise while the brain is enabled/disabled, normal hostile
noise/chase, and friendly actors with different nonzero FriendPlayer values
under the existing managed friendship subset. The attack-cancellation cases
run fifteen ticks and verify the former target takes no damage.
Full Release solution: 4,768 passed, zero failed/skipped; Playsim 3,695.
Warnings-as-errors Release build: zero warnings/errors; whitespace check passes.
Existing hearing, retaliation, resurrection, checksum and invasion tests pass.
No checksum fields were added; the managed player idle baseline is unchanged.

This is source comparison and managed regression self-audit. Native goal
exceptions, team/deathmatch friendship, player friendship defaults, friendly
enemy searches/wandering and complete look/chase scheduling remain incomplete.
Representative native invasion synchronization sessions and phases 1–3 acceptance
are still required.

## Conversion and audit: visual player acquisition friendship (2026-10-03)

Converted the IsFriend rejection in `isTargetablePlayer` from
`src/playsim/p_enemy.cpp` into managed visual player acquisition. Current-target
and hearing paths already rejected friends, but the nearest-visible-player
fallback could reacquire the same friend or select a friend ahead of an eligible
enemy. The fallback now excludes friends before distance/ID ordering, leaving
normal sight, range and damage-eligibility checks intact.

Eight cases cover the existing IsFriend subset with zero/matching/different
FriendPlayer values, selection of a farther hostile player over a nearby friend,
real noise followed by visible fallback, and a player becoming friendly during
windup. The latter two run fifteen brain ticks to check reacquisition; the
friendship-change case also verifies no damage to the former target.
Fixtures select the map monster by DoomEdNum; map actor IsMonster initialization
was not assumed by this test and remains a separate class-default audit concern.

Full Release suite: 4,776 passed, zero failed/skipped; Playsim 3,703.
Warnings-as-errors build: zero warnings/errors; whitespace check passes. Existing
hearing, chase, resurrection, checksum and invasion cases pass. No checksum
fields were added and the managed player idle baseline is unchanged.

This closes a bounded acquisition gap under the current managed friendship
predicate. Full native friendship/team/deathmatch/player defaults, look ordering
and RNG, stealth/cheat visibility, class flags and complete enemy/hate-TID searches
remain incomplete. Native invasion sessions and phase 1–3 acceptance remain open.

## Conversion and audit: monster spawn classification (2026-10-03)

Followed up the map-classification concern from the prior acquisition audit.
Native Monster in `src/scripting/thingdef_properties.cpp` sets MF3_ISMONSTER;
Doom imp and lost-soul definitions use that property. Supported map monsters
previously received brains without IsMonster, unlike managed bots. Map spawning
now initializes classification for supported monster brains. Pain-elemental
lost-soul spawning sets it explicitly too. Pickups, unknown things and player
starts remain unclassified by this rule.

Native DEHACKED in `src/gamedata/d_dehacked.cpp` synchronizes ISMONSTER with
COUNTKILL for non-player actors. Explicit Bits patches now set/clear managed
IsMonster from bit 0x00400000; non-flag patches preserve class classification.
This does not convert COUNTKILL accounting or the other native flag mutations.

Eleven new cases cover five map monster types, pickups/unknown things, COUNTKILL
set/clear, a health-only patch, dynamically spawned souls and a real infighting
damage interaction. The last case verifies NoInfighting blocks plain-monster
damage to a classified map monster. Full Release suite: 4,787 passed, zero
failed/skipped; Playsim 3,714. Warnings-as-errors build: zero warnings/errors;
whitespace check passes. Existing gameplay/invasion cases remain passing.
The existing checksum already includes IsMonster; no new checksum fields were
added and the player-only idle baseline remains unchanged.

This closes supported spawn classification and explicit DEHACKED flag handling,
not full class inheritance, mod-defined monsters, standard flag restoration,
COUNTKILL totals, actor-state saves or networking. Native invasion sessions and
phase 1–3 acceptance remain open.

## Conversion and audit: spawned lost-soul friendship inheritance (2026-10-03)

Compared `A_PainShootSkull` in
`wadsrc/static/zscript/actors/doom/painelemental.zs` with `CopyFriendliness`
in `src/playsim/p_mobj.cpp`. Native spawning copies the parent's friendliness
before starting a skull attack. Managed SpawnLostSoul now copies Friendly,
FriendPlayer, TidToHate and NoHatePlayers after successful placement and before
charging. Existing explicit target handling, placement checks and invasion child
registration remain in place. Targetless spawns also receive the four fields.

Six new cases cover friendly/hostile parents, zero/positive/negative hate TIDs,
player ownership, targetless children, and actual attack/death action spawning.
The action cases verify inheritance on the single attack soul and all three
death-burst souls. Full Release suite: 4,793 passed, zero failed/skipped;
Playsim 3,720. Warnings-as-errors build: zero warnings/errors; whitespace check
passes. Existing soul placement/charge and invasion child-count cases pass.
These fields already participate in actor checksums; no checksum fields were
added and the player idle baseline is unchanged.

This converts the four currently supported friendliness fields. Native team,
last-look and additional friendship flags, LastHeard copying, PAF action options,
class replacements, complete soul limits and death-time friendly-target policy
remain unfinished. This is a source/regression self-audit; native invasion
sessions and complete phase 1–3 acceptance remain open.

## Conversion and audit: spawned soul target exclusions and memory (2026-10-03)

Compared pain-elemental `A_PainShootSkull` and `AActor::CopyFriendliness`
(`wadsrc/static/zscript/actors/doom/painelemental.zs`, `src/playsim/p_mobj.cpp`).
Native copying rejects targets with MF3_NOTARGET or MF7_NEVERTARGET and sets
LastHeard alongside the accepted target. The managed spawned-soul path now
checks both supported flags and records LastHeardTargetId before StartCharge.
Null targets leave both pointers empty. The existing managed CanTakeDamage
requirement is retained; native copying itself does not require health/shootability,
so dead/non-shootable target copying and charge behavior still need conversion.

Six new cases cover all four exclusion-flag combinations, a null target and
normal target recovery from copied hearing memory after stopping the charge and
clearing the current target. The recovery target is a monster, with the player
outside visual acquisition range, to ensure the hearing path supplies it.
Full Release solution: 4,799 passed, zero failed/skipped; Playsim 3,726.
Warnings-as-errors Release build: zero warnings/errors; whitespace check passes.
Existing pain-elemental, friendship and invasion cases remain passing. No checksum
fields were added; the managed player idle baseline remains unchanged.

This closes the exclusion/memory subset, not full native CopyFriendliness,
PAF action options, team/last-look flags, soul target pointer lifetime or native
charge-state scheduling. Death-time friendship changes, representative native
invasion sessions and phase 1–3 acceptance remain open. This is source comparison
and managed regression self-audit.

## Validation

- Release solution: **4,799 passed, zero failed/skipped**, 3,876 cases above baseline.
  Protocol 15; Gamedata 10; Transport 10; Master 1; RCON 6; MapLoader 500;
  Playsim 3,726; Client 12; Net.Core 351; Pregame 97; Server 63; Scripting 8.
- The lighting pass initially saw a five-second timeout in
  `Pump_LiveSessionReceivesGuestClientInput`; it passed the targeted rerun and the
  subsequent full solution run. No timeout/test-disabling workaround was added.
- `dotnet build csharp/HCDE.sln -c Release --no-restore --verbosity quiet -warnaserror`:
  zero warnings/errors. Tests/build run from the repository root using SDK
  10.0.201 targeting net8.0; the nested SDK pin is unchanged.
- Existing invasion multi-client, stale-tic, timer/wave/enemy, input retry,
  physics, combat, map/mod and save regressions remain passing.
- `tests/invasion_stage9/invasion_policy_tests.cpp` compiles with MSVC C++17,
  `/W4 /WX`; compile-time policy assertions pass. This is not a full engine build.
- `git diff --check`: no whitespace errors. No dependencies or native sources
  changed in this pass. Prior committed work is retained.

This is a source review and regression self-audit, not an independent review or
an exhaustive native parity audit. Passing this suite does not close the gates
above. Full completion requested by the user has not been achieved.

## Audit: ACS actor properties and UseInventory (2026-09-29)

`SetActorProperty` / `GetActorProperty` (opcodes 245/246) route through
`AcsActorProperties` with tid 0 meaning the activator and non-zero tids iterating
all matches (`AcsActorTid`). Health refuses writes on dead actors. Step and
dropoff heights accept fixed-point raw values like native. `APROP_TARGETTID` reads
and writes through `MonsterBrain` only. Unknown property ids read as 0 and ignore
sets. `AcsActorPropertyTests` covers health by tid and tid-zero activator paths.

`UseInventory` (418) pops a string id and pushes 0/1 like `p_acs.cpp`
`UseInventory` → `DoUseInv`. Only weapon class names the inventory layer already
knows are supported; success sets `Pending` when the ready weapon differs. Health,
armor, keys, activator-null all-player use, and `UseActorInventory` are absent.

Release `HCDE.Playsim.Tests`: **2,229** passed (includes two new inventory tests
and two actor-property tests from this slice). Phase 1 gate 5 and master-plan
checkboxes stay open. Managed gameplay trace checksum **2927491796** unchanged.

## Audit: UseActorInventory (2026-09-29)

`UseActorInventory` (419) pops string id then tid (top = string), matching
`PCD_USEACTORINVENTORY` in `p_acs.cpp`. Tid 0 calls the same all-player
`UseInventory` path as a null activator. Non-zero tids sum `DoUseInv` across
`GetActorIterator` matches. Weapon rules match activator `UseInventory`.
`AcsPlayerInventoryTests.UseActorInventory_SelectsWeaponOnTidTarget` covers a
cooperative second player. `ACSF_GetMaxInventory` and non-weapon use remain
absent.

Release `HCDE.Playsim.Tests`: **2,230** passed after this note.

## Audit: actor-property flags and UseActorInventory tid 0 (2026-09-29)

Regression-only slice: `AcsActorPropertyTests` now covers `APROP_Ambush` (10) and
`APROP_TARGETTID` (26) on a monster tid, including brain target resolution by thing
id. `UseActorInventory_TidZeroUsesEveryPlayer` checks cooperative tid **0** selects
the weapon on both spawned players. No playsim behavior changes.

Release `HCDE.Playsim.Tests`: **2,233** passed after this note.

## Audit: ACS ammo capacity opcodes (2026-09-29)

`GetAmmoCapacity` (271) and `SetAmmoCapacity` (272) follow `p_acs.cpp`
`PCD_GETAMMOCAPACITY` / `PCD_SETAMMOCAPACITY`: Doom ammo string names only
(`Clip`, `Shell`, `Cell`, `RocketAmmo`, …). Reads and writes the managed
`MaxBullets` / `MaxShells` / `MaxRockets` / `MaxCells` fields on the activator
player. Weapons, armor, and `ACSF_GetMaxInventory` are still absent. Tests cover
pistol-start read and a set/get round trip.

Release `HCDE.Playsim.Tests`: **2,235** passed after this note.

## Audit: CheckWeapon and SetWeapon ACS (2026-09-30)

`CheckWeapon` (223) and `SetWeapon` (224) follow `p_acs.cpp` ready-weapon checks
and `SetWeapon` script util. Non-players read as no match. Class names use the same
Doom weapon string table as inventory (`Pistol`, `Shotgun`, …). Ready weapon is
`PlayerInventory.Selected`; success on set reuses `Use` (pending raise, ammo gate).
DECORATE class indices, `SetMarineWeapon`, and psprite state are absent.

Release `HCDE.Playsim.Tests`: **2,237** passed after this note.

## Audit: actor-property damage and mass regressions (2026-09-30)

Regression slice only: `APROP_INVULNERABLE` (11) is wired to `Actor.Invulnerable` and
`ActorDamage.Apply` already honors it for non-forced hits.
`SetActorProperty_InvulnerableBlocksOrdinaryDamage` sets the flag through ACS then
damages the imp. `APROP_MASS` (32) set/get round trip is covered. Render, sound,
and remaining `APROP_*` ids are still absent.

Release `HCDE.Playsim.Tests`: **2,239** passed after this note.

## Audit: health UseInventory and Friendly property (2026-09-30)

`UseInventory` now handles `Health` / `Stimpack` / `Medikit` / `HealthBonus` /
`Soulsphere` with the same per-type amounts and caps as `PickupCatalog` gifts
(no inventory item instances; each use applies one dose). Weapons are unchanged.
`APROP_FRIENDLY` (16) gains an ACS get/set regression on a monster tid.

Armor, keys, and `UseActorInventory` health use remain absent.

Release `HCDE.Playsim.Tests`: **2,242** passed after this note.

## Audit: touching-sector scroll carry (2026-09-30)

`ActorPhysics.ApplySectorScroll` now sums carry from every sector sampled at the
actor center and at radius on the four cardinals, then averages each axis when
more than one touched sector contributed a non-zero scroll on that axis (player
`COMPATF_BOOMSCROLL` monster quirk is not split). Hexen player carry still applies
per touched sector. `MF8_INSCROLLSEC` thinker marking and full sector node lists
are absent. `TouchingAdjacentScrollerCarriesWhenRadiusOverlaps` places the player
in sector 0 with radius overlap into a scrolling sector 1.

Release `HCDE.Playsim.Tests`: **2,243** passed after this note.

## Audit: armor UseInventory and more APROP regressions (2026-09-30)

`UseInventory` now applies `BasicArmor` / `Armor` (green suit to 100 with save
percent) and `ArmorBonus` (+1 up to 200) without inventory items. `APROP_NOTARGET`
(19) and `APROP_SPAWNHEALTH` (17) ACS get/set tests join the existing ambush,
mass, and invulnerable coverage. Mega armor, keys, and `UseActorInventory` armor
use remain absent.

Release `HCDE.Playsim.Tests`: **2,246** passed after this note.

## Audit: inventory max health, mega armor use, actor step height (2026-09-30)

`CheckInventory` max mode for the `Health` string now uses **200**
(`MaxHealthBonus`) instead of 100. `UseInventory` adds `MegaArmor` / `GreenArmor`
with mega/green amounts and save percents. `UseActorInventory` health applies to
cooperative tid targets. `APROP_MAXSTEPHEIGHT` (44) fixed-raw set/get is covered.
`ACSF_GetMaxInventory`, DECORATE max amounts, and real inventory items remain absent.

Release `HCDE.Playsim.Tests`: **2,250** passed after this note.

## Audit: NoTarget wake, dropoff height, UseActorInventory armor (2026-09-30)

`NoTargetOnPlayerBlocksMonsterWakeTarget` confirms ACS `APROP_NOTARGET` on the
activator ties into `OkayToSwitchTarget` / `WakeOnDamage` (imp acquires the player
without the flag, not after). `APROP_MAXDROPOFFHEIGHT` (45) fixed-raw set/get is
covered. `UseActorInventory` applies basic armor on a cooperative tid; backpack
max clip is checked through `GetAmmoCapacity`. Scroll affect masks and
`ACSF_GetMaxInventory` remain absent.

Release `HCDE.Playsim.Tests`: **2,254** passed after this note (backpack
`GetAmmoCapacity` test uses `GiveBackpack`, not `HasBackpack` alone).

## Audit: ACSF_GetMaxInventory CallFunc (2026-09-30)

`CallFunc` now handles native function **93** (`ACSF_GetMaxInventory`) with two
stack args (tid, string id) and returns `CheckInventory` max amounts on the
resolved actor (tid **0** = activator). Zero-arg invasion `CallFunc` queries
are unchanged. Other ACS library functions remain absent.

Release `HCDE.Playsim.Tests`: **2,255** passed after this note.

## Audit: carry scroll affect masks (2026-09-30)

`SectorCarryScroll` stores native-style `ScrollCarryAffect` flags (players,
monsters, static objects). `SumCarryScrollForActor` filters thinkers per actor
before carry; `SetSectorScroll` / `AppendSectorCarryScroll` default to **All**
so line **223** behavior is unchanged. Tests cover player-only, monster-only,
and split affect on one sector. `MF8_INSCROLLSEC` marking and `Scroll_Floor`
arg-driven affect from map load remain absent.

Release `HCDE.Playsim.Tests`: **2,258** passed after this note.

## Audit: COMPATF_BOOMSCROLL carry averaging (2026-09-30)

`CompatSurface.BoomScroll` mirrors native `COMPATF_BOOMSCROLL`: when set, grounded
monsters **sum** scroll from multiple touching sectors; players still **average**.
Default compat keeps averaging for everyone. Split-sector tests park the player
away so carry moves are not blocked by overlap. `CallFunc_GetMaxInventory` on a
cooperative tid reads **Health** max (**200**).

Release `HCDE.Playsim.Tests`: **2,262** passed after this note.

## Audit: MF8_INSCROLLSEC and ACSF_DamageActor (2026-09-30)

`Actor.InScrollSector` mirrors native `MF8_INSCROLLSEC`: each tic,
`MarkInScrollSectorActors` sets it on grounded non-players touching an active
carry scroller (respecting `ScrollCarryAffect`); players always carry. Monsters
skip carry when the flag is clear. `CallFunc` function **201** (`ACSF_DamageActor`)
applies `ActorDamage` with six stack args (target tid, unused ptr, inflictor tid,
unused ptr, amount, damage-type string). `InternalsVisibleTo` exposes scroll
internals to playsim tests.

Release `HCDE.Playsim.Tests`: **2,266** passed after this note.

## Audit: CheckClass CallFunc and Backpack use/give (2026-09-30)

`CallFunc` function **200** (`ACSF_CheckClass`) returns whether a spawn class name
is known to `DoomActorCatalog` (not full DECORATE). `UseInventory` and
`GiveInventory` accept **Backpack** and call `PlayerInventory.GiveBackpack`.
DECORATE inventory items, keys via use, and arbitrary `PClass` lookup remain absent.

Release `HCDE.Playsim.Tests`: **2,269** passed after this note.

## Audit: CheckActorClass, TID CallFunc helpers (2026-09-30)

`CallFunc` adds **27** (`ACSF_CheckActorClass`, tid + spawn class name vs
`DoomActorCatalog`), **46** (`ACSF_UniqueTID`, linear search), and **47**
(`ACSF_IsTIDUsed`). `GiveInventoryDirect` on **Backpack** is covered. Random
`UniqueTID` when start is zero and full `PClass` class names remain absent.

Release `HCDE.Playsim.Tests`: **2,273** passed after this note.

## Audit: Backpack take, ACSF_Sqrt, invuln damage (2026-09-30)

`TakeInventory` on **Backpack** calls `PlayerInventory.RemoveBackpack` (caps and ammo
clamp). `CallFunc` **48** (`ACSF_Sqrt`) returns `floor(sqrt(n))` for non-negative
ints. `ACSF_DamageActor` respects `APROP_INVULNERABLE` (11) via existing damage
gates. `ACSF_FixedSqrt` and DECORATE `PClass` damage types remain absent.

Release `HCDE.Playsim.Tests`: **2,276** passed after this note.

## Audit: fixed ACS math CallFunc and invulnerable APROP (2026-09-30)

`CallFunc` adds **49** (`ACSF_FixedSqrt`) and **50** (`ACSF_VectorLength`) using
16.16 fixed-point conversion (`floor` to ACS). `SetAndGetActorProperty_InvulnerableFlag`
covers `APROP_INVULNERABLE` (11). HUD clip, `GetActorClass`, and random `UniqueTID`
remain absent.

Release `HCDE.Playsim.Tests`: **2,279** passed after this note.

## Audit: GetActorClass CallFunc and random UniqueTID (2026-09-30)

`CallFunc` **68** (`ACSF_GetActorClass`) returns a global ACS string id via
`AcsGlobalStrings` (`DoomPlayer` for players, catalog spawn names for known monsters,
`None` otherwise). `FindUniqueTID` with start **0** matches native random probing
(`pr_uniquetid` stream on `AuthoritySimulation`). `strcmp` and full DECORATE class
names beyond the catalog remain absent.

Release `HCDE.Playsim.Tests`: **2,282** passed after this note.

## Audit: ACS strcmp/stricmp and global string pool ids (2026-09-30)

`CallFunc` **63**/**64** (`ACSF_strcmp` / `ACSF_stricmp`) resolve module and global
pool strings (`STRPOOL_LIBRARYID_OR` encoding on `GetActorClass` results). Optional
third arg uses bounded `string.Compare`. HUD clip and `StrLeft` remain absent.

Release `HCDE.Playsim.Tests`: **2,284** passed after this note.

## Audit: ACS string slice and actor flag CallFunc (2026-09-30)

`CallFunc` **65**–**67** (`StrLeft` / `StrRight` / `StrMid`) return global pool
strings. **75** (`CheckFlag`) and **202** (`SetActorFlag`) cover a small DECORATE-style
flag name set (`INVULNERABLE`, `AMBUSH`, `FRIENDLY`, `FLOAT`, etc.). Full `FFlagDef`
tables and HUD clip remain absent.

Release `HCDE.Playsim.Tests`: **2,286** passed after this note.

## Audit: GetWeapon and CheckProximity CallFunc (2026-09-30)

`CallFunc` **69** (`ACSF_GetWeapon`) returns the ready weapon class name in the global
string pool. **98** (`ACSF_CheckProximity`) counts catalog-matched actors within a
fixed-point radius (flags / `PClass` iterators absent). `PickActor` and HUD clip
remain absent.

Release `HCDE.Playsim.Tests`: **2,288** passed after this note.

## Audit: ACS fixed Floor/Round/Ceil CallFunc (2026-09-30)

`CallFunc` **207**–**209** mirror native 16.16 quantization (`& ~0xffff`, half-up
round, ceil `+ 0x10000`). `StrArg` and map-load `DScroller` thinkers remain absent.

Release `HCDE.Playsim.Tests`: **2,289** passed after this note.

## Audit: GetActorFloorTexture CallFunc (2026-09-30)

`CallFunc` **204** (`ACSF_GetActorFloorTexture`) returns the activator/TID sector
`FloorPic` in the global string pool. **205** (`GetActorFloorTerrain`) returns an
empty global string until terrain tables exist. `StrArg` and map-load scroll thinkers
remain absent.

Release `HCDE.Playsim.Tests`: **2,290** passed after this note.

## Audit: ChangeActorAngle and GetArmorInfo CallFunc (2026-09-30)

`CallFunc` **79** (`ACSF_ChangeActorAngle`) sets BAM angle on tid matches (interpolate
ignored). **81** (`ACSF_GetArmorInfo`) returns worn `BasicArmor` classname, caps, and
fixed save percent for players. `DropInventory` and pitch/roll ACS remain absent.

Release `HCDE.Playsim.Tests`: **2,292** passed after this note.

## Audit: ChangeActorPitch CallFunc (2026-09-30)

`CallFunc` **80** (`ACSF_ChangeActorPitch`) maps ACS BAM pitch to `PitchDegrees`
(normalized ±180°, player clamp unchanged). Interpolation and `GetActorRoll` remain
absent.

Release `HCDE.Playsim.Tests`: **2,293** passed after this note.

## Audit: DropInventory CallFunc (2026-09-30)

`CallFunc` **82** (`ACSF_DropInventory`) removes the held inventory stack via
`AcsPlayerInventory.Drop` (no map pickup spawn). Keys, weapons, and ammo use existing
`Take` paths. `DropItem` and DECORATE `FindInventory` remain absent.

Release `HCDE.Playsim.Tests`: **2,294** passed after this note.

## Audit: GetActorVel ACS CallFunc (2026-09-30)

`CallFunc` **9**–**11** (`GetActorVelX` / `Y` / `Z`) return `DoubleToACS` on
`VelocityX`/`Y`/`Z` for tid matches. `GetActorRoll` and UDMF line specials remain
absent.

Release `HCDE.Playsim.Tests`: **2,295** passed after this note.

## Audit: GetActorViewHeight and GetChar CallFunc (2026-09-30)

`CallFunc` **14** (`GetActorViewHeight`) returns fixed `ViewHeight` for players and
half actor height for monsters. **15** (`GetChar`) reads a module/global string
code unit. `GetAirSupply` and `SetActorVelocity` remain absent.

Release `HCDE.Playsim.Tests`: **2,297** passed after this note.

## Audit: SetActorVelocity CallFunc (2026-09-30)

`CallFunc` **23** (`ACSF_SetActorVelocity`) applies fixed X/Y/Z to tid matches; non-add
clears `Velocity*` first (`P_Thing_SetVelocity` without player bob). `CheckActorProperty`
ACS and map-load scroll thinkers remain absent.

Release `HCDE.Playsim.Tests`: **2,298** passed after this note.

## Audit: CheckActorProperty and GetArmorType CallFunc (2026-09-30)

`CallFunc` **22** (`CheckActorProperty`) compares supported `APROP_*` values via
`AcsActorProperties.Check`. **19** (`GetArmorType`) returns worn `BasicArmor` amount
for a player index. String/sound `APROP` checks and DECORATE armor types remain absent.

Release `HCDE.Playsim.Tests`: **2,300** passed after this note.

## Audit: actor roll CallFunc (2026-09-30)

`Actor.Roll` (BAM) plus `CallFunc` **89** (`ChangeActorRoll`) and **90** (`GetActorRoll`).
`SetActorRoll` interpolation and render-facing roll are absent.

Release `HCDE.Playsim.Tests`: **2,301** passed after this note.

## Audit: CheckActorState and StrArg CallFunc (2026-09-30)

`CallFunc` **99** (`CheckActorState`) matches managed state labels (`Spawn`, `See`,
`Pain`, `Death`, `Corpse`, `Death.Extreme`) against the actor state table. DECORATE
`FindStateByString` and custom tables remain absent. **206** (`StrArg`) returns
`SpawnOptions.StrArgs` entries in the global ACS string pool.

Release `HCDE.Playsim.Tests`: **2,304** passed after this note.

## Audit: PickActor CallFunc (2026-09-30)

`CombatTrace.PickActor` implements a `P_LinePickActor` subset (BAM angle/pitch,
shootable mask, wall mask). `CallFunc` **83** assigns `ThingId` with
`PICKAF_FORCETID` / `PICKAF_RETURNTID` behavior. Portals and full `ActorFlags`
masks remain absent.

Release `HCDE.Playsim.Tests`: **2,305** passed after this note.

## Audit: CanRaiseActor CallFunc (2026-09-30)

`ActorRaise.CanRaise` mirrors archvile resurrection prechecks (corpse frame,
`RaiseDuration`, `CanOccupy`). `CallFunc` **85** returns 1 when every actor
for a TID can raise; TID **0** checks the activator only. DECORATE raise states
beyond the managed table remain absent.

Release `HCDE.Playsim.Tests`: **2,307** passed after this note.

## Audit: DropItem CallFunc (2026-09-30)

`PickupCatalog.TryEditorNumberForDropName` maps ACS inventory class names to Doom
pickup editor numbers. `ActorDropItem.Drop` spawns tossable pickups with optional
chance (default 256). `CallFunc` **74** returns the drop count. Custom `amount`
and non-catalog `PClass` actors remain absent.

Release `HCDE.Playsim.Tests`: **2,308** passed after this note.

## Audit: IsPointerEqual CallFunc (2026-09-30)

`AcsActorPointer.Resolve` implements `AAPTR_DEFAULT`, `NULL`, `TARGET` (monster
brain), `PLAYER1`–`8`, `FRIENDPLAYER`, and player line-target via `CombatTrace`.
`CallFunc` **84** compares resolved actors by id. Master, tracer, conversation,
and client-side pointer barriers remain absent.

Release `HCDE.Playsim.Tests`: **2,310** passed after this note.

## Audit: map-load carry scrollers (2026-09-30)

`ScrollCarryInitialize.ApplyMapLoadScrollers` runs at simulation start and
`AppendSectorCarryScroll` for each static `Scroll_Floor` line with carry mode
(`Arg3` &gt; 0). `Scroll_Ceiling` map lines activate only (no ceiling motion).
Control-sector scroll, texture scroll, and displacement thinkers remain absent.

Release `HCDE.Playsim.Tests`: **2,311** passed after this note.

## Audit: CheckFont CallFunc (2026-09-30)

`AcsHudFonts.Exists` answers `CallFunc` **73** for a fixed set of vanilla/HUD font
names (`SmallFont`, `BigFont`, `ConsoleFont` / `CONFONT`, etc.). WAD-backed
`V_GetFont` registration and custom fonts remain absent.

Release `HCDE.Playsim.Tests`: **2,313** passed after this note.

## Audit: line activation CallFunc (2026-09-30)

`AcsLineActivation` packs and applies `SPAC_Cross`, `SPAC_Use`, `SPAC_UseThrough`,
and `SPAC_UseBack` on linedefs matched by UDMF/Hexen `id` (`LevelLine.Tag`).
`CallFunc` **76**/**77** set/get activation; optional repeat toggles `LevelLine.Repeat`.
Monster/projectile activators and full `ML_SPAC` masks remain absent.

Release `HCDE.Playsim.Tests`: **2,314** passed after this note.

## Audit: GetActorPowerupTics and SoundVolume CallFunc (2026-09-30)

`AcsActorPowerups.RemainingTics` backs `CallFunc` **78** for `PowerBuddha` on
`PlayerPawn` only. **70** (`SoundVolume`) parses tid/channel/volume and returns
success without changing actor sound channels (audio stack absent).

Release `HCDE.Playsim.Tests`: **2,316** passed after this note.

## Audit: PlayActorSound and SpawnDecal CallFunc (2026-09-30)

`CallFunc` **71** (`PlayActorSound`) and **72** (`SpawnDecal`) accept two to six
stack arguments and return how many non-destroyed actors match the TID (activator
when TID is zero). Actor sound selectors, `S_Sound`, and decal spawn are absent.

Release `HCDE.Playsim.Tests`: **2,318** passed after this note.

## Audit: telefog and SetActorRoll CallFunc (2026-09-30)

`Actor.TeleFogSource` / `TeleFogDest` back `CallFunc` **86**/**87** via
`AcsActorTeleFog` (TID iterator; activator when TID is zero). **88**
(`SetActorRoll`) and **89** (`ChangeActorRoll`) both set absolute BAM roll;
optional interpolate is ignored. Teleport fog spawn and roll interpolation are
absent.

Release `HCDE.Playsim.Tests`: **2,321** passed after this note.

## Audit: DropItem amount CallFunc (2026-09-30)

`CallFunc` **74** third argument sets `Actor.PickupAmount` on tossed catalog
pickups; `PickupCatalog.TryGive` honors it for ammo and health gifts. Zero keeps
the catalog default. Non-catalog `PClass` drops remain absent. Managed gameplay
trace checksum is now **3995474422** (`PickupAmount` mixed into actor save hash).

Release `HCDE.Playsim.Tests`: **2,322** passed after this note.

## Audit: StopSound, SetSectorDamage, SetMusicVolume CallFunc (2026-09-30)

`CallFunc` **62** (`StopSound`) accepts tid/channel stack args and returns success
without stopping channels (audio absent). **94** (`SetSectorDamage`) shares
`SectorDamage.ApplyTagged` with line special 214. **97** (`SetMusicVolume`) writes
fixed volume to `AuthoritySimulation.MusicVolume`. `PlaySound` ACS remains absent.

Release `HCDE.Playsim.Tests`: **2,325** passed after this note.

## Audit: PlaySound, SetSectorTerrain, GetActorFloorTerrain CallFunc (2026-09-30)

`CallFunc` **61** (`PlaySound`) returns TID match count without `S_Sound`. **95**
(`SetSectorTerrain`) stores floor or ceiling terrain names on tagged
`LevelSector` rows via `SectorTerrain.ApplyTagged`. **205** (`GetActorFloorTerrain`)
returns the activator sector `FloorTerrain`. Footstep audio and WAD terrain defs
remain absent.

Release `HCDE.Playsim.Tests`: **2,327** passed after this note.

## Audit: control-sector carry scroll and SpawnParticle (2026-09-30)

Map-load <c>Scroll_Floor</c> lines with displacement bits (`Arg1` &amp; 3) register
`ControlSectorCarryScroll` thinkers that add carry from control-sector center-height
delta each tic. `CallFunc` **96** (`SpawnParticle`) accepts one to sixteen arguments
and returns success without spawning particles. Accelerative scroll and texture
`DScroller` modes remain absent.

Release `HCDE.Playsim.Tests`: **2,330** passed after this note.

## Audit: LineAttack, QuakeEx, displacement scroll activation (2026-09-30)

`CallFunc` **60** (`LineAttack`) traces from a TID source with BAM angle/pitch offsets
and applies catalog damage to the first `PickActor` hit. **91** (`QuakeEx`) accepts eight
to nineteen arguments and returns success without screen shake. Activated
`Scroll_Floor` lines with displacement bits register the same control-sector carry
thinkers as map load via `ScrollCarryInitialize.ApplyControlSectorCarryLine`. Puff
actors and quake falloff remain absent.

Release `HCDE.Playsim.Tests`: **2,333** passed after this note.

## Audit: CheckSight and Radius_Quake2 CallFunc (2026-10-01)

`CallFunc` **35** (`CheckSight`) mirrors native TID pairing: activator-as-source when
the source TID is zero, self-visible when the destination TID is zero, and
`CombatTrace.HasLineOfSight` for each source/destination pair. Water-boundary and
see-past-block-everything flag bits are accepted but not modeled yet.

`CallFunc` **26** (`Radius_Quake2`) accepts six or more stack arguments and returns
success without `P_StartQuake` (screen shake stack still absent).

Release `HCDE.Playsim.Tests`: **2,336** passed after this note.

## Audit: accelerative carry scroll and GetPolyobj CallFunc (2026-10-01)

Accelerative `DScroller::sc_carry` now matches native velocity accumulation: each tic
adds the base rate to `Vdx`/`Vdy` before applying carry. Hexen `Scroll_Floor` **mode 3**
(map load and line activation) registers accelerative scrollers; displacement
control-sector lines with **Arg1 bit 2** accumulate control-sector deltas the same way.
Constant modes 1 and 2 are unchanged. Floor/ceiling texture `DScroller` modes remain absent.

`CallFunc` **33**/**34** (`GetPolyobjX`/`GetPolyobjY`) return `FIXED_MAX` until polyobjects exist.

Release `HCDE.Playsim.Tests`: **2,340** passed after this note.

## Audit: LineAttack puff and CallFunc argc fix (2026-10-01)

`CombatTrace.TraceLineAttack` picks the nearest shootable actor or blocking wall within
range. `AcsLineAttack` applies damage only to an actor hit, always spawns a short-lived
`PuffActor` (two tics) at the impact point, and honors optional puff TID (arg 8).

`TryLineAttack` stack layout now matches native ACS: arg 4 puff class name (ignored until
DECORATE puffs exist), arg 5 damage type, arg 6 range, arg 7 flags, arg 8 puff TID.
Arg 4 had been misread as damage type.

Puff decals, blood versus puff selection, and DECORATE puff classes remain absent.

Release `HCDE.Playsim.Tests`: **2,341** passed after this note.

## Audit: SetActivator CallFunc and script activator binding (2026-10-01)

`CallFunc` **12** (`SetActivator`) resolves a TID (optional `COPY_AAPTREX` pointer when arg 2 is not
`AAPTR_DEFAULT`) and stores the actor on the running ACS fiber. **13** (`SetActivatorToTarget`) follows
native rules: live players use `CombatTrace.FindTarget`; monsters use `MonsterBrain.TargetId`.

`AcsActivatorBinding` carries the mutable activator through `TryInvoke` and `AcsVm` fibers so later
`CallFunc` calls in the same script see the updated activator. `SetActivatorByNetID` remains absent.

Release `HCDE.Playsim.Tests`: **2,343** passed after this note.

## Audit: GetLineX / GetLineY CallFunc (2026-10-01)

`CallFunc` **300** / **301** (`GetLineX` / `GetLineY`) use `AcsLineActivation.FirstLineFromId` (first
linedef with matching tag), interpolate along the segment from v1 using arg 2 as a fixed fraction,
and when arg 3 is non-zero apply a perpendicular offset of `ACSToDouble(arg 3)` using the native
normal `(delta.Y, -delta.X)`. Missing lines return 0.

Release `HCDE.Playsim.Tests`: **2,345** passed after this note.

## Audit: floor/ceiling texture DScroller (2026-10-01)

`LevelSector` stores floor and ceiling texture scroll offsets. `SectorTextureScroll` thinkers apply
`sc_floor` / `sc_ceiling` rates each tic. Map-load and activated `Scroll_Floor` lines follow native
arg3 rules: modes **0** and **2** scroll textures (`-dx`, `dy`); modes **1**/**3** clear texture
rates and keep carry-only behavior. `Scroll_Ceiling` sets ceiling texture scroll. Sector
`RotationComp` for scrolling textures and `sc_side` wall scroll remain absent.

Release `HCDE.Playsim.Tests`: **2,348** passed after this note.

## Audit: sc_side wall texture scroll (2026-10-01)

`LevelSide` tracks top/mid/bottom texture offsets. `SideTextureScroll` thinkers mirror native
`DScroller::sc_side` (per-sidedef rates and `EScrollPos` part masks). `SetWallTextureScroll` /
`Scroll_Wall` (Hexen action **52**, wired on the UDMF/Hexen activation path only) resolves linedefs
by tag via `AcsLineActivation.LinesFromId`, picks front/back from arg3, and uses fixed-point args 1–2
for rates. Mid textures skip when the owning line is two-sided with `ML_3DMIDTEX`. Control-sector and
accelerative wall scroll remain absent.

Release `HCDE.Playsim.Tests`: **2,350** passed after this note.

## Audit: sector texture RotationComp (2026-10-01)

Floor/ceiling texture scroll now applies native `RotationComp` using `LevelSector.FloorTextureAngle` /
`CeilingTextureAngle` (BAM). Zero angle leaves scroll deltas unchanged. UDMF angle import and
`baseAngle` stacking remain absent until map loader sets these fields.

Release `HCDE.Playsim.Tests`: **2,351** passed after this note.

## Audit: GetLineHealth / GetSectorHealth CallFunc (2026-10-01)

`CallFunc` **213** (`GetLineHealth`) returns the first linedef health for a matching tag/id via
`AcsLineActivation.FirstLineFromId`. **212** (`GetSectorHealth`) reads floor (0), ceiling (1), or 3D
midtex (2) health on the first tagged sector. `LevelLine` / `LevelSector` carry health fields;
`FHealthGroup` pooling is not modeled (group ids return 0 until groups exist).

Release `HCDE.Playsim.Tests`: **2,353** passed after this note.

## Audit: destructible health groups (2026-10-01)

`LevelHealthGroups.Build` mirrors `P_InitHealthGroups` startup pooling: lines and sector floor/ceiling/3D
parts seed shared group health (max when members disagree). `GetLineHealth` / `GetSectorHealth` read
pooled values when a group id is set. Line/sector damage and group depletion remain absent.

Release `HCDE.Playsim.Tests`: **2,354** passed after this note.

## Audit: GetNetID / SetActivatorByNetID CallFunc (2026-10-01)

`CallFunc` **215** (`GetNetID`) uses `AcsActorTid.SelectFromTid` (tid, index, activator) with optional
AAPTR arg3, returning `Actor.NetworkId` or **0** (`WorldNetID`) when no actor matches. **216**
(`SetActivatorByNetID`) resolves `Actor.NetworkId` on the playsim actor list and updates the script
activator binding. Live `NetworkEntityManager` assignment remains absent.

Release `HCDE.Playsim.Tests`: **2,356** passed after this note.

## Audit: UDMF destructible health and wall scroll import (2026-10-01)

`UdmfTextMapParser` / `LevelBuilder.FromUdmf` now load linedef `health` / `healthgroup`, sector
`healthfloor` / `healthceiling` / `health3d` and matching group ids, ZDoom sidedef per-part offsets,
and `xscroll` / per-part scroll keys. Map start applies UDMF wall scroll rates via
`ScrollCarryInitialize.ApplyUdmfWallScrolls`. `LevelDestructibleDamage` updates line/sector health and
pooled `HealthGroups` (line/sector death specials and hitscan destructible traces remain absent).

Release `HCDE.Playsim.Tests`: **2,358** passed; `HCDE.MapLoader.Tests`: **402** passed after this note.

## Audit: loaded UDMF floor/ceiling texture rotation (2026-10-01)

`UdmfTextMapParser` imports `rotationfloor` / `rotationceiling` in degrees.
`LevelBuilder.FromUdmf` normalizes them to a full turn and converts them into
the existing BAM texture-angle fields, following native
`src/maploader/udmf.cpp` (`CheckAngle`, `NAME_Rotationfloor`,
`NAME_Rotationceiling`). Existing `SectorTextureRotation` applies the imported
angles to scroll rates, matching `src/playsim/mapthinkers/a_scroll.cpp`.

Regression coverage includes missing values, negative angles, wrapped angles,
fractional angles and a loaded UDMF map ticking floor and ceiling scrollers.
Base-angle stacking and native runtime/rendering comparison remain pending;
the managed BAM representation also retains its existing angular quantization.

Release solution tests: **3,341** passed, including **409** MapLoader and
**2,359** Playsim tests.

## Audit and conversion: runtime sector rotation (2026-10-01)

Initial review compared UDMF import and scroll compensation against native
`src/maploader/udmf.cpp` and `src/playsim/mapthinkers/a_scroll.cpp`. The covered
angle cases matched; the remaining conversion gaps were base-angle addition
and runtime special 185.

`LevelSector` now retains separate floor/ceiling base angles. Scroll compensation
uses their sum with the mutable rotation, following `sector_t::GetAngle` in
`src/gamedata/r_defs.h`. Special **185** (`Sector_SetRotation`) replaces mutable
rotations while preserving bases, following `LS_Sector_SetRotation` in
`src/playsim/p_lnspec.cpp`. It is wired into Hexen/UDMF map activation and both
ACS direct/stack special dispatch paths. Doom binary special 185 retains its
existing crusher meaning.

The follow-up audit checked dispatch, native tag iterator behavior, map-line
consumption, preservation of bases, and effects on existing scrollers. Tag zero
selects all untagged sectors, without trigger-side fallback; missing tags still
return success, matching native. Five new regression cases cover these paths,
including cancelling base and mutable angles and observing the next scroll tic.

Release solution tests: **3,346** passed (**2,364** Playsim, **409** MapLoader).
Remaining scope: native base-angle producers such as plane alignment are not
converted; managed angle precision remains BAM rather than native double;
sector tags retain the existing single signed-16-bit representation. Visual
plane transforms still lack complete save/network/checksum coverage and native
runtime/rendering comparison. These tests do not establish full phase parity.

## Conversion and audit: line-based plane texture alignment (2026-10-01)

Converted Hexen/UDMF specials **183** (`Line_AlignCeiling`) and **184**
(`Line_AlignFloor`) against `LS_Line_AlignCeiling` / `LS_Line_AlignFloor` in
`src/playsim/p_lnspec.cpp` and `FLevelLocals::AlignFlat` in
`src/playsim/p_sectors.cpp`. Matching line IDs select front or back sectors;
the line direction determines the negative base angle and its first vertex
determines the perpendicular base Y offset. Back-side alignment reverses the
offset and adds 180 degrees before negation. Mutable texture rotations and the
other plane remain unchanged. Both ACS direct and stack paths dispatch these
actions; Doom binary crusher meanings for 183/184 are preserved.

Follow-up review checked native return/side semantics, angle/offset signs,
missing targets and integration with existing scrollers. It found the native
unset line-ID sentinel (-1), which is now explicitly rejected. Twelve new
regression cases cover floor/ceiling, front/back (including non-boolean nonzero
side), missing side/ID, direct/stack ACS and base-plus-mutable scroll behavior.

Release solution tests: **3,358** passed, including **2,376** Playsim tests.
Release build passes with zero warnings/errors. Base Y offsets are retained
for future rendering; no rendered alignment acceptance is claimed. Multi-ID
line tags, BAM precision, transform persistence/replication and other
base-angle producers remain outside this conversion slice.

## Conversion and audit: plane panning (2026-10-01)

Converted specials **186** (`Sector_SetCeilingPanning`) and **187**
(`Sector_SetFloorPanning`) against `src/playsim/p_lnspec.cpp`. Map activation
and ACS direct/stack calls replace the selected plane offsets using integer
plus fractional hundredths. Tag zero selects untagged sectors; missing tags
return native success. Base alignment and the other plane are preserved, and
existing scrollers continue from the replacement offset.

UDMF imports `xpanningfloor`, `ypanningfloor`, `xpanningceiling` and
`ypanningceiling`. Review against `src/maploader/udmf.cpp` found the namespace
gate missing from this and the earlier rotation import. Both now apply only
for ZDoom, ZDoomTranslated and Vavoom, matching the native Zd/Zdt/Va mask.

Eight panning regression cases cover replacement, signed and out-of-range
hundredths, all matching sectors, missing tags, tag zero, both ACS paths and
loaded-map scrolling. Six namespace cases cover allowed and ignored plane
fields. Release solution tests: **3,372** passed (**2,384** Playsim and **415**
MapLoader). Release build has zero warnings/errors. Rendering, transform
save/network/checksum coverage, plane scale actions and multi-tag sector
representation remain incomplete; managed tests do not establish native
runtime visual acceptance.

## Conversion and audit: plane texture scales (2026-10-01)

Converted native specials **170/171** (`Sector_SetCeilingScale2` /
`Sector_SetFloorScale2`) and **188/189** (`Sector_SetCeilingScale` /
`Sector_SetFloorScale`) from `src/playsim/p_lnspec.cpp`. Map activation and
ACS direct/stack dispatch update each selected axis to the reciprocal of its
nonzero input factor. The Scale2 variants decode signed 16.16 arguments;
the other variants combine integer plus hundredths. Zero leaves that axis
unchanged. Tag-zero behavior matches the untagged sector iterator.

UDMF plane scale keys load directly, without inversion, following
`src/maploader/udmf.cpp`; missing keys default to one. The existing native
Zd/Zdt/Va namespace gate applies. Audit checked this distinction, action IDs,
axis argument layout, signed factors, zero axes and plane isolation. Seventeen
new regression cases cover the four actions via map and both ACS paths, plus
UDMF values, namespace gating and defaults.

Release solution tests: **3,389** passed (**2,396** Playsim, **420** MapLoader).
Release build passes with zero warnings/errors. Changed tracked code passes
the whitespace check. Scale values are retained as plane transform state;
native rendering acceptance and full transform save/network/checksum coverage
remain pending. This closes the bounded scale action/import gap, not full
rendering or phase parity.

## Conversion and audit: wall texture offset action (2026-10-01)

Converted special **53** (`Line_SetTextureOffset`) against
`LS_Line_SetTextureOffset` in `src/playsim/p_lnspec.cpp`. Hexen/UDMF map
activation and both ACS special dispatch paths decode signed 16.16 offsets,
select front/back sidedefs and honor top/mid/bottom mask bits 1/2/4. Bit 8 adds
instead of replacing. Each axis independently preserves its existing value
when the native `32767 << 16` sentinel is supplied.

Audit checked part-mask behavior, signed offsets, add versus set, sentinel
handling and native returns: ID zero or a side outside 0/1 fails; a missing ID
or missing selected sidedef succeeds without mutation. The native unset ID
(-1) cannot select lines. Doom binary special 53 is unchanged because the new
dispatch is confined to the Hexen/UDMF branch.

Fifteen new regression cases exercise all part masks and add modes, front/back
isolation, direct/stack ACS, the Y sentinel and validation/no-target returns.
Release solution tests: **3,404** passed (**2,411** Playsim). Release build has
zero warnings/errors; changed tracked code passes the whitespace check.
Remaining scope includes wall texture scale action 56, multi-ID line tags,
complete wall transform persistence/replication and native rendering
acceptance. This is managed action coverage, not a completed visual client.

## Conversion and audit: wall texture scale action (2026-10-01)

Converted special **56** (`Line_SetTextureScale`) against
`src/playsim/p_lnspec.cpp` and sidedef scale setters in
`src/gamedata/r_defs.h`. Map activation and both ACS dispatch paths decode
signed 16.16 values and select top/mid/bottom parts on front/back sides.
Bit 8 multiplies existing scales, unlike offset addition. Set mode stores the
factor directly, without sector-scale inversion. Setter-level audit caught
and corrected the native zero rule: set mode replaces zero with one, while
multiply mode can produce zero. Negative factors are retained. The
`32767 << 16` sentinel preserves each axis independently.

Audit checked IDs, side validation, unset ID, missing targets, part isolation,
default-one scales and direct versus multiplication semantics. Fifteen new
regression cases cover every part mask in both modes, signed/zero factors,
back-side ACS direct/stack calls, the X sentinel and native return behavior.
Release solution tests: **3,419** passed (**2,426** Playsim). Release build
passes with zero warnings/errors; changed tracked code passes whitespace
checks. No native rendered acceptance is claimed. UDMF per-part wall scale
import, full transform save/network/checksum coverage and multi-ID line tags
remain incomplete.
