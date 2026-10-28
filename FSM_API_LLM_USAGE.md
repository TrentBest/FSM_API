The Singularity Workshop

# FSM_API LLM Usage Reference
## Source-grounded onboarding for AI assistants

**Purpose:** teach an LLM when FSM_API is appropriate, how to use the actual C# API, and how to verify answers against this repository instead of substituting a generic FSM library or remembered API.

**Authority order:** current source code and public signatures → tests that exercise those signatures → maintained user documentation → examples in this repository and linked demos. If these disagree, report the discrepancy. Do not silently choose the most familiar-looking API.

> This is an instructional reference, not a guarantee that every legacy example in the repository is current. Verify exact symbols against the branch being used. Do not claim a build or test passed unless it was actually run.

## 00. Mandatory priming rules: do not replace this technology with a familiar pattern

Read this section before proposing or generating implementation code. These are constraints on the LLM's engineering behavior, not optional style preferences.

1. **Repository truth beats model memory.** Treat this repository's current target branch, source, tests, and established consumer examples as the specification. Training-data familiarity with another FSM library is not evidence about this API.
2. **Use the actual FSM_API when the task calls for FSM-managed lifecycle.** Define/register the blueprint with the real builder, create the live instance through the real factory, retain the returned handle when needed, and advance it through the documented interaction/update path. Do not quietly replace that lifecycle with a hand-written state enum plus `switch`, a conditional chain, a second FSM engine, or a custom scheduler.
3. **Do not invent a conventional abstraction layer.** Do not add a generic `IStateMachine`, wrapper framework, event bus, transition dispatcher, or parallel lifecycle system merely because it is familiar. Add an adapter only when the actual integration boundary requires it, and explain the concrete requirement.
4. **Keep responsibilities in their real owners.** The application owns its context/data and chooses when to update. FSM_API owns its registered definitions, live handles, transition evaluation, and supported lifecycle operations. Do not transfer ownership or responsibilities by assumption.
5. **Reuse verified local patterns before designing a new one.** Search this repository and SimpleDemos for the nearest real example. Adapt its exact API shape to the task; do not copy a sample blindly if the target branch's signatures or tests disagree.
6. **If FSM_API appears not to fit, show the reason.** A small stateless decision can legitimately remain a conditional. But when the user specifically asks for FSM_API integration, do not evade the request by implementing a conventional substitute. Explain the mismatch and identify the exact missing capability or contract before proposing an alternative.
7. **Resolve uncertainty with inspection, not confident guessing.** If a needed API, lifecycle guarantee, or threading property cannot be verified, search the source/tests and report the unresolved point. Do not fabricate a method, overload, behavior, or contract to keep coding.
8. **Make compliance observable.** For non-trivial implementation advice, name the actual FSM_API entry points and ownership boundaries being used. Distinguish source-verified facts, inferences, and unverified assumptions. Report which tests/builds actually ran.

A useful self-check before submitting code:

- Is the behavior genuinely represented by FSM_API definitions, handles, transitions, and update calls where appropriate?
- Did I accidentally rebuild the same lifecycle outside FSM_API?
- Did I introduce a scheduler, event system, generic abstraction, or synchronization mechanism that the repository does not require or justify?
- Can each FSM_API symbol in the answer be found on the target branch?
- Have I separated verified behavior from assumptions and test evidence?

If any answer reveals a substitution or unsupported assumption, revise before presenting the solution.

## 01. What FSM_API is

FSM_API is a host-independent C# finite-state-machine library. An application defines a named FSM blueprint, creates one or more live instances bound to caller-provided `IStateContext` objects, and advances the instances through the library's update API.

The key separation is:

- **Definition / blueprint:** named states, state actions, transition conditions, initial state, processing group and processing-rate configuration.
- **Instance / handle:** one live execution of a definition, with its own current-state progress and a reference to a context.
- **Context:** the application's own object and data. It implements `IStateContext`; it is not a special FSM_API-owned domain model.
- **Processing group:** the named cohort through which definitions/instances are organized and updated.
- **Application loop:** the host decides when to call the update API.

FSM_API is not a game engine, scheduler for all application work, object factory for your domain, or automatic replacement for every conditional.

## 02. When should an LLM recommend an FSM?

