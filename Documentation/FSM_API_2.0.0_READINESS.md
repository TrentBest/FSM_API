# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-25  
**Candidate branch:** `fix/integer-definition-reregistration`  
**Candidate PR:** [#42](https://github.com/TrentBest/FSM_API/pull/42) — open, draft, targeting `development`

## Public API migration

**Status: Implemented on development**

- The original string-backed public implementation is now `TheSingularityWorkshop.FSM_API.FsmApi`.
- Its partial implementation and timer surface are consolidated under `FsmApi`.
- The concise `FSM_API.FsmApi` namespace facade forwards to that authoritative implementation.
- `Create`, `Interaction`, `Error`, `Internal`, and explicit `Transfer` are exposed through the established namespace; the concise namespace exposes `Create`, `Interact`, `Transfer`, and `Error`.
- Integer-backed execution remains a separate runtime with its own registry and identity model.
- The repository name, NuGet package ID, and established namespaces have not been renamed.

## Candidate validation — 2026-11-25

The current integration candidate is PR [#42](https://github.com/TrentBest/FSM_API/pull/42), head `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9`, based on `development`. The PR remains open and draft.

Latest PR #42 validation: [run #1109](https://github.com/TrentBest/FSM_API/actions/runs/45571547836) on head `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9`.

- Dependency restore: passed.
- Solution build: passed across the six library target frameworks: `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`.
- .NET 10 test suite: **558 passed, 0 failed, 0 skipped**.
- Build diagnostics: **0 warnings, 0 errors**.
- NuGet package artifact: generated and uploaded.
- DLL artifacts: generated and uploaded.
- Documentation link validation: passed.
- DocFX generation and documentation artifact upload: passed.
- `publish_nuget`: skipped; no publication was performed.
- `sith_branch_policy`: skipped because this was a pull-request run, not an authorized publication dispatch.

The earlier run #1069 emitted 327 warnings (predominantly missing XML documentation); that issue was addressed by removing unnecessary XML-doc generation from the test assembly and documenting the remaining public library members/parameters. Run #1109 confirms zero build warnings and errors. Coverage upload succeeded; no coverage percentage is claimed here.

The project file still declares package version `1.0.13`. The generated package is a CI artifact only; this candidate has not been assigned final 2.0.0 release metadata and has not been published.

## Follow-up parity changes — 2026-11-25 (validated)

After the previously recorded candidate workflow, the integration branch received additional parity work:

- The concise `FSM_API.FsmApi.Interact` facade now exposes matching per-handle operations for both representations: `UpdateInstance`, `EvaluateConditions`, `TransitionTo`, and `ResetInstance`.
- Integer definition construction and `FSMBuilderInt.WithProcessRate` now normalize rates below `-1` to `0` (event-driven) and emit a diagnostic, matching the string-backed creation contract.
- Added paired facade tests and integer process-rate boundary tests.
- Added integer-backed per-handle movement to a compatible target FSM ID, with callback-requested moves committed after the complete tick; the concise facade exposes the same operation.

The integer-move implementation was validated by [run #1106](https://github.com/TrentBest/FSM_API/actions/runs/45571015161) on head `73f6236f2f6c119ab88fb33e9d92cc1e81092903`: **558/558 tests passed**, with **0 warnings and 0 errors**. The latest follow-up head `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9` was revalidated by [run #1109](https://github.com/TrentBest/FSM_API/actions/runs/45571547836): **558/558 tests passed**, **0 warnings**, **0 errors**, all six library target frameworks built, and documentation links/DocFX generation passed. NuGet publication remained skipped.

A separate documentation draft, [PR #43](https://github.com/TrentBest/FSM_API/pull/43), adds the theory guide and GUI.SVG-based diagram generator/workflow. The dedicated workflow [run #45566774697](https://github.com/TrentBest/FSM_API/actions/runs/45566774697) succeeded and generated all four SVGs. A subsequent generator correction expanded the viewBox from 16:9 to 5:4 so every authored element fits; the workflow regenerated and committed all four corrected snapshots at `0d63e40925de914bef3e09992e301aa7b1cdf740`. Each SVG now declares a matching `1280×1024` canvas and `100×80` viewBox. Browser-level visual review remains an editorial check.

## String/integer representation and performance evidence — 2026-11-25

The two representations are intentionally distinct. The string API preserves name-oriented authoring and diagnostics; the integer runtime uses caller-supplied FSM/group IDs and array-indexed FSM/state storage. The explicit compiler bridge transfers definitions and returns state-name mappings; it does not transfer live handles. Its ordinal-sorted state IDs are deterministic for the same exact state set, but can shift when that set changes, so they are not durable persistence IDs without a preserved/versioned mapping.

The integer runtime's direct-indexed lookups are not proof that the whole scheduler is constant-time: a group update scans the definition array and scans the tick's handle snapshot for each eligible definition. The paired benchmark matrix must measure that scheduler behavior separately from narrow lookup costs. The full contract and future adapter boundary are documented in [String and integer backing](User%20Guide/12_String_and_Integer_Backing.md).

## Remaining 2.0 release gates

1. Complete the full source/API review of the 134-commit integration candidate; the passing CI run is not a substitute for reviewing the complete diff.
2. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, destruction, and error thresholds/policy. The integer runtime now exposes per-handle movement to a compatible target FSM ID, with callback-requested moves deferred until the full tick boundary. The new regression suite covers compatible/incompatible targets, unknown targets, context/state preservation, last-request-wins behavior, and prevention of same-tick double execution. The remaining parity audit includes multi-handle/threshold interactions and diagnostic-event semantics.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts, including the integer direct-array boundary.
4. Produce reproducible 2.0 performance evidence. The companion [FSM_API_Benchmark PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) contains a .NET 10 source-linked 2.0 paired project alongside the historical 1.x package-based suite. Its initial cases compare no-transition scheduler updates (1/10/50 definitions × 1/10/50 instances) and successful public identity lookup. [Validation run #23](https://github.com/TrentBest/FSM_API_Benchmark/actions/runs/45576383255) built both projects, generated the API inventory, and passed both Dry smoke tests; the calibrated measurement job was skipped. The workflow now accepts `fsm_api_ref` so a manual run can pin a branch, tag, or exact commit SHA; the latest smoke run still checked out FSM_API `development`, not this PR #42 candidate head. No 2.0 timing or integer speedup has been measured. The public-update pair also includes a known wrapper asymmetry: string `Update(string)` starts/stops a Stopwatch and checks a warning threshold, while integer `Update(int)` forwards directly to the runtime. A separate scheduler-core comparison is required before attributing a public-update delta solely to integer backing. Next: run the paired matrix against exact source SHA `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9` (or the selected release candidate), preserve raw results and provenance, and expand coverage to transitions, construction, mutation, lifecycle, and transfer before any performance conclusion. [Benchmark PR #5](https://github.com/TrentBest/FSM_API_Benchmark/pull/5) adds the stable-ID catalog and report-only regression policy; it is also open and draft.
5. Verify the intended compatibility matrix with clean consumer restores/tests. The library currently builds `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`; the automated test project currently runs on .NET 10 only. Confirm older .NET Framework/.NET Core consumers through the appropriate `netstandard2.0` asset and explicitly decide whether the direct `net47` target is sufficient for the intended legacy reach.
6. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations. The current CI verifies link integrity and DocFX generation, not completeness or semantic accuracy of every public member.
7. Prepare final 2.0.0 package metadata, release notes, migration guidance, and package-content review. The project file still declares version `1.0.13`; no version bump or publication has been performed.
8. Reconcile the 24-branch live inventory to the two enduring branches. The publication gate is not currently satisfiable; preserve the branch ledger and obtain owner approval for any eventual batch deletion.
9. Review all evidence and obtain explicit owner authorization before merge to `master`, release, or NuGet publication.

## Branch and publication gate

The live GitHub branch inventory checked on 2026-11-25 contains **24 branches**: `master`, `development`, and 22 additional branches, including the active documentation branch. The canonical two-branch policy is therefore not satisfied. The candidate pull-request workflow skips its dedicated publication-gate job by design. The separate push-triggered Sith Policy workflow ran successfully and refreshed the branch badge; the live inventory still confirms that the two-branch condition is not met. No surplus branch has been deleted as part of this review.

Publication must remain blocked until unique branch work is reconciled, a fresh live inventory proves that exactly `master` and `development` exist, and explicit owner authorization is independently provided. PRs #40 and #41 have had their relevant changes ported into #42, but their source branches remain present and must not be deleted until the reconciliation is reviewed and the owner approves batch cleanup.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
