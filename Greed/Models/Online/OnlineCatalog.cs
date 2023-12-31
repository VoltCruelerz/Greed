﻿using Greed.Controls.Popups;
using Greed.Exceptions;
using Greed.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Greed.Models.Online
{
    public class OnlineCatalog
    {
        private const string ALPHA_CHANNEL = "alpha";
        private const string ALPHA_PATH = "https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/catalogs/1.0.0_alpha.json";

        private const string BETA_CHANNEL = "beta";
        private const string BETA_PATH = "https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/catalogs/1.0.0_beta.json";

        private const string LIVE_CHANNEL = "live";
        private const string LIVE_PATH = "https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/catalogs/1.0.0.json";

        [JsonRequired]
        [JsonProperty(PropertyName = "latestGreed")]
        public Version LatestGreed = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "latestSins")]
        public Version LatestSins = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public List<OnlineMod> Mods { get; set; } = new();

        public async static Task<OnlineCatalog> GetOnlineListing()
        {
            var client = new HttpClient();
            try
            {
                var result = await client.GetAsync(GetChannelPath());
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new CatalogLoadException($"HTTP Status Code was {result.StatusCode}{Environment.NewLine}{result.ReasonPhrase}", new HttpRequestException());
                }
                var json = await result.Content.ReadAsStringAsync();
                var listing = JsonConvert.DeserializeObject<OnlineCatalog>(json);
                return listing!;
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.ThrowAsync("Failed to Load Online Mod Listing", ex);
            }
            return new OnlineCatalog();
        }

        private static string GetChannelPath()
        {
            var channel = Settings.GetChannel();
            return channel switch
            {
                ALPHA_CHANNEL => ALPHA_PATH,
                BETA_CHANNEL => BETA_PATH,
                LIVE_CHANNEL => LIVE_PATH,
                "" => LIVE_PATH,
                _ => channel,
            };
        }
    }
}
