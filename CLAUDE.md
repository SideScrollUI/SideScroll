# Steering

## Code style

- **Braces around assignments in control flow.** When the body of an `if`/`else`/`for`/`foreach`/`while` is an assignment statement, always wrap it in braces on their own lines — even for a single statement:

  ```csharp
  // Yes
  if (i < snapshot.Count - 1)
  {
      ItemsTruncated = true;
  }

  // No
  if (i < snapshot.Count - 1)
      ItemsTruncated = true;
  ```

  (This is a convention, not enforced by `.editorconfig` — `csharp_prefer_braces` can't target assignments specifically. Other single-statement bodies such as `return`/`continue`/`break` may stay braceless to match the surrounding code.)
