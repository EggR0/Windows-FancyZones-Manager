# FancyZones Hotkeys (v2.0.0)

A native .NET 10 application that manages window positions (FancyZones-style) via global hotkeys.

## Changes in v2.0.0
- Completely rewritten from PowerShell to C# (.NET 10 WinForms) to avoid false-positive malware detections by Windows Defender.
- Full mathematical port of the PowerToys FancyZones layout logic (both `grid` and `canvas` formats, including spacing and ref-resolution).
- Includes multi-monitor support with target modifiers like `primary`, `active`, `next`, and `previous`.
- Single-file native executable with self-contained .NET runtime and partial trimming for optimal size.
- Uses `YamlDotNet` for parsing `preset.yaml` and `System.Text.Json` for parsing PowerToys JSON configs.
- Runs silently in the background with a system tray icon.

## Building
Run `build.ps1` to publish the single executable and package it via Inno Setup (using `setup.iss`).

## Usage
1. Modify `preset.yaml` to define your zones and hotkeys.
2. Run `FancyZonesHotkeys.exe`.
3. An icon will appear in the system tray. Use the hotkeys to snap the active window!
