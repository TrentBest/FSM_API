# Renderer LLM — FSM_API source integration finding

## 2026-10-18: DestroyInstance throws while disposing Renderer computation machine

Please investigate this failure in FSM_API `development`:

- Consumer: [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer)
- Failing source-integration run: [Renderer workflow 39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875)
- Test: `TheSingularityWorkshop.Renderer.Tests.ObserverRepresentationCoordinatorTests.Evaluate_InvalidatesSelectedBucketWhenObserverRevisionChanges`
- Exception: `System.ArgumentOutOfRangeException` — index out of range.
- Stack: `System.Collections.Generic.List<T>.RemoveAt` -> `List<T>.Remove(T item)` -> `TheSingularityWorkshop.FSM_API.FSM_API.Internal.DestroyInstance(FSMHandle handle)` at `FSM_API.Internal.cs:254` -> `FSM_API.Interaction.DestroyInstance` at `FSM_API.Interaction.cs:522` -> Renderer `RendererComputationMachine.Dispose()` -> `ObserverRepresentationCoordinator.Dispose()`.

The Renderer source restore and build passed; only its source-integration test step failed. Renderer creates one FSM instance per computation machine and calls `DestroyInstance` once from idempotent disposal. Please inspect the internal instance-list / handle lookup / removal invariant and determine why `List<T>.Remove` reaches `RemoveAt` with an invalid index. The problem may involve a stale handle, a mismatched equality/index lookup, or registry mutation, but those are hypotheses until source inspection confirms the cause.

Please reproduce against FSM_API's own tests and/or the Renderer source-integration suite. Fix and test the defect in FSM_API if confirmed. Avoid a workaround in Renderer that suppresses disposal errors or changes the rendering API.

Renderer-side coordination note: https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/RendererLLM.md
