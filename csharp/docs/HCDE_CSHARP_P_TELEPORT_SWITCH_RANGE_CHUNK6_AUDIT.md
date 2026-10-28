# p_teleport remainders and P_CheckSwitchRange — chunk 6 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `P_CheckSwitchRange` | 98–234 | `SwitchActions.CheckUseRange` + `ActivateMapLine` gate |
| `SelectTeleDest` random TID | 274–340 | `TeleportActivator` + `NextTeleportRandom` |

**Behavior native LOC credited:** 378.

## Tests

`SwitchCheckRangeTests`, `TeleportRandomDestTests`, existing `TeleportDestinationSelectionTests`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