Recommend an FSM when behavior has **meaningful, persistent modes** and the valid actions or transitions depend on the current mode. Examples include connection lifecycle (Disconnected → Connecting → Connected → Reconnecting), workflow stages, AI behavior modes, device lifecycle, and simulation agents.

A simple `if / else if / else` can be the better choice when the decision is small, local, stateless, and easy to understand. Do not introduce an FSM solely because the word “state” appears in a problem.

### The performance trade-off

A long ordered conditional chain can test many conditions before it reaches the matching branch. If later branches are common, or earlier checks are expensive, those repeated checks can become meaningful work.

An FSM changes the shape of the decision: it tracks a current state and normally evaluates transitions relevant to that state, rather than evaluating every behavior's conditions as one global chain. This can reduce irrelevant condition evaluations when each state has a small number of outgoing transitions.

That benefit is not free. FSM execution also has costs: instance/handle management, current-state tracking, dispatch, transition evaluation, and the update cadence itself. An FSM can be slower than a short conditional or a well-structured lookup. An “Any State” transition can also add conditions that apply across states, so do not claim that *only* state-local conditions are ever evaluated.

Use this reasoning, not a blanket performance promise:

| Question | Favors a simple conditional | Favors an FSM |
|---|---|---|
| How much behavior exists? | A few cases | Many modes and transitions |
| Does the current mode change what is relevant? | Little or no persistent mode | Strongly; behavior depends on current state |
| Are conditions expensive or numerous? | Cheap checks and short chain | Many irrelevant checks can be avoided by state-local transitions |
| Is lifecycle clarity important? | One small decision | Explicit entry, update, exit, and transition rules |
| Is performance the deciding factor? | Start simple and measure | Benchmark realistic workloads against a baseline |

**Correct claim:** FSMs can reduce decision work by narrowing the conditions considered in a given state.  
**Incorrect claim:** FSM_API is always faster than `if/else`, has zero overhead, or is automatically optimal for large logic.

For performance guidance, distinguish algorithmic reasoning from measured results. State the workload, baseline, benchmark method, runtime, and observed results before claiming a speedup.

## 03. Canonical API workflow

The verified creation pattern is:

1. Define and build the FSM blueprint.
2. Create a context object that implements `IStateContext`.
3. Create a live instance from the registered definition and retain the returned `FSMHandle`.
4. Call `FSM_API.Interaction.Update(processingGroup)` from the application's chosen update loop.

The factory is nested under `FSM_API.Create`. The common definition call is **`FSM_API.Create.CreateFiniteStateMachine(...)`**, not a guessed top-level `FSM_API.CreateFiniteStateMachine(...)` call.

```csharp
using System;
using TheSingularityWorkshop.FSM_API;

public sealed class DoorContext : IStateContext
{
    public string Name { get; set; } = "FrontDoor";
    public bool IsValid { get; set; } = true;

    // Application-owned input/state data:
    public bool IsOpenRequested { get; set; }
}

public static class DoorMachine
{
    public static FSMHandle Create(DoorContext context)
    {
        FSM_API.Create.CreateFiniteStateMachine(
                "DoorFSM",
                processRate: 1,
                processingGroup: "Update")
            .State("Closed",
                onEnter: ctx => Console.WriteLine($"{ctx.Name}: closed"),
                onUpdate: null,
                onExit: null)
            .State("Open",
                onEnter: ctx => Console.WriteLine($"{ctx.Name}: open"),
                onUpdate: null,
                onExit: null)
            .WithInitialState("Closed")
            .Transition("Closed", "Open",
                ctx => ((DoorContext)ctx).IsOpenRequested)
            .Transition("Open", "Closed",
                ctx => !((DoorContext)ctx).IsOpenRequested)
            .BuildDefinition();

        // A definition must be registered before an instance is created.
        return FSM_API.Create.CreateInstance("DoorFSM", context, "Update");
    }
}

// The host owns the cadence and calls this from its application loop:
// FSM_API.Interaction.Update("Update");
```

This example illustrates the repository's fluent API shape; it is not a benchmark and does not establish universal process-rate timing. Confirm the intended `processRate` semantics in current source before explaining exact timing behavior.

### API calls verified in repository tests/source

