using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Text.Json.JsonSerializer;

namespace Greed.Models
{
    public class OnlineListing
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "latestGreed")]
        public Version LatestGreed = new("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public List<Metadata> Mods { get; set; } = new List<Metadata>();

        public async static Task<OnlineListing> GetOnlineListing(MainWindow window)
        {
            var client = new HttpClient();
            try
            {
                var result = await client.GetAsync("https://raw.githubusercontent.com/League-of-Greedy-Modders/Greedy-Mods/main/mods.json");
                var json = await result.Content.ReadAsStringAsync();
                var listing = JsonConvert.DeserializeObject<OnlineListing>(json);
                return listing!;
            }
            catch (Exception ex)
            {
                window.PrintAsync(ex.Message + "\n" + ex.StackTrace);
            }
            return new OnlineListing();
        }
    }
}
