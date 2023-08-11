using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Greed.Models
{
    class Metadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string SinsVersion { get; set; }
        public List<string> Dependencies { get; set; }// TODO
        public List<string> Conflicts { get; set; }// TODO
        public int Priority { get; set; }

        public Metadata(string id, string json)
        {
            Id = id;
            var obj = JObject.Parse(json);
            Name = obj["name"].ToString();
            Description = obj["description"].ToString();
            Author = obj["author"].ToString();
            Version = obj["version"].ToString();
            Url = obj["url"].ToString();
            SinsVersion = obj["sinsVersion"].ToString();
            Dependencies = new List<string>();
            Conflicts = new List<string>();
        }
    }
}
