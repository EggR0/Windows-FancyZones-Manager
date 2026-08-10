using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Interop;
using FancyZonesHotkeys.FancyZones;

namespace FancyZonesHotkeys.Core
{
    public class HotkeyActionHandler
    {
        private readonly KeyboardHook _hook;
        private readonly Dictionary<string, ActionDefinition> _actionMap;
        private readonly Dictionary<string, string> _internalHotkeyMapping;

        public HotkeyActionHandler(KeyboardHook hook)
        {
            _hook = hook;
            _hook.KeyPressed += Hook_KeyPressed;
            _actionMap = new Dictionary<string, ActionDefinition>();
            _internalHotkeyMapping = new Dictionary<string, string>();
        }

        public void LoadConfig(PresetConfig config)
        {
            _actionMap.Clear();
            _internalHotkeyMapping.Clear();

            var targetMap = new Dictionary<string, Target>();
            if (config.Targets != null)
            {
                foreach (var t in config.Targets)
                {
                    if (!string.IsNullOrEmpty(t.Id))
                    {
                        targetMap[t.Id] = t;
                    }
                }
            }

            if (config.Settings?.AutoGenerateHotkeys == true)
            {
                GenerateAutoHotkeys();
            }

            if (config.Presets != null)
            {
                foreach (var preset in config.Presets)
                {
                    if (string.IsNullOrEmpty(preset.Hotkey)) continue;

                    var def = new ActionDefinition { Hotkey = preset.Hotkey };

                    if (!string.IsNullOrEmpty(preset.TargetId) && targetMap.TryGetValue(preset.TargetId, out var target))
                    {
                        def.Action = target.Action ?? "";
                        def.Monitor = target.Monitor ?? "";
                        def.Layout = target.Layout ?? "";
                        def.Zone = target.Zone;
                    }

                    if (!string.IsNullOrEmpty(preset.Action)) def.Action = preset.Action;
                    if (!string.IsNullOrEmpty(preset.Monitor)) def.Monitor = preset.Monitor;
                    if (!string.IsNullOrEmpty(preset.Layout)) def.Layout = preset.Layout;
                    if (preset.Zone > 0) def.Zone = preset.Zone;
                    if (!string.IsNullOrEmpty(preset.Placement)) def.Placement = preset.Placement;

                    if (string.IsNullOrEmpty(def.Action))
                    {
                        def.Action = def.Zone > 0 ? "zone" : "monitor";
                    }
                    if (string.IsNullOrEmpty(def.Monitor))
                    {
                        def.Monitor = "active";
                    }

                    _actionMap[preset.Hotkey] = def;
                }
            }

            // Finally, register all hotkeys in the map
            foreach (var kvp in _actionMap)
            {
                try
                {
                    var (mod, key) = ParseHotkeyString(kvp.Key);
                    _hook.RegisterHotKey(mod, key);
                    
                    string internalKey = $"{mod}+{key}";
                    _internalHotkeyMapping[internalKey] = kvp.Key;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to register hotkey {kvp.Key}: {ex.Message}");
                }
            }
        }

        private void GenerateAutoHotkeys()
        {
            try
            {
                var data = FancyZonesData.Load();
                var monitors = MonitorManager.GetAllMonitors();
                // Sort monitors by X coordinate left to right
                var sortedMonitors = monitors.OrderBy(m => m.WorkArea.Left).ToList();

                int hotkeyCounter = 1;

                foreach (var monitor in sortedMonitors)
                {
                    // Convert native monitor name to string that might match PowerToys ID
                    string monitorIdFallback = monitor.DeviceName.Replace("\\\\.\\", "").Replace("DISPLAY", "");
                    int.TryParse(monitorIdFallback, out int monitorNum);
                    
                    var appliedLayout = data.AppliedLayouts?.LastOrDefault(a => 
                        a.Device != null && (
                        a.Device.Monitor.Contains(monitor.DeviceName, StringComparison.OrdinalIgnoreCase) ||
                        a.Device.MonitorNumber == monitorNum
                    ));

                    if (appliedLayout != null)
                    {
                        int zoneCount = 0;
                        string targetLayoutId = appliedLayout.AppliedLayout.Uuid;
                        
                        if (appliedLayout.AppliedLayout.Type.Equals("custom", StringComparison.OrdinalIgnoreCase))
                        {
                            var customLayout = data.CustomLayouts?.FirstOrDefault(c => c.Uuid == targetLayoutId);
                            if (customLayout != null)
                            {
                                zoneCount = ZoneCalculator.GetZoneCount(customLayout);
                            }
                        }
                        else
                        {
                            // For priority-grid, grid, etc.
                            zoneCount = appliedLayout.AppliedLayout.ZoneCount;
                        }

                        for (int i = 1; i <= zoneCount; i++)
                        {
                            string hotkeyStr = $"Alt+{hotkeyCounter}";
                            _actionMap[hotkeyStr] = new ActionDefinition
                            {
                                Hotkey = hotkeyStr,
                                Action = "zone",
                                Monitor = monitor.DeviceName,
                                Layout = "@applied",
                                Zone = i
                            };
                            hotkeyCounter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to auto-generate hotkeys: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Hook_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            string internalKey = $"{e.Modifier}+{e.Key}";
            if (_internalHotkeyMapping.TryGetValue(internalKey, out string? originalHotkey))
            {
                if (_actionMap.TryGetValue(originalHotkey, out var def))
                {
                    try
                    {
                        var data = FancyZonesData.Load();
                        WindowManager.ApplyAction(def, data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing hotkey {def.Hotkey}: {ex.Message}");
                    }
                }
            }
        }

        private (ModifierKeys, Keys) ParseHotkeyString(string hotkeyStr)
        {
            ModifierKeys modifiers = ModifierKeys.None;
            Keys key = Keys.None;

            var parts = hotkeyStr.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string p = part.Trim().ToUpperInvariant();
                if (p == "CTRL" || p == "CONTROL") modifiers |= ModifierKeys.Control;
                else if (p == "ALT") modifiers |= ModifierKeys.Alt;
                else if (p == "SHIFT") modifiers |= ModifierKeys.Shift;
                else if (p == "WIN" || p == "WINDOWS") modifiers |= ModifierKeys.Win;
                else
                {
                    if (p.Length == 1 && char.IsDigit(p[0]))
                    {
                        key = (Keys)Enum.Parse(typeof(Keys), "D" + p, true);
                    }
                    else if (p.Length == 1 && char.IsLetter(p[0]))
                    {
                        key = (Keys)Enum.Parse(typeof(Keys), p, true);
                    }
                    else if (Enum.TryParse(p, true, out Keys parsedKey))
                    {
                        key = parsedKey;
                    }
                }
            }

            return (modifiers, key);
        }
    }
}
