﻿using Greed.Exceptions;
using Greed.Models.Json;
using System;
using System.IO;

namespace Greed.Models.ListItem
{
    public class SourceListItem
    {
        public string DeltaSymbol { get; set; }

        public string Folder { get; set; }

        public string Filename { get; set; }

        public string ShortFilename { get; set; }

        public string Mergename { get; set; }

        public bool IsEven { get; set; }

        public SourceListItem(JsonSource s, bool isEven)
        {
            IsEven = isEven;
            DeltaSymbol = "+";
            if (File.Exists(s.GoldPath))
            {
                try
                {
                    var goldStr = new JsonSource(s.GoldPath).Minify();
                    var modStr = s.Minify();
                    DeltaSymbol = modStr == goldStr ? "" : Utils.Constants.UNI_DELTA;
                }
                catch (Exception ex)
                {
                    throw new ModLoadException("Gold copy exists, but failed to parse JSON: " + s.GoldPath, ex);
                }
            }
            Folder = s.Folder;
            Filename = s.Filename;
            Mergename = s.Mergename;
            ShortFilename = s.Filename.Replace(Mergename, "*");
            IsEven = isEven;
        }
    }
}
