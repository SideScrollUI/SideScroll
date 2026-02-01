# Contributing to SideScroll

Thank you for your interest in contributing to SideScroll! We welcome contributions from the community.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Code Style Guidelines](#code-style-guidelines)
- [Making Changes](#making-changes)
- [Submitting Pull Requests](#submitting-pull-requests)
- [Testing](#testing)
- [Documentation](#documentation)

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a branch for your changes
4. Make your changes
5. Push to your fork
6. Submit a pull request

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Git
- Your preferred IDE (Visual Studio, Visual Studio Code, Rider, etc.)

### Clone and Build

```bash
git clone https://github.com/YOUR-USERNAME/SideScroll.git
cd SideScroll
dotnet restore
dotnet build
```

### Run the Sample Application

```bash
dotnet run --project Programs/SideScroll.Demo.Avalonia.Desktop/SideScroll.Demo.Avalonia.Desktop.csproj
```

### Run Tests

```bash
dotnet test
```

## Code Style Guidelines

We use `.editorconfig` to maintain consistent code style. Your IDE should automatically pick up these settings.

### Key Style Points

- **Indentation:** Use tabs (size 4)
- **Charset:** UTF-8
- **Line Endings:** Insert final newline
- **Namespace Declarations:** File-scoped namespaces are preferred
- **Braces:** Open braces on new line (Allman style)
- **Spacing:** Space after control flow keywords, around operators

### C# Conventions

- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Follow standard C# naming conventions:
  - `PascalCase` for classes, methods, properties
  - `camelCase` for local variables, parameters
  - `_camelCase` for private fields
- Keep methods focused and concise

## Making Changes

### Before You Start

- Check existing issues and pull requests to avoid duplicate work
- For major changes, open an issue first to discuss your proposal
- Make sure you're working on the latest code from the main branch

### Branching Strategy

- Create feature branches from `main`
- Use descriptive branch names: `feature/add-xyz`, `fix/issue-123`, `docs/update-readme`

### Commit Messages

Write clear, concise commit messages:

```
Short summary (50 chars or less)

More detailed explanation if needed. Wrap at 72 characters.
Explain what and why, not how.

- Bullet points are okay
- Use present tense: "Add feature" not "Added feature"

Fixes #123
```

## Submitting Pull Requests

### Pull Request Checklist

Before submitting your PR, ensure:

- [ ] Code builds without errors or warnings
- [ ] All existing tests pass
- [ ] New tests added for new functionality
- [ ] Code follows the style guidelines
- [ ] Documentation updated (if applicable)
- [ ] No sensitive data or credentials in code

### Pull Request Process

1. **Update Documentation:** If you've added or changed functionality, update relevant documentation
2. **Update CHANGELOG:** Add your changes to the `[Unreleased]` section of CHANGELOG.md
3. **Fill Out PR Template:** Provide a clear description of what your PR does and why
4. **Link Issues:** Reference any related issues in your PR description
5. **Request Review:** Maintainers will review your PR and may request changes
6. **Address Feedback:** Make any requested changes and push updates to your branch
7. **Merge:** Once approved, a maintainer will merge your PR

### PR Title Format

Use a clear, descriptive title:

- `Add: New feature description`
- `Fix: Bug description (#issue-number)`
- `Docs: Documentation improvement`
- `Refactor: Code improvement description`
- `Test: Test improvement description`

## Testing

### Writing Tests

- Add tests for new functionality in the appropriate test project
- Follow existing test patterns and naming conventions
- Use descriptive test method names that explain what is being tested
- Test projects are located in the `Tests/` directory

### Test Structure

We use **NUnit** for testing. Follow this structure:

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var testObject = new TestClass();
    var expected = 42;
    
    // Act
    var result = testObject.Method();
    
    // Assert
    Assert.That(result, Is.EqualTo(expected));
}
```

Common NUnit assertions:
- `Assert.That(actual, Is.EqualTo(expected))` - Equality check
- `Assert.That(collection, Has.Exactly(2).Items)` - Collection count
- `Assert.That(result, Is.Not.Null)` - Null check
- `Assert.That(value, Is.True)` or `Assert.That(value, Is.False)` - Boolean check

## Documentation

### Code Documentation

- Add XML documentation comments to all public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<example>` tags where appropriate
- Keep documentation clear and concise

### User Documentation

- Update relevant files in the `Docs/` directory
- For new features, consider adding examples to the samples project
- Update README.md if your changes affect the main features or setup

## Project Structure

```
SideScroll/
├── Libraries/          # Core libraries
│   ├── SideScroll/                    # Core functionality
│   ├── SideScroll.Avalonia/          # Avalonia UI components
│   ├── SideScroll.Serialize/         # Serialization
│   ├── SideScroll.Tabs/              # Tab system
│   └── ...
├── Programs/           # Sample applications
├── Tests/             # Test projects
├── Docs/              # Documentation
└── Images/            # Screenshots and assets
```

## Need Help?

- Check the [Documentation](Docs/Dev/Development.md)
- Look at [existing issues](https://github.com/SideScrollUI/SideScroll/issues)
- Ask questions by opening a new issue with the "question" label

## Code of Conduct

Be respectful and constructive in all interactions. We aim to maintain a welcoming and inclusive community.

## License

By contributing to SideScroll, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to SideScroll! 🎉
