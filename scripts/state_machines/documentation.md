# StateMachine System

## What makes up the FSM system?

Classes

- StateMachine
	- EStateMachine
- State
	- Estate
	  - IdleState
	  - ChaseState
	  - KnockbackState
	  - DieState
- Context
	- InstanceContext
		- EInstanceContext
	- GroupContext
		- EGroupContext

## StateMachine

state machine is the base class that contains all required behaviours
and data for any individual implementation of a state machine

### High Level

- keeps a reference to its Owner (Implemented in subtypes)
- keeps a reference to its Context (Implemented in subtypes)
- handles state transition
- keeps track of current state
- calls its current states update methods

### Low Level

- Initializes references to its states via scene tree (automatic)
	- adds all children of type SM

### Relationships

- self -> Context (again subtypes)
- self <-> States (Bidirectional so all states can access Owner and Context)
- self -> Owner (again subtypes)

** Owner and Context are declared in subtypes due to changing Types dependent on Type of State Machine **

## State

state is responsible for carrying out state specific behaviour and holding state specific data

### High Level

- keep a reference to State Machine
	- therefor has a reference to Owner and Context
- carry out per frame and per physics frame logic required of that state
- handle required logical setup when entering state
- hanlde required logical teardown when exiting state
- change the state given an event or action that should transition state

### Low Level

- Initialize reference to its state machine via scene tree (automatic)
- use state machine ref to access information about Player or Context that will be required for any frame processing
	- self._stateMachine.Owner
	- self._stateMachine.Context

### Relationships

- self <-> StateMachine

## Context

### InstanceContext

acts as a data bucket for information needed between state changes,
concerns only per single instance information not group

### GroupContext

acts as a data bucket for information shared across multiple instances
of the same type (e.g., multiple enemies coordinating behavior)

**Note:** GroupContext is optional and only used when enemies need
to share information (swarm behavior, coordinated attacks, etc.)

not yet implemented!

## EStateMachine

The specific implementation for Enemy State Machines

- Contains the correct Enemy type for Owner
- Contains the correct EInstanceContext for Context

that's all this does!

## EState

The specific implementation for Enemy States

- Contains the correct EStateMachine type for _stateMachine


## EInstanceContext

The specific implementation for Enemy Instance Context

- Contains properties specifically required for enemies to hold data between certain states
