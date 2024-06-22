# Visual Studio Code


## Installing

* https://code.visualstudio.com/download
  - `sudo apt install ./code_1.15.1-1502903936_amd64.deb`
	- Install dotnet core
		- https://www.microsoft.com/net/core
	- After starting, go to extensions (Cubes icon on left)
    - Install "C#" by Microsoft
      - Reload
      - Load Folder
    - Try building
      - `Ctrl-Shift-B`
    - Access Terminal
      - `Ctrl + ~`

# Update Settings

* Settings
  - Enable Soft Wrap
    - View->Toggle Word Wrap

* Click Debug icon on left

* Windows
  - Get rid of annoying beep in PowerShell (that vscode uses)
    - https://superuser.com/questions/1113429/disable-powershell-beep-on-backspace
    - make sure to restart after


`dotnet restore SideScroll.sln`

- Files
  - Make sure the `launch.preLaunchTask` matches the `tasks.label`

## launch.json
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "preLaunchTask": "build",
        },
    ]
}
```
## tasks.json
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
        }
    ]
}
```