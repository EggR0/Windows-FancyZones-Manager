using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Core;

namespace FancyZonesHotkeys.UI
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly HotkeyActionHandler _actionHandler;
        private readonly string _configPath;

        public TrayIconManager(HotkeyActionHandler actionHandler)
        {
            _actionHandler = actionHandler;
            _contextMenu = new ContextMenuStrip();
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "presets.yaml");
            
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = _contextMenu,
                Text = "FancyZones Hotkeys",
                Visible = true
            };
        }

        public void Initialize(PresetConfig config)
        {
            BuildMenu();
            _notifyIcon.ShowBalloonTip(3000, "FancyZones Hotkeys", "Application started and running in the background.", ToolTipIcon.Info);
        }

        private void BuildMenu()
        {
            _contextMenu.Items.Clear();

            var titleItem = new ToolStripMenuItem("FancyZones Hotkeys (v2.0.0)")
            {
                Enabled = false
            };
            _contextMenu.Items.Add(titleItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            _contextMenu.Items.Add("Open Settings (presets.yaml)", null, (s, e) => 
            {
                if (File.Exists(_configPath))
                {
                    Process.Start(new ProcessStartInfo("notepad.exe", _configPath) { UseShellExecute = true });
                }
            });

            _contextMenu.Items.Add("Reload Settings", null, (s, e) => 
            {
                try
                {
                    var newConfig = ConfigManager.LoadConfig(_configPath);
                    _actionHandler.LoadConfig(newConfig);
                    _notifyIcon.ShowBalloonTip(2000, "FancyZones Hotkeys", "Settings reloaded successfully.", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to reload configuration:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            _contextMenu.Items.Add(new ToolStripSeparator());

            _contextMenu.Items.Add("Register Startup", null, (s, e) => 
            {
                CreateStartupShortcut();
            });

            _contextMenu.Items.Add("Unregister Startup", null, (s, e) => 
            {
                RemoveStartupShortcut();
            });

            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
        }

        private void CreateStartupShortcut()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, "FancyZones Hotkeys.lnk");
                
                // Uses WshShell to create shortcut (COM interop not always available, but let's use a simple powershell script for it to be safe)
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Application.ExecutablePath;
                string psCommand = $"$s=(New-Object -COM WScript.Shell).CreateShortcut('{shortcutPath}');$s.TargetPath='{exePath}';$s.WorkingDirectory='{Path.GetDirectoryName(exePath)}';$s.Save()";
                
                var startInfo = new ProcessStartInfo("powershell.exe", $"-NoProfile -Command \"{psCommand}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(startInfo)?.WaitForExit();
                _notifyIcon.ShowBalloonTip(2000, "FancyZones Hotkeys", "Startup shortcut registered.", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register startup:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveStartupShortcut()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, "FancyZones Hotkeys.lnk");
                
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    _notifyIcon.ShowBalloonTip(2000, "FancyZones Hotkeys", "Startup shortcut removed.", ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove startup:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}
