# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-21  
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

The current integration candidate is PR [#42](https://github.com/TrentBest/FSM_API/pull/42), head `7f8f8fd0aa584685ceac99bdb30a0c63a0eafd05`, based on `development` with **90 commits ahead and 0 behind**. The PR remains open and draft.

Latest full candidate workflow: [run #1069](https://github.com/TrentBest/FSM_API/actions/runs/44891965031).

- Dependency restore: passed.
- Solution build: passed across the six library target frameworks: `net10.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`.
- .NET 10 test suite: **533 passed, 0 failed, 0 skipped**.
- NuGet package artifact: generated and uploaded.
- DLL artifacts: generated and uploaded.
- Documentation link validation: passed.
- DocFX generation and documentation artifact upload: passed.
- `publish_nuget`: skipped; no publication was performed.
- `sith_branch_policy`: skipped because this was a pull-request run, not an authorized publication dispatch.

The build emitted **327 warnings** (predominantly missing XML documentation, CS1591; plus CS1573 parameter documentation warnings). They do not fail CI, but they are a public API/documentation quality issue to reduce before release. The Codecov upload completed; the latest processed patch-coverage report must still be checked separately because the upload step is configured to continue on error and does not enforce the coverage threshold.

The project file still declares package version `1.0.13`. The generated package is a CI artifact only; this candidate has not been assigned final 2.0.0 release metadata and has not been published.

## Remaining 2.0 release gates

1. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, and destruction.
2. Complete and verify error-policy and threshold parity across both runtimes.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts.
4. Produce reproducible performance benchmarks with equivalent workloads and historical comparisons.
5. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations.
6. Review all evidence and obtain explicit owner authorization before release or NuGet publication.

## Branch and publication gate

The live GitHub branch inventory checked on 2026-11-21 contains **23 branches**: `master`, `development`, and 21 additional branches. The canonical two-branch policy is therefore not satisfied. The current candidate workflow skipped its Sith branch-policy job because it was a pull-request run. No surplus branch has been deleted as part of this review.

Publication must remain blocked until unique branch work is reconciled, a fresh live inventory proves that exactly `master` and `development` exist, and explicit owner authorization is independently provided. PRs #40 and #41 have had their relevant changes ported into #42, but their source branches remain present and must not be deleted until the reconciliation is reviewed and the owner approves batch cleanup.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
