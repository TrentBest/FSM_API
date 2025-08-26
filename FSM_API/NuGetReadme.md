# FSM_API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version](https://img.shields.io/nuget/v/TheSingularityWorkshop.FSM_API?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)
[![NuGet downloads](https://img.shields.io/nuget/dt/TheSingularityWorkshop.FSM_API?logo=nuget&style=flat-square)](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API)

[![GitHub stars](https://img.shields.io/github/stars/TrentBest/FSM_API?style=social)](https://github.com/TrentBest/FSM_API/stargazers)
[![GitHub contributors](https://img.shields.io/github/contributors/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/graphs/contributors)
[![Open Issues](https://img.shields.io/github/issues/TrentBest/FSM_API)](https://github.com/TrentBest/FSM_API/issues)

[![Build Status](https://img.shields.io/github/actions/workflow/status/TrentBest/FSM_API/dotnet.yml?branch=master&style=flat-square&logo=github)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)

[![Last commit](https://img.shields.io/github/last-commit/TrentBest/FSM_API/master)](https://github.com/TrentBest/FSM_API/commits/master)
[![Code Coverage](https://img.shields.io/codecov/c/github/TrentBest/FSM_API?branch=master)](https://github.com/TrentBest/FSM_API/actions?query=workflow%3A%22dotnet.yml%22+branch%3Amaster)

[![Known Vulnerabilities](https://snyk.io/test/github/TrentBest/FSM_API/badge.svg)](https://snyk.io/test/github/TrentBest/FSM_API)

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

var fsmDefinition = FSM_API.Create.CreateFiniteStateMachine("LightSwitchFSM", processRate: 1, processingGroup: "MainLoop")
    .State("Off")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = false; })
        .TransitionIf("On", ctx => ctx is LightSwitch l && l.IsOn)
    .State("On")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = true; })
        .TransitionIf("Off", ctx => ctx is LightSwitch l && !l.IsOn)
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

| Capability                       | Description                                                        |
| -------------------------------- | ------------------------------------------------------------------ |
| 🔄 **Deterministic State Logic** | Predictable transitions with formal correctness.                   |
| 🎭 **Context-Driven Behavior**   | Works directly on your C# objects (POCOs).                         |
| 🧪 **Flexible Update Control**   | Event-driven, tick-based, or manual.                               |
| 🧯 **Robust Error Escalation**   | Per-instance & per-definition safety.                              |
| 🔁 **Runtime Redefinition**      | Redefine FSMs on the fly, live-patching supported.                 |
| 🎯 **Lightweight & Performant**  | Minimal memory allocations, high-speed execution.                  |
| ✅ **Easy to Unit Test**          | Logic and context are decoupled.                                   |
| 💯 **Mathematically Provable**   | Strong foundations for formal verification.                        |
| 🤝 **Collaborative Design**      | Clear, visualizable behavior definitions.                          |
| 🎮 **Unity Integration**         | See [FSM\_API\_Unity](https://github.com/TrentBest/FSM_API_Unity). |

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

<img src="FSM_API/Assets/TheSingularityWorkshop.jpg" alt="The Singularity Workshop" width="200" height="200">

Because state shouldn’t be a mess.

**Support the project:** [**Donate via PayPal**](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

```

