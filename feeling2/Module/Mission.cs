using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feeling
{
    interface IMission
    {
        void AddFleet(ShipType shipType, int count);
        void SetTargetPos(Pos pos);
        void SetTargetPos(string target);
    }

    class Mission: IMission
    {
        public Pos TargetPos = new Pos();
        public PlanetType PlanetType = PlanetType.Star;
        public List<Fleet> FleetList = new List<Fleet>();
        public string PlanetName = "";

        public int X => TargetPos.X;
        public int Y => TargetPos.Y;
        public int Z => TargetPos.Z;

        public void AddFleet(ShipType shipType, int count)
        {
            if (count <= 0) return;

            FleetList.Add(new Fleet { ShipType = shipType, Count = count });
        }

        public void SetTargetPos(Pos pos)
        {
            TargetPos = pos;
        }

        public void SetTargetPos(string target)
        {
            TargetPos.Parse(target);
        }
    }
}
