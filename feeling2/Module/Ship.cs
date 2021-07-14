using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    public class Ship
    {
        public static IDictionary<ShipType, string> ShipNameDict = new Dictionary<ShipType, string>
        {
            { ShipType.SC, "小型运输机" },
            { ShipType.LC, "大型运输舰" },
            { ShipType.LF, "轻型战斗机" },
            { ShipType.HF, "重型战斗机" },
            { ShipType.CC, "巡洋舰" },
            { ShipType.BB, "战列舰" },
            { ShipType.CS, "殖民舰" },
            { ShipType.RS, "回收舰" },
            { ShipType.SPY, "间谍卫星" },
            { ShipType.BF, "轰炸机" },
            { ShipType.SUN, "太阳能卫星" },
            { ShipType.DD, "毁灭者" },
            { ShipType.DS, "死星" },
            { ShipType.BC, "战斗巡洋舰" },
        };

        public static string GetShipId(ShipType shipType)
        {
            return $"ship{(int)shipType}";
        }

        public static string GetShipName(ShipType shipType)
        {
            ShipNameDict.TryGetValue(shipType, out string name);
            return name ?? "";
        }
    }
}
