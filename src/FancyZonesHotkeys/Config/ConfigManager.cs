using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FancyZonesHotkeys.Config
{
    public static class ConfigManager
    {
        public static PresetConfig LoadConfig(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Configuration file not found at {path}");
            }

            var yaml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<PresetConfig>(yaml);
        }
    }
}
