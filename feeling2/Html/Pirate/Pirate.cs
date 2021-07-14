using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace feeling
{
    class Pirate
    {
        public int Index = 0;
        public int Mode = 0;
        public int Count = 0;
        public List<string> Options = new List<string>();
        public string PlanetName = "";

        [JsonIgnore]
        public List<string> AllOptions = new List<string>();

    }
}
