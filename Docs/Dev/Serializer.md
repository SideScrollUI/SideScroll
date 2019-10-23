# Serialization

Atlas includes it's own serializer, which allows it to load gigabytes of data in a few seconds.
## Features
* Automatically serializes any object passed to it. No extra logic is required.
* All public Fields and Properties are automatically serialized
* Serialization is binary (MUCH faster than JSON serialization)
* Circular references are supported
* If you declare a property as virtual, you can load objects in a lazy manner. If you set `lazy` = `true` when loading the deserializer, it will create a wrapper class that will only load the virtual properties when referenced (subsequent references don't reload the data)
  - `public virtual string reallyLongString { get; set; } = "...";`
  
## Limitations
* You must add a default constructor if you add a constructor with parameters
```
  public class MyClass
  {
    public MyClass() {}
    public MyClass(int param) {}
  }
```
* Renaming fields or changing the object type will cause the default value to be loaded instead
  - Will probably add an attribute for renaming fields/properties at some point
* There is probably a 2 GB file limit due to array limits in .Net. This hasn't been hit yet, but will probably require some slight refactoring in the serializer to handle it as the database size grows.

## Object Cloning

You can call the Serializer.Clone<Type> to do a deep clone of any object that can be serialized. Additionally, any class with a [Static] will not be cloned to speed things up (useful for objects that won't change). This can be useful for simulating things where 90% of the data doesn't change and you want to take snapshots at intervals.