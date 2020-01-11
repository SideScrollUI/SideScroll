# Getting Started

1. Download Atlas
    - `git clone https://github.com/garyhertel/atlas.git`
2. Install a C# IDE
   - Visual Studio 2019 recommended (community edition is fine)
3. Open `Atlas.sln` in IDE
4. Start Atlas in Debugger
    - It's recommended to always run it in debug mode
5. Configure Paths
    - Select `Settings` to change any of the default locations
    
## Visual Studio 2019
* Installing `Custom Document Well` for vertical tabs
  - https://tabsstudio.com/documentation/installing-custom-document-well-for-visual-studio-2019.html
  - [Official Support on the roadmap](https://developercommunity.visualstudio.com/idea/467369/vertical-group-tab.html)

## Visual Studio 2017

* Customizing (you can search for these options in the Options Menu)
  - Insert tabs instead of spaces (spaces only make sense if you don't use IDEs)
    - Tools -> Options -> Text Editor -> C# -> Tabs
	  - Select `Keep tabs`
  - Disable automatically adding `}`
	- Tools -> Options -> Text Editor -> C#
	  - Uncheck `Automatic brace completion`
  - Enable word wrap
	- Tools -> Options -> Text Editor -> C#
	  - Check `Word wrap`
  - Stopping on all(?) Exceptions
	- Tools -> Options -> Debugging -> General
	-   Check `Enable Just My Code`

## Extensions

* Avalonia for Visual Studio
  - Previewer and templates for Avalonia applications and libraries

## Productivity Power Tools
* Shows your open files as tabs on the left side instead of tabs across the top
* Visual Studio 2017
			https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.ProductivityPowerPack2017
* Configuring
  - Tools Menu
    - Productivity Power Tools
      - Custom Document Well
        - Set `Place tabs on the` = `Left`
        - Set `Maximum tab size` = `300`

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
