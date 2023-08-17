﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Diff
{
    public class DiffResult
    {
        public string Gold = "";
        public string Greedy = "";
        public string Diff = "";

        public DiffResult(string gold, string greedy, string diff)
        {
            Gold = Lineify(gold);
            Greedy = Lineify(greedy);
            Diff = diff;
        }

        private string Lineify(string json)
        {
            var lines = json
                .Split('\n')
                .Select((line, index) => index.ToString("D5") + " ｜ " + line);
            return string.Join("\n", lines);
        }
    }
}
