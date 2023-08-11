using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Entities
{
    public class Entity : JsonSource
    {
        public Entity(string path) : base(path)
        {
        }
    }
}
