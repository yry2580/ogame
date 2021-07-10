using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class Fleet
    {
        public ShipType ShipType = ShipType.None;
        public int Count = 0;
    }

    class FleetQueue
    {
        public int Count = 0;
        public int MaxCount = 0;
        public int ExCount = 0;
        public int ExMaxCount = 0;
    }
}
