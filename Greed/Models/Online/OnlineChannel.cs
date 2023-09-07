using Greed.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Greed.Models.Online
{
    public class OnlineChannel
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "latestGreed")]
        public Version LatestGreed = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public List<OnlineMod> Mods { get; set; } = new();

        public async static Task<OnlineChannel> GetOnlineListing(MainWindow window)
        {
            var client = new HttpClient();
            try
            {
                var result = await client.GetAsync("https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/beta.json");// TODO - go back to live
                var json = await result.Content.ReadAsStringAsync();
                var listing = JsonConvert.DeserializeObject<OnlineChannel>(json);
                return listing!;
            }
            catch (Exception ex)
            {
                window.CriticalAlertPopup("Failed to Load Online Mod Listing", ex);
            }
            return new OnlineChannel();
        }
    }
}
