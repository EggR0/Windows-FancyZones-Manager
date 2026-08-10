using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FancyZonesHotkeys.FancyZones
{
    public class CustomLayoutsFile
    {
        [JsonPropertyName("custom-layouts")]
        public List<CustomLayout> CustomLayouts { get; set; } = new List<CustomLayout>();
    }

    public class CustomLayout
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("info")]
        public LayoutInfo Info { get; set; } = new LayoutInfo();
    }

    public class LayoutInfo
    {
        // Grid
        [JsonPropertyName("rows")]
        public int Rows { get; set; }

        [JsonPropertyName("columns")]
        public int Columns { get; set; }

        [JsonPropertyName("show-spacing")]
        public bool ShowSpacing { get; set; }

        [JsonPropertyName("spacing")]
        public int Spacing { get; set; }

        [JsonPropertyName("columns-percentage")]
        public List<int>? ColumnsPercentage { get; set; }

        [JsonPropertyName("rows-percentage")]
        public List<int>? RowsPercentage { get; set; }

        [JsonPropertyName("cell-child-map")]
        public List<List<int>>? CellChildMap { get; set; }

        // Canvas
        [JsonPropertyName("ref-width")]
        public double RefWidth { get; set; }

        [JsonPropertyName("ref-height")]
        public double RefHeight { get; set; }

        [JsonPropertyName("zones")]
        public List<CanvasZone>? Zones { get; set; }
    }

    public class CanvasZone
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class AppliedLayoutsFile
    {
        [JsonPropertyName("applied-layouts")]
        public List<AppliedLayoutWrapper> AppliedLayouts { get; set; } = new List<AppliedLayoutWrapper>();
    }

    public class AppliedLayoutWrapper
    {
        [JsonPropertyName("device")]
        public DeviceInfo Device { get; set; } = new DeviceInfo();

        [JsonPropertyName("applied-layout")]
        public AppliedLayout AppliedLayout { get; set; } = new AppliedLayout();
    }

    public class DeviceInfo
    {
        [JsonPropertyName("monitor")]
        public string Monitor { get; set; } = "";

        [JsonPropertyName("monitor-number")]
        public int MonitorNumber { get; set; }
    }

    public class AppliedLayout
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("zone-count")]
        public int ZoneCount { get; set; }
    }
}
