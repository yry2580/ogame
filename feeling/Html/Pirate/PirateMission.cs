using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    class PirateMission
    {
        public List<Pirate> List = new List<Pirate>();
        public int MissionCount => mMissions.Count;
        public int Interval = 120; // 分钟
        public bool IsCross = false;

        Pos mPos = new Pos();
        List<Mission> mMissions = new List<Mission>();

        public void Add(Pirate pirate)
        {
            if (null == pirate) return;
            
            List.Add(pirate);

            var planetName = pirate.PlanetName;
            if (planetName.Length <= 0) return;
            var count = pirate.Count;
            if (count <= 0) return;
            var allOptions = pirate.AllOptions;

            if (allOptions.Count <= 0) return;

            if (pirate.Mode == 0)
            {
                allOptions.ForEach(e =>
                {
                    if (!mPos.Parse(e)) return;

                    Mission mission = new Mission();
                    mission.AddFleet(ShipType.SC, count);
                    mission.PlanetName = planetName;
                    mission.SetTargetPos(new Pos(mPos.X, mPos.Y, mPos.Z));
                    mMissions.Add(mission);
                });
            }
            else if (pirate.Mode == 1)
            {
                allOptions.ForEach(e =>
                {
                    if (!mPos.Parse(e)) return;
                    if (e.Contains("海盗王")) return;

                    Mission mission = new Mission();
                    mission.AddFleet(ShipType.SC, count);
                    mission.PlanetName = planetName;
                    mission.SetTargetPos(new Pos(mPos.X, mPos.Y, mPos.Z));
                    mMissions.Add(mission);
                });
            }
            else
            {
                pirate.Options.ForEach(e =>
                {
                    if (!mPos.Parse(e)) return;
                    
                    Mission mission = new Mission();
                    mission.AddFleet(ShipType.SC, count);
                    mission.PlanetName = planetName;
                    mission.SetTargetPos(new Pos(mPos.X, mPos.Y, mPos.Z));
                    mMissions.Add(mission);
                });
            }
        }

        public Mission GetMission(int index)
        {
            if (index < 0 || index >= mMissions.Count) return null;

            return mMissions[index];
        }
    }
}
