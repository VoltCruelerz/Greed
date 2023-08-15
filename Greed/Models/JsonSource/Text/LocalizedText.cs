using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.JsonSource.Text
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LocalizedText : Source
    {
        [JsonProperty(PropertyName = "text")]
        public List<List<string>> Text { get; set; } = new();

        public List<string> GetKeys => Text.Select(p => p[0]).ToList();

        public List<string> GetValues => Text.Select(p => p[1]).ToList();

        public LocalizedText(string path) : base(path)
        {
            var manifest = JObject.Parse(Json);
            var arr = (JArray)manifest["text"]!;
            Text = new List<List<string>>();
            foreach (var item in arr)
            {
                var kv = ((JArray)item).ToList();
                Text.Add(new List<string>()
                {
                    kv[0].ToString(),
                    kv[1].ToString()
                });
            }
        }

        public override Source Merge(Source other)
        {
            var otherText = (LocalizedText)other;
            otherText.Text.ForEach(okv => Upsert(okv));
            Json = JsonConvert.SerializeObject(this, Formatting.None);
            return this;
        }

        public override string Minify()
        {
            Json = JsonConvert.SerializeObject(this, Formatting.None);
            return Json;
        }

        public override Source Clone()
        {
            return new LocalizedText(SourcePath);
        }

        public bool HasKey(string key) => Text.Any(p => p[0] == key);

        public string? GetValue(string key) => Text.FirstOrDefault(p => p[0] == key)?[1];

        private void Upsert(List<string> kv) => Upsert(kv[0], kv[1]);

        private void Upsert(string key, string value)
        {
            var index = Text.FindIndex(kv => kv[0] == key);
            if (index != -1)
            {
                Text[index][1] = value;
            } else
            {
                Text.Add(new List<string>() { key, value });
            }
        }
    }
}
