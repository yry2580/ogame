using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class GatherMission
    {
        public List<Gather> List = new List<Gather>();
        public int MissionCount => mMissions.Count;
        public bool IsCross = false;

        Pos mPos = new Pos();
        List<Mission> mMissions = new List<Mission>();

        public void Add(Gather gather)
        {
            if (null == gather) return;
            
            List.Add(gather);

            var planetName = gather.PlanetName;
            if (planetName.Length <= 0) return;
            var count = gather.Count;
            var allOptions = gather.AllOptions;

            if (allOptions.Count <= 0) return;

            gather.Options.ForEach(e =>
            {
                if (!mPos.Parse(e)) return;
                if (e.Trim() == planetName) return;

                Mission mission = new Mission();
                // mission.AddFleet(ShipType, count);
                mission.FleetList.Add(new Fleet { ShipType = ShipType.LC, Count = count });
                mission.PlanetName = e;
                mission.SetTargetPos(planetName);
                mMissions.Add(mission);
            });
        }

        public Mission GetMission(int index)
        {
            if (index < 0 || index >= mMissions.Count) return null;

            return mMissions[index];
        }
    }
}
