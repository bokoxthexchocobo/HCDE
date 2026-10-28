# p_pusher and a_lighttransfer — chunk 6 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `DPusher::Tick` wind/current | 149–299 | `PusherActions` |
| `FLevelLocals::AdjustPusher` | 307–351 | specials 218 / 220 / 227 |
| `DLightTransfer` | 45–95 | `LightTransferActions` |

**Behavior native LOC credited:** 524.

## Tests

`PusherWindTests`, `LightTransferActionsTests`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