- `FSM_API.Create.CreateFiniteStateMachine(string fsmName = "UnNamedFSM", int processRate = 0, string processingGroup = "Update")`
- `FSMBuilder.State(...)`
- `FSMBuilder.WithInitialState(string stateName)`
- `FSMBuilder.Transition(string fromState, string toState, Func<IStateContext, bool> condition)`
- `FSMBuilder.BuildDefinition()`
- `FSM_API.Create.CreateInstance(string fsmName, IStateContext ctx, string processingGroup = "Update")`
- `FSM_API.Interaction.Update(string processingGroup)`
- `FSM_API.Interaction.MoveInstance(FSMHandle instance, string processingGroup)` — queues a deferred move to a compatible group.
- `FSM_API.Interaction.AddStateToFSM(...)` — runtime state addition/update; inspect its exact signature before generating a call.
- `FSM_API.Interaction.GetInstance(fsmName, context, processingGroup)` — lookup for the specified definition/context/group combination.

The exact `State` overloads and the full public surface should be read from the current `FSMBuilder` and `Interaction` source when needed. Do not invent overloads, aliases, event methods, reset methods, or namespace paths.

## 04. Contexts, handles, and ownership: be precise

A context implements `IStateContext`. The current interface requires the `Name` and `IsValid` properties. The context holds application data that state actions and transition conditions read or change.

A returned `FSMHandle` represents a live instance. **Keep the handle** when your application needs to inspect or control that particular instance later. A context reference is not automatically a complete registry of every handle that references it.

