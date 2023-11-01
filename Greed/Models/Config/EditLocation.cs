using Greed.Models.Mutations;
using Greed.Models.Mutations.Paths;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static Greed.Models.Config.GlobalScalar;

namespace Greed.Models.Config
{
    public class EditLocation
    {
        [JsonProperty(PropertyName = "path")]
        public string RawPath { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "condition")]
        public string RawCondition { get; set; } = string.Empty;

        [JsonIgnore]
        public List<ActionPath>? NodePath { get; set; }

        [JsonIgnore]
        public Resolvable? Condition { get; set; }

        public void Init()
        {
            NodePath = ActionPath.Build(RawPath);
            Condition = string.IsNullOrEmpty(RawCondition) ? null : Resolvable.GenerateResolvable(RawCondition);
        }

        public int Exec(JObject root, GlobalScalar parent)
        {
            if (NodePath == null) throw new InvalidOperationException("Never called Init()");

            int changes = 0;
            NodePath[0].DoWork(root, NodePath, 0, new(), (token, variables, depth) =>
            {
                if (token == null) return;
                if (Condition != null)
                {
                    var pass = Resolvable.IsTruthy(Condition.Exec(root, variables), root, variables);
                    if (!pass) return;
                }

                if (token is JObject obj)
                {
                    var fp = (FieldPath)NodePath[^1];
                    var child = obj[fp.Name];
                    if (child == null) return;
                    if (parent.Type == ScalarType.DOUBLE)
                    {
                        obj[fp.Name] = child!.Value<double>() * parent.Value;
                        changes++;
                    }
                    else if (parent.Type == ScalarType.INT)
                    {
                        obj[fp.Name] = (int)(child!.Value<int>() * parent.Value);
                        changes++;
                    }
                }
                else if (token is JArray arr)
                {
                    var i = arr.IndexOf(token);
                    if (parent.Type == ScalarType.DOUBLE)
                    {
                        arr[i] = token.Value<double>() * parent.Value;
                        changes++;
                    }
                    else if (parent.Type == ScalarType.INT)
                    {
                        arr[i] = (int)(token.Value<int>() * parent.Value);
                        changes++;
                    }
                }
                if (changes == 0)
                {
                    throw new Exception($"Failed to find work for global {parent.Name}::{RawPath}");
                }
            });
            return changes;
        }
    }
}
