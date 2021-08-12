﻿using System;
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
        Pos mPos = new Pos();

        public void Add(int ship0, string count0, int ship1, string count1, string planetName)
        {
            int _count0 = string.IsNullOrWhiteSpace(count0) ? 0 : int.Parse(count0);
            int _count1 = string.IsNullOrWhiteSpace(count1) ? 0 : int.Parse(count1);

            if (_count0 <= 0 && _count1 <= 0) return;
            if (!mPos.Parse(planetName)) return;

            Mission mission = new Mission();
            mission.AddFleet(Expedition.ShipOptions[ship0], _count0);
            mission.AddFleet(Expedition.ShipOptions[ship1], _count1);
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
