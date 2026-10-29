![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API Performance Evidence and Benchmark Contract

**Owner:** The Singularity Workshop  
**Status (2026-10-29):** documentation baseline; performance gates are not yet active.  
**Related implementation:** [FSM_API_Benchmark](https://github.com/TrentBest/FSM_API_Benchmark)  
**Benchmark coverage catalog:** [stable IDs and coverage status](https://github.com/TrentBest/FSM_API_Benchmark/blob/development/Documentation/BenchmarkCatalog.md)  
**Regression policy:** [staged comparison and gate policy](https://github.com/TrentBest/FSM_API_Benchmark/blob/development/Documentation/RegressionPolicy.md)

## Purpose

FSM_API is core technology. Performance changes must be measured against reproducible evidence, not intuition or a single noisy timing. This document separates the existing large-simulation stress-test narrative from controlled operation-level microbenchmarks and establishes the evidence contract for future changes.

## Two different kinds of evidence

### Large-simulation stress test

[`FSM_API/InitialBenchmark.md`](../../FSM_API/InitialBenchmark.md) records a historical predator/prey simulation with high agent counts and time-sliced updates. It is useful as a record of the workload and historical observations, but its published table does not provide enough environment, source revision, run identity, or raw result provenance to serve as a strict per-commit regression gate without further verification.

Do not compare these FPS or memory-delta values directly with BenchmarkDotNet nanosecond/op or allocated-bytes/op values. They answer different questions and use different workload boundaries.

### Operation-level microbenchmarks

The separate [FSM_API_Benchmark repository](https://github.com/TrentBest/FSM_API_Benchmark) currently references the published FSM_API package version 1.0.13 and targets .NET 8. Its checked-in source currently contains two BenchmarkDotNet entry points:

- Single-FSM update with callbacks and transition predicates.
- Multiple processing-group updates with 1, 10, and 50 groups.

These provide a useful starting point, not complete API coverage. The benchmark catalog marks additional scenarios uncovered until they are implemented and measured. Existing `.diagsession` files are preserved as historical artifacts; their presence alone does not make them normalized or comparable across environments.

## Stable benchmark identity

Every measured operation receives a semantic ID of the form:

`FSMAPI.<Area>.<Operation>.<Scenario>`

Examples:

- `FSMAPI.Execution.Update.SingleFsmTick`
- `FSMAPI.Execution.Update.ManyGroups`
- `FSMAPI.Builder.BuildDefinition.Empty`
- `FSMAPI.Lookup.State.ById.Hit`
- `FSMAPI.Security.InvalidInput.Reject`

The ID stays stable across implementation refactors. Workload parameters, commit SHA, package version, environment, and run identity are metadata. If the semantics genuinely change, record an explicit migration instead of silently joining incompatible history.

## Evidence required for each run

- FSM_API package/version and exact source commit where known.
- Benchmark-suite commit and stable benchmark ID.
- Runtime/SDK, OS, architecture, CPU, configuration, and relevant execution context.
- Exact workload dimensions and fixture/setup behavior.
- Distribution and uncertainty data, throughput where meaningful, allocation and GC data where meaningful.
- Compatible baseline run, absolute and percentage delta, and report/gate decision.
- Immutable raw artifact and normalized CSV/Markdown report; reruns must have distinct identities.

Unknown provenance must be recorded as unknown. Never fill it with guesses.

## Regression handling

The initial mode is report-only. Characterize same-environment variation before activating failure gates. Once calibrated, a credible, material slowdown beyond an operation-specific budget should block the performance check unless an explicit review documents a justified correctness or security tradeoff.

A security or correctness requirement must not be removed to improve a score. Benchmark valid, invalid, boundary, and adversarial paths as applicable; document the cost and rationale, then optimize without weakening the requirement.

Use multiple workload sizes to detect suspicious growth-rate changes. Scaling measurements are evidence about observed growth, not proof of a formal asymptotic complexity guarantee.

## Sith / release discipline

- Preserve the published 1.0.13 package; this work does not authorize a NuGet publish.
- Keep `master` and `development` as the two enduring branches.
- Reconcile and ledger unique branch work before any retirement. The user performs surplus-branch deletion; this document is not permission to delete.
- Do not merge benchmark or documentation changes merely because they exist; review diffs, run relevant validation, and keep release publication explicitly gated.

The current branch reconciliation record is [Documentation/BRANCH_RECONCILIATION.md](../BRANCH_RECONCILIATION.md). The benchmark implementation work is tracked by [FSM_API issue #24](https://github.com/TrentBest/FSM_API/issues/24) and [FSM_API_Benchmark issue #4](https://github.com/TrentBest/FSM_API_Benchmark/issues/4).
