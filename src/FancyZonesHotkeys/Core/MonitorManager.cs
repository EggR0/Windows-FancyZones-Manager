using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.Core
{
    public class MonitorInfo
    {
        public string DeviceName { get; set; } = "";
        public int DisplayNumber { get; set; }
        public bool IsPrimary { get; set; }
        public Rectangle Bounds { get; set; }
        public Rectangle WorkArea { get; set; }
    }

    public static class MonitorManager
    {
        public static List<MonitorInfo> GetAllMonitors()
        {
            var monitors = new List<MonitorInfo>();
            foreach (var screen in Screen.AllScreens)
            {
                monitors.Add(new MonitorInfo
                {
                    DeviceName = screen.DeviceName,
                    DisplayNumber = GetDisplayNumberFromDeviceName(screen.DeviceName),
                    IsPrimary = screen.Primary,
                    Bounds = screen.Bounds,
                    WorkArea = screen.WorkingArea
                });
            }
            return monitors;
        }

        public static int GetDisplayNumberFromDeviceName(string deviceName)
        {
            var match = Regex.Match(deviceName, @"DISPLAY(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
            {
                return num;
            }
            return 9999;
        }

        public static MonitorInfo GetMonitorInfoForWindow(IntPtr windowHandle)
        {
            IntPtr monitorHandle = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (monitorHandle == IntPtr.Zero)
            {
                throw new Exception("Could not determine the monitor for the active window.");
            }

            var info = new NativeMethods.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
            
            if (!NativeMethods.GetMonitorInfo(monitorHandle, ref info))
            {
                throw new Exception("GetMonitorInfo failed.");
            }

            var allMonitors = GetAllMonitors();
            var matchedMonitor = allMonitors.FirstOrDefault(m => m.DeviceName.Equals(info.szDevice, StringComparison.OrdinalIgnoreCase));
            
            if (matchedMonitor != null)
            {
                return matchedMonitor;
            }

            return new MonitorInfo
            {
                DeviceName = info.szDevice,
                DisplayNumber = GetDisplayNumberFromDeviceName(info.szDevice),
                IsPrimary = false,
                Bounds = new Rectangle(info.rcMonitor.Left, info.rcMonitor.Top, info.rcMonitor.Right - info.rcMonitor.Left, info.rcMonitor.Bottom - info.rcMonitor.Top),
                WorkArea = new Rectangle(info.rcWork.Left, info.rcWork.Top, info.rcWork.Right - info.rcWork.Left, info.rcWork.Bottom - info.rcWork.Top)
            };
        }

        public static MonitorInfo ResolveMonitorSelector(string selector, MonitorInfo currentMonitor, List<MonitorInfo> allMonitors)
        {
            var sortedMonitors = allMonitors.OrderBy(m => m.DisplayNumber).ThenBy(m => m.DeviceName).ToList();

            if (string.IsNullOrEmpty(selector) || selector.Equals("active", StringComparison.OrdinalIgnoreCase) || selector.Equals("current", StringComparison.OrdinalIgnoreCase))
            {
                return currentMonitor;
            }

            if (int.TryParse(selector, out int displayNum))
            {
                var monitor = allMonitors.FirstOrDefault(m => m.DisplayNumber == displayNum);
                if (monitor == null) throw new Exception($"No monitor with display number '{displayNum}' was found.");
                return monitor;
            }

            if (selector.Equals("primary", StringComparison.OrdinalIgnoreCase))
            {
                var monitor = allMonitors.FirstOrDefault(m => m.IsPrimary);
                if (monitor == null) throw new Exception("No primary monitor was found.");
                return monitor;
            }

            if (selector.Equals("next", StringComparison.OrdinalIgnoreCase))
            {
                int currentIndex = sortedMonitors.FindIndex(m => m.DeviceName == currentMonitor.DeviceName);
                if (currentIndex < 0) throw new Exception($"Current monitor '{currentMonitor.DeviceName}' was not found in the monitor list.");
                return sortedMonitors[(currentIndex + 1) % sortedMonitors.Count];
            }

            if (selector.Equals("previous", StringComparison.OrdinalIgnoreCase))
            {
                int currentIndex = sortedMonitors.FindIndex(m => m.DeviceName == currentMonitor.DeviceName);
                if (currentIndex < 0) throw new Exception($"Current monitor '{currentMonitor.DeviceName}' was not found in the monitor list.");
                int previousIndex = currentIndex == 0 ? sortedMonitors.Count - 1 : currentIndex - 1;
                return sortedMonitors[previousIndex];
            }

            var matchedDevice = allMonitors.FirstOrDefault(m => m.DeviceName.Equals(selector, StringComparison.OrdinalIgnoreCase));
            if (matchedDevice != null)
            {
                return matchedDevice;
            }

            throw new Exception($"Unknown monitor selector '{selector}'. Use active, primary, next, previous, a display number, or a device name like \\.\\DISPLAY2.");
        }
    }
}
