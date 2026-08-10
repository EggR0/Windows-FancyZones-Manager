using System;
using System.Drawing;
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

        public TrayIconManager(HotkeyActionHandler actionHandler)
        {
            _actionHandler = actionHandler;
            _contextMenu = new ContextMenuStrip();
            
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
