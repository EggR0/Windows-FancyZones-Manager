using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FancyZonesHotkeys.Config
{
    public class PresetConfig
    {
        [YamlMember(Alias = "targets")]
        public List<Target>? Targets { get; set; }

        [YamlMember(Alias = "presets")]
        public List<Preset>? Presets { get; set; }
    }

    public class Target
    {
        [YamlMember(Alias = "id")]
        public string? Id { get; set; }

        [YamlMember(Alias = "action")]
        public string? Action { get; set; }

        [YamlMember(Alias = "monitor")]
        public string? Monitor { get; set; }

        [YamlMember(Alias = "layout")]
        public string? Layout { get; set; }

        [YamlMember(Alias = "zone")]
        public int Zone { get; set; }
    }

    public class Preset
    {
        [YamlMember(Alias = "hotkey")]
        public string? Hotkey { get; set; }

        [YamlMember(Alias = "action")]
        public string? Action { get; set; }

        [YamlMember(Alias = "target")]
        public string? TargetId { get; set; }

        [YamlMember(Alias = "monitor")]
        public string? Monitor { get; set; }

        [YamlMember(Alias = "layout")]
        public string? Layout { get; set; }

        [YamlMember(Alias = "zone")]
        public int Zone { get; set; }
        
        [YamlMember(Alias = "placement")]
        public string? Placement { get; set; }
    }
}
