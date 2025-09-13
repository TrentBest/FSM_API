```mermaid
classDiagram
    class Create {
        <<static>>
        + CreateProcessingGroup(name : string) : void
        + CreateFiniteStateMachine(name : string, id : int, processingGroup : string) : FSMBuilder
        + CreateInstance(fsmName : string, context : object, processingGroup : string) : FSMHandle
    }

```

## Methods:

[Create Processing Group Method](./CreateProcessGroup/CreateProcessGroup.md)

[Create Finite State Machine Method](./CreateFiniteStateMachine/CreateFiniteStateMachine.md)

[Create Instance Method](./CreateInstance/CreateInstance.md)