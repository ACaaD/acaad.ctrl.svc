# Any PC as IoT Device

This project's goal is it to turn any PC, be it linux or windows, into an IoT device that can be queried and actioned by
software such as home-assistant or IoBroker.

This includes

- defining `Buttons`, `Sensors` and `Switches` (referred to as [components](./Documentation/Components.md)) which can be
  attached to [`Triggers`](./Documentation/Triggers.md) or
  manually executed
- providing fine-grained control to Audio and Video settings for Windows PCs
- any CLI command can be configured to act as a button or sensor respectively
- easy to understand transformation expressions to parse the result of a command

__Note:__ This project is still being worked on and did _not yet_ reach an MVP stage.

The documents will be split between a users documentation _how to use_ the software and architecture &
implementation details for contributors. The latter will be indicated by a preceding `Design` heading.

## Contents

- [Getting Started](#getting-started)
- [Prologue](#prologue)
- [Components](./Documentation/Components.md)
    - [Commands](./Documentation/Commands.md)
    - [Transformations](./Documentation/Transformations.md)
    - [Triggers](./Documentation/Triggers.md)
- [Consuming IoT Device](#consumption)

## Getting Started

todo

## Prologue

todo1.5

## Features

todo2

## Consumption

There are multiple ways to consume the information that the configured components provide. Each component hosts its own
ReST endpoints (available through OpenAPI) that allows interacting with it directly; However the main usage will
presumably be through SignalR, which allows _pushing_ of state changes directly to a connected consumer. On the basis of
this, a IoBroker adapter is under development. Said adapter will read the configuration of the service from the OpenAPI
endpoint, create the objects automatically and keep them in sync through the bidirectional SignalR channel. 
