using System;
using System.Runtime.InteropServices;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.FancyZones
{
    public static class WindowManager
    {
        public static void ApplyZoneToForegroundWindow(Config.Zone zone)
        {
            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return;

            // Restore if maximized or minimized
            int style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
            if ((style & NativeMethods.WS_MAXIMIZE) == NativeMethods.WS_MAXIMIZE || 
                (style & NativeMethods.WS_MINIMIZE) == NativeMethods.WS_MINIMIZE)
            {
                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
            }

            // Get Monitor Info
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            NativeMethods.MONITORINFO monitorInfo = new NativeMethods.MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFO));

            if (NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                NativeMethods.RECT targetRect = ZoneCalculator.CalculateTargetRect(monitorInfo.rcWork, zone);
                
                int cx = targetRect.Right - targetRect.Left;
                int cy = targetRect.Bottom - targetRect.Top;
                
                NativeMethods.SetWindowPos(
                    hWnd,
                    IntPtr.Zero,
                    targetRect.Left,
                    targetRect.Top,
                    cx,
                    cy,
                    NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE
                );
            }
        }
    }
}
