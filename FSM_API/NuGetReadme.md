FSM_API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)

[![GitHub stars](https://img.shields.io/github/stars/TrentBest/FSM_API?style=social)](https://github.com/TrentBest/FSM_API/stargazers)
[![GitHub contributors](https://img.shields.io/github/contributors/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/graphs/contributors)

[![Open Issues](https://img.shields.io/github/issues/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/issues)

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amain)
[![Last commit](https://img.shields.io/github/last-commit/TrentBest/FSM_API/master)](https://github.com/TrentBest/FSM_API/commits/master)
[![Code Coverage](https://img.shields.io/codecov/c/github/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amain)


[![Known Vulnerabilities](https://snyk.io/test/github/TrentBest/FSM_API/badge.svg)](https://snyk.io/test/github/TrentBest/FSM_API)

[Support Us](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

<br>
Blazing-fast, software-agnostic Finite State Machine system for any C# application.

🔍 Overview

FSM_API is a modular, runtime-safe, and fully event-aware Finite State Machine (FSM) 
system designed to plug directly into any C# application from enterprise software to 
games, simulations, robotics, or reactive systems. It provides a powerful and decoupled
approach to managing complex state-driven logic, ensuring clarity, consistency, and 
control across diverse domains.

    ✅ Thread-safe operations (main thread only, deferred mutation handling)

    🧠 Decoupled state logic from data (POCO-friendly)

    🏗️ Define once, instantiate many

    🛠️ Error-tolerant FSM lifecycle management

    🧪 Dynamic update ticking with frame/process throttling

No external dependencies. No frameworks required. No boilerplate setup. Pure C# power for your 
application's core logic.


💡 Why FSM_API?


Traditional FSM systems often suffer from tight coupling to specific environments or force rigid 
coding patterns. FSM_API liberates your state management:

| Feature                          | FSM_API ✅ | Traditional FSM ❌ |
|----------------------------------|------------|--------------------|
| Framework agnostic               | ✅         | ❌                 |
| Runtime-modifiable definitions   | ✅         | ❌                 |
| Deferred mutation safety         | ✅         | ❌                 |
| Named FSMs & Processing Groups   | ✅         | ❌                 |
| Built-in diagnostics & thresholds| ✅         | ❌                 |
| Pure C# with no external deps    | ✅         | ❌                 |

🚀 Quickstart

1. Define a simple context (your data model):
C#
```csharp
public class LightSwitch : IStateContext
{
    public bool IsOn = false;
    public bool IsValid => true; // Essential for FSM validation
    public string Name { get; set; } = "KitchenLight";
}
```
2. Define and build your FSM:
C#
```csharp
// Optional: Create a named processing group for organizing FSM updates
FSM_API.CreateProcessingGroup("MainLoop");

var fsmDefinition = FSM_API.Create.CreateFiniteStateMachine("LightSwitchFSM", processRate: 1, processingGroup: "MainLoop")
    .State("Off")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = false; }) // Action when entering "Off" state
        .TransitionIf("On", ctx => ctx is LightSwitch l && l.IsOn) // Transition to "On" if IsOn is true
    .State("On")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = true; }) // Action when entering "On" state
        .TransitionIf("Off", ctx => ctx is LightSwitch l && !l.IsOn) // Transition to "Off" if IsOn is false
    .BuildDefinition(); // Finalize the FSM definition
```
3. Create an instance for your context:
C#
```csharp
var kitchenLight = new LightSwitch();
// Associate your context with an FSM instance and assign it to a processing group
var handle = FSM_API.Create.CreateInstance("LightSwitchFSM", kitchenLight, "MainLoop");
```
4. Tick the FSM from your application's main loop:
C#
```csharp
// Process all FSMs in the "MainLoop" group
FSM_API.Interaction.Update("MainLoop");
```
🔧 Core Concepts

    FSMBuilder: Fluently define states, transitions, and associated OnEnter/OnExit actions. 
    This is your declarative interface for FSM construction.

    FSMHandle: Represents a runtime instance of an FSM operating on a specific context. 
    Provides full control over instance lifecycle, including pausing, resetting, and 
    retrieving current state.

    IStateContext: The interface your custom data models (Plain Old C# Objects - POCOs) 
    must implement. This ensures clean separation of FSM logic from your application's data.

    Processing Groups: Organize and control the update cycles of multiple FSM instances. Ideal 
    for managing FSMs that need to tick together or at different rates (e.g., UI, AI, physics). 
     If you were to use the API across multiple threads you would need to ensure context isn't 
     accessed across different process groups, each process group will itself be sequential...  

    Error Handling: Built-in thresholds and diagnostics prevent runaway logic or invalid state
     contexts, ensuring application stability without crashing.

    Thread-Safe by Design: All modifications to FSM definitions and instances are meticulously 
    deferred and processed safely on the main thread post-update, eliminating common concurrency
     issues.

## 📦 Features at a Glance

| Capability                      | Description                                                                                                                                                                                                                                                                                                                                    |
| :------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 🔄 **Deterministic State Logic** | Effortlessly define **predictable state changes** based on **dynamic conditions** or **explicit triggers**, ensuring your application's behavior is consistent and, where applicable, **mathematically provable**. Ideal for complex workflows and reliable automation.                                                                                   |
| 🎭 **Context-Driven Behavior** | Your FSM logic directly operates on **any custom C# object (POCO)** that implements `IStateContext`. This enables **clean separation of concerns** (logic vs. data) and allows domain experts (e.g., BIM specifiers) to define behavior patterns that developers then implement.                                                                 |
| 🧪 **Flexible Update Control** | Choose how FSMs are processed: **event-driven**, **tick-based** (every N frames), or **manual**. This adaptability means it's perfect for **real-time systems, background processes, or even complex user interactions** within any application loop.                                                                                             |
| 🧯 **Robust Error Escalation** | Benefit from **per-instance and per-definition error tracking**, providing immediate insights to prevent runaway logic or invalid states **without crashing your application**. Critical for long-running services and mission-critical software.                                                                                              |
| 🔁 **Runtime Redefinition** | Adapt your application on the fly! FSM definitions can be **redefined while actively running**, enabling **dynamic updates, live patching, and extreme behavioral variation** without recompilation or downtime. Perfect for highly configurable systems.                                                                                      |
| 🎯 **Lightweight & Performant** | Engineered for **minimal memory allocations** and **optimized performance**, ensuring your FSMs are efficient even in demanding enterprise or simulation scenarios. No overhead, just pure C# power.                                                                                                                                        |
| ✅ **Easy to Unit Test** | The inherent **decoupling of FSM logic from context data** ensures your state machines are **highly testable in isolation**, leading to more robust and reliable code with simplified unit testing.                                                                                                                                        |
| 💯 **Mathematically Provable** | With clearly defined states and transitions, the FSM architecture lends itself to **formal verification and rigorous analysis**, providing a strong foundation for high-assurance systems where correctness is paramount.                                                                                                                       |
| 🤝 **Collaborative Design** | FSMs provide a **visual and structured way to define complex behaviors**, fostering better communication between developers, designers, and domain experts, and enabling less code-savvy individuals to contribute to core logic definitions.   |
|  🎮 Unity Integration Available | For game and interactive application development, a dedicated [Unity integration package](https://github.com/TrentBest/FSM_API_Unity) is available, built on this core FSM_API library.  |

📘 What’s Next?

    📖 Full Documentation & Wiki (TBD)

    🧪 Unit Tests & Benchmarks (Currently Under Development)

    🔌 Plugins & Extension Framework (e.g., for visual editors, debugging tools)

🤝 Contributing

Contributions welcome! Whether you're integrating FSM_API into your enterprise application,
designing new extensions, or just fixing typos, PRs and issues are appreciated.


📄 License

MIT License. Use it, hack it, build amazing things with it.


🧠 Brought to you by:


The Singularity Workshop - Tools for the curious, the bold, and the systemically inclined.

<img src="TheSingularityWorkshop.jpg" alt="The Singularity Workshop" width="200" height="200">

Because state shouldn't be a mess.

**Support the project:** [**Donate via PayPal**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)