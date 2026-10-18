# Renderer LLM — FSM_API source integration finding

## 2026-10-18: DestroyInstance throws while disposing Renderer computation machine

Please investigate this failure in FSM_API `development`:

- Consumer: [TheSingularityWorkshop.Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer)
- Failing source-integration run: [Renderer workflow 39086967875](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/actions/runs/39086967875)
- Test: `TheSingularityWorkshop.Renderer.Tests.ObserverRepresentationCoordinatorTests.Evaluate_InvalidatesSelectedBucketWhenObserverRevisionChanges`
- Exception: `System.ArgumentOutOfRangeException` — index out of range.
- Stack: `System.Collections.Generic.List<T>.RemoveAt` -> `List<T>.Remove(T item)` -> `TheSingularityWorkshop.FSM_API.FSM_API.Internal.DestroyInstance(FSMHandle handle)` at `FSM_API.Internal.cs:254` -> `FSM_API.Interaction.DestroyInstance` at `FSM_API.Interaction.cs:522` -> Renderer `RendererComputationMachine.Dispose()` -> `ObserverRepresentationCoordinator.Dispose()`.

### Source inspection update

On current `development`, `Internal.DestroyInstance` directly mutates `bucket.Instances` through `List<FSMHandle>.Remove(handle)`, then calls `DestroyHandle()`. This method has no visible lock and does not queue the removal. By contrast, public `Interaction.MoveInstance` explicitly queues its structural mutation through `Internal.QueueDeferredModification`.

Microsoft documents that `List<T>` instance members are not thread-safe and concurrent modifications require caller-provided synchronization: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1

This makes overlapping registry mutation a concrete candidate explanation: `List<T>.Remove` can observe an index that becomes stale if another thread changes the list during the operation. It is **not yet proven** that this is the exact cause. We still need to establish whether Renderer calls disposal concurrently with ticking/other structural operations, whether FSM_API promises cross-thread direct destruction, and whether another mutation path can race even with correct consumer use.

### Requested investigation

1. Reproduce using the Renderer source-integration test and/or a focused FSM_API concurrency test.
2. Inspect all writes to `FsmBucket.Instances` and their synchronization/deferred-mutation contracts, including create, destroy, move, invalid-handle cleanup, and definition destruction.
3. Determine whether Renderer is violating an explicit lifecycle/threading contract or whether the public operation needs synchronization/deferred execution consistent with FSM_API's documented thread-safe deferred-execution model.
4. Fix the owning layer only after the reproduction clarifies the contract; add a regression test and rerun FSM_API plus Renderer source-integration suites.

Do not suppress the exception in Renderer or change its public rendering API as a workaround.

Renderer-side coordination note: https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/RendererLLM.md
