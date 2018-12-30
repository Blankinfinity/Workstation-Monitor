using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace Workstation_Monitor
{
    class Program
    {
        

        static void Main(string[] args)
        {
            PingSweep pingSweep = new PingSweep();
            pingSweep.RunPingSweep_Async();
            Console.ReadLine();
        }
    }
}
