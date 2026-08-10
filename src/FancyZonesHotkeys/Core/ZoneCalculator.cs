using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FancyZonesHotkeys.FancyZones;

namespace FancyZonesHotkeys.Core
{
    public class ZoneRect
    {
        public int Zone { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public static class ZoneCalculator
    {
        public static CustomLayout ResolveCustomLayout(FancyZonesData data, string layoutReference, MonitorInfo monitor)
        {
            if (layoutReference.Equals("@applied", StringComparison.OrdinalIgnoreCase))
            {
                int monitorNumber = monitor.DisplayNumber;

                var appliedEntry = data.AppliedLayouts
                    .Where(a => a.Device.MonitorNumber == monitorNumber && a.AppliedLayout.Type.Equals("custom", StringComparison.OrdinalIgnoreCase))
                    .LastOrDefault();

                if (appliedEntry == null)
                    throw new Exception($"No custom applied FancyZones layout found for monitor {monitorNumber}.");

                var layout = data.CustomLayouts.FirstOrDefault(l => l.Uuid == appliedEntry.AppliedLayout.Uuid);
                if (layout == null)
                    throw new Exception($"Applied layout '{appliedEntry.AppliedLayout.Uuid}' was not found in custom-layouts.json.");

                return layout;
            }

            var matchingLayout = data.CustomLayouts.FirstOrDefault(l => l.Uuid.Equals(layoutReference, StringComparison.OrdinalIgnoreCase) || l.Name.Equals(layoutReference, StringComparison.OrdinalIgnoreCase));
            if (matchingLayout == null)
                throw new Exception($"Layout '{layoutReference}' was not found in custom-layouts.json.");

            return matchingLayout;
        }

        public static List<ZoneRect> GetGridZoneRects(CustomLayout layout, MonitorInfo monitor)
        {
            int rows = layout.Info.Rows;
            int columns = layout.Info.Columns;
            int spacing = layout.Info.ShowSpacing ? layout.Info.Spacing : 0;

            int effectiveWidth = monitor.WorkArea.Width - (spacing * Math.Max(columns - 1, 0));
            int effectiveHeight = monitor.WorkArea.Height - (spacing * Math.Max(rows - 1, 0));

            if (effectiveWidth <= 0 || effectiveHeight <= 0)
                throw new Exception($"Monitor work area is too small for layout '{layout.Name}'.");

            var columnWidths = ConvertPercentagesToLengths(effectiveWidth, layout.Info.ColumnsPercentage ?? new List<int>());
            var rowHeights = ConvertPercentagesToLengths(effectiveHeight, layout.Info.RowsPercentage ?? new List<int>());

            var columnStarts = new List<int>();
            int cursor = monitor.WorkArea.Left;
            foreach (var width in columnWidths)
            {
                columnStarts.Add(cursor);
                cursor += width + spacing;
            }

            var rowStarts = new List<int>();
            cursor = monitor.WorkArea.Top;
            foreach (var height in rowHeights)
            {
                rowStarts.Add(cursor);
                cursor += height + spacing;
            }

            var zoneCells = new Dictionary<int, (int MinRow, int MaxRow, int MinColumn, int MaxColumn)>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int zoneId = layout.Info.CellChildMap?[r][c] ?? 0;
                    if (!zoneCells.ContainsKey(zoneId))
                    {
                        zoneCells[zoneId] = (r, r, c, c);
                    }
                    else
                    {
                        var existing = zoneCells[zoneId];
                        zoneCells[zoneId] = (
                            Math.Min(existing.MinRow, r),
                            Math.Max(existing.MaxRow, r),
                            Math.Min(existing.MinColumn, c),
                            Math.Max(existing.MaxColumn, c)
                        );
                    }
                }
            }

            var rects = new List<ZoneRect>();
            foreach (var kvp in zoneCells.OrderBy(k => k.Key))
            {
                int zoneId = kvp.Key;
                var cell = kvp.Value;

                int left = columnStarts[cell.MinColumn];
                int top = rowStarts[cell.MinRow];

                int width = 0;
                for (int c = cell.MinColumn; c <= cell.MaxColumn; c++)
                {
                    width += columnWidths[c];
                }
                width += spacing * (cell.MaxColumn - cell.MinColumn);

                int height = 0;
                for (int r = cell.MinRow; r <= cell.MaxRow; r++)
                {
                    height += rowHeights[r];
                }
                height += spacing * (cell.MaxRow - cell.MinRow);

                rects.Add(new ZoneRect
                {
                    Zone = zoneId + 1, // 1-indexed
                    X = left,
                    Y = top,
                    Width = width,
                    Height = height
                });
            }

            return rects;
        }

        public static List<ZoneRect> GetCanvasZoneRects(CustomLayout layout, MonitorInfo monitor)
        {
            double refWidth = layout.Info.RefWidth;
            double refHeight = layout.Info.RefHeight;

            if (refWidth <= 0 || refHeight <= 0)
                throw new Exception($"Canvas layout '{layout.Name}' has an invalid reference size.");

            double scaleX = monitor.WorkArea.Width / refWidth;
            double scaleY = monitor.WorkArea.Height / refHeight;

            var rects = new List<ZoneRect>();
            if (layout.Info.Zones != null)
            {
                for (int i = 0; i < layout.Info.Zones.Count; i++)
                {
                    var zone = layout.Info.Zones[i];
                    rects.Add(new ZoneRect
                    {
                        Zone = i + 1,
                        X = monitor.WorkArea.Left + (int)Math.Round(zone.X * scaleX),
                        Y = monitor.WorkArea.Top + (int)Math.Round(zone.Y * scaleY),
                        Width = (int)Math.Round(zone.Width * scaleX),
                        Height = (int)Math.Round(zone.Height * scaleY)
                    });
                }
            }
            return rects;
        }

        private static List<int> ConvertPercentagesToLengths(int totalLength, List<int> percentages)
        {
            var result = new List<int>();
            int previousBoundary = 0;
            int runningPercent = 0;

            for (int i = 0; i < percentages.Count; i++)
            {
                runningPercent += percentages[i];
                int boundary = (i == percentages.Count - 1) 
                    ? totalLength 
                    : (int)Math.Round(totalLength * (runningPercent / 10000.0));

                result.Add(boundary - previousBoundary);
                previousBoundary = boundary;
            }
            return result;
        }

        public static Rectangle ClampWindowRectToMonitor(int x, int y, int width, int height, MonitorInfo monitor)
        {
            int safeWidth = Math.Min(width, monitor.WorkArea.Width);
            int safeHeight = Math.Min(height, monitor.WorkArea.Height);

            int maxLeft = monitor.WorkArea.Right - safeWidth;
            int maxTop = monitor.WorkArea.Bottom - safeHeight;

            return new Rectangle(
                Math.Max(monitor.WorkArea.Left, Math.Min(x, maxLeft)),
                Math.Max(monitor.WorkArea.Top, Math.Min(y, maxTop)),
                safeWidth,
                safeHeight
            );
        }

        public static int GetZoneCount(CustomLayout layout)
        {
            if (layout.Type.Equals("canvas", StringComparison.OrdinalIgnoreCase))
            {
                return layout.Info.Zones?.Count ?? 0;
            }
            else if (layout.Type.Equals("grid", StringComparison.OrdinalIgnoreCase))
            {
                if (layout.Info.CellChildMap == null) return 0;
                
                var uniqueZones = new HashSet<int>();
                foreach (var row in layout.Info.CellChildMap)
                {
                    foreach (var cell in row)
                    {
                        uniqueZones.Add(cell);
                    }
                }
                return uniqueZones.Count;
            }
            return 0;
        }
    }
}
