# FSM_API

**FSM_API is a framework-independent C# finite-state-machine library for defining behavior separately from the objects whose behavior it controls.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![NuGet downloads](https://img.shields.io/nuget/dt/TheSingularityWorkshop.FSM_API?logo=nuget&style=flat-square)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)
[![Code Coverage](https://img.shields.io/codecov/c/github/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)

## What and Why

Application behavior often becomes tangled into large update methods, UI callbacks, or framework-specific components. FSM_API gives that behavior an explicit model: **states, transition conditions, and actions**, bound to a context object that remains owned by your application.

Define a state-machine blueprint once, create independent instances for your own objects, and update the processing group from your application's existing loop. FSM_API does not require a game engine, UI framework, web host, database, or other external package.

A useful mental model:

```text
Your application owns the object and its data
                 │
                 ▼
       IStateContext (your POCO)
                 ▲
                 │ reads / updates through callbacks
          FSM_API instance
                 ▲
                 │ created from
          FSM blueprint
      states + transitions + actions
```

## 60-Second Quick Start

This tutorial uses the published **TheSingularityWorkshop.FSM_API 1.0.13** package and a Visual Studio Console App.

### 1. Create a project

In Visual Studio, choose **Create a new project → Console App**, select C#, and target **.NET 8**.

### 2. Open the terminal

Choose **View → Terminal** in Visual Studio. Make sure the terminal is in the directory containing your new project's `.csproj` file.

### 3. Install FSM_API

```powershell
dotnet add package TheSingularityWorkshop.FSM_API --version 1.0.13
```

### 4. Replace `Program.cs` with this example

```csharp
using TheSingularityWorkshop.FSM_API;

var door = new DoorContext { RequestOpen = true };

FSM_API.Create.CreateFiniteStateMachine(
        "DoorFSM",
        processRate: 1,
        processingGroup: "Tutorial")
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

_ = FSM_API.Create.CreateInstance("DoorFSM", door, "Tutorial");

// The host application decides when the FSM is updated.
for (var tick = 0; tick < 3; tick++)
    FSM_API.Interaction.Update("Tutorial");

Console.WriteLine($"{door.Name} open: {door.IsOpen}");

public sealed class DoorContext : IStateContext
{
    public string Name { get; set; } = "FrontDoor";
    public bool IsValid { get; set; } = true;
    public bool RequestOpen { get; set; }
    public bool IsOpen { get; set; }
}
```

Expected output:

```text
FrontDoor open: True
```

**What happened?** Your application owns `DoorContext`. FSM_API owns the state-machine definition and instance lifecycle. The context's `RequestOpen` value satisfies a transition condition, and the `Open` state's entry action updates `IsOpen`. The host explicitly drives the update calls.

## Add FSM_API to an Existing Project

Already have an application? You do not need to rebuild it around FSM_API.

1. Add the package to the existing C# project that owns the behavior:
   ```powershell
   dotnet add package TheSingularityWorkshop.FSM_API --version 1.0.13
   ```
2. Implement `IStateContext` on an application-owned object or create a small adapter around an existing model. Provide `Name` and `IsValid`.
3. Define the state machine during your existing startup/setup phase.
4. Create an FSM instance for each object that needs the behavior.
5. Call `FSM_API.Interaction.Update("YourProcessingGroup")` from the update point your application already controls.

Your application continues to own its data, services, UI, networking, and main loop. FSM_API supplies the state/transition mechanism; it does not take over application scheduling or presentation.

For a library or service, use the same pattern at the lifecycle point appropriate to that host. See the [source repository](https://github.com/TrentBest/FSM_API) and [documentation directory](https://github.com/TrentBest/FSM_API/tree/development/Documentation) for deeper guides.

## How It Fits

- **FSM blueprint:** the named definition of states, transitions, guards, and callbacks.
- **`IStateContext`:** the object whose behavior is being modeled; the application owns it.
- **`FSMHandle`:** a live instance bound to a context.
- **Processing group:** the named set of instances that the host updates together.
- **Host application:** decides when to call update and owns platform-specific behavior.

FSM_API is a foundational behavior package. It does not depend on FSM_COS, MicroBundleDomain, or any Workshop application layer.

## What You Can Build

- Lifecycle and workflow state machines.
- Application/domain behavior separated from UI and hosting code.
- Independent stateful agents, entities, and simulations.
- Multiple FSM instances sharing a definition while keeping their own contexts.
- Behavior that can be tested without a specific presentation framework.

## Version and 2.0.0 direction

The currently published 1.0.13 package remains sufficient for existing string-backed use. **The next planned release is 2.0.0; another 1.x release is not planned.** The 2.0.0 direction is string and/or integer backing, but integer-backed operation is still under development and is not represented here as completed or released.

Downstream Workshop work—including FSM_COS integration, documentation, and validation—can proceed using the current package wherever it meets the contract. Integer backing must be completed and verified before the 2.0.0 release, but it does not need to block useful ecosystem work.

See [FSM_API 2.0.0 Readiness](Documentation/FSM_API_2.0.0_READINESS.md) for the release gates. No publication is authorized by this roadmap.

## What This Does Not Do

- It does not provide a GUI, web server, game engine, or application host.
- It does not own your domain data; your context object remains application-owned.
- It does not replace your application's update loop or choose when the application runs.
- It does not require the rest of the Workshop ecosystem.
- It does not promise thread-safe access to arbitrary context objects across concurrently updated processing groups. Keep context ownership and cross-thread access under your application's control.

## Development

Open `FSM_API.sln` in Visual Studio, or use the terminal from the repository root:

```powershell
dotnet build FSM_API.sln -c Release
dotnet test FSM_API.sln -c Release
```

The library project currently targets `net8.0`, `net6.0`, `netcoreapp3.1`, `netstandard2.1`, `netstandard2.0`, and `net47`. Confirm the relevant SDK/reference packs are installed when building all target frameworks.

## Status and Compatibility

- **Published package:** `TheSingularityWorkshop.FSM_API` version `1.0.13`.
- **Dependencies:** no external package dependencies declared by the library project.
- **License:** MIT.
- **API and runtime details:** see the guides and reference material in the [Documentation directory](https://github.com/TrentBest/FSM_API/tree/development/Documentation).

---

## 📦 Features at a Glance

| Capability                       | Description                                                                                                                                                                                                                                                                                                                                                                                                       |
| :------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 🔄 **Deterministic State Logic** | Effortlessly define **predictable state changes** based on **dynamic conditions** or **explicit triggers**, ensuring your application's behavior is consistent and, where applicable, **mathematically provable**. Ideal for complex workflows and reliable automation. |
| 🎭 **Context-Driven Behavior** | Your FSM logic directly operates on **any custom C\\# object (POCO)** that implements `IStateContext`. This enables **clean separation of concerns** (logic vs. data) and allows domain experts (e.g., BIM specifiers) to define behavior patterns that developers then implement. |
| 🧪 **Flexible Update Control** | Choose how FSMs are processed: **event-driven**, **tick-based** (every N frames), or **manual**. This adaptability means it's perfect for **real-time systems, background processes, or even complex user interactions** within your application's loop. |
| 🧯 **Robust Error Escalation** | Benefit from **per-instance and per-definition error tracking**, providing immediate insights to prevent runaway logic or invalid states **without crashing your application**. Critical for long-running services and mission-critical software. |
| 🔁 **Runtime Redefinition** | Adapt your application on the fly! FSM definitions can be **redefined while actively running**, enabling **dynamic updates, live patching, and extreme behavioral variation** without recompilation or downtime. Perfect for highly configurable systems. |
| 🎯 **Lightweight & Performant** | Engineered for **minimal memory allocations** and **optimized performance**, ensuring your FSMs are efficient even in demanding enterprise or simulation scenarios. No overhead, just pure C# power. |
| ✅ **Easy to Unit Test** | The inherent **decoupling of FSM logic from context data** ensures your state machines are **highly testable in isolation**, leading to more robust and reliable code with simplified unit testing. |
| 💯 **Mathematically Provable** | With clearly defined states and transitions, the FSM architecture lends itself to **formal verification and rigorous analysis**, providing a strong foundation for high-assurance systems where correctness is paramount. |
| 🤝 **Collaborative Design** | FSMs provide a **visual and structured way to define complex behaviors**, fostering better communication between developers, designers, and domain experts, and enabling less code-savvy individuals to contribute to core logic definitions. |
| 🧩 Host Neutral | Designed as a pure C# runtime with no engine-specific dependency. |

---

## 🔬 Internal Architecture (Advanced)

For academic interest or advanced debugging, the `FSM_API.Internal` namespace exposes the engine's core machinery. 

> **Warning:** These methods are intended for internal use. Direct modification may bypass safety checks.

### Core Components
* **FsmBucket**: The atomic container holding an FSM Definition (`FSM`) and its active Instances (`List<FSMHandle>`).
* **TickAll(group)**: The heart of the system. It iterates `FsmBucket`s, respects `ProcessRate`, and executes the logic cycle.
* **Deferred Modifications**: To ensure thread safety on the main loop, structural changes (like destroying an FSM) are queued and executed only *after* the tick cycle completes.

### Tick Cycle Logic
A single tick performs at most one "Enter", one "Update", and one "Exit" operation to ensure deterministic execution time.

1. **Enter (If Needed)**: If the instance is new to its current state, `OnEnter` is executed and the `HasEntered` flag is set.
2. **Update**: The `OnUpdate` action of the `CurrentState` is executed.
3. **Evaluate & Transition**: Transitions attached to the `CurrentState` are evaluated.
    * If a condition is met:
        1. `OnExit` (current state) is executed.
        2. `CurrentState` is updated to the **Target State**.
        3. `HasEntered` is reset to `false`.
    * **Note:** The `OnEnter` logic for the *new* state will not execute until the **next tick**.

---

🤝 Contributing

Contributions welcome! Whether you're integrating FSM_API into your enterprise application,
designing new extensions, or just fixing typos, PRs and issues are appreciated.

📄 License

MIT License. Use it, hack it, build amazing things with it.

---

## 🔗 Resources & Support

### 📦 Get FSM_API

- **NuGet:** [TheSingularityWorkshop.FSM_API](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
- **Source Code:** [GitHub Repository](https://github.com/TrentBest/FSM_API)

### 💖 Support The Singularity Workshop

- **Patreon:** [Support us on Patreon](https://www.patreon.com/c/TheSingularityWorkshop)
- **PayPal:** [Make a donation](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

<p align="center">
  <a href="https://github.com/TrentBest/FSM_API">
    <img src="Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" height="200">
  </a>
</p>

<p align="center">
  <em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br>
  <strong>Because state shouldn't be a mess.</strong>
</p>


---

## 🔗 The Singularity Workshop

This project is part of a deliberately troublesome ecosystem:

- **[FSM_API](https://github.com/TrentBest/FSM_API)** — behavior and state.
- **[FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS)** — composition and runtime assembly.
- **[FSM_Serialization](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)** — representation and the byte boundary.
- **[WebPage](https://github.com/TrentBest/WebPage)** — browser manifestation and proving ground.

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
