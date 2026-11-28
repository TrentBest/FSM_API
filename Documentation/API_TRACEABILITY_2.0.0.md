# FSM_API 2.0.0 — Public API Traceability

**Status:** Initial source-backed inventory; not a parity sign-off.  
**Reviewed:** 2026-11-27  
**Branch:** `development`

This document maps the concise `FSM_API.FsmApi` consumer facade to the authoritative string-backed implementation, the independent integer-backed runtime, and the available verification/documentation evidence. It is intentionally explicit about gaps. A public method existing is not proof of behavioral parity.

## 1. Public entry point and implementation ownership

The concise consumer facade is `FSM_API.FsmApi` in `FSM_API/FsmApi.Namespace.cs`. It forwards string-backed operations to `TheSingularityWorkshop.FSM_API.FsmApi` (the authoritative string runtime) and integer-backed operations to `TheSingularityWorkshop.FSM_API.IntegerBacked.FsmApi` / `FSMRuntime`.

The integer runtime is independent: it owns its own registry and integer identities. It does not obtain integer execution by translating every operation through the string registry.

| Public module | Facade source | Implementation owner | Current scope |
|---|---|---|---|
| `Create` | `FSM_API/FsmApi.Namespace.cs` | `FSM_API/FSM_API.Create.cs`; `FSM_API/IntegerBacked/FsmApi.cs`; `FSM_API/IntegerBacked/FSMBuilderInt.cs` | String and integer builders/instances; integer definition registration; named processing-group creation |
| `Transfer` | `FSM_API/FsmApi.Namespace.cs` | `FSM_API/FsmApi.cs` → `IntegerBacked.FSMCompiler.Compile` / `Decompile` | Explicit conversion between definitions; runtime instances are not transferred |
| `Interact` | `FSM_API/FsmApi.Namespace.cs` | `FSM_API/FSM_API.Interaction.cs`; `FSM_API/IntegerBacked/FsmApi.cs`; `FSM_API/IntegerBacked/FSMRuntime.cs` | Broad string lifecycle/registry operations plus a smaller integer runtime surface |
| `Error` | `FSM_API/FsmApi.Namespace.cs` | `FSM_API/FSM_API.Error.cs`; `FSM_API/IntegerBacked/FSMRuntime.cs` | Shared thresholds/diagnostic event; representation-specific counters and integer snapshots/resets |

## 2. Concise facade inventory

### Create

| Facade member | Implementation | Verification/documentation status |
|---|---|---|
| `CreateFiniteStateMachine(string, int, string)` | `FsmApi.Create.CreateFiniteStateMachine` → `FSMBuilder` | XML docs exist on implementation; builder deep-dive and core concepts explain the fluent definition model. Verify invalid input, process-rate normalization, existing-definition behavior, and build/registration semantics with focused tests. |
| `CreateFiniteStateMachine(int, int, int)` | `IntegerBacked.FsmApi.Create.CreateFiniteStateMachine` → `FSMBuilderInt` | Integer builder validates non-negative FSM/group IDs and duplicate state IDs. Verify sparse IDs, initial-state ordering, invalid process rates, and empty-definition behavior. |
| `CreateInstance(string, IStateContext, string)` | `FsmApi.Create.CreateInstance` | XML docs exist; verify missing definition, null context, handle identity, initial validity, and registry insertion. |
| `CreateInstance(int, IStateContext)` | `IntegerBacked.FsmApi.Create.CreateInstance` → `FSMRuntime.CreateInstance` | Integer surface tests exist; verify parity for lifecycle and context validity. |
| `RegisterDefinition(IntegerBacked.FSM)` | `IntegerBacked.FsmApi.Create.RegisterDefinition` | Verify null rejection, duplicate registration/re-registration, and replacement behavior. |
| `CreateProcessingGroup(string)` | `FsmApi.Create.CreateProcessingGroup` | Verify idempotence, invalid names, and interaction with removal/recreation. |

### Transfer

| Facade member | Implementation | Verification/documentation status |
|---|---|---|
| `ToInteger(FSM, int, int)` | `FsmApi.Transfer.ToInteger` → `IntegerBacked.FSMCompiler.Compile` | `FSMCompiler_Tests` covers deterministic dense IDs, initial state, actions, regular/Any-State transitions, invalid initial/source/target states, and immutable mappings. |
| `ToString(IntegerBacked.FSM, string, string, IReadOnlyDictionary<int,string>)` | `FsmApi.Transfer.ToString` → `IntegerBacked.FSMCompiler.Decompile` | `FSMCompiler_Tests` covers reverse mapping, actions/transition behavior, Any-State preservation, missing/duplicate names, and missing initial-state mapping. Facade round-trip also exists in `FsmApiFacadeTests`. |

