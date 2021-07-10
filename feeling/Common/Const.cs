using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    public enum ShipType
    {
        None = 0,
        SC = 202, // 小型运输机
        LC, // 大型运输舰
        LF, // 轻型战斗机
        HF, // 重型战斗机
        CC, // 巡洋舰
        BB, // 战列舰
        CS, // 殖民舰
        RS, // 回收舰
        SPY, // 间谍卫星
        BF, // 轰炸机
        SUN, // 太阳能
        DD, // 毁灭者
        DS, // 死星
        BC, // 战斗巡洋舰
    }

    public enum PlanetType
    {
        Star = 1,
        Ruin,
        Moon,
    }
}
