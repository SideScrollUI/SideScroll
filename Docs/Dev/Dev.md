

# Development
---
* IDEs
  * [Visual Studio](IDEs/VisualStudio.md) (recommended)
  * [Visual Studio Code](IDEs/VisualStudioCode.md)
  * JetBrains Rider
* Modules
  * [Libraries](Modules/Libraries.md)
  * [Programs](Modules/Programs.md)
* [Logs](Logs.md)
* [Serializer](Serializer.md)


## AvaloniaUI
- If you need to update the `AvaloniaUI.diff`
- Generate AvaloniaUI patches (mostly to update versions, need to add PRs for these)
  - `git diff --submodule=diff Imports > AvaloniaUI.diff`