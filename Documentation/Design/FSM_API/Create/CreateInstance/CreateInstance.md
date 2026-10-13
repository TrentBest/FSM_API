# CreateInstance: give a definition a real context

A definition describes behavior. An instance is where that behavior meets one particular object in your application.

<p align="center">
  <a href="../../Visuals/Create_Journey.svg">
    <img src="../../Visuals/Create_Journey.svg" alt="Animated creation workflow highlighting the distinction between a reusable FSM definition and a context-bound instance" width="1000" />
  </a>
</p>

## Method signature

```csharp
public static FSMHandle CreateInstance(
    string fsmName,
    IStateContext ctx,
    string processingGroup = "Update")
```

### Parameters

- **`fsmName`** — the name of a previously built definition.
- **`ctx`** — the non-null object implementing `IStateContext` that this instance will operate on.
- **`processingGroup`** — the group in which the definition was registered; defaults to `"Update"`.

### Returns

An `FSMHandle` for the new instance. Use the handle to interact with that instance, inspect its state, and manage its lifecycle through the supported API.

### Preconditions and errors

- A null, empty, or whitespace-only definition name or group name throws `ArgumentException`.
- A null context throws `ArgumentNullException`.
- If the named definition cannot be found in the selected group, `KeyNotFoundException` is thrown.
- The definition must be built before the instance is requested.

## Example: reuse one definition

```csharp
var firstDoor = new DoorContext { Name = "FrontDoor" };
var secondDoor = new DoorContext { Name = "GarageDoor" };

FSMHandle front = FSM_API.Create.CreateInstance("DoorFSM", firstDoor, "Doors");
FSMHandle garage = FSM_API.Create.CreateInstance("DoorFSM", secondDoor, "Doors");
```

Both handles refer to the same named definition, but each instance is associated with its own context. The context objects remain application-owned; FSM_API does not require you to turn your domain model into a library-specific object graph.

The example assumes `DoorFSM` has already been defined and built in the `Doors` processing group, and that `DoorContext` implements `IStateContext`.

## Theory connection

This is the practical boundary between a **state-machine specification** and an **execution of that specification**. Keeping the definition reusable and the context separate makes behavior easier to test, reuse, and reason about. See [What Are FSMs?](../../../../User%20Guide/Theory/01_What_Are_FSMs.md) and the [Create workflow overview](../../Create.md).

*The diagram is an illustrative teaching animation, not a live process monitor.*
