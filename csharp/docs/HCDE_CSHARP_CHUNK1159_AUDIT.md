# Chunk 1159 — Player poison and the kill/remove cheats from `p_interaction.cpp` (**1834–2052**).

## Scope

Player poison and the kill/remove cheats from `p_interaction.cpp` (**1834–2052**).

## Native ranges

- `src/playsim/p_interaction.cpp` **Poison player and kill cheats** 1834–2052 (219 lines)

**Credited native LOC:** 219 (tally `nativeLoc` 219).

## Managed

`VmPInteractionPoison1159Slice.cs`
Managed LOC (tally): 250.
Conversion commit: `5f4f048df5fbeb4a13a0fb1be65a678c39fa3183`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_PoisonPlayer` (**1834–1873**) | yes |
| `P_PoisonDamage` (**1881–1990**) | yes |
| `CCMD kill` (**1992–2033**) | yes |
| `CCMD remove` (**2035–2052**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1822–1833** | fallback return already owned by the previous function, plus the poison comment |
| `GetModifiedDamage` / `ApplyDamageFactor` / `P_AutoUseHealth` / `hasBuddha` | host-supplied results |

`CF_GODMODE` is **1<<1**. `CF_GODMODE2` is **1<<25**. `MF2_INVULNERABLE` is **0x08000000**. `MF5_NODAMAGE` is **0x40**. `MF7_LAXTELEFRAGDMG` is **0x00100000**. `TELEFRAG_DAMAGE` is **1000000**. `DF2_NOSUICIDE` is **1<<22**. `DEM_GENERICCHEAT` is **10**, `DEM_SUICIDE` is **15**, `DEM_KILLCLASSCHEAT` is **50**, and `DEM_REMOVE` is **68**. `CHT_MASSACRE` is **19** and `CHT_MASSACRE2` is **53**.

God, invulnerable, absolute god, or no-damage refuses the poison and leaves the count. A source that is not this player and is a teammate scales the dose with `(int)(poison * teamdamage)` before the sign check. A positive dose adds to the count, stores the source as the poisoner, copies the poisoner's damage type, and uses its pain type unless that name is `None`. The count then caps at **100**. A non-positive dose still returns true.

Poison damage of a missing player or a dead actor returns immediately. Telefrag is raw damage at or above **1000000** without lax telefrag, and it is decided before any modifier. Absolute god and no-damage always ignore the hit. Ordinary god and invulnerable ignore it only when it is not a telefrag. The dose is then `(int)(damage * skillFactor * playerFactor)`, replaced by the host modifier result when one is supplied. A non-positive result stops. When that damage meets or exceeds health, auto-use is allowed or this is a deathmatch, and the actor has no alternative, health is replaced by the host's post-use value and the request is `damage - health + 1`. Health then falls by the damage. Below **50** outside a deathmatch, strife health replaces it. Health below **0** becomes **0**, and the attacker is the source. Actor health falls by the same damage. At or below **0**, buddha **1** is cleared by a telefrag; any remaining buddha sets both healths to **1**. Otherwise `special1` is the damage. With no alternative, a `Fire` poison, actor health above **-50**, and damage above **25** sets the damage type to `Fire`; any other death copies the poison type. The actor then dies and pain is skipped. A survivor plays pain only when `(levelTime & 63) == 0`, the sound is requested, and a pain state exists. The commented random pain block is not live.

`kill monsters` and `kill baddies` write generic cheat **19** or **53** when cheat mode allows it, matching the word without case. Any other class writes kill-class plus that string. A bare `kill` writes suicide unless `DF2_NOSUICIDE` is set, and it does not consult cheat mode. `remove` with one argument writes remove plus that string, or prints usage for any other argument count. A sent command hides the console.

| Managed (`VmPInteractionPoison1159Slice.cs`) | Native (`p_interaction.cpp`) |
|----------------------------------------------|-------------------------------|
| Poison, poison damage, kill, remove | **1834–2052** |
| Band hull (exclusive slice) | **1834–2052** |

### Chunk **1159** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **219** |
| **Net headline** | **+219** vs post-**1158** |


**Post-review ratio:** 250/219 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionPoison1159SliceTests.Poison_ScalesTeamDamageAndCapsAtOneHundred`
- `VmPInteractionPoison1159SliceTests.Damage_BypassesGodForATelefragAndSavesABuddha`
- `VmPInteractionPoison1159SliceTests.KillAndRemove_WriteTheCheatBytesAndRefuseSuicide`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1159 --to 1159` (0 hard / 0 soft).

## Seams

Damage modifiers, inventory auto-use, strife health, buddha, and pain-state lookup stay host-supplied.

## Credit rationale

UNCredited **`p_interaction.cpp`** **1834–2052** (+219), through the end of the 2052-line file, with no conversion or recognition overlap. Lines **1822–1833** stay uncredited (the fallback return and the poison comment).
