# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-26  
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

- Per-handle execution and lifecycle operations remain on the concrete string-backed and integer-backed handle types; the concise `FSM_API.FsmApi.Interact` facade does not forward a complete matching set of per-handle methods.
- Integer definition construction and `FSMBuilderInt.WithProcessRate` now normalize rates below `-1` to `0` (event-driven) and emit a diagnostic, matching the string-backed creation contract.
- Added paired facade tests and integer process-rate boundary tests.
- Integer live-instance movement is implemented on the public concrete `IntegerBacked.FSMRuntime.MoveInstance(FSMHandleInt, targetFSMID)` surface and covered by dedicated runtime tests. It is not forwarded by the concise integer interaction facade; this is an API-surface asymmetry, not a missing runtime capability.

The integer-move implementation was validated by [run #1106](https://github.com/TrentBest/FSM_API/actions/runs/45571015161) on head `73f6236f2f6c119ab88fb33e9d92cc1e81092903`: **558/558 tests passed**, with **0 warnings and 0 errors**. The latest follow-up head `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9` was revalidated by [run #1109](https://github.com/TrentBest/FSM_API/actions/runs/45571547836): **558/558 tests passed**, **0 warnings**, **0 errors**, all six library target frameworks built, and documentation links/DocFX generation passed. NuGet publication remained skipped.

A separate documentation draft, [PR #43](https://github.com/TrentBest/FSM_API/pull/43), adds the theory guide and GUI.SVG-based diagram generator/workflow. The earlier PR head `8465b4c82729193f3acedff9397c7b828880df76` is historical; the current PR head is recorded in the 2026-11-26 validation refresh below.

- Living visuals workflow [run #21](https://github.com/TrentBest/FSM_API/actions/runs/45728782885) succeeded on that exact head. It checked out the pinned GUI.SVG source, generated and committed the SVG snapshots, and uploaded the visuals artifact.
- Pages build [run #60](https://github.com/TrentBest/FSM_API/actions/runs/45728783381) succeeded on the same head. GUI.SVG generation, documentation-link validation, DocFX generation, and Pages artifact upload passed. The deploy job was skipped for pull-request review; no Pages deployment occurred.
- GUI.SVG is pinned to source commit [`b4268c8db4e69908fb0b876f081aa323497d00d3`](https://github.com/TrentBest/TheSingularityWorkshop.GUI/commit/b4268c8db4e69908fb0b876f081aa323497d00d3). The seven generated SVG snapshots are committed on the PR head.
- At the 2026-11-25 snapshot, the documentation branch compared 84 commits ahead / 25 behind `fix/integer-definition-reregistration` (merge base `6aa84d7a6db1f246e963d52d98041667430c8294`). Its changed-file footprint is documentation, generated visuals, generator/workflows, README, and site landing page. This is a materially divergent stacked branch and must be reconciled selectively against the current implementation candidate before integration.

Browser-level visual review remains an editorial check. The successful Pages run proves build and artifact generation, not deployment or semantic correctness of every diagram.


## Validation refresh — 2026-11-26

The current documentation candidate is `0a53d420e9705047a2a17c0522a6c09976567c1a`. Both [Living visuals run #23](https://github.com/TrentBest/FSM_API/actions/runs/45748841747) and [Pages build #62](https://github.com/TrentBest/FSM_API/actions/runs/45748841948) completed successfully on this exact SHA. The Pages build generated and uploaded the `github-pages` artifact (25,123,514 bytes); its deployment job was skipped because this was a pull-request validation run. These results supersede the earlier run references above for current-head CI status.

The seven committed SVG snapshots were inspected at source level for their accessible labels, descriptions, and instructional text. They cover ownership, lifecycle, state addition, transitions, string/integer representations, tick consistency, and deferred mutation. This was not a browser-rendered visual/editorial review; layout, animation behavior, and visual legibility in target browsers remain open checks.

At the 2026-11-26 03:56Z refresh, the live PR comparison is **87 commits ahead / 25 behind** `fix/integer-definition-reregistration`, with merge base `6aa84d7a6db1f246e963d52d98041667430c8294` and 26 changed files. The branch remains materially diverged. The comparison footprint is documentation, SVG snapshots, generator/workflow, README, and site landing-page changes; reconcile selectively against the implementation candidate before integration. No deployment or merge has occurred.


## String/integer representation and performance evidence — 2026-11-25

The two representations are intentionally distinct. The string API preserves name-oriented authoring and diagnostics; the integer runtime uses caller-supplied FSM/group IDs and array-indexed FSM/state storage. The explicit compiler bridge transfers definitions and returns state-name mappings; it does not transfer live handles. Its ordinal-sorted state IDs are deterministic for the same exact state set, but can shift when that set changes, so they are not durable persistence IDs without a preserved/versioned mapping.

The integer runtime's direct-indexed lookups are not proof that the whole scheduler is constant-time: a group update scans the definition array and scans the tick's handle snapshot for each eligible definition. The paired benchmark matrix must measure that scheduler behavior separately from narrow lookup costs. The full contract and future adapter boundary are documented in [String and integer backing](User%20Guide/12_String_and_Integer_Backing.md).

## Remaining 2.0 release gates

1. Complete the full source/API review of the 138-commit integration candidate; the passing CI run is not a substitute for reviewing the complete diff.
2. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, destruction, and error thresholds/policy. Both runtimes expose live-instance movement, but their public surfaces differ: the string-backed API exposes `MoveInstance(FSMHandle, processingGroup)`, while the integer operation is on `IntegerBacked.FSMRuntime.MoveInstance(FSMHandleInt, targetFSMID)` and is not forwarded by the concise integer interaction facade. The remaining parity audit includes multi-handle/threshold interactions and diagnostic-event semantics.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts, including the integer direct-array boundary.
4. Produce reproducible 2.0 performance evidence. The companion [FSM_API_Benchmark PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) contains a .NET 10 source-linked 2.0 paired project alongside the historical 1.x package-based suite. Its initial cases compare no-transition scheduler updates (1/10/50 definitions × 1/10/50 instances) and successful public identity lookup. [Validation run #23](https://github.com/TrentBest/FSM_API_Benchmark/actions/runs/45576383255) built both projects, generated the API inventory, and passed both Dry smoke tests; the calibrated measurement job was skipped. The workflow now accepts `fsm_api_ref` so a manual run can pin a branch, tag, or exact commit SHA; the latest smoke run still checked out FSM_API `development`, not this PR #42 candidate head. No 2.0 timing or integer speedup has been measured. The public-update pair also includes a known wrapper asymmetry: string `Update(string)` starts/stops a Stopwatch and checks a warning threshold, while integer `Update(int)` forwards directly to the runtime. A separate scheduler-core comparison is required before attributing a public-update delta solely to integer backing. Next: run the paired matrix against exact source SHA `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9` (or the selected release candidate), preserve raw results and provenance, and expand coverage to transitions, construction, mutation, lifecycle, and transfer before any performance conclusion. [Benchmark PR #5](https://github.com/TrentBest/FSM_API_Benchmark/pull/5) adds the stable-ID catalog and report-only regression policy; it is also open and draft.
5. Verify the intended compatibility matrix with clean consumer restores/tests. The library currently builds `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`; the automated test project currently runs on .NET 10 only. Confirm older .NET Framework/.NET Core consumers through the appropriate `netstandard2.0` asset and explicitly decide whether the direct `net47` target is sufficient for the intended legacy reach.
6. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations. The current CI verifies link integrity and DocFX generation, not completeness or semantic accuracy of every public member.
7. Prepare final 2.0.0 package metadata, release notes, migration guidance, and package-content review. The project file still declares version `1.0.13`; no version bump or publication has been performed.
8. Reconcile the 24-branch live inventory to the two enduring branches. The publication gate is not currently satisfiable; preserve the branch ledger and obtain owner approval for any eventual batch deletion.
9. Review all evidence and obtain explicit owner authorization before merge to `master`, release, or NuGet publication.

## Branch and publication gate

The live GitHub branch inventory checked on 2026-11-26 contains **24 branches**: `master`, `development`, and 22 additional branches, including the active documentation branch. The canonical two-branch policy is therefore not satisfied. Sith Policy run [#490](https://github.com/TrentBest/FSM_API/actions/runs/45751236880) completed successfully as a workflow, but its evaluated result was **NON-COMPLIANT**. Because this evaluation ran on the documentation branch rather than the default branch, it did not publish or refresh the default-branch badge. No surplus branch has been deleted as part of this review.

Publication must remain blocked until unique branch work is reconciled, a fresh live inventory proves that exactly `master` and `development` exist, and explicit owner authorization is independently provided. PRs #40 and #41 have had their relevant changes ported into #42, but their source branches remain present and must not be deleted until the reconciliation is reviewed and the owner approves batch cleanup.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.


## Current evidence — 2026-11-26 03:56Z

This section supersedes earlier point-in-time statements above where their branch heads, run numbers, or comparisons differ.

### Implementation candidate — PR #42

- Current head: `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9`; base: `development` at `9c06ec955713bbe6db1c4b5f395acdbd1bcfaa7a`.
- Current compare: **138 commits ahead / 0 behind** `development`. PR #42 remains open and draft.
- Validation [run #1109](https://github.com/TrentBest/FSM_API/actions/runs/45571547836) passed on that exact head: six library target frameworks built; .NET 10 tests **558 passed, 0 failed, 0 skipped**; 0 warnings and 0 errors; documentation-link validation and DocFX build passed.
- CI package and DLL artifacts were produced, but `publish_nuget` was skipped. The project package version remains `1.0.13`; no 2.0 package was published.

### Documentation candidate — PR #43

- Current head: `680555b1d3934e49d4330c58c115a60b7a4e50df`; base: `fix/integer-definition-reregistration` at `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9`.
- Current compare: **87 commits ahead / 25 behind**, merge base `6aa84d7a6db1f246e963d52d98041667430c8294`; 26 changed files. This remains a materially diverged stacked branch and is not ready for wholesale integration.
- Living visuals [run #24](https://github.com/TrentBest/FSM_API/actions/runs/45751243069) passed on the exact current head. GUI.SVG checkout, .NET setup, generation, snapshot commit step, and artifact upload all completed successfully.
- Pages build [run #63](https://github.com/TrentBest/FSM_API/actions/runs/45751243354) passed on the exact current head. GUI.SVG generation, local link validation, DocFX build, and Pages artifact upload passed. Deployment was skipped for PR review; this is not a live Pages deployment.
- Sith Policy [run #490](https://github.com/TrentBest/FSM_API/actions/runs/45751236880) evaluated this branch and found 24 branches, so the repository remains non-compliant with the exact-two-branch rule. Workflow success means the evaluation completed, not that the policy passed.
- The SVG review remains source-level only. Browser-rendered layout, animation behavior, and visual legibility have not yet been signed off.

### Reconciliation and deletion disposition

PR #42 is the current implementation candidate and PR #43 is a stacked documentation/generator candidate. Keep the latter isolated until its documentation claims and workflow changes are reconciled against the current #42 implementation head. The three previously owner-approved cleanup branches are now absent from the live inventory. The current inventory is 21 branches: `master`, `development`, and 19 noncanonical branches. Every remaining noncanonical branch has at least one commit ahead of `development`; none is currently cleared for deletion. See [the live branch reconciliation audit](BRANCH_RECONCILIATION.md) for the latest compare counts and exact refs. No further branch deletion, PR merge, rebase, retarget, deployment, package publication, or release was performed.


## Source/API audit correction — 2026-11-26 04:10Z

A focused review of the current documentation candidate (`b9d045cc98481cccf864e2cd303009fe90e69274`) against the actual facade, runtime, handle types, and regression tests corrected an earlier overstatement:

- Any-State precedence is implemented in the integer evaluator (`FSM.Step` evaluates Any-State transitions before state-specific transitions) and is covered by `IntegerParityTests.AnyStateTransition_TakesPriorityOverStateSpecificTransition` plus the cross-representation callback-trace parity test. The string-backed suite independently covers first-match precedence and fallback to ordinary transitions.
- The current public integer interaction facade exposes scheduled update, definition lookup, unregister, and destruction. `FSMHandleInt` exposes its own per-handle operations; the facade does not provide a complete set of representation-matched per-handle forwarding methods.
- Error-policy parity is not complete: string error counters are static/shared and keyed by handle or definition name, while integer counters are owned by each `FSMRuntime` and keyed by handle or integer FSM ID. The direct parity test covers one failing handle per definition; it does not establish multi-handle behavior or cross-group counter isolation. `OnInternalApiError` is synchronous and subscriber exceptions are not caught by `InvokeInternalApiError`; in either runtime, a throwing subscriber can interrupt the instance-error path before threshold cleanup is queued. Add focused regression tests before treating these semantics as a stable parity contract.
- The string-backed API exposes `MoveInstance(FSMHandle, processingGroup)`. The integer runtime exposes `IntegerBacked.FSMRuntime.MoveInstance(FSMHandleInt, targetFSMID)`, with target registration/current-state validation, state/context/entry-flag preservation, and tick-boundary queuing (last request wins). Dedicated `FSMRuntime_MoveInstance_Tests` cover successful movement, incompatible/unknown targets, deferred commit after the full tick, and last-target-wins behavior. The concise integer interaction facade does not forward the runtime method. The earlier claim that no integer runtime movement method exists was incorrect; the actual limitation is the facade surface, not runtime capability.
- `FSM_API_THEORY.md` was corrected to describe both runtime movement APIs and the narrower concise-facade surface accurately.

This is a documentation correction, not a runtime change. No package was published and no branch was deleted.


## Error-counter API surface audit — 2026-11-26 04:34Z

The counter-store audit found an additional observable difference that must remain explicit in parity claims:

- String-backed `FsmApi.Error` stores instance and definition counters in static dictionaries shared by the string API. Definition counters are keyed by definition name alone, not by processing group.
- `FsmApi.Error.GetErrorCounts()` and `GetDefinitionErrorCounts()` return the underlying dictionaries. Their XML documentation calls them read-only, but the returned `Dictionary` instances are mutable and changes affect live runtime accounting.
- Integer-backed counters are owned by each `FSMRuntime`; instance counters are keyed by `FSMHandleInt` and definition counters by integer FSM ID. `GetInstanceErrorCounts()` and `GetDefinitionErrorCounts()` return copies, so callers cannot mutate the live stores through those returned dictionaries.
- The existing parity test exercises one failing handle per definition and a definition threshold of one. It does not prove behavior for multiple failing handles, same-name definitions in different groups, cross-runtime string counter interference, or mutation through the string getters.

This is a source/documentation finding, not a runtime fix. Error-policy parity remains incomplete until the intended public contract is decided and focused tests cover these cases.


## Current evidence refresh — 2026-11-26 04:41Z

This refresh supersedes earlier point-in-time heads, branch counts, and compare values in this report.

- PR #42 remains open and draft at `cb81e27062e30d5f0d2022cfb0aa4b0af3ceebb9`, **138 ahead / 0 behind** `development`. Its previously recorded exact-head build/test result remains the latest validation evidence; this refresh did not rerun it.
- PR #43 remains open and draft at `71088447aeb418e0de405d1862225dc5391e2718`. It is **103 ahead / 25 behind** PR #42's runtime branch (merge base `6aa84d7a6db1f246e963d52d98041667430c8294`) and **216 ahead / 0 behind** `development`. The branch has 27 changed files and must be reconciled selectively, not merged wholesale.
- Live inventory: **21 branches** (`master`, `development`, and 19 noncanonical). All 19 noncanonical refs retain unique commits ahead of `development`; none is cleared for owner deletion.
- Sith Policy [run #45756358275](https://github.com/TrentBest/FSM_API/actions/runs/45756358275) completed on the exact PR #43 head. Its workflow conclusion is success, but the evaluated policy is **NON-COMPLIANT** at 21 branches. This branch-scoped evaluation does not refresh the default-branch badge.
- The last successful Pages and living-visuals validations cited above predate this head. Fresh current-head generation/Pages validation and browser-rendered visual review remain outstanding. The Pages artifact is not a deployment.

### Error-policy parity remains an open implementation/test gate

The source audit confirms that string-backed definition error counts are incremented from the deferred instance-threshold action, while integer-backed definition counts are incremented synchronously in `RecordInstanceError` and teardown is applied at the integer tick boundary. The existing cross-representation test uses one failing handle per definition and a definition threshold of one; it does not prove multi-handle ordering, same-name definitions in different string processing groups, cross-runtime interference from static string counters, or behavior when a diagnostic subscriber throws at each notification stage. Keep error-policy parity explicitly incomplete until the intended contract is selected and focused tests cover these cases.

No merge, rebase, retarget, deletion, deployment, package publication, or release was performed by this refresh.