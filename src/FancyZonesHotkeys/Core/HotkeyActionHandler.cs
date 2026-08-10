using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.Core
{
    public class HotkeyActionHandler
    {
        private readonly KeyboardHook _hook;
        private readonly Dictionary<string, Preset> _hotkeyPresetMapping;
        private readonly Dictionary<string, string> _internalHotkeyMapping;

        public HotkeyActionHandler(KeyboardHook hook)
        {
            _hook = hook;
            _hook.KeyPressed += Hook_KeyPressed;
            _hotkeyPresetMapping = new Dictionary<string, Preset>();
            _internalHotkeyMapping = new Dictionary<string, string>();
        }

        public void LoadConfig(PresetConfig config)
        {
            _hotkeyPresetMapping.Clear();
            _internalHotkeyMapping.Clear();
            
            if (config.Presets == null) return;

            foreach (var preset in config.Presets)
            {
                if (!string.IsNullOrEmpty(preset.Hotkey))
                {
                    try
                    {
                        var (mod, key) = ParseHotkeyString(preset.Hotkey);
                        _hook.RegisterHotKey(mod, key);
                        
                        string internalKey = $"{mod}+{key}";
                        if (!_internalHotkeyMapping.ContainsKey(internalKey))
                        {
                            _internalHotkeyMapping[internalKey] = preset.Hotkey;
                            _hotkeyPresetMapping[preset.Hotkey] = preset;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to register hotkey {preset.Hotkey}: {ex.Message}");
                    }
                }
            }
        }

        private void Hook_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            string internalKey = $"{e.Modifier}+{e.Key}";
            if (_internalHotkeyMapping.TryGetValue(internalKey, out string? originalHotkey))
            {
                if (_hotkeyPresetMapping.TryGetValue(originalHotkey, out Preset? preset))
                {
                    // FancyZones Data Parsing is missing in C# port!
                    // FancyZones.WindowManager.ApplyZoneToForegroundWindow(zone);
                    Console.WriteLine($"Hotkey pressed: {preset.Hotkey}. (FancyZones layout logic pending implementation)");
                }
            }
        }

        private (ModifierKeys, Keys) ParseHotkeyString(string hotkeyStr)
        {
            ModifierKeys modifiers = ModifierKeys.None;
            Keys key = Keys.None;

            var parts = hotkeyStr.Split('+');
            foreach (var part in parts)
            {
                string p = part.Trim();
                if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                    modifiers |= ModifierKeys.Control;
                else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    modifiers |= ModifierKeys.Alt;
                else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    modifiers |= ModifierKeys.Shift;
                else if (p.Equals("Win", StringComparison.OrdinalIgnoreCase))
                    modifiers |= ModifierKeys.Win;
                else
                {
                    if (Enum.TryParse(p, true, out Keys parsedKey))
                    {
                        key = parsedKey;
                    }
                    else
                    {
                        // Fallback parsing or ignore
                        // throw new ArgumentException($"Unknown key component: {p}");
                    }
                }
            }

            return (modifiers, key);
        }
    }
}
