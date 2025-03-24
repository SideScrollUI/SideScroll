# Serialization

- SideScroll includes it's own serializer to handle object identification, grouping, permissions, and lazy deserialization.

## Features
- Automatically serializes most objects with no additional logic
- For classes that don't support serialization, custom TypeRepo wrappers can be registered in the Serializer
- All public Properties and Fields are automatically serialized except for those with special attributes set
- Binary Serialization is used for improved speed
- Circular references are supported
  
## Constructors
- Every object must either have a default constructor, or use a class with public properties/field names that match a constructor
```csharp
public class MyClass
{
    public MyClass() {}
    public MyClass(int param) {}
}

public class MyClass(int param)
{
    public int Param { get; set; } = param;
}
```

## Limitations
- Renaming fields or changing the object type will cause the default value to be loaded instead
  - Todo: Add an attribute for renaming fields & properties
- There is probably a 2 GB file limit due to array limits in .Net. This hasn't been hit yet, but will probably require some slight refactoring in the serializer to handle it as the database size grows.

## Object Cloning

- You can call the `Serializer.DeepClone<Type>()` to do a deep clone of any object that can be serialized.
- Any class with a `[Static]` will not be cloned to speed things up (useful for objects that won't change). This can be useful for copying objects where most of the data doesn't change and you want to take snapshots at intervals.

## Restricting Types & Members
- To prevent serialization, set `[Unserialized]` or `[NonSerialized]` on the type or member
- Only public properties and fields are serialized, and `static` members are not serialized

### Export Permissions
- When importing or exporting data or links, you might want to restrict which data can be exported
- When calling any SerializerMemory method, you can set `publicOnly = true` to disable exporting any data without the `[PublicData]` / `[ProtectedData]` attribute
- When serializing or deserializing, the debug output will print a warning whenever it encounters a type without a `[PublicData]`, `[ProtectedData]`, or `[PrivateData]` attribute. A warning log entry will also be added.
- `[PublicData]`
  - Any type or class members that specify this attribute will be included
- `[ProtectedData]`
  - All class members will default to `[PrivateData]`, but can be overridden with a `[PublicData]`
- `[PrivateData]`
  - Any types or class members that specify this attribute will be ignored, and the debug output will not output a warning for them
- The following types are also allowed by default
  - `string`
  - `DateTime`
  - `DateTimeOffset`
  - `TimeSpan`
  - `TimeZoneInfo`
  - `Type`
  - `Version`
  - `object` (exact type or an allowed type only)
  - `Array`
  - `List`
  - `Dictionary`
  - `HashSet`
- Any other types will be ignored

## Lazy Deserialization
- If you declare a property as virtual, you can load that property value in a lazy manner.
- Lazy deserialization isn't currently thread safe, and loaded results can only be used in a single thread. This feature isn't recommended for most usage.
- Set `lazy` = `true` when loading with the deserializer, and it will create a wrapper class that will only load the virtual properties when referenced (subsequent references won't reload the data)
- `public virtual string ReallyLongString { get; set; } = "...";`
