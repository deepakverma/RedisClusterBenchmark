using ClusterBenchmark;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterBenchmark
{
    public class Program
    {
        private long _requests = 0;
        private long _responses = 0;
        private readonly Random _rand = new Random();

        static void Main(string[] args)
        {
            Program prog = new Program();
            if(File.Exists("args.txt"))
            {
                //args = File.ReadAllText("args.txt").Split(null);

            }
            var options = Options.Parse(args);
            Socket rpsserver = null;

            if (options == null) return;
            if (options.verbose)
            {
                DisplayConfigSettings(options);
            }
            if (options.servermode)
            {
                int listeningport = options.RPSport;
                BenchmarkServer.StartListening(options.RPSport);
                return;
            }
            else
            {
                if (options.RPSServer != null)
                {
                    rpsserver = BenchmarkClient.StartClient(options.RPSServer, options.RPSport);
                    if (rpsserver == null || rpsserver.Connected == false)
                    {
                        return;
                    }
                }
            }
            IClient client = new StackExchangeClient(options);
            if (!client.CreateClients())
            {
                return;
            }
            string[] keyNames;
            if ((keyNames = client.SetupCluster()) == null)
            {
                Console.Write("Setting up cluster failed");
            }
            prog.SendRequests(client, keyNames, options);
            if (!prog.ReportRPS(options, rpsserver))
            {
                Console.WriteLine("There was an error");
            }

        }

        private static void DisplayConfigSettings(Options options)
        {
            Console.WriteLine("Executing with following parameters:");
            foreach (PropertyInfo prop in typeof(Options).GetProperties())
            {
                Console.WriteLine("{0,-15} = {1}", prop.Name, prop.GetValue(options, null));
            }
        }



        public void SendRequests(IClient client, string[] keysarr, Options option)
        {
            for (int i = 0; i < option.asyncRequests; i++)
            {
                SendRequestAsync(client, i % option.clients, keysarr[i % keysarr.Length]);
            }

        }

        public void SendRequestAsync(IClient client, int clientID, string val)
        {
            Interlocked.Increment(ref _requests);
            client.StringGetAsync(clientID, val).ContinueWith(t =>
            {
                Interlocked.Increment(ref _responses);
                SendRequestAsync(client, clientID, val);
            });
        }

        public bool ReportRPS(Options option, Socket rpsServer)
        {
            Console.WriteLine("Warmup for {0}ms...", option.warmup);
            Thread.Sleep(option.warmup);
            DateTime start = DateTime.Now;
            long _responsesAfterWarmup = _responses;
            long lastRequests = 0;
            long lastResponses = 0;
            double rps;

            while (true)
            {
                var elapsed = (DateTime.Now - start).TotalSeconds;
                var requests = _requests;
                var responses = _responses;
                lastRequests = requests;
                lastResponses = responses;
                try
                {
                    if (elapsed > 0)
                    {
                        rps = Math.Round((responses - _responsesAfterWarmup) / elapsed);
                        if (option.verbose)
                        {
                            Console.WriteLine("[{0,8}] Requests:{1,8} Responses:{2,8} RPS:{3,8}", DateTime.Now.ToString("HH:mm:ss.fff"), requests, responses, rps);
                        }
                        else
                        {
                            Console.WriteLine("[{0,8}] RPS:{1,8}", DateTime.Now.ToString("HH:mm:ss.fff"), rps);
                        }
                        if (rpsServer != null && rpsServer.Connected)
                        {
                            BenchmarkClient.Send(rpsServer, rps);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
                Thread.Sleep(1000);
            }
        }
    }
}
