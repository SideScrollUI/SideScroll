# SideScroll

[![Live Demo](https://img.shields.io/badge/demo-live-success)](https://sidescrollui.github.io/SideScroll/)
[![NuGet](https://img.shields.io/nuget/v/SideScroll.svg)](https://www.nuget.org/packages/SideScroll)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/SideScrollUI/SideScroll/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3-blue.svg)](https://github.com/AvaloniaUI/Avalonia)

**A cross-platform Avalonia UI framework for building fast, navigable, data-driven applications with automatically generated forms and smart tab navigation.**

SideScroll is designed for navigating complex data hierarchies with speed and developer productivity in mind. It automatically selects the most likely next tabs as you navigate, allowing you to drill down 20, 30, or even 50 levels deep with ease. Every view can be saved as a shareable link, and any object can be instantly displayed or edited with auto-generated controls.

## ✨ Key Features

- **🚀 Smart Tab Navigation** — Automatically predicts and displays the next most relevant tabs based on your navigation patterns
- **🖥️ Cross-Platform** — Built on [Avalonia UI](https://github.com/AvaloniaUI/Avalonia), runs on **Windows, macOS, Linux**, and **Web (WASM)**
- **⚡ Rapid Development** — Create tabs in code for easy linking and refactoring; no XAML required
- **🎨 Dynamic Forms** — Load any object into a **TabForm** and get auto-generated Avalonia controls with attribute-based customization
- **💾 DataRepos** — Manage local data storage with data-bound views and automatic serialization
- **🔗 Shareable Links** — Create and share deep links to any view or data state
- **📊 Interactive Charting** — Integrated [LiveCharts 2](https://github.com/beto-rodriguez/LiveCharts2) with smooth animations, zooming, and interactive legends
- **📝 Rich Text Editing** — Built-in [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) support for syntax highlighting and text manipulation
- **🎨 Customizable Themes** — Light, dark, and custom themes with exportable/importable JSON configurations
- **🔍 Advanced Filtering** — Powerful DataGrid filtering with nested queries and operators

## 🎬 Live Demo

**Try SideScroll in your browser:** [https://sidescrollui.github.io/SideScroll/](https://sidescrollui.github.io/SideScroll/)

The demo runs entirely in WebAssembly using Avalonia's browser support—no installation required!

> **Note:** The browser version is experimental. For the best performance and full features, use the desktop application.

## 📸 Screenshots & Examples

### VideoGamesDB Sample

![VideoGamesDB](https://raw.githubusercontent.com/SideScrollUI/SideScroll/6b611a162f3ee741b767457f21ef08b2569fc11f/Images/Animations/SideScroll-VideoGamesDB.gif)

### Light Theme

![Light Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Screenshots/PlanetsLight.png)

### Dark Theme

![Dark Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Screenshots/PlanetsDark.png)

### Custom Theming - Hybrid Theme

![Hybrid Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Screenshots/HybridTheme.png)

### Rich Text Editing with AvaloniaEdit

![Rich Text Editing](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Screenshots/TextEditorJsonAndXml.png)

### Shareable Links

![Links](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Screenshots/Links.png)

## 🚀 Quick Start

1. **Install SideScroll templates**
   ```bash
   dotnet new install SideScroll.Templates
   ```

2. **Create a new SideScroll application**
   ```bash
   dotnet new sidescroll.app -o MyApp
   ```

   Or create a solution with library and program projects:
   ```bash
   dotnet new sidescroll.sln -o MyApp
   ```

3. **Run your new app**
   ```bash
   cd MyApp
   dotnet run
   ```

## 📚 Documentation

### For Users
- **[User Guide](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/UserGuide.md)** — How to use SideScroll applications
- **[Live Demo](https://sidescrollui.github.io/SideScroll/)** — Try it in your browser

### For Developers
- **[Project Setup](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/ProjectSetup.md)** — Setting up your first SideScroll project
- **[Development Guide](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Development.md)** — Core concepts and development workflow
- **[Adding Tabs](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/AddingTabs.md)** — Creating custom tabs
- **[TabForms](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/TabForms.md)** — Auto-generated forms for objects
- **[DataRepos](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/DataRepos.md)** — Data storage and management
- **[Bookmarks](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Bookmarks.md)** — Creating shareable links
- **[Charts](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Charts.md)** — Adding interactive charts
- **[Logs](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Logs.md)** — Logging and debugging
- **[Projects](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Projects.md)** — Project configuration
- **[Serializer](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Serializer.md)** — Object serialization

### Additional Resources
- **[Credits](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Credits.md)** — Acknowledgments and third-party libraries
- **[Changelog](https://github.com/SideScrollUI/SideScroll/blob/main/CHANGELOG.md)** — Version history and release notes
- **[Contributing Guide](https://github.com/SideScrollUI/SideScroll/blob/main/CONTRIBUTING.md)** — How to contribute to SideScroll

## 🎯 Sample Projects

- **[SideScroll Samples](https://github.com/SideScrollUI/SideScroll/blob/main/Libraries/SideScroll.Avalonia.Samples/MainWindow.cs)** — Included in the repository
- **[Video Game Database](https://github.com/SideScrollUI/VideoGamesDB)** — External sample project demonstrating real-world usage

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](https://github.com/SideScrollUI/SideScroll/blob/main/LICENSE) file for details.

![Logo](https://raw.githubusercontent.com/SideScrollUI/SideScroll/7b67c08951361310fa23bec67e4f953343bf8af2/Images/Logo/png/SideScroll_40.png)
