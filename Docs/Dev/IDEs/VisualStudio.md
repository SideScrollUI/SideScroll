# Visual Studio

1. Download SideScroll
    - `git clone https://github.com/garyhertel/sidescroll.git`
2. Install a C# IDE
   - Visual Studio 2022 recommended (community edition is fine)
3. Open `SideScroll.sln` in IDE
4. Start SideScroll in Debugger
    - It's recommended to always run it in debug mode
  
## Visual Studio 2022

* Customizing (you can search for these options in the Options Menu)
  - Insert tabs instead of spaces (spaces only make sense if you don't use IDEs)
    - Text Editor -> C# -> Tabs
	  - Select `Keep tabs`
  - Disable automatically adding `}`
	  - Text Editor -> C# -> General
	  - Uncheck `Automatic brace completion`
  - Enable word wrap
	  - Text Editor -> C# -> General
	  - Check `Word wrap`
  - Stopping on all(?) Exceptions
	  - Tools -> Options -> Debugging -> General
	  - Check `Enable Just My Code`

## Extensions

* Avalonia for Visual Studio
  - Previewer and templates for Avalonia applications and libraries

## NUnit 3 Test Adapter
* GUI for running unit tests in Visual Studio
* Install
  - Tools -> Extensions and Updates -> Online
    - Search for `NUnit 3 Test Adapter` and then install
* Setup
  - Test Menu -> Test Settings -> Default Processor Architecture -> x64
* Displaying
  - Test->Windows->Test Explorer
* Using
  - Right click on any test and select `Run` or `Debug`

# Suggested Visual Studio Addons

## dotMem
* For tracking down memory leaks
* Commercial with 5 day eval trial
  - Better than Visual Studio at tracking down memory leaks
  - Alternatives?
