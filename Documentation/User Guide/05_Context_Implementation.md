# 05\. Understanding and Implementing Your Context (`IStateContext`)

> The `IStateContext` interface is the fundamental link between your FSM\_API logic and **your application's specific data and behavior**. It allows your FSMs to interact with the objects they are managing, whether they are game characters, UI elements, network connections, or any other entity that exhibits state-driven behavior.

Think of your `IStateContext` as the **actor** in a play, and the FSM as its **director**. The director tells the actor *what kind of action to perform* (e.g., "enter state 'Idle'"), but the actor (your `IStateContext` object) is the one who *actually knows how to perform* that action (e.g., "play the idle animation"). This separation ensures your FSMs are generic and reusable, while your application's data and logic remain encapsulated.

-----

## 📚 Table of Contents

[00. Introduction to FSM_API](00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)

[03. Getting Started with C# (Non-Unity)](03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)

[08. Performance Tips & Best Practices](08_Performance_Tips.md)

[09. Common Use Cases & Examples](09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](11_FAQ.md)


-----

## 🧐 Why Do We Need a Context?

FSM\_API's core strength lies in its **decoupling**. The FSM itself doesn't know anything about "player health," "door animations," or "enemy positions." It only understands abstract concepts like "states" and "transitions."

The `IStateContext` provides this crucial bridge:

  * **Access to Data:** It allows state actions (`onEnter`, `onUpdate`, `onExit`) and transition conditions to read and modify the specific properties of your object (e.g., `currentHealth`, `IsDoorOpen`).
  * **Execution of Logic:** It provides the methods that states will call to perform actual work (e.g., `PlayAnimation()`, `ApplyDamage()`).
  * **Lifecycle Management:** For engines like Unity, `IStateContext`'s `IsValid` property allows FSM\_API to track if the underlying object is still active and relevant, preventing errors if an object is destroyed.

-----

## 🔀 The `IContext` Interface: The Basic Identifier

At its most fundamental level, FSM\_API identifies objects that an FSM can work with through the `IContext` interface.

```csharp
// FSM_API.IContext.cs
namespace TheSingularityWorkshop.FSM.API
{
    public interface IContext
    {
        /// <summary>
        /// A string that represents the name of the entity this context refers to.
        /// Useful for logging, debugging, and identifying specific FSM instances.
        /// </summary>
        string Name { get; set; }
    }
}
```

Every object used as a context for an FSM **must** provide a `Name`. This is primarily used for:

  * **Debugging:** When FSM\_API logs messages, it uses the context's name to identify which object is performing the action or experiencing a state change.
  * **Identification:** Helps you easily distinguish between multiple FSM instances controlling different objects.

-----

## 🤝 The `IStateContext` Interface: The FSM Link

The `IStateContext` interface extends `IContext`, adding the vital `IsValid` property. This is the interface that your custom classes will implement to fully integrate with FSM\_API.

```csharp
// FSM_API.IStateContext.cs
namespace TheSingularityWorkshop.FSM.API
{
    // IStateContext builds upon the simpler IContext.
    // This means any class implementing IStateContext also fulfills the IContext contract.
    public interface IStateContext : IContext
    {
        /// <summary>
        /// A flag indicating whether the context (and thus the FSM instance bound to it) is still valid and active.
        /// When false, the FSM instance will be automatically cleaned up and removed from processing.
        /// </summary>
        bool IsValid { get; }
    }
}
```

The `IsValid` property is crucial for robust FSM management, especially in dynamic environments like game engines:

  * **Automatic Cleanup:** If `IsValid` returns `false`, FSM\_API will automatically **unregister and clean up** the associated `FSMHandle` instance. This prevents memory leaks and errors that can occur if an FSM tries to interact with a non-existent or invalid object.
  * **Performance:** Invalid FSMs are removed from processing queues, reducing overhead.
  * **Example Scenarios:**
      * In Unity, if a `GameObject` with an `IStateContext` component is destroyed, `IsValid` should return `false`.
      * In a non-Unity application, if a data model is no longer relevant (e.g., a network connection closes, a background task completes and is disposed), `IsValid` should be set to `false`.

-----

## 🛠 Implementing `IStateContext`

Let's look at how you would implement `IStateContext` in both a pure C\# class and a Unity MonoBehaviour.

### Example 1: Pure C\# Class

For console applications, backend services, or custom engines, you'll implement `IStateContext` directly on your data models or entities.

```csharp
using System;
using TheSingularityWorkshop.FSM.API;

public class MyCharacterData : IStateContext
{
    // --- Your Character's Specific Data ---
    public float CurrentHealth { get; private set; }
    public string CharacterName { get; private set; }
    public bool IsAlive { get; private set; } = true; // Your game logic controls this

    // --- IContext & IStateContext Implementation ---
    // The 'Name' property, required by IContext
    public string Name { get; set; }

    // The 'IsValid' property, required by IStateContext
    // For pure C#, this might reflect if the object is considered "active" or "disposed"
    public bool IsValid => IsAlive; // For this example, if character is not alive, it's not valid

    public MyCharacterData(string name, float initialHealth)
    {
        CharacterName = name;
        CurrentHealth = initialHealth;
        Name = name; // Set the FSM_API name
        Console.WriteLine($"{Name}: Initialized with {CurrentHealth} health.");
    }

    // --- Methods your FSM States will call ---
    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            IsAlive = false; // Mark as not alive
            Console.WriteLine($"{Name}: Took {amount} damage. Health: {CurrentHealth}. Character is no longer alive!");
        }
        else
        {
            Console.WriteLine($"{Name}: Took {amount} damage. Health: {CurrentHealth}.");
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > 100) CurrentHealth = 100;
        IsAlive = true; // Reviving
        Console.WriteLine($"{Name}: Healed {amount}. Health: {CurrentHealth}.");
    }

    public void PlaySound(string sound)
    {
        Console.WriteLine($"Playing sound '{sound}' for {Name}.");
    }
}
```

In this pure C\# example, `IsValid` directly reflects the `IsAlive` state of the character. When `IsAlive` becomes `false` (e.g., after `TakeDamage` reduces health to zero), `IsValid` will also become `false`, prompting FSM\_API to clean up the associated FSM instance.

### Example 2: Unity MonoBehaviour

In Unity, your FSM contexts will typically be `MonoBehaviour` scripts attached to `GameObject`s. The `IsValid` check for Unity usually involves checking the `gameObject.activeInHierarchy` property or if the MonoBehaviour itself is `null` (which indicates the GameObject or component has been destroyed).

```csharp
using UnityEngine;
using TheSingularityWorkshop.FSM.API; // Your FSM_API namespace

public class EnemyAI : MonoBehaviour, IStateContext
{
    // --- Your Enemy's Specific Data ---
    public float Health = 100f;
    public float AttackRange = 5f;
    public Transform Target; // The player or another target

    // --- IContext & IStateContext Implementation ---
    // The 'Name' property, required by IContext.
    // We typically set this to the GameObject's name for easy debugging.
    public string Name { get; set; }

    // The 'IsValid' property, required by IStateContext.
    // For Unity, we check if the GameObject is still active in the hierarchy.
    // 'this != null' is crucial if the MonoBehaviour script itself might be nullified.
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // --- FSM Handle for this instance ---
    private FSMHandle myFSM;

    void Awake()
    {
        Name = gameObject.name; // Assign GameObject's name for FSM_API debugging

        // Example FSM Definition (often defined once in a central manager or at startup)
        if (!FSM_API.Exists("EnemyCombatFSM"))
        {
            FSM_API.CreateFiniteStateMachine("EnemyCombatFSM", processingGroup: "Update")
                .State("Patrol",
                    onEnter: ctx => ((EnemyAI)ctx).StartPatrol(),
                    onUpdate: ctx => ((EnemyAI)ctx).DoPatrol(),
                    onExit: ctx => ((EnemyAI)ctx).StopPatrol())
                .State("Chase",
                    onEnter: ctx => ((EnemyAI)ctx).StartChase(),
                    onUpdate: ctx => ((EnemyAI)ctx).DoChase(),
                    onExit: ctx => ((EnemyAI)ctx).StopChase())
                .State("Attack",
                    onEnter: ctx => ((EnemyAI)ctx).StartAttack(),
                    onUpdate: ctx => ((EnemyAI)ctx).DoAttack())
                .WithInitialState("Patrol")
                .Transition("Patrol", "Chase", ctx => ((EnemyAI)ctx).TargetDetected())
                .Transition("Chase", "Attack", ctx => ((EnemyAI)ctx).IsInAttackRange())
                .Transition("Attack", "Chase", ctx => !((EnemyAI)ctx).IsInAttackRange())
                .Transition("Chase", "Patrol", ctx => !((EnemyAI)ctx).TargetDetected())
                .AnyTransition("Dead", ctx => ((EnemyAI)ctx).Health <= 0) // Global transition to Dead
                .BuildDefinition();
        }
    }

    void Start()
    {
        // Create an FSM instance and link *this* MonoBehaviour as its context.
        myFSM = FSM_API.CreateInstance("EnemyCombatFSM", this);
        Debug.Log($"[{Name}] FSM initialized to: {myFSM.CurrentState}");
    }

    void OnDestroy()
    {
        // Crucial for Unity: Unregister the FSM instance when the GameObject is destroyed.
        // This prevents the FSM from trying to access a null GameObject/MonoBehaviour.
        // FSM_API's IsValid check will also catch this, but explicit unregistration is good practice.
        if (myFSM != null)
        {
            FSM_API.Unregister(myFSM);
            myFSM = null;
            Debug.Log($"[{Name}] FSM instance unregistered due to GameObject destruction.");
        }
    }

    // --- Methods your FSM States will call ---
    public void StartPatrol() { Debug.Log($"[{Name}] Starting patrol."); }
    public void DoPatrol() { /* Implement patrol movement */ }
    public void StopPatrol() { Debug.Log($"[{Name}] Stopping patrol."); }
    public void StartChase() { Debug.Log($"[{Name}] Starting chase."); }
    public void DoChase() { /* Implement chase movement towards Target */ }
    public void StopChase() { Debug.Log($"[{Name}] Stopping chase."); }
    public void StartAttack() { Debug.Log($"[{Name}] Starting attack."); }
    public void DoAttack() { /* Implement attack logic */ }
    public bool TargetDetected() => Target != null && Vector3.Distance(transform.position, Target.position) < 20f;
    public bool IsInAttackRange() => Target != null && Vector3.Distance(transform.position, Target.position) < AttackRange;
}
```

-----

## ✅ Best Practices for `IStateContext` Implementation

  * **Keep Contexts Lean (but functional):** While your context holds data and methods, avoid putting complex, high-level FSM logic directly into it. That's what the FSM definition is for. The context should be focused on providing the raw data and atomic operations that states need.
  * **Clear Naming:** Ensure your `Name` property provides useful debugging information. For Unity, `gameObject.name` is often sufficient.
  * **Accurate `IsValid`:** Implement `IsValid` carefully. It's the primary way FSM\_API knows if an instance is still relevant. Incorrect `IsValid` logic can lead to memory leaks or NullReferenceExceptions.
  * **Explicit Unregistration (Unity):** For MonoBehaviours, always unregister your `FSMHandle` in `OnDestroy()`. While `IsValid` will eventually lead to cleanup, explicit unregistration is safer and faster in Unity's lifecycle.
  * **Casting in State Actions:** Remember that the `context` parameter in state actions (`onEnter`, `onUpdate`, `onExit`) and transition conditions is of type `IStateContext`. You'll almost always need to cast it to your specific context type (e.g., `((MyCharacterData)ctx)`) to access your custom properties and methods.

By adhering to the `IStateContext` interface, your FSMs become incredibly flexible and reusable, able to control diverse objects across various C\# environments.

-----

[➡️ Continue to: 06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](06_FSM_Modifier_Deep_Dive.md)