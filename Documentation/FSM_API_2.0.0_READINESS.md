# FSM_API 2.0.0 Readiness

## Release intent

The next planned FSM_API release is **2.0.0**. No additional 1.x release is planned.

The 2.0.0 direction is to support **string and/or integer backing** for state identity. The precise public shape must be established by the implementation and tests before release; documentation must not promise a specific selector, configuration API, or performance result until it exists and is verified.

## Immediate priority: deliver value without blocking on integer backing

The current string-backed implementation is sufficient for existing consumers and for ongoing ecosystem integration. FSM_COS and other Workshop packages can continue to be integrated, documented, tested, and prepared while integer-backed operation is completed.

Integer backing is a 2.0.0 readiness concern—not a reason to freeze all other work or to force every consumer to adopt integer identities.

## Readiness gates

- [ ] Decide and document the supported backing modes and their public configuration surface.
- [ ] Preserve the intended string-backed usage where supported.
- [ ] Complete integer-backed identity and lookup behavior.
- [ ] Verify behavioral parity across backing modes for lifecycle, transitions, processing groups, mutation/deferred operations, and handles.
- [ ] Test identity boundaries, missing/unknown names, duplicate definitions, and invalid input.
- [ ] Verify that definitions and runtime instances remain correctly associated when using either backing mode.
- [ ] Add benchmarks that compare equivalent workloads and allocations; report measurements rather than promises.
- [ ] Update the README quick start and existing-project integration guide to the actual 2.0.0 public API.
- [ ] Review downstream packages, beginning with FSM_COS, for assumptions about backing representation.
- [ ] Build, test, pack, and validate a clean consumer restore before declaring the release candidate ready.
- [ ] Verify release notes, package metadata, compatibility notes, and migration guidance.
- [ ] Keep the NuGet publishing gate disabled until the owner explicitly approves publication.

## Downstream contract

FSM_COS needs the FSM_API behavior/state primitives exposed by the supported public contract. It should not depend on private integer registries, hidden caches, or implementation-specific lookup details.

A change to FSM_API backing storage should not turn into an unnecessary dependency inversion: FSM_API remains foundational, and FSM_COS remains a consumer of FSM_API—not the other way around.

## Release safety

This document is a readiness checklist, not a release declaration. The existence of a branch, passing isolated tests, or a successful package build does not mean 2.0.0 has been published or is ready to publish. No package should be published without explicit approval.
