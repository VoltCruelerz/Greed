using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Greed.Models.Mutations
{
    public class Mutation
    {
        [JsonProperty(PropertyName = "type")]
        public MutationType Type { get; set; } = MutationType.NONE;

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "value")]
        public JObject Value { get; set; } = new();

        [JsonProperty(PropertyName = "breakIndex")]
        public string Break { get; set; } = string.Empty;

        public void Execute(JObject obj)
        {
            var pathTerms = Path.Split(".");

            // Find all workspaces
            List<JToken> workspaces = new();
            FindWorkspacesObj(obj, pathTerms, 0, workspaces);

            // Do work
        }

        private void FindWorkspacesObj(JObject obj, string[] path, int i, List<JToken> workspaces)
        {
            var term = path[i];
            var child = obj[term];
            if (child is null)
            {
                return;
            }
            if (child is JArray array)
            {
                FindWorkspacesArr(array, path, i, workspaces);
            }
            else if (path.Length - 1 == i)
            {
                workspaces.Add(child);
            }
            else if (child is JObject childObj)
            {
                FindWorkspacesObj(childObj, path, i + 1, workspaces);
            }
            else
            {
                throw new Exception("Unrecognized type for " + child.ToString());
            }
        }

        private void FindWorkspacesArr(JArray arr, string[] path, int i, List<JToken> workspaces)
        {
            foreach (var item in arr)
            {
                if (item is JArray subArray)
                {
                    FindWorkspacesArr(subArray, path, i, workspaces);
                }
                else if (path.Length - 1 == i)
                {
                    workspaces.Add(item);
                }
                else if (item is JObject subObject)
                {
                    FindWorkspacesObj(subObject, path, i + 1, workspaces);
                }
                else
                {
                    throw new Exception("Unrecognized type for " + item.ToString());
                }
            }
        }
    }
}
