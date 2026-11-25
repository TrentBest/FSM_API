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

The current integration candidate is PR [#42](https://github.com/TrentBest/FSM_API/pull/42), head `56a7da6ff985ecbb8b30aa75666683949601dd9f`, based on `development`. The PR remains open and draft.

Latest PR #42 validation for the integer-move candidate: [run #1105](https://github.com/TrentBest/FSM_API/actions/runs/45570660734).

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

The earlier run #1069 emitted 327 warnings (predominantly missing XML documentation); that issue was addressed by removing unnecessary XML-doc generation from the test assembly and documenting the remaining public library members/parameter. Run #1093 confirms zero build warnings and errors. Coverage upload succeeded; the latest processed patch percentage should be read from Codecov for this exact head before making a coverage claim.

The project file still declares package version `1.0.13`. The generated package is a CI artifact only; this candidate has not been assigned final 2.0.0 release metadata and has not been published.

## Follow-up parity changes — 2026-11-25 (validated)

After the previously recorded candidate workflow, the integration branch received additional parity work:

- The concise `FSM_API.FsmApi.Interact` facade now exposes matching per-handle operations for both representations: `UpdateInstance`, `EvaluateConditions`, `TransitionTo`, and `ResetInstance`.
- Integer definition construction and `FSMBuilderInt.WithProcessRate` now normalize rates below `-1` to `0` (event-driven) and emit a diagnostic, matching the string-backed creation contract.
- Added paired facade tests and integer process-rate boundary tests.

These changes are validated by run #1094 on the current PR head: **558/558 tests passed**, with **0 warnings and 0 errors**. The run also generated and uploaded package/DLL artifacts; NuGet publication remained skipped.

A separate documentation draft, [PR #43](https://github.com/TrentBest/FSM_API/pull/43), adds the theory guide and GUI.SVG-based diagram generator/workflow. The dedicated workflow [run #45566774697](https://github.com/TrentBest/FSM_API/actions/runs/45566774697) succeeded and generated all four SVGs. A subsequent generator correction expanded the viewBox from 16:9 to 5:4 so every authored element fits; the workflow regenerated and committed all four corrected snapshots at `0d63e40925de914bef3e09992e301aa7b1cdf740`. Each SVG now declares a matching `1280×1024` canvas and `100×80` viewBox. Browser-level visual review remains an editorial check.

## Remaining 2.0 release gates

1. Complete the full source/API review of the 122-commit integration candidate; the passing CI run is not a substitute for reviewing the complete diff.
2. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, destruction, and error thresholds/policy. The integer runtime now exposes per-handle movement to a compatible target FSM ID, with callback-requested moves deferred until the full tick boundary. The new regression suite covers compatible/incompatible targets, unknown targets, context/state preservation, last-request-wins behavior, and prevention of same-tick double execution. The remaining parity audit includes multi-handle/threshold interactions and diagnostic-event semantics.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts, including the integer direct-array boundary.
4. Produce reproducible 2.0 performance evidence. The companion [FSM_API_Benchmark PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) contains a broad benchmark suite but is still a draft targeting that repository's `development`, and its project currently references the published FSM_API 1.0.13 package. It does not yet provide equivalent string-backed-versus-integer-backed measurements for this candidate. Align the suite with the exact 2.0 source/package and record environment/provenance before making performance claims.
5. Verify the intended compatibility matrix with clean consumer restores/tests. The library currently builds `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`; the automated test project currently runs on .NET 10 only. Confirm older .NET Framework/.NET Core consumers through the appropriate `netstandard2.0` asset and explicitly decide whether the direct `net47` target is sufficient for the intended legacy reach.
6. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations. The current CI verifies link integrity and DocFX generation, not completeness or semantic accuracy of every public member.
7. Prepare final 2.0.0 package metadata, release notes, migration guidance, and package-content review. The project file still declares version `1.0.13`; no version bump or publication has been performed.
8. Reconcile the 23-branch live inventory to the two enduring branches. The publication gate is not currently satisfiable; preserve the branch ledger and obtain owner approval for any eventual batch deletion.
9. Review all evidence and obtain explicit owner authorization before merge to `master`, release, or NuGet publication.

## Branch and publication gate

The live GitHub branch inventory checked on 2026-11-25 contains **24 branches**: `master`, `development`, and 22 additional branches, including the active documentation branch. The canonical two-branch policy is therefore not satisfied. The candidate pull-request workflow skips its dedicated publication-gate job by design. The separate push-triggered Sith Policy workflow ran successfully and refreshed the branch badge; the live inventory still confirms that the two-branch condition is not met. No surplus branch has been deleted as part of this review.

Publication must remain blocked until unique branch work is reconciled, a fresh live inventory proves that exactly `master` and `development` exist, and explicit owner authorization is independently provided. PRs #40 and #41 have had their relevant changes ported into #42, but their source branches remain present and must not be deleted until the reconciliation is reviewed and the owner approves batch cleanup.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
