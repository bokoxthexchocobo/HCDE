# thingdef actor flags and ParseActorProperty routing — chunk 22 audit

## Scope

Port native actor flag mutation (`ModActorFlag`, `CheckActorFlag`) and DECORATE `ParseActorProperty` name routing (builtin scalars, state labels, signed flag properties) without ZScript `FindProperty` / `DispatchScriptProperty`.

**Native:**

- `thingdef_properties.cpp` `ModActorFlag` (FFlagDef) 76–122
- `thingdef_properties.cpp` `ModActorFlag` (by name) 132–189
- `thingdef_properties.cpp` `CheckActorFlag` 197–248
- `thingdef_parse.cpp` `ParseActorProperty` 943–996

**Managed:** `DecorateActorFlagModifier.cs`, `DecorateActorPropertyRouter.cs`, `DecorateActorFlagAndPropertyTests`.

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
