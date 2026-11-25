![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API 2.0 — Behavior, made explicit

**A framework-independent finite-state-machine runtime for .NET.** FSM_API gives software a clear way to describe how something behaves as its situation changes.

A finite-state machine models behavior as a set of **states** and the **transitions** that move between them. Think of a door: it can be *Closed* or *Open*. A request to open it is not an arbitrary collection of conditionals scattered through an application; it is an explicit transition from one state to another, with defined actions along the way.

FSM_API lets you define that behavior once, create independent running instances for your own objects, and advance them from the update loop your application already owns. The library is deliberately independent of any particular game engine, UI framework, web host, or application.

**Development preview · unreleased 2.0 line · not a NuGet release**

This site documents the 2.0 development candidate and its intended public API. It is **not** a released package: NuGet remains on 1.x until the 2.0 verification and release gates are complete.

[Read the public API contract](Documentation/FSM_API_2.0.0_PUBLIC_API_CONTRACT.md) · [Review release readiness](Documentation/FSM_API_2.0.0_READINESS.md) · [Browse source](https://github.com/TrentBest/FSM_API/tree/development)

## What FSM_API is responsible for

FSM_API separates three things that applications often accidentally tangle together:

- **Behavior definition:** the reusable blueprint—states, transition conditions, and callbacks that describe what should happen.
- **Live execution:** an FSM handle that runs one instance of that blueprint and tracks its current state and lifecycle.
- **Application context:** the object and data being acted upon. Your application owns it; FSM_API retains its reference and invokes the behavior against it.

Your application also owns the clock. It decides when to call Update; FSM_API does not start a background loop or take over the host's main loop.

\`\`\`mermaid
flowchart TD
    A["Application / Host"] -->|"owns data and update loop"| B["IStateContext"]
    A -->|"calls Update when ready"| C["FsmApi.Interact"]
    D["FsmApi.Create"] --> E["FSM Definition"]
    E --> F["Live FSM Handle"]
    B <-->|"caller-owned context reference"| F
    C --> F
    F --> G["State callbacks and transitions"]
    H["FsmApi.Transfer"] -. "explicit conversion" .-> I["Integer-backed definition"]
    E -. "source representation" .-> H
\`\`\`

The context is not copied into an FSM-owned model. The handle keeps the caller's context reference; callbacks operate on that object. Setting \`IsValid = false\` suspends execution without implicitly destroying the instance.

## Try the 2.0 API shape

The concise consumer facade is \`FSM_API.FsmApi\`. Runtime data types, including \`IStateContext\`, remain in the established \`TheSingularityWorkshop.FSM_API\` namespace.

\`\`\`csharp
using TheSingularityWorkshop.FSM_API;
using FsmApi = FSM_API.FsmApi;

var door = new DoorContext();

FsmApi.Create.CreateFiniteStateMachine(
        "Door",
        processRate: -1,
        processingGroup: "Update")
    .State("Closed",
        onEnter: context => ((DoorContext)context).IsOpen = false)
    .State("Open",
        onEnter: context => ((DoorContext)context).IsOpen = true)
    .WithInitialState("Closed")
    .Transition("Closed", "Open",
        context => ((DoorContext)context).RequestOpen)
    .Transition("Open", "Closed",
        context => !((DoorContext)context).RequestOpen)
    .BuildDefinition();

_ = FsmApi.Create.CreateInstance("Door", door, "Update");

// The host owns the clock. One call advances one update step.
door.RequestOpen = true;
FsmApi.Interact.Update("Update");

public sealed class DoorContext : IStateContext
{
    public string Name { get; set; } = "FrontDoor";
    public bool IsValid { get; set; } = true;
    public bool RequestOpen { get; set; }
    public bool IsOpen { get; set; }
}
\`\`\`

**What to notice:** the definition describes behavior; the context carries application data; the host explicitly advances the processing group. With a process rate of \`-1\`, the definition is eligible on each host update call. FSM_API does not start a background loop or take ownership of the application.

## Two representations, one behavioral contract

2.0 retains both representations. They have separate runtime registries; the integer path is not a replacement for the string path.

| String-backed | Integer-backed |
| --- | --- |
| Readable definition and state names | Integer definition and state identities |
| Named processing groups | Integer processing-group identities |
| \`FsmApi.Interact.Update("Update")\` | \`FsmApi.Interact.Update(7)\` |
| \`FSMHandle\` | \`FSMHandleInt\` |

The facade exposes overloads for both creation paths:

\`\`\`csharp
// String-backed definition
var named = FsmApi.Create.CreateFiniteStateMachine(
    "Worker", processRate: -1, processingGroup: "Update");

// Integer-backed definition
var indexed = FsmApi.Create.CreateFiniteStateMachine(
    42, processRate: -1, processingGroupID: 7);
\`\`\`

Conversion is explicit at the boundary, through \`FsmApi.Transfer\`. The runtimes do not silently share registries or translate every integer lookup back into a string lookup.

**Parity is tested as behavior, not as matching method names.** The candidate includes trace comparisons for callback order, transition timing, invalid-context suspension/resumption, and tick-boundary definition replacement. Full parity—including diagnostic thresholds and exception paths—remains a release gate.

## A predictable lifecycle

\`\`\`mermaid
stateDiagram-v2
    [*] --> Created
    Created --> Entered: first eligible update
    Entered --> Updating: host update
    Updating --> Transitioned: guard succeeds
    Transitioned --> Entered: next update
    Updating --> Suspended: context IsValid = false
    Suspended --> Updating: context becomes valid
    Entered --> Destroyed: explicit destruction
    Suspended --> Destroyed: explicit destruction
    Destroyed --> [*]
\`\`\`

The important distinction: invalidity suspends callback execution; it does not mean destruction. Definition changes requested during callbacks are committed at a defined tick boundary so handles in one tick observe a stable definition.

See the [runtime tick and mutation contract](Documentation/FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md) for the exact rules.

## What we've built around this foundation

FSM_API is a foundation, not the whole application. It provides the behavior machinery that other Workshop components can use; each surrounding project has a separate responsibility. These are examples of the wider system, **not a list of packages that FSM_API itself depends on**.

- **[FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) — composition.** Builds on FSM_API's state and context primitives to resolve dependencies and assemble MicroBundles into a runtime for a host. It answers how capabilities fit together; it does not replace the host or render the interface.
- **[The Singularity Workshop GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI) — manifestation.** Separates a semantic description of an interface from the technology that presents it. Its GUI.SVG work can turn semantic GUI structures into inspectable vector artifacts, including the living diagrams used in this documentation.
- **[FSM_Serialization](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization) — representation boundary.** Supplies explicit binary contracts and stream adapters so domain-owned state can cross a byte-oriented boundary without making the serializer the owner of its meaning.
- **[WebPage](https://github.com/TrentBest/WebPage) and [AnyApp](https://github.com/TrentBest/AnyApp) — host experiences.** Explore browser-facing and desktop manifestations of the Workshop architecture. They demonstrate how higher-level hosts can compose and present experiences rather than pushing those responsibilities down into FSM_API.

The important architectural distinction is that **FSM_API describes and advances behavior; the surrounding systems compose capabilities, represent interfaces and data, and deliver experiences.** The ecosystem demonstrates what becomes possible when those responsibilities remain separate.

## Explore the theory and living visuals

- [FSM_API theory — behavior as an explicit runtime model](Documentation/FSM_API_THEORY.md)
- [Living visuals and GUI.SVG generator](Documentation/Visuals/README.md)
- [Runtime tick and mutation contract](Documentation/FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md)

The diagrams in \`Documentation/Visuals\` are generated from semantic GUI trees with the Workshop's GUI.SVG adapter. Their source is versioned beside the documentation, and the dedicated workflow regenerates them when the visual contract changes.

## Explore the engineering evidence

- [Public API and namespace contract](Documentation/FSM_API_2.0.0_PUBLIC_API_CONTRACT.md)
- [2.0 readiness and remaining release gates](Documentation/FSM_API_2.0.0_READINESS.md)
- [Runtime tick and mutation contract](Documentation/FSM_RUNTIME_TICK_AND_MUTATION_CONTRACT.md)
- [Benchmarking contract](Documentation/Performance/BenchmarkingContract.md)
- [Generated API reference](api/index.html)
- [Source and tests for this candidate](https://github.com/TrentBest/FSM_API/tree/development)

The Pages build validates local documentation links and generates the DocFX API reference from the same candidate source. The site is a review surface, not a substitute for the test suite or a claim that every 2.0 release gate has passed.

## Release boundary

The current published package remains **TheSingularityWorkshop.FSM_API 1.0.13**. The 2.0 candidate is not published. Package metadata, complete cross-runtime parity, reproducible string-versus-integer performance evidence, compatibility validation, documentation review, and branch reconciliation remain part of release readiness.

No website content, successful build, or readiness percentage authorizes NuGet publication or merge to \`master\`.

---

<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" height="200" /></p>
