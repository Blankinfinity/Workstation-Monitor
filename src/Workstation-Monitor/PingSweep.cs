using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Workstation_Monitor
{
    public class PingSweep
    {
        private string BaseIP = "192.168.1.";
        private int StartIP = 1;
        private int StopIP = 255;
        private string ip;

        private int timeout = 100;
        private int nFound = 0;

        static object lockObj = new object();
        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;

        public void RunPingSweep_Sync()
        {
            nFound = 0;

            stopWatch.Start();
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();

            for (int i = StartIP; i <= StopIP; i++)
            {
                ip = BaseIP + i.ToString();
                System.Net.NetworkInformation.PingReply rep = p.Send(ip, timeout);

                if (rep.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    Console.WriteLine("{ 0} is up: ({ 1}ms)", ip, rep.RoundtripTime);
                    nFound++;
                }
            }

            stopWatch.Stop();
            ts = stopWatch.Elapsed;

            Console.WriteLine(nFound.ToString() + " devices found! Elapsed time: " + ts.ToString(), "Synchronous");
        }

        public async Task RunPingSweep_Async()
        {
            nFound = 0;

            var tasks = new List<Task<PingReply>>();

            stopWatch.Start();

            for (int i = StartIP; i <= StopIP; i++)
            {
                ip = BaseIP + i.ToString();

                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                var task = PingAndUpdateAsync(p, ip);
                tasks.Add(task);
            }

            var results = Task.WhenAll(tasks);

            try
            {
                results.Wait();
            }
            catch (AggregateException)
            {

            }

            if (results.Status == TaskStatus.RanToCompletion)
            {
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                foreach (var result in results.Result)
                {

                    if (result.Address.ToString() != "0.0.0.0")
                    {
                        Console.WriteLine("{0} is up: ({1}ms)", result.Address.ToString(), result.RoundtripTime.ToString());
                    }
                }
                Console.WriteLine(nFound.ToString() + " devices found! Elapsed time: " + ts.ToString(), "Asynchronous");
            }
        }

        private async Task<PingReply> PingAndUpdateAsync(System.Net.NetworkInformation.Ping ping, string ip)
        {
            var reply = await ping.SendPingAsync(ip, timeout);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                lock (lockObj)
                { 
                    nFound++;
                    return reply;
                }
            }
            return reply;
        }
    }
}
