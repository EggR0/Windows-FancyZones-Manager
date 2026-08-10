using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FancyZonesHotkeys.Config
{
    public class PresetConfig
    {
        [YamlMember(Alias = "default_preset")]
        public string? DefaultPreset { get; set; }

        [YamlMember(Alias = "presets")]
        public Dictionary<string, Preset>? Presets { get; set; }
    }

    public class Preset
    {
        [YamlMember(Alias = "zones")]
        public List<Zone>? Zones { get; set; }
    }

    public class Zone
    {
        [YamlMember(Alias = "hotkey")]
        public string? Hotkey { get; set; }

        [YamlMember(Alias = "width_percent")]
        public double WidthPercent { get; set; }

        [YamlMember(Alias = "height_percent")]
        public double HeightPercent { get; set; }

        [YamlMember(Alias = "x_percent")]
        public double XPercent { get; set; }

        [YamlMember(Alias = "y_percent")]
        public double YPercent { get; set; }
    }
}
