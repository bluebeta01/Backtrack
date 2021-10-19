using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTrack
{
    static class Arguments
    {
        public static string configFilePath { get; set; }

        public static void parse(string[] args)
        {
            if(args.Length != 0)
                configFilePath = args[0];
        }
    }
}
