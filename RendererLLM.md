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


## 2026-10-18: Concurrency contract needs explicit verification

Further source inspection shows that `Internal.DestroyInstance` immediately executes `bucket.Instances.Remove(handle)`; it does not use the deferred-mutation queue. The README currently makes broader claims that structural modifications such as destroying an FSM are queued until after the tick, while the public `Interaction.DestroyInstance` directly calls the internal method. By contrast, `Interaction.MoveInstance` explicitly queues its mutation via `QueueDeferredModification`.

The exception occurring inside `List<T>.Remove` at `List<T>.RemoveAt` is consistent with the list being concurrently mutated between index lookup and removal; it is not, by itself, proof of the exact competing operation or proof that the caller violated the contract. Please inspect every path that writes to `FsmBucket.Instances` (creation, tick/error cleanup, destroy-definition, move, and explicit destroy), inspect the deferred queue implementation and tick boundary, and establish the actual concurrency guarantees. The README contract and implementation must agree.

Do not assume this can be fixed by adding a lock only to `DestroyInstance`: all readers and writers of the shared registry/list must follow one coherent synchronization or deferred-mutation policy. Add a deterministic regression test for the demonstrated cause and run the FSM_API suite plus Renderer source integration before marking verified.
