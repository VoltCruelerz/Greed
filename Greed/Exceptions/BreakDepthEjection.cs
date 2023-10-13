using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly int ResolutionDepth;


        public BreakDepthEjection(Action<JArray, int> handler, int resolutionDepth) : base("If you see this, you probably have a typo in your array path or break depth.")
        {
            Handler = handler;
            ResolutionDepth = resolutionDepth;
        }

        public void TryHandle(int currentDepth, JArray arr, int index, Dictionary<string, Variable> variables)
        {
            // Remove out-of-scope variables
            var varList = variables.Values.Where(v => v.ScopeDepth > currentDepth).ToList();
            varList.ForEach(v => variables.Remove(v.Name));

            // Handle if this is the appropriate place
            if (ResolutionDepth == currentDepth)
            {
                Handler(arr, index);
                return;
            }

            // Go back up the call chain if this isn't the appropriate place.
            throw this;
        }
    }
}
