using Greed.Models.Mutations;
using Greed.Models.Mutations.Operations.Functions;
using Greed.Models.Mutations.Paths;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Greed.Models.Config.GlobalScalar;
using System.Xml.Linq;

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

        public bool Exec(JObject root, GlobalScalar parent)
        {
            if (NodePath == null) throw new InvalidOperationException("Never called Init()");

            bool madeAChange = false;
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
                    if (obj[fp.Name] == null) return;
                    if (parent.Type == GlobalType.DOUBLE)
                    {
                        obj[fp.Name] = obj[fp.Name]!.Value<double>() * parent.Value;
                        madeAChange = true;
                    }
                    else if (parent.Type == GlobalType.INT)
                    {
                        obj[fp.Name] = (int)(obj[fp.Name]!.Value<int>() * parent.Value);
                        madeAChange = true;
                    }
                }
                else if (token is JArray arr)
                {
                    var i = arr.IndexOf(token);
                    if (parent.Type == GlobalType.DOUBLE)
                    {
                        arr[i] = token.Value<double>() * parent.Value;
                        madeAChange = true;
                    }
                    else if (parent.Type == GlobalType.INT)
                    {
                        arr[i] = (int)(token.Value<int>() * parent.Value);
                        madeAChange = true;
                    }
                }
                if (!madeAChange)
                {
                    throw new Exception($"Failed to find work for global {parent.Name}::{RawPath}");
                }
            });
            return madeAChange;
        }
    }
}
