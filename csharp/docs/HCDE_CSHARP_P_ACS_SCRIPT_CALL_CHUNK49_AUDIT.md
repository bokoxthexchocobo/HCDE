# ScriptCall — chunk 49 audit

**Native:** `p_acs.cpp` `DLevelScript::ScriptCall` 5310–5431.

**Managed:** `AcsScriptCallService.cs`, `AcsScriptCallRegistry.cs`, `AcsScriptCallServiceTests`.

Registry-backed static script calls mirror native argument coercion (int, bool, fixed double, string pool returns). Full `PClass` / VM dispatch is not wired; unregistered class/function pairs no-op with zero like the prior stub.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
