# Quick Start

When I reference State Machine I am considering the specific subtype implementation of state machine. (example EStateMachine for enemies)

- Create a State Machine Node as child of Owner root node
  - attach the StateMachine subtype script designed for Owner
  - assign Owner Export via inspector
- Create a Node for each required state of the Owner as children of the State Machine
  - attach each with their custom State subtype scripts
  - connect state specific signals
- Create an Instance Context Node as child of State Machine
  - attach the Instance Context subtype script designed for Owner
- assign State Machine Instance Context Export via inspector



- Owner
  - StateMachine
    - state
    - state
    - state
    - context
