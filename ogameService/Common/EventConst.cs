using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameService
{
    public delegate void OgEventHandler();
    public delegate void OgEventHandler<T>(T data);
}