### Interact

| Facade member | Representation | Current verification focus |
|---|---|---|
| `Update(string)` | String | Host-driven tick; process-rate and callback-order tests |
| `Update(int)` | Integer | Integer scheduler tests; compare observable trace with string runtime |
| `UpdateAllIntegerBacked()` | Integer | Multi-group scheduling and eligibility tests |
| `Unregister(int)` | Integer | Definition removal and instance teardown tests |
| `Contains(int)` | Integer | Registration/re-registration boundary tests |
| `MoveInstance(FSMHandle, string)` | String | Deferred-mutation boundary tests |
| `AddStateToFSM(...)` | String | Runtime modifier tests; duplicate-state/update semantics |
| `AddTransition(...)` | String | Runtime modifier tests; replacement and guard semantics |
| `DestroyFiniteStateMachine(string, string)` | String | Definition destruction and dependent-instance teardown |
| `Exists(string, string)` | String | Registry lookup tests |
| `GetAllDefinitionNames(string)` | String | Snapshot/ordering/empty-group semantics |
| `GetInstances(string, string)` | String | Instance lookup and collection ownership semantics |
| `RemoveProcessingGroup(string)` | String | Group removal and dependent definition/instance lifecycle |
| `DestroyInstance(FSMHandle)` | String | Explicit destruction, repeated destruction, and registry safety |
| `RemoveStateFromFSM(...)` | String | Fallback-state validity and active-instance behavior |
| `RemoveTransition(...)` | String | Transition removal and active-instance behavior |
| `GetInstance(string, IStateContext, string)` | String | Identity/context matching and missing-instance behavior |
| `GetFSMDefinition(string, string)` | String | Definition lookup and missing-definition behavior |
| `GetFSMDefinition(int)` | Integer | Integer registry lookup |
| `GetInstances(int)` | Integer | Integer instance snapshot semantics |
| `DestroyInstance(FSMHandleInt)` | Integer | Integer explicit removal and repeated-removal behavior |

**Important surface distinction:** the concise facade is not a one-for-one projection of the full string interaction API. The string implementation also contains lower-level interaction members not currently forwarded by the concise facade (for example, the canonical implementation surface is larger than the concise `Interact` list). The integer facade is smaller still. Do not describe these as full capability parity.

### Error

| Facade member group | Implementation | Verification/documentation status |
|---|---|---|
| `InstanceErrorThreshold`, `DefinitionErrorThreshold` | Shared string API policy consumed by both runtimes | Shared configuration, not per-representation settings. Test threshold boundary and teardown behavior with equivalent inputs. |
| `OnInternalApiError`, `InvokeInternalApiError`, `Report` | Shared diagnostic event/channel | Test subscriber invocation, exception payloads, and callback-failure reporting from both runtimes. |
| `InvokeInstanceError(FSMHandle,...)`, `InvokeDefinitionError(...)` | String-backed error injection/accounting | XML docs exist for legacy instance error method; test count increments, threshold action, and reset interactions. |
| `ResetDefinitionErrorCount`, `ResetInstanceErrorCount`, `GetErrorCounts`, `GetDefinitionErrorCounts`, `Reset` | String-backed counters | The two getters return the live mutable backing dictionaries (not snapshots); `Reset` clears only string-backed counters. This differs from the integer getters, which return copies. Verify and document the ownership/reset distinction; do not imply parity. |
| `GetIntegerInstanceErrorCounts`, `GetIntegerDefinitionErrorCounts`, `ResetIntegerInstanceErrorCount`, `ResetIntegerDefinitionErrorCount` | Integer runtime counters | The two getters return copies; per-handle/per-definition resets are exposed. The concise facade has no global integer-counter reset and no independent integer threshold configuration or public integer-specific error-injection method. Verify counter lifecycle and shared threshold behavior. |

## 3. Existing test evidence located

The following test areas were confirmed by opening their source files during this review. These are evidence anchors, not a claim that every row above is fully covered:

- `Tests/IntegerBacked/IntegerRuntimeSurfaceTests.cs` — integer facade build/register/create/update surface.
- `Tests/IntegerBacked/IntegerParityTests.cs` — lifecycle timing, Any-State priority/callback trace, invalid-context gate, force-transition/reset semantics, sparse IDs, and timer parity.
- `Tests/IntegerBacked/FSMCompiler_Tests.cs` — conversion/compiler validation, deterministic mapping, callback/transition preservation, read-only maps, and decompile failure cases.
- `Tests/FsmApiFacadeTests.cs` — concise facade overloads, conversion round-trip, and integer facade execution.
- `Tests/Error/FSM_API_ErrorAPI_Tests.cs` — string-backed error API and threshold behavior.
- `Tests/Interaction/FSM_API_Interaction_MoveInstance_Tests.cs` — deferred move behavior.
- `Tests/Internal/FSM_API_Internal_DestroyInsance_Tests.cs` — internal destruction behavior.
- `Tests/FSM_API_Engine_Tests.cs` — broader engine/runtime integration.

