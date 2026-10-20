# P_ZMovement smooth step-up view — chunk 63 audit

**Native:** `p_mobj.cpp` `P_ZMovement` 2965–2969.

**Managed:** `ActorPhysics.ApplySmoothStepUpView` hooked from `StepCore`; `PZMovementSmoothStepViewTests`.

Remaining `P_ZMovement` body (2961–3480) stays recognition/`ActorPhysics` except this pre-step view slice.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
