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

            string configPath = Path.Combine(Application.StartupPath, "presets.yaml");
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
                
                // Load all hotkeys
                actionHandler.LoadConfig(config);

                // Log detected monitors for debugging
                var monitors = MonitorManager.GetAllMonitors();
                Console.WriteLine($"Detected {monitors.Count} monitor(s):");
                foreach (var m in monitors)
                {
                    Console.WriteLine($"  Display #{m.DisplayNumber} ({m.DeviceName}) | Primary={m.IsPrimary} | Bounds={m.Bounds} | WorkArea={m.WorkArea}");
                }

                using (var trayIcon = new TrayIconManager(actionHandler, hook))
                {
                    trayIcon.Initialize(config);
                    Application.Run();
                }
            }
        }
    }
}
