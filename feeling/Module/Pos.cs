using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace feeling
{
    class Pos
    {
        public int X = 0;
        public int Y = 0;
        public int Z = 0;

        public Pos()
        {
        }

        public Pos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Parse(string content)
        {
            var mat = Regex.Match(content, @"(?<x>[1-9]):(?<y>\d{1,3}):(?<z>\d{1,2})");
            if (mat.Success)
            {
                X = int.Parse(mat.Groups["x"].Value);
                Y = int.Parse(mat.Groups["y"].Value);
                Z = int.Parse(mat.Groups["z"].Value);
                return true;
            }

            return false;
        }
    }
}
