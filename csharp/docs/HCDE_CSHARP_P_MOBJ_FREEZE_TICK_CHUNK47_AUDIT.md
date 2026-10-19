# P_MobjThinker freeze gate — chunk 47 audit

**Native:** `p_mobj.cpp` `AActor::Tick` `freezetics` gate 4561–4565.

**Managed:** `ActorFreezeTickGate.cs`, `Actor.Tick` hook, `ActorFreezeTickGateTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
