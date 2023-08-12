﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Greed.Models.JsonSource.Entities
{
    public class EntityManifest : Source
    {
        [JsonProperty(PropertyName = "ids")]
        public readonly List<string> Ids;

        public EntityManifest(string path) : base(path)
        {
            NeedsGold = true;
            var manifest = JObject.Parse(Json);
            var arr = (JArray)manifest["ids"];
            Ids = arr.Select(i => i.ToString()).ToList();
        }

        public void Upsert(string key)
        {
            if (!Ids.Contains(key))
            {
                Ids.Add(key);
            }
        }
    }
}
