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

## Candidate validation — 2026-11-21

The current integration candidate is PR [#42](https://github.com/TrentBest/FSM_API/pull/42), head `9aa8345c57e19e2ace5dc235a0045d51f4755034`, based on `development` with **109 commits ahead and 0 behind**. The PR remains open and draft.

Latest full candidate workflow: [run #1084](https://github.com/TrentBest/FSM_API/actions/runs/44894528903).

- Dependency restore: passed.
- Solution build: passed across the six library target frameworks: `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`.
- .NET 10 test suite: **539 passed, 0 failed, 0 skipped**.
- Build diagnostics after the documentation cleanup: **0 warnings, 0 errors**.
- NuGet package artifact: generated and uploaded.
- DLL artifacts: generated and uploaded.
- Documentation link validation: passed.
- DocFX generation and documentation artifact upload: passed.
- `publish_nuget`: skipped; no publication was performed.
- `sith_branch_policy`: skipped because this was a pull-request run, not an authorized publication dispatch.

The earlier run #1069 emitted 327 warnings (predominantly missing XML documentation); that issue was addressed by removing unnecessary XML-doc generation from the test assembly and documenting the remaining public library members/parameter. Runs #1074 and #1077 confirm zero build warnings. Codecov's latest processed patch check on the same code/test snapshot reports **88.64% diff coverage against a 68.10% target** (check passed). Forty-one changed lines remain unhit, so the passing threshold should not be mistaken for complete branch coverage. The upload step continues on error and does not make coverage a hard workflow gate.

The project file still declares package version `1.0.13`. The generated package is a CI artifact only; this candidate has not been assigned final 2.0.0 release metadata and has not been published.

## Follow-up parity changes — 2026-11-25 (validation pending)

After the previously recorded candidate workflow, the integration branch received additional parity work:

- The concise `FSM_API.FsmApi.Interact` facade now exposes matching per-handle operations for both representations: `UpdateInstance`, `EvaluateConditions`, `TransitionTo`, and `ResetInstance`.
- Integer definition construction and `FSMBuilderInt.WithProcessRate` now normalize rates below `-1` to `0` (event-driven) and emit a diagnostic, matching the string-backed creation contract.
- Added paired facade tests and integer process-rate boundary tests.

These commits have **not yet been validated by a fresh CI run**. The previously cited run #1084 predates these changes and cannot be used as their test evidence.

A separate documentation draft, [PR #43](https://github.com/TrentBest/FSM_API/pull/43), adds the theory guide and GUI.SVG-based diagram generator/workflow. The generated SVG snapshots and visual review are still pending; do not count the visuals work as complete until the workflow produces the four assets and those images are inspected.

## Remaining 2.0 release gates

1. Complete the full source/API review of the 98-commit integration candidate; the passing CI run is not a substitute for reviewing the complete diff.
2. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, destruction, and error thresholds/policy. A first direct cross-runtime test now verifies repeated callback failures, instance-threshold removal, definition-threshold unregistration, and error-counter cleanup; multi-handle/threshold interactions and diagnostic-event semantics still need review.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts, including the integer direct-array boundary.
4. Produce reproducible 2.0 performance evidence. The companion [FSM_API_Benchmark PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) contains a broad benchmark suite but is still a draft targeting that repository's `development`, and its project currently references the published FSM_API 1.0.13 package. It does not yet provide equivalent string-backed-versus-integer-backed measurements for this candidate. Align the suite with the exact 2.0 source/package and record environment/provenance before making performance claims.
5. Verify the intended compatibility matrix with clean consumer restores/tests. The library currently builds `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`; the automated test project currently runs on .NET 10 only. Confirm older .NET Framework/.NET Core consumers through the appropriate `netstandard2.0` asset and explicitly decide whether the direct `net47` target is sufficient for the intended legacy reach.
6. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations. The current CI verifies link integrity and DocFX generation, not completeness or semantic accuracy of every public member.
7. Prepare final 2.0.0 package metadata, release notes, migration guidance, and package-content review. The project file still declares version `1.0.13`; no version bump or publication has been performed.
8. Reconcile the 23-branch live inventory to the two enduring branches. The publication gate is not currently satisfiable; preserve the branch ledger and obtain owner approval for any eventual batch deletion.
9. Review all evidence and obtain explicit owner authorization before merge to `master`, release, or NuGet publication.

## Branch and publication gate

The live GitHub branch inventory checked on 2026-11-21 contains **23 branches**: `master`, `development`, and 21 additional branches. The canonical two-branch policy is therefore not satisfied. The current candidate workflow skipped its Sith branch-policy job because it was a pull-request run. No surplus branch has been deleted as part of this review.

Publication must remain blocked until unique branch work is reconciled, a fresh live inventory proves that exactly `master` and `development` exist, and explicit owner authorization is independently provided. PRs #40 and #41 have had their relevant changes ported into #42, but their source branches remain present and must not be deleted until the reconciliation is reviewed and the owner approves batch cleanup.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
