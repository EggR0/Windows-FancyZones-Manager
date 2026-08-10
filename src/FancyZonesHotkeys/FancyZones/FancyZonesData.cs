using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FancyZonesHotkeys.FancyZones
{
    public class FancyZonesData
    {
        public string RootPath { get; private set; }
        public List<CustomLayout> CustomLayouts { get; private set; }
        public List<AppliedLayoutWrapper> AppliedLayouts { get; private set; }

        public FancyZonesData(string rootPath, List<CustomLayout> customLayouts, List<AppliedLayoutWrapper> appliedLayouts)
        {
            RootPath = rootPath;
            CustomLayouts = customLayouts;
            AppliedLayouts = appliedLayouts;
        }

        public static FancyZonesData Load()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string rootPath = Path.Combine(localAppData, "Microsoft", "PowerToys", "FancyZones");

            string customLayoutsPath = Path.Combine(rootPath, "custom-layouts.json");
            string appliedLayoutsPath = Path.Combine(rootPath, "applied-layouts.json");

            var customLayouts = new List<CustomLayout>();
            if (File.Exists(customLayoutsPath))
            {
                string json = File.ReadAllText(customLayoutsPath);
                var parsed = JsonSerializer.Deserialize<CustomLayoutsFile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed?.CustomLayouts != null)
                {
                    customLayouts = parsed.CustomLayouts;
                }
            }

            var appliedLayouts = new List<AppliedLayoutWrapper>();
            if (File.Exists(appliedLayoutsPath))
            {
                string json = File.ReadAllText(appliedLayoutsPath);
                var parsed = JsonSerializer.Deserialize<AppliedLayoutsFile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed?.AppliedLayouts != null)
                {
                    appliedLayouts = parsed.AppliedLayouts;
                }
            }

            return new FancyZonesData(rootPath, customLayouts, appliedLayouts);
        }
    }
}
