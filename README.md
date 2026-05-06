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
