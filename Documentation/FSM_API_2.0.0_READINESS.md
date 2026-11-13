# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-13  
**Branch:** `development`  
**Current development commit:** `69d3027198e154502ecab58583a2320d6ac22523`

## Public API migration

<badge>Implemented on development</badge>

- The original string-backed public implementation is now `TheSingularityWorkshop.FSM_API.FsmApi`.
- Its partial implementation and timer surface are consolidated under `FsmApi`.
- The concise `FSM_API.FsmApi` namespace facade forwards to that authoritative implementation.
- `Create`, `Interaction`, `Error`, `Internal`, and explicit `Transfer` are exposed through the established namespace; the concise namespace exposes `Create`, `Interact`, `Transfer`, and `Error`.
- Integer-backed execution remains a separate runtime with its own registry and identity model.
- The repository name, NuGet package ID, and established namespaces have not been renamed.

## Validation

The latest completed full validation is [workflow run #979](https://github.com/TrentBest/FSM_API/actions/runs/43530428269), on commit `0271cd487941437d739a4354dbbdf4e0addaa89d`.

- .NET 8 build: passed.
- Tests: 489 passed, 0 failed, 0 skipped.
- NuGet package build artifact: generated successfully.
- DocFX documentation generation: passed.
- Local documentation links: validated.
- Codecov upload: succeeded.
- NuGet publication: skipped.

The current commit only refines the XML documentation on the concise facade. Its validation run is tracked separately; do not treat the previous run as proof for that comment-only commit.

## Remaining 2.0 release gates

1. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, and destruction.
2. Complete and verify error-policy and threshold parity across both runtimes.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts.
4. Produce reproducible performance benchmarks with equivalent workloads and historical comparisons.
5. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations.
6. Review all evidence and obtain explicit owner authorization before release or NuGet publication.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.
