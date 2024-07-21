# Logs

```csharp
Log log = new();
log.Add("New Log entry",
	new Tag("Name", "value"),
	new Tag("Count", 123));
```
* Logs have a tree structure and can be nested as deep as necessary.

## Tags
* All the tags get added to the log line.
* You can embed any object in the Tag `Value`, but these can consume lots of memory if you're not careful. These values can later be viewed in the log entry's `Tags` properties.

## Timers
* You can time any operation by using a Log or Call Timer with the `using` operator

#### Log Timers
```csharp
using (LogTimer logTimer = call.Log.Timer("Doing work"))
{
    logTimer.Add("Child log entry");
}
```

#### Call Timers
```csharp
using CallTimer callTimer = call.Timer("Doing work");

callTimer.Log.Add("Child log entry");
```
