using System;
using System.Collections.Generic;
using System.Drawing;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.Core
{
    public static class WindowManager
    {
        public static void ApplyAction(ActionDefinition definition, FancyZones.FancyZonesData data)
        {
            IntPtr windowHandle = NativeMethods.GetForegroundWindow();
            if (windowHandle == IntPtr.Zero)
                throw new Exception("No foreground window is available.");

            var allMonitors = MonitorManager.GetAllMonitors();
            var sourceMonitor = MonitorManager.GetMonitorInfoForWindow(windowHandle);
            var targetMonitor = MonitorManager.ResolveMonitorSelector(definition.Monitor, sourceMonitor, allMonitors);

            if (definition.Action.Equals("zone", StringComparison.OrdinalIgnoreCase))
            {
                if (definition.Zone <= 0)
                    throw new Exception($"Preset '{definition.Hotkey}' is missing a valid zone number.");

                string layoutRef = string.IsNullOrEmpty(definition.Layout) ? "@applied" : definition.Layout;
                var layout = ZoneCalculator.ResolveCustomLayout(data, layoutRef, targetMonitor);

                List<ZoneRect> rects;
                if (layout.Type.Equals("grid", StringComparison.OrdinalIgnoreCase))
                    rects = ZoneCalculator.GetGridZoneRects(layout, targetMonitor);
                else if (layout.Type.Equals("canvas", StringComparison.OrdinalIgnoreCase))
                    rects = ZoneCalculator.GetCanvasZoneRects(layout, targetMonitor);
                else
                    throw new Exception($"Unsupported FancyZones layout type '{layout.Type}'.");

                var targetRect = rects.Find(r => r.Zone == definition.Zone);
                if (targetRect == null)
                    throw new Exception($"Layout '{layout.Name}' does not contain zone {definition.Zone}.");

                InvokeWindowMove(windowHandle, targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);
                Console.WriteLine($"[{definition.Hotkey}] -> {targetMonitor.DeviceName} / {layout.Name} / zone {definition.Zone} ({targetRect.X}, {targetRect.Y}, {targetRect.Width}x{targetRect.Height})");
            }
            else if (definition.Action.Equals("monitor", StringComparison.OrdinalIgnoreCase))
            {
                var windowRect = GetWindowRect(windowHandle);
                string placement = string.IsNullOrEmpty(definition.Placement) ? "preserve-relative" : definition.Placement;
                var targetRect = GetMonitorPlacementRect(sourceMonitor, targetMonitor, windowRect, definition);
                
                InvokeWindowMove(windowHandle, targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);
                Console.WriteLine($"[{definition.Hotkey}] -> monitor {targetMonitor.DisplayNumber} ({targetMonitor.DeviceName}) using {placement} ({targetRect.X}, {targetRect.Y}, {targetRect.Width}x{targetRect.Height})");
            }
            else
            {
                throw new Exception($"Unsupported action '{definition.Action}'.");
            }
        }

        private static Rectangle GetWindowRect(IntPtr windowHandle)
        {
            if (!NativeMethods.GetWindowRect(windowHandle, out NativeMethods.RECT rect))
                throw new Exception("GetWindowRect failed.");

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        private static Rectangle GetMonitorPlacementRect(MonitorInfo source, MonitorInfo target, Rectangle windowRect, ActionDefinition def)
        {
            string placement = string.IsNullOrEmpty(def.Placement) ? "preserve-relative" : def.Placement;
            switch (placement.ToLowerInvariant())
            {
                case "maximize":
                    return target.WorkArea;
                case "center":
                    int w = Math.Min(windowRect.Width, target.WorkArea.Width);
                    int h = Math.Min(windowRect.Height, target.WorkArea.Height);
                    return new Rectangle(
                        target.WorkArea.Left + (int)Math.Round((target.WorkArea.Width - w) / 2.0),
                        target.WorkArea.Top + (int)Math.Round((target.WorkArea.Height - h) / 2.0),
                        w, h
                    );
                case "preserve-size":
                    int x1 = target.WorkArea.Left + (windowRect.Left - source.WorkArea.Left);
                    int y1 = target.WorkArea.Top + (windowRect.Top - source.WorkArea.Top);
                    return ZoneCalculator.ClampWindowRectToMonitor(x1, y1, windowRect.Width, windowRect.Height, target);
                case "top-left":
                    return ZoneCalculator.ClampWindowRectToMonitor(target.WorkArea.Left, target.WorkArea.Top, windowRect.Width, windowRect.Height, target);
                case "custom":
                    int customX = target.WorkArea.Left + (def.X ?? 0);
                    int customY = target.WorkArea.Top + (def.Y ?? 0);
                    int customW = def.Width ?? windowRect.Width;
                    int customH = def.Height ?? windowRect.Height;
                    return ZoneCalculator.ClampWindowRectToMonitor(customX, customY, customW, customH, target);
                case "preserve-relative":
                default:
                    double widthRatio = source.WorkArea.Width > 0 ? windowRect.Width / (double)source.WorkArea.Width : 1.0;
                    double heightRatio = source.WorkArea.Height > 0 ? windowRect.Height / (double)source.WorkArea.Height : 1.0;
                    double xRatio = source.WorkArea.Width > 0 ? (windowRect.Left - source.WorkArea.Left) / (double)source.WorkArea.Width : 0.0;
                    double yRatio = source.WorkArea.Height > 0 ? (windowRect.Top - source.WorkArea.Top) / (double)source.WorkArea.Height : 0.0;

                    int w2 = (int)Math.Round(target.WorkArea.Width * widthRatio);
                    int h2 = (int)Math.Round(target.WorkArea.Height * heightRatio);
                    int x2 = target.WorkArea.Left + (int)Math.Round(target.WorkArea.Width * xRatio);
                    int y2 = target.WorkArea.Top + (int)Math.Round(target.WorkArea.Height * yRatio);

                    return ZoneCalculator.ClampWindowRectToMonitor(x2, y2, w2, h2, target);
            }
        }

        private static void InvokeWindowMove(IntPtr windowHandle, int x, int y, int width, int height)
        {
            if (NativeMethods.IsIconic(windowHandle) || NativeMethods.IsZoomed(windowHandle))
            {
                NativeMethods.ShowWindow(windowHandle, NativeMethods.SW_RESTORE);
            }

            uint flags = NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW;
            bool result = NativeMethods.SetWindowPos(windowHandle, IntPtr.Zero, x, y, width, height, flags);

            if (!result)
                throw new Exception("SetWindowPos failed.");
        }
    }

    public class ActionDefinition
    {
        public string Hotkey { get; set; } = "";
        public string Action { get; set; } = "";
        public string Monitor { get; set; } = "";
        public string Layout { get; set; } = "";
        public int Zone { get; set; }
        public string Placement { get; set; } = "";
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
