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
        private readonly Dictionary<string, Zone> _hotkeyZoneMapping;
        private readonly Dictionary<string, string> _internalHotkeyMapping;

        public HotkeyActionHandler(KeyboardHook hook)
        {
            _hook = hook;
            _hook.KeyPressed += Hook_KeyPressed;
            _hotkeyZoneMapping = new Dictionary<string, Zone>();
            _internalHotkeyMapping = new Dictionary<string, string>();
        }

        public void LoadPreset(Preset preset)
        {
            _hotkeyZoneMapping.Clear();
            _internalHotkeyMapping.Clear();
            
            if (preset.Zones == null) return;

            foreach (var zone in preset.Zones)
            {
                if (!string.IsNullOrEmpty(zone.Hotkey))
                {
                    try
                    {
                        var (mod, key) = ParseHotkeyString(zone.Hotkey);
                        _hook.RegisterHotKey(mod, key);
                        
                        // Create a unique key string for the dictionary mapping
                        string internalKey = $"{mod}+{key}";
                        if (!_internalHotkeyMapping.ContainsKey(internalKey))
                        {
                            _internalHotkeyMapping[internalKey] = zone.Hotkey;
                            _hotkeyZoneMapping[zone.Hotkey] = zone;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to register hotkey {zone.Hotkey}: {ex.Message}");
                    }
                }
            }
        }

        private void Hook_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            string internalKey = $"{e.Modifier}+{e.Key}";
            if (_internalHotkeyMapping.TryGetValue(internalKey, out string? originalHotkey))
            {
                if (_hotkeyZoneMapping.TryGetValue(originalHotkey, out Zone? zone))
                {
                    FancyZones.WindowManager.ApplyZoneToForegroundWindow(zone);
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
                        throw new ArgumentException($"Unknown key component: {p}");
                    }
                }
            }

            if (key == Keys.None)
                throw new ArgumentException("No primary key found in hotkey string.");

            return (modifiers, key);
        }
    }
}
