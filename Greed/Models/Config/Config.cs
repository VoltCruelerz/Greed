﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Greed.Utils;

namespace Greed.Models.Config
{
    public class Config
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; } = new Version(0,0,0);

        [JsonRequired]
        [JsonProperty(PropertyName = "channel")]
        public string Channel = "live";

        [JsonRequired]
        [JsonProperty(PropertyName = "dirs")]
        public DirConfig Dirs { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "groups")]
        public List<ScalarGroup> Groups { get; set; } = new();

        public void Init()
        {
            Groups.ForEach(g => g.Init());
        }
    }
}
