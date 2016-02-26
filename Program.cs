using Cluster;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Cluster
{
    public class Program
    {
        private int _requests = 0;
        private int _responses = 0;
        private readonly Random _rand = new Random();
        private IDatabase [] _dbs;

        static void Main(string[] args)
        {
            Program prog = new Program();
            var options = Options.Parse(args);
            Socket rpsserver = null;
            if (options == null) return;
            if(options.verbose)
            {
                PrintOptions(options);
            }
            if (options.servermode)
            {
                int listeningport = options.RPSport;
                AsynchronousSocketListener.StartListening(options.RPSport);
                return;
            }
            else
            {
                if (options.RPSServer != null)
                {
                    rpsserver = AsynchronousClient.StartClient(options.RPSServer, options.RPSport);
                    if (rpsserver == null || rpsserver.Connected == false)
                    {
                        return;                        
                    }
                }
            }
            if(!prog.CreateClients(options))
            {
                return;
            }
            string[] keyNames;
            if ((keyNames=prog.SetupCluster(options)) == null)
            {
                Console.Write("Setting up cluster failed");
            }
            prog.SendRequests(keyNames, options);
            if(!prog.ReportRPS(options,rpsserver))
            {
                Console.WriteLine("There was an error");
            }
            
        }

        private static void PrintOptions(Options options)
        {
            Console.WriteLine("Executing with following parameters:");
            foreach(PropertyInfo prop in typeof(Options).GetProperties())
            {
                Console.WriteLine("{0,-15} = {1}", prop.Name, prop.GetValue(options, null));
            }
        }

        public bool CreateClients(Options options)
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add(options.redisserver,options.redisport);
            config.Ssl = options.ssl;
            config.Password = options.password;
            config.AllowAdmin = true;

            _dbs = new IDatabase[options.clients];

            for (int i = 0; i < options.clients; i++)
            {
                try
                {
                    var cm = ConnectionMultiplexer.Connect(config);
                    var db = cm.GetDatabase();
                    _dbs[i] = db;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    return false;
                }
            }
            return true;
        }

        public string[] SetupCluster(Options option)
        {
            string[] keysarr = null;
            if (_dbs[0] == null)
            {
                return null;
            }

            byte[] val = new byte[option.valueSize];
            _rand.NextBytes(val);

            EndPoint e = _dbs[0].Multiplexer.GetEndPoints()[0];
            var serverType = _dbs[0].Multiplexer.GetServer(e);
            if (serverType.ServerType == ServerType.Cluster)
            {
                Console.WriteLine("Setting up cluster with keyname prefix {0}, value size {1}", option.keyPrefix, option.valueSize );
                var cn = serverType.ClusterNodes();
                Dictionary<int, string> keys = new Dictionary<int, string>();
                int count = 0;
                int totalmasters = _dbs[0].Multiplexer.GetEndPoints().Length / 2;

                do 
                {
                    string keyname = option.keyPrefix + count;
                    keys[Int32.Parse(cn.GetBySlot(_dbs[0].Multiplexer.HashSlot(keyname)).EndPoint.ToString().Split(':')[1])] = keyname;
                    _dbs[0].StringSet(keyname, val);
                    count++;
                }while (keys.Count < totalmasters);

                keysarr = keys.Values.ToArray();
             
            }
            else
            {
                Console.WriteLine("Setting up cache with test keys, value size {0}", option.valueSize);
                _dbs[0].StringSet("test", val);

                keysarr = new string[] { option.keyPrefix };
            }
            return keysarr;
        }

        public void SendRequests(string[] keysarr,Options option)
        {
            for (int i = 0; i < option.asyncRequests; i++)
            {
                SendRequestAsync(_dbs[i % option.clients], keysarr[i % keysarr.Length]);
            }
        }

        public void SendRequestAsync(IDatabase db,string val)
        {
            Interlocked.Increment(ref _requests);
            db.StringGetAsync(val).ContinueWith(t =>
            {
                Interlocked.Increment(ref _responses);
                SendRequestAsync(db,val);
            });
        }

        public bool ReportRPS(Options option, Socket rpsServer)
        {
            Console.WriteLine("Warmup for {0}ms...", option.warmup);
            Thread.Sleep(option.warmup);
            var start = DateTime.Now;
            var _responsesAfterWarmup = _responses;

            int lastRequests = 0;
            int lastResponses = 0;
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
                        Console.WriteLine("[{0,8}] Requests:{1,8} Responses:{2,8} RPS:{3,8}", DateTime.Now.ToString("HH:mm:ss.fff"), requests, responses, rps);
                        if (rpsServer != null && rpsServer.Connected)
                        {
                            AsynchronousClient.Send(rpsServer, rps);
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
