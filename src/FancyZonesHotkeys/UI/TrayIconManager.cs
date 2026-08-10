using System;
using System.Drawing;
using System.Windows.Forms;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Core;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.UI
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly HotkeyActionHandler _actionHandler;
        private PresetConfig? _currentConfig;

        public TrayIconManager(HotkeyActionHandler actionHandler)
        {
            _actionHandler = actionHandler;
            _contextMenu = new ContextMenuStrip();
            
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Fallback icon, will try to load custom one
                ContextMenuStrip = _contextMenu,
                Text = "FancyZones Hotkeys",
                Visible = true
            };
        }

        public void Initialize(PresetConfig config)
        {
            _currentConfig = config;
            BuildMenu();
        }

        private void BuildMenu()
        {
            _contextMenu.Items.Clear();

            // Config settings title
            var titleItem = new ToolStripMenuItem("FancyZones Hotkeys (v2.0.0)")
            {
                Enabled = false
            };
            _contextMenu.Items.Add(titleItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            if (_currentConfig?.Presets != null)
            {
                var presetsItem = new ToolStripMenuItem("Presets");
                foreach (var preset in _currentConfig.Presets)
                {
                    var item = new ToolStripMenuItem(preset.Key, null, (s, e) =>
                    {
                        // Uncheck all other preset items
                        foreach (ToolStripItem child in presetsItem.DropDownItems)
                        {
                            if (child is ToolStripMenuItem mi) mi.Checked = false;
                        }
                        // Check selected item
                        if (s is ToolStripMenuItem selectedItem)
                        {
                            selectedItem.Checked = true;
                            // Load preset
                            _actionHandler.LoadPreset(preset.Value);
                        }
                    });
                    
                    if (preset.Key == _currentConfig.DefaultPreset)
                    {
                        item.Checked = true;
                    }
                    
                    presetsItem.DropDownItems.Add(item);
                }
                _contextMenu.Items.Add(presetsItem);
            }

            _contextMenu.Items.Add(new ToolStripSeparator());

            // Exit
            _contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}
