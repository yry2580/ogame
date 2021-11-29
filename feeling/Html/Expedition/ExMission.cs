using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class ExMission
    {
        public List<Mission> List= new List<Mission>();
        public bool IsCross = false;
        public int Interval = 120; // 分钟

        Pos mPos = new Pos();

        public void Add(string planetName, int ship0, string count0, int ship1, string count1, int ship2, string count2)
        {
            int _count0 = string.IsNullOrWhiteSpace(count0) ? 0 : int.Parse(count0);
            int _count1 = string.IsNullOrWhiteSpace(count1) ? 0 : int.Parse(count1);
            int _count2 = string.IsNullOrWhiteSpace(count2) ? 0 : int.Parse(count2);

            if (_count0 <= 0 && _count1 <= 0 && _count2 <= 0) return;
            if (!mPos.Parse(planetName)) return;

            Mission mission = new Mission();
            mission.AddFleet(Expedition.ShipOptions[ship0], _count0);
            mission.AddFleet(Expedition.ShipOptions[ship1], _count1);
            mission.AddFleet(Expedition.ShipOptions[ship2], _count2);
            mission.PlanetName = planetName;
            mission.SetTargetPos(new Pos(mPos.X, mPos.Y, 16));

            if (mission.FleetList.Count < 0) return;

            List.Add(mission);
        }

        public Mission GetMission(int index)
        {
            if (index < 0 || index >= List.Count) return null;

            return List[index];
        }
    }
}
