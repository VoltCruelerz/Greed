using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models
{
    public class LocalizedText
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "text")]
        public List<List<string>> Text { get; set; } = new();

        public List<string> GetKeys => Text.Select(p => p[0]).ToList();

        public List<string> GetValues => Text.Select(p => p[1]).ToList();

        public bool HasKey(string key) => Text.Any(p => p[0] == key);

        public string? GetValue(string key) => Text.FirstOrDefault(p => p[0] == key)?[1];

        public void Upsert(List<string> kv) => Upsert(kv[0], kv[1]);

        public void Upsert(string key, string value)
        {
            for (int i = 0; i < Text.Count; i++)
            {
                if (Text[i][0] == key)
                {
                    Text[i][1] = value;
                    return;
                }
            }

            Text.Add(new List<string>(){ key, value });
        }
    }
}