The test project currently targets `net10.0` only. The library build matrix targets `net10.0;net6.0;netcoreapp3.1;netstandard2.1;netstandard2.0;net47`. Thus current CI establishes compilation across configured library TFMs and executes tests on .NET 10; it does not establish runtime test coverage on all historical targets.

## 4. Theory, examples, and visual evidence

| Topic | Current source of explanation | Evidence limitation / next action |
|---|---|---|
| FSM model, state, transition, deterministic execution | `Documentation/User Guide/Theory/01_What_Are_FSMs.md`; theory index | Keep general FSM theory distinct from library-specific guarantees. Link each implementation claim to source and tests. |
| Mealy/Moore and hierarchical/concurrent patterns | `Documentation/User Guide/Theory/02_Types_Of_FSMs.md` | Explicitly label patterns that are conceptual rather than native FSM_API features. |
| Tick boundary, deferred mutation, registry ownership | `Documentation/User Guide/Theory/06_Tick_Boundaries_And_Registry_Ownership.md` | Source-linked to Update, deferred queue, creation, and destruction. Keep warning that deferred work is not a global lock. |
| Builder and fluent definition | `Documentation/User Guide/04_FSM_Builder_Deep_Dive.md`; XML docs on `FSMBuilder` | Modernize examples to the 2.0 `FsmApi.Create...` entry point where the page claims 2.0. Preserve explicitly labelled 1.x examples. |
| API class relationships | `Documentation/Technical/FSM_CoreClassRelationships.md`; `Documentation/Design/FSM_API/Create.md` | Review diagrams against the current facade and separate string/integer runtimes. |
| Living visuals | `Documentation/User Guide/Theory/Visuals/Registry_Tick_Boundary.svg` (linked by theory chapter) | SVG is explanatory, not telemetry. Validate accessibility, static fallback, and source/test correspondence. |

## 5. Release-blocking traceability gaps

1. **Facade contract accuracy:** the prose contract must distinguish the concise facade from the full established-namespace API and list the actual overloads. Do not promise integer equivalents for string-only operations.
2. **Integer parity:** map each supported string capability to an integer equivalent or record an intentional difference. Existing parity tests cover selected behavior, not the full matrix.
3. **Transfer correctness:** baseline round-trip/compiler tests exist. Extend them for malformed mapping edge cases, null/empty arguments, action/guard identity expectations, and explicit documentation that conversion transfers definitions—not live runtime instances.
4. **Error parity:** thresholds are shared, but cleanup timing differs (string runtime uses the deferred-modification queue; integer runtime processes pending lifecycle changes at the end of its update call). String counter getters expose live mutable dictionaries, integer getters return copies, and `Reset()` clears only string counters. Prove intended semantics for shared thresholds, exception paths, pending removal, definition unregister, counter resets, and validity/destruction interactions with equivalent string/integer traces.
5. **API docs:** verify every public facade overload has useful XML documentation, parameter/default/exception details, and a 2.0 example where appropriate.
6. **Examples:** compile representative examples against the development project; do not use published 1.0.13 NuGet as evidence for the unreleased 2.0 facade.
7. **Visuals:** ensure each behavior visual names its source contract and tests, distinguishes conceptual from measured behavior, and has a readable static state.
8. **Target frameworks:** retain the six intended compatibility targets. State clearly that current runtime tests execute only on net10.0.

## 6. Source references

- [Concise facade](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FsmApi.Namespace.cs)
- [String-backed Create](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FSM_API.Create.cs)
- [String-backed Interaction](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FSM_API.Interaction.cs)
- [Integer-backed facade](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/IntegerBacked/FsmApi.cs)
- [Integer-backed builder](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/IntegerBacked/FSMBuilderInt.cs)
- [Theory learning path](User%20Guide/Theory/README.md)
- [Tick boundary theory](User%20Guide/Theory/06_Tick_Boundaries_And_Registry_Ownership.md)
- [2.0 public API contract](FSM_API_2.0.0_PUBLIC_API_CONTRACT.md)
- [2.0 readiness](FSM_API_2.0.0_READINESS.md)

This inventory should be expanded whenever a public member, runtime capability, test, example, or visual changes. Completion requires evidence, not merely a filled table.
