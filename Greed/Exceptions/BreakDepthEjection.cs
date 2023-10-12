﻿using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Exceptions
{
    /// <summary>
    /// This is thrown when a break depth ejection needs to happen with a predecessor.
    ///
    /// This *could* be handled by gobs of conditional return statements, but this is cleaner, even if it is a ~goto.
    /// </summary>
    public class BreakDepthEjection : Exception
    {
        private Action<JArray, int> Handler { get; set; }
        private readonly int BreakDepth;


        public BreakDepthEjection(Action<JArray,int> handler, int breakDepth) : base("If you see this, you probably have a typo in your array path or break depth.")
        {
            Handler = handler;
            BreakDepth = breakDepth;
        }

        public void TryHandle(int currentDepth, JArray arr, int index)
        {
            if (BreakDepth == currentDepth)
            {
                Handler(arr, index);
                return;
            }

            throw this;
        }
    }
}
