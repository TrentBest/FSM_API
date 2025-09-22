# 08\. Performance Tips & Best Practices

> FSM\_API is built for **high performance and efficiency**, capable of managing hundreds or even thousands of active state machines simultaneously. Achieving optimal performance, however, also depends on how you design and integrate your FSMs. This guide provides best practices and tips to ensure your applications run smoothly.

FSM\_API's core design prioritizes speed and minimal overhead. By following these guidelines, you can maximize its benefits and avoid common pitfalls that might impact performance.

-----

## 📚 Table of Contents

[00. Introduction to FSM_API](User%20Guide/00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](User%20Guide/01_Core_Concepts.md)

[03. Getting Started with C# (Non-Unity)](User%20Guide/03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](User%20Guide/04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](User%20Guide/05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](User%20Guide/06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](User%20Guide/07_Error_Handling.md)

[08. Performance Tips & Best Practices](User%20Guide/08_Performance_Tips.md)

[09. Common Use Cases & Examples](User%20Guide/09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](User%20Guide/10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](User%20Guide/11_FAQ.md)


-----

## 🚀 General Principles for High Performance FSMs

### 1\. **Keep `onUpdate` Actions Lean**

The `onUpdate` delegates for states are executed every time the FSM "ticks" (based on its `processRate`). Heavy computations, excessive allocations, or frequent complex queries within `onUpdate` can quickly become a bottleneck, especially with many active FSM instances.

  * **Avoid:** Large data traversals, complex pathfinding (unless batched or optimized), or real-time physics calculations in `onUpdate` if they can be done less frequently.
  * **Prefer:** Simple checks, state variable updates, or calling optimized methods on your context.
  * **Delegate Heavy Work:** If a complex operation is needed, consider:
      * **Batching:** Collect data over several frames and process it less frequently.
      * **Asynchronous Operations:** Use coroutines (Unity) or `async/await` (C\#) for non-blocking operations.
      * **Dedicated Systems:** Offload to manager classes or specialized systems.

### 2\. **Optimize Transition Conditions**

Transition conditions (`Func<IStateContext, bool>`) are evaluated frequently, often every tick. Just like `onUpdate`, keep these conditions as cheap as possible.

  * **Avoid:** Expensive Raycasts, `FindObjectOfType`, LINQ queries over large collections, or complex string manipulations.
  * **Prefer:** Simple property checks, boolean flags, or cached values.
  * **Order Matters:** Define your transitions from most likely/cheapest to least likely/most expensive. FSM\_API checks conditions in the order they are defined. If an early condition is met, subsequent conditions for that tick are skipped.

### 3\. **Smart Use of `processRate`**

The `processRate` parameter (set via `FSMBuilder.WithProcessRate()` or `FSM_API.CreateFiniteStateMachine()`) controls how often an FSM instance's `onUpdate` and transition conditions are evaluated. This is your primary tool for managing CPU load.

  * **`processRate = 1` (Default):** Updates every tick. Use for critical, real-time FSMs (e.g., player input, combat AI).
  * **`processRate = N` (N \> 1):** Updates every N ticks. Ideal for less critical or slower-moving FSMs (e.g., ambient AI, background processes, distant enemies). For example, a `processRate` of `5` means the FSM only updates its `onUpdate` and checks conditions every 5 frames/ticks.
  * **`processRate = 0`:** Updates *only* when explicitly called via `FSM_API.Interaction.Step()`. Perfect for purely event-driven FSMs (e.g., UI elements that only react to user clicks, backend processes triggered by messages).

<!-- end list -->

```csharp
// Example: An ambient bird FSM only needs to update every 10 frames
FSM_API.CreateFiniteStateMachine("AmbientBirdFSM", processRate: 10)
    .State("Flying", onUpdate: ctx => { /* ... */ })
    // ...
    .BuildDefinition();
```

-----

## 📈 Effective Update Strategies

### 1\. **Leverage Processing Groups**

As seen in **[02. Getting Started with Unity](/User Guide/02_Getting_Started_Unity.md)** and **[03. Getting Started with C\# (Non-Unity)](03_Getting_Started_CSharp.md)**, processing groups allow you to organize FSMs and update them at different rates or from different sources.

  * **Unity:** Use `FSM_UnityIntegration.cs` to drive groups like "Update," "FixedUpdate," and "LateUpdate."
      * **"Update"**: For most game logic, input, and visual updates.
      * **"FixedUpdate"**: For physics-related FSMs where precise, fixed-time-step updates are critical.
      * **"LateUpdate"**: For camera logic or actions that depend on all other updates being complete.
  * **Pure C\#:** Create custom groups (e.g., "AI\_Slow", "NetworkUpdates", "UI\_Input") and call `FSM_API.Update("GroupName")` from your own game loop, scheduler, or event handlers.
      * This allows you to update certain groups less frequently than your main loop, even for FSMs with `processRate = 1`.

<!-- end list -->

```csharp
// In your main game loop or manager (Pure C#)
// Update core AI every frame
FSM_API.Update("CoreAI");

// Update background AI less frequently
if (Time.elapsed % 0.5f == 0) // Example: update every 0.5 seconds
{
    FSM_API.Update("BackgroundAI");
}
```

### 2\. **Avoid Redundant FSM Updates**

Ensure you only call `FSM_API.Update("GroupName")` once per tick for each group. Multiple calls will result in redundant processing and wasted CPU cycles. The `FSM_UnityIntegration` script is designed to handle this correctly for Unity.

### 3\. **Batch Updates for Large Numbers of FSMs**

If you have thousands of FSMs, consider structuring your system so that not all FSMs in a group are "active" simultaneously or are updated every single frame.

  * **Distance Culling:** For AI, only update FSMs for characters near the player.
  * **Level of Detail (LOD):** Use different `processRate` values or even different FSM definitions based on an entity's distance from the camera or criticality.
  * **FSM\_API.Interaction.SetProcessRate():** Dynamically adjust an `FSMHandle`'s `processRate` based on game conditions (e.g., reduce update rate for far-away enemies).

-----

## 🧹 Efficient Context Implementation (`IStateContext`)

Recall that your `IStateContext` object is where your FSM reads and writes data. How you design this object and its interaction methods can impact performance.

  * **Minimize Allocations:** Avoid creating new objects (e.g., `new Vector3()`, `new List<T>()`) within your `onUpdate` or transition conditions unless absolutely necessary. Reuse existing objects or use object pools.
  * **Cache References:** If your context needs to access other components or systems frequently, cache those references rather than looking them up repeatedly (e.g., `GetComponent<T>()` in Unity's `Awake()` or `Start()`).
  * **Simple Data Access:** Direct property access (`myContext.Health`) is faster than method calls or complex logic to retrieve data.
  * **Accurate `IsValid`:** A correctly implemented `IsValid` property is vital for performance and stability. When `IsValid` returns `false`, FSM\_API automatically unregisters and cleans up the FSM instance. This prevents the system from wasting cycles trying to update invalid or destroyed objects.
      * **Unity Example:**
        ```csharp
        public bool IsValid => this != null && gameObject.activeInHierarchy;
        ```
      * **Pure C\# Example:**
        ```csharp
        public bool IsValid => !IsDisposed && _isRunning; // Custom logic
        ```

-----

## 🚫 Avoid Common Performance Traps

  * **Excessive Logging:** While debugging, extensive `Debug.Log` or `Console.WriteLine` calls in `onUpdate` can severely impact performance. Remove or gate them with `DEBUG` preprocessor directives for release builds.
  * **Unnecessary FSMs:** Not every single behavior needs an FSM. Simple, one-off logic might be better handled directly in code. FSMs shine for complex, multi-state behaviors.
  * **Large Delegates/Lambdas:** While convenient, very large and complex lambda expressions for state actions or conditions can sometimes be less readable and harder to optimize. Consider moving complex logic into private methods on your `IStateContext` class and calling those methods from the lambda.

By thoughtfully applying these performance tips and best practices, you can ensure that your FSM\_API implementation runs efficiently, contributing to a smooth and responsive user experience in your games and applications. The goal is to make FSM\_API an unobtrusive yet powerful tool in your toolkit, allowing you to focus on the exciting aspects of development without performance headaches, especially as you prepare for the Unity Asset Store package submission and integration into AnyApp.

-----

[➡️ Continue to: 09. Common Use Cases & Examples](09_Common_Use_Cases.md)