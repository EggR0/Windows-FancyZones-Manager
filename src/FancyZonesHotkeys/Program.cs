using System;
using System.IO;
using System.Windows.Forms;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Core;
using FancyZonesHotkeys.Interop;
using FancyZonesHotkeys.UI;

namespace FancyZonesHotkeys
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "presets.yaml");
            PresetConfig config;

            try
            {
                config = ConfigManager.LoadConfig(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration:\n{ex.Message}", "FancyZones Hotkeys Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var hook = new KeyboardHook())
            {
                var actionHandler = new HotkeyActionHandler(hook);
                
                // Load default preset
                if (!string.IsNullOrEmpty(config.DefaultPreset) && config.Presets != null && config.Presets.TryGetValue(config.DefaultPreset, out var defaultPreset))
                {
                    actionHandler.LoadPreset(defaultPreset);
                }

                using (var trayIcon = new TrayIconManager(actionHandler))
                {
                    trayIcon.Initialize(config);
                    Application.Run();
                }
            }
        }
    }
}
