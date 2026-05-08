# Windrose Captain's Console

## About this tool

Windrose Captain's Console is an unofficial, local desktop admin panel for **Windrose Dedicated Server** management.

It is built for server owners who want one place to:

- install or update the dedicated server with SteamCMD
- load and edit Captain/server settings
- load and edit World settings
- create, import, delete, and switch worlds
- start, stop, and restart the server
- back up and restore important server data
- schedule backups and reboots
- review warnings and logs
- manage community mod workflow

Instead of manually browsing folders and editing config files by hand, Captain's Console gives you a dedicated UI for the most common Windrose server tasks.

## Features

- SteamCMD install support
- Windrose dedicated server install and update support
- first-run provisioning help after install
- Captain/server config editing
- World config editing
- discovered world management
- active world switching
- start / stop / restart controls
- backup and restore tools
- scheduled backups
- scheduled reboots
- logbook with filtering and export
- community mod workflow tools

## Requirements

- Windows operating system
- A valid Windrose Dedicated Server installation, or use Captain's Console to install one
- Internet connection if you want to install or update the server through SteamCMD
- Enough permissions to read and write your Windrose server files
- It is recommended to run the app on the same machine that hosts the server, or on a machine with direct access to the server files


## Compile the application

Captain's Console is a Windows desktop application built from the C# source files under `desktop`.

### What you need

- Windows
- .NET Framework C# compiler available at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
- PowerShell

### Build steps

1. Open PowerShell in the project root.
2. Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\desktop\build-desktop.ps1
```

3. If the build succeeds, the compiled application will be created at:

```text
dist\WindroseCaptainsConsole.exe
```

## Run the application

After building, launch the app by opening:

```text
dist\WindroseCaptainsConsole.exe
```

You can also run it from PowerShell:

```powershell
.\dist\WindroseCaptainsConsole.exe
```

If Windows blocks the file because it came from another machine or download source, right-click the executable, open `Properties`, and allow it to run if needed.

## Configure app updates

Captain's Console can show its own version at the top of the app and check whether a newer installer is available.

### How it works

The app looks for a plain text file named:

```text
update-feed-url.txt
```

The file should contain one URL only: the URL to a JSON manifest that describes the latest release.

The app checks for this file in:

- the same folder as `WindroseCaptainsConsole.exe`
- the parent folder
- the grandparent folder

### Manifest format

The JSON manifest should look like this:

```json
{
  "version": "0.9.6.0",
  "downloadUrl": "https://your-download-host.example.com/WindroseCaptainsConsoleSetup_v0.9.6.0.exe",
  "notes": "Bug fixes, backup improvements, and RCON UI updates."
}
```

`downloadUrl` can also be named `installerUrl` if you prefer.

### Example setup

1. Upload your latest installer somewhere public.
2. Upload a JSON manifest like [update-manifest.example.json](./update-manifest.example.json).
3. Create `update-feed-url.txt` next to the exe and paste the manifest URL into it.

Example `update-feed-url.txt` contents:

```text
https://your-download-host.example.com/update-manifest.json
```

An example feed file is included here:

- [update-feed-url.example.txt](./update-feed-url.example.txt)
- [update-manifest.example.json](./update-manifest.example.json)

## Build the installer

Captain's Console also includes an Inno Setup installer project so you can ship a normal Windows setup wizard with install location selection, shortcuts, and uninstall support.

### What you need

- Inno Setup 6 installed
- PowerShell

### Build steps

1. Open PowerShell in the project root.
2. Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\build-installer.ps1
```

3. If Inno Setup is installed, the finished installer will be created in:

```text
dist\
```

The installer script itself is stored at:

```text
installer\WindroseCaptainsConsole.iss
```

## Why use it

Captain's Console is meant to reduce the repeated pain points of hosting a Windrose server:

- manual config editing
- hunting for the right server files
- forgetting backup steps
- switching between tools for worlds, logs, and mods
- repeating first-time setup steps manually

## Best for

- private server owners
- co-op groups
- community hosts
- admins who want a desktop control panel instead of manual file management

## Important notes

- It is still safest to stop the server before making config changes.
- Some gameplay systems are still limited by what Windrose officially exposes in its server files.
- This tool helps manage official server files more easily; it does not replace the game's own dedicated server structure.

## Release status

This is an early public release of Captain's Console.
It is already useful for real server management and improvements will happen based on community feedback and additional server settings provided by the developers of Windrose.
