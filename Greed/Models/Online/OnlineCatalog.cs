﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Greed.Models.Online
{
    public class OnlineCatalog
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "latestGreed")]
        public Version LatestGreed = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "latestSins")]
        public Version LatestSins = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public List<OnlineMod> Mods { get; set; } = new();

        public async static Task<OnlineCatalog> GetOnlineListing(MainWindow window)
        {
            var client = new HttpClient();
            try
            {
                var result = await client.GetAsync("https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/catalogs/1.0.0.json");
                var json = await result.Content.ReadAsStringAsync();
                var listing = JsonConvert.DeserializeObject<OnlineCatalog>(json);
                return listing!;
            }
            catch (Exception ex)
            {
                window.CriticalAlertPopup("Failed to Load Online Mod Listing", ex);
            }
            return new OnlineCatalog();
        }
    }
}
