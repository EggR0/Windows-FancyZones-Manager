using System;
using FancyZonesHotkeys.Config;
using FancyZonesHotkeys.Interop;

namespace FancyZonesHotkeys.FancyZones
{
    public static class ZoneCalculator
    {
        public static NativeMethods.RECT CalculateTargetRect(NativeMethods.RECT workArea, Zone zone)
        {
            int monitorWidth = workArea.Right - workArea.Left;
            int monitorHeight = workArea.Bottom - workArea.Top;

            int targetWidth = (int)Math.Round(monitorWidth * (zone.WidthPercent / 100.0));
            int targetHeight = (int)Math.Round(monitorHeight * (zone.HeightPercent / 100.0));
            int targetX = workArea.Left + (int)Math.Round(monitorWidth * (zone.XPercent / 100.0));
            int targetY = workArea.Top + (int)Math.Round(monitorHeight * (zone.YPercent / 100.0));

            return new NativeMethods.RECT
            {
                Left = targetX,
                Top = targetY,
                Right = targetX + targetWidth,
                Bottom = targetY + targetHeight
            };
        }
    }
}
