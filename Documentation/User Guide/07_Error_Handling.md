# 07\. Robust Error Handling: Cascading Degradation System

> FSM\_API is designed with a **Robust Error Handling** system, internally referred to as the **Cascading Degradation System** (or sometimes the "FUBAR" system). This system ensures the stability and resilience of your application by intelligently handling exceptions that occur during FSM processing, preventing them from crashing your entire system.

In complex state machine logic, unexpected issues (e.g., a `NullReferenceException` in an `onUpdate` action, an infinite loop in a transition condition) can lead to application instability. The Cascading Degradation System is built to catch these errors gracefully, isolate the problem, prevent wider failures, and provide developers with clear diagnostics.

-----

## 📚 Table of Contents

[00. Introduction to FSM\_API](https://www.google.com/search?q=00_Introduction.md)

[01. Core Concepts: Your Guide to FSM\_API](https://www.google.com/search?q=01_Core_Concepts.md)

[02. Getting Started with Unity](https://www.google.com/search?q=02_Getting_Started_Unity.md)

[03. Getting Started with C\# (Non-Unity)](https://www.google.com/search?q=03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](https://www.google.com/search?q=04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](https://www.google.com/search?q=05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](https://www.google.com/search?q=06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](https://www.google.com/search?q=07_Error_Handling.md)

[08. Performance Tips & Best Practices](https://www.google.com/search?q=08_Performance_Tips.md)

[09. Common Use Cases & Examples](https://www.google.com/search?q=09_Common_Use_Cases.md)

[10. FSM\_API for Non-Coders: A Big Picture Overview](https://www.google.com/search?q=10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](https://www.google.com/search?q=11_FAQ.md)

-----

## 🧐 Why is Robust Error Handling Critical?

Imagine a game with hundreds or thousands of AI characters, each driven by an FSM. If one character's FSM throws an unhandled exception during its update, it could:

  * **Crash the entire game:** A single bug brings down the whole application.
  * **Corrupt game state:** Leave other systems in an undefined or broken state.
  * **Create hidden bugs:** Silently break a single character's AI without you knowing, leading to subtle, hard-to-debug issues.

The Cascading Degradation System is designed to mitigate these risks by isolating failures and maintaining overall system stability.

-----

## 🧠 How the Cascading Degradation System Works

The system operates on multiple levels to ensure that errors are contained and reported:

### 1\. **Catching All Exceptions**

Every operation within FSM\_API that executes user-provided code (e.g., `onEnter`, `onUpdate`, `onExit` actions, transition conditions) is wrapped in a `try-catch` block. This ensures that no exception goes unhandled within the FSM's execution cycle.

### 2\. **Error Reporting: `FSM_API.Error.OnInternalApiError`**

When an internal error occurs within FSM\_API (e.g., an exception in user code, an attempt to use an unregistered FSM), it triggers the `OnInternalApiError` event. This is your primary mechanism for receiving detailed error reports.

**C\# Example: Subscribing to Errors**

```csharp
using TheSingularityWorkshop.FSM.API;
using System;

public class MyErrorHandler
{
    public MyErrorHandler()
    {
        // Subscribe to the event, typically once at application startup.
        FSM_API.Error.OnInternalApiError += HandleFSMAPIError;
    }

    private void HandleFSMAPIError(FSMAPIErrorType errorType, string errorMessage, Exception exception)
    {
        Console.WriteLine($"[FSM_API ERROR] Type: {errorType}");
        Console.WriteLine($"[FSM_API ERROR] Message: {errorMessage}");
        if (exception != null)
        {
            Console.WriteLine($"[FSM_API ERROR] Exception: {exception.GetType().Name} - {exception.Message}");
            Console.WriteLine($"[FSM_API ERROR] Stack Trace: {exception.StackTrace}");
        }
        // Log to your application's logging system (e.g., Unity's Debug.LogError, Serilog, NLog)
        // Send to a telemetry service (e.g., Sentry, Crashlytics)
    }

    public void Shutdown()
    {
        // Unsubscribe when your error handler is no longer needed to prevent memory leaks.
        FSM_API.Error.OnInternalApiError -= HandleFSMAPIError;
    }
}
```

**`FSMAPIErrorType` Enum:** This enum categorizes the type of error that occurred, allowing you to filter or respond differently based on the severity or source of the problem. Common types might include `UserCodeException`, `InvalidFSMState`, `UnregisteredDefinition`, etc.

### 3\. **Instance Degradation (FUBAR'd Instances)**

If an `FSMHandle` instance repeatedly throws exceptions during its `onUpdate` or transition checks (or if a critical error occurs, like a state not being found), the system will mark that specific `FSMHandle` as **unstable** or "FUBAR'd."

  * **Temporary Instability:** For the first few errors, an FSM instance might be given a grace period or retry attempts.
  * **Automatic Unregistration:** If an instance continues to be problematic, or if a severe enough error occurs, the system will **automatically unregister that `FSMHandle`**. This removes it from its processing group, preventing it from being updated further and isolating the problematic logic from the rest of your application.
  * **`FSM_API.Internal.TryUnregisterFubarInstance()`:** This internal method (not directly exposed for public use) is part of the mechanism that performs this cleanup.

### 4\. **Definition Degradation (FUBAR'd Definitions)**

In rare cases, if an FSM Definition (the blueprint itself) leads to repeated errors across *multiple* instances (suggesting a fundamental flaw in the definition rather than an instance-specific problem), the system can escalate.

  * **Definition Instability:** A counter might track errors attributed to the definition.
  * **Definition Deactivation/Removal:** If a definition proves consistently problematic, the system may **temporarily deactivate or even remove the entire FSM Definition**. This prevents further instances from being created from a faulty blueprint and isolates the source of widespread errors.
  * **`FSM_API.Internal.TryUnregisterFubarDefinition()`:** This internal method is part of the mechanism that performs this higher-level cleanup.

### 5\. **`IsValid` Property in `IStateContext`**

As discussed in **[05. Understanding and Implementing Your Context (IStateContext)](https://www.google.com/search?q=05_Context_Implementation.md)**, the `IsValid` property is another crucial part of error handling. If `context.IsValid` returns `false`, FSM\_API knows that the underlying object is no longer viable and automatically unregisters the `FSMHandle`. This is particularly important for managing object lifetimes in environments like Unity where GameObjects can be destroyed.

-----

## ✅ Benefits of the Cascading Degradation System

  * **Application Stability:** Prevents crashes and ensures your application remains operational even when individual FSMs encounter bugs.
  * **Fault Isolation:** Confines errors to the specific FSM instance or definition, protecting unrelated parts of your system.
  * **Automatic Cleanup:** Reduces memory leaks and resource consumption by automatically unregistering problematic FSMs.
  * **Clear Diagnostics:** The `OnInternalApiError` event provides structured, actionable information (error type, message, exception details) directly to your logging and monitoring systems.
  * **Predictability:** Ensures that even in error scenarios, the system behaves in a defined, rather than chaotic, manner.

By leveraging FSM\_API's robust error handling, you can build more resilient and trustworthy applications, knowing that your FSM logic is protected against common pitfalls.

-----

[➡️ Continue to: 08. Performance Tips & Best Practices](https://www.google.com/search?q=08_Performance_Tips.md)