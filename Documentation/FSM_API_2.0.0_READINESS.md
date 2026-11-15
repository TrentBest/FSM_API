# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-15  
**Branch:** `development`

## Public API migration

**Status: Implemented on development**

- The original string-backed public implementation is now `TheSingularityWorkshop.FSM_API.FsmApi`.
- Its partial implementation and timer surface are consolidated under `FsmApi`.
- The concise `FSM_API.FsmApi` namespace facade forwards to that authoritative implementation.
- `Create`, `Interaction`, `Error`, `Internal`, and explicit `Transfer` are exposed through the established namespace; the concise namespace exposes `Create`, `Interact`, `Transfer`, and `Error`.
- Integer-backed execution remains a separate runtime with its own registry and identity model.
- The repository name, NuGet package ID, and established namespaces have not been renamed.

## Validation

The latest verified full development validation is [.NET workflow run #992](https://github.com/TrentBest/FSM_API/actions/runs/43697312085), on exact commit `4298f7eb002129b09c2e1bc4e1defd0f0f6c7d35`.

- `build_test_and_package`: completed successfully.
  - Dependency restore: passed.
  - .NET 10 build and test step: passed (**489 passed, 0 failed, 0 skipped**).
  - NuGet package artifact generation: passed.
  - DLL and NuGet artifact uploads: passed.
- `build_documentation`: completed successfully.
  - Local documentation link validation: passed.
  - DocFX generation: passed.
  - Documentation artifact upload: passed.
- `publish_nuget`: skipped; no publication was performed.
- `sith_branch_policy`: skipped on this ordinary development validation; this run is not evidence of Sith compliance.
- The commit's Codecov patch status is successful.

This records workflow/job outcomes and the exact test totals reported by the successful .NET 10 test step.

## Remaining 2.0 release gates

1. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, and destruction.
2. Complete and verify error-policy and threshold parity across both runtimes.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts.
4. Produce reproducible performance benchmarks with equivalent workloads and historical comparisons.
5. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations.
6. Review all evidence and obtain explicit owner authorization before release or NuGet publication.

## Branch and publication gate

The live branch inventory checked on 2026-11-15 contains 22 branches: `master`, `development`, and 20 additional branches. The canonical two-branch policy is therefore not currently satisfied. A separate Sith Policy workflow reported success, but the direct branch inventory contradicts that result; treat branch compliance as unverified/non-compliant until the discrepancy is resolved. No surplus branch has been deleted as part of this review.

The .NET 10 validation run above skipped its embedded Sith policy job, so it is not evidence of branch compliance. Publication must remain blocked until a fresh live inventory proves that exactly `master` and `development` exist and the explicit owner authorization requirement is independently met.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