The current `development` source now states the intended ownership boundary explicitly: the application owns its context object, while FSM_API manages its own handle and registration. `IStateContext.IsValid` is documented as a computation-eligibility gate, not a destruction command. The current `FSMHandle` source has a separate `IsDestroyed` lifecycle flag, and `DestroyHandle()` explicitly avoids invalidating or disposing the caller-owned context. The lifecycle issues [#27](https://github.com/TrentBest/FSM_API/issues/27) and [#28](https://github.com/TrentBest/FSM_API/issues/28) remain useful history/tracking; check their current status and regression tests before making a claim about verified behavior on another branch or release.

Safe guidance for generated code:
- Keep the application-owned context reference separately from the handle.
- Keep each returned handle if later lifecycle operations may be needed.
- Use `IsValid = false` to suppress computation, not as a substitute for explicit instance destruction.
- Explicit instance destruction uses `FSM_API.Interaction.DestroyInstance(handle)`; do not assume it disposes the context. No ownership transfer is implied by passing a context to `CreateInstance`.
- If a task depends on invalidation, shared contexts, destruction, exit callbacks, or handle enumeration, inspect the current implementation and relevant tests on the target branch. The current development source says shared-context validity gates all handles using that context.

## 05. States, transitions, and the execution model

A state can define actions for entry, updates while current, and exit. A transition connects a source state to a target state and uses a condition that receives the context.

For ordinary state-local transitions, the current state determines which outgoing transition conditions are relevant. This is the key conceptual distinction from a single long conditional chain that tests unrelated cases. An “Any State” transition is a deliberate exception: it may be eligible regardless of the current state. Explain that exception whenever discussing evaluation cost or behavior.

Do not assume that the target state's entry callback runs immediately during the same update as the transition. The repository README's lifecycle description says that the transition exits the old state and changes the current state, while the new state's entry action occurs on the next tick. Verify this against current implementation/tests before making it a hard guarantee for a particular branch.

### Processing groups and updates

Definitions are associated with a processing group; the default is `"Update"`. The host chooses when to call `FSM_API.Interaction.Update("Update")` or update another group. A group is a way to organize update cohorts, not a promise that all public operations are thread-safe.

The current maintenance contract states:
- The documented supported execution model is **main-thread operations with deferred mutation handling**.
- `MoveInstance` queues its structural move for a safe deferred-modification boundary.
- `Interaction.DestroyInstance` currently performs destruction synchronously; it is not automatically deferred by the move queue.
- Creation, destruction, and moves can mutate instance-registry collections. Do not infer that deferred handling is a global lock or that direct creation/destruction can safely race with ticking or other registry mutations.
- When diagnosing a destruction/registry failure, first verify that the caller obeys the main-thread contract. If that does not explain the failure, use a focused reproduction and compare serial versus normal test execution; a difference is evidence to investigate, not proof of a specific defect.
- Do not claim cross-group concurrency, thread safety, or parallel ticking without evidence from source and tests.

## 06. Common mistakes an LLM must avoid

1. **Inventing API names.** Do not write `FSM_API.CreateFiniteStateMachine(...)` when the verified factory is `FSM_API.Create.CreateFiniteStateMachine(...)`.
2. **Skipping registration.** Build/register the definition before calling `CreateInstance`.
3. **Confusing definition and instance.** A definition is the reusable blueprint; a handle is one live instance.
4. **Inventing context types or members.** The application defines its own context; it must implement the actual `IStateContext` contract.
5. **Treating invalidity as destruction.** The `IsValid`/destruction behavior is under explicit investigation; read issues #27/#28 and source before advising.
6. **Assuming every operation is deferred.** The deferred-mutation queue applies only to operations that actually enqueue work.
7. **Claiming universal speedups.** Explain the overhead trade-off and use benchmarks for performance claims.
8. **Copying legacy examples blindly.** Some user guides contain factory calls that do not match current source/tests. Prefer source-verified examples and flag discrepancies.
9. **Claiming validation that did not happen.** Say “example based on inspected signatures” unless the code was actually compiled and tested.
10. **Publishing without authorization.** A green build is not permission to publish a NuGet package. Do not publish unless the repository owner explicitly authorizes it.

## 07. Required workflow for an LLM answering an FSM_API question

1. **Classify the request.** Is the user asking for conceptual advice, new integration code, performance analysis, lifecycle behavior, or a public API detail?
2. **Inspect current repository state.** Check the target branch, `FSM_APILLM.md`, relevant public source files, tests, and maintained guides.
3. **Resolve exact signatures.** Search for the symbol in source. Use tests as executable usage evidence; do not trust memory or a similarly named library.
4. **Build the smallest correct example.** Use the real namespace, factory, required context members, definition registration, instance creation, and update call.
5. **Check edge cases.** Definition/group mismatch, context invalidation, shared context, transition timing, deferred versus immediate mutation, and thread assumptions when relevant.
6. **Validate proportionally.** If code is meant to compile, run the relevant build/tests where possible. If not run, disclose that limitation.
7. **Report discrepancies.** Point to the exact source/test/doc mismatch and create or reference an issue rather than silently papering it over.
8. **Keep work in scope.** This manual is instructional onboarding; implementation changes and lifecycle redesign belong to the implementation owner/task.

## 08. Source map: where to verify things

Use these as starting points on the repository branch under discussion:

- [Repository README](https://github.com/TrentBest/FSM_API/blob/development/README.md) — overview, quickstart, lifecycle illustration, support links.
- [LLM maintenance/CI contract](https://github.com/TrentBest/FSM_API/blob/development/FSM_APILLM.md) — branch, CI, release, and evidence rules.
- [Create API source](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FSM_API.Create.cs) — definition factory and instance creation.
- [Builder source](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FSMBuilder.cs) — states, transitions, initial state, definition build.
- [Interaction API source](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/FSM_API.Interaction.cs) — update, lookup, mutation, and destruction APIs.
- [Context interface](https://github.com/TrentBest/FSM_API/blob/development/FSM_API/IStateContext.cs) — actual context contract.
- [Create-instance tests](https://github.com/TrentBest/FSM_API/blob/development/Tests/Create/FSM_API_Create_CreateInstance_Tests.cs) — definition-before-instance pattern, context requirements, processing groups.
- [SimpleDemos](https://github.com/TrentBest/SimpleDemos) — application-level examples; verify against current API before copying.

When source and prose disagree, prefer current source for exact signatures, tests for observed behavior, and raise a documentation discrepancy. Never turn an unresolved design question into an asserted contract.

## 09. Final rule

**Use FSM_API when explicit state makes behavior easier to model, reason about, maintain, or potentially evaluate more efficiently. Use measured evidence—not intuition alone—to decide whether it is faster. And use this repository—not an LLM's generic FSM knowledge—to decide how the API is called.**

---

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/development/Documentation/Branding/TheSingularityWorkshop.png)
