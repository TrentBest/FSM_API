# FSM_API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![NuGet downloads](https://img.shields.io/nuget/dt/TheSingularityWorkshop.FSM_API?logo=nuget&style=flat-square)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)
[![Last commit](https://img.shields.io/github/last-commit/TrentBest/FSM_API/master)](https://github.com/TrentBest/FSM_API/commits/master)
[![Code Coverage](https://img.shields.io/codecov/c/github/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)
[![Known Vulnerabilities](https://snyk.io/test/github/TrentBest/FSM_API/badge.svg)](https://snyk.io/test/github/TrentBest/FSM_API)

[![GitHub stars](https://img.shields.io/github/stars/TrentBest/FSM_API?style=social)](https://github.com/TrentBest/FSM_API/stargazers)
[![GitHub contributors](https://img.shields.io/github/contributors/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/graphs/contributors)
[![Open Issues](https://img.shields.io/github/issues/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/issues)

[![CoderLegion](https://coderlegion.com/cl_badge_logo1.png) Join the CoderLegion Community](https://coderlegion.com/user/The+Singularity+Workshop)


[**💖 Support Us**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

---

Blazing-fast, software-agnostic Finite State Machine system for any C# application.

---

## 🔍 Overview

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

No external dependencies. No frameworks required. No boilerplate setup. Pure C# power.

---

## 💡 Why FSM_API?

Traditional FSM systems often suffer from tight coupling to specific environments or force rigid coding patterns. FSM_API liberates your state management:

| Feature                          | FSM_API ✅ | Traditional FSM ❌ |
|----------------------------------|------------|--------------------|
| Framework agnostic               | ✅         | ❌                 |
| Runtime-modifiable definitions   | ✅         | ❌                 |
| Deferred mutation safety         | ✅         | ❌                 |
| Named FSMs & Processing Groups   | ✅         | ❌                 |
| Built-in diagnostics & thresholds| ✅         | ❌                 |
| Pure C# with no external deps    | ✅         | ❌                 |

---

High-Performance Stress Testing & Benchmarks The FSM API is built for scale. Our initial baseline stress tests demonstrate the capacity to handle over 240,000 active agents at playable framerates (>30 FPS) using standard time-slicing techniques.

We are currently optimizing our backend lookup architecture, moving from string-based identifiers to a hash-based system. This change is anticipated to significantly reduce memory allocation overhead and CPU cycles per logic operation.

View the full Benchmark Report & Optimization Roadmap here

: [Benchmark Report & Roadmap](InitialBenchmark.md)

---

## 🚀 Quickstart

1. Define a simple context (your data model):

```csharp
public class LightSwitch : IStateContext
{
    public bool IsOn = false;
    public bool IsValid => true; // Essential for FSM validation
    public string Name { get; set; } = "KitchenLight";
}
```

2. Define and build your FSM:

```csharp
FSM_API.CreateProcessingGroup("MainLoop");

// Define the condition for the transition from "Off" to "On".
Func<IStateContext, bool> shouldTurnOn = ctx =>
{
    if (ctx is LightSwitch l)
    {
        return l.IsOn;
    }
    return false;
};

// Define the condition for the transition from "On" to "Off".
Func<IStateContext, bool> shouldTurnOff = ctx =>
{
    if (ctx is LightSwitch l)
    {
        return !l.IsOn;
    }
    return false;
};

// Use the fluent API to create and define the FSM.
// The .BuildDefinition() call finalizes the blueprint.
FSM_API.Create.CreateFiniteStateMachine("LightSwitchFSM", processRate: 1, processingGroup: "MainLoop")
    // Define the "Off" state and its OnEnter action.
    .State("Off", 
        onEnter: ctx => { if (ctx is LightSwitch l) l.IsOn = false; },
        onUpdate: null, 
        onExit: null)
    // Define the "On" state and its OnEnter action.
    .State("On", 
        onEnter: ctx => { if (ctx is LightSwitch l) l.IsOn = true; },
        onUpdate: null,
        onExit: null)
    .WithInitialState("Off") // Set the starting state.
    // Define the transitions using the condition functions.
    .Transition("Off", "On", shouldTurnOn)
    .Transition("On", "Off", shouldTurnOff)
    .BuildDefinition();
```

3. Create an instance:

```csharp
var kitchenLight = new LightSwitch();
var handle = FSM_API.Create.CreateInstance("LightSwitchFSM", kitchenLight, "MainLoop");
```

4. Tick the FSM:

```csharp
FSM_API.Interaction.Update("MainLoop");
```

---

## 🔧 Core Concepts

* **FSMBuilder** → fluently define states, transitions, and actions
* **FSMHandle** → runtime instance control (pause, reset, query state)
* **IStateContext** → implement for your domain objects (POCOs)
* **Processing Groups** → organize FSM ticking (UI, AI, physics, etc.)
* **Error Handling** → thresholds & diagnostics built-in
* **Thread Safety** → deferred mutation model keeps concurrency safe

---

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
|  🎮 Unity Integration Available | Now preparing for submission to the Unity Asset Store.  |


---

## 📘 What’s Next?

* 📖 Full Documentation & Wiki (TBD)
* 🧪 Unit Tests & Benchmarks (in development)
* 🔌 Plugins & Extension Framework (e.g., editors, debugging)

---

## 🤝 Contributing

PRs, issues, and extensions welcome! Let’s build smarter state logic together.

---

## 📄 License

MIT License – use it, hack it, build amazing things with it.

---

## 🧠 Brought to you by

**The Singularity Workshop – Tools for the curious, the bold, and the systemically inclined.**

<a href="https://www.patreon.com/TheSingularityWorkshop" target="_blank">
    <img src="Branding/TheSingularityWorkshop.png" alt="Support The Singularity Workshop on Patreon" height="200" style="display: block;">
</a>

Because state shouldn’t be a mess.

**Support the project:** [**Donate via PayPal**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

```

