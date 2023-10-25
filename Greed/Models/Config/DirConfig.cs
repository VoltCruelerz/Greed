﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Config
{
    public class DirConfig
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public string Mods { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "export")]
        public string Export { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "sins")]
        public string Sins { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "download")]
        public string Download { get; set; } = string.Empty;
    }
}
