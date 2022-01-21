using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class Gather
    {
        public int Count = 0;
        public List<string> Options = new List<string>();
        public string PlanetName = "";

        [JsonIgnore]
        public List<string> AllOptions = new List<string>();
    }
}
