```mermaid
classDiagram
        direction BT
        class FSMHandle {
            +string Name
            +string CurrentState
            +IStateContext Context
            +FSM Definition
        }
        class FSM {
            +string Name
            +string InitialState
        }
        class IStateContext {
            <<interface>>
            +string Name
            +bool IsValid
        }
    
        FSMHandle "1" *-- "1" FSM : <<uses>>
        FSMHandle "1" *-- "1" IStateContext : <<controls>>
        ```