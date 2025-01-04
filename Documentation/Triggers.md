# Triggers

Triggers define _when_ the actions (commands) of a component are to be executed. The following trigger types are
available:

| Trigger | Description                                                                                                                                            | Implemented |
|---------|--------------------------------------------------------------------------------------------------------------------------------------------------------|-------------|
| cron    | Typical [cron](https://www.baeldung.com/cron-expressions) expression                                                                                   | WIP         |
| rate    | Similar to [AWS Event Bridge](https://docs.aws.amazon.com/eventbridge/latest/userguide/eb-scheduled-rule-pattern.html#eb-rate-expressions) expressions | NO          |
| event   | Use other events (system events, other triggers, custom triggers) as trigger                                                                           | WIP         |

__Important Note:__ To prevent misconfigured triggers to fire too often, leading to reduced performance or worse
effects, there is a `at-most` semantic implicitly attached to each trigger, with the default being
`atMost=rate(15 seconds)`. This can be overriden if desired (and a higher frequency is needed); See
the [At-Most](#at-most) section for more details.

## Trigger Types

### Cron

``` json
{
    // [..]
    "trigger": {
      "type": "cron",
      "expression": "0 12 * * ?"
    }
}
```

Based on the passed cron expression, executes the component's command. In the case of the example at noon every day (as
of now, in UTC!).

### Rate

``` json
{
    // [..]
    "trigger": {
      "type": "rate",
      "expression": "5 minutes"
    }
}
```

Allows specifying the interval of execution in a more human-readable format compared to cron (with less control over the
exact time of execution). The behaviour and syntax is the same as
with [AWS EventBridge](https://docs.aws.amazon.com/eventbridge/latest/userguide/eb-scheduled-rule-pattern.html#eb-rate-expressions).

__Note:__ There is no possibility to define a start time for scheduling; The only guarantee is that the execution
happens based on the provided rate.

In the above example, the command would be executed every 5 minutes; The first execution is only guaranteed to happen
_within_ 5 minutes after the service starts.

### Event

``` json
{
    // [..]
    "trigger": {
      "type": "event",
      "topic": "command-execution",
      "name": "CommandExecutionFailed",
      "match": "$.[?(@.component= 'ping-google')]"
    }
}
```

Defines that the command is executed when an event occurs that matches the provided combination. The `topic` is
mandatory and defines the area from where the event originated, while match is optional and allows you to specify a json
path expression to limit the considered events.

The following properties can be provided and are combined by `AND`-ing if present to filter events:

| Property      | Description                                                                       | Mandatory |
|---------------|-----------------------------------------------------------------------------------|-----------|
| Topic         | Topics are the category of an event                                               | No        |
| Name          | Name of the event to listen for                                                   | No        |
| ComponentName | Name of the component (from configuration)                                        | No        |
| Match         | Allows specifying a json path expression (must evaluate to non-empty set to fire) | No        |

The above trigger fires if a command fails where the component's name is _exactly_ `ping-google`.

TODO: To inspect available events an API will be made available that shows all raised events from the past.

## At-Most

The triggers described above allow for a very fine-grained setup of when to execute the commands (of components).
However, a misconfiguration, for example a forgotten filter (match) on the wrong event topic can lead to commands being
executed too often. For that reason all commands operate on a (implicitly defined, if not provided actively) `at-most`
basis.
This means that _by default_ commands will be executed `at-most` once every 15 seconds. (This setting can be changed in
the configuration). This is to protect the user from effects of misconfiguration, as there will be _no_ restriction on
events.
For example, it is possible (even though very nonsensical) to define a trigger to fire whenever any API receives a
request.

## Design - For Contributors

This section describes the overall architecture to implement triggers. The intended audience is contributors.

### Model

The different trigger types are bound via polymorphism utilizing `BaseTrigger` as a base class (defining the `at-most`
property). The same approach is used for `Commands` and `Transformations` as well.

### Processing

The processing of triggers and execution of commands is purposefully decoupled, that means that triggers (even specific
to commands) are raised independent of the command they are attached to and a listener for that event is generated
automatically from the configuration. This approach aligns nicely with the `Event` type trigger.

### Additional Ideas:

- Specify events/triggers globally in the configuration, so that these events can trigger multiple commands
    - Potentially add a GUID (or such) to the event for easier filtering/referencing.
- Create a `GET:/event` endpoint that shows events raised in the past
    - allow providing the same properties (`topic`, `name`, `match`) as in the event-trigger payload to filter events (
      can help setting up configuration)
- Create a `POST:/event` endpoint (with the component API?) that allows raising custom events from the consumer to
  trigger command executions

