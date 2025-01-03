# Components

Components can be thought of as a `device` that is registered with the service and can be queried or executed. Usually
they are composed of at least a [command](./Commands.md), a [transformation](./Transformations.md) and potentially
a [trigger](./Triggers.md).

The following component types are supported:

| Type   | Description                                                                   |
|--------|-------------------------------------------------------------------------------|
| Sensor | Reads one or more values from the operating machine                           |
| Button | A one-way execution of an action                                              |
| Switch | Stateful; Allows switching between two states and obtaining the current value |

The command mapped to a component can be executed ad-hoc utilizing the ReST API, but more usefully
a [trigger](./Triggers.md) can be attached for automated execution.
This allows for modeling surprisingly complex/enhanced behaviour.

## Design
