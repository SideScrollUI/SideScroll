# Logs

```csharp
Log log = new Log();
log.Add("New Log entry", new Tag("Name", "value"), new Tag("Count", 123));
```
* Logs have a tree structure and can be nested as deep as necessary.

# Tags
* All the tags get added to the log line.
* You can embed any object in the Tag `Value`, but these can consume lots of memory if you're not careful. These values can later be viewed in the log entry's `Tags` properties.

```csharp
log.Add("New Log entry", 
	new Tag("Name", "value"), 
	new Tag("Count", 123));
```

# Timer
* You can time any operation by using a log or call Timer with the `using` operator
```csharp
	using (LogTimer logTimer = call.Log.Timer("Doing work"))
	{
		...
	}
```
