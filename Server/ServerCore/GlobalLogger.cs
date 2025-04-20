using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public static class GlobalLogger
    {
        public static Action<string>? WriteLog { get; set; }
    }
}
