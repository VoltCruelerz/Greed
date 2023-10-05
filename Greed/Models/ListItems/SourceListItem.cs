﻿using Greed.Models.Json;
using System;
using System.IO;
using Greed.Exceptions;

namespace Greed.Models.ListItem
{
    public class SourceListItem
    {
        public string DeltaSymbol { get; set; }

        public string Folder { get; set; }

        public string Name { get; set; }

        public SourceListItem(JsonSource s)
        {
            DeltaSymbol = "+";
            if (File.Exists(s.GoldPath))
            {
                try
                {
                    var goldStr = new JsonSource(s.GoldPath).Minify();
                    var modStr = s.Minify();
                    DeltaSymbol = modStr == goldStr ? "" : Constants.UNI_DELTA;
                }
                catch (Exception ex)
                {
                    throw new ModLoadException("Gold copy exists, but failed to parse JSON: " + s.GoldPath, ex);
                }
            }
            Folder = s.Folder;
            Name = s.Mergename;
        }
    }
}
